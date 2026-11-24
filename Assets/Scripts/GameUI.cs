using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;
    static Sprite thoughtDotSprite;
    class ThoughtBubbleEntry
    {
        public Transform speaker;
        public RectTransform bubble;
        public GameObject root;
    }
    public Health playerHealth;

    RectTransform healthFill;
    Text healthText;
    Image flash;
    GameObject endScreen;
    Text endTitle, endSub;
    GameObject expeditionReportScreen;
    Text expeditionReportTitle, expeditionReportDetails;
    bool expeditionReportOpen;
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
    Text expeditionTimerText;
    Text expeditionStatusText;
    Text pickupPromptText;
    GameObject levelUpScreen;
    RunPassive[] offeredPassives = new RunPassive[3];
    Button[] passiveChoiceButtons = new Button[3];
    Transform uiRoot;
    GameObject archivistMenu;
    Text archivistBalanceText, archivistClassText, archivistUpgradeText;
    Button archivistUpgradeButton;
    readonly List<Button> archivistClassButtons = new List<Button>();
    readonly List<string> archivistClassNames = new List<string>();
    readonly List<ThoughtBubbleEntry> thoughtBubbles = new List<ThoughtBubbleEntry>();
    string selectedMasteryClass;
    int selectedMasteryPage;
    bool archivistMenuOpen;
    float expeditionStartedAt;
    Coroutine expeditionStatusRoutine;

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
        uiRoot = root;

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

        expeditionTimerText = NewText(root, "Expedition Timer", "EXPEDITION  00:00", 22, TextAnchor.MiddleLeft, Color.white);
        expeditionTimerText.rectTransform.anchorMin = expeditionTimerText.rectTransform.anchorMax = new Vector2(0f, 1f);
        expeditionTimerText.rectTransform.pivot = new Vector2(0f, 1f);
        expeditionTimerText.rectTransform.anchoredPosition = new Vector2(40f, -158f);
        expeditionTimerText.rectTransform.sizeDelta = new Vector2(420f, 30f);
        expeditionTimerText.gameObject.SetActive(inExpedition);
        expeditionStartedAt = Time.time;

        expeditionStatusText = NewText(root, "Expedition Status", "", 18, TextAnchor.MiddleLeft, new Color(1f, 0.88f, 0.65f));
        expeditionStatusText.rectTransform.anchorMin = expeditionStatusText.rectTransform.anchorMax = new Vector2(0f, 1f);
        expeditionStatusText.rectTransform.pivot = new Vector2(0f, 1f);
        expeditionStatusText.rectTransform.anchoredPosition = new Vector2(40f, -190f);
        expeditionStatusText.rectTransform.sizeDelta = new Vector2(560f, 30f);
        expeditionStatusText.gameObject.SetActive(false);

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

        expeditionReportScreen = new GameObject("Expedition Report Screen");
        expeditionReportScreen.transform.SetParent(root, false);
        Image reportDim = expeditionReportScreen.AddComponent<Image>();
        reportDim.color = new Color(0f, 0f, 0f, 0.86f);
        Stretch(reportDim.rectTransform);
        expeditionReportTitle = NewText(expeditionReportScreen.transform,
            "Expedition Report Title", "EXPEDITION REPORT", 66, TextAnchor.MiddleCenter, Color.white);
        SetBox(expeditionReportTitle.rectTransform, new Vector2(0f, 260f), new Vector2(1500f, 100f));
        expeditionReportDetails = NewText(expeditionReportScreen.transform,
            "Expedition Report Details", "", 38, TextAnchor.MiddleCenter, new Color(0.93f, 0.9f, 0.83f));
        SetBox(expeditionReportDetails.rectTransform, new Vector2(0f, 15f), new Vector2(1400f, 360f));
        CreateButton(expeditionReportScreen.transform, "CONTINUE TO PORTAL", new Vector2(0f, -270f),
            CloseExpeditionReport, new Vector2(420f, 64f));
        expeditionReportScreen.SetActive(false);

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
        if (expeditionReportOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
                CloseExpeditionReport();
            return;
        }

        if (archivistMenuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseArchivistMenu();
            return;
        }

        // Handle Pause Input
        if (!archivistMenuOpen && !ended && (levelUpScreen == null || !levelUpScreen.activeSelf) && Input.GetKeyDown(KeyCode.Escape))
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
            int elapsedSeconds = Mathf.Max(0, Mathf.FloorToInt(Time.time - expeditionStartedAt));
            expeditionTimerText.text = "EXPEDITION  " + (elapsedSeconds / 60).ToString("00") + ":" + (elapsedSeconds % 60).ToString("00");

            if (!ended && playerExperience.PendingLevelChoices > 0 && !levelUpScreen.activeSelf)
                ShowLevelUpChoices();
        }

        if (archivistMenuOpen) RenderArchivistMenu();
        UpdateThoughtBubbles();

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

    public void OpenArchivistMenu()
    {
        if (SceneManager.GetActiveScene().name != "HubScene" || !GameProgression.ArchivistRecruited)
            return;

        EnsureArchivistMenu();
        if (archivistClassNames.Count > 0 && string.IsNullOrEmpty(selectedMasteryClass))
            selectedMasteryClass = archivistClassNames[0];
        archivistMenuOpen = true;
        archivistMenu.SetActive(true);
        Time.timeScale = 0f;
        RenderArchivistMenu();
    }

    void EnsureArchivistMenu()
    {
        if (archivistMenu != null) return;

        archivistMenu = new GameObject("Archivist Memory Menu");
        archivistMenu.transform.SetParent(uiRoot, false);
        Image dim = archivistMenu.AddComponent<Image>();
        dim.color = new Color(0.015f, 0.02f, 0.045f, 0.86f);
        Stretch(dim.rectTransform);

        Image panel = NewImage(archivistMenu.transform, "Memory Archive Panel", new Color(0.12f, 0.105f, 0.16f, 0.98f));
        SetBox(panel.rectTransform, Vector2.zero, new Vector2(1080f, 690f));
        Outline panelBorder = panel.gameObject.AddComponent<Outline>();
        panelBorder.effectColor = new Color(0.66f, 0.55f, 0.82f, 0.95f);
        panelBorder.effectDistance = new Vector2(3f, 3f);

        Text title = NewText(panel.transform, "Title", "THE ARCHIVIST'S MEMORY ARCHIVE", 48, TextAnchor.MiddleCenter, new Color(0.94f, 0.86f, 1f));
        SetBox(title.rectTransform, new Vector2(0f, 285f), new Vector2(980f, 70f));
        archivistBalanceText = NewText(panel.transform, "Memory Shard Balance", "MEMORY SHARDS  0", 30, TextAnchor.MiddleCenter, new Color(0.83f, 0.75f, 1f));
        SetBox(archivistBalanceText.rectTransform, new Vector2(0f, 228f), new Vector2(900f, 50f));

        CreateButton(panel.transform, "WEAPON MASTERY TRAITS", new Vector2(-245f, 163f), () => SelectMasteryPage(0), new Vector2(430f, 60f));
        CreateButton(panel.transform, "CLASS SKILL TREES", new Vector2(245f, 163f), () => SelectMasteryPage(1), new Vector2(430f, 60f));

        archivistClassText = NewText(panel.transform, "Selected Class", "SELECT A CLASS", 27, TextAnchor.MiddleCenter, Color.white);
        SetBox(archivistClassText.rectTransform, new Vector2(0f, 106f), new Vector2(900f, 45f));

        string[] unlockedNames = GameProgression.GetUnlockedClassNames();
        System.Array.Sort(unlockedNames);
        for (int i = 0; i < unlockedNames.Length && i < 4; i++)
        {
            string className = unlockedNames[i];
            float x = (i - (Mathf.Min(unlockedNames.Length, 4) - 1) * 0.5f) * 225f;
            Button classButton = CreateButton(panel.transform, className, new Vector2(x, 48f), () => SelectMasteryClass(className), new Vector2(205f, 54f));
            archivistClassButtons.Add(classButton);
            archivistClassNames.Add(className);
        }

        archivistUpgradeButton = CreateButton(panel.transform, "", new Vector2(0f, -95f), PurchaseSelectedMastery, new Vector2(830f, 185f));
        archivistUpgradeText = archivistUpgradeButton.GetComponentInChildren<Text>();
        Text help = NewText(panel.transform, "Menu Help", "Spend shards earned from completed expeditions. Upgrades are retained for this play session.", 21, TextAnchor.MiddleCenter, new Color(0.72f, 0.7f, 0.78f));
        SetBox(help.rectTransform, new Vector2(0f, -225f), new Vector2(940f, 45f));
        CreateButton(panel.transform, "BACK", new Vector2(0f, -290f), CloseArchivistMenu, new Vector2(250f, 58f));
        archivistMenu.SetActive(false);
    }

    void SelectMasteryPage(int page)
    {
        selectedMasteryPage = Mathf.Clamp(page, 0, 1);
        RenderArchivistMenu();
    }

    void SelectMasteryClass(string className)
    {
        selectedMasteryClass = className;
        RenderArchivistMenu();
    }

    void PurchaseSelectedMastery()
    {
        if (string.IsNullOrEmpty(selectedMasteryClass)) return;
        if (selectedMasteryPage == 0)
            GameProgression.TryUpgradeWeaponTrait(selectedMasteryClass);
        else
            GameProgression.TryUpgradeSkillTree(selectedMasteryClass);
        RenderArchivistMenu();
    }

    void RenderArchivistMenu()
    {
        if (archivistBalanceText == null) return;
        archivistBalanceText.text = "MEMORY SHARDS  " + GameProgression.MasteryResourceBalance;
        archivistClassText.text = string.IsNullOrEmpty(selectedMasteryClass)
            ? "No unlocked class is available"
            : selectedMasteryClass + (selectedMasteryPage == 0 ? "  /  WEAPON MASTERY" : "  /  CLASS SKILL TREE");

        for (int i = 0; i < archivistClassButtons.Count; i++)
        {
            Text label = archivistClassButtons[i].GetComponentInChildren<Text>();
            label.text = archivistClassNames[i] == selectedMasteryClass
                ? "◆  " + archivistClassNames[i]
                : archivistClassNames[i];
        }

        if (string.IsNullOrEmpty(selectedMasteryClass))
        {
            archivistUpgradeText.text = "UNAVAILABLE\nUnlock a class during an expedition to study its memories.";
            archivistUpgradeButton.interactable = false;
            return;
        }

        bool weaponTrait = selectedMasteryPage == 0;
        int tier = weaponTrait
            ? GameProgression.GetWeaponTraitTier(selectedMasteryClass)
            : GameProgression.GetSkillTreeTier(selectedMasteryClass);
        int cost = weaponTrait
            ? GameProgression.GetWeaponTraitCost(selectedMasteryClass)
            : GameProgression.GetSkillTreeCost(selectedMasteryClass);
        string title = weaponTrait ? "WEAPON DISCIPLINE" : "EMPOWERED SKILLS";
        string effect = weaponTrait
            ? "Reduce this class's skill cooldowns by 6% per tier."
            : "Increase this class's skill damage by 10% per tier.";

        if (cost < 0)
        {
            archivistUpgradeText.text = title + "  •  MAX RANK\n" + effect + "\nCurrent rank: " + tier + "/" + GameProgression.MaximumMasteryTier;
            archivistUpgradeButton.interactable = false;
        }
        else
        {
            archivistUpgradeText.text = title + "  •  RANK " + tier + "/" + GameProgression.MaximumMasteryTier +
                "\n" + effect + "\n\nUPGRADE  •  " + cost + " MEMORY SHARDS";
            archivistUpgradeButton.interactable = GameProgression.MasteryResourceBalance >= cost;
        }
    }

    void CloseArchivistMenu()
    {
        archivistMenuOpen = false;
        if (archivistMenu != null) archivistMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ShowThoughtBubble(Transform speaker, string message, float duration = 6f)
    {
        if (speaker == null || uiRoot == null) return;

        for (int i = thoughtBubbles.Count - 1; i >= 0; i--)
        {
            ThoughtBubbleEntry previous = thoughtBubbles[i];
            if (previous == null || previous.speaker != speaker) continue;
            if (previous.root != null) Destroy(previous.root);
            thoughtBubbles.RemoveAt(i);
        }

        GameObject bubbleObject = new GameObject("Archivist Thought Bubble");
        bubbleObject.transform.SetParent(uiRoot, false);
        RectTransform bubbleRect = bubbleObject.AddComponent<RectTransform>();
        bubbleRect.anchorMin = bubbleRect.anchorMax = new Vector2(0.5f, 0.5f);
        bubbleRect.pivot = new Vector2(0.5f, 0.5f);
        bubbleRect.sizeDelta = new Vector2(500f, 126f);

        Image background = NewImage(bubbleObject.transform, "Thought Bubble", new Color(0.22f, 0.22f, 0.25f, 0.72f));
        Stretch(background.rectTransform);
        background.raycastTarget = false;
        Outline outline = background.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.06f, 0.06f, 0.08f, 0.88f);
        outline.effectDistance = new Vector2(2f, 2f);

        Text dialogue = NewText(bubbleObject.transform, "Dialogue", message, 26, TextAnchor.MiddleCenter, Color.white);
        dialogue.font = font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        dialogue.fontStyle = FontStyle.Bold;
        dialogue.horizontalOverflow = HorizontalWrapMode.Wrap;
        dialogue.verticalOverflow = VerticalWrapMode.Truncate;
        dialogue.resizeTextForBestFit = true;
        dialogue.resizeTextMinSize = 18;
        dialogue.resizeTextMaxSize = 26;
        SetBox(dialogue.rectTransform, Vector2.zero, new Vector2(452f, 104f));

        Sprite circle = GetThoughtDotSprite();
        AddThoughtDot(bubbleObject.transform, circle, new Vector2(0f, -75f), 25f);
        AddThoughtDot(bubbleObject.transform, circle, new Vector2(27f, -99f), 18f);
        AddThoughtDot(bubbleObject.transform, circle, new Vector2(47f, -116f), 12f);
        thoughtBubbles.Add(new ThoughtBubbleEntry { speaker = speaker, bubble = bubbleRect, root = bubbleObject });
        UpdateThoughtBubblePosition(thoughtBubbles[thoughtBubbles.Count - 1]);
        StartCoroutine(DestroyThoughtBubble(bubbleObject, duration));
    }

    void AddThoughtDot(Transform parent, Sprite circle, Vector2 position, float size)
    {
        Image dot = NewImage(parent, "Thought Dot", new Color(0.22f, 0.22f, 0.25f, 0.72f));
        dot.sprite = circle;
        dot.rectTransform.anchorMin = dot.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        dot.rectTransform.sizeDelta = new Vector2(size, size);
        dot.rectTransform.anchoredPosition = position;
        Outline outline = dot.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.06f, 0.06f, 0.08f, 0.88f);
        outline.effectDistance = new Vector2(1.5f, 1.5f);
    }

    static Sprite GetThoughtDotSprite()
    {
        if (thoughtDotSprite != null) return thoughtDotSprite;

        const int textureSize = 48;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        texture.name = "Thought Bubble Dot Texture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[textureSize * textureSize];
        float center = (textureSize - 1) * 0.5f;
        float radius = textureSize * 0.43f;
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(radius + 0.75f - distance);
                pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        thoughtDotSprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize),
            new Vector2(0.5f, 0.5f), 100f);
        thoughtDotSprite.name = "Thought Bubble Dot";
        return thoughtDotSprite;
    }

    IEnumerator DestroyThoughtBubble(GameObject bubble, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (bubble != null)
        {
            thoughtBubbles.RemoveAll(entry => entry == null || entry.root == bubble);
            Destroy(bubble);
        }
    }

    void UpdateThoughtBubbles()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        for (int i = thoughtBubbles.Count - 1; i >= 0; i--)
        {
            ThoughtBubbleEntry entry = thoughtBubbles[i];
            if (entry == null || entry.root == null || entry.speaker == null)
            {
                thoughtBubbles.RemoveAt(i);
                continue;
            }
            UpdateThoughtBubblePosition(entry);
        }
    }

    void UpdateThoughtBubblePosition(ThoughtBubbleEntry entry)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || entry == null || entry.speaker == null || entry.bubble == null) return;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(entry.speaker.position + Vector3.up * 3.2f);
        bool visible = screenPosition.z > 0f;
        entry.root.SetActive(visible);
        if (!visible) return;

        RectTransform canvasRect = uiRoot as RectTransform;
        if (canvasRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPosition, null, out Vector2 localPosition))
            entry.bubble.anchoredPosition = localPosition + new Vector2(0f, 72f);
    }

    public void ShowExpeditionStatus(string message, float duration = 2.2f)
    {
        if (expeditionStatusText == null) return;
        expeditionStatusText.text = message;
        expeditionStatusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        if (expeditionStatusRoutine != null) StopCoroutine(expeditionStatusRoutine);
        if (!string.IsNullOrEmpty(message))
            expeditionStatusRoutine = StartCoroutine(ClearExpeditionStatus(duration, message));
    }

    IEnumerator ClearExpeditionStatus(float duration, string message)
    {
        yield return new WaitForSeconds(duration);
        if (expeditionStatusText != null && expeditionStatusText.text == message)
        {
            expeditionStatusText.text = "";
            expeditionStatusText.gameObject.SetActive(false);
        }
        expeditionStatusRoutine = null;
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
        bool hasRunLog = subtitle.Contains("Enemies killed:");
        endSub.fontSize = hasRunLog ? 36 : 40;
        SetBox(endSub.rectTransform, hasRunLog ? new Vector2(0f, -135f) : new Vector2(0f, -80f),
            hasRunLog ? new Vector2(1500f, 300f) : new Vector2(1400f, 80f));
    }

    public void ShowExpeditionReport(ExpeditionRunLog runLog, bool archivistRescued)
    {
        if (runLog == null || expeditionReportScreen == null) return;

        expeditionReportTitle.text = "EXPEDITION REPORT — RUN " + runLog.runNumber;
        expeditionReportDetails.text = runLog.ToDisplayString() +
            (archivistRescued
                ? "\nThe Archivist is waiting beside the plaza portal."
                : "\nThe return portal has opened in the village plaza.");
        if (levelUpScreen != null) levelUpScreen.SetActive(false);
        expeditionReportScreen.SetActive(true);
        expeditionReportOpen = true;
        Time.timeScale = 0f;
    }

    void CloseExpeditionReport()
    {
        expeditionReportOpen = false;
        if (expeditionReportScreen != null) expeditionReportScreen.SetActive(false);
        Time.timeScale = 1f;
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
