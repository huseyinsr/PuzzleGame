using UnityEngine;
using System.Collections;

public class DrawrSliding : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float openDistance = 1f;
    [SerializeField] private Vector3 openDirection = Vector3.forward;

    [SerializeField] private Transform linkedItem;
    [SerializeField] private float minZoomWaitTime = 1f;

    private Vector3 closedPosition;
    private Vector3 openedPosition;

    private bool isOpen;
    private bool isMoving;

    private void Start()
    {
        if (cam == null)
            cam = Camera.main;

        closedPosition = transform.localPosition;
        openedPosition = closedPosition + openDirection.normalized * openDistance;
    }

    private void Update()
    {
        if (UIBlocker.IsPointerOverUI)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (ZoomManager.Instance != null)
        {
            if (!ZoomManager.Instance.IsZoomed)
                return;

            if (Time.time - ZoomManager.Instance.ZoomStartTime < minZoomWaitTime)
                return;
        }

        if (isMoving)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;
        if (hit.transform != transform && !hit.transform.IsChildOf(transform))
            return;

        StartCoroutine(Slide(
            isOpen ? closedPosition : openedPosition
        ));

        isOpen = !isOpen;
    }

    private IEnumerator Slide(Vector3 targetPosition)
    {
        isMoving = true;

        Vector3 startPosition = transform.localPosition;
        float t = 0f;

        while (t < speed)
        {
            Vector3 previousWorldPosition = transform.position;

            t += Time.deltaTime;

            transform.localPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                t / speed
            );

            if (linkedItem != null)
            {
                Vector3 delta = transform.position - previousWorldPosition;
                linkedItem.position += delta;
            }

            yield return null;
        }

        transform.localPosition = targetPosition;
        isMoving = false;
    }
}