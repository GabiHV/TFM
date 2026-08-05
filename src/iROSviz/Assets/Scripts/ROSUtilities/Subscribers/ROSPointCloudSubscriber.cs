using UnityEngine;
using RosMessageTypes.Sensor;

using System;
using System.Collections.Generic;
using System.Linq;

using App.Utilities;

using App.ROSUtilities.Subscribers;
using App.ROSUtilities.Resolvers;

namespace App.ROSUtilities.Subscribers
{
    public class ROSPointCloudSubscriber : ROSSubscriber<Dictionary<string, PointCloud2Msg>, KeyValuePair<string, PointCloud2Msg>>
    {
        private static ROSPointCloudSubscriber _instance;

        private ROSPointCloudSubscriber() =>
            base.result = new();

        public static Dictionary<string, PointCloud2Msg> GetPointClouds() =>
            GetOrCreateInstance().GetResult();

        private static ROSPointCloudSubscriber GetOrCreateInstance()
        {
            if(_instance == null) _instance = new();
            return _instance;
        }

        protected override void Subscribe() =>
            LoadPointClouds();

        private void LoadPointClouds()
        {
            PointCloud2Msg pCloudMsg = new();
            List<string> topics = ROSTopicInfoSubscriber.GetTopicsByType(pCloudMsg.RosMessageName);

            foreach (var topic in topics)
                base.SubscribeToTopic(
                    topic, 
                    pCloudMsg, 
                    msg => LoadPointCloudCallback(topic, (PointCloud2Msg)msg)
                );
        }

        private void LoadPointCloudCallback(string topic, PointCloud2Msg msg)
        {
            if(base.result.ContainsKey(topic))
                base.result[topic] = msg;
            else
                base.result.Add(topic, msg);
        }
    }
}
