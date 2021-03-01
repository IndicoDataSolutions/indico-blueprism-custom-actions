using System.Data;
using Indico.BluePrism.Connector.Helpers.Converters;
using Newtonsoft.Json.Linq;

namespace Indico.BluePrism.Connector.Helpers
{
    internal class IndicoJsonUtility
    {
        private readonly DataTableToJsonConverter _dataTableToJsonConverter = new DataTableToJsonConverter();
        private readonly JsonToDataTableConverter _jsonToDataTableConverter = new JsonToDataTableConverter();

        public DataTable ConvertToDataTable(JObject json) => _jsonToDataTableConverter.ToDataTable(json);
        public JToken ConvertToJSON(DataTable dataTable) => _dataTableToJsonConverter.ToJson(dataTable);
    }
}
