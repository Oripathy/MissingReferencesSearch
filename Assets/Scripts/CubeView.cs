using UnityEngine;

public class CubeView : MonoBehaviour
{
    [SerializeField] private Texture2D _initialTexture;
    [SerializeField] private MeshRenderer _meshRenderer;

    private Material _material;
    
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    private void Awake()
    {
        _material = _meshRenderer.materials[0];
        if (_initialTexture)
        {
            _material.SetTexture(MainTex, _initialTexture);
        }
    }

    private void OnDestroy()
    {
        if (_material)
        {
            Destroy(_material);
        }
    }
}