using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

using Unity.Robotics.ROSTCPConnector;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace App.Utilities
{
    public static class ConfigHelper
    {
        private static readonly string _configPath = Path.Combine(Application.persistentDataPath, "config.json");
        public static GameConfig GetConfig()
        {
            try
            {
                if (!File.Exists(_configPath)) return new();

                string json = File.ReadAllText(_configPath);
                return JsonUtility.FromJson<GameConfig>(json);
            } catch (FileNotFoundException)
            {
                Debug.LogWarning("Error when deserializing");
                return new();
            }
        }

        public static void SaveConfig(GameConfig config)
        {
            string json = JsonUtility.ToJson(config, true);
            File.WriteAllText(_configPath, json);

            ChangeLocale(config);
            ChangeFiduciarySys(config);
            ChangeROSConfig(config);
        }

        public static void ChangeLocale(GameConfig config)
        {
            List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
            Locale locale = locales.
                Where(l => l.ToString() == config.LANG).
                FirstOrDefault();

            if(locale == null) return;

            LocalizationSettings.SelectedLocale = locale;
        }

        public static void ChangeFiduciarySys(GameConfig config) =>
            AprilTagWrapper.familyTag = 
                (AprilTagFamilies)Enum.Parse(typeof(AprilTagFamilies), config.FIDUSYS);

        public static void ChangeROSConfig(GameConfig config)
        {
            ROSConnection _ros = ROSConnection.GetOrCreateInstance();

            _ros.Disconnect();

            _ros.RosIPAddress = config.ROS_IP;
            _ros.RosPort = config.ROS_PORT;
            _ros.ConnectOnStart = config.ROS_COS;
            _ros.KeepaliveTime = config.ROS_KAT;
            _ros.NetworkTimeoutSeconds = config.ROS_NTS;
            _ros.SleepTimeSeconds = config.ROS_STS;
            _ros.ShowHud = config.ROS_HUD; 
            _ros.listenForTFMessages = config.ROS_TF; 
            _ros.TFTopics = config.ROS_TF_TOPICS.ToArray();

            _ros.TryGetComponent<HudPanel>(out var hud);
            if(hud != null) hud.enabled = config.ROS_HUD;

            if(!config.ROS_HUD) GameObject.Destroy(_ros.HUDPanel);
            if(config.ROS_HUD && _ros.gameObject.GetComponent<HudPanel>() == null) 
                _ros.gameObject.AddComponent<HudPanel>();

            _ros.Connect();
        }
    } 

    [System.Serializable]
    public class GameConfig
    {
        public string LANG = "English (en)";
        public string FIDUSYS = "tag36h11";
        public string ROS_IP = "127.0.0.1";
        public int ROS_PORT = 10000;
        public bool ROS_COS = true;
        public float ROS_KAT = 1.0f;
        public float ROS_NTS = 2.0f;
        public float ROS_STS = 0.01f;
        public bool ROS_HUD = false;
        public bool ROS_TF = true;
        public List<string> ROS_TF_TOPICS = new();
    }
}

