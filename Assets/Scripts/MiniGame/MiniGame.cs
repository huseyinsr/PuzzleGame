using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MiniGame : MonoBehaviour
{
    [Serializable]
    private class Level
    {
        [SerializeField] private GameObject level;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform obstacles;
        [SerializeField] private Collider winArea;

        public GameObject LevelObject => level;
        public Transform SpawnPoint => spawnPoint;
        public Transform Obstacles => obstacles;
        public Collider WinArea => winArea;
    }

    [Header("Game")]
    [SerializeField] private Rocket rocket;
    [SerializeField] private GameObject game;
    [SerializeField] private TMP_Text levelText;

    [Header("Crystal")]
    [SerializeField] private GameObject[] crystalOn;
    [SerializeField] private GameObject[] crystalOff;

    [Header("Telescope")]
    [SerializeField] private GameObject[] telescopeOn;
    [SerializeField] private GameObject[] telescopeOff;

    [Header("Levels")]
    [SerializeField] private List<Level> levels = new();

    [Header("Buttons")]
    [SerializeField] private Collider leftButton;
    [SerializeField] private Collider rightButton;
    [SerializeField] private Collider forwardButton;

    [SerializeField] private Camera gameCamera;
    [SerializeField] private float buttonDistance = 10f;

    [Header("Button Hover")]
    [SerializeField] private float hoverSize = 1.08f;
    [SerializeField] private float hoverSpeed = 10f;

    [Header("Button Click")]
    [SerializeField] private float clickSize = 0.9f;
    [SerializeField] private float clickDepth = 0.01f;
    [SerializeField] private float clickTime = 0.1f;
    [SerializeField] private float clickSpeed = 15f;

    [Header("Zoom")]
    [SerializeField] private bool needsZoom = true;
    [SerializeField] private float zoomDelay = 0.15f;

    private readonly Dictionary<Collider, Vector3> buttonSizes = new();
    private readonly Dictionary<Collider, Vector3> buttonPositions = new();

    private bool crystalReady;
    private bool telescopeReady;
    private bool gameStarted;
    private bool gameFinished;

    private int currentLevel;

    private Collider hoverButton;
    private Collider clickedButton;

    private void OnEnable()
    {
        CrystalDetector.OnPlaced += CrystalPlaced;
        TelescopeDetecter.OnPlaced += TelescopePlaced;

        if (rocket != null)
        {
            rocket.OnColliderEntered += RocketHit;
        }
    }

    private void OnDisable()
    {
        CrystalDetector.OnPlaced -= CrystalPlaced;
        TelescopeDetecter.OnPlaced -= TelescopePlaced;

        if (rocket != null)
        {
            rocket.OnColliderEntered -= RocketHit;
        }
    }

    private void Start()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }

        SaveButton(leftButton);
        SaveButton(rightButton);
        SaveButton(forwardButton);

        crystalReady = CrystalDetector.IsPlaced;
        telescopeReady = TelescopeDetecter.IsPlaced;

        UpdateItems();

        if (!BothItemsReady())
        {
            if (game != null)
            {
                game.SetActive(false);
            }
        }
        else
        {
            StartGame();
        }
    }

    private void Update()
    {
        bool zoomReady = ZoomReady();

        bool canPlay =
            BothItemsReady() &&
            gameStarted &&
            !gameFinished &&
            zoomReady;

        if (rocket != null)
        {
            rocket.SetControlEnabled(canPlay);
        }

        hoverButton = zoomReady ? FindButton() : null;

        if (zoomReady &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            hoverButton != null)
        {
            ButtonClicked(hoverButton, canPlay);
        }

        AnimateButton(leftButton);
        AnimateButton(rightButton);
        AnimateButton(forwardButton);
    }

    private void CrystalPlaced()
    {
        crystalReady = true;

        UpdateItems();
        TryStartGame();
    }

    private void TelescopePlaced()
    {
        telescopeReady = true;

        UpdateItems();
        TryStartGame();
    }

    private void TryStartGame()
    {
        if (!gameStarted && BothItemsReady())
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        if (levels.Count == 0)
        {
            return;
        }

        gameStarted = true;
        gameFinished = false;

        if (game != null)
        {
            game.SetActive(true);
        }

        SetLevel(0);
    }

    private void UpdateItems()
    {
        HashSet<GameObject> sharedObjects = new();

        foreach (GameObject obj in crystalOn)
        {
            if (obj != null && ContainsObject(telescopeOn, obj))
            {
                obj.SetActive(crystalReady && telescopeReady);
                sharedObjects.Add(obj);
            }
        }

        foreach (GameObject obj in crystalOff)
        {
            if (obj != null && ContainsObject(telescopeOff, obj))
            {
                obj.SetActive(!crystalReady || !telescopeReady);
                sharedObjects.Add(obj);
            }

        SetObjects(crystalOn, crystalReady, sharedObjects);
        SetObjects(crystalOff, !crystalReady, sharedObjects);

        }
        SetObjects(telescopeOn, telescopeReady, sharedObjects);
        SetObjects(telescopeOff, !telescopeReady, sharedObjects);
    }

    private void SetObjects(
        GameObject[] objects,
        bool active,
        HashSet<GameObject> ignoredObjects)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null && !ignoredObjects.Contains(obj))
            {
                obj.SetActive(active);
            }
        }
    }

    private bool ContainsObject(
        GameObject[] objects,
        GameObject target)
    {
        foreach (GameObject obj in objects)
        {
            if (obj == target)
            {
                return true;
            }
        }

        return false;
    }

    private bool BothItemsReady()
    {
        return crystalReady && telescopeReady;
    }

    private bool ZoomReady()
    {
        if (!needsZoom)
        {
            return true;
        }

        if (ZoomManager.Instance == null)
        {
            return false;
        }

        if (!ZoomManager.Instance.IsZoomed)
        {
            return false;
        }

        return Time.time - ZoomManager.Instance.ZoomStartTime >= zoomDelay;
    }

    private void SaveButton(Collider button)
    {
        if (button == null)
        {
            return;
        }

        buttonSizes[button] = button.transform.localScale;
        buttonPositions[button] = button.transform.localPosition;
    }

    private Collider FindButton()
    {
        if (gameCamera == null || Mouse.current == null)
        {
            return null;
        }

        Ray ray = gameCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            buttonDistance,
            ~0,
            QueryTriggerInteraction.Collide
        );

        Collider foundButton = null;
        float closestDistance = Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            bool isButton =
                hit.collider == leftButton ||
                hit.collider == rightButton ||
                hit.collider == forwardButton;

            if (!isButton)
            {
                continue;
            }

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                foundButton = hit.collider;
            }
        }

        return foundButton;
    }

    private void ButtonClicked(Collider button, bool canPlay)
    {
        StartCoroutine(ClickEffect(button));

        if (!canPlay || rocket == null)
        {
            return;
        }

        if (button == leftButton)
        {
            rocket.TryTurnLeft();
        }
        else if (button == rightButton)
        {
            rocket.TryTurnRight();
        }
        else if (button == forwardButton)
        {
            rocket.TryMoveForward();
        }
    }

    private IEnumerator ClickEffect(Collider button)
    {
        clickedButton = button;

        yield return new WaitForSeconds(clickTime);

        if (clickedButton == button)
        {
            clickedButton = null;
        }
    }

    private void AnimateButton(Collider button)
    {
        if (button == null)
        {
            return;
        }

        if (!buttonSizes.ContainsKey(button) ||
            !buttonPositions.ContainsKey(button))
        {
            return;
        }

        Vector3 normalSize = buttonSizes[button];
        Vector3 normalPosition = buttonPositions[button];

        Vector3 targetSize = normalSize;
        Vector3 targetPosition = normalPosition;

        float speed = hoverSpeed;

        if (clickedButton == button)
        {
            targetSize = normalSize * clickSize;

            Vector3 pressDirection =
                -(button.transform.localRotation * Vector3.forward);

            targetPosition =
                normalPosition +
                pressDirection * clickDepth;

            speed = clickSpeed;
        }
        else if (hoverButton == button)
        {
            targetSize = normalSize * hoverSize;
        }

        button.transform.localScale = Vector3.Lerp(
            button.transform.localScale,
            targetSize,
            Time.deltaTime * speed
        );

        button.transform.localPosition = Vector3.Lerp(
            button.transform.localPosition,
            targetPosition,
            Time.deltaTime * speed
        );
    }

    private void RocketHit(Collider other)
    {
        if (!gameStarted ||
            gameFinished ||
            levels.Count == 0)
        {
            return;
        }

        Level level = levels[currentLevel];

        if (other == level.WinArea)
        {
            NextLevel();
            return;
        }

        if (level.Obstacles == null)
        {
            return;
        }

        Transform hit = other.transform;

        if (hit == level.Obstacles ||
            hit.IsChildOf(level.Obstacles))
        {
            Respawn();
        }
    }

    private void NextLevel()
    {
        int nextLevel = currentLevel + 1;

        if (nextLevel >= levels.Count)
        {
            gameFinished = true;

            if (rocket != null)
            {
                rocket.SetControlEnabled(false);
            }

            if (levelText != null)
            {
                levelText.text = "Completed";
            }

            return;
        }

        SetLevel(nextLevel);
    }

    private void SetLevel(int levelNumber)
    {
        currentLevel = levelNumber;
        gameFinished = false;

        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i].LevelObject != null)
            {
                levels[i].LevelObject.SetActive(
                    i == currentLevel
                );
            }
        }

        if (levelText != null)
        {
            levelText.text =
                "Level " + (currentLevel + 1);
        }

        Respawn();
    }

    private void Respawn()
    {
        if (levels.Count == 0 || rocket == null)
        {
            return;
        }

        Level level = levels[currentLevel];

        if (level.SpawnPoint == null)
        {
            return;
        }

        rocket.ResetRocket(
            level.SpawnPoint.position,
            level.SpawnPoint.rotation
        );
    }
}