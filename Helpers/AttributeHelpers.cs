using System;
using Cassandra.NET.Attributes;
using Cassandra.NET.Exceptions;
using System.Linq;

namespace Cassandra.NET.Helpers
{
    public static class AttributeHelpers
    {
        public static string ExtractTableName<T>(this Type type)
        {
            var tableNameAttribute = typeof(T).GetCustomAttributes(typeof(CassandraTableAttribute), true).FirstOrDefault() as CassandraTableAttribute;
            if (tableNameAttribute != null)
                return tableNameAttribute.TableName;

            throw new MissingTableAttributeException(typeof(T));
        }
    }
}
