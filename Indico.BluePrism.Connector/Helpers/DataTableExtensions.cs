using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Indico.BluePrism.Connector.Helpers
{
    public static class DataTableExtensions
    {
        public static IEnumerable<T> ToList<T>(this DataTable dataTable, int columnId = 0) 
            => dataTable.Rows.Cast<DataRow>().Select(dr => (T)dr[columnId]).ToList();
    }
}
