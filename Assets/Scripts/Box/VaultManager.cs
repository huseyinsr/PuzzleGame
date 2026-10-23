using UnityEngine;
using System.Collections;

public class VaultManager : MonoBehaviour
{
    [SerializeField] private VaultBox[] boxes;
    [SerializeField] private int[] correctNumbers;

    [Header("Door")]
    [SerializeField] private Transform door;
    [SerializeField] private float openAngle = 100f;
    [SerializeField] private float openDuration = 1.5f;

    private bool doorIsOpen;

    private void Start()
    {
        foreach (var box in boxes)
            box.SetManager(this);
    }

    public void CheckCombination()
    {
        if (boxes.Length != correctNumbers.Length)
            return;

        for (int i = 0; i < boxes.Length; i++)
        {
            if (boxes[i].CurrentNumber != correctNumbers[i])
                return;
        }

        if (!doorIsOpen && door != null)
        {
            doorIsOpen = true;
            StartCoroutine(OpenDoor());
        }
    }

    private IEnumerator OpenDoor()
    {
        Quaternion startRotation = door.localRotation;

        Quaternion targetRotation =
            startRotation * Quaternion.Euler(0, openAngle, 0);

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, openDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            // Smooth opening
            t = t * t * (3f - 2f * t);

            door.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        door.localRotation = targetRotation;
    }
}