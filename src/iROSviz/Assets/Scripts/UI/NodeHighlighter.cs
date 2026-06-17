using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using App.Utilities;
using App.ROSUtilities.Subscribers;
using System.Threading.Tasks;

public class NodeHighlighter : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public GameObject TFRootGO;

    private static string selectedNode = string.Empty;
    private static int selectedNodeID = -1;
    private float lastClickTime = 0f;
    private Vector2 lastClickPosition;
    private const float clickTimeThreshold = 0.5f; // Maximum time between clicks to be considered as a double click
    private const float clickDistance = 20f; // Maximum distance between clicks to be considered as a double click
    private List<string> frames = new();

    void Start() =>
        StartCoroutine(RefreshDropdown());

    void Update()
    {
        PerformAutoSelectionBasedOnTagsInCamera();
        PerformClickIfHappened();
        RefreshSelectedTagInfo();
    }

    private void PerformClickIfHappened()
    {
        if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame) return;
        
        float distanceSinceLastClick = 
            Vector2.Distance(lastClickPosition, Pointer.current.position.ReadValue());
        lastClickPosition = Pointer.current.position.ReadValue();

        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick <= clickTimeThreshold && 
            distanceSinceLastClick <= clickDistance)
            OnDoubleClick();

        lastClickTime = Time.time;
    }

    private void PerformAutoSelectionBasedOnTagsInCamera()
    {
        if(FiducialSystemFrameProcessor.lastResult == null) return;

        List<DetectionResult.ApriltagDetection> detectedTags = 
            FiducialSystemFrameProcessor.lastResult.ids;
        if (detectedTags == null || detectedTags.Count < 1) return;

        bool tagExists = TagDatabase.TryGetTagName(detectedTags[0].id, out string tagName);
        if(!tagExists) return;

        DropdownHelper.SetDropdownOption(dropdown, tagName);
    }

    private IEnumerator RefreshDropdown()
    {
        while (true)
        {
            RefreshFrames();
            DropdownHelper.ClearDropdownAndSetOption(
                dropdown, 
                frames, 
                GetDropdownSelectedTag
            );
            ROSRobotInfoSubscriber.NotifyReading();

            yield return new WaitForSeconds(.5f);
        }
    }

    private void RefreshFrames()
    {
        this.frames.Clear();
        if(TFRootGO == null) return;

        TFRoot tfRoot = TFRootGO.GetComponent<TFRoot>();
        Dictionary<string, Dictionary<string, TFRoot.FrameNode>> topicFrames = tfRoot.GetFramesByTopic();
        foreach (var (topic, frames) in topicFrames)
            this.frames.AddRange(frames.Select(kvp => $"{topic}: {kvp.Key}"));
    }

    private void OnDoubleClick()
    {
        DetectionResult.ApriltagDetection? selectedTag = null;
        float minDistance = float.MaxValue;
        foreach(DetectionResult.ApriltagDetection tag in 
            FiducialSystemFrameProcessor.lastResult.ids)
        {
            Vector2 tagPosition = new Vector2((float)tag.cx, (float)tag.cy);
            float distance = Vector2.Distance(lastClickPosition, tagPosition);
            if(distance >= minDistance) continue;
            
            selectedTag = tag;
            minDistance = distance;
        }

        if(selectedTag == null) return;
        selectedNodeID = selectedTag?.id ?? -1;
        if(selectedNodeID == -1) return;
        selectedNode = TagDatabase.TryGetTagName(
                selectedNodeID, 
                out string name
            ) ? name : $"Unknown Tag {selectedNodeID}";

        DropdownHelper.SetDropdownOption(dropdown, selectedNode);
        RefreshSelectedTagInfo();
    }

    private string GetDropdownSelectedTag() =>
        DropdownHelper.GetDropdownSelectedText(dropdown);

    public static string GetSelectedTag() =>
        selectedNode;
    
    public static int GetSelectedTagID() =>
        selectedNodeID;
    
    public void OnDropdownValueChanged() => RefreshSelectedTagInfo();

    private void RefreshSelectedTagInfo()
    {
        string selectedTag = GetDropdownSelectedTag();
        if(!TagDatabase.TryGetTagId(selectedTag, out int id)) return;
        
        selectedNodeID = id;
        selectedNode = selectedTag;
    }

    public void DeleteNodePairing()
    {
        if(selectedNodeID == -1) return;

        TagDatabase.DeleteTag(selectedNodeID);
        TagDatabase.SaveDatabase();

        dropdown.value = 0;
    }
    
}
