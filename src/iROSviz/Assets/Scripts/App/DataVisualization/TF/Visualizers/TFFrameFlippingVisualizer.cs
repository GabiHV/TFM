using UnityEngine;

using System.Collections.Generic;
using System.Linq;

public class TFFrameFlippingVisualizer : MonoBehaviour
{
    public TFRoot.FrameNode frame;
    public GameObject TFFrameObj;

    private float flipping;
    private static float flippingThreshold = 0;

    void Update() =>
        PerformFlippingDetection();

    private bool IsFrameSet() =>
        frame != null;

    private void PerformFlippingDetection()
    {
        if(!IsFrameSet()) return;
        DetectFlipping();
        ApplyFlippingWarning();
    }

    private void DetectFlipping()
    {
        List<Quaternion> rotations = frame.GetHistRotation();
        Quaternion currentRot = frame.Rotation;
        Quaternion lastRot = rotations.Last();

        flipping = Quaternion.Dot(currentRot, lastRot);
    }

    private void ApplyFlippingWarning()
    {
        // if(!IsFlipping()) return;
        Debug.Log($"Frame: {frame.Name}. Flipping: {flipping}");
    }

    private bool IsFlipping() =>
        flipping < flippingThreshold;
}
