using UnityEngine;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;

using System.Collections.Generic;

namespace App.ROSUtilities.Subscribers
{    
    public class ROSPlannedPathSubscriber : 
        ROSSubscriber<Dictionary<string, Dictionary<string, List<PoseStampedMsg>>>, KeyValuePair<string, Dictionary<string, List<PoseStampedMsg>>>>
    {
        private static string topicToSubscribe;
        private static ROSPlannedPathSubscriber _instance;
        private ROSPlannedPathSubscriber() =>
            base.result = new();

        public static Dictionary<string, Dictionary<string, List<PoseStampedMsg>>> GetPlannedPaths() =>
            GetOrCreateInstance().GetResult();

        public static void SubscribeToTopic(string topic)
        {
            var topics = ROSTopicInfoSubscriber.GetTopicsWithTypes();
            if(!topics.ContainsKey(topic)) return;
            if(!topics[topic].Contains("nav_msgs/msg/Path")) return;

            topicToSubscribe = topic;
            GetOrCreateInstance().LoadPath();
        }

        private static ROSPlannedPathSubscriber GetOrCreateInstance()
        {
            if(_instance == null) _instance = new();
            return _instance;
        }

        protected override void Subscribe() {}
        
        private void LoadPath()
        {
            base.SubscribeToTopic(
                topicToSubscribe,
                new PathMsg(),
                msg => LoadPathsCallback(topicToSubscribe, (PathMsg)msg)
            );
        }

        private void LoadPathsCallback(string topic, PathMsg msg)
        {
            string frameId = msg.header.frame_id;

            if(!base.result.ContainsKey(topic))
                base.result.Add(topic, new());
            if(!base.result[topic].ContainsKey(frameId))
                base.result[topic].Add(frameId, new());
            base.result[topic][frameId].Clear(); // Clearing previous poses
            base.result[topic][frameId].AddRange(msg.poses); // Add new poses
        }
    }
}
