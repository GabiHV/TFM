using UnityEngine;
using RosMessageTypes.Geometry;
using RosMessageTypes.Tf2;
using TMPro;

using System.Collections.Generic;

using App.Utilities;
using App.ROSUtilities;

public class TFRoot : MonoBehaviour
{
    public Dictionary<string, Dictionary<string, FrameNode>> frames = new();
    Dictionary<int, Vector3> frameOriginAnchors = new();
    public GameObject framePrefab;
    public Camera arCamera;

    public float staleThreshold = 0.5f; // seconds
    public float lowHzThreshold = 5.0f; // Hz

    Transform tfRoot;

    public class FrameNode
    {
        public string Name;
        public FrameNode Parent;
        public List<FrameNode> Children = new();
        public Vector3 Position;
        public Quaternion Rotation;
        public GameObject GO;

        public float LastUpdateTime;
        public float LastROSTimestamp;
        public float UpdateInterval;
        public float SmoothedHZ;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tfRoot = this.transform;
        ProcessTF();
    }

    // Update is called once per frame
    void Update() =>
        ProcessTF();

    private void ProcessTF()
    {
        Dictionary<string, TFMessageMsg> tfInfo = ROSTFService.RefreshTF();  
        foreach (var topicEntry in tfInfo)
        {
            TFMessageMsg msg = topicEntry.Value;
            // Assuming topic format is something like "/tf" or "/tf_robot1", we can extract the robot name from the topic.
            string[] parts = topicEntry.Key.Split('/');
            string robotName = parts.Length > 1 ? parts[1] : "robot";

            var firstTf = msg.transforms.Length > 0 ? msg.transforms[0] : null;
            string firstChild = firstTf != null ? firstTf.child_frame_id : "base_link";

            UpdateOrCreateRootFrame(robotName);

            foreach (var tf in msg.transforms)
            {
                string parent = tf.header.frame_id;
                string child  = tf.child_frame_id;

                Vector3 pos = RosToUnityPosition(tf.transform.translation);
                Quaternion rot = RosToUnityRotation(tf.transform.rotation);

                UpdateOrCreateFrame(robotName, parent, child, pos, rot);
            }

            LinkRootFrame(robotName);

            ApplyUnity();
        } 
    }   

    private void UpdateOrCreateRootFrame(string rootName = "robot")
    {
        if (frames.ContainsKey(rootName)) return;
        
        frames.Add(rootName, new Dictionary<string, FrameNode>());

        FrameNode rootFrame = GetOrCreateFrame(rootName, rootName);

        rootFrame.Position = Vector3.zero;
        rootFrame.Rotation = Quaternion.identity;
    }

    private void LinkRootFrame(string rootName)
    {
        FrameNode rootNode = frames[rootName][rootName];
        FrameNode firstChild = null;
        foreach (var frame in frames[rootName].Values)
        {
            if (frame.Name != rootName)
            {
                firstChild = frame;
                break;
            }
        }
        if(firstChild == null) return;

        rootNode.Children.Add(firstChild);
        firstChild.Parent = rootNode;
    }

    private void UpdateOrCreateFrame(
        string root,
        string parent, 
        string child, 
        Vector3 pos, 
        Quaternion rot    
    )
    {
        
        FrameNode parentFrame = GetOrCreateFrame(root, parent);
        FrameNode childFrame  = GetOrCreateFrame(root, child);

        FrequencyAndTimeUpdate(childFrame);
        UpdateFramePositionAndRotation(childFrame, pos, rot);

        UpdateDescendants(parentFrame, childFrame);
    }

    private void FrequencyAndTimeUpdate(FrameNode frame)
    {   
        float now = Time.time;

        float deltaT = now - frame.LastUpdateTime;
        frame.UpdateInterval = deltaT;

        float instantHz = 1.0f / Mathf.Max(deltaT, 0.0001f);

        // Smoothing
        frame.SmoothedHZ = Mathf.Lerp(frame.SmoothedHZ, instantHz, 0.1f);

        frame.LastUpdateTime = now;
    }

    private void UpdateFramePositionAndRotation(
        FrameNode frame, 
        Vector3 pos, 
        Quaternion rot
    )
    {
        frame.Position = pos;
        frame.Rotation = rot;
    }

    private void UpdateDescendants(FrameNode parentFrame, FrameNode childFrame)
    {
        if(childFrame.Parent == null)
            childFrame.Parent = parentFrame;
        if(!parentFrame.Children.Contains(childFrame))
            parentFrame.Children.Add(childFrame);
    }

    private FrameNode GetOrCreateFrame(string robot, string frame)
    {
        if (frames[robot].ContainsKey(frame))
            return frames[robot][frame];

        FrameNode frameNode = new()
            {
                Name = frame,
                GO = new GameObject(frame)
            };

        frames[robot][frame] = frameNode;

        return frameNode;
    }

    private void ApplyUnity()
    {
        foreach (var robotFrames in frames)
        {
            string rootName = robotFrames.Key;
            TagDatabase.TryGetTagId(rootName, out int tagId);
            TagDatabase.TryGetTagVisualization(tagId, out string visualization);

            Dictionary<string, FrameNode> robotFramesDict = robotFrames.Value;
            foreach (var frame in robotFramesDict.Values)
            {
                if (frame.GO == null)
                    continue;

                Transform t = frame.GO.transform;
                Transform parentT = 
                    frame.Parent != null && 
                    robotFramesDict.ContainsKey(frame.Parent.Name) ? 
                    robotFramesDict[frame.Parent.Name].GO.transform : 
                    tfRoot;

                SetFrameParent(t, parentT);

                SetUnityPosition(frame, visualization, tagId);
                SetUnityRotation(frame);

                SimpleVisulizeFrame(frame, rootName);
            }    
        }
    }

    private void SimpleVisulizeFrame(FrameNode frame, string rootName)
    {
        GameObject jointMarker = GetOrCreateUnityFrame(frame);

        SetUnityParent(jointMarker, frame.GO);

        UpdatePositionAndRotation(
            jointMarker, 
            frame.GO.transform.position, 
            frame.GO.transform.rotation
        );

        // Sphere color indicates staleness
        UpdateSphereColor(frame);

        // Label
        UpdateLabel(jointMarker, rootName + ": " + frame.Name);
            
        // Line to parent
        RenderLineToParent(frame);
    }

    private void UpdateSphereColor(FrameNode frame)
    {
        GameObject frameObj = frame.GO;
        if(frameObj == null) return;

        GameObject sphere = frameObj.transform.Find("JointMarker/Origin")?.gameObject;
        if(sphere == null) return;
        Debug.Log($"Frame {frame.Name} - Smoothed Hz: {frame.SmoothedHZ:F2}, Last Update: {Time.time - frame.LastUpdateTime:F2}s ago");


        Renderer rend = sphere.GetComponent<Renderer>();
        if(rend == null) return;

        Color color = GetColorBasedOnStaleness(frame);
        rend.material.color = color;
    }

    private Color GetColorBasedOnStaleness(FrameNode frame)
    {
        if (IsStale(frame))
            return Color.red;
        
        if (frame.SmoothedHZ < lowHzThreshold)
            return Color.yellow;
            
        return Color.green;
    }

    private bool IsStale(FrameNode frame)
    {
        float now = Time.time;
        return (now - frame.LastUpdateTime) > staleThreshold;
    }

    private GameObject GetOrCreateUnityFrame(FrameNode frame)
    {
        GameObject jointMarker = 
            frame.GO.transform.Find("JointMarker")?.gameObject; 

        if (jointMarker == null)
        {
            jointMarker = Instantiate(framePrefab);
            jointMarker.name = "JointMarker";
            jointMarker.transform.localScale = Vector3.one * 0.05f;        
        }
        
        return jointMarker;
    }

    private void SetUnityParent(GameObject child, GameObject parent)
    {
        if(child.transform.parent != parent.transform)
            child.transform.SetParent(parent.transform, false);
    }

    private void UpdatePositionAndRotation(GameObject go, Vector3 pos, Quaternion rot)
    {
        go.transform.position = pos;
        go.transform.rotation = rot;
    }

    private void UpdateLabel(GameObject jointMarker, string text)
    {
        Transform labelTransform = jointMarker.transform.Find("Label");
        if(labelTransform == null) return;
        
        // Text
        labelTransform.GetComponent<TextMeshPro>().text = text;

        // Billboard
        Transform cam = arCamera.transform;
        labelTransform.rotation = cam.rotation;
        labelTransform.LookAt(
            labelTransform.position + cam.rotation * Vector3.forward,
            cam.rotation * Vector3.up
        );
    }

    private void RenderLineToParent(FrameNode frame)
    {
        if(frame.Parent == null) return;
        Vector3 parentPos = frame.Parent.GO.transform.position;
        Vector3 childPos = frame.GO.transform.position;
        if(Vector3.Distance(parentPos, childPos) < 0.01f) return; // Skip if too close

        LineRenderer lr = frame.GO.GetComponent<LineRenderer>();
        if(lr == null)
            lr = frame.GO.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, parentPos);
        lr.SetPosition(1, childPos);
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.red;
        lr.endColor = Color.red;
        lr.widthMultiplier = 0.1f;
    }

    private void SetFrameParent(Transform child, Transform parent)
    {
        if(child.parent != parent)
            child.SetParent(parent, false);
    }

    private void SetUnityPosition(FrameNode frame, string visualizationFrame, int tagId)
    {
        Vector3 finalPos = frame.Position;
        // If this frame is the root visualization frame, we need to adjust its position 
        // based on the tag anchor and the original position of the frame when it was first seen.
        if(frame.Name == visualizationFrame)
        {
            if(!frameOriginAnchors.ContainsKey(tagId))
                frameOriginAnchors[tagId] = frame.Position;
            Vector3 realAnchor = 
                FiducialSystemFrameProcessor.tagAnchors.ContainsKey(tagId) ? 
                FiducialSystemFrameProcessor.tagAnchors[tagId] : 
                Vector3.zero;
            Vector3 offset = frameOriginAnchors[tagId];
            Vector3 adjustedPos = frame.Position - offset;
            finalPos = realAnchor + adjustedPos;
        }

        frame.GO.transform.localPosition = finalPos;
    }
    
    private void SetUnityRotation(FrameNode frame) =>
        frame.GO.transform.localRotation = frame.Rotation;

    Vector3 RosToUnityPosition(Vector3Msg ros) => 
        new Vector3(
            (float)-ros.y,
            (float)ros.z,
            (float)ros.x
        );

    Quaternion RosToUnityRotation(QuaternionMsg ros) =>
        new Quaternion(
            (float)-ros.y,
            (float)ros.z,
            (float)ros.x,
            (float)-ros.w
        );

}
