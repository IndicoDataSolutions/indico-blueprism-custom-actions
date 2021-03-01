using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Indico.BluePrism.Connector.Helpers.Converters
{
    internal class JsonToDataTableConverter
    {
        internal static string ArrayItemColumnName { get; } = "ArrayItem";
        private readonly DataTableHelper _tableHelper = new DataTableHelper();

        public DataTable ToDataTable(JArray array) => ToDataTable(array.Type, array.Children());

        public DataTable ToDataTable(JObject obj) => ToDataTable(obj.Type, new[] { obj });

        private DataTable ToDataTable<TToken>(JTokenType sourceType, IEnumerable<TToken> items)
            where TToken : JToken
        {
            var rows = items.ToArray();
            var table = _tableHelper.CreateTable(sourceType, GetColumnTypes(rows));

            foreach (var item in rows)
            {
                var row = table.NewRow();
                table.Rows.Add(row);
                Fill(row, item);
            }

            return table;
        }

        private IEnumerable<DataColumn> GetColumnTypes(IEnumerable<JToken> items) =>
            GetColumnsForCells(items)
                .GroupBy(x => x.ColumnName, x => x.CellType)
                .Select(grp => (
                    ColumnName: grp.Key, 
                    ColumnType: PickCorrectType(grp))) 
                .Select(grp => new DataColumn(grp.ColumnName, grp.ColumnType));

        private Type PickCorrectType(IEnumerable<Type> allTypesInColumn)
        {
            var types = allTypesInColumn.Distinct().Where(t => t != typeof(DBNull)).ToArray();

            return types.Length == 1 ? types[0] : typeof(string);
        }

        private IEnumerable<(string ColumnName, Type CellType)> GetColumnsForCells(IEnumerable<JToken> rows)
        {
            foreach (var row in rows)
            {
                if (row is JObject jObject)
                {
                    foreach (var cell in jObject.Properties())
                    {
                        yield return (cell.Name, GetColumnType(cell.Value));
                    }
                }

                if (row is JValue jValue)
                {
                    yield return (ArrayItemColumnName, GetColumnType(jValue));
                }
            }
        }

        private void Fill(DataRow row, JToken token)
        {
            switch (token)
            {
                case JObject obj:
                    Fill(row, obj);
                    break;
                case JValue val:
                    row[ArrayItemColumnName] = val.Value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Cannot convert {token.Type} to {nameof(DataRow)}");
            }
        }

        private void Fill(DataRow row, JObject obj)
        {
            foreach (var property in obj.Properties())
            {
                var propertyName = property.Name;

                if (property.Value.Type != JTokenType.Null)
                {
                    row[propertyName] = ToCellValue(property.Value);
                }
            }
        }

        private object ToCellValue(JToken token)
        {
            switch (token)
            {
                case JObject obj:
                    return ToDataTable(obj);
                case JValue value when value.Type != JTokenType.Null:
                    return value.Value;
                case JArray array:
                    return ToDataTable(array);
                default:
                    throw new ArgumentOutOfRangeException($"Cannot convert {token.Type} token to {nameof(DataTable)}");
            };
        }

        private Type GetColumnType(JToken propertyValue)
        {
            switch (propertyValue.Type)
            {
                case JTokenType.Boolean:
                    return typeof(bool);
                case JTokenType.Integer:
                    return typeof(int);
                case JTokenType.Float:
                    return typeof(float);
                case JTokenType.String:
                    return typeof(string);
                case JTokenType.Date:
                    return typeof(DateTime);
                case JTokenType.Null:
                    return typeof(DBNull);
                case JTokenType.Object:
                    return typeof(DataTable);
                case JTokenType.Array:
                    return typeof(DataTable);
                default:
                    throw new ArgumentOutOfRangeException();
            };
        }
    }
}
