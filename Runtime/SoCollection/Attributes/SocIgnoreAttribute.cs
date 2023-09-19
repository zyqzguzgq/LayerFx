using System;
using UnityEngine;

namespace LayerFx
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SocIgnoreAttribute : PropertyAttribute
    {
    }
}