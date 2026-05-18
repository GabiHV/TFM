using UnityEngine;
using RosMessageTypes.Geometry;
using RosMessageTypes.Tf2;

using System.Collections.Generic;

using App.ROSUtilities;

public class TFRoot : MonoBehaviour
{
    Dictionary<string, Dictionary<string, FrameNode>> frames = new();

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
            Dictionary<string, FrameNode> robotFramesDict = robotFrames.Value;
            foreach (var frame in robotFramesDict.Values)
            {
                if (frame.GO == null)
                    continue;

                Transform t = frame.GO.transform;

                if (!(frame.Parent == null) &&
                    robotFramesDict.ContainsKey(frame.Parent.Name))
                {
                    Transform parentT = robotFramesDict[frame.Parent.Name].GO.transform;

                    if (t.parent != parentT)
                        t.SetParent(parentT, false);
                }
                else
                {
                    if (t.parent != tfRoot)
                        t.SetParent(tfRoot, false);
                }

                t.localPosition = frame.Position;
                t.localRotation = frame.Rotation;
            }    
        }
        

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

    
    void CreateAxisVisual(GameObject parent)
    {
        float scale = 0.05f;

        CreateAxis(parent, Vector3.right, Color.red, scale);
        CreateAxis(parent, Vector3.up, Color.green, scale);
        CreateAxis(parent, Vector3.forward, Color.blue, scale);
    }

    void CreateAxis(GameObject parent, Vector3 direction, Color color, float scale)
    {
        GameObject axis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        axis.transform.SetParent(parent.transform, false);

        axis.transform.localScale = new Vector3(scale * 0.2f, scale, scale * 0.2f);
        axis.transform.localPosition = direction * scale;
        axis.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);

        var renderer = axis.GetComponent<Renderer>();
        renderer.material.color = color;

        Destroy(axis.GetComponent<Collider>());
    }

}
