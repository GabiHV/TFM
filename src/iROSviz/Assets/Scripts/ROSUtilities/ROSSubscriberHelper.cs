using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

using System.Collections;
using System;

using App.Exceptions;

namespace App.ROSUtilities
{
    public class ROSSubscriberHelper
    {
        private static ROSConnection _ros = ROSConnection.GetOrCreateInstance();

        public static void Subscribe(
            string topicName,
            string messgeType,
            Action<Message> callback
        ) =>
            _ros.SubscribeByMessageName(topicName, messgeType, callback);

        public static void Unsubscribe(string topicName) =>
            _ros.Unsubscribe(topicName);
    }   
}
