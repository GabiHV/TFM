using UnityEngine;
using RosMessageTypes.Std;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.ROSUtilities
{
    public static class ROSTopicListService
    {
        private static Dictionary<string, List<string>>  _topicsMessages = new();
        private static Task currentServiceInvoke;

        public static Dictionary<string, List<string>> GetTopicsWithTypes()
        {
            if(IsTopicDictEmpty()) return _topicsMessages;

            LoadTopicsWithTypes();

            return _topicsMessages;
        }
        
        private static bool IsTopicDictEmpty() =>
            _topicsMessages != null && _topicsMessages.Count > 0;

        public static Dictionary<string, List<string>> RefreshTopicsWithTypes()
        {
            LoadTopicsWithTypes();

            return _topicsMessages;
        }

        private static void LoadTopicsWithTypes()
        {
            if(currentServiceInvoke == null || currentServiceInvoke.IsCompleted) 
                currentServiceInvoke = ROSServicesHelper.InvokeService("/get_topics", 
                    new SetBoolRequest(),
                    new SetBoolResponse(),
                    msg => LoadTopicsWithTypesCallback((SetBoolResponse)msg));
        }
            

        private static void LoadTopicsWithTypesCallback(SetBoolResponse res)
        {
            _topicsMessages.Clear();
            
            string[] topicsWithTypes = res.message.Split("\n");

            // Add topic/type to Dictionary
            foreach (string topicType in topicsWithTypes)
            {
                if(string.IsNullOrEmpty(topicType)) continue;

                string[] parts = topicType.Split(":");
                if(parts.Length < 2) continue;

                string topic = parts[0];
                string typesString = parts[1];

                // Types list has the form ['message_type1', 'message_type2', ...]
                string[] types = typesString.Replace("[", "").Replace("]", "").Split(",");
                List<string> typesList = new();
                foreach (string type in types)
                {
                    typesList.Add(type.Replace("'", ""));
                }

                _topicsMessages.Add(topic, typesList);
            }
        }
    }    
}

