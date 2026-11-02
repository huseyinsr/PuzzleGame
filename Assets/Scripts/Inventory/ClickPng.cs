
using UnityEngine;
using UnityEngine.UI;

public class PngPulseBillboard : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Vector3 rotationOffset = new Vector3(0f, 180f, 0f);

    [Header("Pulse")]
    [Min(0f)]
    [SerializeField] private float minScale = 0.9f;

    [Min(0f)]
    [SerializeField] private float maxScale = 1.15f;

    [Min(0f)]
    [SerializeField] private float pulseSpeed = 1f;

    [Header("PNG Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image uiImage;

    private Vector3 originalScale;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (uiImage == null)
            uiImage = GetComponent<Image>();

        originalScale = transform.localScale;
    }

    private void LateUpdate()
    {
        bool isZoomed =
            ZoomManager.Instance != null &&
            ZoomManager.Instance.IsZoomed;

        if (spriteRenderer != null)
            spriteRenderer.enabled = !isZoomed;

        if (uiImage != null)
            uiImage.enabled = !isZoomed;

        if (isZoomed)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            transform.rotation =
                mainCamera.transform.rotation *
                Quaternion.Euler(rotationOffset);
        }

        float t = (
            Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f)
            + 1f
        ) * 0.5f;

        float scale = Mathf.Lerp(
            Mathf.Min(minScale, maxScale),
            Mathf.Max(minScale, maxScale),
            t
        );

        transform.localScale = originalScale * scale;
    }
}
