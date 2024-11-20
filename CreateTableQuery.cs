using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QueryForge
{

    public class CreateTableQuery<T> : CreateTableQuery
    {
        public CreateTableQuery(QueryForgeContext context) : base( context,
             typeof(T).Name){
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                if(!Attribute.IsDefined(propertyInfo, typeof(IgnoreField))
                   && !Attribute.IsDefined(propertyInfo, typeof(ForeignKey))
                   )
                    this.AddColumn(propertyInfo.Name,
                        propertyInfoToSql(propertyInfo),Attribute.IsDefined(propertyInfo, typeof(PrimaryKey)),!Attribute.IsDefined(propertyInfo, typeof(NotNull)));
            }
        }

        private string propertyInfoToSql(PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType == typeof(string))
                return "TEXT";
            if (propertyInfo.PropertyType == typeof(int))
                return "INTEGER";
            if (propertyInfo.PropertyType == typeof(Guid))
                return "TEXT"; // Store GUIDs as TEXT
            if (propertyInfo.PropertyType == typeof(bool))
                return "INTEGER"; // Store booleans as INTEGER (0 or 1)
            throw new Exception($@"Property {propertyInfo.Name} of type {propertyInfo.PropertyType} is not supported");

        }
    }

 
public class CreateTableQuery : Query
    {
        private readonly string _tableName;
        private readonly List<string> _columns = new List<string>();

        public CreateTableQuery(QueryForgeContext context, string tableName) : base(context)
        {
            _tableName = tableName;
        }

        public CreateTableQuery AddColumn(string name, string type, bool isPrimaryKey = false, bool isNullable = true)
        {
            var columnDefinition = $"{name} {type}";
            if (isPrimaryKey)
                columnDefinition += " PRIMARY KEY";
            if (!isNullable)
                columnDefinition += " NOT NULL";
            _columns.Add(columnDefinition);
            return this;
        }

        public override string QueryString(Query query = null)
        {
            var columnsDefinition = string.Join(", ", _columns);
            return $"CREATE TABLE IF NOT EXISTS {_tableName} ({columnsDefinition});";
        }
    }
}
