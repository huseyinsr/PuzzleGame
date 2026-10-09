using UnityEngine;

public class TVStaticSwap : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private SpriteRenderer screen;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private float changeTime = 1f;
    [SerializeField] private bool randomOrder = false;

    [Header("Edge Only")]
    [SerializeField] private bool edgeOnly = false;
    [SerializeField] private SpriteRenderer edgeReference;

    [Header("Screen Edges")]
    [SerializeField] private bool darkEdges = true;
    [SerializeField] private Color edgeColor = Color.black;

    [Range(0f, 1f)]
    [SerializeField] private float edgeDarkness = 0.7f;

    [Range(0.05f, 0.8f)]
    [SerializeField] private float edgeSize = 0.3f;

    [Range(0.2f, 5f)]
    [SerializeField] private float edgeSoftness = 1.5f;

    [Range(0f, 1f)]
    [SerializeField] private float cornerDarkness = 0.5f;

    [SerializeField] private int overlayResolution = 128;

    private float timer;
    private int currentSprite;

    private Vector3 startScale;
    private Vector2 targetSize;

    private SpriteRenderer edgeOverlay;
    private Texture2D edgeTexture;

    private void Start()
    {
        if (screen == null)
        {
            screen = GetComponent<SpriteRenderer>();
        }

        CreateEdgeOverlay();

        if (edgeOnly)
        {
            SetEdgeOnly();
            return;
        }

        if (screen == null || sprites.Length == 0)
        {
            return;
        }

        startScale = screen.transform.localScale;

        targetSize = new Vector2(
            sprites[0].bounds.size.x,
            sprites[0].bounds.size.y
        );

        ChangeToSprite(0);
    }

    private void Update()
    {
        if (edgeOnly)
        {
            if (screen != null)
            {
                screen.enabled = false;
            }

            UpdateEdgePosition();
            return;
        }

        if (screen == null || sprites.Length == 0)
        {
            return;
        }

        screen.enabled = true;

        timer += Time.deltaTime;

        if (timer >= changeTime)
        {
            timer = 0f;
            ChangeSprite();
        }
    }

    private void SetEdgeOnly()
    {
        if (screen != null)
        {
            screen.enabled = false;
        }

        UpdateEdgePosition();
    }

    private void UpdateEdgePosition()
    {
        if (edgeOverlay == null)
        {
            return;
        }

        SpriteRenderer reference = edgeReference;

        if (reference == null)
        {
            reference = screen;
        }

        if (reference == null || reference.sprite == null)
        {
            return;
        }

        edgeOverlay.transform.position =
            reference.transform.position;

        edgeOverlay.transform.rotation =
            reference.transform.rotation;

        Vector2 spriteSize =
            reference.sprite.bounds.size;

        Vector3 referenceScale =
            reference.transform.lossyScale;

        edgeOverlay.transform.localScale =
            new Vector3(
                spriteSize.x * referenceScale.x,
                spriteSize.y * referenceScale.y,
                1f
            );

        edgeOverlay.sortingLayerID =
            reference.sortingLayerID;

        edgeOverlay.sortingOrder =
            reference.sortingOrder + 10;

        edgeOverlay.enabled = darkEdges;
    }

    private void ChangeSprite()
    {
        if (randomOrder)
        {
            int newSprite = currentSprite;

            if (sprites.Length > 1)
            {
                while (newSprite == currentSprite)
                {
                    newSprite = Random.Range(
                        0,
                        sprites.Length
                    );
                }
            }

            currentSprite = newSprite;
        }
        else
        {
            currentSprite++;

            if (currentSprite >= sprites.Length)
            {
                currentSprite = 0;
            }
        }

        ChangeToSprite(currentSprite);
    }

    private void ChangeToSprite(int index)
    {
        Sprite sprite = sprites[index];

        screen.sprite = sprite;

        float scaleX =
            targetSize.x / sprite.bounds.size.x;

        float scaleY =
            targetSize.y / sprite.bounds.size.y;

        screen.transform.localScale =
            new Vector3(
                startScale.x * scaleX,
                startScale.y * scaleY,
                startScale.z
            );

        UpdateEdgePosition();
    }

    private void CreateEdgeOverlay()
    {
        GameObject overlay =
            new GameObject("TV Edge Effect");

        edgeOverlay =
            overlay.AddComponent<SpriteRenderer>();

        CreateEdgeTexture();

        edgeOverlay.sprite = Sprite.Create(
            edgeTexture,
            new Rect(
                0,
                0,
                edgeTexture.width,
                edgeTexture.height
            ),
            new Vector2(0.5f, 0.5f),
            overlayResolution
        );

        edgeOverlay.enabled = darkEdges;
    }

    private void CreateEdgeTexture()
    {
        edgeTexture = new Texture2D(
            overlayResolution,
            overlayResolution,
            TextureFormat.RGBA32,
            false
        );

        edgeTexture.filterMode =
            FilterMode.Bilinear;

        for (int y = 0; y < overlayResolution; y++)
        {
            for (int x = 0; x < overlayResolution; x++)
            {
                float u =
                    (float)x /
                    (overlayResolution - 1);

                float v =
                    (float)y /
                    (overlayResolution - 1);

                float horizontal =
                    Mathf.Abs(u - 0.5f) * 2f;

                float vertical =
                    Mathf.Abs(v - 0.5f) * 2f;

                float edgeDistance =
                    Mathf.Max(
                        horizontal,
                        vertical
                    );

                float edgeStart =
                    1f - edgeSize;

                float edge =
                    Mathf.InverseLerp(
                        edgeStart,
                        1f,
                        edgeDistance
                    );

                edge = Mathf.SmoothStep(
                    0f,
                    1f,
                    edge
                );

                edge = Mathf.Pow(
                    edge,
                    edgeSoftness
                );

                float cornerDistance =
                    Mathf.Sqrt(
                        horizontal * horizontal +
                        vertical * vertical
                    );

                float corner =
                    Mathf.InverseLerp(
                        0.8f,
                        1.4f,
                        cornerDistance
                    );

                float alpha =
                    edge * edgeDarkness;

                alpha +=
                    corner *
                    cornerDarkness;

                alpha = Mathf.Clamp01(alpha);

                Color pixel = edgeColor;
                pixel.a *= alpha;

                edgeTexture.SetPixel(
                    x,
                    y,
                    pixel
                );
            }
        }

        edgeTexture.Apply();
    }

    private void OnDestroy()
    {
        if (edgeTexture != null)
        {
            Destroy(edgeTexture);
        }

        if (edgeOverlay != null)
        {
            Destroy(edgeOverlay.gameObject);
        }
    }
}