using System.Data;
using FluentAssertions;
using NUnit.Framework;

namespace Indico.BluePrism.Connector.IntegrationTests.IndicoConnectorTests
{
    public class SubmitReviewTests : ConnectorTestsBase
    {
        [Test]
        public void SubmitReview_ShouldReturn__Result()
        {
            // Arrange
            var submissionResult = _dataHelper.GetNewSubmissionResult();
            var results = (DataTable)submissionResult.submissionResult.Rows[0]["results"];
            var document = (DataTable)results.Rows[0]["document"];
            var resultsInner = (DataTable)document.Rows[0]["results"];

            // Act
            var submitReviewResult = _connector.SubmitReview(submissionResult.submisisonId, resultsInner, false);

            // Assert
            var submitReviewTable = (DataTable)submitReviewResult;
            submitReviewTable.Rows[0]["submission_status"].Should().Be("pending_review");
            submitReviewTable.Rows[0]["success"].Should().Be(true);
        }
    }
}
