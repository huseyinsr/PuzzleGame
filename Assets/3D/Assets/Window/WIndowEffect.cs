using UnityEngine;

public class SlowMaterialOffset : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Vector2 speed = new Vector2(0.01f, 0.005f);

    private Material material;
    private Vector2 offset;

    private void Start()
    {
        material = targetRenderer.material;
    }

    private void Update()
    {
        offset += speed * Time.deltaTime;
        material.mainTextureOffset = offset;
    }
}

