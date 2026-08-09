using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using System.Collections.Generic;
using System.Linq;

using App.Utilities;

public class TFAdjustment : MonoBehaviour
{
    public static Dictionary<string, Vector3> offsets = new Dictionary<string, Vector3>();

    private static float stepSize = 0.01f;
    private const float holdTimeThreshold = 0.6f;
    private const float clickDistanceThreshold = 10f;

    private GameObject selectedFrame;
    private string selectedFrameName;
    public Camera arCamera; 
    public GameObject TFRoot;
    
    private bool isSelecting = false;
    private bool wasButton = false;
    private float timeSinceLastClick = 0f;
    private float clickDownTimestamp = 0f;
    public Button pauseButton;
    public Sprite pauseSprite;
    public Sprite playSprite;

    void Update()
    {
        ComputeClickTiming();
        PerformFrameSelection();
        PerformFrameScaling();        
    }

    private void ComputeClickTiming()
    {
        if(Pointer.current == null) return;

        if (Pointer.current.press.wasPressedThisFrame)
        {
            clickDownTimestamp = Time.time;
            Debug.Log("Pointer down detected at: " + clickDownTimestamp);
            return;
        }
        if (Pointer.current.press.wasReleasedThisFrame && !wasButton)
        {
            timeSinceLastClick = Time.time - clickDownTimestamp;
            clickDownTimestamp = 0f;
            Debug.Log("Pointer up detected. Time since last click: " + timeSinceLastClick);
            isSelecting = timeSinceLastClick >= holdTimeThreshold;
        }
    }

    private void PerformFrameSelection()
    {
        if(Pointer.current == null) return;

        if (wasButton)
        {
            wasButton = false;
            return;
        }

        if (!isConsideredSelection() && !lastClickWasNear())
            Deselect();

        if (isConsideredSelection() && !isFrameSelected())
            Select();
    }

    private bool isConsideredSelection() =>
        timeSinceLastClick >= holdTimeThreshold;
    
    private bool lastClickWasNear() =>
        selectedFrame != null && 
            Vector3.Distance(
                selectedFrame.transform.position, 
                Pointer.current.position.ReadValue()
            ) < clickDistanceThreshold;
    
    private bool isFrameSelected() =>
        selectedFrame != null;

    private void Deselect()
    {
        ClearHighlight(selectedFrame);
        selectedFrame = null;
    }

    private void Select()
    {
        Vector2 pointerPosition = Pointer.current.position.ReadValue();
        Ray ray = arCamera.ScreenPointToRay(pointerPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return; // No hit
        if (hit.collider == null) return; // No collider
        GameObject hitObject = hit.collider.gameObject;
        Debug.Log($"Hit object: {hitObject.name}");

        if(hitObject.name != TFFrameCreator.FrameMarker) return;
        
        StoreSelectedFrame(hitObject);
        HighlightSelectedFrame(selectedFrame);
        SetSelectedTagName();
    }

    private void StoreSelectedFrame(GameObject frame) =>
        selectedFrame = frame;

    private void HighlightSelectedFrame(GameObject frame) =>
        frame?.GetComponent<TFFrameAdjustmentShadersController>().Highlight();

    private void ClearHighlight(GameObject frame) =>
        frame?.GetComponent<TFFrameAdjustmentShadersController>().Unhighlight();

    private void PerformFrameScaling()
    {
        PCScaling();
        MobileScaling();
    }

    private void PCScaling()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if(scroll == 0 || !isFrameSelected()) return;
        ScaleSelectedFrame(new Vector3(scroll * stepSize, scroll * stepSize, scroll * stepSize));
    }

    private void MobileScaling()
    {
        if (Input.touchCount != 2 || !isFrameSelected()) return;
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);
        Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

        float prevDistance = Vector2.Distance(touch0PrevPos, touch1PrevPos);
        float currentDistance = Vector2.Distance(touch0.position, touch1.position);

        float difference = currentDistance - prevDistance;

        if (difference > 0.5f)
            ScaleSelectedFrame(new Vector3(stepSize, stepSize, stepSize));
        if (difference < -0.5f)
            ScaleSelectedFrame(new Vector3(-stepSize, -stepSize, -stepSize));
    }

    private void ScaleSelectedFrame(Vector3 scaleChange)
    {
        if (selectedFrame == null) return;
        selectedFrame.transform.localScale += scaleChange;
    }

    public void MoveLeft()
    {
        wasButton = true;
        if(selectedFrame == null) return;

        Vector3 move = new Vector3(-stepSize, 0, 0);

        if(offsets.ContainsKey(selectedFrameName))
            offsets[selectedFrameName] += move;
        else
            offsets[selectedFrameName] = move;
    }   

    public void MoveRight()
    {
        wasButton = true;
        if(selectedFrame == null) return;

        Vector3 move = new Vector3(stepSize, 0, 0);

        if(offsets.ContainsKey(selectedFrameName))
            offsets[selectedFrameName] += move;
        else
            offsets[selectedFrameName] = move;
    }

    public void MoveForward()
    {
        wasButton = true;
        if(selectedFrame == null) return;

        Vector3 move = new Vector3(0, 0, stepSize);
        
        if(offsets.ContainsKey(selectedFrameName))
            offsets[selectedFrameName] += move;
        else
            offsets[selectedFrameName] = move;
    }

    public void MoveBackward()
    {
        wasButton = true;
        if(selectedFrame == null) return;

        Vector3 move = new Vector3(0, 0, -stepSize);

        if(offsets.ContainsKey(selectedFrameName))
            offsets[selectedFrameName] += move;
        else
            offsets[selectedFrameName] = move;
    }

    public void MoveUp()
    {
        wasButton = true;
        if(selectedFrame == null) return;

        Vector3 move = new Vector3(0, stepSize, 0);

        if(offsets.ContainsKey(selectedFrameName))
            offsets[selectedFrameName] += move;
        else
            offsets[selectedFrameName] = move;
    }

    public void MoveDown()
    {
        wasButton = true;
        if(selectedFrame == null) return;

        Vector3 move = new Vector3(0, -stepSize, 0);

        if(offsets.ContainsKey(selectedFrameName))
            offsets[selectedFrameName] += move;
        else
            offsets[selectedFrameName] = move;
    }

    private void SetSelectedTagName()
    {
        string rootName = string.Empty;
        string frameName = string.Empty;
        GameObject currentFrame = selectedFrame;
        while(currentFrame != null && currentFrame.transform != TFRoot.transform)
        {
            if(currentFrame.name == TFFrameCreator.FrameMarker)
                frameName = currentFrame.transform.parent.name;

            if(currentFrame.transform.parent == TFRoot.transform)
                rootName = currentFrame.name;

            currentFrame = currentFrame.transform.parent.gameObject;
        }
        selectedFrameName = rootName + ": " + frameName;
        Debug.Log($"Selected frame name: {selectedFrameName}");
    }

    public void TogglePause()
    {
        wasButton = true;

        Debug.Log("Is paused: " + TFRuntimeController.IsPaused());
        if(TFRuntimeController.IsPaused())
            PerformPlay();
        else 
            PerformPause();
    }

    private void PerformPause()
    {
        TFRuntimeController.Pause();
        pauseButton.image.sprite = playSprite;
    }

    private void PerformPlay()
    {
        TFRuntimeController.Play();
        pauseButton.image.sprite = pauseSprite;
    }

}
