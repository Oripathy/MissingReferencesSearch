using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class MissingReferencesSearchTool : EditorWindow
    {
        [MenuItem("Tool/Missing References Search Tool")]
        public static void ShowWindow()
        {
            GetWindow<MissingReferencesSearchTool>("Missing References Search Tool");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Search For Missing References In All Assets"))
            {
                Search();
            }
        }
        
        private void Search()
        {
            var assetPaths = SearchForAllAssets();
            var propertyInfos = SearchForPropertiesWithMissingObjectReferences(assetPaths).ToList();
            var searchResultsWindow = GetWindow<SearchResults>("Search Results");
            searchResultsWindow.Clear();
            searchResultsWindow.Initialize(propertyInfos);
        }

        private List<string> SearchForAllAssets()
        {
            var assetsGUIDs = AssetDatabase.FindAssets("t:Object", new []{"Assets/"});
            return assetsGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToList();
        }

        private IEnumerable<PropertyInfo> SearchForPropertiesWithMissingObjectReferences(List<string> assetPaths)
        {
            foreach (var assetPath in assetPaths)
            {
                if (TryLoadObject(assetPath, out GameObject obj))
                {
                    var gameObjectPropertyInfos = SearchForMissingReferencesInGameObject(obj, assetPath);
                    foreach (var gameObjectPropertyInfo in gameObjectPropertyInfos)
                    {
                        yield return gameObjectPropertyInfo;
                    }
                }

                if (!TryLoadObject(assetPath, out ScriptableObject scriptableObject))
                {
                    continue;
                }
                
                var scriptableObjectPropertyInfos =
                    SearchForMissingReferencesInScriptableObject(scriptableObject, assetPath);
                foreach (var scriptableObjectPropertyInfo in scriptableObjectPropertyInfos)
                {
                    yield return scriptableObjectPropertyInfo;
                }
            }
        }

        private bool TryLoadObject<TObj>(string assetPath, out TObj obj)
            where TObj : Object
        {
            obj = AssetDatabase.LoadAssetAtPath<TObj>(assetPath);
            return obj != null;
        }

        private IEnumerable<PropertyInfo> SearchForMissingReferencesInGameObject(GameObject obj, string path)
        {
            var components = obj.GetComponentsInChildren<Component>(true);
            foreach (var component in components)
            {
                if (component == null)
                {
                    Debug.LogWarning("Component on object is missing", obj);
                    continue;
                }
                
                using var serializedObject = new SerializedObject(component);
                using var serializedProperty = serializedObject.GetIterator();
                var tempPropertyInfos =
                    SearchForMissingReferencesInProperties(serializedProperty, component, path, obj);
                foreach (var tempPropertyInfo in tempPropertyInfos)
                {
                    yield return tempPropertyInfo;
                }
            }
        }

        private IEnumerable<PropertyInfo> SearchForMissingReferencesInScriptableObject(Object scriptableObject,
            string path)
        {
            using var serializedObject = new SerializedObject(scriptableObject);
            using var serializedProperty = serializedObject.GetIterator();
            var propertyInfos =
                SearchForMissingReferencesInProperties(serializedProperty, null, path, scriptableObject);
            foreach (var propertyInfo in propertyInfos)
            {
                yield return propertyInfo;
            }
        }

        private IEnumerable<PropertyInfo> SearchForMissingReferencesInProperties(SerializedProperty serializedProperty,
            Object component, string path, Object obj)
        {
            var propertyInfos = new List<PropertyInfo>();
            while (serializedProperty.NextVisible(true))
            {
                var isObjectReference = serializedProperty.propertyType == SerializedPropertyType.ObjectReference;
                if (!isObjectReference)
                {
                    continue;
                }
                
                var isNull = serializedProperty.objectReferenceValue == null;
                if (!isNull)
                {
                    continue;
                }
                
                var isInstanceIDValueEqualsZero = serializedProperty.objectReferenceInstanceIDValue == 0;
                if (isInstanceIDValueEqualsZero)
                {
                    continue;
                }
                
                var componentName = component == null ? "-" : component.ToString();
                var propertyInfo = new PropertyInfo(serializedProperty.displayName, componentName,
                    serializedProperty.propertyPath, path, obj);
                propertyInfos.Add(propertyInfo);
            }
            
            return propertyInfos;
        }
    }
}
