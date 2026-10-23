
using System.Collections;
using System.Linq;
using UnityEngine;

public class StunPuzzleManager : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private StunPiece[] stuns;

    [Header("Drag Settings")]
    [SerializeField] private Camera dragCamera;
    [SerializeField] private float dragStartPixels = 8f;
    [SerializeField] private float dropDistancePixels = 120f;
    [SerializeField] private float dragTowardCamera = 0.15f;
    [SerializeField] private float dragScaleMultiplier = 1.05f;
    [SerializeField] private float snapDuration = 0.15f;

    public bool AllStunsAreInCorrectPosition { get; private set; }

    public int StunCount =>
        stuns == null ? 0 : stuns.Length;

    private StunPiece draggedStun;

    private Vector3 dragStartPosition;
    private Vector3 dragStartScale;
    private Vector2 mouseStartPosition;
    private Vector3 planeStartPoint;

    private Plane dragPlane;

    private bool movedEnough;
    private bool isAnimating;
    private bool locked;

    private void OnEnable()
    {
        PuzzleValidator.PuzzleSolved += LockStuns;
    }

    private void OnDisable()
    {
        PuzzleValidator.PuzzleSolved -= LockStuns;
    }

    private void Awake()
    {
        if (dragCamera == null)
            dragCamera = Camera.main;
    }

    private void LockStuns()
    {
        locked = true;
    }

    private void Update()
    {
        if (locked || PuzzleValidator.IsPuzzleFinished)
            return;

        if (draggedStun != null || isAnimating)
        {
            AllStunsAreInCorrectPosition = false;
            return;
        }

        CheckOrder();
    }

    public void BeginDrag(StunPiece stun, Vector2 mousePosition)
    {
        if (locked || PuzzleValidator.IsPuzzleFinished ||
            stun == null || dragCamera == null ||
            draggedStun != null || isAnimating ||
            stuns == null || !stuns.Contains(stun))
            return;

        draggedStun = stun;

        dragStartPosition = stun.transform.position;
        dragStartScale = stun.transform.localScale;

        mouseStartPosition = mousePosition;
        movedEnough = false;

        dragPlane = new Plane(
            dragCamera.transform.forward,
            dragStartPosition
        );

        Ray ray =
            dragCamera.ScreenPointToRay(mousePosition);

        if (dragPlane.Raycast(ray, out float distance))
            planeStartPoint = ray.GetPoint(distance);
        else
            planeStartPoint = dragStartPosition;
    }

    public void Drag(StunPiece stun, Vector2 mousePosition)
    {
        if (locked || PuzzleValidator.IsPuzzleFinished ||
            stun == null || stun != draggedStun ||
            dragCamera == null)
            return;

        if (!movedEnough)
        {
            float mouseDistance = Vector2.Distance(
                mouseStartPosition,
                mousePosition
            );

            if (mouseDistance < Mathf.Max(0f, dragStartPixels))
                return;

            movedEnough = true;
        }

        Ray ray =
            dragCamera.ScreenPointToRay(mousePosition);

        if (!dragPlane.Raycast(ray, out float distance))
            return;

        Vector3 worldPoint = ray.GetPoint(distance);

        Vector3 mouseDelta =
            worldPoint - planeStartPoint;

        stun.transform.position =
            dragStartPosition +
            mouseDelta -
            dragCamera.transform.forward * dragTowardCamera;

        stun.transform.localScale =
            dragStartScale * dragScaleMultiplier;
    }

    public void EndDrag(StunPiece stun, Vector2 mousePosition)
    {
        if (locked || PuzzleValidator.IsPuzzleFinished ||
            stun == null || stun != draggedStun)
            return;

        Vector3 originalPosition = dragStartPosition;

        stun.transform.localScale = dragStartScale;

        StunPiece destination = movedEnough
            ? FindDropTarget(stun, mousePosition)
            : null;

        draggedStun = null;

        if (destination == null)
        {
            StartCoroutine(
                AnimatePositions(
                    stun,
                    originalPosition,
                    null,
                    Vector3.zero
                )
            );

            return;
        }

        Vector3 otherPosition =
            destination.transform.position;

        Vector3 newStunPosition = new Vector3(
            otherPosition.x,
            originalPosition.y,
            otherPosition.z
        );

        Vector3 newOtherPosition = new Vector3(
            originalPosition.x,
            otherPosition.y,
            originalPosition.z
        );

        StartCoroutine(
            AnimatePositions(
                stun,
                newStunPosition,
                destination,
                newOtherPosition
            )
        );
    }

    private StunPiece FindDropTarget(
        StunPiece held,
        Vector2 mousePosition)
    {
        Ray ray =
            dragCamera.ScreenPointToRay(mousePosition);

        RaycastHit[] hits =
            Physics.RaycastAll(ray, 1000f);

        float closestHit = float.MaxValue;
        StunPiece hitStun = null;

        foreach (RaycastHit hit in hits)
        {
            StunPiece candidate =
                hit.collider.GetComponentInParent<StunPiece>();

            if (candidate == null ||
                candidate == held ||
                !stuns.Contains(candidate))
                continue;

            if (hit.distance < closestHit)
            {
                closestHit = hit.distance;
                hitStun = candidate;
            }
        }

        if (hitStun != null)
            return hitStun;

        float bestDistance =
            Mathf.Max(0f, dropDistancePixels);

        StunPiece nearest = null;

        foreach (StunPiece candidate in stuns)
        {
            if (candidate == null || candidate == held)
                continue;

            Collider col =
                candidate.GetComponent<Collider>();

            Vector3 targetCenter = col != null
                ? col.bounds.center
                : candidate.transform.position;

            Vector3 screenPoint =
                dragCamera.WorldToScreenPoint(targetCenter);

            if (screenPoint.z <= 0f)
                continue;

            float distance = Vector2.Distance(
                mousePosition,
                new Vector2(
                    screenPoint.x,
                    screenPoint.y
                )
            );

            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearest = candidate;
            }
        }

        return nearest;
    }

    private IEnumerator AnimatePositions(
        StunPiece first,
        Vector3 firstDestination,
        StunPiece second,
        Vector3 secondDestination)
    {
        isAnimating = true;

        Vector3 firstStart =
            first.transform.position;

        Vector3 secondStart = second != null
            ? second.transform.position
            : Vector3.zero;

        float elapsed = 0f;
        float duration = Mathf.Max(0f, snapDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);

            first.transform.position =
                Vector3.Lerp(
                    firstStart,
                    firstDestination,
                    t
                );

            if (second != null)
            {
                second.transform.position =
                    Vector3.Lerp(
                        secondStart,
                        secondDestination,
                        t
                    );
            }

            yield return null;
        }

        first.transform.position = firstDestination;

        if (second != null)
            second.transform.position = secondDestination;

        isAnimating = false;

        CheckOrder();
    }

    private void CheckOrder()
    {
        if (stuns == null ||
            stuns.Length == 0 ||
            stuns.Any(s => s == null))
        {
            AllStunsAreInCorrectPosition = false;
            return;
        }

        // Sort from smallest X to largest X
        var ordered = stuns
            .OrderBy(s => s.transform.position.x)
            .ToArray();

        AllStunsAreInCorrectPosition = true;

        for (int i = 0; i < ordered.Length; i++)
        {
            // Correct Order: 0, 1, 2, 3, 4
            if (ordered[i].CorrectOrder != i)
            {
                AllStunsAreInCorrectPosition = false;
                break;
            }
        }
    }
}
