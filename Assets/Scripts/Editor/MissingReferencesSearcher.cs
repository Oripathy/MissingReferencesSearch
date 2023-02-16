using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class MissingReferencesSearcher : EditorWindow
    {
        private bool _isProcessing;
        
        [MenuItem("Window/MissingReferencesSearch")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MissingReferencesSearcher));
        }

        private void OnGUI()
        {
            GUILayout.Label("Search For Missing References In All Assets");
            if (!_isProcessing)
            {
                if (GUILayout.Button("Search"))
                {
                    _isProcessing = true;
                    var assetInfos = SearchForAllAssets();
                    var propertyInfos = SearchForPropertiesWithMissingObjectReferences(assetInfos);
                    var searchResultsWindow = GetWindow<SearchResults>();
                    searchResultsWindow.Dispose();
                    searchResultsWindow.Initialize(propertyInfos);
                    _isProcessing = false;
                }
            }
        }

        private List<AssetInfo> SearchForAllAssets()
        {
            var assets = AssetDatabase.FindAssets("t:Object", new []{"Assets/"});
            var assetInfos = new List<AssetInfo>(assets.Length);
            assetInfos.AddRange(assets.Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => new AssetInfo("asset", path)));
            return assetInfos;
        }

        private List<PropertyInfo> SearchForPropertiesWithMissingObjectReferences(List<AssetInfo> assetInfos)
        {
            var propertyInfos = new List<PropertyInfo>();
            foreach (var assetInfo in assetInfos)
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetInfo.Path);
                if (obj == null)
                {
                    Debug.LogWarning("Obj cast to GameObject failed " + assetInfo.Path);
                    var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetInfo.Path);
                    if (scriptableObject == null)
                    {
                        Debug.LogWarning("Obj cast to ScriptableObject failed " + assetInfo.Path);
                        continue;
                    }

                    var scriptableObjectPropertyInfos =
                        SearchForMissingReferencesInScriptableObject(scriptableObject, assetInfo.Path);
                    propertyInfos.AddRange(scriptableObjectPropertyInfos);
                    continue;
                }

                var gameObjectPropertyInfos = SearchForMissingReferencesInGameObject(obj, assetInfo.Path);
                propertyInfos.AddRange(gameObjectPropertyInfos);
            }

            return propertyInfos;
        }

        private List<PropertyInfo> SearchForMissingReferencesInGameObject(GameObject obj, string path)
        {
            var propertyInfos = new List<PropertyInfo>();
            var components = obj.GetComponentsInChildren<Component>(true);
            foreach (var component in components)
            {
                var serializedObject = new SerializedObject(component);
                var serializedProperty = serializedObject.GetIterator();
                var tempPropertyInfos = SearchForMissingReferencesInProperties(serializedProperty, path, obj);
                propertyInfos.AddRange(tempPropertyInfos);
                foreach (var propertyInfo in propertyInfos)
                {
                    Debug.Log(propertyInfo.Name + " " + propertyInfo.Type + " PATH: " + path);
                }
            }

            return propertyInfos;
        }

        private List<PropertyInfo> SearchForMissingReferencesInScriptableObject(ScriptableObject scriptableObject, string path)
        {
            var serializedObject = new SerializedObject(scriptableObject);
            var serializedProperty = serializedObject.GetIterator();
            var propertyInfos = SearchForMissingReferencesInProperties(serializedProperty, path, scriptableObject);
            foreach (var propertyInfo in propertyInfos)
            {
                Debug.Log(propertyInfo.Name + " " + propertyInfo.Type + " PATH: " + path);
            }

            return propertyInfos;
        }

        private List<PropertyInfo> SearchForMissingReferencesInProperties(SerializedProperty serializedProperty, string path, Object obj)
        {
            var propertyInfos = new List<PropertyInfo>();
            while (serializedProperty.NextVisible(true))
            {
                if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference &&
                    serializedProperty.objectReferenceValue == null &&
                    serializedProperty.objectReferenceInstanceIDValue != 0)
                {
                    var propertyInfo = new PropertyInfo(ObjectNames.NicifyVariableName(serializedProperty.name),
                        serializedProperty.type, path, obj);
                    propertyInfos.Add(propertyInfo);
                }
            }

            return propertyInfos;
        }
    }
}
