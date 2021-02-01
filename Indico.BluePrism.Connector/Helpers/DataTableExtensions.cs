using System.Collections.Generic;
using System.Data;

namespace Indico.BluePrism.Connector.Helpers
{
    public static class DataTableExtensions
    {
        public static IEnumerable<T> ToList<T>(this DataTable dataTable, int columnId = 0)
        {
            var result = new List<T>();
            foreach (DataRow row in dataTable.Rows)
            {
                var value = (T)row[columnId];
                result.Add(value);
            }

            return result;
        }
    }
}
