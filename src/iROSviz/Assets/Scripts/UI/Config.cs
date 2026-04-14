using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

using App.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class Config : MonoBehaviour
{
    public TMP_Dropdown languageDropdown;
    public TMP_Dropdown fiducSysDropdown;

    public TMP_InputField ipInput;
    public TMP_InputField portInput;
    public TMP_InputField keepaliveInput;
    public TMP_InputField networkTMInput;
    public TMP_InputField sleepTimeInput;
    public GameObject topicScrollViewContent;
    public GameObject topicInputPrefab;

    public Toggle conectOnStart;
    public Toggle showHUD;
    public Toggle listenTFMessages;

    private static GameConfig _config;
    private bool flagCOS;
    private bool flagHUD;
    private bool flagTF;

    private List<GameObject> topics = new();
    private int topicCount = 0;

    private static readonly int _portLimit = 65535;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Load current config
        LoadConfig();

        // Load dropdowns
        LocalizationSettings.InitializationOperation.Completed += 
            (handle) => LoadAvailableLanguages();
        LoadAvailableFiduciarySys();
    }

    private void LoadConfig()
    {   
        LoadConfigObject();

        LoadCurrentIP();
        LoadCurrentPort();
        LoadCurrentCOS();
        LoadCurrentKAT();
        LoadCurrentNTS();
        LoadCurrentSTS();
        LoadCurrentShowHUD();
        LoadCurrentListenTF();
        LoadCurrentTFTopics();
    }

    private void LoadConfigObject() => 
        _config = ConfigHelper.GetConfig();

    private void LoadCurrentPort() =>
        portInput.text = _config.ROS_PORT.ToString();        
    
    private void LoadCurrentIP() =>
        ipInput.text = _config.ROS_IP;  

    private void LoadCurrentCOS()
    {
        flagCOS = _config.ROS_COS;
        conectOnStart.SetIsOnWithoutNotify(_config.ROS_COS);              
    }

    private void LoadCurrentKAT() =>
        keepaliveInput.text = _config.ROS_KAT.ToString();

    private void LoadCurrentNTS() =>
        networkTMInput.text = _config.ROS_NTS.ToString();

    private void LoadCurrentSTS() =>
        sleepTimeInput.text = _config.ROS_STS.ToString();

    private void LoadCurrentShowHUD()
    {
        flagHUD = _config.ROS_HUD;
        showHUD.SetIsOnWithoutNotify(_config.ROS_HUD);
    }

    private void LoadCurrentListenTF()
    {
        flagTF = _config.ROS_TF;
        listenTFMessages.SetIsOnWithoutNotify(_config.ROS_TF);        
    }
    
    private void LoadCurrentTFTopics()
    {
        foreach(string topic in _config.ROS_TF_TOPICS)
            AddTopicInput(topic);
    }

    private void LoadAvailableLanguages()
    {
        if(!LocalizationSettings.InitializationOperation.IsDone) return;
        List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;

        DropdownHelper.ClearDropdownAndSetOption(languageDropdown, locales, () => _config.LANG);
    }

    private int GetSelectedLanguage() =>
        DropdownHelper.GetDropdownSelectedValue(languageDropdown);
    
    private string GetSelectedLanguageText() =>
        DropdownHelper.GetDropdownSelectedText(languageDropdown);

    private string GetSelectedFiduciarySysText() =>
        DropdownHelper.GetDropdownSelectedText(fiducSysDropdown);
    
    private void LoadAvailableFiduciarySys()
    {
        List<string> systems = Enum.GetNames(typeof(AprilTagFamilies)).ToList();
        DropdownHelper.ClearDropdownAndSetOption(fiducSysDropdown, systems, () => _config.FIDUSYS);
    }

    private bool ValidIP()
    {
        string ipPattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        Regex regex = new Regex(ipPattern);
        return regex.IsMatch(ipInput.text);
    }

    private bool ValidPort()
    {
        int port = GetPort();
        return port > 0 && port <= _portLimit;
    }

    public void CheckIP()
    {
        if(!ValidIP()) ipInput.text = _config.ROS_IP;
    }
        
    public void CheckPort() =>
        BoundariesHelper.CheckCustomNumberInputTextBoundaries(
            portInput, 
            _config.ROS_PORT,
            0,
            _portLimit, 
            GetPort
        ); 

    public void CheckKAT() =>
        BoundariesHelper.CheckUFloatInputTextBoundaries(
            keepaliveInput,
            _config.ROS_KAT,
            GetKAT
        );
     
    public void CheckSTS() =>
        BoundariesHelper.CheckUFloatInputTextBoundaries(
            sleepTimeInput,
            _config.ROS_STS,
            GetSTS
        );

    public void CheckNTS() =>
        BoundariesHelper.CheckUFloatInputTextBoundaries(
            networkTMInput,
            _config.ROS_NTS,
            GetNTS
        );

    public void SwitchCOS() =>
        flagCOS = !flagCOS;
    
    public void SwitchHUD() =>
        flagHUD = !flagHUD;
    
    public void SwitchTF() =>
        flagTF = !flagTF;

    private string GetIP() =>
        ipInput.text;
    
    private int GetPort() =>
        CastHelper.CastStringToInt(portInput.text);

    private float GetKAT() =>
        CastHelper.CastStringToFloat(keepaliveInput.text);

    private float GetSTS() =>
        CastHelper.CastStringToFloat(sleepTimeInput.text);

    private float GetNTS() =>
        CastHelper.CastStringToFloat(networkTMInput.text);

    private bool GetCOS() =>
        flagCOS;
    
    private bool GetHUD() =>
        flagHUD;

    private bool GetTF() =>
        flagTF;
    
    private List<string> GetTFTopics()
    {
        List<string> topicList = new();
        foreach(GameObject topicItem in topics)
        {
            Transform textTransform = topicItem.transform.Find("Panel/InputField");

            if(textTransform == null) continue;

            TMP_InputField input = textTransform.gameObject.GetComponent<TMP_InputField>();
            string value = input.text;
            if(string.IsNullOrEmpty(value)) continue;

            topicList.Add(value);
        }
        return topicList;
    }

    public void SaveConfig()
    {
        GameConfig config = new GameConfig
        {
            LANG = GetSelectedLanguageText(),
            FIDUSYS = GetSelectedFiduciarySysText(),
            ROS_IP = GetIP(),
            ROS_PORT = GetPort(),
            ROS_COS = GetCOS(),
            ROS_KAT = GetKAT(),
            ROS_NTS = GetNTS(),
            ROS_STS = GetSTS(),
            ROS_HUD = GetHUD(),
            ROS_TF = GetTF(),
            ROS_TF_TOPICS = GetTFTopics(),
        };
        ConfigHelper.SaveConfig(config);
        _config = config;
    }

    public void AddNewTopicInput() =>
        AddTopicInput(string.Empty);

    private void AddTopicInput(string topic)
    {
        GameObject topicInput = (GameObject)Instantiate(topicInputPrefab);
        topicInput.transform.SetParent(topicScrollViewContent.transform);
        topics.Add(topicInput);

        // Change topic string
        Transform textTransform = topicInput.transform.Find("Panel/TopicText");
        if(textTransform == null) return;
        TMP_Text topicText = textTransform.gameObject.GetComponent<TMP_Text>();
        topicText.text = $"Topic {topicCount++}";

        // Change topic input field value
        if(string.IsNullOrEmpty(topic)) return;

        textTransform = topicInput.transform.Find("Panel/InputField");
        if(textTransform == null) return;
        TMP_InputField input = textTransform.gameObject.GetComponent<TMP_InputField>();
        input.text = topic;
    }

    public void DelTopicInput()
    {
        if(topics.Count() <= 0) return;

        GameObject lastTopic = topics.Last();
        topics.RemoveAt(topics.Count() - 1); // Remove last topic from list
        topicCount--;
        Destroy(lastTopic);
    }

    public void CloseConfig() =>
        this.gameObject.SetActive(false);
    
} 


