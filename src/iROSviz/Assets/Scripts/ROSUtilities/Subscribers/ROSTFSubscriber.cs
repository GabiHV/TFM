using UnityEngine;
using RosMessageTypes.Tf2;
using RosMessageTypes.UnityServerInterfaces;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App.Utilities;
using App.ROSUtilities.Resolvers;

namespace App.ROSUtilities.Subscribers
{
    public class ROSTFSubscriber : ROSSubscriber<Dictionary<string, TFMessageMsg>, KeyValuePair<string, TFMessageMsg>> 
    {
        public static string TFStaticTopicsIndicator = "_static";
        private static ROSTFSubscriber _instance;

        private ROSTFSubscriber() =>
            base.result = new();

        public static Dictionary<string, TFMessageMsg> GetOrRefreshTF() =>
            GetOrCreateInstance().GetResult();

        private static ROSTFSubscriber GetOrCreateInstance()
        {
            if(_instance == null) _instance = new();
            return _instance;
        }

        protected override void Subscribe()
        {
            LoadStaticTF();
            LoadTFAutomatically();
        }

        private void LoadStaticTF() =>
            base.SubscribeToTopic(
                "/tf_static_unity", 
                new TFStaticArrayMsg(), 
                msg => LoadStaticTFCallback((TFStaticArrayMsg)msg)
            );

        private void LoadTFAutomatically()
        {
            TFMessageMsg tfMsg = new();
            List<string> topics = ROSTopicInfoSubscriber.GetTopicsByType(tfMsg.RosMessageName);

            foreach (var topic in topics)
                base.SubscribeToTopic(
                    topic, 
                    tfMsg, 
                    msg => LoadTFCallback(topic, (TFMessageMsg)msg)
                );   
        }

        private void LoadTFCallback(string topic, TFMessageMsg msg)
        {
            string adjustedTopic = TopicNameHelper.GetSimplifiedName(topic.ToLower());
            if(!base.result.ContainsKey(adjustedTopic)) base.result.Add(adjustedTopic, msg);
            base.result[adjustedTopic] = msg;
        }

        private void LoadStaticTFCallback(TFStaticArrayMsg msg)
        {
            foreach(TFStaticMsg tfStaticObj in msg.tf)
            {
                string topic = TopicNameHelper.GetSimplifiedName(tfStaticObj.topic.ToLower());
                topic = topic.Replace(TFStaticTopicsIndicator, "");
                TFMessageMsg castedMsg = new TFMessageMsg(tfStaticObj.transforms);
                if(!base.result.ContainsKey(topic)) 
                    base.result.Add(topic, castedMsg);
                base.result[topic] = castedMsg;
            }
        }
    }
}

