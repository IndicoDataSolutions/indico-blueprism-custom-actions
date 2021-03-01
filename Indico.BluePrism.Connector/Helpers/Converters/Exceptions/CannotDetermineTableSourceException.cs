using System;
using System.Data;

namespace Indico.BluePrism.Connector.Helpers.Converters.Exceptions
{
    internal class CannotDetermineTableSourceException : ArgumentException
    {
        public CannotDetermineTableSourceException(DataTable table) :
            base($"Cannot determine if DataTable '{table.TableName}' should be converted to JSON Array or Object")
        {
        }
    }
}
