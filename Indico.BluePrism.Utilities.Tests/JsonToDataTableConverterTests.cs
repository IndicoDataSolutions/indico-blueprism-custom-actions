using System.Data;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Indico.BluePrism.Utilities.Tests
{
    public class JsonToDataTableConverterTests
    {
        private const string _submissionResultResourceName = "Indico.BluePrism.Utilities.Tests.JsonToDataTableConverter.SubmissionResult.json";

        private async Task<string> GetSampleSubmissionResultAsync()
        {
            using var reader = new StreamReader(
                    typeof(JsonToDataTableConverterTests).Assembly.GetManifestResourceStream(
                        _submissionResultResourceName));

            return await reader.ReadToEndAsync();
        }

        [Test]
        public async Task ConvertToDataTable_ShouldDeserializeSubmissionResult()
        {
            var result = new JsonUtility().ConvertToDataTable(await GetSampleSubmissionResultAsync());

            result.Rows.Should().HaveCount(1);
            result.Rows[0]["results"].Should().BeOfType<DataTable>();
        }
    }
}
