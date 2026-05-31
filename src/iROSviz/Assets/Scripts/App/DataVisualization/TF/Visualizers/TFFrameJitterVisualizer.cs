using UnityEngine;

public class TFFrameJitterVisualizer : MonoBehaviour
{
    public TFRoot.FrameNode frame;
    public GameObject TFFrameObj;
    public float varianceThreshold;

    private float variance;

    void Update() =>
        PerformJitterDetection();

    private bool IsFrameSet() =>
        frame != null;

    private void PerformJitterDetection()
    {
        if(!IsFrameSet()) return;
        DetectJitter();
        ApplyJitterVisualization();
    }

    private void DetectJitter()
    {
        float currentDt = frame.UpdateInterval;
        float avgDt = frame.GetAvgDeltaTime();
        variance = Mathf.Lerp(variance, Mathf.Pow(currentDt - avgDt, 2), 0.1f);
    }
 
    private bool IsInconsistent() =>
        variance > varianceThreshold;

    private void ApplyJitterVisualization()
    {
        // if(!IsInconsistent()) return;
        Debug.Log($"Frame: {frame.Name}. Variance DT: {variance}");
    }
}
