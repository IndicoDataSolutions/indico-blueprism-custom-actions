using System;
using System.Data;
using System.Linq;
using FluentAssertions;

namespace Indico.BluePrism.Connector.IntegrationTests.IndicoConnectorTests
{
    public class SubmitReviewTests : ConnectorTestsBase<DataTable>
    {
        protected override DataTable PerformAction(IndicoConnector connector)
        {
            var submissionResult = _dataHelper.GetNewSubmissionResult();
            var results = (DataTable)submissionResult.submissionResult.Rows[0]["results"];
            var document = (DataTable)results.Rows[0]["document"];
            var changes = (DataTable)document.Rows[0]["results"];

            var modelPredictions = (DataTable)changes.Rows[0][0];
            // blueprism is converting integers to decimal, we need to make sure it's converted back to int 
            // before being sent to indico
            ChangeColumnType(modelPredictions, "start", typeof(decimal));
            ChangeColumnType(modelPredictions, "end", typeof(decimal));

            // Act
            var submitReviewResult = connector.SubmitReview(submissionResult.submisisonId, changes, false);

            return submitReviewResult;
        }

        protected override void AssertResultCorrect(DataTable submitReviewResult)
        {
            submitReviewResult.Rows[0]["submission_status"].Should().Be("pending_review");
            submitReviewResult.Rows[0]["success"].Should().Be(true);
        }

        private void ChangeColumnType(DataTable modelPredictions, string columnName, Type columnType)
        {
            var columnNameTemp = $"{columnName}old";

            modelPredictions.Columns[columnName].ColumnName = columnNameTemp;
            modelPredictions.Columns.Add(columnName, columnType);

            foreach (var row in modelPredictions.Rows.Cast<DataRow>())
            {
                row[columnName] = Convert.ChangeType(row[columnNameTemp], columnType);
            }

            modelPredictions.Columns.Remove(columnNameTemp);
        }
    }
}
