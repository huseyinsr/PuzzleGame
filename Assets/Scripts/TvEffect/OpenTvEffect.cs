using UnityEngine;

public class TVMaterialStatic : MonoBehaviour
{
    [SerializeField] private Renderer objectRenderer;

    [SerializeField] private float changeTime = 0.08f;
    [SerializeField] private float moveRange = 0.03f;
    [SerializeField] private float smoothSpeed = 8f;

    private Material material;

    private Vector2 startOffset;
    private Vector2 targetOffset;
    private Vector2 currentOffset;

    private float timer;

    private void Start()
    {
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }

        material = objectRenderer.material;

        if (material.HasProperty("_BaseMap"))
        {
            startOffset = material.GetTextureOffset("_BaseMap");
        }
        else
        {
            startOffset = material.mainTextureOffset;
        }

        currentOffset = startOffset;
        targetOffset = startOffset;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= changeTime)
        {
            timer = 0f;
            PickNewOffset();
        }

        currentOffset = Vector2.Lerp(
            currentOffset,
            targetOffset,
            Time.deltaTime * smoothSpeed
        );

        ApplyOffset(currentOffset);
    }

    private void PickNewOffset()
    {
        targetOffset = startOffset + new Vector2(
            Random.Range(-moveRange, moveRange),
            Random.Range(-moveRange, moveRange)
        );
    }

    private void ApplyOffset(Vector2 offset)
    {
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTextureOffset("_BaseMap", offset);
        }
        else
        {
            material.mainTextureOffset = offset;
        }
    }
}