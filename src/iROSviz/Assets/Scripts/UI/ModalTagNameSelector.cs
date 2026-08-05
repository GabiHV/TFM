using UnityEngine;
using TMPro;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App.Utilities;
using App.ROSUtilities.Subscribers;

public class ModalTagNameSelector : MonoBehaviour
{
    private static ModalTagNameSelector _instance;
    private static List<string> tagNames = new();
    private static List<string> pathsTopics = new();

    public TMP_Dropdown namespaceDropdown;
    public TMP_Dropdown pathVisualizerDropdown;
    private bool nodesAreBeingRetrieved = false;
    public TFRoot TFRootGO;

    private enum Result {None, Ok};
    private Result result;

    public static IEnumerator ShowDialog()
    {
        if(_instance == null) yield break;
        _instance.ShowWindow();

        if(!_instance.nodesAreBeingRetrieved)
        {
            _instance.nodesAreBeingRetrieved = true;
            _instance.InvokeRepeating(nameof(_instance.RefreshTagNames), 0f, 5f);
            _instance.InvokeRepeating(nameof(_instance.RefreshPathVisualizer), 0f, 5f);
        }

        yield return new WaitWhile(() => _instance.result == Result.None);

        _instance.DismissWindow();
    }

    public static void DismissDialog()
    {
        if(_instance == null) return;
        _instance.DismissWindow();
    }

    public static string GetTagName() =>
        _instance == null ? string.Empty : _instance.GetSelectedTagName();

    public static string GetFrame()
    {
        if(_instance == null) return string.Empty;
        try
        {
            return _instance.GetSelectedTagName().Split(":")[1].Replace(" ", "");
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string GetPathVisualization() =>
        _instance == null ? string.Empty : _instance.GetSelectedPathTopic();

    void Awake()
    {
        _instance = this;
        gameObject.SetActive(false);
    }

    private void RefreshTagNames()
    {
        tagNames.Clear();
        Debug.LogWarning($"Clearing frames");

        TFRoot tfRoot = TFRootGO.GetComponent<TFRoot>();
        Dictionary<string, Dictionary<string, TFRoot.FrameNode>> topicFrames = tfRoot.GetFramesByTopic();
        foreach (var (topic, frames) in topicFrames)
        {
            Debug.LogWarning($"Adding {topic} frames");
            tagNames.AddRange(frames.Select(kvp => $"{topic}: {kvp.Key}"));
        }
        RefreshTagDropdown();
    }

    private void RefreshPathVisualizer()
    {
        pathsTopics = 
            ROSTopicInfoSubscriber.
            GetTopicsByType("nav_msgs/Path");
        
        RefreshPathVisualizationDropdown();
    }

    private void RefreshTagDropdown() =>
        DropdownHelper.ClearDropdownAndSetOption(
            namespaceDropdown,
            tagNames,
            GetSelectedTagName
        );

    private void RefreshPathVisualizationDropdown() =>
        DropdownHelper.ClearDropdownAndSetOption(
            pathVisualizerDropdown, 
            pathsTopics,
            GetSelectedPathTopic 
        );

    private void ShowWindow() =>
        this.gameObject.SetActive(true);

    private void DismissWindow()
    {
        CancelInvoke(nameof(RefreshTagNames));

        this.result = Result.None;

        this.gameObject.SetActive(false);
        this.nodesAreBeingRetrieved = false;
    }

    private string GetSelectedTagName() =>
        DropdownHelper.GetDropdownSelectedText(namespaceDropdown);  

    private string GetSelectedPathTopic() =>
        DropdownHelper.GetDropdownSelectedText(pathVisualizerDropdown);

    private int GetTagId() =>
        namespaceDropdown.value;

    public void Confirm() =>
        this.result = Result.Ok;

    public void OnFrameChange() =>
        PredictPathVisualizer();
    
    private void PredictPathVisualizer()
    {
        string predictedPath = pathsTopics.OrderBy(
            t => OutputHelper.PathSimilarity(GetTopic(), t)
        ).FirstOrDefault();
        DropdownHelper.SetDropdownOption(pathVisualizerDropdown, predictedPath);
    }

    private string GetTopic()
    {
        if(GetSelectedTagName().Split(":").Length <= 0) return string.Empty;    
        return GetSelectedTagName().Split(":")[0].Replace(" ", "");
    }

}