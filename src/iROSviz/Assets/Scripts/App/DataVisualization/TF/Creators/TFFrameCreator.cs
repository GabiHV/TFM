using UnityEngine;

public class TFFrameCreator : MonoBehaviour
{
    public static readonly string FrameMarker = "FrameMarker";

    public TFRoot.FrameNode frame;
    public GameObject prefab;
    public Camera arCamera;

    private GameObject TFFrameGO;

    public void CreateTFFrame(TFRoot.FrameNode frame)
    {
        this.frame = frame;
        PerformCreation();
    } 

    private void PerformCreation()
    {
        if(IsTFFrameCreated()) return;
        
        CreateTFFrame();
        DependecyInjection();
        SetTFFrameObjParent();
        SetTFFrameParent();
    }

    private bool IsTFFrameCreated() =>
        frame.GO.transform.Find(FrameMarker)?.gameObject != null;
    
    private void CreateTFFrame()
    {
        TFFrameGO = Instantiate(prefab);
        TFFrameGO.name = FrameMarker;
        TFFrameGO.transform.localScale = Vector3.one * 0.05f; 
    }

    private void DependecyInjection()
    {
        TFFrameSpeedVisualizer fsv = TFFrameGO.GetComponent<TFFrameSpeedVisualizer>();
        fsv.frame = frame;      
        TFFrameStatusVisualizer fstv = TFFrameGO.GetComponent<TFFrameStatusVisualizer>(); 
        fstv.frame = frame;  
        TFFrameJumpVisualizer fjv = TFFrameGO.GetComponent<TFFrameJumpVisualizer>();
        fjv.frame = frame;
        TFFrameJitterVisualizer fjiv = TFFrameGO.GetComponent<TFFrameJitterVisualizer>();
        fjiv.frame = frame;
        TFFrameFlippingVisualizer ffv  = TFFrameGO.GetComponent<TFFrameFlippingVisualizer>();
        ffv.frame = frame;
        TFFrameTrailVisualizer ftv = TFFrameGO.GetComponent<TFFrameTrailVisualizer>();
        ftv.frame = frame;
        TFFramePositionController fpc = TFFrameGO.GetComponent<TFFramePositionController>();
        fpc.frame = frame;
        TFFrameRotationController frc = TFFrameGO.GetComponent<TFFrameRotationController>();
        frc.frame = frame;
        TFFrameTreeController ftc = TFFrameGO.GetComponent<TFFrameTreeController>();
        ftc.frame = frame;
        TFFrameLabelsController flc = TFFrameGO.GetComponent<TFFrameLabelsController>();
        flc.frame = frame;
        flc.arCamera = arCamera;

        PointCloudVisualizer pcv = TFFrameGO.GetComponent<PointCloudVisualizer>();
        pcv.frame = frame;
    }

    private void SetTFFrameObjParent()
    {
        if(TFFrameGO.transform.parent != frame.GO.transform)
            TFFrameGO.transform.SetParent(frame.GO.transform, false);
    }

    private void SetTFFrameParent()
    {
        TFRoot.FrameNode parent = frame.Parent;
        if(parent == null) return;
        Transform childT = frame.GO.transform;
        Transform parentT = parent.GO.transform;

        if(childT.parent != parentT)
            childT.SetParent(parentT, false);
    }
}
