using System.Data;

namespace Indico.BluePrism.Connector.IntegrationTests.Utils
{
    public class TestDataHelper
    {
        private readonly IndicoConnector _connector;

        public TestDataHelper(IndicoConnector connector) => _connector = connector;

        public decimal GetAnyWorkflowId() => (int)GetSubmission()["WorkflowId"];

        public decimal GetAnySubmissionId() => (int)GetSubmission()["Id"];

        public decimal GetNewSubmissionId() => (int)_connector.WorkflowSubmission(
            null,
            GetUrls(),
            GetAnyWorkflowId()).Rows[0]["Id"];

        public DataTable GetUrls() => new DataTable()
        {
            Columns = {new DataColumn("url", typeof(string))},
            Rows = {"https://www.wmaccess.com/downloads/sample-invoice.pdf"}
        };

        public (decimal submisisonId, DataTable submissionResult) GetNewSubmissionResult() => GetSubmissionResult(GetNewSubmissionId());

        private (decimal submisisonId, DataTable submissionResult) GetSubmissionResult(decimal submissionId) => (submissionId,
            _connector.SubmissionResult(submissionId, default));

        private DataRow GetSubmission() => _connector.ListSubmissions(null, null, null, null, null, 1).Rows[0];
    }
}
