using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using TMPro;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security;

using App.Utilities;
using App.ROSUtilities.Subscribers;
using App.ROSUtilities.Resolvers;

public class TopicEcho : MonoBehaviour
{
    public GameObject scrollViewContent;
    public TMP_Dropdown topicDropdown;
    public TMP_Dropdown messageTypeDropdown;
    public TextMeshProUGUI templateText;

    private string subscribedTopic;
    Dictionary<string, List<string>> typeDict = new();
    HashSet<string> subscribedTopics = new();
    ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        Debug.Log("ROS instance obtained");

        InvokeRepeating(nameof(GetTopics), 0.0f, 1.0f);
    }

    void GetTopics()
    {
        typeDict = ROSTopicInfoSubscriber.GetTopicsWithTypes();
        if (typeDict == null || typeDict.Count <= 0)
        {
            Debug.LogWarning("Service didn't respond or no topics available");
            return;
        }

        AddTopicsToDropdown();
        ChangeMessageTypesDropdown();
    }


    void AddTopicsToDropdown() =>
        DropdownHelper.ClearDropdownAndSetOption(
            topicDropdown,
            new List<string>(typeDict.Keys),
            GetSelectedTopic
        );

    public void ChangeMessageTypesDropdown() =>
        DropdownHelper.ClearDropdownAndSetOption(
            messageTypeDropdown, 
            GetCurrentMessageTypes(),
            GetSelectedMessageType
        );

    private List<string> GetCurrentMessageTypes()
    {
        try
        {
            string selectedTopic = GetSelectedTopic(); // Get current topic
            return typeDict[selectedTopic]; // Get current message types
        }
        catch (KeyNotFoundException)
        {
            return new List<string>();
        }
    }
    
    public void Save()
    {
        Unsave();

        string selectedTopic = GetSelectedTopic();
        subscribedTopic = selectedTopic;
        string completeMessageType = GetSelectedMessageType();
        string messageName = ROSResolver.GetMessageNameWithoutType(completeMessageType);
        
        if(!subscribedTopics.Contains($"{selectedTopic} : {messageName}"))
            ros.SubscribeByMessageName(selectedTopic, messageName, OnMessage(selectedTopic, messageName));
        Debug.Log($"Subscribed to: {selectedTopic}");

        DisableDropdowns();
    }

    public void Unsave()
    {
        // There is a known bug unsubscribing ROS topics with ROS-TCP-Connector
        // Due to this, it is necessary to change method invokation with a flag to stop
        // processing incoming messages
        Debug.Log($"Unsubscribed to: {subscribedTopic}");
        subscribedTopic = string.Empty;
        // ros.Unsubscribe(currentTopic ?? "");

        EnableDropdowns();
    }

    Action<Message> OnMessage(string topicName, string messageType)
    {
        return (Message msg) =>
        {
            if(subscribedTopic != topicName) return;
            subscribedTopics.Add($"{topicName} : {messageType}");
            string retunedMessage = msg.ToString().Replace("\n", " ");
            string completeMsg = $"{DateTime.Now}: {retunedMessage}";
            TextMeshProUGUI text = (TextMeshProUGUI)Instantiate(templateText);
            text.text = completeMsg;
            text.transform.SetParent(scrollViewContent.transform);
            text.color = Color.white;
        };
    } 

    private string GetSelectedTopic() =>
        DropdownHelper.GetDropdownSelectedText(topicDropdown);

    private int GetSelectedMessageTypeValue() =>
        DropdownHelper.GetDropdownSelectedValue(messageTypeDropdown);

    private int GetSelectedTopicValue() =>
        DropdownHelper.GetDropdownSelectedValue(topicDropdown);

    private string GetSelectedMessageType() =>
        DropdownHelper.GetDropdownSelectedText(messageTypeDropdown);

    private void DisableInteractionTopicDropdown() =>
        topicDropdown.interactable = false;

    private void DisableInteractionMessageTypeDropdown() =>
        messageTypeDropdown.interactable = false;

    private void EnableInteractionTopicDropdown() =>
        topicDropdown.interactable = true;

    private void EnableInteractionMessageTypeDropdown() =>
        messageTypeDropdown.interactable = true;

    private void DisableDropdowns()
    {
        DisableInteractionTopicDropdown();
        DisableInteractionMessageTypeDropdown();
    }

    private void EnableDropdowns()
    {
        EnableInteractionTopicDropdown();
        EnableInteractionMessageTypeDropdown();
    }

    public void Close()
    {
        Unsave();
        gameObject.SetActive(false);
    }
}
