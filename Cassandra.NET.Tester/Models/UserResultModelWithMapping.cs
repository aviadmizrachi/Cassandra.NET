﻿using System;
using Cassandra.NET.Attributes;

namespace Cassandra.NET.Tester.Models
{
    [CassandraTable("user_results")]
    public class UserResultModelWithMapping
    {
        [CassandraProperty("user_id")]
        public string UserId { get; set; }
        [CassandraProperty("time")]
        public DateTimeOffset Timestamp { get; set; }
        [CassandraProperty("result")]
        public float Result { get; set; }
    }
}
