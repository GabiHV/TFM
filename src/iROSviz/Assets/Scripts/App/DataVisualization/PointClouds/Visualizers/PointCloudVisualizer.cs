using UnityEngine;
using RosMessageTypes.Sensor;

using System.Collections.Generic;
using System.Linq;

using App.ROSUtilities;
using App.Utilities;

public class PointCloudVisualizer : MonoBehaviour
{
    public GameObject meshPrefab;
    public GameObject pointCloudParent;
    public TFRoot.FrameNode frame;

    private int frameCount = 0;
    private const int frameInterval = 15;
    private const float updateInterval = 0.1f;
    private float lastUpdateTime = 0f;
    private const int maxPoints = 50000;
    private const int step = 100;
    private Dictionary<string, PointCloudData> pointClouds = new();
    private HashSet<string> pointCloudTopics = new();
    private HashSet<string> pointCloudsTopicsSubscribed = new();
    private static string pointCloudMessageName = new PointCloud2Msg().RosMessageName;
    private static string targetFrame;

    class PointCloudData
    {
        private Mesh mesh;
        private GameObject pointCloudObj;
        private bool isUpdated;

        private MeshFilter mf;
        private Vector3[] bufferA = new Vector3[maxPoints];
        private Vector3[] bufferB = new Vector3[maxPoints];
        private Vector3[] writeBuffer;
        private Vector3[] readBuffer;
        private int pointCloudCountWrite;
        private int pointCloudCountRead;
        private int[] indices = new int[maxPoints];

        public PointCloudData(Mesh mesh, GameObject pointCloudObj)
        {
            this.mesh = mesh;
            this.pointCloudObj = pointCloudObj;
            writeBuffer = bufferA;
            readBuffer = bufferB;
            InitMesh();
        }

        private void InitMesh()
        {
            mesh.MarkDynamic();
            MeshFilter mf = this.pointCloudObj.GetComponent<MeshFilter>();
            mf.mesh = this.mesh;
        }

        public void ParsePointCloud(PointCloud2Msg msg)
        {
            
            ProcessMessage(msg);
            Downsample();
            SwapBuffers();
            MarkAsUpdated();
        }

        private void ProcessMessage(PointCloud2Msg msg)
        {
            targetFrame = msg.header.frame_id;
            byte[] data = msg.data;
            int pointStep = (int)msg.point_step;
            int totalPoints = (int)(msg.width * msg.height);

            int step = Mathf.Max(1, totalPoints / maxPoints);

            int count = 0;

            for (int i = 0; i < totalPoints; i += step)
            {
                if (count >= maxPoints) break;

                int offset = i * pointStep;

                float x = ReadFloat(data, offset + 0);
                float y = ReadFloat(data, offset + 4);
                float z = ReadFloat(data, offset + 8);

                Vector3 p = new Vector3(-y, z, x);
                if (p.magnitude > 20f) continue;

                this.writeBuffer[count++] = p;
            }
            this.pointCloudCountWrite = count;
        }

        unsafe float ReadFloat(byte[] data, int offset)
        {
            fixed (byte* ptr = &data[offset])
            {
                return *(float*)ptr;
            }
        }

        private void Downsample()
        {
            int j = 0;
            for (int i = 0; i < this.pointCloudCountWrite; i += step)
            {
                this.writeBuffer[j++] = this.writeBuffer[i];
            }
            this.pointCloudCountWrite = j;
        }

        private void SwapBuffers()
        {
            Vector3[] temp = readBuffer;
            readBuffer = writeBuffer;
            writeBuffer = temp;
            pointCloudCountRead = pointCloudCountWrite;
        }

        private void MarkAsUpdated() =>
            this.isUpdated = true;

        public void VisualizePoints()
        {
            if(!CheckIfUpdated()) return;
            UpdateMesh();
            MarkAsUnupdated();
        }

        private bool CheckIfUpdated() =>
            isUpdated;

        private void UpdateMesh()
        {
            for (int i = 0; i < this.pointCloudCountRead; i++)
                this.indices[i] = i;

            this.mesh.SetVertices(this.readBuffer, 0, this.pointCloudCountRead);
            this.mesh.SetIndices(this.indices, 0, this.pointCloudCountRead, MeshTopology.Points, 0);
        }

        private void MarkAsUnupdated() =>
            isUpdated = false;
    }

    private void DisableIfNotMainFrame()
    {
        TagDatabase.TryGetTagId(frame.Name, out int id);
        TagDatabase.TryGetTagVisualization(id, out string visualization);

        if(!string.IsNullOrEmpty(targetFrame))
            this.enabled = targetFrame == frame.Name;
    }

    void Update()
    {
        DisableIfNotMainFrame();
        IncrementFrameCount();
        if (!IsRefreshFrame()) return;
        
        LoadPointCloudTopics();
        PerformVisualization();
        UpdateMeshes();
        ResetFrameCount();
    }

    private void LoadPointCloudTopics()
    {
        pointCloudTopics = ROSTopicListService.GetTopicsWithTypes().
            Where(
                    kvp => 
                        kvp.Value.Select(v => ROSResolver.GetMessageNameWithoutType(v)).Contains(pointCloudMessageName) && 
                        kvp.Key.Contains(frame.Name) &&
                        kvp.Key.Contains(frame.Root)
                ).
            Select(kvp => kvp.Key).
            ToHashSet();
    }

    private bool IsRefreshFrame() =>
        frameCount % frameInterval == 0;

    private void IncrementFrameCount() =>
        frameCount++;

    private void ResetFrameCount() =>
        frameCount = 0;

    private void PerformVisualization()
    {
        foreach (string topic in pointCloudTopics)
        {
            if (pointCloudsTopicsSubscribed.Contains(topic)) continue;
            ROSPointCloudSubscriber.SubscribeToPointCloudTopic(
                topic, 
                LoadPointCloudCallback
            );
            pointCloudsTopicsSubscribed.Add(topic);
        }
    }

    private void LoadPointCloudCallback(string topic, PointCloud2Msg msg)
    {
        if (Time.time - lastUpdateTime < updateInterval) return;
        lastUpdateTime = Time.time;
        PointCloudData pointCloudData = GetOrCreatePointCloud(topic);
        pointCloudData.ParsePointCloud(msg);
    }

    private PointCloudData GetOrCreatePointCloud(string topic)
    {
        if (pointClouds.ContainsKey(topic)) return pointClouds[topic];
        
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        PointCloudData pointCloudData = new PointCloudData
        (
            mesh,
            Instantiate(meshPrefab, pointCloudParent.transform)
        );
        pointClouds.Add(topic, pointCloudData);
        return pointCloudData;
    }

    private void UpdateMeshes()
    {
        foreach (var (_, pointCloudData) in pointClouds)
            pointCloudData.VisualizePoints();
    }
}
