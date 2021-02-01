using System.Collections.Generic;
using System.Data;

namespace Indico.BluePrism.Connector.Helpers
{
    public static class EnumerableExtensions
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> list, string columnName = "Id")
        {
            var result = new DataTable();
            result.Columns.Add(new DataColumn(columnName, typeof(T)));

            foreach (var value in list)
            {
                result.Rows.Add(value);
            }

            return result;
        }
    }
}
