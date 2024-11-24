using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace QueryForge
{
    public class IntegerToBooleanConverter : JsonConverter<bool>
    {
        public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
        {
            writer.WriteValue(value ? 1 : 0);
        }

        public override bool ReadJson(JsonReader reader, Type objectType, bool existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            int intValue = Convert.ToInt32(reader.Value);
            return intValue == 1;
        }
    }
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