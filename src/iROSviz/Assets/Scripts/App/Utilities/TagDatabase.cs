using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace App.Utilities
{
    [System.Serializable]
    public static class TagDatabase
    {
        class TagInfo
        {
            public string name;
            public string frame;
            public string pathVisualizer;
        }
        private static Dictionary<int, TagInfo> tags = new();
        static readonly string APRILTAG_DB_PATH = Application.persistentDataPath + "/april_tags_database.json";

        public static void SaveDatabase()
        {
            string json = JsonConvert.SerializeObject(tags);
            File.WriteAllText(APRILTAG_DB_PATH, json);
            Debug.Log($"Tags database saved to {APRILTAG_DB_PATH}");
        }

        public static void LoadDatabase()
        {
            try
            {
                if(!File.Exists(APRILTAG_DB_PATH))
                    return;

                string json = File.ReadAllText(APRILTAG_DB_PATH);
                tags = 
                    JsonConvert.DeserializeObject<Dictionary<int, TagInfo>>(json);
                
                Debug.Log($"Tags database loaded from {APRILTAG_DB_PATH}. Loaded {tags.Count} tags.");
            }
            catch(Exception ex)
            {
                Debug.LogError($"Error loading tags database: {ex.Message}");
            }
        }

        public static void StoreTag(int id, string name, string frame, string pathVisualizer)
        {
            if(tags.ContainsKey(id))
                tags[id] = new TagInfo { name = name, frame = frame, pathVisualizer =  pathVisualizer};
            else
                tags.Add(id, new TagInfo { name = name, frame = frame, pathVisualizer = pathVisualizer });
        }

        public static void DeleteTag(int id) => tags.Remove(id);

        public static bool TryGetTagName(int id, out string name) =>
            tags.TryGetValue(id, out TagInfo tagInfo) ? (name = tagInfo.name) != null : (name = null) != null;

        public static bool TryGetTagFrame(int id, out string frame) =>
            tags.TryGetValue(id, out TagInfo tagInfo) ? (frame = tagInfo.frame) != null : (frame = null) != null;

        public static bool TryGetTagPathVisualizer(int id, out string pathTopic) =>
            tags.TryGetValue(id, out TagInfo tagInfo) ? (pathTopic = tagInfo.pathVisualizer) != null : (pathTopic = null) != null;

        public static bool TryGetTagId(string name, out int id)
        {
            foreach(var item in tags)
            {
                if(item.Value.name == name)
                {
                    id = item.Key;
                    return true;
                }
            }
            id = -1;
            return false;
        }

        public static List<string> GetAllTagNames() => tags.Values.Select(ti => ti.name).ToList();
    }
}
