using System.Data;
using FluentAssertions;
using Indico.BluePrism.Connector.IntegrationTests.Utils;
using NUnit.Framework;

namespace Indico.BluePrism.Connector.IntegrationTests.IndicoConnectorTests
{
    public class SubmissionResultTests
    {
        private IndicoConnector _connector;
        private TestDataHelper _dataHelper;

        [SetUp]
        public void SetUp()
        {
            _connector = new IndicoConnectorBuilder().Build();
            _dataHelper = new TestDataHelper(_connector);
        }

        [Test]
        public void SubmissionResult_ShouldReturnJobResult()
        {
            // Arrange
            var submissionId = _dataHelper.GetAnySubmissionId();

            // Act
            var submissionResult = _connector.SubmissionResult(submissionId, 200, 5000, null);

            // Assert
            submissionResult.Should().NotBeNull();
            submissionResult.Rows.Should().HaveCount(1);
            submissionResult.Rows[0]["results"].Should().BeOfType<DataTable>();
        }
    }
}
