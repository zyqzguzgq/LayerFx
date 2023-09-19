using System;
using UnityEngine;

namespace LayerFx
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SocUniqueAttribute : PropertyAttribute
    {
        public Type[] _except;
        
        public SocUniqueAttribute(params Type[] except)
        {
            _except = except;
        }
    }
}