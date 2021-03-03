using System.Data;
using FluentAssertions;
using NUnit.Framework;

namespace Indico.BluePrism.Connector.IntegrationTests.IndicoConnectorTests
{
    public class SubmissionResultTests : ConnectorTestsBase<DataTable>
    {
        private decimal _submissionId;

        public override void SetUp()
        {
            base.SetUp();
            _submissionId = _dataHelper.GetAnySubmissionId();
        }

        protected override DataTable PerformAction(IndicoConnector connector) =>
            connector.SubmissionResult(_submissionId, null);

        protected override void AssertResultCorrect(DataTable submissionResult)
        {
            submissionResult.Rows.Should().HaveCount(1);
            submissionResult.Rows[0]["results"].Should().BeOfType<DataTable>();
        }
    }
}
