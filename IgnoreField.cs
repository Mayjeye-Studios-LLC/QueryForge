using System;

namespace QueryForge
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IgnoreField : Attribute
    {
    }
    
    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrimaryKey : Attribute
    {
    }
    
    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NotNull : Attribute
    {
    }
    
    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ForeignKey: Attribute
    {
        public string ParentField { get; }
        public string ChildField{ get; }
        public ForeignKey( string parentField, string childField )
        {
            ParentField = parentField;
            ChildField = childField;
        }
    }
}
