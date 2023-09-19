using System;
using UnityEngine;

//  LayerFx © NullTale - https://twitter.com/NullTale/
namespace LayerFx
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SocTypesAttribute : PropertyAttribute
    {
        public Type[] Types;

        public SocTypesAttribute(params Type[] types)
        {
            Types = types;
        }
    }
}