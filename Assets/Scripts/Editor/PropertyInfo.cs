using UnityEngine;

namespace Editor
{
    public class PropertyInfo
    {
        public string PropertyName { get; }
        public string ComponentName { get; }
        public string PropertyPath { get; }
        public string ObjectPath { get; }
        public Object Object { get; }

        public PropertyInfo(string propertyName, string componentName, string propertyPath, string objectPath, Object o)
        {
            PropertyName = propertyName;
            ComponentName = componentName;
            PropertyPath = propertyPath;
            ObjectPath = objectPath;
            Object = o;
        }
    }
}