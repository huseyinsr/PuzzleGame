using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Rocket : MonoBehaviour
{
    public event Action<Collider> OnColliderEntered;

    [Header("Forward Movement")]
    [SerializeField] private float reactionDelay = 0.08f;
    [SerializeField] private float maxSpeed = 0.7f;
    [SerializeField] private float cruiseSpeed = 0.35f;
    [SerializeField] private float accelerationTime = 0.45f;
    [SerializeField] private float cruiseTime = 0.4f;
    [SerializeField] private float slowDownTime = 0.8f;

    [Header("Forward Smoothness")]
    [SerializeField]
    private AnimationCurve accelerationCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField]
    private AnimationCurve slowDownCurve =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Turning")]
    [SerializeField] private float leftTurnAngle = 30f;
    [SerializeField] private float rightTurnAngle = -30f;
    [SerializeField] private float turnReactionDelay = 0.05f;
    [SerializeField] private float turnDuration = 0.55f;

    [Header("Turning Smoothness")]
    [SerializeField]
    private AnimationCurve turnCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Rigidbody rocketBody;

    private bool controlEnabled;
    private bool moving;
    private bool turning;

    private void Awake()
    {
        rocketBody = GetComponent<Rigidbody>();

        rocketBody.useGravity = false;
        rocketBody.isKinematic = true;
        rocketBody.interpolation = RigidbodyInterpolation.Interpolate;
        rocketBody.collisionDetectionMode =
            CollisionDetectionMode.ContinuousSpeculative;
    }

    public void SetControlEnabled(bool state)
    {
        controlEnabled = state;
    }

    public bool TryMoveForward()
    {
        if (!controlEnabled || moving || turning)
        {
            return false;
        }

        StartCoroutine(MoveForward());
        return true;
    }

    public bool TryTurnLeft()
    {
        if (!controlEnabled || moving || turning)
        {
            return false;
        }

        StartCoroutine(Turn(leftTurnAngle));
        return true;
    }

    public bool TryTurnRight()
    {
        if (!controlEnabled || moving || turning)
        {
            return false;
        }

        StartCoroutine(Turn(rightTurnAngle));
        return true;
    }

    private IEnumerator MoveForward()
    {
        moving = true;

        yield return new WaitForSeconds(reactionDelay);

        if (!controlEnabled)
        {
            moving = false;
            yield break;
        }

        Vector3 direction = transform.up.normalized;

        float time = 0f;

        while (time < accelerationTime)
        {
            time += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(time / accelerationTime);
            float curve = accelerationCurve.Evaluate(t);

            float speed = Mathf.Lerp(0f, maxSpeed, curve);

            rocketBody.MovePosition(
                rocketBody.position +
                direction * speed * Time.fixedDeltaTime
            );

            yield return new WaitForFixedUpdate();
        }

        time = 0f;

        while (time < cruiseTime)
        {
            time += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(time / cruiseTime);

            float speed = Mathf.Lerp(
                maxSpeed,
                cruiseSpeed,
                t
            );

            rocketBody.MovePosition(
                rocketBody.position +
                direction * speed * Time.fixedDeltaTime
            );

            yield return new WaitForFixedUpdate();
        }

        time = 0f;

        while (time < slowDownTime)
        {
            time += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(time / slowDownTime);
            float curve = slowDownCurve.Evaluate(t);

            float speed = cruiseSpeed * curve;

            rocketBody.MovePosition(
                rocketBody.position +
                direction * speed * Time.fixedDeltaTime
            );

            yield return new WaitForFixedUpdate();
        }

        moving = false;
    }

    private IEnumerator Turn(float angle)
    {
        turning = true;

        yield return new WaitForSeconds(turnReactionDelay);

        if (!controlEnabled)
        {
            turning = false;
            yield break;
        }

        Quaternion startRotation = rocketBody.rotation;

        Quaternion targetRotation =
            startRotation *
            Quaternion.Euler(0f, 0f, angle);

        float time = 0f;

        while (time < turnDuration)
        {
            time += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(time / turnDuration);
            float curve = turnCurve.Evaluate(t);

            Quaternion rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                curve
            );

            rocketBody.MoveRotation(rotation);

            yield return new WaitForFixedUpdate();
        }

        rocketBody.MoveRotation(targetRotation);

        turning = false;
    }

    public void ResetRocket(
        Vector3 position,
        Quaternion rotation)
    {
        StopAllCoroutines();

        moving = false;
        turning = false;

        rocketBody.position = position;
        rocketBody.rotation = rotation;

        transform.position = position;
        transform.rotation = rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        OnColliderEntered?.Invoke(other);
    }
}