using System;
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
            var submissionResult = _dataHelper.GetAnySubmissionResult();
            // using model described in https://indicodatasolutions.github.io/indico-client-python/auto-review.html?highlight=submitreview
            var results = (DataTable)submissionResult.submissionResult.Rows[0]["results"];
            var document = (DataTable)results.Rows[0]["document"];
            var changes = (DataTable)document.Rows[0]["results"];

            // Act
            var submitReviewResult = _connector.SubmitReview(submissionResult.submisisonId, changes, false);

            // Assert
            submitReviewResult.Should().NotBeNull();
            throw new NotImplementedException("TODO: check why submit review ends with an error.");
        }
    }
}
