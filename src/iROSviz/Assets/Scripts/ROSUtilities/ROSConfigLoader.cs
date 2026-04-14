using UnityEngine;

using App.Utilities;

public class ROSConfigLoader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameConfig config = ConfigHelper.GetConfig();
        ConfigHelper.ChangeROSConfig(config);
    }

}
