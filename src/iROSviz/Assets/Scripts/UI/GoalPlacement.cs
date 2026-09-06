using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using TMPro;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using App.Utilities;
using App.Utilities.Collections;
using App.ROSUtilities.Subscribers;

public class GoalPlacement : MonoBehaviour
{
    public Camera arCamera;
    public GameObject goalMarkerPrefab;
    public GameObject goalPathGO;
    public GameObject tfRootGO;
    public TMP_Dropdown topicDropdown;
    public GameObject ARTrackablesSlider;
    public GameObject XROrigin;


    private GameObject clickedObj;
    private Vector3 hitPoint;
    private static readonly int maxPath = 10;
    private App.Utilities.Collections.Queue<GoalMarker> pathQueue = new (maxPath);
    private App.Utilities.Collections.Queue<GameObject> reusableMarkers;
    private bool isReady = true;
    private string selectedNode;
    private bool wasTrackablesActive;

    class GoalMarker
    {
        public Vector3 position;
        public Quaternion rotation;
        public GameObject goalMarkerGO;

        public GoalMarker()
        {
            SpawnGoalMarker();   
        }

        public GoalMarker(Vector3 position, GameObject goalMarkerGO = null)
        {
            this.position = position;
            this.goalMarkerGO = goalMarkerGO;
            SpawnGoalMarker();
        }

        private void SpawnGoalMarker()
        {
            this.goalMarkerGO.transform.position = this.position;
            this.goalMarkerGO.SetActive(true);
        }
    
        public void DespawnGoalMarker() =>
            this.goalMarkerGO.SetActive(false);

        public void UpdateLineRenderer(Vector3 previousPosition)
        {
            LineRenderer lr = this.goalMarkerGO?.GetComponent<LineRenderer>();
            if(lr == null) return;

            lr.positionCount = 2;
            lr.SetPosition(0, this.position);
            lr.SetPosition(1, previousPosition);
        }

        public void UpdateRotation(Quaternion rot) =>
            this.rotation = rot;
        
        public void UpdatePosition(Vector3 pos) =>
            this.position = pos;

        public void UpdateRotation(Vector3 lookingAt) 
        {
            Vector3 direction = this.position - lookingAt;
            this.rotation = Quaternion.LookRotation(direction.normalized);
            Debug.Log($"Rotation: {rotation}");
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LoadPoseStampedTopics());        
        InstantiateReusableGoalMarkers();
        UpdateSelectedFrame();
    }

    void OnEnable() =>
        EnableTrackables();

    void OnDisable() =>
        DisableTrackables();

    private IEnumerator LoadPoseStampedTopics()
    {
        while (true)
        {
            List<string> topics = ROSTopicInfoSubscriber.GetTopicsByType("geometry_msgs/PoseStamped");
            string selectedTopic = DropdownHelper.GetDropdownSelectedText(topicDropdown);

            DropdownHelper.ClearDropdownAndSetOption(topicDropdown, topics, () => selectedTopic);
            yield return new WaitForSeconds(5);
        }
    }

    private void InstantiateReusableGoalMarkers()
    {
        reusableMarkers = new(maxPath);
        for(int i = 0; i < maxPath; i++)
        {
            GameObject go = Instantiate(goalMarkerPrefab, goalPathGO.transform);
            go.SetActive(false);
            reusableMarkers.Enqueue(go);
        }
    }

    private void UpdateSelectedFrame() =>
        NodeHighlighter.GetSelectedTag();

    private void EnableTrackables()
    {
        GameObject trackables = XROrigin.transform.Find("Trackables").gameObject;
        wasTrackablesActive = trackables.activeSelf;
        if(wasTrackablesActive) return; // if trackables are currently active
        trackables.SetActive(true);

        // Change Trackables slider to on
        DebugSlider slider = ARTrackablesSlider.GetComponent<DebugSlider>();
        slider.value = 1;
    }

    private void DisableTrackables()
    {
        if(wasTrackablesActive) return;
        GameObject trackables = XROrigin.transform.Find("Trackables").gameObject;
        trackables.SetActive(false);

        // Change Trackables slider to off
        DebugSlider slider = ARTrackablesSlider.GetComponent<DebugSlider>();
        slider.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        ChangePredictedTopic();
        PerformGoalPlacement();
        ResetPathIfFrameChanged();
        UpdateSelectedNode();
    }

    private void ChangePredictedTopic()
    {
        if(!HasChangedFrame()) return; // No changes
        List<string> options = topicDropdown.options.Select(o => o.text).ToList();
        string tfTopic = NodeHighlighter.GetSelectedTFTopic();

        string preferredTopic = 
                options.OrderByDescending(c => OutputHelper.PathSimilarity(tfTopic, c)).FirstOrDefault();
        
        DropdownHelper.SetDropdownOption(topicDropdown, preferredTopic);
    }
    
    private bool HasChangedFrame() =>
        selectedNode != NodeHighlighter.GetSelectedTag();

    private void UpdateSelectedNode() =>
        selectedNode = NodeHighlighter.GetSelectedTag();

    private void PerformGoalPlacement()
    {
        if(!IsClickPerformed()) return;
        if(IsPointingUI()) return;
        if(!IsFrameReady()) return;
        GetClickedObj();
        PlaceMarker();
    }

    private void ResetPathIfFrameChanged()
    {
        if (!HasChangedFrame()) return;
        CleanPath();
    }

    private bool IsClickPerformed() =>
        Pointer.current != null && Pointer.current.press.wasPressedThisFrame;

    private bool IsPointingUI() =>
        EventSystem.current.IsPointerOverGameObject();

    private void GetClickedObj()
    {
        // Clean previous references
        clickedObj = null;
        hitPoint = Vector3.zero;

        Vector2 pointerPosition = Pointer.current.position.ReadValue();
        Ray ray = arCamera.ScreenPointToRay(pointerPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return; // No hit
        if (hit.collider == null) return; // No collider

        clickedObj = hit.collider.gameObject;
        hitPoint = hit.point;
    } 

    private bool IsClickedInTrackablePlane() => 
        clickedObj != null && clickedObj.name.Contains("ARPlane");

    private void PlaceMarker()
    {
        if(!IsClickedInTrackablePlane()) return;
        if(IsMarkerQueueFull()) return;
        
        Debug.Log($"Point clicked: {hitPoint}");
        StoreGoalMarker();
    }

    private bool IsMarkerQueueFull() =>
        pathQueue.QueueFull();

    private void StoreGoalMarker() =>
        pathQueue.Enqueue(CreateNewGoalMarker());

    public void DeleteLastMarker()
    {
        GoalMarker lastGM = pathQueue.Dequeue();
        lastGM?.DespawnGoalMarker();
        if(lastGM != null) reusableMarkers.Enqueue(lastGM.goalMarkerGO);
    }

    private GoalMarker CreateNewGoalMarker()
    {
        GameObject goalMarkerGO = reusableMarkers.Dequeue();
        GoalMarker lastGM = pathQueue.Peek();
        GoalMarker newGM = new(hitPoint, goalMarkerGO);

        if(lastGM != null) newGM.UpdateLineRenderer(lastGM.position);
        if(lastGM != null) lastGM.UpdateRotation(hitPoint);

        return newGM;
    }

    public void ExecutePath()
    {
        if(!IsFrameReady()) return;
        SendPathToFrame();
        CleanPath();
    }

    private bool IsFrameReady() =>
        isReady;

    private void SendPathToFrame()
    {
        TFRoot.FrameNode frame = GetSelectedFrame();
        if(frame == null) return;

        Vector3[] positions = GetPositions();
        Quaternion[] rotations = pathQueue.ToArray().Select(e => UnityToRosRotation(e.rotation)).ToArray();
        // positions[0] = new Vector3(0, 0, 1);
        Debug.Log($"Positions: {positions.ToArray()}");

        PathController pc = frame.GO.GetComponent<PathController>();
        SetNotReady();
        pc.SetGoalTopic(DropdownHelper.GetDropdownSelectedText(topicDropdown));
        pc.AddNotification(SetReady);
        pc.ExecutePath(positions, rotations);
    }

    private Vector3[] GetPositions()
    {
        Vector3[] positions = pathQueue.ToArray().Select(e => UnityToRosPosition(e.position)).ToArray();
        string tfTopic = NodeHighlighter.GetSelectedTFTopic();
        List<string> tags = TagDatabase.GetAllTagNames();
        string relatedTag = tags.Where(t => t.Contains(tfTopic)).FirstOrDefault() ?? string.Empty;
        bool exists = TagDatabase.TryGetTagId(relatedTag, out int tagId);
        Vector3 anchor = new Vector3();
        
        if(FiducialSystemFrameProcessor.tagAnchors.ContainsKey(tagId)) 
            anchor = FiducialSystemFrameProcessor.tagAnchors[tagId];
        
        for(int i = 0; i < positions.Length; i++)
            positions[i] = positions[i] - anchor;
        
        return positions;
    }

    private void SetNotReady() =>
        isReady = false;

    private void SetReady() =>
        isReady = true;

    private void CleanPath()
    {
        while(!pathQueue.QueueEmpty())
            DeleteLastMarker();
    }

    private TFRoot.FrameNode GetSelectedFrame()
    {
        // Tag pattern: <tf_topic>: <frame>
        string frameName = NodeHighlighter.GetSelectedFrame();
        string tfTopic = NodeHighlighter.GetSelectedTFTopic();

        if(string.IsNullOrEmpty(frameName) || string.IsNullOrEmpty(tfTopic)) return null;

        TFRoot tfRoot = tfRootGO.GetComponent<TFRoot>();
        if(tfRoot == null) return null;
        
        if(!tfRoot.GetFramesByTopic().ContainsKey(tfTopic)) return null;
        Dictionary<string, TFRoot.FrameNode> namespaceFrames = 
            tfRoot.GetFramesByTopic()[tfTopic];

        if(!namespaceFrames.ContainsKey(frameName)) return null;
        return namespaceFrames[frameName];
    }

    Vector3 UnityToRosPosition(Vector3 unity) => 
        new Vector3(
            (float)unity.z,
            (float)-unity.x,
            (float)unity.y
        );

    Quaternion UnityToRosRotation(Quaternion unity) =>
        new Quaternion(
            (float)unity.z,
            (float)-unity.x,
            (float)unity.y,
            (float)-unity.w
        );
}
