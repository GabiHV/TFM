using UnityEngine;
using RosMessageTypes.UnityServerInterfaces;

using System.Collections.Generic;
using System.Threading.Tasks;


namespace App.ROSUtilities
{   
    public class ROSRobotListService
    {
        private static HashSet<string> _nodes = new();

        public static async Task<HashSet<string>> GetOrLoadRobotList()
        {
            if (!IsRobotListEmpty())  return _nodes;

            await LoadRobotList();
            return _nodes;
        }

        public static async Task<HashSet<string>> RefreshRobotList()
        {
            await LoadRobotList();
            return _nodes;
        }

        private static bool IsRobotListEmpty() =>
            _nodes == null || _nodes.Count <= 0;

        private static async Task LoadRobotList() =>
            await ROSServicesHelper.InvokeService(
                "/get_robots",
                new RobotListSrvRequest(),
                new RobotListSrvResponse(),
                msg => LoadRobots((RobotListSrvResponse)msg)
            );
        
        private static void LoadRobots(RobotListSrvResponse res)
        {
           _nodes.Clear();

           string[] nodes =  res.robot_names;
           foreach (string node in nodes)
           {
                if(string.IsNullOrEmpty(node)) continue;
                if(node.Contains("_RosService") 
                    || node.Contains("_RosSubscriber")
                    || node.Contains("_RosPublisher")) continue;
                _nodes.Add(node);
           }
        }
    }
}