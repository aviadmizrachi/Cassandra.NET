using System;
using Cassandra.NET.Attributes;
using Cassandra.NET.Exceptions;
using System.Linq;
using System.Reflection;

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

        public static PropertyInfo[] GetCassandraRelevantProperties(this Type type)
        {
            var properties = type.GetProperties()
                                 .Where(p => p.GetCustomAttributes(false)
                                              .OfType<CassandraIgnoreAttribute>()
                                              .Count() == 0);

            return properties.ToArray();
        }

        public static string GetColumnNameMapping(this PropertyInfo property)
        {
            var cassandraPropertyAttribute = property.GetCustomAttribute<CassandraPropertyAttribute>();
            if (cassandraPropertyAttribute != null)
                return cassandraPropertyAttribute.AttributeName;

            return property.Name;
        }
    }
}
