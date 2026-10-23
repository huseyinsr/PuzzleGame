
using UnityEngine;

[RequireComponent(typeof(StunPiece))]
public class StunClick : MonoBehaviour
{
    [SerializeField] private StunPuzzleManager puzzleManager;

    private StunPiece stunPiece;

    private void Awake()
    {
        stunPiece = GetComponent<StunPiece>();

        if (puzzleManager == null)
        {
            puzzleManager =
                FindFirstObjectByType<StunPuzzleManager>();
        }
    }

    private void OnMouseDown()
    {
        if (puzzleManager != null)
        {
            puzzleManager.BeginDrag(
                stunPiece,
                Input.mousePosition
            );
        }
    }

    private void OnMouseDrag()
    {
        if (puzzleManager != null)
        {
            puzzleManager.Drag(
                stunPiece,
                Input.mousePosition
            );
        }
    }

    private void OnMouseUp()
    {
        if (puzzleManager != null)
        {
            puzzleManager.EndDrag(
                stunPiece,
                Input.mousePosition
            );
        }
    }
}
