using System;
using System.Linq;
using Cassandra.NET.Helpers;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Cassandra.NET
{
    public class CassandraDataContext : IDisposable
    {
        private Cluster cluster;
        private ISession session;

        public CassandraDataContext(string[] contactPoints)
        {
            cluster = Cluster.Builder().AddContactPoints(contactPoints).Build();
            session = cluster.Connect();
        }

        public CassandraDataContext(string[] contactPoints, string keyspace)
        {
            cluster = Cluster.Builder().AddContactPoints(contactPoints).Build();
            session = cluster.Connect(keyspace);
        }

        public void Dispose()
        {
            session.Dispose();
        }

        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> predicate)
        {
            var output = new List<T>();

            var queryStatement = QueryBuilder.EvaluateQuery(predicate);
            var tableName = typeof(T).ExtractTableName<T>();
            var selectQuery = $"select * from {tableName} where {queryStatement.Statment}";
            var statement = new SimpleStatement(selectQuery, queryStatement.Values);
            var rows = session.Execute(statement);

            foreach (var row in rows)
                output.Add(Mapper.Map<T>(row));

            return output;
        }

        public T SingleOrDefault<T>(Expression<Func<T, bool>> predicate)
        {
            return Select(predicate).SingleOrDefault();
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate)
        {
            return Select(predicate).FirstOrDefault();
        }

    }
}
