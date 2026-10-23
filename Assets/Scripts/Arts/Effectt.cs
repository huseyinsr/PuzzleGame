
using UnityEngine;
using System.Collections;

public class RandomObjectSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject[] objects;
    [SerializeField] private float switchSpeed = 0.1f;

    [Header("Subtle Shake")]
    [SerializeField] private float shakeAmount = 0.002f;

    private Vector3[] originalPositions;
    private int currentIndex = -1;

    private void Start()
    {
        originalPositions = new Vector3[objects.Length];

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null) continue;

            originalPositions[i] = objects[i].transform.localPosition;
            objects[i].SetActive(false);
        }

        StartCoroutine(SwitchObjects());
    }

    private IEnumerator SwitchObjects()
    {
        while (true)
        {
            if (currentIndex >= 0 && objects[currentIndex] != null)
            {
                objects[currentIndex].transform.localPosition =
                    originalPositions[currentIndex];

                objects[currentIndex].SetActive(false);
            }

            if (objects.Length > 0)
            {
                currentIndex = Random.Range(0, objects.Length);

                GameObject selected = objects[currentIndex];

                if (selected != null)
                {
                    selected.SetActive(true);

                    selected.transform.localPosition =
                        originalPositions[currentIndex] +
                        Random.insideUnitSphere * shakeAmount;
                }
            }

            yield return new WaitForSeconds(Mathf.Max(0.01f, switchSpeed));
        }
    }
}
