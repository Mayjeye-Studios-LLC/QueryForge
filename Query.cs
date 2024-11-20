using System;
using System.Collections;
using System.Collections.Generic;

namespace QueryForge
{
    public static class QueryForgeTestExtensions
    {
        public static string ToJson(this object ob) => Newtonsoft.Json.JsonConvert.SerializeObject(ob);
    }
    public class Query
    {
        protected readonly QueryForgeContext _context;
        private string output;

        public Query(QueryForgeContext context)
        {
            _context = context;
        }

        public Query(QueryForgeContext context,string output)
        {
            this.output = output;
        }

        public virtual string QueryString(Query query = null) => output;

        public virtual List<T> Execute<T>()
        {
            return this._context.ExecuteQuery<T>(this);
        }
        
        public virtual object Execute(Type type)
        {
            return this._context.ExecuteQuery(type,this);
        }
    }
}