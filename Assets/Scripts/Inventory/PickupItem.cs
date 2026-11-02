using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private ItemData item;
    [SerializeField] private bool requiresZoom = true;
    [SerializeField] private float minZoomTime = 0.2f;

    private void OnMouseDown()
    {
        if (requiresZoom)
        {
            if (!ZoomManager.Instance.IsZoomed) return;
            if (Time.time - ZoomManager.Instance.ZoomStartTime < minZoomTime) return;
        }

        if (item != null && Inventory.Instance != null)
        {
            Inventory.Instance.AddItem(item);
            SoundEffect.Instance?.PlayPickup();
            Destroy(gameObject);
        }
    }
}
