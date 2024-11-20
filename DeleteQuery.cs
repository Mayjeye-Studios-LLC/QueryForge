using System;

namespace QueryForge
{
    public class DeleteQuery<T> : Query
    {
        private readonly string _tableName;
        private string _whereClause;

        public DeleteQuery(QueryForgeContext context) : base(context)
        {
            _tableName = typeof(T).Name;
        }

        public DeleteQuery<T> Where(string whereClause)
        {
            _whereClause = whereClause;
            return this;
        }

        public override string QueryString(Query query = null)
        {
            if (string.IsNullOrEmpty(_whereClause))
            {
                throw new InvalidOperationException("A WHERE clause is required for DELETE queries to prevent accidental data loss.");
            }
            return $"DELETE FROM {_tableName} WHERE {_whereClause};";
        }
    }
}
