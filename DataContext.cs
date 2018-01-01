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

        private int currentBatchSize = 0;
        private object batchLock = new object();
        private BatchStatement currentBatch = new BatchStatement();

        public int BatchSize { get; set; } = 50;
        public bool UseBatching { get; set; } = false;

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
            lock (currentBatch)
            {
                if (currentBatchSize > 0)
                    session.Execute(currentBatch);
            }

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

        public double Average<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression)
        {
            var columnName = QueryBuilder.EvaluatePropertyName(propertyExpression);

            var queryStatement = QueryBuilder.EvaluateQuery(predicate);
            var tableName = typeof(T).ExtractTableName<T>();
            var selectQuery = $"select avg({columnName}) from {tableName} where {queryStatement.Statment}";

            var statement = new SimpleStatement(selectQuery, queryStatement.Values);
            var rows = session.Execute(statement);

            var avg = Convert.ToDouble(rows.First()[0]);

            return avg;
        }

        public double Sum<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression)
        {
            var columnName = QueryBuilder.EvaluatePropertyName(propertyExpression);

            var queryStatement = QueryBuilder.EvaluateQuery(predicate);
            var tableName = typeof(T).ExtractTableName<T>();
            var selectQuery = $"select sum({columnName}) from {tableName} where {queryStatement.Statment}";

            var statement = new SimpleStatement(selectQuery, queryStatement.Values);
            var rows = session.Execute(statement);

            var sum = Convert.ToDouble(rows.First()[0]);

            return sum;
        }

        public double Min<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression)
        {
            var columnName = QueryBuilder.EvaluatePropertyName(propertyExpression);

            var queryStatement = QueryBuilder.EvaluateQuery(predicate);
            var tableName = typeof(T).ExtractTableName<T>();
            var selectQuery = $"select min({columnName}) from {tableName} where {queryStatement.Statment}";

            var statement = new SimpleStatement(selectQuery, queryStatement.Values);
            var rows = session.Execute(statement);

            var sum = Convert.ToDouble(rows.First()[0]);

            return sum;
        }

        public double Max<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression)
        {
            var columnName = QueryBuilder.EvaluatePropertyName(propertyExpression);

            var queryStatement = QueryBuilder.EvaluateQuery(predicate);
            var tableName = typeof(T).ExtractTableName<T>();
            var selectQuery = $"select max({columnName}) from {tableName} where {queryStatement.Statment}";

            var statement = new SimpleStatement(selectQuery, queryStatement.Values);
            var rows = session.Execute(statement);

            var sum = Convert.ToDouble(rows.First()[0]);

            return sum;
        }


        public T SingleOrDefault<T>(Expression<Func<T, bool>> predicate)
        {
            return Select(predicate).SingleOrDefault();
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate)
        {
            return Select(predicate).FirstOrDefault();
        }

        public void AddOrUpdate<T>(T entity)
        {
            var insertStatment = CreateAddStatement(entity);

            if (UseBatching)
            {
                lock (batchLock)
                {
                    currentBatch.Add(insertStatment);
                    ++currentBatchSize;
                    if (currentBatchSize == BatchSize)
                    {
                        session.Execute(currentBatch);
                        currentBatchSize = 0;
                        currentBatch = new BatchStatement();
                    }
                }
            }
            else
            {
                session.Execute(insertStatment);
            }
        }

        private Statement CreateAddStatement<T>(T entity)
        {
            var tableName = typeof(T).ExtractTableName<T>();

            // We are interested only in the properties we are not ignoring
            var properties = entity.GetType().GetCassandraRelevantProperties();
            var properiesNames = properties.Select(p => p.GetColumnNameMapping()).ToArray();
            var parametersSignals = properties.Select(p => "?").ToArray();
            var propertiesValues = properties.Select(p => p.GetValue(entity)).ToArray();
            var insertCql = $"insert into {tableName}({string.Join(",", properiesNames)}) values ({string.Join(",", parametersSignals)})";
            var insertStatment = new SimpleStatement(insertCql, propertiesValues);

            return insertStatment;
        }
    }
}
