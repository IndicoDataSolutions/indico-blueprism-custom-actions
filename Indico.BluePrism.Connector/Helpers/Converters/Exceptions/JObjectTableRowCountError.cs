using System;
using System.Data;

namespace Indico.BluePrism.Connector.Helpers.Converters.Exceptions
{
    internal class JObjectTableRowCountError : ArgumentException
    {
        public JObjectTableRowCountError(DataTable table) : base($"Invalid row count for the table '{table.TableName}' : {table.Rows.Count}")
        {
        }
    }
}
