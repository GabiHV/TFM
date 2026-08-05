using UnityEngine;

using App.Utilities;

public class TFFramePositionController : MonoBehaviour
{
    public TFRoot.FrameNode frame;

    public GameObject TFFrameObj;

    private Vector3 finalPos;

    void Update() =>
        UpdateFrame();

    private void UpdateFrame()
    {
        if(!IsFrameSet()) return;
        UpdatePosition();
    }

    private bool IsFrameSet() =>
        frame != null;

    private void UpdatePosition()
    {
        if(IsPaused()) return;

        SetTFPosition();
        SetTFFrameObjPosition();
    }

    private void SetTFPosition()
    {
        bool isAssignedToFrame = 
            TagDatabase.TryGetTagId($"{frame.Root}: {frame.Name}", out int tagId);

        finalPos = frame.Position;
        // If this frame is the root visualization frame, we need to adjust its position 
        // based on the tag anchor and the original position of the frame when it was first seen.
        if(isAssignedToFrame)
        {
            Vector3 realAnchor = 
                FiducialSystemFrameProcessor.tagAnchors.ContainsKey(tagId) ? 
                FiducialSystemFrameProcessor.tagAnchors[tagId] : 
                Vector3.zero;
            Vector3 offset = finalPos;
            Vector3 adjustedPos = frame.Position - offset;
            finalPos = realAnchor + adjustedPos;
        }

        frame.GO.transform.localPosition = finalPos;
    }

    private void SetTFFrameObjPosition() =>
        TFFrameObj.transform.position = frame.GO.transform.position;

    private bool IsPaused() =>
        TFRuntimeController.IsPaused();
}
