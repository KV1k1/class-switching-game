
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    public PlayerLoadout player;

    class FrameUI
    {
        public Text cooldownText;
        public Text keyText;
        public RectTransform cooldownOverlay;
        public Image iconImg;
        public Outline comboGlow;
    }

    FrameUI skill1, skill2, skill3, classFrame;
    Font font;

    void Start()
    {
        font = Font.CreateDynamicFontFromOSFont("Arial", 16);

        GameObject canvasObj = new GameObject("HUD Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        Transform root = canvasObj.transform;

        skill1 = CreateFrame(root, new Vector2(0.5f, 0f),
            new Vector2(-150, 90), new Vector2(135, 135), "1");

        skill2 = CreateFrame(root, new Vector2(0.5f, 0f),
            new Vector2(0, 90), new Vector2(135, 135), "2");

        skill3 = CreateFrame(root, new Vector2(0.5f, 0f),
            new Vector2(150, 90), new Vector2(135, 135), "3");

        classFrame = CreateFrame(root, new Vector2(0f, 0f),
            new Vector2(120, 90), new Vector2(135, 135), "Q");
    }

    void Update()
    {
        ClassData c = player.CurrentClass;

        UpdateFrameData(skill1, c.skill1);
        UpdateFrameData(skill2, c.skill2);
        UpdateFrameData(skill3, c.skill3);

        if (c.classIcon != null)
        {
            classFrame.iconImg.sprite = c.classIcon;
            classFrame.iconImg.color = Color.white;
        }
        else
        {
            classFrame.iconImg.color = c.color;
        }

        UpdateCooldown(skill1, 0);
        UpdateCooldown(skill2, 1);
        UpdateCooldown(skill3, 2);

        UpdateComboGlow();
    }

    void UpdateFrameData(FrameUI f, Skill s)
    {
        if (s != null && s.icon != null)
        {
            f.iconImg.sprite = s.icon;
            f.iconImg.color = Color.white;
        }
        else
        {
            f.iconImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        }
    }

    void UpdateCooldown(FrameUI f, int slot)
    {
        float remaining = player.GetCooldownRemaining(slot);
        float duration = player.GetCooldownDuration(slot);

        float fraction = duration > 0f
            ? Mathf.Clamp01(remaining / duration) : 0f;

        f.cooldownOverlay.anchorMax = new Vector2(1f, fraction);

        if (remaining > 0f)
        {
            f.cooldownText.text = remaining.ToString("0.0");
            f.iconImg.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        }
        else
        {
            f.cooldownText.text = "";
            f.iconImg.color = Color.white;
        }
    }

    void UpdateComboGlow()
    {
        if (player.comboWindowOpen)
        {
            if (classFrame.comboGlow == null)
                classFrame.comboGlow =
                    classFrame.iconImg.gameObject.AddComponent<Outline>();

            classFrame.comboGlow.enabled = true;

            float pulse = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f;

            classFrame.comboGlow.effectColor =
                Color.Lerp(Color.red, Color.yellow, pulse);

            classFrame.comboGlow.effectDistance =
                Vector2.one * Mathf.Lerp(4f, 10f, pulse);
        }
        else if (classFrame.comboGlow != null)
        {
            classFrame.comboGlow.enabled = false;
        }
    }

    FrameUI CreateFrame(
        Transform parent,
        Vector2 anchor,
        Vector2 position,
        Vector2 size,
        string key
    )
    {
        FrameUI ui = new FrameUI();

        GameObject frame = new GameObject("Frame");
        frame.transform.SetParent(parent, false);

        RectTransform rt = frame.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(frame.transform, false);

        ui.iconImg = iconObj.AddComponent<Image>();
        ui.iconImg.color = new Color(1, 1, 1, 0);
        ui.iconImg.preserveAspect = false;

        RectTransform iconRt = ui.iconImg.rectTransform;
        iconRt.anchorMin = Vector2.zero;
        iconRt.anchorMax = Vector2.one;
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;

        // Cooldown Overlay
        GameObject overlay = new GameObject("Cooldown Overlay");
        overlay.transform.SetParent(frame.transform, false);

        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.75f);

        ui.cooldownOverlay = overlay.GetComponent<RectTransform>();
        ui.cooldownOverlay.anchorMin = Vector2.zero;
        ui.cooldownOverlay.anchorMax = new Vector2(1f, 0f);
        ui.cooldownOverlay.offsetMin = Vector2.zero;
        ui.cooldownOverlay.offsetMax = Vector2.zero;

        // Cooldown Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(frame.transform, false);

        ui.cooldownText = textObj.AddComponent<Text>();
        ui.cooldownText.font = font;
        ui.cooldownText.fontSize = 40;
        ui.cooldownText.alignment = TextAnchor.MiddleCenter;
        ui.cooldownText.color = Color.white;

        Outline textShadow = textObj.AddComponent<Outline>();
        textShadow.effectColor = Color.black;
        textShadow.effectDistance = new Vector2(2, -2);

        RectTransform textRt = ui.cooldownText.rectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        // KEY BINDING LABEL
        GameObject keyObj = new GameObject("Key Binding");
        keyObj.transform.SetParent(frame.transform, false);

        ui.keyText = keyObj.AddComponent<Text>();
        ui.keyText.font = font;
        ui.keyText.fontSize = 26;
        ui.keyText.fontStyle = FontStyle.Bold;
        ui.keyText.alignment = TextAnchor.MiddleCenter;
        ui.keyText.color = Color.white;
        ui.keyText.text = key;

        Outline keyShadow = keyObj.AddComponent<Outline>();
        keyShadow.effectColor = Color.black;
        keyShadow.effectDistance = new Vector2(2, -2);

        RectTransform keyRt = ui.keyText.rectTransform;
        keyRt.anchorMin = new Vector2(1f, 1f);
        keyRt.anchorMax = new Vector2(1f, 1f);
        keyRt.pivot = new Vector2(1f, 1f);
        keyRt.anchoredPosition = new Vector2(-5f, -5f);
        keyRt.sizeDelta = new Vector2(40f, 35f);

        return ui;
    }
}
