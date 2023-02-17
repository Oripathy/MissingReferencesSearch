using UnityEngine;

namespace ExampleScripts
{
    [CreateAssetMenu(fileName = "ScriptableObj", menuName = ("ScriptableObjects/ScriptableObj"))]
    public class ScriptableObjectWithFields : ScriptableObject
    {
        [SerializeField] private CircleView _circle;
        [SerializeField] private Shader _shader;
    }
}