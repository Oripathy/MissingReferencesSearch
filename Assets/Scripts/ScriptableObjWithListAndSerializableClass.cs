using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObjWithListAndSerializableClass", menuName = "ScriptableObjects/ScriptableObjWithListAndSerializableClass")]
public class ScriptableObjWithListAndSerializableClass : ScriptableObject
{
    [SerializeField] private SerializableObjectExample _serializableObjectExample;
    [SerializeField] private List<CubeView> _cubes;
}