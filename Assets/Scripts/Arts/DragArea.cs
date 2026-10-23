using UnityEngine;

public class DragArea : MonoBehaviour
{
    [SerializeField] private Transform minPoint;
    [SerializeField] private Transform maxPoint;
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject objectToHide;

    private bool hasSpawned = false;

    private void OnEnable()
    {
        MovablePoint.onEveryPointInCorrectPosition.AddListener(SpawnTheObject);
    }

    private void OnDisable()
    {
        MovablePoint.onEveryPointInCorrectPosition.RemoveListener(SpawnTheObject);
    }

    internal Vector3 Clamp(Vector3 position)
    {
        Vector3 min = Vector3.Min(minPoint.position, maxPoint.position);
        Vector3 max = Vector3.Max(minPoint.position, maxPoint.position);

        position.x = Mathf.Clamp(position.x, min.x, max.x);
        position.y = Mathf.Clamp(position.y, min.y, max.y);
        position.z = Mathf.Clamp(position.z, min.z, max.z);

        return position;
    }

    private void SpawnTheObject()
    {
        if (hasSpawned) return;

        if (prefabToSpawn == null || spawnPoint == null)
        {
            Debug.LogWarning("Missing prefab or spawn point!");
            return;
        }
        if (objectToHide != null)
            objectToHide.SetActive(false);

        Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        hasSpawned = true;

        Debug.Log("Spawned!");
    }
}