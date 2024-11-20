using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using QueryForge.Exceptions;
using UnityEngine;

namespace QueryForge
{
    
    
    public class SelectQuery : Query
    {
        protected readonly string _tableName;
        protected Query _whereQuery;
        protected string _alias;
        private Dictionary<string, Query> _columns = new Dictionary<string, Query>();
        private readonly Type _type;
        private bool _isArray;

        private Dictionary<string, Query> _includes = new Dictionary<string, Query>();
        private List<string> _includesList = new List<string>();
        public SelectQuery(Type type, QueryForgeContext context, string tableName, string alias = null, bool isArray = true) 
            : base(context)
        {
            _type = type;
            _isArray = isArray;
            _tableName = tableName;
            _alias = alias ?? tableName;

            InitializeColumns();
        }

        // Initialize columns based on the properties of the type
        private void InitializeColumns()
        {
            foreach (var propertyInfo in _type.GetProperties())
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
                AddColumn(propertyInfo.Name, propertyInfo.Name);
            }
        }

        // Handle properties marked with the ForeignKey attribute
        private void HandleForeignKeyProperty(PropertyInfo propertyInfo, ForeignKey foreignKeyAttr, bool isIEnumerable)
        {
            var primaryKeyName = GetPrimaryKeyName();
            if (primaryKeyName == null)
                throw new Exception($"The property {_type.Name} does not have a primary key");

            var myTableName = _alias;
            var onTableName = propertyInfo.PropertyType.Name;

            if (isIEnumerable)
            {
                var ienumerableType = propertyInfo.PropertyType.GenericTypeArguments[0];
                onTableName = ienumerableType.Name;

                AddInclude(propertyInfo.Name,
                    new SelectQuery(ienumerableType, _context, onTableName, propertyInfo.Name, true)
                        .Where($"{myTableName}.{foreignKeyAttr.ParentField} = {propertyInfo.Name}.{foreignKeyAttr.ChildField}")
                );
            }
            else
            {
                AddInclude(propertyInfo.Name,
                    new SelectQuery(propertyInfo.PropertyType, _context, onTableName, propertyInfo.Name, false)
                        .Where($"{myTableName}.{foreignKeyAttr.ParentField} = {propertyInfo.Name}.{foreignKeyAttr.ChildField}")
                );
            }
        }

        // Retrieve the primary key name for the current type
        private string GetPrimaryKeyName()
        {
            foreach (var property in _type.GetProperties())
            {
                if (Attribute.IsDefined(property, typeof(PrimaryKey)))
                {
                    return property.Name;
                }
            }
            return null;
        }


        public SelectQuery Where(string rawWhere)
        {
            _whereQuery = new Query(_context, $@"where {rawWhere}");
            return this;
        }

        public SelectQuery AddColumn(string columnName, string columnValue)
        {
            _columns.Add(columnName, new Query(_context, this._alias + '.' + columnValue));
            return this;
        }


        public SelectQuery AddColumn(string columnName, Query query)
        {
            _columns.Add(columnName, query);
            return this;
        }
        
        
        public SelectQuery AddInclude(string columnName, Query query)
        {
            _includes.Add(columnName, query);
            return this;
        }

        public SelectQuery Include(string columnName)
        {
            this._includesList.Add(columnName);
            return this;
        }

        public override string QueryString(Query query = null)
        {
            if (query != null && typeof(SelectQuery).IsAssignableFrom(query.GetType()))
            {
                var selectQuery = query as SelectQuery;
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

            return _isArray ? BuildArrayQueryString() : BuildObjectQueryString();
        }

        // Build the query string for array results
        private string BuildArrayQueryString()
        {
            return $"SELECT json_group_array(json_object({ConvertColumnsToJSON()})) as {_alias}_JSON FROM {_tableName} AS {_alias}"
                   + (_whereQuery == null ? "" : $@" {_whereQuery.QueryString()}");
        }

        // Build the query string for single object results
        private string BuildObjectQueryString()
        {
            return $"SELECT json_object({ConvertColumnsToJSON()}) as {_alias}_JSON FROM {_tableName} AS {_alias}"
                   + (_whereQuery == null ? "" : $@" {_whereQuery.QueryString()}");
        }

        // Convert columns to JSON format for the query
        private string ConvertColumnsToJSON()
        {
            if (_columns == null || _columns.Count == 0)
                throw new Exception($@"Columns cannot be null for type {_type.Name}");

            var cols = _columns.ToList();
            foreach (var include in _includesList)
            {
                var inc = _includes.Any(g => g.Key == include);
                if (!include.StartsWith(_alias + ".") && !include.Contains("."))
                {
                    if(!inc)
                        throw new ForeignKeyNotFoundException(
                        $@"Include {inc} is not in the Foreign Key List make sure to use [ForeignKey] Attribute or check your spelling");
                    cols.Add(_includes.First(g => g.Key == include));
                }
            }

            return cols.Select(g => $@"'{g.Key}',({g.Value.QueryString(this)})").Aggregate((a, b) => a + "," + b);
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


    }



    public class SelectQuery<T> : SelectQuery
    {

        public SelectQuery(QueryForgeContext context, string alias = null, bool isArray = true) :
            base(typeof(T), context, typeof(T).Name, alias, isArray)
        {
        }

        public List<T> ToList() => this.Execute<T>();


        public SelectQuery<T> Where(string rawWhere)
        {
            base.Where(rawWhere);
            return this;
        }

        public SelectQuery<T> AddColumn(string columnName, string columnValue)
        {
            base.AddColumn(columnName, columnValue);

            return this;
        }


        public  SelectQuery<T> AddColumn(string columnName, Query query)
        {
            base.AddColumn(columnName,query);
            return this;
        }
        
        
        public SelectQuery<T> AddInclude(string columnName, Query query)
        {
          base.AddInclude(columnName, query);
          return this;
        }

        public SelectQuery<T> Include(string columnName)
        {
           base.Include(columnName);
           return this;
        }


    }
}
