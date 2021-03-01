using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Indico.BluePrism.Connector.Helpers.Converters
{
    internal class DataTableHelper
    {
        private const string _markerColumnNamePrefix = "CreatedFromJson";
        private readonly string _tableFromArrayMarkerColumnName = GetMarkerColumnName(JTokenType.Array);
        private readonly string _tableFromObjectMarkerColumnName = GetMarkerColumnName(JTokenType.Object);

        public DataTable CreateTable(JTokenType type, IEnumerable<DataColumn> columns)
        {
            var table = new DataTable(type.ToString());
            table.Columns.AddRange(columns.ToArray());
            table.Columns.Add(CreateMarker(type));

            return table;
        }

        public bool IsFromJObject(DataTable dataTable) => dataTable.Columns.IndexOf(_tableFromObjectMarkerColumnName) >= 0;

        public bool IsFromArray(DataTable dataTable) => dataTable.Columns.IndexOf(_tableFromArrayMarkerColumnName) >= 0;

        private static string GetMarkerColumnName(JTokenType type) => $"{_markerColumnNamePrefix}{type}";

        public Func<DataColumn, bool> DataColumnsFilter => dc =>
            dc.ColumnName != _tableFromArrayMarkerColumnName && dc.ColumnName != _tableFromObjectMarkerColumnName;

        public DataColumn CreateFromJObjectMarker() => CreateMarker(JTokenType.Object);
        public DataColumn CreateFromJArrayMarker() => CreateMarker(JTokenType.Array);

        private DataColumn CreateMarker(JTokenType jTokenType) => new DataColumn(GetMarkerColumnName(jTokenType));
    }
}
