using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class SearchResults : EditorWindow
    {
        private List<PropertyInfo> _propertyInfos;
        
        public void Initialize(List<PropertyInfo> propertyInfos)
        {
            _propertyInfos = propertyInfos;
        }

        public void Dispose()
        {
            _propertyInfos = default;
        }
        
        private void OnGUI()
        {
            if (_propertyInfos == null)
            {
                return;
            }
            
            foreach (var propertyInfo in _propertyInfos)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Name: {propertyInfo.Name}");
                GUILayout.Space(10);
                GUILayout.Label($"Type: {propertyInfo.Type}");
                GUILayout.Space(10);
                // GUILayout.Label($"Path: {propertyInfo.Path}");
                EditorGUILayout.ObjectField("Reference: ", propertyInfo.Object, typeof(Object), false);
                GUILayout.EndHorizontal();
            }
        }
    }
}