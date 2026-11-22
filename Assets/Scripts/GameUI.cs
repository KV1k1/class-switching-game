using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;
    public Health playerHealth;

    RectTransform healthFill;
    Text healthText;
    Image flash;
    GameObject endScreen;
    Text endTitle, endSub;
    Font font;
    float flashAlpha;
    bool ended;

    // Boss bar
    GameObject bossBar;
    RectTransform bossFill;
    Text bossNameText;
    Health bossHealth;

    // Pause menu
    GameObject pauseScreen;
    bool isPaused;
    Text pauseSchemeText;

    // Run XP and level-up choices
    PlayerExperience playerExperience;
    RectTransform experienceFill;
    Text experienceText, masteryResourceText;
    Text pickupPromptText;
    GameObject levelUpScreen;
    RunPassive[] offeredPassives = new RunPassive[3];
    Button[] passiveChoiceButtons = new Button[3];

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        font = Font.CreateDynamicFontFromOSFont("Arial", 16);

        GameObject canvasObj = new GameObject("GameUI Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        Transform root = canvasObj.transform;

        playerExperience = playerHealth.GetComponent<PlayerExperience>();
        bool inExpedition = SceneManager.GetActiveScene().name == "SampleScene";

        // Run XP bar, directly under the health bar
        Image experienceBg = NewImage(root, "Experience Bar", new Color(0f, 0f, 0f, 0.65f));
        experienceBg.gameObject.SetActive(inExpedition);
        RectTransform experienceBgRect = experienceBg.rectTransform;
        experienceBgRect.anchorMin = experienceBgRect.anchorMax = new Vector2(0f, 1f);
        experienceBgRect.pivot = new Vector2(0f, 1f);
        experienceBgRect.anchoredPosition = new Vector2(40f, -88f);
        experienceBgRect.sizeDelta = new Vector2(420f, 28f);
        Outline experienceBorder = experienceBg.gameObject.AddComponent<Outline>();
        experienceBorder.effectColor = Color.white;
        experienceBorder.effectDistance = new Vector2(2f, 2f);

        Image experienceFillImage = NewImage(experienceBg.transform, "Fill", new Color(0.15f, 0.65f, 0.95f));
        experienceFill = experienceFillImage.rectTransform;
        Stretch(experienceFill);
        experienceText = NewText(experienceBg.transform, "Experience Text", "", 20, TextAnchor.MiddleCenter, Color.white);
        Stretch(experienceText.rectTransform);

        masteryResourceText = NewText(root, "Run Mastery Resource", "", 22, TextAnchor.MiddleLeft, new Color(0.84f, 0.76f, 0.98f));
        masteryResourceText.rectTransform.anchorMin = masteryResourceText.rectTransform.anchorMax = new Vector2(0f, 1f);
        masteryResourceText.rectTransform.pivot = new Vector2(0f, 1f);
        masteryResourceText.rectTransform.anchoredPosition = new Vector2(40f, -122f);
        masteryResourceText.rectTransform.sizeDelta = new Vector2(420f, 30f);
        masteryResourceText.gameObject.SetActive(inExpedition);

        pickupPromptText = NewText(root, "Pickup Prompt", "", 32, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.25f));
        SetBox(pickupPromptText.rectTransform, new Vector2(0f, 205f), new Vector2(1100f, 60f));

        // Red flash over the whole screen when hit
        flash = NewImage(root, "Damage Flash", new Color(1f, 0f, 0f, 0f));
        Stretch(flash.rectTransform);
        flash.raycastTarget = false;

        // Player health bar, top left
        Image bg = NewImage(root, "Health Bar", new Color(0f, 0f, 0f, 0.6f));
        RectTransform bgr = bg.rectTransform;
        bgr.anchorMin = bgr.anchorMax = new Vector2(0, 1);
        bgr.pivot = new Vector2(0, 1);
        bgr.anchoredPosition = new Vector2(40, -40);
        bgr.sizeDelta = new Vector2(420, 40);
        Outline border = bg.gameObject.AddComponent<Outline>();
        border.effectColor = Color.white;
        border.effectDistance = new Vector2(3, 3);

        Image fill = NewImage(bg.transform, "Fill", new Color(0.8f, 0.1f, 0.1f));
        healthFill = fill.rectTransform;
        Stretch(healthFill);

        healthText = NewText(bg.transform, "HP Text", "", 26, TextAnchor.MiddleCenter, Color.white);
        Stretch(healthText.rectTransform);

        // Boss bar, top center (hidden until the boss wakes up)
        Image bossBg = NewImage(root, "Boss Bar", new Color(0f, 0f, 0f, 0.7f));
        bossBar = bossBg.gameObject;
        RectTransform bbr = bossBg.rectTransform;
        bbr.anchorMin = bbr.anchorMax = new Vector2(0.5f, 1f);
        bbr.pivot = new Vector2(0.5f, 1f);
        bbr.anchoredPosition = new Vector2(0, -40);
        bbr.sizeDelta = new Vector2(900, 40);
        Outline bossBorder = bossBar.AddComponent<Outline>();
        bossBorder.effectColor = Color.white;
        bossBorder.effectDistance = new Vector2(3, 3);

        Image bossFillImg = NewImage(bossBg.transform, "Fill", new Color(0.55f, 0.1f, 0.65f));
        bossFill = bossFillImg.rectTransform;
        Stretch(bossFill);

        bossNameText = NewText(bossBg.transform, "Boss Name", "", 26, TextAnchor.MiddleCenter, Color.white);
        Stretch(bossNameText.rectTransform);
        bossBar.SetActive(false);

        // Pause Screen
        pauseScreen = new GameObject("Pause Screen");
        pauseScreen.transform.SetParent(root, false);
        Image pauseDim = pauseScreen.AddComponent<Image>();
        pauseDim.color = new Color(0f, 0f, 0f, 0.85f);
        Stretch(pauseDim.rectTransform);

        Text pauseTitle = NewText(pauseScreen.transform, "Title", "PAUSED", 90, TextAnchor.MiddleCenter, Color.white);
        SetBox(pauseTitle.rectTransform, new Vector2(0, 60), new Vector2(800, 150));

        Text pauseSub = NewText(pauseScreen.transform, "Sub", "Press ESC to Resume\nPress BACKSPACE to Quit", 30, TextAnchor.MiddleCenter, Color.gray);
        SetBox(pauseSub.rectTransform, new Vector2(0, -35), new Vector2(800, 80));
        pauseSchemeText = NewText(pauseScreen.transform, "Input Scheme", "", 28, TextAnchor.MiddleCenter, Color.white);
        SetBox(pauseSchemeText.rectTransform, new Vector2(0, -115), new Vector2(900, 45));
        CreateButton(pauseScreen.transform, "Click-to-Move", new Vector2(0, -180), () => SetInputScheme(InputScheme.ClickToMove));
        CreateButton(pauseScreen.transform, "WASD", new Vector2(0, -245), () => SetInputScheme(InputScheme.Wasd));
        CreateButton(pauseScreen.transform, "Resume", new Vector2(0, -320), TogglePause);
        pauseScreen.SetActive(false);

        // End screen (hidden until needed)
        endScreen = new GameObject("End Screen");
        endScreen.transform.SetParent(root, false);
        Image dim = endScreen.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.75f);
        Stretch(dim.rectTransform);

        endTitle = NewText(endScreen.transform, "Title", "", 110, TextAnchor.MiddleCenter, Color.white);
        SetBox(endTitle.rectTransform, new Vector2(0, 60), new Vector2(1400, 200));
        endSub = NewText(endScreen.transform, "Sub", "", 40, TextAnchor.MiddleCenter, Color.white);
        SetBox(endSub.rectTransform, new Vector2(0, -80), new Vector2(1400, 80));
        endScreen.SetActive(false);

        // Level-up screen is built at runtime so each level presents three passive choices.
        levelUpScreen = new GameObject("Level Up Screen");
        levelUpScreen.transform.SetParent(root, false);
        Image levelUpDim = levelUpScreen.AddComponent<Image>();
        levelUpDim.color = new Color(0f, 0f, 0f, 0.88f);
        Stretch(levelUpDim.rectTransform);

        Text levelUpTitle = NewText(levelUpScreen.transform, "Level Up Title", "LEVEL UP!", 82, TextAnchor.MiddleCenter, Color.white);
        SetBox(levelUpTitle.rectTransform, new Vector2(0f, 210f), new Vector2(1000f, 120f));
        Text levelUpSubtitle = NewText(levelUpScreen.transform, "Level Up Subtitle", "Choose a passive for this run", 32, TextAnchor.MiddleCenter, Color.gray);
        SetBox(levelUpSubtitle.rectTransform, new Vector2(0f, 125f), new Vector2(1000f, 70f));

        passiveChoiceButtons[0] = CreateButton(levelUpScreen.transform, "", new Vector2(-390f, 0f), () => ChoosePassive(0), new Vector2(340f, 150f));
        passiveChoiceButtons[1] = CreateButton(levelUpScreen.transform, "", new Vector2(0f, 0f), () => ChoosePassive(1), new Vector2(340f, 150f));
        passiveChoiceButtons[2] = CreateButton(levelUpScreen.transform, "", new Vector2(390f, 0f), () => ChoosePassive(2), new Vector2(340f, 150f));
        levelUpScreen.SetActive(false);

        playerHealth.Damaged += OnPlayerDamaged;

        if (SceneManager.GetActiveScene().name == "HubScene")
            HubWorldController.Initialize(playerHealth);
        else if (SceneManager.GetActiveScene().name == "SampleScene")
            ArchivistNpcController.SpawnForFirstExpedition(playerHealth);
    }

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.Damaged -= OnPlayerDamaged;
    }

    void OnPlayerDamaged(float amount)
    {
        flashAlpha = 0.45f;
    }

    void Update()
    {
        // Handle Pause Input
        if (!ended && (levelUpScreen == null || !levelUpScreen.activeSelf) && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (pauseSchemeText != null)
            pauseSchemeText.text = "Input scheme: " + SchemeName(InputSchemeSettings.Current);

        if (playerExperience != null && SceneManager.GetActiveScene().name == "SampleScene")
        {
            experienceFill.anchorMax = new Vector2(
                Mathf.Clamp01((float)playerExperience.currentExperience / playerExperience.experienceToNextLevel), 1f);
            experienceText.text = "LV " + playerExperience.currentLevel + "   XP " +
                playerExperience.currentExperience + " / " + playerExperience.experienceToNextLevel;
            masteryResourceText.text = "MEMORY SHARDS  " + GameProgression.RunMasteryResource;

            if (!ended && playerExperience.PendingLevelChoices > 0 && !levelUpScreen.activeSelf)
                ShowLevelUpChoices();
        }

        // Handle Quit while paused
        if (isPaused && Input.GetKeyDown(KeyCode.Backspace))
        {
            Application.Quit();
        }

        float fraction = Mathf.Clamp01(playerHealth.Current / playerHealth.maxHealth);
        healthFill.anchorMax = new Vector2(fraction, 1f);
        healthText.text = "HP  " + Mathf.CeilToInt(playerHealth.Current) + " / " + Mathf.CeilToInt(playerHealth.maxHealth);

        if (bossHealth != null)
            bossFill.anchorMax = new Vector2(Mathf.Clamp01(bossHealth.Current / bossHealth.maxHealth), 1f);

        flashAlpha = Mathf.MoveTowards(flashAlpha, 0f, Time.unscaledDeltaTime * 1.5f);
        flash.color = new Color(1f, 0f, 0f, flashAlpha);

        if (ended && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f; // Ensure time is reset if restarting
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void ShowBossBar(string bossName, Health boss)
    {
        bossHealth = boss;
        bossNameText.text = bossName;
        bossBar.SetActive(true);
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        pauseScreen.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    void SetInputScheme(InputScheme scheme)
    {
        InputSchemeSettings.Set(scheme);
    }

    static string SchemeName(InputScheme scheme)
    {
        return scheme == InputScheme.Wasd ? "WASD" : "Click-to-Move";
    }

    Button CreateButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        return CreateButton(parent, label, position, action, new Vector2(360f, 54f));
    }

    Button CreateButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action, Vector2 size)
    {
        GameObject buttonObject = new GameObject(label + " Button");
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.22f, 0.18f, 0.28f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        Text text = NewText(buttonObject.transform, "Label", label, size.y > 80f ? 25 : 26, TextAnchor.MiddleCenter, Color.white);
        Stretch(text.rectTransform);
        return button;
    }

    void ShowLevelUpChoices()
    {
        RunPassive[] allPassives = { RunPassive.Vitality, RunPassive.Fleetfoot, RunPassive.Siphon, RunPassive.Bounty };
        List<RunPassive> choices = new List<RunPassive>(allPassives);
        for (int i = 0; i < offeredPassives.Length; i++)
        {
            int selected = Random.Range(0, choices.Count);
            offeredPassives[i] = choices[selected];
            choices.RemoveAt(selected);
        }

        for (int i = 0; i < offeredPassives.Length; i++)
        {
            Text label = passiveChoiceButtons[i].GetComponentInChildren<Text>();
            label.text = PassiveName(offeredPassives[i]) + "\n" + PassiveDescription(offeredPassives[i]);
        }

        Time.timeScale = 0f;
        levelUpScreen.SetActive(true);
    }

    void ChoosePassive(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= offeredPassives.Length) return;
        playerExperience.ChoosePassive(offeredPassives[buttonIndex]);
        levelUpScreen.SetActive(false);

        if (playerExperience.PendingLevelChoices > 0)
            ShowLevelUpChoices();
        else
            Time.timeScale = 1f;
    }

    static string PassiveName(RunPassive passive)
    {
        switch (passive)
        {
            case RunPassive.Vitality: return "VITALITY";
            case RunPassive.Fleetfoot: return "FLEETFOOT";
            case RunPassive.Siphon: return "SIPHON";
            default: return "BOUNTY";
        }
    }

    static string PassiveDescription(RunPassive passive)
    {
        switch (passive)
        {
            case RunPassive.Vitality: return "+20 max health and heal 20";
            case RunPassive.Fleetfoot: return "+0.75 movement speed";
            case RunPassive.Siphon: return "+2 health restored per kill";
            default: return "+20% XP from future kills";
        }
    }

    void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }
    public void ShowNotification(string message)
    {
        // Create a temporary text object on the canvas
        GameObject notifObj = new GameObject("Notification Text");
        notifObj.transform.SetParent(healthText.transform.parent.parent, false);

        Text t = notifObj.AddComponent<Text>();
        t.font = font;
        t.text = message;
        t.fontSize = 45;
        t.alignment = TextAnchor.UpperCenter;
        t.color = new Color(1f, 0.85f, 0.2f); // Yellow/Gold
        t.raycastTarget = false;

        // Position it at the top center of the screen
        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -100);
        rt.sizeDelta = new Vector2(800, 200);

        // Add a shadow for better readability
        Outline shadow = notifObj.AddComponent<Outline>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(2, -2);

        // Auto-destroy the message after 4 seconds
        Destroy(notifObj, 4f);
    }

    public void SetPickupPrompt(string message)
    {
        if (pickupPromptText != null) pickupPromptText.text = message;
    }

    public void HideBossBar()
    {
        bossHealth = null;
        bossBar.SetActive(false);
    }

    public void ShowEnd(string title, string subtitle, Color titleColor)
    {
        ended = true;
        endScreen.SetActive(true);
        endTitle.text = title;
        endTitle.color = titleColor;
        endSub.text = subtitle;
    }

    // ---- small helpers ----
    Image NewImage(Transform parent, string name, Color c)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = c;
        return img;
    }

    Text NewText(Transform parent, string name, string content, int size, TextAnchor align, Color c)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = font;
        t.text = content;
        t.fontSize = size;
        t.alignment = align;
        t.color = c;
        t.raycastTarget = false;
        return t;
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void SetBox(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
