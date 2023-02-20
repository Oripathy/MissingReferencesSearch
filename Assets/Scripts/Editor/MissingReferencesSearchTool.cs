using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class MissingReferencesSearchTool : EditorWindow
    {
        [MenuItem("Tools/Missing References Search Tool")]
        public static void ShowWindow()
        {
            GetWindowWithRect<MissingReferencesSearchTool>(new Rect(0f, 0f, 300f, 100f), false,
                "Missing References Search Tool");
        }

        private void OnGUI()
        {
            var guiStyle = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                wordWrap = true
            };
            
            GUILayout.Label(new GUIContent(
                    "This tool searches for missing references in all assets (including it's child objects and scriptable objects) in Assets/"),
                guiStyle);
            GUILayout.Space(10);
            if (GUILayout.Button("Search For Missing References"))
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
                    var gameObjectPropertyInfos = SearchForMissingReferencesInGameObject(obj);
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
                    SearchForMissingReferencesInScriptableObject(scriptableObject);
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

        private IEnumerable<PropertyInfo> SearchForMissingReferencesInGameObject(GameObject obj)
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
                    SearchForMissingReferencesInProperties(serializedProperty, component,  obj);
                foreach (var tempPropertyInfo in tempPropertyInfos)
                {
                    yield return tempPropertyInfo;
                }
            }
        }

        private IEnumerable<PropertyInfo> SearchForMissingReferencesInScriptableObject(Object scriptableObject)
        {
            using var serializedObject = new SerializedObject(scriptableObject);
            using var serializedProperty = serializedObject.GetIterator();
            var propertyInfos =
                SearchForMissingReferencesInProperties(serializedProperty, null, scriptableObject);
            foreach (var propertyInfo in propertyInfos)
            {
                yield return propertyInfo;
            }
        }

        private IEnumerable<PropertyInfo> SearchForMissingReferencesInProperties(SerializedProperty serializedProperty,
            Object component, Object obj)
        {
            var propertyInfos = new List<PropertyInfo>();
            while (serializedProperty.NextVisible(true))
            {
                var isObjectReference = serializedProperty.propertyType == SerializedPropertyType.ObjectReference;
                if (!isObjectReference)
                {
                    continue;
                }
                
                var isValueEqualsNull = serializedProperty.objectReferenceValue == null;
                var isInstanceIDValueEqualsZero = serializedProperty.objectReferenceInstanceIDValue == 0;
                if (!isValueEqualsNull || isInstanceIDValueEqualsZero)
                {
                    continue;
                }
                
                var componentForSelection = component == null ? obj : component;
                var propertyInfo = new PropertyInfo(serializedProperty.displayName, componentForSelection, obj);
                propertyInfos.Add(propertyInfo);
            }
            
            return propertyInfos;
        }
    }
}
