using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QueryForge
{
    public class UpdateQuery : Query
    {
        private readonly string _tableName;
        private readonly object _model;
        private readonly Dictionary<string, object> _setValues = new Dictionary<string, object>();
        private string _whereClause;
        protected string _alias;

        private Dictionary<string, Query> _includes = new Dictionary<string, Query>();
        private List<string> _includesList = new List<string>();
   
        public UpdateQuery(QueryForgeContext context, string tableName, object model,string alias = null) : base(context)
        {
            _tableName = tableName;
            _alias = alias;
            if (this._alias == null)
                this._alias = this._tableName;
            _model = model;
            InitializeColumns();
        }

        // Initialize columns based on the properties of the type
        private void InitializeColumns()
        {
            
            foreach (var propertyInfo in _model.GetType().GetProperties())
            {
                if (!Attribute.IsDefined(propertyInfo, typeof(IgnoreField)))
                {
                   
                    HandleProperty(propertyInfo);


                }
            }
            
           
        }
        // Handle individual properties to determine how they should be included in the query
        private void HandleProperty(PropertyInfo propertyInfo)
        {
            var isIEnumerable = typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType) &&
                                propertyInfo.PropertyType != typeof(string);

            var foreignKeyAttr = propertyInfo.GetCustomAttribute<ForeignKey>();
            if (foreignKeyAttr != null)
            {
                HandleForeignKeyProperty(propertyInfo, foreignKeyAttr, isIEnumerable);
            }
            else
            {
                var value = propertyInfo.GetValue(_model);
                _setValues[propertyInfo.Name] = value;
            }
        }
        private string GetPrimaryKeyName(object ob)
        {
            foreach (var property in ob.GetType().GetProperties())
            {
                if (Attribute.IsDefined(property, typeof(PrimaryKey)))
                {
                    return property.Name;
                }
            }
            return null;
        }
        
        // Handle properties marked with the ForeignKey attribute
        private void HandleForeignKeyProperty(PropertyInfo propertyInfo, ForeignKey foreignKeyAttr, bool isIEnumerable)
        {
            var propertyValue = propertyInfo.GetValue(_model);
            
            if (isIEnumerable)
            {
                var ienumerableType = propertyInfo.PropertyType.GenericTypeArguments[0];
                var ienumerableValue = propertyValue as IEnumerable;
                if (ienumerableValue == null)
                    return;
                List<Query> updates = new List<Query>();
                foreach (var item in ienumerableValue)
                {
                    var primaryKey = GetPrimaryKeyName(item);
                    var formattedValue = FormatValue(item.GetType().GetProperty(primaryKey).GetValue(item));
                
                    var updateQuery = new UpdateQuery(this._context, ienumerableType.Name,item, propertyInfo.Name)
                        .Where($"{primaryKey} = {formattedValue}");
                    updates.Add(updateQuery);
                }
                
               this.AddInclude(propertyInfo.Name,new ListQuery(this._context,updates));
       
            }
            else
            {
                var primaryKey = GetPrimaryKeyName(propertyValue);
                var formattedValue = FormatValue(propertyValue.GetType().GetProperty(primaryKey).GetValue(propertyValue));
                var updateQuery = new UpdateQuery(this._context, propertyValue.GetType().Name,propertyValue,propertyInfo.Name)
                    .Where($"{primaryKey} = {formattedValue}");
                this.AddInclude(propertyInfo.Name,updateQuery);

            }
        }

        
        


        public UpdateQuery Set(string columnName, object value)
        {
            _setValues[columnName] = value;
            return this;
        }

        public UpdateQuery AddInclude(string include, Query includeQuery)
        {
            this._includes.Add(include, includeQuery);
            return this;
        }

        public UpdateQuery Include(string include)
        {
            _includesList.Add(include);
            return this;
        }

        public UpdateQuery Where(string whereClause)
        {
            _whereClause = whereClause;
            return this;
        }
        public List<string> GetIncludes()
        {
            return _includesList
                .Where(g => g.Contains(".")).Select(g =>
                {
                    if (g.StartsWith(_alias + "."))
                        g.Substring(_alias.Length + 1, g.Length - (_alias.Length + 1));
                    return g;
                })
                .ToList();
        }

        public override string QueryString(Query query = null)
        {
            
            
            if (query != null && typeof(UpdateQuery).IsAssignableFrom(query.GetType()))
            {
                var selectQuery = query as UpdateQuery;
                var parentIncludes = selectQuery.GetIncludes();
                foreach (var include in parentIncludes)
                {
                    if (!include.StartsWith(_alias + "."))
                    {
                        Include(include);
                    }
                    else
                    {
                        Include(include.Substring(_alias.Length + 1, include.Length - (_alias.Length + 1)));
                        this._includesList.Remove(include); // remove the parented include that has been reduced so the child does not get tit
                        
                    }
                }
            }
            
            
            var setClause = string.Join(", ", _setValues.Select(kv => $"{kv.Key} = {FormatValue(kv.Value)}"));
            var baseQueryString = $"UPDATE {_tableName} SET {setClause} WHERE {_whereClause};";

            // Add logic to handle includes
            foreach (var include in _includesList)
            {
                if (!include.StartsWith(this._tableName + ".") && !include.Contains("."))
                {
                    var includeSql = this._includes[include].QueryString(this);
                    // Generate SQL for updating related entities
                    // Example: Update related Inventory records for a Player
                    // This is a placeholder for actual logic
                    baseQueryString += $" -- Include logic for {include} \n\n {includeSql}";
                }

              
            }

            return baseQueryString;
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

    internal class ListQuery : Query
    {
        private readonly List<Query> _lst;

     
        public ListQuery(QueryForgeContext context, List<Query> lst):base(context)
        {
            _lst = lst;
        }

        override public string QueryString(Query query = null)
        {
            return string.Join(" ", _lst.Select(q => q.QueryString(query)));
        }
    }
    
    public class UpdateQuery<T> : UpdateQuery
    {

        public UpdateQuery(QueryForgeContext context, object model,string alias = null) : base(context,typeof(T).Name,model,alias)
        {

        }
        public List<T> ToList() => this.Execute<T>();


        public UpdateQuery<T> Where(string rawWhere)
        {
            base.Where(rawWhere);
            return this;
        }


        public UpdateQuery<T> AddInclude(string columnName, Query query)
        {
            base.AddInclude(columnName, query);
            return this;
        }

        public UpdateQuery<T> Include(string columnName)
        {
            base.Include(columnName);
            return this;
        }


    }

  
}
