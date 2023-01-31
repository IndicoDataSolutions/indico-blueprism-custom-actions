using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Indico.BluePrism.Connector.Helpers;
using IndicoV2;
using IndicoV2.Extensions.Jobs;
using IndicoV2.Extensions.SubmissionResult;
using IndicoV2.Reviews;
using IndicoV2.Submissions;
using IndicoV2.Submissions.Models;
using Newtonsoft.Json.Linq;
using Client = IndicoV2.IndicoClient;

namespace Indico.BluePrism.Connector
{
    public class IndicoConnector : IIndicoConnector
    {
        private readonly ISubmissionsClient _submissionsClient;
        private readonly ISubmissionResultAwaiter _submissionResultAwaiter;
        private readonly IReviewsClient _reviewsClient;
        private readonly IJobAwaiter _jobAwaiter;
        private readonly IndicoJsonUtility _jsonUtility = new IndicoJsonUtility();

        private TimeSpan _checkInterval = TimeSpan.FromSeconds(1);
        private TimeSpan _timeout = TimeSpan.FromSeconds(60);

        public decimal CheckIntervalMs { get => (decimal)_checkInterval.TotalMilliseconds; set => _checkInterval = TimeSpan.FromMilliseconds((double)value); }
        public decimal TimeoutMs { get => (decimal)_timeout.TotalMilliseconds; set => _timeout = TimeSpan.FromMilliseconds((double)value); }


        public IndicoConnector(string token, string host)
            : this(host == null ? new Client(token) : new Client(token, new Uri(host)))
        { }

        private IndicoConnector(IndicoV2.IndicoClient indicoClient)
            : this(indicoClient.Submissions(), indicoClient.GetSubmissionResultAwaiter(), indicoClient.Reviews(), indicoClient.JobAwaiter())
        {
        }

        public IndicoConnector(ISubmissionsClient submissionsClient, ISubmissionResultAwaiter submissionResultAwaiter,
            IReviewsClient reviewsClient, IJobAwaiter jobAwaiter)
        {
            _submissionsClient = submissionsClient;
            _submissionResultAwaiter = submissionResultAwaiter;
            _reviewsClient = reviewsClient;
            _jobAwaiter = jobAwaiter;
        }


        public DataTable WorkflowSubmission(DataTable filepaths, DataTable uris, decimal workflowId)
        {
            bool filepathsProvided = HasAnyRows(filepaths);
            bool urisProvided = HasAnyRows(uris);

            if (!filepathsProvided && !urisProvided)
            {
                throw new ArgumentException("No uris or filepaths provided.");
            }

            if (filepathsProvided && urisProvided)
            {
                throw new ArgumentException("Provided uris and filepaths. Pass only one of the parameters.");
            }

            IEnumerable<int> result = null;
            int workflowIntId = Convert.ToInt32(workflowId);

            if (filepathsProvided)
            {
                var fileList = filepaths.ToList<string>();
                result = Task.Run(async () => await _submissionsClient.CreateAsync(workflowIntId, fileList)).GetAwaiter().GetResult();
            }

            if (urisProvided)
            {
                var fileList = uris.ToList<string>().Select(u => new Uri(u));
                result = Task.Run(async () => await _submissionsClient.CreateAsync(workflowIntId, fileList)).GetAwaiter().GetResult();
            }

            return result.ToIdDataTable();
        }

        public DataTable ListSubmissions(DataTable submissionIds, DataTable workflowIds, string inputFileName, string status, string retrieved, decimal limit = 1000)
        {
            var result = CallListSubmissions(submissionIds, workflowIds, inputFileName, status, retrieved, limit);

            return result.ToDetailedDataTable();
        }

        public DataTable SubmissionResult(decimal submissionIdDec, string awaitStatusString)
        {
            var submissionId = (int)submissionIdDec;
            var awaitStatus = string.IsNullOrWhiteSpace(awaitStatusString)
                ? (SubmissionStatus?)null
                : (SubmissionStatus)Enum.Parse((typeof(SubmissionStatus)), awaitStatusString);

            using (var timeoutTokenSource = new CancellationTokenSource(_timeout))
            {
                var cancellationToken = timeoutTokenSource.Token;
                var result = Task.Run(async () => await SubmissionResultAsync(submissionId, awaitStatus, cancellationToken), cancellationToken)
                    .GetAwaiter()
                    .GetResult();
                var dataTableResult = _jsonUtility.ConvertToDataTable(result);

                return dataTableResult;
            }
        }

        public async Task<JObject> SubmissionResultAsync(int submissionId, SubmissionStatus? awaitStatus, CancellationToken cancellationToken) =>
            awaitStatus == null
                ? await _submissionResultAwaiter.WaitReady(submissionId, _checkInterval, cancellationToken)
                : await _submissionResultAwaiter.WaitReady(submissionId, awaitStatus.Value, _checkInterval, cancellationToken);

        public DataTable SubmitReview(decimal submissionIdDec, DataTable changesTable, bool rejected)
            => SubmitReview(submissionIdDec, changesTable, rejected, null);

        public DataTable SubmitReview(decimal submissionIdDec, DataTable changesTable, bool rejected, bool forceComplete)
            => SubmitReview(submissionIdDec, changesTable, rejected, (bool?)forceComplete);

        private DataTable SubmitReview(decimal submissionIdDec, DataTable changesTable, bool rejected, bool? forceComplete)
        {
            var submissionId = (int)submissionIdDec;
            var changesProvided = HasAnyRows(changesTable);
            JObject changes = null;

            if (changesProvided)
            {
                changes = (JObject)_jsonUtility.ConvertToJSON(changesTable);
            }

            var jobResult = Task.Run(() => SubmitReviewAsync(submissionId, changes, rejected, forceComplete))
                .GetAwaiter()
                .GetResult();

            var jobResultTable = _jsonUtility.ConvertToDataTable(jobResult);

            return jobResultTable;
        }

        private async Task<JObject> SubmitReviewAsync(int submissionId, JObject changes, bool rejected, bool? forceComplete)
        {
            using (var tokenSource = new CancellationTokenSource(_timeout))
            {
                var jobId = await _reviewsClient.SubmitReviewAsync(submissionId, changes, rejected, forceComplete, tokenSource.Token);
                var jobResult = (JObject)await _jobAwaiter.WaitReadyAsync(jobId, _checkInterval, tokenSource.Token);

                return jobResult;
            }
        }

        private async Task<string> GenerateSubmissionResult(int submissionId)
        {
            using (var tokenSource = new CancellationTokenSource(_timeout))
            {
                var jobId = await _submissionsClient.GenerateSubmissionResultAsync(submissionId);
                return jobId;
            }
        }

        private static bool HasAnyRows(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return false;
            }

            return true;
        }

        private IEnumerable<ISubmission> CallListSubmissions(DataTable submissionIds, DataTable workflowIds, string inputFileName, string status, string retrieved, decimal limit = 1000)
        {
            bool? parsedRetrieved = null;
            SubmissionStatus? parsedStatus = null;

            if (string.IsNullOrWhiteSpace(inputFileName))
            {
                inputFileName = null;
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (!Enum.TryParse(status, out SubmissionStatus statusValue))
                {
                    throw new ArgumentException("Wrong status value provided. Please provide one of the valid submission statuses.");
                }
                else
                {
                    parsedStatus = statusValue;
                }
            }

            if (!string.IsNullOrEmpty(retrieved))
            {
                if (!bool.TryParse(retrieved, out bool retrievedValue))
                {
                    throw new ArgumentException("Wrong retreived value provided. Please provide \"True\" or \"False\" as a value.");
                }
                else
                {
                    parsedRetrieved = retrievedValue;
                }
            }

            var submissionFilter = new SubmissionFilter
            {
                InputFilename = inputFileName,
                Status = parsedStatus,
                Retrieved = parsedRetrieved
            };

            var submissionIdsList = submissionIds?.ToList<decimal>().Select(s => Convert.ToInt32(s)).ToList();
            var workflowIdsList = workflowIds?.ToList<decimal>().Select(w => Convert.ToInt32(w)).ToList();
            int limitInt = Convert.ToInt32(limit);

            var result = Task.Run(async () => await _submissionsClient.ListAsync(submissionIdsList, workflowIdsList, submissionFilter, limitInt)).GetAwaiter().GetResult();
            return result;
        }
            public DataTable GetSubmissionFileDetails(DataTable submissionIds)
        {

            var submissionIdsList = submissionIds?.ToList<decimal>().Select(s => Convert.ToInt32(s)).ToList();

            var result = CallListSubmissions(submissionIds, new DataTable(), "", "", "");
            var parsed = result.Select(x => x.SubmissionFiles.Select(y => new { y.Filename, NumPages = y.NumPages, Id = y.Id, SubmissionId = x.Id }));
            return parsed.ToDetailedDataTable();
        }
    }
}
