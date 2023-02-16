using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObj", menuName = ("ScriptableObjects/ScriptableObj"))]
public class ScriptableObj : ScriptableObject
{
    [SerializeField] private CircleView _circle;
    [SerializeField] private Shader _shader;
}