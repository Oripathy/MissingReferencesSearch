using System.Collections.Generic;
using UnityEngine;

namespace ExampleScripts
{
    [CreateAssetMenu(fileName = "ScriptableObjWithListAndSerializableClass", menuName = "ScriptableObjects/ScriptableObjWithListAndSerializableClass")]
    public class ScriptableObjectWithListAndSerializableClass : ScriptableObject
    {
        [SerializeField] private SerializableObjectExample _serializableObjectExample;
        [SerializeField] private List<CubeView> _cubes;
    }
}