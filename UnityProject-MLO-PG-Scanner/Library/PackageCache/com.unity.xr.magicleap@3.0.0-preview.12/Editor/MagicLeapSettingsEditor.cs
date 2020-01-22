using UnityEditor;
using UnityEngine;

using UnityEngine.XR.MagicLeap;

namespace UnityEditor.XR.MagicLeap
{
    [CustomPropertyDrawer(typeof(DisabledAttribute))]
    internal class DisabledDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledScope(true))
                EditorGUI.PropertyField(position, property, label);
        }
    }
}
