using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

using System.Collections;

using App.Exceptions;
using App.ROSUtilities.Resolvers;


namespace App.ROSUtilities.Helpers
{
    public static class ROSPublisherHelper
    {
        public static ROSConnection _ros = ROSConnection.GetOrCreateInstance();

        /// <summary>
        /// Function that publish a message into a topic
        /// </summary>
        /// <param name="topicName">Topic name</param>
        /// <param name="messageType">Message type</param>
        /// <param name="message">Message</param>
        public static void PublishMessage(
            string topicName, 
            string messageType, 
            string message
        )
        {
            try
            {
                if(string.IsNullOrEmpty(message)) return;
                Message messageObj = ROSResolver.InstantiateMessage(messageType, message);

                RosTopicState topicState = _ros.RegisterPublisher(
                    topicName, 
                    messageObj.RosMessageName
                );
                
                _ros.Publish(topicName, messageObj);
            }
            catch (JSONParseException ex)
            {
                Debug.LogWarning(ex.message);
            }
        }

        /// <summary>
        /// Function that publiush a message into a topic
        /// </summary>
        /// <param name="topicName">Topic name</param>
        /// <param name="message">Message object</param>
        public static void PublishMessage(
            string topicName,
            Message message
        )
        {
            RosTopicState topicState = _ros.RegisterPublisher(
                    topicName, 
                    message.RosMessageName
                );
                
            _ros.Publish(topicName, message);
        }
    }    
}

