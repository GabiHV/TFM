using UnityEngine;

public class TFFrameRotationController : MonoBehaviour
{
    public TFRoot.FrameNode frame;

    public GameObject TFFrameObj;

    void Update() =>
        UpdateFrame();

    private void UpdateFrame()
    {
        if(!IsFrameSet()) return;
        UpdateRotation();
    }

    private bool IsFrameSet() =>
        frame != null;

    private void UpdateRotation()
    {
        if(IsPaused()) return;
        SetTFRotation();
        SetTFFrameObjRotation();
    }

    private void SetTFRotation() =>
        frame.GO.transform.rotation = frame.Rotation;

    private void SetTFFrameObjRotation() =>
        TFFrameObj.transform.rotation = frame.GO.transform.rotation;

    private bool IsPaused() =>
        TFRuntimeController.IsPaused();
}
