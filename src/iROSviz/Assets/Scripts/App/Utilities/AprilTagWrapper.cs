using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace App.Utilities
{
    public class AprilTagWrapper
    {
        public static AprilTagFamilies familyTag = AprilTagFamilies.tag36h11;

        [DllImport("apriltag_wrapper")]
        private static extern void apriltag_init(string apriltag_family_name);

        [DllImport("apriltag_wrapper")]
        private static extern int apriltag_num_tag(System.IntPtr gray, int width, int height);

        [DllImport("apriltag_wrapper")]
        private static extern int apriltag_detect([Out] DetectionResult.ApriltagDetection[] detectionInfo);

        [DllImport("apriltag_wrapper")]
        private static extern void apriltag_destroy();

        private static void Init() => apriltag_init(nameof(familyTag));

        private static void Dispose() => apriltag_destroy();

        public static DetectionResult Detect(byte[] imgBuf, int width, int height)
        {
            Init();

            GCHandle imageHandle = GCHandle.Alloc(imgBuf, GCHandleType.Pinned);
            int tagQuantity = 0;
            DetectionResult detectionResult;
            try
            {
                tagQuantity = apriltag_num_tag(imageHandle.AddrOfPinnedObject(), width, height);
                
                DetectionResult.ApriltagDetection[] tags = 
                    new DetectionResult.ApriltagDetection[tagQuantity];
                
                apriltag_detect(tags);

                TransformAprilTagCoordinates(height, ref tags);

                detectionResult = new(tagQuantity, tags.ToList());
            }
            finally
            {
                imageHandle.Free();
                Dispose();
            }   
            return detectionResult;
        }

        private static void TransformAprilTagCoordinates(
            int height, 
            ref DetectionResult.ApriltagDetection[] detections
        )
        {
            for(int i = 0; i< detections.Length; i++)
                detections[i].cy = height - detections[i].cy;
        }
    }

    public class DetectionResult
    {
        public struct ApriltagDetection
        {
            public int id;
            public double cx;
            public double cy;
        }

        public int tagCount {get; private set;}
        public List<ApriltagDetection> ids {get; private set;}

        public DetectionResult() {}

        public DetectionResult(int tagCount, List<ApriltagDetection> ids)
        {
            this.tagCount = tagCount;
            this.ids = ids;
        }
    }

    public enum AprilTagFamilies
    {
        tag36h11,
        tag16h5,
        tag25h9,
        tag36h10,
        tagCircle21h7,
        tagCircle49h12,
        tagCustom48h12,
        tagStandard41h12,
        tagStandard52h13,
    }

}


