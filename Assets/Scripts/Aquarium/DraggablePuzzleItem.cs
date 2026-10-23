
using UnityEngine;

public class DraggablePuzzleItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Snap Settings")]
    [SerializeField] private float snapDistancePixels = 120f;

    [Header("X Movement")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;

    [Header("Y Movement")]
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;

    private float fixedZ;

    private bool dragging;
    private bool locked;

    private ItemSocket currentSocket;

    private void OnEnable()
    {
        PuzzleValidator.PuzzleSolved += LockItem;
    }

    private void OnDisable()
    {
        PuzzleValidator.PuzzleSolved -= LockItem;
        dragging = false;
    }

    private void Start()
    {
        fixedZ = transform.position.z;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void LockItem()
    {
        locked = true;
        dragging = false;
    }

    private void OnMouseDown()
    {
        if (locked ||
            PuzzleValidator.IsPuzzleFinished ||
            mainCamera == null)
            return;

        dragging = true;

        fixedZ = transform.position.z;

        if (currentSocket != null)
        {
            currentSocket.SetOccupied(false);
            currentSocket = null;
        }

        transform.SetParent(null, true);
    }

    private void Update()
    {
        if (!dragging ||
            locked ||
            PuzzleValidator.IsPuzzleFinished ||
            mainCamera == null)
            return;

        Plane plane = new Plane(
            Vector3.forward,
            new Vector3(0f, 0f, fixedZ)
        );

        Ray ray = mainCamera.ScreenPointToRay(
            Input.mousePosition
        );

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 point = ray.GetPoint(enter);

            point.x = Mathf.Clamp(
                point.x,
                minX,
                maxX
            );

            point.y = Mathf.Clamp(
                point.y,
                minY,
                maxY
            );

            point.z = fixedZ;

            transform.position = point;
        }
    }

    private void OnMouseUp()
    {
        if (!dragging ||
            locked ||
            PuzzleValidator.IsPuzzleFinished)
            return;

        dragging = false;

        FindNearestSocket();
    }

    private void FindNearestSocket()
    {
        if (mainCamera == null)
            return;

        StunPiece[] stuns =
            FindObjectsByType<StunPiece>(
                FindObjectsSortMode.None
            );

        ItemSocket nearestSocket = null;

        float nearestDistance = snapDistancePixels;

        Vector2 mouseScreenPosition =
            Input.mousePosition;

        foreach (StunPiece stun in stuns)
        {
            if (stun.ItemSocket == null)
                continue;

            ItemSocket socket =
                stun.ItemSocket.GetComponent<ItemSocket>();

            if (socket == null || socket.IsOccupied)
                continue;

            Vector3 screenPosition =
                mainCamera.WorldToScreenPoint(
                    stun.ItemSocket.position
                );

            if (screenPosition.z <= 0f)
                continue;

            float distance = Vector2.Distance(
                mouseScreenPosition,
                new Vector2(
                    screenPosition.x,
                    screenPosition.y
                )
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestSocket = socket;
            }
        }

        if (nearestSocket != null)
            SnapToSocket(nearestSocket);
    }

    private void SnapToSocket(ItemSocket socket)
    {
        currentSocket = socket;

        currentSocket.SetOccupied(true);

        transform.SetParent(socket.transform, true);

        transform.position = socket.transform.position;

        Debug.Log(
            gameObject.name +
            " snapped to " +
            socket.gameObject.name
        );
    }
}
