using UnityEngine;

using System.Collections.Generic;
using System.Linq;

public class TFFrameJumpVisualizer : MonoBehaviour
{
    public TFRoot.FrameNode frame;
    public GameObject TFFrameObj;
    public float distanceThreshold;
    public float rotationThreshold;

    private float deltaDistance;
    private float deltaRotation;

    void Update() =>
        PerformJumpDetection();

    private bool IsFrameSet() =>
        frame != null;

    private void PerformJumpDetection()
    {
        if(!IsFrameSet()) return;
        JumpDetection();
        RotationJumpDetection();
        ApplyJumpWarning();
        ApplyRotationJumpWarning();
    }

    private void JumpDetection()
    {
        Vector3 currentPos = frame.Position;
        List<Vector3> histPos = frame.GetHistPosition();
        Vector3 lastPos = histPos.Last();

        deltaDistance = Vector3.Distance(currentPos, lastPos);
    }

    private void RotationJumpDetection()
    {
        Quaternion currentRotation = frame.Rotation;
        List<Quaternion> histRot = frame.GetHistRotation();
        Quaternion lastRot = histRot.Last();

        deltaRotation = Quaternion.Angle(currentRotation, lastRot);
    }

    private bool IsJump() =>
        deltaDistance > distanceThreshold;

    private bool IsRotationJump() =>
        deltaRotation > rotationThreshold;

    private void ApplyJumpWarning()
    {
        Debug.Log($"Frame: {frame.Name}. DeltaDistance: {deltaDistance}");
    }

    private void ApplyRotationJumpWarning()
    {
        Debug.Log($"Frame: {frame.Name}. DeltaRotatiopn: {deltaRotation}");
    }
}
