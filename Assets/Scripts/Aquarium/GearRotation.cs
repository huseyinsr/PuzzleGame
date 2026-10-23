
using System.Collections.Generic;
using UnityEngine;

public class GearRotation : MonoBehaviour
{
    [System.Serializable]
    public class Gear
    {
        public Transform gear;

        [Header("Rotation")]
        public Vector3 rotationAxis = Vector3.forward;
        public bool reverseDirection = false;
        public float speedMultiplier = 1f;

        [Header("Pivot Correction")]
        public bool autoDetectCenter = true;
        public Vector3 centerOffset = Vector3.zero;

        [Header("Individual Animation")]
        public float startDelay = 0f;
        public float cycleOffset = 0f;
        public bool enableWobble = true;
        public float wobbleMultiplier = 1f;

        [HideInInspector] public Vector3 calculatedCenter;
        [HideInInspector] public float previousWobble;
    }

    [Header("Gears")]
    [SerializeField] private List<Gear> gears = new List<Gear>();

    [Header("Global Rotation")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float accelerationTime = 1f;

    [Header("Fast / Slow Speed Cycle")]
    [SerializeField] private bool enableSpeedCycle = true;
    [SerializeField] private float fastSpeed = 1.8f;
    [SerializeField] private float slowSpeed = 0.3f;

    [SerializeField] private float fastDuration = 1f;
    [SerializeField] private float slowDuration = 1f;
    [SerializeField] private float transitionDuration = 0.2f;

    [Header("Wobble Animation")]
    [SerializeField] private bool enableWobble = true;
    [SerializeField] private float wobbleAngle = 0.7f;
    [SerializeField] private float wobbleFrequency = 4f;

    [Header("Mechanical Pulse")]
    [Range(0f, 0.25f)]
    [SerializeField] private float mechanicalPulse = 0.02f;
    [SerializeField] private float pulseFrequency = 5f;

    private float startTime;

    private void Awake()
    {
        foreach (Gear item in gears)
        {
            if (item.gear == null)
                continue;

            item.calculatedCenter = Vector3.zero;

            if (!item.autoDetectCenter)
                continue;

            Renderer[] renderers =
                item.gear.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
                continue;

            Bounds bounds = renderers[0].bounds;

            foreach (Renderer renderer in renderers)
                bounds.Encapsulate(renderer.bounds);

            item.calculatedCenter =
                item.gear.InverseTransformPoint(bounds.center);
        }
    }

    private void OnEnable()
    {
        startTime = Time.time;

        foreach (Gear item in gears)
            item.previousWobble = 0f;
    }

    private void Update()
    {
        float elapsed = Time.time - startTime;

        foreach (Gear item in gears)
        {
            if (item.gear == null)
                continue;

            float gearTime = elapsed - item.startDelay;

            if (gearTime < 0f)
                continue;

            if (item.rotationAxis.sqrMagnitude < 0.0001f)
                continue;

            // Smooth start
            float acceleration = accelerationTime <= 0f
                ? 1f
                : Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(gearTime / accelerationTime)
                );

            // Alternating fast and slow rotation
            float speedFactor = enableSpeedCycle
                ? GetSpeedCycle(gearTime + item.cycleOffset)
                : 1f;

            // Subtle mechanical speed variation
            float pulse = 1f + Mathf.Sin(
                gearTime * pulseFrequency * Mathf.PI * 2f
            ) * mechanicalPulse;

            float direction =
                item.reverseDirection ? -1f : 1f;

            float rotationAmount =
                rotationSpeed *
                item.speedMultiplier *
                direction *
                acceleration *
                speedFactor *
                pulse *
                Time.deltaTime;

            // Wobble: small back-and-forth rotation
            float currentWobble = 0f;

            if (enableWobble && item.enableWobble)
            {
                currentWobble =
                    Mathf.Sin(
                        gearTime * wobbleFrequency * Mathf.PI * 2f
                    ) *
                    wobbleAngle *
                    item.wobbleMultiplier *
                    acceleration;
            }

            // Delta prevents wobble from accumulating
            float wobbleDelta =
                currentWobble - item.previousWobble;

            item.previousWobble = currentWobble;

            // Corrected pivot center
            Vector3 localCenter =
                item.calculatedCenter + item.centerOffset;

            Vector3 worldCenter =
                item.gear.TransformPoint(localCenter);

            Vector3 worldAxis =
                item.gear.TransformDirection(
                    item.rotationAxis.normalized
                );

            item.gear.RotateAround(
                worldCenter,
                worldAxis,
                rotationAmount + wobbleDelta
            );
        }
    }

    private float GetSpeedCycle(float time)
    {
        float fastTime = Mathf.Max(0.01f, fastDuration);
        float slowTime = Mathf.Max(0.01f, slowDuration);
        float transition = Mathf.Max(0f, transitionDuration);

        float cycleLength =
            fastTime + slowTime + transition * 2f;

        float phase = Mathf.Repeat(time, cycleLength);

        // Fast rotation
        if (phase < fastTime)
            return fastSpeed;

        phase -= fastTime;

        // Smooth transition from fast to slow
        if (transition > 0f && phase < transition)
        {
            float t = Mathf.SmoothStep(
                0f, 1f, phase / transition
            );

            return Mathf.Lerp(fastSpeed, slowSpeed, t);
        }

        phase -= transition;

        // Slow rotation
        if (phase < slowTime)
            return slowSpeed;

        phase -= slowTime;

        // Smooth transition back to fast
        if (transition > 0f)
        {
            float t = Mathf.SmoothStep(
                0f, 1f, phase / transition
            );

            return Mathf.Lerp(slowSpeed, fastSpeed, t);
        }

        return fastSpeed;
    }
}
