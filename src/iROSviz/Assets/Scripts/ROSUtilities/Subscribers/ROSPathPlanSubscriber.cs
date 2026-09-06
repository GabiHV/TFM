using UnityEngine;
using RosMessageTypes.UnityServerInterfaces;

using System;
using System.Collections.Generic;
using System.Linq;

using App.ROSUtilities.Resolvers;

namespace App.ROSUtilities.Subscribers
{
    public class ROSPlanPathSubscriber : ROSSubscriber<HashSet<Int16>, Int16>
    {
        private static ROSPlanPathSubscriber _instance;
        private ROSPlanPathSubscriber() =>
            base.result = new();

        public static HashSet<Int16> GetFinishedTransactions() =>
            GetOrCreateInstance().GetResult();

        private static ROSPlanPathSubscriber GetOrCreateInstance()
        {
            if(_instance == null) _instance = new();
            return _instance;
        }

        protected override void Subscribe() =>
            LoadTopicsWithTypes();

        private void LoadTopicsWithTypes() =>
            base.SubscribeToTopic(
                "/goal_pose_status_unity", 
                new Int16ArrayMsg(), 
                msg => LoadFinishedTransactionsCallback((Int16ArrayMsg)msg)
            );

        private void LoadFinishedTransactionsCallback(Int16ArrayMsg res)
        {
            base.result.Clear();
            
            // Add topic/type to Dictionary
            foreach (Int16 i in res.data)
            {
                base.result.Add(i);
            }
        }
    }    
}

