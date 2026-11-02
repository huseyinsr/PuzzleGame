
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StunPuzzleSuccessActions : MonoBehaviour
{
    [Header("Disable Immediately")]
    [SerializeField] private GameObject[] objectsToDisable;

    [Header("Activate With Fade")]
    [SerializeField] private GameObject[] objectsToActivate;

    [Min(0f)]
    [SerializeField] private float fadeDuration = 1.5f;

    private readonly List<Action<float>> fadeActions =
        new List<Action<float>>();

    private bool triggered;

    private void OnEnable()
    {
        PuzzleValidator.PuzzleSolved += OnPuzzleSolved;
    }

    private void OnDisable()
    {
        PuzzleValidator.PuzzleSolved -= OnPuzzleSolved;
    }

    private void Start()
    {
        if (PuzzleValidator.IsPuzzleFinished)
            OnPuzzleSolved();
    }

    private void OnPuzzleSolved()
    {
        if (triggered) return;

        triggered = true;

        if (ZoomManager.Instance != null &&
            ZoomManager.Instance.IsZoomed)
        {
            ZoomManager.Instance.ExitZoom();
        }

        foreach (GameObject target in objectsToDisable)
        {
            if (target != null)
                target.SetActive(false);
        }

        StartCoroutine(ActivateWithFade());
    }

    private IEnumerator ActivateWithFade()
    {
        fadeActions.Clear();

        foreach (GameObject target in objectsToActivate)
        {
            if (target == null) continue;

            PrepareFade(target);
        }

        SetFade(0f);

        foreach (GameObject target in objectsToActivate)
        {
            if (target != null)
                target.SetActive(true);
        }

        float duration = Mathf.Max(0f, fadeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            SetFade(t * t * (3f - 2f * t));

            yield return null;
        }

        SetFade(1f);
    }

    private void PrepareFade(GameObject target)
    {
        foreach (SpriteRenderer sprite in
                 target.GetComponentsInChildren<SpriteRenderer>(true))
        {
            SpriteRenderer current = sprite;
            Color original = current.color;

            fadeActions.Add(alpha =>
            {
                if (current == null) return;

                Color color = original;
                color.a *= alpha;
                current.color = color;
            });
        }

        foreach (Graphic graphic in
                 target.GetComponentsInChildren<Graphic>(true))
        {
            Graphic current = graphic;
            Color original = current.color;

            fadeActions.Add(alpha =>
            {
                if (current == null) return;

                Color color = original;
                color.a *= alpha;
                current.color = color;
            });
        }

        foreach (Renderer renderer in
                 target.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer is SpriteRenderer) continue;

            foreach (Material material in renderer.materials)
            {
                if (material == null) continue;

                string property = material.HasProperty("_BaseColor")
                    ? "_BaseColor"
                    : material.HasProperty("_Color")
                        ? "_Color"
                        : null;

                if (property == null) continue;

                Material current = material;
                string colorProperty = property;
                Color original = current.GetColor(colorProperty);

                fadeActions.Add(alpha =>
                {
                    if (current == null) return;

                    Color color = original;
                    color.a *= alpha;

                    current.SetColor(colorProperty, color);
                });
            }
        }
    }

    private void SetFade(float alpha)
    {
        foreach (Action<float> setAlpha in fadeActions)
            setAlpha(alpha);
    }
}
