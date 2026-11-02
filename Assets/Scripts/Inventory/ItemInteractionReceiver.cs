using UnityEngine;

public class ItemInteractionReceiver : MonoBehaviour
{
    [SerializeField] private ItemPrefabMapping[] itemPrefabMappings;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool[] useCustomRotations;
    [SerializeField] private Vector3[] customRotationAngles;
    [SerializeField] private bool requiresZoom = true;
    [SerializeField] private float minZoomTime = 0.2f;

    private bool[] usedItemFlags;
    private bool[] usedSpawnFlags;

    private void Awake()
    {
        usedItemFlags = new bool[itemPrefabMappings.Length];
        usedSpawnFlags = new bool[spawnPoints.Length];
    }

    private void OnMouseDown()
    {
        if (requiresZoom)
        {
            if (!ZoomManager.Instance.IsZoomed) return;
            if (Time.time - ZoomManager.Instance.ZoomStartTime < minZoomTime) return;
        }

        InventorySlot selectedSlot = Inventory.Instance.GetSelectedSlot();
        if (selectedSlot == null || selectedSlot.IsEmpty()) return;

        int itemIndex = System.Array.FindIndex(
            itemPrefabMappings,
            m => m.item == selectedSlot.item
        );

        if (itemIndex == -1) return;
        if (usedItemFlags[itemIndex]) return;

        int spawnIndex = GetNextFreeSpawnPointIndex();
        if (spawnIndex == -1)
        {
            //Debug.LogWarning("No free spawn point available");
            return;
        }

        Transform spawnPoint = spawnPoints[spawnIndex];

        if (itemPrefabMappings[itemIndex].prefab != null)
        {
            Quaternion finalRotation = spawnPoint.rotation;

            if (useCustomRotations != null && itemIndex < useCustomRotations.Length && useCustomRotations[itemIndex])
            {
                if (customRotationAngles != null && itemIndex < customRotationAngles.Length)
                {
                    finalRotation = Quaternion.Euler(customRotationAngles[itemIndex]);
                }
            }

            Instantiate(
                itemPrefabMappings[itemIndex].prefab,
                spawnPoint.position,
                finalRotation
            );
        }

        usedItemFlags[itemIndex] = true;
        usedSpawnFlags[spawnIndex] = true;

        Inventory.Instance.RemoveItem(selectedSlot);
        SoundEffect.Instance?.PlayPlace();
    }

    private int GetNextFreeSpawnPointIndex()
    {
        for (int i = 0; i < usedSpawnFlags.Length; i++)
        {
            if (!usedSpawnFlags[i])
                return i;
        }

        return -1;
    }
}
