using UnityEngine;

public class CandleLight : MonoBehaviour
{
    [SerializeField] private Material candleLight;
    [SerializeField] private float minTransparent = 0.5f;
    [SerializeField] private float maxTransparent = 1.5f;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private Light candleLightComponent;
    [SerializeField] private float flickerSpeed = 0.1f;
    [SerializeField] private float flickerStrength = 0.1f;

    private void Start()
    {
        if (candleLightComponent == null)
        {
            return;
        }
    }


    private void Update()
    {
        if (candleLightComponent == null)
        {
            return;
        }
        FlickerLight();
        LightFlicker();

    }

    private void FlickerLight()
    {
        float randomTransparent = Random.Range(minTransparent, maxTransparent);
        Color color = candleLight.color;
        color.a = Mathf.Lerp(color.a, randomTransparent, flickerSpeed * Time.deltaTime * flickerStrength);
        candleLight.color = color;
    }

    private void LightFlicker()
    {
        float randomIntensity = Random.Range(minIntensity, maxIntensity);
        candleLightComponent.intensity = Mathf.Lerp(candleLightComponent.intensity, randomIntensity, flickerSpeed * Time.deltaTime * flickerStrength);
    }



}
