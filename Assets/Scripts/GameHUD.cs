using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    public PlayerLoadout player;

    // Everything we need to control on one frame
    class FrameUI
    {
        public Text main;
        public Text cooldownText;
        public RectTransform cooldownOverlay;
        public Image background;
        public Outline border;
    }

    FrameUI skill1, skill2, skill3, classFrame;
    Font font;

    static readonly Color normalBg = new Color(0f, 0f, 0f, 0.6f);
    static readonly Color glowBg = new Color(0.5f, 0.4f, 0f, 0.85f);

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

        skill1 = CreateFrame(root, new Vector2(0.5f, 0f), new Vector2(-90, 80), new Vector2(160, 100), "1");
        skill2 = CreateFrame(root, new Vector2(0.5f, 0f), new Vector2(90, 80), new Vector2(160, 100), "2");
        skill3 = CreateFrame(root, new Vector2(0.5f, 0f), new Vector2(270, 80), new Vector2(160, 100), "3");
        classFrame = CreateFrame(root, new Vector2(0f, 0f), new Vector2(140, 80), new Vector2(220, 100), "Q");
    }

    void Update()
    {
        ClassData c = player.CurrentClass;

        classFrame.main.text = c.className;
        skill1.main.text = Pretty(c.skill1);
        skill2.main.text = Pretty(c.skill2);
        skill3.main.text = Pretty(c.skill3);

        UpdateCooldown(skill1, 0);
        UpdateCooldown(skill2, 1);
        UpdateCooldown(skill3, 2);
        UpdateComboGlow();
    }

    void UpdateCooldown(FrameUI f, int slot)
    {
        float remaining = player.GetCooldownRemaining(slot);
        float duration = player.GetCooldownDuration(slot);
        float fraction = duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f;

        // The dark overlay is anchored to the bottom; its top edge drops as time runs out
        f.cooldownOverlay.anchorMax = new Vector2(1f, fraction);

        f.cooldownText.text = remaining > 0f ? remaining.ToString("0.0") : "";
        f.main.color = remaining > 0f ? new Color(1f, 1f, 1f, 0.5f) : Color.white;
    }

    void UpdateComboGlow()
    {
        if (player.comboWindowOpen)
        {
            // pulse between 0 and 1, about 1.3 times per second
            float pulse = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f;
            classFrame.border.effectColor = Color.Lerp(Color.white, Color.yellow, pulse);
            classFrame.border.effectDistance = Vector2.one * Mathf.Lerp(3f, 9f, pulse);
            classFrame.background.color = Color.Lerp(normalBg, glowBg, pulse);
        }
        else
        {
            classFrame.border.effectColor = Color.white;
            classFrame.border.effectDistance = new Vector2(3, 3);
            classFrame.background.color = normalBg;
        }
    }

    string Pretty(Skill s)
    {
        if (s == null) return "-";
        return Regex.Replace(s.name, "(?<=[a-z])(?=[A-Z])", " ");
    }

    FrameUI CreateFrame(Transform parent, Vector2 anchor, Vector2 position, Vector2 size, string keyHint)
    {
        FrameUI ui = new FrameUI();

        GameObject frame = new GameObject("Frame " + keyHint);
        frame.transform.SetParent(parent, false);

        ui.background = frame.AddComponent<Image>();
        ui.background.color = normalBg;
        ui.border = frame.AddComponent<Outline>();
        ui.border.effectColor = Color.white;
        ui.border.effectDistance = new Vector2(3, 3);

        RectTransform rt = frame.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        // Cooldown overlay (created before the text so the text draws on top of it)
        GameObject overlay = new GameObject("Cooldown Overlay");
        overlay.transform.SetParent(frame.transform, false);
        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.65f);
        ui.cooldownOverlay = overlay.GetComponent<RectTransform>();
        ui.cooldownOverlay.anchorMin = Vector2.zero;
        ui.cooldownOverlay.anchorMax = new Vector2(1f, 0f);   // starts empty
        ui.cooldownOverlay.offsetMin = Vector2.zero;
        ui.cooldownOverlay.offsetMax = Vector2.zero;

        // Main text (the name)
        ui.main = CreateText(frame.transform, "", 26, TextAnchor.MiddleCenter);
        RectTransform mr = ui.main.rectTransform;
        mr.anchorMin = Vector2.zero;
        mr.anchorMax = Vector2.one;
        mr.offsetMin = new Vector2(6, 6);
        mr.offsetMax = new Vector2(-6, -6);

        // Key hint, top-left corner
        Text hint = CreateText(frame.transform, keyHint, 24, TextAnchor.UpperLeft);
        hint.color = Color.yellow;
        RectTransform hr = hint.rectTransform;
        hr.anchorMin = new Vector2(0, 1);
        hr.anchorMax = new Vector2(0, 1);
        hr.pivot = new Vector2(0, 1);
        hr.anchoredPosition = new Vector2(8, -4);
        hr.sizeDelta = new Vector2(50, 34);

        // Countdown number, bottom-right corner
        ui.cooldownText = CreateText(frame.transform, "", 24, TextAnchor.LowerRight);
        ui.cooldownText.color = Color.cyan;
        RectTransform cr = ui.cooldownText.rectTransform;
        cr.anchorMin = new Vector2(1, 0);
        cr.anchorMax = new Vector2(1, 0);
        cr.pivot = new Vector2(1, 0);
        cr.anchoredPosition = new Vector2(-8, 4);
        cr.sizeDelta = new Vector2(80, 34);

        return ui;
    }

    Text CreateText(Transform parent, string content, int fontSize, TextAnchor alignment)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        Text t = obj.AddComponent<Text>();
        t.font = font;
        t.text = content;
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.color = Color.white;
        return t;
    }
}