
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using Newtonsoft.Json;

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace App.Utilities
{
    public class FiducialSystemFrameProcessor : MonoBehaviour
    {
        static readonly int frameGap = 10;
        static int width = 800;
        static int height = 600;
        
        int frameCounter = 0;
        byte[] rgbaBuffA;
        byte[] rgbaBuffB;
        byte[] grayBuff;
        byte[] flippedImg;
        RenderTexture render;
        Texture2D texture;
        Rect rect;
        public static DetectionResult lastResult = new();
        public static Dictionary<int, Vector3> tagAnchors = new(); 
        public ARRaycastManager raycastManager;    

        // Semaphores
        volatile bool resultReady = false;
        volatile bool bufferAReady = false;
        volatile bool bufferBReady = false;
        volatile bool useBufferA = false;

        IEnumerator Awake()
        {
            yield return new WaitUntil(() => GetComponent<ARRaycastManager>() != null);
            raycastManager = GetComponent<ARRaycastManager>();
        }

        void Start()
        {
            GameConfig config = ConfigHelper.GetConfig();
            ConfigHelper.ChangeFiduciarySys(config);

            width = Screen.width;
            height = Screen.height;
            
            rgbaBuffA = new byte[width * height * 4]; // 4 bytes per pixel (R, G, B, A)
            rgbaBuffB = new byte[width * height * 4]; // 4 bytes per pixel (R, G, B, A)
            grayBuff = new byte[width * height];
            flippedImg =  new byte[grayBuff.Length];
            
            render = new(width, height, 24);
            texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            rect = new Rect(0, 0, width, height);

            TagDatabase.LoadDatabase();

            Thread worker = new Thread(ProcessFrame);
            worker.IsBackground = true;
            worker.Start();
        }

        void Update()
        {
            if(frameCounter++ % frameGap == 0) {
                CaptureFrame();
                frameCounter = 0;
            }

            if(resultReady) StartCoroutine(ProcessAprilTags());
        }

        private void CaptureFrame()
        {
            RenderTexture.active = render;
            Camera.main.targetTexture = render;
            Camera.main.Render();

            texture.ReadPixels(rect, 0, 0);
            texture.Apply();

            AsyncGPUReadback.Request(texture, 0, TextureFormat.RGBA32, OnCompleteReadback);

            Camera.main.targetTexture = null;
            RenderTexture.active = null;
            render.Release();
        }

        private void OnCompleteReadback(AsyncGPUReadbackRequest req)
        {
            if(req.hasError) return;

            var data = req.GetData<byte>();
            byte[] buff = useBufferA ? rgbaBuffA : rgbaBuffB;

            data.CopyTo(buff);
            if(useBufferA)
                bufferAReady = true;
            else
                bufferBReady = true;

            useBufferA = !useBufferA;
        }

        private void ProcessFrame()
        {
            while (true)
            {
                if(!bufferAReady || !bufferBReady)
                {
                    Thread.Sleep(1);
                    continue;
                }

                ConvertToGrayscale();

                if(useBufferA)
                    bufferAReady = false;
                else 
                    bufferBReady = false;
                
                lastResult = DetectAprilTags();

                resultReady = true;      
            }
        }

        private void ConvertToGrayscale()
        {
            byte[]  buffer = useBufferA ? rgbaBuffA : rgbaBuffB; // If buff A is busy then use B and viceversa
            // Grayscale
            for (int i = 0, p = 0; i < grayBuff.Length; i++, p += 4)
                grayBuff[i] = (byte)(
                    (0.299f * buffer[p] + 0.587 * buffer[p + 1] + 0.114f * buffer[p + 2])
                );

            // Unity delivers images flipped
            for(int i = 0; i < height; i++)
                Buffer.BlockCopy(grayBuff, i * width, flippedImg, (height - 1 - i) * width, width);

            Array.Copy(flippedImg, grayBuff, flippedImg.Length);
        }

        private DetectionResult DetectAprilTags() =>
            AprilTagWrapper.Detect(grayBuff, width, height);

        private IEnumerator ProcessAprilTags()
        {
            foreach(DetectionResult.ApriltagDetection tagInfo in lastResult.ids)
            {
                if(!TagDatabase.TryGetTagName(tagInfo.id, out string name))
                {
                    yield return ModalTagNameSelector.ShowDialog();
                    string tagName = ModalTagNameSelector.GetNamespaceResult();
                    string visualization = ModalTagNameSelector.GetVisualizationResult();

                    TagDatabase.StoreTag(tagInfo.id, tagName, visualization);
                    TagDatabase.SaveDatabase();
                }
            }

            resultReady = false;
        }

        private IEnumerator StoreTag(int tagId)
        {
            if(!TagDatabase.TryGetTagName(tagId, out string name))
            {
                yield return ModalTagNameSelector.ShowDialog();
                string tagName = ModalTagNameSelector.GetResult();

                TagDatabase.StoreTag(tagId, tagName);
                TagDatabase.SaveDatabase();
            }       
        }

        private void StoreTagAnchor(int tagId, Vector2 screenPos)
        {
            List<ARRaycastHit> hits = new();
            
            if(!raycastManager.Raycast(screenPos, hits, TrackableType.Planes)) return;
            if(hits.Count < 1) return;

            Pose hitPose = hits[0].pose;
            Vector3 worldPos = hitPose.position;
            if(!tagAnchors.ContainsKey(tagId)) tagAnchors.Add(tagId, worldPos);
            Debug.Log(tagAnchors[tagId]);
        }

        void OnGUI()
        {
            if(lastResult == null || lastResult.ids == null) return;

            GUI.color = Color.red;
            foreach(DetectionResult.ApriltagDetection tag in lastResult.ids)
            {
                Vector2 screenPos = new Vector2((float)tag.cx, height-(float)tag.cy);
                GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 10, 100, 50), 
                    TagDatabase.TryGetTagName(tag.id, out string name) ? name : $"Tag #{tag.id}");
            }
            GUI.color = Color.white;    
        }
    }
    
    [System.Serializable]
    public static class TagDatabase
    {
        class TagInfo
        {
            public string name;
            public string visualization;
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

        public static void StoreTag(int id, string name, string visualization)
        {
            if(tags.ContainsKey(id))
                tags[id] = new TagInfo { name = name, visualization = visualization };
            else
                tags.Add(id, new TagInfo { name = name, visualization = visualization });
        }

        public static void DeleteTag(int id) => tags.Remove(id);

        public static bool TryGetTagName(int id, out string name) =>
            tags.TryGetValue(id, out var tagInfo) ? (name = tagInfo.name) != null : (name = null) != null;

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

        public static List<string> GetAllTagNames() => tags.Values.Select(t => t.name).ToList();
    }
}

