using UnityEngine;

namespace App.Utilities
{
    public static class TopicNameHelper
    {
        public static string GetSimplifiedName(string topic) =>
            OutputHelper.RemoveStarter(topic, "/").Replace("/", "_");
        
        public static string GetCanonicalName(string simplified_topic) =>
            OutputHelper.AddStarter(simplified_topic, "/").Replace("_", "/");
    }
}
