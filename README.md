# Cassandra.NET

Cassandra.NET is available in order to allow C# developers to work with classes reflection when working with the Cassandra keyspace and tables.

## Getting Started

Getting started is rather easy :-).
Just download the package from the nuget and you are good to go.

### Coding it

The main class is DataContext class.
Initialize it with the contact points for the cluster and the keyspace name (optional)

```
using (dataContext = new CassandraDataContext(new[] { "127.0.0.1" }, "demo"))
{
}

```

### Linking the entity model

Linking the class from your domain model to the cassandra table is done using the CassandraTableAttribute

```

[CassandraTable("user_results")]
public class UserResultModel
{
  public string user_id { get; set; }
  public DateTimeOffset time { get; set; }
  public float result { get; set; }
}

```

### Interacting with the cassandra data store

Now we can use simple LINQ queries in order to select and load items from the table

```
using (dataContext = new CassandraDataContext(new[] { "127.0.0.1" }, "demo"))
{
  var topUsers = dc.Select<UserResult>(u => u.result >= 90);
}
```

And simple domain model interaction in order to add / update entities

```
using (dataContext = new CassandraDataContext(new[] { "127.0.0.1" }, "demo"))
{
  var userResult = new UserResult
  {
    result = 99.99F,
    timestamp = DateTime.Now,
    user_id = "user_1"
  };
  
  dataContext.AddOrUpdate(userResult)
}
```

### Mapping attribute names

In most cases we would like to seperate the DB schema from the domain model.
In order to map the property names to the matching column names in the cassandra tables we can use the CassandraPropertyAttribute.

```

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

```

And the query will work now on the mapped properties.

```

using (dataContext = new CassandraDataContext(new[] { "127.0.0.1" }, "demo"))
{
  var result = dataContext.SingleOrDefault<UserResultModelWithMapping>(u => u.UserId == "test_user_id" && u.Timestamp == timestamp);
}

```

### Ignoring specific attributes

In some of the cases we have domain models with internal properties we don't want the DB to store.
In order to use these we can use the CassandraIgnoreAttribute.
The data context will know to ignore these properties on add and map.

```

[CassandraTable("user_results")]
public class UserResultModelWithIgnoreProperty
{
  [CassandraProperty("user_id")]
  public string UserId { get; set; }
  [CassandraProperty("time")]
  public DateTimeOffset Timestamp { get; set; }
  [CassandraProperty("result")]
  public float Result { get; set; }

  [CassandraIgnore]
  public int ResultFactor { get; set; }
}

```


### Good Luck!
