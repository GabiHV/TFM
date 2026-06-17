using UnityEngine;
using TMPro;
using RosMessageTypes.UnityServerInterfaces;

using System.Collections.Generic;

namespace App.ROSUtilities.Subscribers
{
    public class ROSNodeInfoSubscriber : ROSSubscriber<HashSet<string>, string>
    {
        private static ROSNodeInfoSubscriber _instance; 

        private ROSNodeInfoSubscriber() =>
            base.result = new();

        public static HashSet<string> GetOrLoadNodeList() =>
            GetOrCreateInstance().GetResult();

        private static ROSNodeInfoSubscriber GetOrCreateInstance()
        {
            if (_instance == null)  _instance = new();

            return _instance;
        }

        protected override void Subscribe() =>
            LoadNodeList();

        private void LoadNodeList()
        {
            string topic = "/get_nodes_unity";
            if(base.IsAlreadySubscribed(topic)) return;

            ROSSubscriberHelper.Subscribe(
                topic,
                new NodeInfoArrayMsg().RosMessageName,
                msg => LoadNodeCallback((NodeInfoArrayMsg)msg)
            );
            base.RegisterTopic(topic);
        }
            
        private void LoadNodeCallback(NodeInfoArrayMsg res)
        {
           base.result.Clear();

           foreach (NodeInfoMsg node in res.nodes)
           {
                string name = node.name;
                if(name.Contains("_RosService") || name.Contains("_RosSubscriber")) continue;
                base.result.Add(name);
           }
        }
    }    
}

