using System.Collections.Generic;
using UnityEngine;

public class SpriteParticleEffect : MonoBehaviour
{
    [SerializeField] private Sprite sprite;

    [Header("Spawn")]
    [SerializeField] private float spawnEvery = 0.2f;
    [SerializeField] private int amount = 1;
    [SerializeField] private Vector3 spawnArea = new Vector3(0.2f, 0.05f, 0.2f);
    [SerializeField] private Vector3 spawnOffset;

    [Header("Movement")]
    [SerializeField] private float speed = 0.5f;

    [SerializeField]
    private Vector2 moveXRange =
        new Vector2(-10f, 10f);

    [SerializeField]
    private Vector2 moveYRange =
        new Vector2(-10f, 10f);

    [SerializeField]
    private Vector2 moveZRange =
        new Vector2(-10f, 10f);

    [Header("Sprite Rotation X")]
    [SerializeField]
    private Vector2 rotationXRange =
        new Vector2(-20f, 20f);

    [Header("Sprite Rotation Y")]
    [SerializeField]
    private Vector2 rotationYRange =
        new Vector2(-20f, 20f);

    [Header("Sprite Rotation Z")]
    [SerializeField]
    private Vector2 rotationZRange =
        new Vector2(0f, 360f);

    [Header("Look")]
    [SerializeField] private float size = 0.1f;
    [SerializeField] private Color color = Color.white;
    [SerializeField] private int sortingOrder = 10;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private bool fadeOut = true;

    private class Particle
    {
        public GameObject gameObject;
        public SpriteRenderer renderer;
        public Vector3 direction;
        public float life;
        public float startLife;
    }

    private readonly List<Particle> particles = new();

    private float spawnTimer;

    private void Update()
    {
        SpawnParticles();
        MoveParticles();
    }

    private void SpawnParticles()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnEvery)
        {
            return;
        }

        spawnTimer = 0f;

        for (int i = 0; i < amount; i++)
        {
            CreateParticle();
        }
    }

    private void CreateParticle()
    {
        if (sprite == null)
        {
            return;
        }

        GameObject newParticle =
            new GameObject("Sprite Particle");

        newParticle.transform.SetParent(transform, false);

        Vector3 randomSpawnPosition = new Vector3(
            Random.Range(
                -spawnArea.x / 2f,
                spawnArea.x / 2f
            ),
            Random.Range(
                -spawnArea.y / 2f,
                spawnArea.y / 2f
            ),
            Random.Range(
                -spawnArea.z / 2f,
                spawnArea.z / 2f
            )
        );

        newParticle.transform.localPosition =
            spawnOffset + randomSpawnPosition;

        float rotationX = Random.Range(
            rotationXRange.x,
            rotationXRange.y
        );

        float rotationY = Random.Range(
            rotationYRange.x,
            rotationYRange.y
        );

        float rotationZ = Random.Range(
            rotationZRange.x,
            rotationZRange.y
        );

        newParticle.transform.localRotation =
            Quaternion.Euler(
                rotationX,
                rotationY,
                rotationZ
            );

        newParticle.transform.localScale =
            Vector3.one * size;

        SpriteRenderer spriteRenderer =
            newParticle.AddComponent<SpriteRenderer>();

        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;

        float moveX = Random.Range(
            moveXRange.x,
            moveXRange.y
        );

        float moveY = Random.Range(
            moveYRange.x,
            moveYRange.y
        );

        float moveZ = Random.Range(
            moveZRange.x,
            moveZRange.y
        );

        Vector3 direction =
            Quaternion.Euler(
                moveX,
                moveY,
                moveZ
            ) * Vector3.up;

        Particle particle = new Particle
        {
            gameObject = newParticle,
            renderer = spriteRenderer,
            direction = direction.normalized,
            life = lifetime,
            startLife = lifetime
        };

        particles.Add(particle);
    }

    private void MoveParticles()
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            Particle particle = particles[i];

            if (particle.gameObject == null)
            {
                particles.RemoveAt(i);
                continue;
            }

            particle.life -= Time.deltaTime;

            particle.gameObject.transform.localPosition +=
                particle.direction *
                speed *
                Time.deltaTime;

            if (fadeOut)
            {
                Color particleColor =
                    particle.renderer.color;

                particleColor.a =
                    Mathf.Clamp01(
                        particle.life /
                        particle.startLife
                    );

                particle.renderer.color =
                    particleColor;
            }

            if (particle.life <= 0f)
            {
                Destroy(particle.gameObject);
                particles.RemoveAt(i);
            }
        }
    }
}