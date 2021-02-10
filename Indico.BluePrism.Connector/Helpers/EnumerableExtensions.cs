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
                if (prop.PropertyType != typeof(string) && prop.PropertyType.IsClass)
                {
                    throw new NotSupportedException($"Type {prop.PropertyType} is not supported.");
                }

                if (prop.PropertyType.IsEnum)
                {
                    tb.Columns.Add(prop.Name, typeof(string));
                }
                else
                {
                    tb.Columns.Add(prop.Name, prop.PropertyType);
                }
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    var value = prop.GetValue(item, null);
                    if (prop.PropertyType.IsEnum)
                    {
                        values[i] = value.ToString();
                    }
                    else
                    {
                        values[i] = prop.GetValue(item, null);
                    }
                }

                tb.Rows.Add(values);
            }

            return tb;
        }
    }
}
