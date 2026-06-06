using UnityEngine;

using System.Collections.Generic;

public class TFFrameTrailVisualizer : MonoBehaviour
{
    public TFRoot.FrameNode frame;
    public GameObject TFFrameObj;
    public LineRenderer trailRenderer;

    private static readonly string TrailObjLabel = "Trail";
    private Transform TrailObj;
    private float lastSampleTime;

    void Update() =>
        ShowTrail();

    private bool IsFrameSet() =>
        frame != null;

    private void ShowTrail()
    {
        if(!IsFrameSet()) return;
        SetTrailObj();
        DrawTrail();
    }

    private bool IsTrailObjSet() =>
        TrailObj != null;

    private void SetTrailObj()
    {
        if(IsTrailObjSet()) return;

        TrailObj = TFFrameObj.transform.Find(TrailObjLabel) ?? 
            new GameObject(TrailObjLabel).transform;
        TrailObj.SetParent(TFFrameObj.transform);
    }

    private void DrawTrail()
    {
        if(!(Time.time - lastSampleTime > 0.05f)) return;

        List<Vector3> positions = frame.GetHistPosition();
        trailRenderer.positionCount = positions.Count;
        trailRenderer.SetPositions(positions.ToArray());
        
        float jitter = TFFrameObj.GetComponent<TFFrameJitterVisualizer>().GetJitter();
        Gradient g = new Gradient();
        Color c = GetTrailColor(jitter);

        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(c, 0),
                new GradientColorKey(c, 1)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.0f, 0),
                new GradientAlphaKey(1.0f, 1)
            }
        );
        trailRenderer.widthMultiplier = Mathf.Lerp(0.1f, 0.25f, jitter);
        trailRenderer.colorGradient = g;
        lastSampleTime = Time.time;
    }

    Color GetTrailColor(float jitter)
    {
        if (jitter < 0.2f) return Color.green;
        if (jitter < 0.5f) return Color.yellow;
        return Color.red;
    }
        
}
