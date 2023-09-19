using System;
using UnityEngine;

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