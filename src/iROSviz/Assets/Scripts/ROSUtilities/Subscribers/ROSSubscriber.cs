using UnityEngine;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

using System;
using System.Collections.Generic;

using App.ROSUtilities;

namespace App.ROSUtilities.Subscribers
{    
    public abstract class ROSSubscriber<TResult, TItem> : IROSSubscriber<TResult> 
        where TResult : ICollection<TItem>
    {
        private HashSet<string> subscribedTopics = new();
        public TResult result;
        
        public TResult GetResult()
        {
            Subscribe();
            return result;
        }

        public bool HasResult() =>
            result != null && result.Count > 0;

        protected bool IsAlreadySubscribed(string topic) =>
            subscribedTopics.Contains(topic); 

        protected void RegisterTopic(string topic)
        {
            if(IsAlreadySubscribed(topic)) return;
            subscribedTopics.Add(topic);
        }

        protected void SubscribeToTopic(string topic, Message msg, Action<Message>? callback)
        {
            if(IsAlreadySubscribed(topic)) return;

            Debug.Log($"Subscribing to: {topic}");
            ROSSubscriberHelper.Subscribe(
                                    topic,
                                    msg.RosMessageName,
                                    callback
                                );
            RegisterTopic(topic);
        }

        protected bool IsRealEntity(string entityName) =>
            !(entityName.EndsWith("_RosService") ||
                entityName.EndsWith("_RosSubscriber") ||
                entityName.EndsWith("_RosPublisher") ||
                entityName.StartsWith("_"));

        protected abstract void Subscribe();

        protected void UnsubscribeToTopic(string topic) =>
            ROSSubscriberHelper.Unsubscribe(topic);
    }
}
