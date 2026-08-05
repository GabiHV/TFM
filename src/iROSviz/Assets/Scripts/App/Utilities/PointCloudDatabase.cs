using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace App.Utilities
{    
    [System.Serializable]
    public class PointCloudDatabase
    {
        class PointCloudInfo
        {
            public string topic;
            public string frame;
        }

        private static Dictionary<string, PointCloudInfo> pointClouds = new();
        private static bool isLoaded = false;

        public static void StorePointCloud(string name, string topic, string frame)
        {
            if(pointClouds.ContainsKey(name))
                pointClouds[name] = new PointCloudInfo { topic = topic, frame = frame };
            else
                pointClouds.Add(name, new PointCloudInfo { topic = topic, frame = frame });
        }

        public static void DeletePointCloud(string name) 
        {
            LoadDatabase();
            pointClouds.Remove(name);
        }

        public static bool TryGetPointCloudTopic(string name, out string topic)
        {
            LoadDatabase();
            return pointClouds.TryGetValue(name, out PointCloudInfo info) ? (topic = info.topic) != null : (topic = null) != null;
        }

        public static bool TryGetPointCloudFrame(string name, out string frame)
        {
            LoadDatabase();
            return pointClouds.TryGetValue(name, out PointCloudInfo info) ? (frame = info.frame) != null : (frame = null) != null;
        }

        static readonly string POINTCLOUD_DB_PATH = Application.persistentDataPath + "/point_clouds_database.json";

        public static void SaveDatabase()
        {
            string json = JsonConvert.SerializeObject(pointClouds);
            File.WriteAllText(POINTCLOUD_DB_PATH, json);
            Debug.Log($"PointCloud database saved to {POINTCLOUD_DB_PATH}");
        }

        public static void LoadDatabase()
        {
            try
            {
                if(isLoaded) return;
                if(!File.Exists(POINTCLOUD_DB_PATH))
                    return;

                string json = File.ReadAllText(POINTCLOUD_DB_PATH);
                pointClouds = 
                    JsonConvert.DeserializeObject<Dictionary<string, PointCloudInfo>>(json);
                isLoaded = true;
                Debug.Log($"PointCloud database loaded from {POINTCLOUD_DB_PATH}. Loaded {pointClouds.Count} point clouds.");
            }
            catch(Exception ex)
            {
                Debug.LogError($"Error loading point cloud database: {ex.Message}");
            }
        }
    }
}
