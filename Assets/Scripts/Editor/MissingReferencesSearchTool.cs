using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public class MissingReferencesSearchTool : UnityEditor.Editor
    {
        [MenuItem("Missing References Search/Search For Missing References In All Assets")]
        public static void Search()
        {
            var assetPaths = SearchForAllAssets();
            var propertyInfos = SearchForPropertiesWithMissingObjectReferences(assetPaths);
            var searchResultsWindow = EditorWindow.GetWindowWithRect<SearchResults>(new Rect(0, 0, 1200, 500));
            searchResultsWindow.Clear();
            searchResultsWindow.Initialize(propertyInfos);
        }

        private static List<string> SearchForAllAssets()
        {
            var assetsGUIDs = AssetDatabase.FindAssets("t:Object", new []{"Assets/"});
            var assetPaths = new List<string>(assetsGUIDs.Length);
            assetPaths.AddRange(assetsGUIDs.Select(AssetDatabase.GUIDToAssetPath));
            return assetPaths;
        }

        private static List<PropertyInfo> SearchForPropertiesWithMissingObjectReferences(List<string> assetPaths)
        {
            var propertyInfos = new List<PropertyInfo>();
            foreach (var assetsPath in assetPaths)
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetsPath);
                if (obj == null)
                {
                    var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetsPath);
                    if (scriptableObject == null)
                    {
                        continue;
                    }

                    var scriptableObjectPropertyInfos =
                        SearchForMissingReferencesInScriptableObject(scriptableObject, assetsPath);
                    propertyInfos.AddRange(scriptableObjectPropertyInfos);
                    continue;
                }

                var gameObjectPropertyInfos = SearchForMissingReferencesInGameObject(obj, assetsPath);
                propertyInfos.AddRange(gameObjectPropertyInfos);
            }

            return propertyInfos;
        }

        private static List<PropertyInfo> SearchForMissingReferencesInGameObject(GameObject obj, string path)
        {
            var propertyInfos = new List<PropertyInfo>();
            var components = obj.GetComponentsInChildren<Component>(true);
            foreach (var component in components)
            {
                if (component == null)
                {
                    Debug.LogWarning("Component on object is missing", obj);
                    continue;
                }
                
                var serializedObject = new SerializedObject(component);
                var serializedProperty = serializedObject.GetIterator();
                var tempPropertyInfos = SearchForMissingReferencesInProperties(serializedProperty, component.name, path, obj);
                propertyInfos.AddRange(tempPropertyInfos);
            }

            return propertyInfos;
        }

        private static List<PropertyInfo> SearchForMissingReferencesInScriptableObject(ScriptableObject scriptableObject, string path)
        {
            var serializedObject = new SerializedObject(scriptableObject);
            var serializedProperty = serializedObject.GetIterator();
            var propertyInfos = SearchForMissingReferencesInProperties(serializedProperty, "-", path, scriptableObject);
            serializedProperty.Dispose();
            serializedObject.Dispose();
            return propertyInfos;
        }

        private static List<PropertyInfo> SearchForMissingReferencesInProperties(SerializedProperty serializedProperty,
            string componentName, string path, Object obj)
        {
            var propertyInfos = new List<PropertyInfo>();
            while (serializedProperty.NextVisible(true))
            {
                if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference &&
                    serializedProperty.objectReferenceValue == null &&
                    serializedProperty.objectReferenceInstanceIDValue != 0)
                {
                    var propertyInfo = new PropertyInfo(serializedProperty.displayName, componentName,
                        serializedProperty.propertyPath, path, obj);
                    propertyInfos.Add(propertyInfo);
                }
            }

            // while (serializedProperty.NextVisible(true))
            // {
            //     if (serializedProperty.propertyType != SerializedPropertyType.ObjectReference)
            //     {
            //         continue;
            //     }
            //
            //     var o = serializedProperty.objectReferenceValue;
            //     if (o == null)
            //         // Debug.Log(path + " " + serializedProperty.name + " v: " + serializedProperty.isInstantiatedPrefab);
            // }

            return propertyInfos;
        }
    }
}
