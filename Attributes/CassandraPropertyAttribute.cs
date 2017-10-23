using System;

namespace Cassandra.NET.Attributes
{
    public class CassandraPropertyAttribute : Attribute
    {
        public CassandraPropertyAttribute()
        {
        }

        public CassandraPropertyAttribute(string attributeName)
        {
            AttributeName = attributeName;
        }

        public string AttributeName { get; }
    }
}
