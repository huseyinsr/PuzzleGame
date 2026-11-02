
using UnityEngine;
using System.Collections;

public class MusicMixer : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource1;
    [SerializeField] private AudioSource musicSource2;
    [SerializeField] private AudioSource musicSource3;

    [Header("Maximum Volumes")]
    [Range(0f, 1f)]
    [SerializeField] private float maxVolume1 = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float maxVolume2 = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float maxVolume3 = 1f;

    [Header("Transition Settings")]
    [SerializeField] private float transitionEverySeconds = 15f;
    [SerializeField] private float transitionDuration = 5f;

    private AudioSource[] musicSources;
    private float[] maxVolumes;

    private int currentMusicIndex = 0;

    void Start()
    {
        musicSources = new AudioSource[]
        {
            musicSource1,
            musicSource2,
            musicSource3
        };

        maxVolumes = new float[]
        {
            maxVolume1,
            maxVolume2,
            maxVolume3
        };

        // Initialize all music sources
        for (int i = 0; i < musicSources.Length; i++)
        {
            musicSources[i].loop = true;
            musicSources[i].volume = 0f;
        }

        // First music starts at its maximum volume
        musicSources[0].volume = maxVolumes[0];

        // Start all tracks simultaneously
        foreach (AudioSource source in musicSources)
        {
            source.Play();
        }

        StartCoroutine(TransitionMusic());
    }

    IEnumerator TransitionMusic()
    {
        while (true)
        {
            yield return new WaitForSeconds(transitionEverySeconds);

            int nextMusicIndex =
                (currentMusicIndex + 1) % musicSources.Length;

            AudioSource currentSource =
                musicSources[currentMusicIndex];

            AudioSource nextSource =
                musicSources[nextMusicIndex];

            float currentMaxVolume =
                maxVolumes[currentMusicIndex];

            float nextMaxVolume =
                maxVolumes[nextMusicIndex];

            float elapsedTime = 0f;
            float duration = Mathf.Max(0.01f, transitionDuration);

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                float t = Mathf.Clamp01(elapsedTime / duration);

                // Fade out current music
                currentSource.volume =
                    Mathf.Lerp(currentMaxVolume, 0f, t);

                // Fade in next music
                nextSource.volume =
                    Mathf.Lerp(0f, nextMaxVolume, t);

                yield return null;
            }

            // Ensure final volumes are exact
            currentSource.volume = 0f;
            nextSource.volume = nextMaxVolume;

            // Move to next music
            currentMusicIndex = nextMusicIndex;
        }
    }
}
