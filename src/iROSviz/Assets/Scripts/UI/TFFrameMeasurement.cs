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
    private GameObject frameMarkerA;
    private bool isFrameATurn = true;
    private GameObject frameMarkerB;

    public LocalizeStringEvent distanceTextEvent;
    public LocalizeStringEvent rollTextEvent;
    public LocalizeStringEvent pitchTextEvent;
    public LocalizeStringEvent yawTextEvent;

    private int frameCount = 0;
    private static readonly int frameThreshold = 30;

    // Update is called once per frame
    void Update()
    {
        FrameSelection();
        if(RecalculateFrame() && BothSelected())
            VisualizeMeasurement();
    }

    void OnDisable() =>
        ClearSelections();
    
    private void FrameSelection()
    {
        if(Pointer.current == null || !Pointer.current.press.wasPressedThisFrame) return;
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

        if(hitObject == null || hitObject?.name != TFFrameCreator.FrameMarker)
            ClearSelections();
        if(hitObject?.name == TFFrameCreator.FrameMarker)
            PerformSelection(hitObject);
    }

    private void PerformSelection(GameObject frameMarker)
    {
        if (isFrameATurn)
            ClearSelections();
        frameMarker.GetComponent<TFFrameMeasurementShadersController>().Highlight();
        StoreFrameAndChangeTurn(frameMarker);
    }

    private void ClearSelections()
    {
        frameMarkerA?.GetComponent<TFFrameMeasurementShadersController>().Unhighlight();
        frameMarkerB?.GetComponent<TFFrameMeasurementShadersController>().Unhighlight();
        isFrameATurn = true;
    }

    private void StoreFrameAndChangeTurn(GameObject frameMarker)
    {
        StoreFrame(frameMarker);
        ChangeTurn();
    }

    private void StoreFrame(GameObject frameMarker)
    {
        if (isFrameATurn)
        {
            frameMarkerA = frameMarker;
            return;
        }
        
        frameMarkerB = frameMarker;
    }

    private void ChangeTurn() =>
        isFrameATurn = !isFrameATurn;

    private bool BothSelected() =>
        frameMarkerA != null && frameMarkerB != null;

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
        Transform transformA = frameMarkerA.transform;
        Transform transformB = frameMarkerB.transform;
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
