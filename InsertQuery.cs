using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QueryForge
{
    public class InsertQuery<T> : Query
    {
        private readonly string _tableName;
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public InsertQuery(QueryForgeContext context, T model) : base(context)
        {
            _tableName = typeof(T).Name;
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                if(!Attribute.IsDefined(propertyInfo, typeof(IgnoreField))
                   && !Attribute.IsDefined(propertyInfo, typeof(ForeignKey))
                  )
                {
                    var value = propertyInfo.GetValue(model);
                    _values[propertyInfo.Name] = value;
                }
            }
        }

        public InsertQuery<T> AddValue(string columnName, object value)
        {
            _values[columnName] = value;
            return this;
        }

        public override string QueryString(Query query = null)
        {
            var columns = string.Join(", ", _values.Keys);
            var values = string.Join(", ", _values.Values.Select(v => FormatValue(v)));
            return $"INSERT INTO {_tableName} ({columns}) VALUES ({values});";
        }
        
        private string FormatValue(object value)
        {
            if (value is string || value is Guid)
            {
                return $"'{value}'";
            }
            if (value is bool)
            {
                return (bool)value ? "1" : "0";
            }
            return value.ToString();
        }
    }
}
