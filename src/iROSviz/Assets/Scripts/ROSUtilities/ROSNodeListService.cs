using UnityEngine;
using TMPro;
using RosMessageTypes.Std;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.ROSUtilities
{
    public static class ROSNodeListService
    {
        private static HashSet<string> _nodes = new();

        public static async Task<HashSet<string>> GetOrLoadNodeList()
        {
            if (!IsNodeListEmpty())  return _nodes;

            await LoadNodeList();
            return _nodes;
        }

        public static async Task<HashSet<string>> RefreshNodeList()
        {
            await LoadNodeList();
            return _nodes;
        }

        private static bool IsNodeListEmpty() =>
            _nodes == null || _nodes.Count <= 0;

        private static async Task LoadNodeList() =>
            await ROSServicesHelper.InvokeService(
                "/get_nodes",
                new SetBoolRequest(),
                new SetBoolResponse(),
                msg => LoadNode((SetBoolResponse)msg)
            );
        
        private static void LoadNode(SetBoolResponse res)
        {
           _nodes.Clear();

           string[] nodes =  res.message.Split("\n");
           foreach (string node in nodes)
           {
                if(string.IsNullOrEmpty(node)) continue;
                if(node.Contains("_RosService") || node.Contains("_RosSubscriber")) continue;
                _nodes.Add(node);
           }
        }
    }    
}

