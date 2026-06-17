using UnityEngine;
using UnityEngine.Localization;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using TMPro;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using App.Utilities;
using App.Exceptions;
using App.ROSUtilities.Subscribers;
using App.ROSUtilities.Resolvers;
using App.ROSUtilities.Helpers;

public class TopicPublish : MonoBehaviour
{
    Dictionary<string, List<string>> _topicsDict;

    private readonly float _defaultFreqValue = 1.0f;

    public TMP_Dropdown topicDropdown;
    public TMP_Dropdown messageTypeDropdown;
    public TMP_InputField messageInput;
    public TMP_InputField frequencyText;
    public TMP_Text buttonText;
    public LocalizedString confirm;
    public LocalizedString stop;

    private bool stopFlag = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating(nameof(GetTopics), 0.0f, 1.0f);
    }

    private void GetTopics()
    {
        _topicsDict = ROSTopicInfoSubscriber.GetTopicsWithTypes();
        
        AddTopicsToDropdown();
        ChangeMessageTypesDropdown();
    }

    private void AddTopicsToDropdown()
    {
        string prevTopic = GetSelectedTopic();
        DropdownHelper.ClearDropdownAndSetOption(
            topicDropdown,
            new List<string>(_topicsDict.Keys),
            GetSelectedTopic
        );
        string currentTopic = GetSelectedTopic();

        if(prevTopic != currentTopic) ResetMessage();
    }
        

    public void ChangeMessageTypesDropdown()
    {
        string prevMessageType = GetSelectedMessage();
        DropdownHelper.ClearDropdownAndSetOption(
            messageTypeDropdown, 
            GetCurrentMessageTypes(),
            GetSelectedMessage
        );
        string currentMessageType = GetSelectedMessage();
        
        if(prevMessageType != currentMessageType) ResetMessage();
    }
        

    public void ResetMessage()
    {
        messageInput.text = string.Empty;
        Type messageType = ROSResolver.GetMessageType(GetSelectedMessage());
        if(messageType == null) return; 
        Message message = (Message)Activator.CreateInstance(messageType);
        messageInput.text = JsonUtility.ToJson(message, false);
    }

    private List<string> GetCurrentMessageTypes()
    {
        try
        {
            string selectedTopic = GetSelectedTopic(); // Get current topic
            return _topicsDict[selectedTopic]; // Get current message types
        }
        catch (KeyNotFoundException)
        {
            return new List<string>();
        }
    }

    public void PublishMessage()
    {
        if (!stopFlag)
        {
            EnableInputs();
            SetButtonLabelToConfirm();
            SetTrueStopFlag();
            return;
        }

        DisableInputs();
        SetButtonLabelToStop();
        SetFalseStopFlag();
        StartCoroutine(PublishLoop()); 
    }

    public void IncrementFrequency()
    {
        if (!frequencyText.interactable) return;

        BoundariesHelper.IncrementUFloatField(
            frequencyText,
            GetFrequency
        );
    }  

    public void DecrementFrequency()
    {
        if(!frequencyText.interactable) return;

        BoundariesHelper.DecrementUFloatField(
            frequencyText,
            GetFrequency
        );
    }

    public void CheckFrequency() =>
        BoundariesHelper.CheckUFloatInputTextBoundaries(
            frequencyText, 
            _defaultFreqValue,
            GetFrequency
        );

    public void ClearMessage() => 
        messageInput.text = string.Empty;

    private int GetSelectedTopicValue() =>
        DropdownHelper.GetDropdownSelectedValue(topicDropdown);

    private string GetSelectedTopic() =>
        DropdownHelper.GetDropdownSelectedText(topicDropdown);

    private int GetSelectedMessageValue() =>
        DropdownHelper.GetDropdownSelectedValue(messageTypeDropdown);

    private string GetSelectedMessage() =>
        DropdownHelper.GetDropdownSelectedText(messageTypeDropdown);

    private string GetMessage() =>
        messageInput.text;
    
    private string GetFrequencyText() =>
        frequencyText.text;
    
    private float GetFrequency() =>
        CastHelper.CastStringToFloat(GetFrequencyText());

    private void SetButtonLabelToStop() =>
        buttonText.text = stop.GetLocalizedString();
    
    private void SetButtonLabelToConfirm() =>
        buttonText.text = confirm.GetLocalizedString();

    private IEnumerator PublishLoop()
    {
        float waitTime = 1f / GetFrequency();

        while (!stopFlag)
        {
            ROSPublisherHelper.PublishMessage(
                GetSelectedTopic(), 
                GetSelectedMessage(),
                GetMessage()
            );
            yield return new WaitForSeconds(waitTime);
        }

        SetTrueStopFlag();
    }

    private void SetTrueStopFlag() =>
        stopFlag = true;

    private void SetFalseStopFlag() =>
        stopFlag = false;

    private void DisableInputs()
    {
        topicDropdown.interactable = false;
        messageTypeDropdown.interactable = false;
        messageInput.interactable = false;
        frequencyText.interactable = false;
    }

    private void EnableInputs()
    {
        topicDropdown.interactable = true;
        messageTypeDropdown.interactable = true;
        messageInput.interactable = true;
        frequencyText.interactable = true;
    }
}
