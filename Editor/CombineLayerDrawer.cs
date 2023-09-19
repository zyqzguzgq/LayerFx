using UnityEditor;
using UnityEngine;

//  LayerFx © NullTale - https://twitter.com/NullTale/
namespace LayerFx.Editor
{
    [CustomPropertyDrawer(typeof(Combine.Layer.Blending))]
    public class CombineLayerDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var mode  = (Combine.Blending)property.FindPropertyRelative(nameof(Combine.Layer._blending)).intValue;
            var lines = 1;
            if (mode == Combine.Blending.Custom)
                lines += 1;
            
            return lines * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var mode = property.FindPropertyRelative(nameof(Combine.Layer._blending._blending));
            var mat  = property.FindPropertyRelative(nameof(Combine.Layer._blending._material));
            var pass = property.FindPropertyRelative(nameof(Combine.Layer._blending._pass));
            
            var line = 0;
            
            EditorGUI.PropertyField(_fieldRect(line ++), mode, label, true);
            if ((Combine.Blending)mode.intValue == Combine.Blending.Custom)
            {
                EditorGUI.PropertyField(_fieldRect(line++), mat, true);
                //EditorGUI.PropertyField(_fieldRect(line++), pass, true);
            }

            // -----------------------------------------------------------------------
            Rect _fieldRect(int line)
            {
                return new Rect(position.x, position.y + line * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            }
        }
    }
}