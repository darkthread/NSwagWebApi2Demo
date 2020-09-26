using System;

namespace Microsoft.AspNetCore.Mvc
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ConsumesAttribute : Attribute
    {
        public string[] ContentTypes { get; set; }
        public ConsumesAttribute(params string[] contentTypes)
        {
            ContentTypes = contentTypes;
        }
    }
}