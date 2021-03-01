using System;
using System.Data;
using System.IO;
using System.Linq;
using Indico.BluePrism.Connector.Helpers.Converters.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Indico.BluePrism.Connector.Helpers.Converters
{
    public class DataTableToJsonConverter
    {
        private readonly DataTableHelper _dataTableHelper = new DataTableHelper();

        public JToken ToJson(DataTable dataTable)
        {
            using (var output = new MemoryStream())
            {
                var writer = new JsonTextWriter(new StreamWriter(output));
                WriteJson(writer, dataTable);
                writer.Flush();

                output.Seek(0, SeekOrigin.Begin);

                var token = JToken.ReadFrom(new JsonTextReader(new StreamReader(output)));

                return token;
            }
        }

        private void WriteJson(JsonTextWriter writer, DataTable dataTable)
        {
            if (_dataTableHelper.IsFromJObject(dataTable))
            {
                WriteJObject(writer, dataTable);
            }
            else if (_dataTableHelper.IsFromArray(dataTable))
            {
                WriteJArray(writer, dataTable);
            }
            else
            {
                throw new CannotDetermineTableSourceException(dataTable);
            }
        }

        private void WriteJArray(JsonTextWriter writer, DataTable table)
        {
            writer.WriteStartArray();

            bool itemsAreJObjects = table.Columns.Count > 2 
                                    || (table.Columns.Count == 2 && !table.Columns.Contains(JsonToDataTableConverter.ArrayItemColumnName));

            foreach (var row in table.Rows.Cast<DataRow>())
            {
                if (itemsAreJObjects)
                {
                    WriteJObject(writer, row);
                }
                else
                {
                    var column = table.Columns[JsonToDataTableConverter.ArrayItemColumnName];
                    Write(writer, column.DataType, row[column]);
                }
            }

            writer.WriteEndArray();
        }

        private void WriteJObject(JsonTextWriter writer, DataTable dataTable)
        {
            if (dataTable.Rows.Count != 1)
            {
                throw new JObjectTableRowCountError(dataTable);
            }

            WriteJObject(writer, dataTable.Rows[0]);
        }

        private void WriteJObject(JsonTextWriter writer, DataRow row)
        {
            writer.WriteStartObject();
            foreach (var col in row.Table.Columns.Cast<DataColumn>().Where(_dataTableHelper.DataColumnsFilter))
            {
                writer.WritePropertyName(col.ColumnName);
                Write(writer, col.DataType, row[col.ColumnName]);
            }
            writer.WriteEndObject();
        }

        private void Write(JsonTextWriter writer, Type type, object value)
        {
            switch (value)
            {
                case string str:
                    writer.WriteValue(str);
                    break;
                case DataTable table:
                    WriteJson(writer, table);
                    break;
                case DBNull _:
                    writer.WriteNull();
                    break;
                case decimal dec when dec == (int)dec:
                    // it's BluePrism, every number is decimal
                    // we need to format integers without fractional part
                    writer.WriteValue((int)dec);
                    break;
                case object o when o.GetType().IsValueType:
                    writer.WriteValue(value);
                    break;
                default:
                    throw new InvalidOperationException($"Cannot convert {type.Name} to json");
            }
        }
    }
}
