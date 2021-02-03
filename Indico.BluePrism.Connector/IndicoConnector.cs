using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Indico.BluePrism.Connector.Helpers;
using IndicoV2;
using IndicoV2.Submissions;

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

            return result.ToDataTable();
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
