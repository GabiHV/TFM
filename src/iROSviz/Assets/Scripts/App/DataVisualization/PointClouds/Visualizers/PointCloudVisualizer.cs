using UnityEngine;
using RosMessageTypes.Sensor;

using System.Collections.Generic;
using System.Linq;

using App.Utilities;
using App.ROSUtilities.Resolvers;
using App.ROSUtilities.Services;
using App.ROSUtilities.Subscribers;

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
    private PointCloudData pointCloud;
    private string pointCloudTopic;
    private static string targetFrame;

    private bool active = true;
    
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

        public PointCloudData(GameObject meshPrefab, GameObject pointCloudParent)
        {
            this.mesh = new Mesh();
            this.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            this.pointCloudObj = Instantiate(meshPrefab, pointCloudParent.transform);

            writeBuffer = bufferA;
            readBuffer = bufferB;
            InitMesh();
        }

        private void InitMesh()
        {
            mesh.MarkDynamic();
            mf = this.pointCloudObj.GetComponent<MeshFilter>();
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

        public void ClearMesh()
        {
            this.mesh.Clear();
            this.pointCloudCountRead = 0;
            this.pointCloudCountWrite = 0;
        }
    }

    void Update()
    {
        DisableIfNotInDatabase();
        if(!IsActive()) return;
        InstantiatePointCloudIfNeeded();

        IncrementFrameCount();
        if (!IsRefreshFrame()) return;
        
        PerformVisualization();
        UpdateMesh();
        ResetFrameCount();
    }

    private void DisableIfNotInDatabase()
    {
        string frameName = $"{frame.Root}: {frame.Name}";
        bool inDict = PointCloudDatabase.TryGetPointCloudTopic(frameName, out pointCloudTopic);
        if(!inDict) 
        {
            pointCloud?.ClearMesh();
            active = false;
            return;
        }
        active = true;
    }

    private void InstantiatePointCloudIfNeeded()
    {
        if(pointCloud == null)
            pointCloud = new PointCloudData(meshPrefab, pointCloudParent);
    }

    private bool IsActive() =>
        active;   

    private bool IsRefreshFrame() =>
        frameCount % frameInterval == 0;

    private void IncrementFrameCount() =>
        frameCount++;

    private void ResetFrameCount() =>
        frameCount = 0;

    private void PerformVisualization()
    {
        if (Time.time - lastUpdateTime < updateInterval) return;
        lastUpdateTime = Time.time;

        Dictionary<string, PointCloud2Msg> points = ROSPointCloudSubscriber.GetPointClouds();
        
        if(!points.ContainsKey(pointCloudTopic)) return;
        PointCloud2Msg msg = points[pointCloudTopic];
        pointCloud.ParsePointCloud(msg);
    }

    private void UpdateMesh() =>
        pointCloud.VisualizePoints();
}
