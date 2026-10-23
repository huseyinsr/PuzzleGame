using UnityEngine;
using System.Collections;
public class MusicMixer : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource1;
    [SerializeField] private AudioSource musicSource2;
    [SerializeField] private AudioSource musicSource3;

    [SerializeField] private float transitionEverySeconds = 15f;
    [SerializeField] private float transitionDuration = 5f;


    void Start()
    {
        musicSource1.Play();
        musicSource2.Play();
        musicSource3.Play();
        musicSource2.volume = 0f; 
        musicSource3.volume = 0f;

        StartCoroutine(TransitionMusic());
    }   

    IEnumerator TransitionMusic()
    {
        while (true)
        {
            yield return new WaitForSeconds(transitionEverySeconds);
            float elapsedTime = 0f;
            float startVolume1 = musicSource1.volume;
            float startVolume2 = musicSource2.volume;
            float startVolume3 = musicSource3.volume;
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;
                musicSource1.volume = Mathf.Lerp(startVolume1, 0f, t);
                musicSource2.volume = Mathf.Lerp(startVolume2, 1f, t);
                musicSource3.volume = Mathf.Lerp(startVolume3, 0f, t);
                yield return null;
            }
            // Swap sources for the next transition
            AudioSource temp = musicSource1;
            musicSource1 = musicSource2;
            musicSource2 = temp;
            // Reset volumes for the next transition
            musicSource1.volume = 1f;
            musicSource2.volume = 0f;
        }
    }

}
