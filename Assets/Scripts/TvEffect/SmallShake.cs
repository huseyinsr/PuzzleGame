using UnityEngine;

public class SmallShake : MonoBehaviour
{
    [SerializeField] private Transform[] objects;

    [Header("Position Shake")]
    [SerializeField] private float positionAmount = 0.003f;

    [Header("Rotation Shake")]
    [SerializeField] private float rotationAmount = 0.3f;

    [Header("Speed")]
    [SerializeField] private float shakeSpeed = 8f;

    private Vector3[] startPositions;
    private Quaternion[] startRotations;

    private float time;

    private void Start()
    {
        if (objects == null || objects.Length == 0)
        {
            objects = new Transform[] { transform };
        }

        startPositions = new Vector3[objects.Length];
        startRotations = new Quaternion[objects.Length];

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
            {
                continue;
            }

            startPositions[i] = objects[i].localPosition;
            startRotations[i] = objects[i].localRotation;
        }
    }

    private void Update()
    {
        time += Time.deltaTime * shakeSpeed;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
            {
                continue;
            }

            float offset = i * 10f;

            float x =
                Mathf.PerlinNoise(time + offset, 0f) - 0.5f;

            float y =
                Mathf.PerlinNoise(0f, time + offset) - 0.5f;

            float z =
                Mathf.PerlinNoise(
                    time + offset,
                    time + offset
                ) - 0.5f;

            Vector3 positionShake =
                new Vector3(x, y, z) *
                positionAmount;

            Vector3 rotationShake =
                new Vector3(y, z, x) *
                rotationAmount;

            objects[i].localPosition =
                startPositions[i] +
                positionShake;

            objects[i].localRotation =
                startRotations[i] *
                Quaternion.Euler(rotationShake);
        }
    }

    private void OnDisable()
    {
        if (objects == null)
        {
            return;
        }

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
            {
                continue;
            }

            objects[i].localPosition =
                startPositions[i];

            objects[i].localRotation =
                startRotations[i];
        }
    }
}