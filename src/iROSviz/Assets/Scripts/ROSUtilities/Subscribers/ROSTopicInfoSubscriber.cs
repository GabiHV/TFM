using UnityEngine;
using RosMessageTypes.UnityServerInterfaces;

using System.Collections.Generic;
using System.Linq;

using App.ROSUtilities.Resolvers;

namespace App.ROSUtilities.Subscribers
{
    public class ROSTopicInfoSubscriber : ROSSubscriber<Dictionary<string, List<string>>, KeyValuePair<string, List<string>>>
    {
        private static ROSTopicInfoSubscriber _instance;
        private ROSTopicInfoSubscriber() =>
            base.result = new();

        public static Dictionary<string, List<string>> GetTopicsWithTypes() =>
            GetOrCreateInstance().GetResult();

        public static List<string> GetTopicsByType(string messageType) =>
            GetOrCreateInstance().GetResult()
                .Where(
                    t => t.Value.Any(tType => ROSResolver.GetMessageNameWithoutType(tType) == messageType)
                )
                .Select(
                    t => t.Key
                )
                .ToList();

        private static ROSTopicInfoSubscriber GetOrCreateInstance()
        {
            if(_instance == null) _instance = new();
            return _instance;
        }

        protected override void Subscribe() =>
            LoadTopicsWithTypes();

        private void LoadTopicsWithTypes() =>
            base.SubscribeToTopic(
                "/get_topics_unity", 
                new TopicInfoArrayMsg(), 
                msg => LoadTopicsWithTypesCallback((TopicInfoArrayMsg)msg)
            );

        private void LoadTopicsWithTypesCallback(TopicInfoArrayMsg res)
        {
            base.result.Clear();
            
            // Add topic/type to Dictionary
            foreach (TopicInfoMsg topicInfo in res.topics)
            {
                string topic = topicInfo.name;
                List<string> types = topicInfo.types.ToList();
                if(!base.result.ContainsKey(topic))
                    base.result.Add(topic, new List<string>());
                base.result[topic] = types;
            }
        }
    }    
}

