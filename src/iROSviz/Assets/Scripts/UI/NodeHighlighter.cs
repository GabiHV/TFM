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

    void Update()
    {

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Debug.Log("Click/Tap en pantalla: " + screenPos);
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
