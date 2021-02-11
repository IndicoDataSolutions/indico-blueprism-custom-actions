using System.Data;
using System.Threading.Tasks;
using FluentAssertions;
using Indico.BluePrism.Connector.Helpers;
using Indico.BluePrism.Connector.Tests.TestData;
using NUnit.Framework;

namespace Indico.BluePrism.Connector.Tests.Helpers
{
    public class IndicoJsonUtilityTests
    {
        private readonly JsonTestData _testData = new JsonTestData();
        private readonly IndicoJsonUtility _sut = new IndicoJsonUtility();

        [Test]
        public async Task ConvertToDataTable_ShouldDeserializeSubmissionResult()
        {
            var result = _sut.ConvertToDataTable(await _testData.SubmissionResult());

            result.Rows.Should().HaveCount(1);
            result.Rows[0]["results"].Should().BeOfType<DataTable>();
        }

        [Test]
        public async Task ConvertToJson_ShouldCreateEquivalentOfSource()
        {
            // Arrange
            var sourceJson = await _testData.SubmissionResult();
            var sourceTable = _sut.ConvertToDataTable(sourceJson);

            // Act
            var outputJson = _sut.ConvertToJSON(sourceTable);

            // Assert
            outputJson.Should().BeEquivalentTo(sourceJson);
        }
    }
}
