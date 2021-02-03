using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Indico.BluePrism.Connector.Helpers
{
    public static class EnumerableExtensions
    {
        public static DataTable ToIdDataTable<T>(this IEnumerable<T> list, string columnName = "Id")
        {
            var result = new DataTable();
            result.Columns.Add(new DataColumn(columnName, typeof(T)));

            foreach (var value in list)
            {
                result.Rows.Add(value);
            }

            return result;
        }

        public static DataTable ToDetailedDataTable<T>(this IEnumerable<T> items)
        {
            var tb = new DataTable(typeof(T).Name);

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            return tb;
        }
    }
}
