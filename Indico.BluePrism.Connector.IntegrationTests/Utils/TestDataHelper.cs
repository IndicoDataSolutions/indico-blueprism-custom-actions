using System.Data;

namespace Indico.BluePrism.Connector.IntegrationTests.Utils
{
    public class TestDataHelper
    {
        private readonly IndicoConnector _connector;

        public TestDataHelper(IndicoConnector connector) => _connector = connector;

        public decimal GetAnySubmissionId() => (int)_connector.ListSubmissions(null, null, null, null, null, 1).Rows[0]["Id"];

        public (decimal submisisonId, DataTable submissionResult) GetAnySubmissionResult() => GetSubmissionResult(GetAnySubmissionId());

        private (decimal submisisonId, DataTable submissionResult) GetSubmissionResult(decimal submissionId) => (submissionId,
            _connector.SubmissionResult(submissionId, default));
    }
}
