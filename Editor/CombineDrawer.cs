using System;
using UnityEditor;
using UnityEngine;

namespace LayerFx.Editor
{
    [CustomPropertyDrawer(typeof(Combine.Output))]
    public class CombineDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var mode  = (Combine.Output.Target)property.FindPropertyRelative(nameof(Combine._output._target)).intValue;
            var lines = 1;
            if (mode == Combine.Output.Target.GlobalTex)
                lines += 2;
            
            return lines * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var target    = property.FindPropertyRelative(nameof(Combine.Output._target));
            var globalTex = property.FindPropertyRelative(nameof(Combine.Output._globalTex));
            var clear = property.FindPropertyRelative(nameof(Combine.Output._clear));
            
            var line = 0;
            
            EditorGUI.PropertyField(_fieldRect(line ++), target, label, true);
            switch ((Combine.Output.Target)target.intValue)
            {
                case Combine.Output.Target.Camera:
                    break;
                case Combine.Output.Target.GlobalTex:
                    EditorGUI.PropertyField(_fieldRect(line ++), globalTex, true);
                    EditorGUI.PropertyField(_fieldRect(line ++), clear, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // -----------------------------------------------------------------------
            Rect _fieldRect(int line)
            {
                return new Rect(position.x, position.y + line * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            }
        }
    }
}