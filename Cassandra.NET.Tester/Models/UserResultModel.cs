using System;
using Cassandra.NET.Attributes;

namespace Cassandra.NET.Tester.Models
{
    [CassandraTable("user_results")]
    public class UserResultModel
    {
        public string user_id { get; set; }
        public DateTimeOffset time { get; set; }
        public float result { get; set; }
    }
}
