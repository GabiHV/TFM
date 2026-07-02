using UnityEngine;
using RosMessageTypes.UnityServerInterfaces;

using System.Collections.Generic;
using System.Threading.Tasks;

using App.Exceptions;

namespace App.ROSUtilities.Subscribers
{   
    public class ROSRobotInfoSubscriber: ROSSubscriber<HashSet<string>, string>
    {
        private static ROSRobotInfoSubscriber _instance;
        private HashSet<string> _buffA = new();
        private HashSet<string> _buffB = new();
        private HashSet<string> _writeBuff;
        private HashSet<string> _readBuff;

        private ROSRobotInfoSubscriber() // Singleton
        {
            _writeBuff = _buffA;
            _readBuff = _buffB;
            base.result = new();
        }

        public static HashSet<string> GetOrLoadRobotList() =>
            GetOrCreateInstance().GetResult();

        private static ROSRobotInfoSubscriber GetOrCreateInstance()
        {
            if(_instance == null) _instance = new();
            return _instance; 
        }

        protected override void Subscribe() =>
            LoadRobotList();

        public static void NotifyReading() =>
            GetOrCreateInstance().SwapBuff();

        private void LoadRobotList() =>
            base.SubscribeToTopic(
                "/get_robots_unity", 
                new RobotArrayMsg(), 
                msg => LoadRobotsCallback((RobotArrayMsg)msg)
            );
        
        private void LoadRobotsCallback(RobotArrayMsg res)
        {
           _writeBuff.Clear();

           RobotMsg[] robots =  res.robots;
           foreach (RobotMsg robot in robots)
           {
                if(string.IsNullOrEmpty(robot.id)) continue;
                if(!base.IsRealEntity(robot.id)) continue;

                if(_writeBuff.Contains(robot.id)) continue;
                _writeBuff.Add(robot.id);
           }
        }

        private void SwapBuff()
        {
            var temp = _readBuff;
            _readBuff = _writeBuff;
            _writeBuff = temp;
        }
    }
}