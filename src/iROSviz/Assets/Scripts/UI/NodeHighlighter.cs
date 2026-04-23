using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using App.Utilities;

public class NodeHighlighter : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public static string selectedNode = string.Empty;
    public static int selectedNodeID = -1;
    private float lastClickTime = 0f;
    private Vector2 lastClickPosition;
    private const float clickTimeThreshold = 0.5f; // Maximum time between clicks to be considered as a double click
    private const float clickDistance = 20f; // Maximum distance between clicks to be considered as a double click

    void Update()
    {

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            float distanceSinceLastClick = 
                Vector2.Distance(lastClickPosition, Pointer.current.position.ReadValue());
            lastClickPosition = Pointer.current.position.ReadValue();

            float timeSinceLastClick = Time.time - lastClickTime;
            if (timeSinceLastClick <= clickTimeThreshold && 
                distanceSinceLastClick <= clickDistance)
                OnDoubleClick();

            lastClickTime = Time.time;
        }

        if (FiducialSystemFrameProcessor.lastResult == null) return;

        List<DetectionResult.ApriltagDetection> tags = 
            FiducialSystemFrameProcessor.lastResult.ids;
        if(tags == null) return;

        List<string> tagNames = tags.Select(t => 
            TagDatabase.TryGetTagName(t.id, out string name) ? name : $"Unknown Tag {t.id}"
        ).ToList();

        DropdownHelper.ClearDropdownAndSetOption(
            dropdown, 
            tagNames, 
            GetSelectedTag
        );

        RefreshTagInfo();
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
        RefreshTagInfo();
    }

    private string GetSelectedTag() =>
        DropdownHelper.GetDropdownSelectedText(dropdown);
    
    public void OnDropdownValueChanged() => RefreshTagInfo();

    private void RefreshTagInfo()
    {
        string selectedTag = GetSelectedTag();
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
