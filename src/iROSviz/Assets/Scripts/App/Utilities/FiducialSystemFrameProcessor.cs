
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

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

            Thread worker = new Thread(ProcessFrame);
            worker.IsBackground = true;
            worker.Start();
        }

        void Update()
        {
            if(frameCounter++ >= frameGap) 
            {
                CaptureFrame();
                frameCounter = 0;
            }

            if(resultReady) HighlightAprilTags();
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

        private void HighlightAprilTags()
        {
            foreach(DetectionResult.ApriltagDetection tagInfo in lastResult.ids)
                DrawTagIndicator(tagInfo);

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
}

