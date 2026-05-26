using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using System.Collections.Generic;
using System.Linq;

using App.Utilities;

public class TFAdjustment : MonoBehaviour
{
    public static bool isPaused;
    private static float stepSize = 0.01f;
    private const float holdTimeThreshold = 0.6f;
    private const float clickDistanceThreshold = 10f;

    private GameObject selectedFrame;
    public Camera arCamera; 
    public Shader highlightShader;
    public TFRoot tfRoot;
    
    private bool isDragging = false;
    private bool isSelecting = false;
    private float timeSinceLastClick = 0f;
    private float clickDownTimestamp = 0f;
    private Dictionary<Transform, Dictionary<Material, Shader>> defaultShaders;
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
            isDragging = true;
            clickDownTimestamp = Time.time;
            Debug.Log("Pointer down detected at: " + clickDownTimestamp);
            return;
        }
        if (Pointer.current.press.wasReleasedThisFrame)
        {
            isDragging = false;
            timeSinceLastClick = Time.time - clickDownTimestamp;
            clickDownTimestamp = 0f;
            Debug.Log("Pointer up detected. Time since last click: " + timeSinceLastClick);
            isSelecting = timeSinceLastClick >= holdTimeThreshold;
        }
    }

    private void PerformFrameSelection()
    {
        if(Pointer.current == null) return;

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

        foreach (var robotEntry in tfRoot.frames)
        {
            foreach (var frameEntry in robotEntry.Value)
            {
                if (frameEntry.Value.GO == hitObject || frameEntry.Value.GO.transform.Find("JointMarker")?.gameObject == hitObject)
                {
                    selectedFrame = frameEntry.Value.GO.transform.Find("JointMarker")?.gameObject ?? 
                        frameEntry.Value.GO;
                    Debug.Log($"Selected frame: {frameEntry.Key}");
                    HighlightSelectedFrame(selectedFrame);
                    return;
                }
            }
        }
    }

    private void HighlightSelectedFrame(GameObject selectedFrame)
    {
        if(selectedFrame == null) return;

        StoreDefaultShader(selectedFrame);
        Dictionary<Transform, Dictionary<Material, Shader>> highlightShaders = 
            GameObjectHelper.ChangeChildrenShaders(
                GameObjectHelper.GetChildrenShaders(
                    selectedFrame, 
                    new HashSet<Transform>{GetJointMarkerLabel(selectedFrame)}
                ),
                highlightShader
            );

        ApplyShaderToFrame(selectedFrame, highlightShaders);
    }

    private Transform GetJointMarkerLabel(GameObject selectedFrame) =>
        selectedFrame.transform.Find("Label");

    private void StoreDefaultShader(GameObject frame) =>
        defaultShaders = GameObjectHelper.GetChildrenShaders(
            frame, 
            new HashSet<Transform> {GetJointMarkerLabel(frame)}
        );

    private void ClearHighlight(GameObject frame)
    {
        if(frame == null) return;

        ApplyShaderToFrame(frame, defaultShaders);
    }

    private void ApplyShaderToFrame(
        GameObject frame, 
        Dictionary<Transform, Dictionary<Material, Shader>> shaders
    ) =>
        GameObjectHelper.ApplyShaderToChildrenGameObject(frame, shaders);

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
        int tagId = GetSelectedTagId();
        if(tagId == -1) return;

        Vector3 move = new Vector3(-stepSize, 0, 0);
        FiducialSystemFrameProcessor.tagAnchors[tagId] += move;
    }   

    public void MoveRight()
    {
        int tagId = GetSelectedTagId();
        if(tagId == -1) return;

        Vector3 move = new Vector3(stepSize, 0, 0);
        FiducialSystemFrameProcessor.tagAnchors[tagId] += move;
    }

    public void MoveForward()
    {
        int tagId = GetSelectedTagId();
        if(tagId == -1) return;

        Vector3 move = new Vector3(0, 0, stepSize);
        FiducialSystemFrameProcessor.tagAnchors[tagId] += move;
    }

    public void MoveBackward()
    {
        int tagId = GetSelectedTagId();
        if(tagId == -1) return;

        Vector3 move = new Vector3(0, 0, -stepSize);
        FiducialSystemFrameProcessor.tagAnchors[tagId] += move;
    }

    public void MoveUp()
    {
        int tagId = GetSelectedTagId();
        if(tagId == -1) return;

        Vector3 move = new Vector3(0, stepSize, 0);
        FiducialSystemFrameProcessor.tagAnchors[tagId] += move;
    }

    public void MoveDown()
    {
        int tagId = GetSelectedTagId();
        if(tagId == -1) return;

        Vector3 move = new Vector3(0, -stepSize, 0);
        FiducialSystemFrameProcessor.tagAnchors[tagId] += move;
    }

    private int GetSelectedTagId()
    {
        string selectedNode = NodeHighlighter.selectedNode;
        TagDatabase.TryGetTagId(selectedNode, out int tagId);
        return tagId;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if(isPaused)
            pauseButton.image.sprite = playSprite;
        else 
            pauseButton.image.sprite = pauseSprite;
    }

}
