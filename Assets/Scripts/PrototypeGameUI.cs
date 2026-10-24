using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Runtime HUD, title screen, and game-over overlay shared across scenes.
/// </summary>
public sealed class PrototypeGameUI : MonoBehaviour
{
    private enum ScreenState
    {
        Title,
        Running,
        GameOver
    }

    private const string HighScoreKey = "Prototype.HighScore";
    private const string TitleSceneName = "TitleScene";
    private static PrototypeGameUI instance;

    private Text hudText;
    private Text panelTitle;
    private Text panelDetails;
    private Button primaryButton;
    private GameObject overlay;
    private Image overlayBackdrop;
    private PrototypePlayerController player;
    private PrototypeHealth playerHealth;
    private string gameplaySceneName = "Scene01";
    private ScreenState state;
    private int score;
    private bool sessionStarted;

    public static void AddScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        EnsureInstance();
        instance.score += amount;
    }

    public static void GameOver(string reason)
    {
        EnsureInstance();
        instance.EnsureUI();
        instance.ShowGameOver(reason);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateAtStartup()
    {
        EnsureInstance();
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject root = new("PrototypeGameUI");
        instance = root.AddComponent<PrototypeGameUI>();
        DontDestroyOnLoad(root);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureUI();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    private void Update()
    {
        if (hudText == null || player == null || playerHealth == null)
        {
            return;
        }

        hudText.text = $"HP: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}    "
            + $"MISSILE: {player.MissileCount}/{player.MaxMissiles}    SCORE: {score}";
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = FindPlayerInScene(scene);
        playerHealth = player != null ? player.GetComponent<PrototypeHealth>() : null;
        hudText.gameObject.SetActive(scene.name != TitleSceneName);

        if (player != null && !string.IsNullOrWhiteSpace(player.GameplaySceneName))
        {
            gameplaySceneName = player.GameplaySceneName;
        }

        if (scene.name == TitleSceneName)
        {
            ShowTitle();
            return;
        }

        if (!sessionStarted)
        {
            state = ScreenState.Title;
            Time.timeScale = 1f;
            SceneManager.LoadScene(TitleSceneName);
            return;
        }

        if (state == ScreenState.Running)
        {
            SetPlayerEnabled(true);
            Time.timeScale = 1f;
            overlay.SetActive(false);
        }
    }

    private static PrototypePlayerController FindPlayerInScene(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            PrototypePlayerController found = root.GetComponentInChildren<PrototypePlayerController>(true);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void EnsureUI()
    {
        if (overlay != null && hudText != null && panelTitle != null && panelDetails != null && primaryButton != null)
        {
            return;
        }

        BuildUI();
    }

    private void BuildUI()
    {
        GameObject canvasObject = new("Prototype UI Canvas");
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new("Prototype Event System");
            eventSystemObject.transform.SetParent(transform, false);
            eventSystemObject.AddComponent<EventSystem>();
            InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }

        hudText = CreateText(canvasObject.transform, "Status HUD", 30, TextAnchor.MiddleCenter);
        RectTransform hudRect = hudText.rectTransform;
        hudRect.anchorMin = new Vector2(0.03f, 0.92f);
        hudRect.anchorMax = new Vector2(0.97f, 0.99f);
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;
        hudText.color = Color.white;

        overlay = new GameObject("Title and Game Over Overlay", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(canvasObject.transform, false);
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayBackdrop = overlay.GetComponent<Image>();
        overlayBackdrop.color = new Color(0.025f, 0.035f, 0.055f, 0.98f);

        GameObject panel = new GameObject("Menu Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(overlay.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(620f, 430f);
        panelRect.anchoredPosition = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.15f, 0.97f);

        panelTitle = CreateText(panel.transform, "Panel Title", 54, TextAnchor.MiddleCenter);
        SetAnchors(panelTitle.rectTransform, new Vector2(0.06f, 0.68f), new Vector2(0.94f, 0.94f));
        panelTitle.color = Color.white;

        panelDetails = CreateText(panel.transform, "Panel Details", 30, TextAnchor.MiddleCenter);
        SetAnchors(panelDetails.rectTransform, new Vector2(0.08f, 0.36f), new Vector2(0.92f, 0.7f));
        panelDetails.color = new Color(0.82f, 0.88f, 0.95f);

        primaryButton = CreateButton(panel.transform, "Start", 32);
        SetAnchors(primaryButton.GetComponent<RectTransform>(), new Vector2(0.22f, 0.1f), new Vector2(0.78f, 0.3f));
        primaryButton.onClick.AddListener(StartGame);
        overlay.SetActive(false);
    }

    private static Text CreateText(Transform parent, string objectName, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new(objectName, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(Transform parent, string label, int fontSize)
    {
        GameObject buttonObject = new("Primary Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.08f, 0.48f, 0.72f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.12f, 0.61f, 0.86f, 1f);
        colors.selectedColor = new Color(0.12f, 0.61f, 0.86f, 1f);
        colors.pressedColor = new Color(0.05f, 0.34f, 0.52f, 1f);
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.Automatic };

        Text text = CreateText(buttonObject.transform, "Button Label", fontSize, TextAnchor.MiddleCenter);
        SetAnchors(text.rectTransform, Vector2.zero, Vector2.one);
        text.color = Color.white;
        text.text = label;
        return button;
    }

    private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void ShowTitle()
    {
        EnsureUI();
        state = ScreenState.Title;
        Time.timeScale = 0f;
        SetPlayerEnabled(false);
        overlayBackdrop.color = new Color(0.025f, 0.035f, 0.055f, 0.98f);
        panelTitle.text = "TITLE";
        panelDetails.text = $"HIGH SCORE: {PlayerPrefs.GetInt(HighScoreKey, 0)}";
        SetButton("START", StartGame);
        overlay.SetActive(true);
        FocusPrimaryButton();
    }

    private void StartGame()
    {
        score = 0;
        sessionStarted = true;
        state = ScreenState.Running;
        Time.timeScale = 1f;
        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("PrototypeGameUI: Set a gameplay scene name on the Player prefab.");
            ShowTitle();
            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    private void ShowGameOver(string reason)
    {
        if (state == ScreenState.GameOver)
        {
            return;
        }

        state = ScreenState.GameOver;
        Time.timeScale = 0f;
        SetPlayerEnabled(false);
        int highScore = Mathf.Max(PlayerPrefs.GetInt(HighScoreKey, 0), score);
        PlayerPrefs.SetInt(HighScoreKey, highScore);
        PlayerPrefs.Save();
        overlayBackdrop.color = new Color(0.025f, 0.035f, 0.055f, 0.58f);
        panelTitle.text = "GAME OVER";
        panelDetails.text = $"{reason}\nSCORE: {score}\nHIGH SCORE: {highScore}";
        SetButton("TITLE", ReturnToTitle);
        overlay.SetActive(true);
        FocusPrimaryButton();
    }

    private void ReturnToTitle()
    {
        state = ScreenState.Title;
        Time.timeScale = 1f;
        SceneManager.LoadScene(TitleSceneName);
    }

    private void SetButton(string label, UnityEngine.Events.UnityAction action)
    {
        primaryButton.onClick.RemoveAllListeners();
        primaryButton.onClick.AddListener(action);
        primaryButton.GetComponentInChildren<Text>().text = label;
    }

    private void FocusPrimaryButton()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(primaryButton.gameObject);
    }

    private void SetPlayerEnabled(bool enabled)
    {
        if (player != null)
        {
            player.enabled = enabled;
        }
    }
}
