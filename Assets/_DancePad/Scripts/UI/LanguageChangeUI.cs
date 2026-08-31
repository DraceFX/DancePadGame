using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageChangeUI : MonoBehaviour
{
    public void SetLanguage(string language)
    {
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(language);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
        }
    }
}
