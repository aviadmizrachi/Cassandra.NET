# Project Title

Cassandra.NET is available in order to allow C# developers to work with classes reflection when working with the Cassandra keyspace and tables.

## Getting Started

Getting started is rather easy :-).
Just download the package from the nuget and you are good to go.

### Getting started with some code

The main class is DataContext class.
Initialize it with the contact points for the cluster and the keyspace name (optional)

```
using (var dataContext = new CassandraDataContext(new[] {"127.0.0.1"}, "mykeyspace"))
{
}

```
