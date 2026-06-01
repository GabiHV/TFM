using UnityEngine;

using System.Collections;

public class TFFrameJitterVisualizer : MonoBehaviour
{
    public TFRoot.FrameNode frame;
    public GameObject TFFrameObj;
    public float varianceThreshold;

    private float variance;
    private Coroutine jitterCoroutine;

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
        if(IsInconsistent()) TriggerJitter();
        if(!IsInconsistent()) StopJitterEffect();
    }

    private void TriggerJitter()
    {
        if(!IsJitterGOSet()) return;
        if(IsJitterActive()) return;
        StartJitterEffect();
    }
    
    private bool IsJitterGOSet() =>
        TFFrameObj != null;

    private bool IsJitterActive() =>
        jitterCoroutine != null;

    private void StopJitterEffect()
    {
        if(!IsJitterActive()) return;
        StopCoroutine(jitterCoroutine);

        jitterCoroutine = null;
    }

    private void StartJitterEffect() =>
        jitterCoroutine = StartCoroutine(JitterEffect(TFFrameObj.transform));

    private IEnumerator JitterEffect(Transform GOTransform)
    {
        while (true)
        {
            GOTransform.localPosition += GetJitterOffset(variance);
      
            float pulse = Mathf.Abs(Mathf.Sin(Time.time * 5f));
            Color baseColor = GetJitterColor(variance);
            Color finalColor = Color.Lerp(baseColor, Color.white, pulse * variance);   
            yield return null;
        }
    }

    Vector3 GetJitterOffset(float jitterStrength)
    {
        float freq = Mathf.Lerp(5f, 40f, jitterStrength);
        float amp  = Mathf.Lerp(0.001f, 0.01f, jitterStrength);

        float t = Time.time;

        float x = Mathf.PerlinNoise(t * freq, 0f) - 0.5f;
        float y = Mathf.PerlinNoise(0f, t * freq) - 0.5f;
        float z = Mathf.PerlinNoise(t * freq, t * 0.5f) - 0.5f;

        return new Vector3(x, y, z) * amp;
    }

    Color GetJitterColor(float jitter)
    {
        if (jitter < 0.2f) return Color.green;
        if (jitter < 0.5f) return Color.yellow;
        return Color.red;
    }
}
