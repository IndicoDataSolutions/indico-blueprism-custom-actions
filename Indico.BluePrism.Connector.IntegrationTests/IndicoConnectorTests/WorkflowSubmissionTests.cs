using System.Data;
using FluentAssertions;

namespace Indico.BluePrism.Connector.IntegrationTests.IndicoConnectorTests
{
    public class WorkflowSubmissionTests : ConnectorTestsBase<DataTable>
    {
        protected override DataTable PerformAction(IndicoConnector connector) =>
            connector.WorkflowSubmission(null, _dataHelper.GetUrls(), _dataHelper.GetAnyWorkflowId());

        protected override void AssertResultCorrect(DataTable result) => result.Rows.Count.Should().Be(1);
    }
}
