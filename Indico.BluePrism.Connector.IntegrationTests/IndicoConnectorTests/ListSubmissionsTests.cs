using System.Data;
using FluentAssertions;
using Indico.BluePrism.Connector.IntegrationTests.Utils;
using Indico.Exception;
using NUnit.Framework;

namespace Indico.BluePrism.Connector.IntegrationTests.IndicoConnectorTests
{
    public class ListSubmissionsTests : ConnectorTestsBase<DataTable>
    {
        [Test]
        public void ListSubmissions_Throws_WhenIncorrectWorkflowId()
        {
            var connector = new IndicoConnectorBuilder().Build();
            const string expectedExceptionMessage =
                @"1 : http://sunbow:5000/api/submission?workflow_ids=0&limit=1&filters=%7B%7D: {""code"": 403, ""content"": {}, ""error_class"": ""ServiceException"", ""error_type"": ""ForbiddenAccess"", ""message"": ""This user does not have access to this resource""}";
            connector.Invoking(c => c.ListSubmissions(null, new decimal[] { 0 }.ToDataTable(), null, null, null, 1))
                .Should()
                .Throw<GraphQLException>()
                .WithMessage(expectedExceptionMessage);
        }


        protected override DataTable PerformAction(IndicoConnector connector) =>
            connector.ListSubmissions(
                null,
                 DataTableHelpers.ToDataTable(new[] { _dataHelper.GetAnySubmissionId() }),
                        null,
                        null,
                        null,
                        1);

        protected override void AssertResultCorrect(DataTable result) => result.Rows.Count.Should().Be(1);
    }
}
