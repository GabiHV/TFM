using UnityEngine;

public class TFFrameStatusVisualizer : MonoBehaviour
{
    public TFRoot.FrameNode frame;
    public Renderer sphereRenderer;
    public float lowHzThreshold = 5.0f; // Hz
    public float staleThreshold = 0.5f; // seconds

    void Update() =>
        UpdateFrameStatus();
    
    private void UpdateFrameStatus() =>
        UpdateSphereColor();

    private void UpdateSphereColor() =>
        sphereRenderer.material.color = GetColorBasedOnStaleness();

    private bool IsPaused() =>
        TFRuntimeController.IsPaused();

    private bool IsStale()
    {
        float now = Time.time;
        return (now - frame.LastUpdateTime) > staleThreshold;
    }

    private bool IsFrequencyLow() =>
        frame.SmoothedHZ < lowHzThreshold;

    private Color GetColorBasedOnStaleness()
    {
        if(IsPaused())
            return GetSnapshotColor();

        if (IsStale())
            return Color.red;
        
        if (IsFrequencyLow())
            return Color.yellow;
            
        return Color.green;
    }

    private Color GetSnapshotColor() =>
        Color.blue;
}
