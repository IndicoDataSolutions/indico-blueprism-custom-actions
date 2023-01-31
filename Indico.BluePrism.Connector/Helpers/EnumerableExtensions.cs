using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Indico.Entity;

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
                //unsupported properties -- basically complex objects... except collections.
                if (prop.PropertyType != typeof(string) && prop.PropertyType.IsClass)
                {
                    throw new NotSupportedException($"Type {prop.PropertyType} is not supported.");
                }
               
                if (prop.PropertyType.IsEnum)
                {
                    tb.Columns.Add(prop.Name, typeof(string));
                }
                //collections of sub-objects (for example, submission files), as data tables
                //the typecheck for string is because string is technically IEnumerable<char>
                else if (prop.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
                {

                    tb.Columns.Add(prop.Name, typeof(DataTable));
                }
                else
                {
                    tb.Columns.Add(prop.Name, prop.PropertyType);
                }
            }

            foreach (var item in items)
            {
                //doing some unsafe type things here to get the member variables and serialize them
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    var value = prop.GetValue(item, null);
                    if (prop.PropertyType.IsEnum)
                    {
                        values[i] = value.ToString();
                    }
                    if (prop.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
                    {
                        //todo: figure out how to do this generically but for now just assign the type based on the name...
                        if(prop.Name == "SubmissionFiles")
                        {
                            var submissionFiles = value as IEnumerable<SubmissionFiles>;
                            values[i] = submissionFiles?.ToDetailedDataTable();
                        }
  
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
