using System;

namespace Cassandra.NET.Attributes
{
    public class CassandraTableAttribute : Attribute
    {
        public CassandraTableAttribute()
        {
        }

        public CassandraTableAttribute(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; set; }
    }
}
