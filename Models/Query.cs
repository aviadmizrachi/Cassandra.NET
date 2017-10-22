using System;
using System.Collections.Generic;
using System.Text;

namespace Cassandra.NET.Models
{
    internal class Query
    {
        public Query()
        {
        }

        public Query(string statment, object[] values)
        {
            Statment = statment;
            Values = values;
        }

        public string Statment { get; set; }
        public object[] Values { get; set; }
    }
}
