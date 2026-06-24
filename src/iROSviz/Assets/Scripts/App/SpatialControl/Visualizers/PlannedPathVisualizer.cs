using UnityEngine;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;

using App.Utilities;
using App.ROSUtilities.Subscribers;

using System.Collections.Generic;

public class PlannedPathVisualizer : MonoBehaviour
{
    public GameObject tfRootGO;
    public GameObject plannedPathGO;
    public GameObject plannedMarkerPrefab;
    public TFRoot.FrameNode frame;

    private GameObject[] reusableMarkers;
    private static readonly int maxPath = 10;
    private string tag;
    private string topic;
    private int id;

    void Update()
    {
        SetFrameInfo();
        SubscribeToPathPlanTopic();
        InstantiateMarkerPool();
        UpdatePath();
    }

    private void SetFrameInfo()
    {
        tag = frame?.GetCompleteTag();
        Debug.Log($"Tag: {tag}");
        if(!TagDatabase.TryGetTagId(tag, out id)) return;
        Debug.Log($"ID: {id}");
        if(!TagDatabase.TryGetTagPathVisualizer(id, out topic)) return;
        Debug.Log($"Topic: {topic}");
    }

    private void SubscribeToPathPlanTopic()
    {
        if(string.IsNullOrEmpty(topic)) return;
        Debug.Log($"Subscribed to: {topic}");
        
        ROSPlannedPathSubscriber.SubscribeToTopic(topic);
    }
        
    private void InstantiateMarkerPool()
    {
        if(string.IsNullOrEmpty(topic)) return;
        if(reusableMarkers != null) return;

        reusableMarkers = new GameObject[maxPath];
        for(int i = 0; i < maxPath; i++)
        {
            GameObject go = Instantiate(plannedMarkerPrefab, plannedPathGO.transform);
            go.SetActive(false);
            reusableMarkers[i] = go;
        }
    }

    private void UpdatePath()
    {
            Debug.Log($"Topic: {topic}");

        if(string.IsNullOrEmpty(topic)) return;

        Dictionary<string, Dictionary<string, List<PoseStampedMsg>>> paths = 
            ROSPlannedPathSubscriber.GetPlannedPaths();
        foreach(string key in paths.Keys)
            Debug.Log($"Keys: {key}");
        if(!paths.ContainsKey(topic)) return;
        if(!paths[topic].ContainsKey(frame.Name)) return;
        List<PoseStampedMsg> poses = paths[topic][frame.Name];
        Debug.Log($"Poses in {frame.Name}: {poses.Count}");
        UpdateMarkers(poses);
    }

    private void UpdateMarkers(List<PoseStampedMsg> poses)
    {
        for(int i = 0; i < reusableMarkers.Length; i++)
        {
            if(i >= poses.Count) 
            {
                reusableMarkers[i].SetActive(false);
                continue;
            }

            PoseStampedMsg poseStamped = poses[i];
            PoseMsg pose = poseStamped.pose;
            PointMsg point = pose.position;
            Vector3 position = RosToUnityPosition(point);
            reusableMarkers[i].transform.position = position;
            UpdateLineRenderer(
                i - 1 >= 0 ? reusableMarkers[i - 1].transform : null, 
                reusableMarkers[i].transform
            );
            reusableMarkers[i].SetActive(true);
        }
    }

     Vector3 RosToUnityPosition(PointMsg ros) => 
        new Vector3(
            (float)-ros.y,
            (float)ros.z,
            (float)ros.x
        );

    private void UpdateLineRenderer(Transform parent, Transform child)
    {
        LineRenderer lr = child.GetComponent<LineRenderer>();
        if(lr == null) return;

        lr.positionCount = 2;
        lr.SetPosition(0, parent == null ? plannedPathGO.transform.position : parent.position);
        lr.SetPosition(1, child.position);
    }
}
