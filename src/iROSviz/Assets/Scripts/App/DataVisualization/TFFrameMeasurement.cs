using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

using System.Collections.Generic;

using App.Utilities;

public class TFFrameMeasurement : MonoBehaviour
{
    public Camera arCamera;
    public TFRoot tFRoot;
    private TFRoot.FrameNode frameA;
    private bool isFrameATurn = true;
    private TFRoot.FrameNode frameB;
    public Shader highlightShader;

    public LocalizeStringEvent distanceTextEvent;
    public LocalizeStringEvent rollTextEvent;
    public LocalizeStringEvent pitchTextEvent;
    public LocalizeStringEvent yawTextEvent;

    private int frameCount = 0;
    private static readonly int frameThreshold = 30;

    Dictionary<Transform, Dictionary<Material, Shader>> defaultShadersA;
    Dictionary<Transform, Dictionary<Material, Shader>> defaultShadersB;

    // Update is called once per frame
    void Update()
    {
        FrameSelection();
        if(RecalculateFrame() && BothSelected())
            VisualizeMeasurement();
    }
    
    private void FrameSelection()
    {
        if(Pointer.current == null || !Pointer.current.press.wasPressedThisFrame) return;
        // TODO: Check deselecion
        Select();
    }

    private void Select()
    {
        Vector2 pointerPosition = Pointer.current.position.ReadValue();
        Ray ray = arCamera.ScreenPointToRay(pointerPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return; // No hit
        if (hit.collider == null) return; // No collider
        GameObject hitObject = hit.collider.gameObject;
        Debug.Log($"Hit object: {hitObject.name}. Turn: {(isFrameATurn ? 'A' : 'B')}");

        foreach (var robotEntry in tFRoot.frames)
        {
            foreach (var (_, frameEntry) in robotEntry.Value)
            {
                if (frameEntry.GO == hitObject || frameEntry.GO.transform.Find("JointMarker")?.gameObject == hitObject)
                {
                    PerformSelection(frameEntry);
                    return;
                }
            }
        }
    }

    private void PerformSelection(TFRoot.FrameNode frame)
    {
        if (isFrameATurn)
        {
            UnhighlightFrame(frameA);
            UnhighlightFrame(frameB);
        }
        HighlightFrame(frame);
        StoreFrameAndChangeTurn(frame);
    }

    private void UnhighlightFrame(TFRoot.FrameNode frame)
    {
        if(frame == null) return;
        var originalShaders = GetOriginalFrameShaders(frame);
        ApplyShaderToFrame(frame, originalShaders);
    }

    private void HighlightFrame(TFRoot.FrameNode frame)
    {
        GameObject jointMarker = GetJointMarker(frame);
        if(jointMarker == null) return;

        StoreFrameOriginalShaders(frame);   
        Dictionary<Transform, Dictionary<Material, Shader>> highlightShaders = 
            GameObjectHelper.ChangeChildrenShaders(
                GameObjectHelper.GetChildrenShaders(
                    jointMarker, 
                    new HashSet<Transform> {GetJointMarkerLabel(frame)}
                ),
                highlightShader
            );

        ApplyShaderToFrame(frame, highlightShaders);
    }

    private void StoreFrameOriginalShaders(TFRoot.FrameNode frame)
    {
        GameObject jointMarker = GetJointMarker(frame);
        if(jointMarker == null) return;

        var shaders = GameObjectHelper.GetChildrenShaders(
            jointMarker, 
            new HashSet<Transform> {GetJointMarkerLabel(frame)}
        );

        if(isFrameATurn)
            defaultShadersA = shaders;
        if(!isFrameATurn)
            defaultShadersB = shaders;
    }

    private Dictionary<Transform, Dictionary<Material, Shader>> GetOriginalFrameShaders(
        TFRoot.FrameNode frame
    )
    {
        if(frame == frameA)
            return defaultShadersA;
        
        return defaultShadersB;
    }

    private GameObject GetJointMarker(TFRoot.FrameNode frame) =>
        frame?.GO?.transform.Find("JointMarker")?.gameObject;

    private Transform GetJointMarkerLabel(TFRoot.FrameNode frame) =>
        frame?.GO?.transform.Find("JointMarker")?.Find("Label");
    
    private void ApplyShaderToFrame(
        TFRoot.FrameNode frame, 
        Dictionary<Transform, Dictionary<Material, Shader>> shader
    )
    {
        GameObject jointMarker = GetJointMarker(frame);
        if(jointMarker == null) return;

        GameObjectHelper.ApplyShaderToChildrenGameObject(jointMarker, shader);
    }

    private void StoreFrameAndChangeTurn(TFRoot.FrameNode frame)
    {
        StoreFrame(frame);
        ChangeTurn();
    }

    private void StoreFrame(TFRoot.FrameNode frame)
    {
        if (isFrameATurn)
        {
            frameA = frame;
            return;
        }
        
        frameB = frame;
    }

    private void ChangeTurn() =>
        isFrameATurn = !isFrameATurn;

    private bool BothSelected() =>
        frameA != null && frameB != null;

    private bool RecalculateFrame()
    {
        if(frameCount++ >= frameThreshold)
        {
            frameCount = 0;
            return true;
        } 

        return false;
    }

    private void VisualizeMeasurement()
    {
        Transform transformA = frameA.GO?.transform;
        Transform transformB = frameB.GO?.transform;
        if(transformA == null || transformB == null) return;

        float distance = CalculateDistance(transformA, transformB);
        Vector3 orientation = CalculateOrientation(transformA, transformB);

        Vector3 mid = (transformA.position + transformB.position) / 2f;

        ChangePanelInfo(distance, orientation);
    }

    private float CalculateDistance(Transform a, Transform b) =>
        Vector3.Distance(a.position, b.position);

    private Vector3 CalculateOrientation(Transform a, Transform b)
    {
        Quaternion delta = Quaternion.Inverse(a.rotation) * b.rotation;

        return delta.eulerAngles;
    }

    private void ChangePanelInfo(float distance, Vector3 orientation)
    {
        if (distanceTextEvent.StringReference.TryGetValue("distance", out var distanceValue))
            (distanceValue as FloatVariable).Value = distance;
        if (rollTextEvent.StringReference.TryGetValue("roll", out var rollValue))
            (rollValue as FloatVariable).Value = orientation.x;
        if (pitchTextEvent.StringReference.TryGetValue("pitch", out var pitchValue))
            (pitchValue as FloatVariable).Value = orientation.y;
        if (yawTextEvent.StringReference.TryGetValue("yaw", out var yawValue))
            (yawValue as FloatVariable).Value = orientation.z;

        distanceTextEvent.StringReference.RefreshString();
        rollTextEvent.StringReference.RefreshString();
        pitchTextEvent.StringReference.RefreshString();
        yawTextEvent.StringReference.RefreshString();
    }
}
