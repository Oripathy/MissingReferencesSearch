using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public class SearchResults : EditorWindow
    {
        private List<PropertyInfo> _propertyInfos;
        private Vector2 _scrollPosition = Vector2.zero;
        
        public void Initialize(List<PropertyInfo> propertyInfos)
        {
            _propertyInfos = propertyInfos;
        }

        public void Clear()
        {
            _propertyInfos = default;
        }

        private void OnGUI()
        {
            if (_propertyInfos == null)
            {
                return;
            }

            if (_propertyInfos.Count == 0)
            {
                GUILayout.Label("There Is No Missing References In Assets");
                return;
            }
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (var propertyInfo in _propertyInfos)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Property Name: {propertyInfo.PropertyName}");
                GUILayout.Label($"Component Name: {propertyInfo.ComponentName}");
                GUILayout.Label($"Property Path: {propertyInfo.PropertyPath}");
                EditorGUILayout.ObjectField("Object Reference: ", propertyInfo.Object, typeof(Object), false);
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}