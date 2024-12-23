using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QueryForge
{
    public class QueryForgeContext:IDisposable
    {
        private readonly string _fileName;
        private SqliteConnection _connection;

        public QueryForgeContext(string fileName)
        {
            _fileName = fileName;

        }

        protected virtual void onCreated()
        {
        }

        protected virtual void onLoaded()
        {
        }

        public void Connect()
        {
            this._connection = new SqliteConnection(_fileName);
            if (this._connection.IsNew)
                onCreated();
            else
                onLoaded();
            
        }


        public void Dispose()
        {
            _connection?.Dispose();
        }

        public SelectQuery<T> Select<T>(string alias)=>new SelectQuery<T>(this, alias,true);
        public SelectQuery<T> Select<T>()=>new SelectQuery<T>(this, null,true);
        public SelectQuery<T> Select<T>(string alias,bool isArray)=>new SelectQuery<T>(this, alias,isArray);
        public SelectQuery<T> Select<T>(bool isArray)=>new SelectQuery<T>(this, null,isArray);

        public List<T> ExecuteQuery<T>(Query query)
        {
            string jsonObject = "";
            try
            {
                jsonObject = ExecuteQuery(query);
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(jsonObject);;
                }
                catch (Exception e)
                {
                    Debug.Log($@"Returned json object did not conform to model ${jsonObject} : Model {typeof(T).FullName}");
                    throw new Exception($@"Returned json object did not conform to model ${jsonObject} : Model {typeof(T).FullName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(
                    new SqlException(e,query.QueryString()));
                Debug.Log($@"Sql query was invalid :{query.QueryString()} ");

                throw new Exception($@"Sql query was invalid ");

            }
        }
        
        
        public object ExecuteQuery(Type type, Query query)
        {
            string jsonObject = "";
            try
            {
                jsonObject = ExecuteQuery(query);
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject(jsonObject,type);
                }
                catch (Exception e)
                {
                    Debug.Log($@"Returned json object did not conform to model ${jsonObject} : Model {type.FullName}");
                    throw new Exception($@"Returned json object did not conform to model ${jsonObject} : Model {type.FullName}");
                }
            }
            catch (Exception e)
            {
                Debug.Log($@"Sql query was invalid :{query.QueryString()} ");

                throw new Exception($@"Sql query was invalid ");

            }
        }
        public string ExecuteQuery(Query query)
        {
            return this._connection.ExecuteSQL(query.QueryString());
        }
        public bool TableExists(string tableName)
        {
            var query = new Query(this, $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';");
            var result = ExecuteQuery(query);
            return !string.IsNullOrEmpty(result);
        }
    }

    public class SqlException : Exception
    {
        public SqlException(Exception exception, string queryString):base($@"Query was invalid :{queryString}",exception)
        {
            //throw new NotImplementedException();
        }
    }
}