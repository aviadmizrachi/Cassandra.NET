# Cassandra.NET

Cassandra.NET is available in order to allow C# developers to work with classes reflection when working with the Cassandra keyspace and tables.

## Getting Started

Getting started is rather easy :-).
Just download the package from the nuget and you are good to go.

### Coding it

The main class is DataContext class.
Initialize it with the contact points for the cluster and the keyspace name (optional)

```
using (var dataContext = new CassandraDataContext(new[] {"127.0.0.1"}, "mykeyspace"))
{
}

```

### Linking the entity model

Linking the class from your domain model to the cassandra table is done using the CassandraTableAttribute

```
[CassandraTable(TableName = "user_results")]
public class UserResult
{
  public string user_id { get; set; }
  public DateTimeOffset timestamp { get; set; }
  public float result { get; set; }
}
```

### Interacting with the cassandra data store

Now on we can use simple LINQ in order to select and load items from the table

```
using (var dc = new CassandraDataContext(new[] { "127.0.0.1" }, "mykeyspace"))
{
  var topUsers = dc.Select<UserResult>(u => u.result >= 90);
}
```

And simple domain model interaction in order to add / update entities

```
using (var dc = new CassandraDataContext(new[] { "127.0.0.1" }, "mykeyspace"))
{
  var userResult = new UserResult
  {
    result = 99.99F,
    timestamp = DateTime.Now,
    user_id = "user_1"
  };
  
  dc.AddOrUpdate(userResult)
}
```

### Good Luck!
