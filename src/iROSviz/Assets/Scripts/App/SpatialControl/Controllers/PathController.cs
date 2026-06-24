using UnityEngine;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

using System;
using System.Collections;
using System.Collections.Generic;

using App.ROSUtilities.Helpers;
using System.Reflection;

public class PathController : MonoBehaviour
{
    private TFRoot.FrameNode frame;
    private string goalTopic;

    private PoseStampedMsg[] poseStampedMsgs;
    private int _msgPtr = 0;
    private Vector3 posTarget;
    private HashSet<Action> notifActions = new();
    private static float error = .05f;
    
    public void SetGoalTopic(string topic) =>
        goalTopic = topic;

    public void SetFrameNode(TFRoot.FrameNode frame) =>
        this.frame = frame; 

    void Start() =>
        StartCoroutine(SendMessages());

    /// <summary>
    /// Adds one notification that will be executed when path is complete.
    /// </summary>
    /// <param name="callback">Action to execute when path is complete</param>
    public void AddNotification(Action callback)
    {
        if(notifActions.Contains(callback)) return;
        notifActions.Add(callback);
    }


    /// <summary>
    /// Deletes a specified notification.
    /// </summary>
    /// <param name="callback">Callback to unset</param>
    public void DeleteNotification(Action callback)
    {
        if(!notifActions.Contains(callback)) return;
        notifActions.Remove(callback);
    }
    
    /// <summary>
    /// Function that executes planned path
    /// </summary>
    /// <param name="path">Positions</param>
    /// <param name="orientations">Rotations</param>
    /// <returns>True if path has been set successfully, false otherwise</returns>
    public bool ExecutePath(Vector3[] path, Quaternion[] orientations)
    {
        if(path.Length != orientations.Length) return false;
        if(ThereAreMoreMessages()) return false;

        PopulateMessageArray(path, orientations);
        return true;
    }

    private void PopulateMessageArray(Vector3[] path, Quaternion[] orientations)
    {
        poseStampedMsgs = new PoseStampedMsg[path.Length];
        _msgPtr = 0;
        for(int i = 0; i < poseStampedMsgs.Length; i++)
        {
            PoseStampedMsg poseStamped = CreatePoseStampedMsg(path[i], orientations[i]);
            poseStampedMsgs[i] = poseStamped;
        }        
    } 

    private PoseStampedMsg CreatePoseStampedMsg(Vector3 position, Quaternion orientation)
    {
        PointMsg point = new(position.x, position.y, position.z);
        QuaternionMsg ori = new(orientation.x, orientation.y, orientation.z, orientation.w);
        PoseMsg pose = new(point, ori);
        PoseStampedMsg poseStamped = new();
        poseStamped.pose = pose;
        poseStamped.header.frame_id = frame.Name;

        return poseStamped;
    }

    private IEnumerator SendMessages()
    {
        while (true)
        {
            PerformMessageSending();
            yield return new WaitForSeconds(.5f);
        }
    }

    private void PerformMessageSending()
    {
        if(IsStampedMsgsNull()) return;

        if(!ThereAreMoreMessages()) 
        {
            NotifyFinishSending();
            return;
        }
        if(!IsFrameInTarget()) return;
        SendNextMessage();
    }

    private bool IsFrameInTarget()
    {
        Vector3 distance = frame.Position - posTarget;
        return distance.magnitude <= error;
    }

    private bool ThereAreMoreMessages() =>
        !IsStampedMsgsNull() && _msgPtr < poseStampedMsgs.Length;

    private bool IsStampedMsgsNull() =>
        poseStampedMsgs == null;

    private void SendNextMessage()
    {
        PoseStampedMsg msg = poseStampedMsgs[_msgPtr++];
        Debug.Log($">> [{nameof(PathController)}] Sending message pose: {msg}");

        float x = (float)msg.pose.position.x;
        float y = (float)msg.pose.position.y;
        float z = (float)msg.pose.position.z;
        posTarget = new Vector3(x, y, z);
        ROSPublisherHelper.PublishMessage(goalTopic, msg);
    }

    private void NotifyFinishSending()
    {
        foreach (Action action in notifActions)
            action();
    }

}
