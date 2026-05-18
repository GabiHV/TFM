using UnityEngine;
using RosMessageTypes.Tf2;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.ROSUtilities
{
    public class ROSTFService
    {
        private static Dictionary<string, TFMessageMsg>  _tfMessages = new();
        private static Task currentServiceInvoke;

        public static Dictionary<string, TFMessageMsg> GetTF()
        {
            if(IsTFDictEmpty()) return _tfMessages;

            LoadTF();

            return _tfMessages;
        }
        
        private static bool IsTFDictEmpty() =>
            _tfMessages != null && _tfMessages.Count > 0;

        public static Dictionary<string, TFMessageMsg> RefreshTF()
        {
            LoadTF();

            return _tfMessages;
        }

        private static void LoadTF()
        {
            Dictionary<string, List<string>> topics = ROSTopicListService.GetTopicsWithTypes();

            foreach (string topic in topics.Keys)
            {
                if(!topic.Contains("tf")) continue;

                if(CheckIfTopicHasType(topics[topic])) 
                    ROSSubscriberHelper.Subscribe(
                                            topic, 
                                            new TFMessageMsg().RosMessageName, 
                                            msg => LoadTFCallback(topic, (TFMessageMsg)msg)
                                        );
            }
        }

        private static bool CheckIfTopicHasType(List<string> topicTypes) =>
            topicTypes.
                Any(t => ROSResolver.GetMessageNameWithoutType(t) == new TFMessageMsg().RosMessageName);
            
        private static void LoadTFCallback(string topic, TFMessageMsg msg)
        {
            if(!_tfMessages.ContainsKey(topic)) _tfMessages.Add(topic, msg);
            _tfMessages[topic] = msg;
        }
    }    
}

