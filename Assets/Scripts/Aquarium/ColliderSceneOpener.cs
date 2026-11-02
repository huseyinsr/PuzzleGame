
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ColliderSceneOpener : MonoBehaviour
{
    [Header("Clickable Objects")]
    [SerializeField] private GameObject[] clickableObjects;

    [Header("Scene")]
    [SerializeField] private string sceneName;

    [Header("Raycast")]
    [SerializeField] private Camera clickCamera;
    [SerializeField] private float maxDistance = 1000f;

    private bool isLoading;

    private void Update()
    {
        if (isLoading || !Input.GetMouseButtonDown(0)) return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        Camera cam = clickCamera != null ? clickCamera : Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            return;

        foreach (GameObject target in clickableObjects)
        {
            if (target == null || !target.activeInHierarchy)
                continue;

            if (!hit.collider.transform.IsChildOf(target.transform))
                continue;

            if (string.IsNullOrWhiteSpace(sceneName) ||
                !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning(
                    "Scene is missing or not included in Build Settings: "
                    + sceneName
                );
                return;
            }

            isLoading = true;
            SceneManager.LoadScene(sceneName);
            return;
        }
    }
}
