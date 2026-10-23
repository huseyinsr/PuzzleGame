
using System;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleValidator : MonoBehaviour
{
    [SerializeField] private StunPuzzleManager stunPuzzleManager;

    public static event Action PuzzleSolved;

    public static bool IsPuzzleFinished { get; private set; }

    public bool IsPuzzleSolved { get; private set; }

    private bool previousStunState;
    private bool previousItemState;

    private void Awake()
    {
        IsPuzzleFinished = false;
    }

    private void Update()
    {
        if (IsPuzzleSolved || stunPuzzleManager == null)
            return;

        bool stunOrderCorrect =
            stunPuzzleManager.AllStunsAreInCorrectPosition;

        bool itemOrderCorrect = CheckItems();

        if (stunOrderCorrect && !previousStunState)
            Debug.Log("All stuns are in the correct positions");

        if (itemOrderCorrect && !previousItemState)
            Debug.Log("All items are on the correct stuns");

        previousStunState = stunOrderCorrect;
        previousItemState = itemOrderCorrect;

        if (!stunOrderCorrect || !itemOrderCorrect)
            return;

        IsPuzzleSolved = true;
        IsPuzzleFinished = true;

        Debug.Log("Puzzle solved!");

        PuzzleSolved?.Invoke();
    }

    private bool CheckItems()
    {
        int expectedCount = stunPuzzleManager.StunCount;

        if (expectedCount <= 0)
            return false;

        ItemForStun[] items =
            FindObjectsByType<ItemForStun>(
                FindObjectsSortMode.None
            );

        if (items.Length != expectedCount)
            return false;

        HashSet<StunPiece> matchedStuns =
            new HashSet<StunPiece>();

        foreach (ItemForStun item in items)
        {
            StunPiece stun =
                item.GetComponentInParent<StunPiece>();

            if (stun == null || stun.ItemSocket == null)
                return false;

            if (!item.transform.IsChildOf(stun.ItemSocket))
                return false;

            if (item.ItemID != stun.CorrectOrder)
                return false;

            if (!matchedStuns.Add(stun))
                return false;
        }

        return matchedStuns.Count == expectedCount;
    }
}
