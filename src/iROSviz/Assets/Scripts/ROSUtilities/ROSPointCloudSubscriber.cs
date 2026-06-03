using UnityEngine;
using RosMessageTypes.Sensor;

using System;
using System.Collections.Generic;
using System.Linq;

using App.Utilities;

namespace App.ROSUtilities
{
    public class ROSPointCloudSubscriber
    {
        private static Dictionary<string, PointCloud2Msg>  _pointClouds = new();
        private static HashSet<string> subscribedTopics = new();

        public static Dictionary<string, PointCloud2Msg> GetPointClouds()
        {
            if(IsPointCloudDictEmpty()) return _pointClouds;

            LoadPointCloudsAutomatically();

            return _pointClouds;
        }

        private static Dictionary<string, PointCloud2Msg> RefreshPointClouds()
        {
            LoadPointCloudsAutomatically();

            return _pointClouds;
        }

        public static void SubscribeToPointCloudTopic(
            string topic, 
            Action<string, PointCloud2Msg> callback
        )
        {
            // if(ROSTopicListService.GetTopicsWithTypes()[topic].Any(t => ROSResolver.GetMessageNameWithoutType(t) == new PointCloud2Msg().RosMessageName))
            //     return;
            string messageName = "sensor_msgs/PointCloud2";
            ROSSubscriberHelper.Subscribe(
                                    topic,
                                    messageName,
                                    msg => {
                                        callback(topic, (PointCloud2Msg)msg);
                                        // ROSSubscriberHelper.Unsubscribe(topic);
                                    }
                                );
        }

        private static bool IsPointCloudDictEmpty() =>
            _pointClouds != null && _pointClouds.Count > 0;

        private static void LoadPointCloudsAutomatically()
        {
            Dictionary<string, List<string>> topics = ROSTopicListService.GetTopicsWithTypes();

            foreach (var topic in topics)
            {
                if(!topic.Value.Any(t => ROSResolver.GetMessageNameWithoutType(t) == new PointCloud2Msg().RosMessageName))
                    continue;
                if(subscribedTopics.Contains(topic.Key)) continue;
                subscribedTopics.Add(topic.Key);
                
                ROSSubscriberHelper.Subscribe(
                                        topic.Key,
                                        new PointCloud2Msg().RosMessageName,
                                        msg => LoadPointCloudCallback(topic.Key, (PointCloud2Msg)msg)
                                    );
            }
        }

        private static void LoadPointCloudCallback(string topic, PointCloud2Msg msg)
        {
            if(_pointClouds.ContainsKey(topic))
                _pointClouds[topic] = msg;
            else
                _pointClouds.Add(topic, msg);
            
            ROSSubscriberHelper.Unsubscribe(topic);
            subscribedTopics.Remove(topic);
        }
    }
}
