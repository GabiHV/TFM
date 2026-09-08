using UnityEngine;
using TMPro;

using System.Collections.Generic;

public class TFFrameLabelsController : MonoBehaviour
{
    public TFRoot.FrameNode frame;
    public Camera arCamera;
    public GameObject mainLabel;
    public List<Transform> labelTransforms;

    // Update is called once per frame
    void Update() =>
        UpdateLabel();

    private bool IsFrameSet() =>
        frame != null;

    private void UpdateLabel()
    {
        if (!IsFrameSet()) return;
        SetMainLabel();
        LabelsBillboard();
    }

    private void SetMainLabel() =>
        mainLabel.GetComponent<TextMeshPro>().text = $"{frame.Root}: {frame.Name}";

    private void LabelsBillboard()
    {
        foreach(Transform labelTransform in labelTransforms)
            PerformBillboard(labelTransform);
    }

    private void PerformBillboard(Transform labelTransform)
    {
        Transform cam = arCamera.transform;
        labelTransform.rotation = cam.rotation;
        labelTransform.LookAt(
            labelTransform.position + cam.rotation * Vector3.forward,
            cam.rotation * Vector3.up
        );
    }
}
