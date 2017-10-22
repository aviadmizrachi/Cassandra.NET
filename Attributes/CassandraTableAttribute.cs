using System;

namespace Cassandra.NET.Attributes
{
    public class CassandraTableAttribute : Attribute
    {
        public string TableName { get; set; }
    }
}
