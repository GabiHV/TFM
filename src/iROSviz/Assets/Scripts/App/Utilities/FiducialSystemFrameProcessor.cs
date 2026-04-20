
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

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
        static Plane rayCastPlane = new(Vector3.up, Vector3.zero);
        static Vector3 rayCastVector = new Vector3(0, 0, 0);
        
        int frameCounter = 0;
        byte[] rgbaBuffA;
        byte[] rgbaBuffB;
        byte[] grayBuff;
        byte[] flippedImg;
        RenderTexture render;
        Texture2D texture;
        Rect rect;
        DetectionResult lastResult = new();
        private List<GameObject> rays = new();


        // Semaphores
        volatile bool resultReady = false;
        volatile bool bufferAReady = false;
        volatile bool bufferBReady = false;
        volatile bool useBufferA = false;

        void Start()
        {
            GameConfig config = ConfigHelper.GetConfig();
            ConfigHelper.ChangeFiduciarySys(config);

            width = Screen.currentResolution.width;
            height = Screen.currentResolution.height;
            
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
                    string tagName = ModalTagNameSelector.GetResult();
Debug.LogWarning($"Tag #{tagInfo.id} has been named '{tagName}'");
                    TagDatabase.StoreTag(tagInfo.id, tagName);
                    TagDatabase.SaveDatabase();
                }
            }

            resultReady = false;
        }

        private void DrawTagIndicator(DetectionResult.ApriltagDetection tag)
        {
            Vector3? wordlPos = Raytracing((float)tag.cx, (float)tag.cy);
            if(wordlPos == null) return;

            Debug.Log($"Tag #{tag.id} position: ({wordlPos?.x}, {wordlPos?.y}, {wordlPos?.z})");
        }

        private Vector3? Raytracing(float x, float y)
        {
            rayCastVector.x = x;
            rayCastVector.y = y;
            Ray ray = Camera.main.ScreenPointToRay(rayCastVector);
            float distance;

            if(!rayCastPlane.Raycast(ray, out distance)) return null;

            Vector3 worldPos = ray.GetPoint(distance);
            return worldPos;
        }
    }
    
    [System.Serializable]
    public static class TagDatabase
    {
        private static Dictionary<int, string> tags = new();
        static readonly string APRILTAG_DB_PATH = Application.persistentDataPath + "/april_tags_database.json";

        public static void SaveDatabase()
        {
            string json = JsonUtility.ToJson(tags, false);
            Debug.Log($"{string.Join(", ", tags.Keys)} tags serialized to JSON.");
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
                tags = JsonUtility.FromJson<Dictionary<int, string>>(json);
                
                Debug.Log($"Tags database loaded from {APRILTAG_DB_PATH}. Loaded {tags.Count} tags.");
            }
            catch(Exception ex)
            {
                Debug.LogError($"Error loading tags database: {ex.Message}");
            }
        }

        public static void StoreTag(int id, string name)
        {
            if(tags.ContainsKey(id))
                tags[id] = name;
            else
                tags.Add(id, name);
        }

        public static bool TryGetTagName(int id, out string name) =>
            tags.TryGetValue(id, out name);

        public static bool TryGetTagId(string name, out int id)
        {
            foreach(var item in tags)
            {
                if(item.Value == name)
                {
                    id = item.Key;
                    return true;
                }
            }
            id = -1;
            return false;
        }
    }
}

