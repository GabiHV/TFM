using UnityEngine;
using RosMessageTypes.Geometry;
using RosMessageTypes.Tf2;

using System;
using System.Collections.Generic;
using System.Linq;

using App.Utilities;
using App.Utilities.Collections;
using App.ROSUtilities.Subscribers;

public class TFRoot : MonoBehaviour
{
    private Dictionary<string, Dictionary<string, FrameNode>> frames = new();

    private GameObject TFRootGO;

    public class FrameNode
    {
        public string Name;
        public string Root;
        public FrameNode Parent;
        public List<FrameNode> Children = new();
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        public GameObject GO;

        public float LastUpdateTime;
        public float UpdateInterval;
        public float SmoothedHZ;

        private const int frameQueueCap = 20;
        public RingBuffer<FrameNode> Hist = new(frameQueueCap);

        public void StoreHist()
        {
            FrameNode copy = new FrameNode
            {
              Name = this.Name,
              Root = this.Root,
              Parent = this.Parent,
              Position = this.Position,
              Rotation = this.Rotation,
              Velocity = this.Velocity,
              AngularVelocity = this.AngularVelocity,
              GO = this.GO,
              LastUpdateTime = this.LastUpdateTime,
              UpdateInterval = this.UpdateInterval,
              SmoothedHZ = this.SmoothedHZ,
              Hist = null,
            };

            Hist.Push(copy);
        }

        public void UpdateDescendants(FrameNode parent)
        {
            if(this.Parent == null)
                this.Parent = parent;
            if(!parent.Children.Contains(this))
                parent.Children.Add(this);
        }

        public void UpdatePosition(Vector3 pos) =>
            this.Position = pos;

        public void UpdateRotation(Quaternion rot) =>
            this.Rotation = rot;

        public void UpdateFrequency()
        {
            float now = Time.time;

            float deltaT = now - this.LastUpdateTime;
            this.UpdateInterval = deltaT;

            float instantHz = 1.0f / Mathf.Max(deltaT, 0.0001f);

            // Smoothing
            this.SmoothedHZ = Mathf.Lerp(this.SmoothedHZ, instantHz, 0.1f);

            this.LastUpdateTime = now;
        }

        public void UpdateVelocity()
        {
            LinealVelocityUpdate();
            AngularVelocityUpdate();
        }

        public List<Vector3> GetHistPosition() =>
            Hist.GetAll().Select(e => e.Position).ToList();

        public List<Vector3> GetHistPosition(Vector3 anchor) =>
            Hist.GetAll().Select(e => e.Position + anchor).ToList();

        public List<Quaternion> GetHistRotation() =>
            Hist.GetAll().Select(e => e.Rotation).ToList();

        public float GetAvgDeltaTime() =>
            Hist.GetAll().Select(e => e.UpdateInterval).Average();

        private void LinealVelocityUpdate()
        {
            FrameNode hist = GetLastFrameNode();
            if(hist == null) return;

            float now = this.LastUpdateTime;
            float dt = now - hist.LastUpdateTime;

            if (dt < 0.0001f) return;

            Vector3 deltaPos = this.Position - hist.Position;
            this.Velocity = deltaPos / dt;
        }

        private void AngularVelocityUpdate()
        {
            FrameNode hist = GetLastFrameNode();
            if(hist == null) return;
    
            float now = this.LastUpdateTime;
            float dt = now - hist.LastUpdateTime;

            if (dt < 0.0001f) return;

            Quaternion delta = this.Rotation * Quaternion.Inverse(hist.Rotation);

            delta.ToAngleAxis(out float angleDeg, out Vector3 axis);

            float angleRad = angleDeg * Mathf.Deg2Rad;

            this.AngularVelocity = axis * (angleRad / dt);
        }

        private FrameNode GetLastFrameNode() =>
            Hist.Peek();
        
        public string GetCompleteTag() =>
            $"{this.Root}: {this.Name}";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    {
        SetTFRootGO();
        ProcessTF();
    }

    // Update is called once per frame
    void Update() =>
        ProcessTF();

    private void ProcessTF()
    {
        PerformTFMessageProcessing();
        StoreFramesInHist();
    }   

    private void SetTFRootGO() =>
        TFRootGO = this.gameObject;

    private void PerformTFMessageProcessing()
    {
        Dictionary<string, TFMessageMsg> tfInfo = ROSTFSubscriber.GetOrRefreshTF();  
        foreach (var (root, msg) in tfInfo)
        {
            UpdateOrCreateRootFrame(root);

            foreach (var tf in msg.transforms)
            {
                CreateFrameAndUpdateDescendants(root, tf);
                UpdateFrameParameters(root, tf);   
            }
        }
    }

    private void StoreFramesInHist()
    {
        foreach(var (_, frameNode) in frames)
            foreach (var (_, frame) in frameNode)
                frame.StoreHist();
    }

    private void UpdateOrCreateRootFrame(string root = "default")
    {
        if (frames.ContainsKey(root)) return;
        
        frames.Add(root, new Dictionary<string, FrameNode>());
    }

    private void CreateFrameAndUpdateDescendants(string root, TransformStampedMsg tf)
    {
        string parent = tf.header.frame_id;
        string child  = tf.child_frame_id;

        FrameNode parentFrame = GetOrCreateFrame(root, parent);
        FrameNode childFrame  = GetOrCreateFrame(root, child);

        childFrame.UpdateDescendants(parentFrame);
    }

    private void UpdateFrameParameters(string root, TransformStampedMsg tf)
    {
        string child  = tf.child_frame_id;

        Vector3 pos = RosToUnityPosition(tf.transform.translation);
        Quaternion rot = RosToUnityRotation(tf.transform.rotation);

        FrameNode frame = GetOrCreateFrame(root, child);
        frame.UpdateFrequency();
        frame.UpdateVelocity();
        frame.UpdatePosition(pos);
        frame.UpdateRotation(rot);
    }

    private FrameNode GetOrCreateFrame(string root, string frame)
    {
        if (frames[root].ContainsKey(frame))
            return frames[root][frame];
        
        FrameNode frameNode = new()
            {
                Name = frame,
                Root = root,
                GO = new GameObject(frame)
            };
        PathController pc = frameNode.GO.AddComponent<PathController>();
        pc.SetFrameNode(frameNode);

        frames[root][frame] = frameNode;

        return frameNode;
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

    public Dictionary<string, Dictionary<string, FrameNode>> GetFramesByTopic() =>
        frames;

}
