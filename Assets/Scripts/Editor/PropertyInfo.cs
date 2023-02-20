using UnityEngine;

namespace Editor
{
    public class PropertyInfo
    {
        public string PropertyName { get; }
        public Object Component { get; }
        public Object Object { get; }

        public PropertyInfo(string propertyName, Object component, Object obj)
        {
            PropertyName = propertyName;
            Component = component;
            Object = obj;
        }
    }
}