using UnityEngine;

public enum InputScheme
{
    ClickToMove = 0,
    Wasd = 1
}

public static class InputSchemeSettings
{
    const string PreferenceKey = "InputScheme";

    public static InputScheme Current => (InputScheme)PlayerPrefs.GetInt(PreferenceKey, (int)InputScheme.ClickToMove);

    public static void Set(InputScheme scheme)
    {
        PlayerPrefs.SetInt(PreferenceKey, (int)scheme);
        PlayerPrefs.Save();
    }
}
