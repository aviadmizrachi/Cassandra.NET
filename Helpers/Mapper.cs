using System;

namespace Cassandra.NET.Helpers
{
    public static class Mapper
    {
        public static T Map<T>(Row row)
        {
            var mapped = Activator.CreateInstance<T>();
            var properties = mapped.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = row[property.Name];
                property.SetValue(mapped, value);
            }

            return mapped;


        }
    }
}
