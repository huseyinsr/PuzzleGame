
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SoundEffect : MonoBehaviour
{
    public static SoundEffect Instance { get; private set; }

    [System.Serializable]
    private class SoundSettings
    {
        public AudioSource source;

        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.25f, 3f)] public float pitch = 1f;
        [Range(0f, 0.3f)] public float randomPitch = 0.03f;
        [Range(-1f, 1f)] public float stereoPan = 0f;
        [Min(0f)] public float startOffsetSeconds = 0f;

        public bool useLowPass = false;
        [Range(10f, 22000f)] public float lowPassCutoff = 5000f;

        public bool useHighPass = false;
        [Range(10f, 22000f)] public float highPassCutoff = 500f;
    }

    [Header("One Shared Click Sound")]
    [SerializeField] private AudioClip clickClip;

    [Header("1 - Pickup")]
    [SerializeField]
    private SoundSettings pickup = new SoundSettings
    {
        volume = 0.8f,
        pitch = 1.25f,
        randomPitch = 0.05f
    };

    [Header("2 - Place Item")]
    [SerializeField]
    private SoundSettings place = new SoundSettings
    {
        volume = 0.95f,
        pitch = 0.75f,
        randomPitch = 0.04f,
        useLowPass = true,
        lowPassCutoff = 4500f
    };

    [Header("3 - Inventory Slot")]
    [SerializeField]
    private SoundSettings inventory = new SoundSettings
    {
        volume = 0.55f,
        pitch = 1.6f,
        randomPitch = 0.08f,
        useHighPass = true,
        highPassCutoff = 700f
    };

    [Header("4 - Empty Area Click")]
    [SerializeField]
    private SoundSettings emptyArea = new SoundSettings
    {
        volume = 0.3f,
        pitch = 0.9f,
        randomPitch = 0.04f,
        startOffsetSeconds = 0.01f,
        useLowPass = true,
        lowPassCutoff = 3000f
    };

    [Header("5 - Camera Buttons")]
    [SerializeField]
    private SoundSettings arrowButtons = new SoundSettings
    {
        volume = 0.65f,
        pitch = 1.1f,
        randomPitch = 0.04f,
        useHighPass = true,
        highPassCutoff = 450f
    };

    [Header("Camera Button References")]
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button upArrowButton;
    [SerializeField] private Button downArrowButton;
    [SerializeField] private Button exitZoomButton;

    [Header("Empty Area Detection")]
    [SerializeField] private Camera clickCamera;
    [SerializeField] private float rayDistance = 1000f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("There is already a SoundEffect manager in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        PrepareSource(pickup);
        PrepareSource(place);
        PrepareSource(inventory);
        PrepareSource(emptyArea);
        PrepareSource(arrowButtons);
    }

    private void OnEnable()
    {
        if (Instance != this) return;

        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(PlayArrowClick);

        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(PlayArrowClick);

        if (upArrowButton != null)
            upArrowButton.onClick.AddListener(PlayArrowClick);

        if (downArrowButton != null)
            downArrowButton.onClick.AddListener(PlayArrowClick);

        if (exitZoomButton != null)
            exitZoomButton.onClick.AddListener(PlayArrowClick);
    }

    private void OnDisable()
    {
        if (rightArrowButton != null)
            rightArrowButton.onClick.RemoveListener(PlayArrowClick);

        if (leftArrowButton != null)
            leftArrowButton.onClick.RemoveListener(PlayArrowClick);

        if (upArrowButton != null)
            upArrowButton.onClick.RemoveListener(PlayArrowClick);

        if (downArrowButton != null)
            downArrowButton.onClick.RemoveListener(PlayArrowClick);

        if (exitZoomButton != null)
            exitZoomButton.onClick.RemoveListener(PlayArrowClick);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayPickup() => PlaySound(pickup);
    public void PlayPlace() => PlaySound(place);
    public void PlayInventory() => PlaySound(inventory);
    public void PlayEmptyClick() => PlaySound(emptyArea);
    public void PlayArrowClick() => PlaySound(arrowButtons);

    private void PrepareSource(SoundSettings settings)
    {
        if (settings.source == null) return;

        settings.source.playOnAwake = false;
        settings.source.loop = false;
        settings.source.spatialBlend = 0f;
        settings.source.clip = clickClip;

        ConfigureFilters(settings);
    }

    private void PlaySound(SoundSettings settings)
    {
        if (clickClip == null || settings.source == null) return;

        AudioSource source = settings.source;

        source.Stop();
        source.clip = clickClip;
        source.volume = settings.volume;

        source.pitch = Mathf.Clamp(
            settings.pitch + Random.Range(
                -settings.randomPitch,
                settings.randomPitch
            ),
            0.1f,
            3f
        );

        source.panStereo = settings.stereoPan;

        ConfigureFilters(settings);

        source.timeSamples = Mathf.Clamp(
            Mathf.RoundToInt(
                settings.startOffsetSeconds * clickClip.frequency
            ),
            0,
            Mathf.Max(0, clickClip.samples - 1)
        );

        source.Play();
    }

    private void ConfigureFilters(SoundSettings settings)
    {
        if (settings.source == null) return;

        GameObject sourceObject = settings.source.gameObject;

        AudioLowPassFilter lowPass =
            sourceObject.GetComponent<AudioLowPassFilter>();

        if (settings.useLowPass && lowPass == null)
            lowPass = sourceObject.AddComponent<AudioLowPassFilter>();

        if (lowPass != null)
        {
            lowPass.enabled = settings.useLowPass;
            lowPass.cutoffFrequency = settings.lowPassCutoff;
        }

        AudioHighPassFilter highPass =
            sourceObject.GetComponent<AudioHighPassFilter>();

        if (settings.useHighPass && highPass == null)
            highPass = sourceObject.AddComponent<AudioHighPassFilter>();

        if (highPass != null)
        {
            highPass.enabled = settings.useHighPass;
            highPass.cutoffFrequency = settings.highPassCutoff;
        }
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        Camera cam = clickCamera != null
            ? clickCamera
            : Camera.main;

        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit3D, rayDistance))
        {
            if (IsItemInteraction(hit3D.collider.gameObject))
                return;
        }

        RaycastHit2D hit2D =
            Physics2D.GetRayIntersection(ray, rayDistance);

        if (hit2D.collider != null &&
            IsItemInteraction(hit2D.collider.gameObject))
            return;

        PlayEmptyClick();
    }

    private bool IsItemInteraction(GameObject target)
    {
        return
            target.GetComponentInParent<PickupItem>() != null ||
            target.GetComponentInParent<ItemInteractionReceiver>() != null;
    }
}
