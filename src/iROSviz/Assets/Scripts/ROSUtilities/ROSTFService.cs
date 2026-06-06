using UnityEngine;
using RosMessageTypes.Tf2;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App.Utilities;

namespace App.ROSUtilities
{
    public class ROSTFService
    {
        private static Dictionary<string, TFMessageMsg>  _tfMessages = new();
        private static HashSet<string> subscribedTopics = new();
        private static Task currentServiceInvoke;

        public static Dictionary<string, TFMessageMsg> GetTF()
        {
            if(IsTFDictEmpty()) return _tfMessages;

            LoadTFAutomatically();

            return _tfMessages;
        }

        private static bool IsTFDictEmpty() =>
            _tfMessages != null && _tfMessages.Count > 0;

        public static Dictionary<string, TFMessageMsg> RefreshTF()
        {
            LoadTFAutomatically();

            return _tfMessages;
        }

        private static void LoadTFAutomatically()
        {
            Dictionary<string, List<string>> topics = ROSTopicListService.GetTopicsWithTypes();

            foreach (var topic in topics)
            {
                if(!topic.Key.Contains("tf")) continue;
                if(!topic.Value.Any(t => ROSResolver.GetMessageNameWithoutType(t) == new TFMessageMsg().RosMessageName))
                    continue;
                if(subscribedTopics.Contains(topic.Key)) continue;
                subscribedTopics.Add(topic.Key);
                
                ROSSubscriberHelper.Subscribe(
                                        topic.Key,
                                        new TFMessageMsg().RosMessageName,
                                        msg => LoadTFCallback(topic.Key, (TFMessageMsg)msg)
                                    );
            }
        }

        private void LoadTFBySelectedTFTopics()
        {
            Dictionary<string, List<string>> topics = ROSTopicListService.GetTopicsWithTypes();

            GameConfig config = ConfigHelper.GetConfig();
            foreach (string topic in config.ROS_TF_TOPICS)
            {
                if(!topics[topic].Any(t => ROSResolver.GetMessageNameWithoutType(t) == new TFMessageMsg().RosMessageName))
                    continue;

                if(subscribedTopics.Contains(topic)) continue;
                subscribedTopics.Add(topic);
                
                ROSSubscriberHelper.Subscribe(
                                        topic,
                                        new TFMessageMsg().RosMessageName,
                                        msg => LoadTFCallback(topic, (TFMessageMsg)msg)
                                    );
            }
        }

        private static void LoadTFCallback(string topic, TFMessageMsg msg)
        {
            if(!_tfMessages.ContainsKey(topic)) _tfMessages.Add(topic, msg);
            _tfMessages[topic] = msg;
        }

        public static List<string> GetTFFrames(string namesp)
        {
            List<string> frames = new();
            foreach (var tf in _tfMessages)
            {
                if(tf.Key.Contains(namesp))
                    frames.AddRange(tf.Value.transforms.Select(t => t.child_frame_id));
            }
            return frames;
        }
    }
}

