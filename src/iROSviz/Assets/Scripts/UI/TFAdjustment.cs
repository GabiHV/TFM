using UnityEngine;

using App.Utilities;

public class TFAdjustment : MonoBehaviour
{
    private static float stepSize = 0.01f;

    public void MoveLeft()
    {
        string selectedNode = NodeHighlighter.selectedNode;
        TagDatabase.TryGetTagId(selectedNode, out int tagId);
        if(tagId == -1) return;

        Vector3 move = new Vector3(-stepSize, 0, 0);
        FiducialSystemFrameProcessor.tagAnchors[tagId] += move;
    }   

    public void MoveRight()
    {
        string selectedNode = NodeHighlighter.selectedNode;
        TagDatabase.TryGetTagId(selectedNode, out int tagId);
        if(tagId == -1) return;

        Vector3 move = new Vector3(stepSize, 0, 0);
        FiducialSystemFrameProcessor.tagAnchors[tagId] += move;
    }

    public void MoveForward()
    {
        string selectedNode = NodeHighlighter.selectedNode;
        TagDatabase.TryGetTagId(selectedNode, out int tagId);
        if(tagId == -1) return;

        Vector3 move = new Vector3(0, 0, stepSize);
        FiducialSystemFrameProcessor.tagAnchors[tagId] += move;
    }

    public void MoveBackward()
    {
        string selectedNode = NodeHighlighter.selectedNode;
        TagDatabase.TryGetTagId(selectedNode, out int tagId);
        if(tagId == -1) return;

        Vector3 move = new Vector3(0, 0, -stepSize);
        FiducialSystemFrameProcessor.tagAnchors[tagId] += move;
    }

    public void MoveUp()
    {
        string selectedNode = NodeHighlighter.selectedNode;
        TagDatabase.TryGetTagId(selectedNode, out int tagId);
        if(tagId == -1) return;

        Vector3 move = new Vector3(0, stepSize, 0);
        FiducialSystemFrameProcessor.tagAnchors[tagId] += move;
    }

    public void MoveDown()
    {
        string selectedNode = NodeHighlighter.selectedNode;
        TagDatabase.TryGetTagId(selectedNode, out int tagId);
        if(tagId == -1) return;

        Vector3 move = new Vector3(0, -stepSize, 0);
        FiducialSystemFrameProcessor.tagAnchors[tagId] += move;
    }
}
