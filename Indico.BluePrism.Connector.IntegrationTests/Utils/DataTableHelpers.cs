using System.Collections.Generic;
using System.Data;

namespace Indico.BluePrism.Connector.IntegrationTests.Utils
{
    internal static class DataTableHelpers
    {
        public static DataTable ToDataTable(this IEnumerable<decimal> numbers)
        {
            var table = new DataTable("Test") {Columns = {new DataColumn("Id", typeof(decimal))},};
            foreach (var num in numbers)
            {
                table.Rows.Add(num);
            }

            return table;
        }
    }
}
