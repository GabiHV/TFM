using UnityEngine;

using App.Utilities;

public class LanguageConfigLoader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameConfig config = ConfigHelper.GetConfig();
        ConfigHelper.ChangeLocale(config);
    }
}
