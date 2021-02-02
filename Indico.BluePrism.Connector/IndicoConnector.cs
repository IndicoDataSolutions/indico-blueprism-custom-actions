using System;
using System.Data;
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


        public DataTable WorkflowFileSubmission(DataTable filepaths, decimal workflowId)
        {
            if (filepaths == null)
            {
                throw new ArgumentNullException("No collection with filepaths was passed to the client.");
            }
            else if (filepaths.Rows.Count == 0)
            {
                throw new ArgumentException("No filepaths were passed to the client.");
            }
            var files = filepaths.ToList<string>();

            var result = _submissionsClient.CreateAsync(Convert.ToInt32(workflowId), files).GetAwaiter().GetResult();

            return result.ToDataTable();
        }

        public DataTable WorkflowUrisSubmission(DataTable uris, decimal workflowId) => throw new NotImplementedException();
    }
}
