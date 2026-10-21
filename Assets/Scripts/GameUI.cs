using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        Transform root = canvasObj.transform;

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

        playerHealth.Damaged += OnPlayerDamaged;
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
        float fraction = Mathf.Clamp01(playerHealth.Current / playerHealth.maxHealth);
        healthFill.anchorMax = new Vector2(fraction, 1f);
        healthText.text = "HP  " + Mathf.CeilToInt(playerHealth.Current) + " / " + Mathf.CeilToInt(playerHealth.maxHealth);

        if (bossHealth != null)
            bossFill.anchorMax = new Vector2(Mathf.Clamp01(bossHealth.Current / bossHealth.maxHealth), 1f);

        flashAlpha = Mathf.MoveTowards(flashAlpha, 0f, Time.unscaledDeltaTime * 1.5f);
        flash.color = new Color(1f, 0f, 0f, flashAlpha);

        if (ended && Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowBossBar(string bossName, Health boss)
    {
        bossHealth = boss;
        bossNameText.text = bossName;
        bossBar.SetActive(true);
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