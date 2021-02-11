using System.Data;
using FluentAssertions;
using NUnit.Framework;

namespace Indico.BluePrism.Connector.IntegrationTests.IndicoConnectorTests
{
    public class SubmissionResultTests : ConnectorTestsBase
    {
        [Test]
        public void SubmissionResult_ShouldReturnJobResult()
        {
            // Arrange
            var submissionId = _dataHelper.GetAnySubmissionId();

            // Act
            var submissionResult = _connector.SubmissionResult(submissionId, null);

            // Assert
            submissionResult.Should().NotBeNull();
            submissionResult.Rows.Should().HaveCount(1);
            submissionResult.Rows[0]["results"].Should().BeOfType<DataTable>();
        }
    }
}
