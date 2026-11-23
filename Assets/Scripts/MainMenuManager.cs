using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    Transform menuRoot;
    GameObject settingsPanel;
    TMP_Text currentSchemeText;

    void Start()
    {
        BuildSettingsMenu();
    }

    void Update()
    {
        if (currentSchemeText != null)
            currentSchemeText.text = "Current input: " + SchemeName(InputSchemeSettings.Current);
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("HubScene");
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit!");
        Application.Quit();
    }

    void BuildSettingsMenu()
    {
        GameObject playButton = GameObject.Find("PlayButton");
        GameObject quitButton = GameObject.Find("QuitButton");
        if (playButton == null || quitButton == null)
        {
            Debug.LogError("Main menu PlayButton or QuitButton was not found.");
            return;
        }

        menuRoot = playButton.transform.parent;
        RectTransform playRect = playButton.GetComponent<RectTransform>();
        RectTransform quitRect = quitButton.GetComponent<RectTransform>();
        const float buttonSpacing = 98f;
        Vector2 playPosition = playRect.anchoredPosition;

        // Normalize the existing Quit button to the Play button's bounds and
        // place all three menu actions on the same vertical centerline.
        quitRect.anchorMin = playRect.anchorMin;
        quitRect.anchorMax = playRect.anchorMax;
        quitRect.pivot = playRect.pivot;
        quitRect.sizeDelta = playRect.sizeDelta;
        quitRect.localScale = playRect.localScale;
        quitRect.anchoredPosition = playPosition + Vector2.down * (buttonSpacing * 2f);

        TMP_Text labelTemplate = playButton.GetComponentInChildren<TMP_Text>();
        if (labelTemplate == null)
        {
            Debug.LogError("The main menu buttons need a TextMeshPro label to build Settings.");
            return;
        }

        labelTemplate.text = "Play";
        TMP_Text quitLabel = quitButton.GetComponentInChildren<TMP_Text>();
        if (quitLabel != null)
        {
            quitLabel.text = "Quit";
            RectTransform playLabelRect = labelTemplate.rectTransform;
            RectTransform quitLabelRect = quitLabel.rectTransform;
            quitLabelRect.anchorMin = playLabelRect.anchorMin;
            quitLabelRect.anchorMax = playLabelRect.anchorMax;
            quitLabelRect.pivot = playLabelRect.pivot;
            quitLabelRect.sizeDelta = playLabelRect.sizeDelta;
            quitLabelRect.anchoredPosition = playLabelRect.anchoredPosition;
            quitLabelRect.localScale = playLabelRect.localScale;
            quitLabelRect.localRotation = playLabelRect.localRotation;
        }

        Button settingsButton = CloneStyledButton(playButton, menuRoot, "SettingsButton", "Settings");
        RectTransform settingsRect = settingsButton.GetComponent<RectTransform>();
        settingsRect.anchoredPosition = playPosition + Vector2.down * buttonSpacing;
        settingsButton.onClick.AddListener(OpenSettings);

        settingsPanel = new GameObject("Settings Window", typeof(RectTransform), typeof(Image));
        settingsPanel.transform.SetParent(menuRoot, false);
        RectTransform panelRect = settingsPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image background = settingsPanel.GetComponent<Image>();
        background.color = new Color(0.035f, 0.025f, 0.055f, 0.97f);
        background.raycastTarget = true;

        CreateText(labelTemplate, settingsPanel.transform, "SETTINGS", new Vector2(0f, 285f), 52f, new Vector2(900f, 100f));
        currentSchemeText = CreateText(labelTemplate, settingsPanel.transform, "", new Vector2(0f, 180f), 30f, new Vector2(900f, 70f));

        Button clickButton = CloneStyledButton(playButton, settingsPanel.transform, "ClickToMoveButton", "Click-to-Move");
        clickButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 70f);
        clickButton.onClick.AddListener(() => InputSchemeSettings.Set(InputScheme.ClickToMove));

        Button wasdButton = CloneStyledButton(playButton, settingsPanel.transform, "WASDButton", "WASD");
        wasdButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -35f);
        wasdButton.onClick.AddListener(() => InputSchemeSettings.Set(InputScheme.Wasd));

        Button backButton = CloneStyledButton(playButton, settingsPanel.transform, "BackButton", "Back");
        backButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -185f);
        backButton.onClick.AddListener(CloseSettings);

        settingsPanel.SetActive(false);
    }

    Button CloneStyledButton(GameObject template, Transform parent, string objectName, string label)
    {
        GameObject buttonObject = Instantiate(template, parent, false);
        buttonObject.name = objectName;
        Button button = buttonObject.GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent();
        TMP_Text buttonLabel = buttonObject.GetComponentInChildren<TMP_Text>();
        if (buttonLabel != null) buttonLabel.text = label;
        return button;
    }

    TMP_Text CreateText(TMP_Text template, Transform parent, string value, Vector2 position, float fontSize, Vector2 dimensions)
    {
        TMP_Text text = Instantiate(template, parent, false);
        text.name = value + " Text";
        text.text = value;
        text.fontSize = fontSize;
        text.raycastTarget = false;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        rect.localScale = Vector3.one;
        return text;
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    static string SchemeName(InputScheme scheme)
    {
        return scheme == InputScheme.Wasd ? "WASD" : "Click-to-Move";
    }

}
