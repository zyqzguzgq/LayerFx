using System;
using UnityEngine;

//  LayerFx © NullTale - https://twitter.com/NullTale/
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