using System.IO;
using System.Threading.Tasks;
using Indico.BluePrism.Connector.Tests.Helpers;
using Newtonsoft.Json.Linq;

namespace Indico.BluePrism.Connector.Tests.TestData
{
    internal class JsonTestData
    {
        public async Task<JObject> SubmissionResult()
        {
            using var reader = new StreamReader(
                typeof(IndicoJsonUtilityTests).Assembly.GetManifestResourceStream(
                    "Indico.BluePrism.Connector.Tests.TestData.SubmissionResult.json"));

            return JObject.Parse(await reader.ReadToEndAsync());
        }
    }
}
