using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Indico.BluePrism.Connector.Helpers;
using IndicoV2;
using IndicoV2.Submissions;
using IndicoV2.Submissions.Models;

namespace Indico.BluePrism.Connector
{
    public class IndicoConnector : IIndicoConnector
    {
        protected ISubmissionsClient _submissionsClient;

        public IndicoConnector(string token, string host)
            : this(
                new IndicoV2.IndicoClient(
                    token,
                    host == null ? null : new Uri(host)))
        { }

        private IndicoConnector(IndicoV2.IndicoClient indicoClient)
         : this(indicoClient.Submissions())
        { }

        public IndicoConnector(ISubmissionsClient submissionsClient) => _submissionsClient = submissionsClient;


        public DataTable WorkflowSubmission(DataTable filepaths, DataTable uris, decimal workflowId)
        {
            bool filepathsProvided = ValidateInputDataTable(filepaths);
            bool urisProvided = ValidateInputDataTable(uris);

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
                result = Task.Run(async () => await _submissionsClient.CreateAsync(workflowIntId, fileList)).Result;
            }

            if (urisProvided)
            {
                var fileList = uris.ToList<string>().Select(u => new Uri(u));
                result = Task.Run(async () => await _submissionsClient.CreateAsync(workflowIntId, fileList)).Result;
            }

            return result.ToIdDataTable();
        }

        public DataTable ListSubmissions(DataTable submissionIds, DataTable workflowIds, string inputFileName, string status, string retrieved, decimal limit = 1000)
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

            var result = Task.Run(async () => await _submissionsClient.ListAsync(submissionIdsList, workflowIdsList, submissionFilter, limitInt)).Result;

            return result.ToDetailedDataTable();
        }

        private static bool ValidateInputDataTable(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return false;
            }

            return true;
        }

    }
}
