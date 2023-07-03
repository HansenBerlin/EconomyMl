using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NewScripts
{
    public class PropertyConverter<T, TA>
    {
        public List<object> GetCorrspondingValues(T enumtype, List<TA> aggregates)
        {
            var values = aggregates
                .GetRange(0, aggregates.Count - 1)
                .Select(x => GetProperty(x, enumtype.ToString()).GetValue(x))
                .ToList();
            return values;
        }

        public PropertyInfo GetProperty(object obj, string propertyName)
        {
            Type type = obj.GetType();
            var property = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == propertyName);
            return property;
        }
    }
}