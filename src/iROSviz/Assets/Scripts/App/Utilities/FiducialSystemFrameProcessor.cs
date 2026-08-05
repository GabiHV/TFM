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

        void Awake() =>
            StartCoroutine(LoadRaycastManager());

        IEnumerator LoadRaycastManager()
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
                Vector2 screenPos = new Vector2((float)tagInfo.cx, height-(float)tagInfo.cy);

                yield return StoreTag(tagInfo.id);
                StoreTagAnchor(tagInfo.id, screenPos);
            }

            resultReady = false;
        }

        private IEnumerator StoreTag(int tagId)
        {
            if(!TagDatabase.TryGetTagName(tagId, out string name))
            {
                yield return ModalTagNameSelector.ShowDialog();
                string tagName = ModalTagNameSelector.GetTagName();
                string frame = ModalTagNameSelector.GetFrame();
                string pathVisualizer = ModalTagNameSelector.GetPathVisualization();

                TagDatabase.StoreTag(tagId, tagName, frame, pathVisualizer);
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
}
