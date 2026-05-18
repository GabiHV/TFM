using UnityEngine;
using RosMessageTypes.Geometry;
using RosMessageTypes.Tf2;

using System.Collections.Generic;

using App.Utilities;
using App.ROSUtilities;

public class TFRoot : MonoBehaviour
{
    Dictionary<string, Dictionary<string, FrameNode>> frames = new();
    Dictionary<int, Vector3> frameOriginAnchors = new();

    Transform tfRoot;

    class FrameNode
    {
        public string Name;
        public FrameNode Parent;
        public List<FrameNode> Children = new();
        public Vector3 Position;
        public Quaternion Rotation;
        public GameObject GO;
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

        // CreateAxisVisual(rootFrame.GO);
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

        childFrame.Parent = parentFrame;
        childFrame.Position = pos;
        childFrame.Rotation = rot;
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

                SetUnityParent(t, parentT);

                SetUnityPosition(frame, visualization, tagId);
                SetUnityRotation(frame);

                SimpleVisulize(frame.GO);
            }    
        }
    }

    private void SimpleVisulize(GameObject go)
    {
        if (go.transform.childCount > 0) return;
    
        GameObject jointMarker = 
            go.transform.Find("JointMarker") != null ? 
            go.transform.Find("JointMarker").gameObject : 
            GameObject.CreatePrimitive(PrimitiveType.Sphere);
        jointMarker.name = "JointMarker";
        if(jointMarker.transform.parent != go.transform)
            jointMarker.transform.SetParent(go.transform, false);
        jointMarker.transform.position = go.transform.position;
        jointMarker.transform.localScale = Vector3.one * 0.05f;
        jointMarker.GetComponent<Renderer>().material.color = Color.red;
    }

    private void SetUnityParent(Transform child, Transform parent)
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
    
    private void SetUnityRotation(FrameNode frame)
    {
        frame.GO.transform.localRotation = frame.Rotation;
        // t.localRotation = rot;
    }

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
