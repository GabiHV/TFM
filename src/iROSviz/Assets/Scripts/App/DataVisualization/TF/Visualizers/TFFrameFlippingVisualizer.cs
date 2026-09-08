using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TFFrameFlippingVisualizer : MonoBehaviour
{
    public TFRoot.FrameNode frame;
    public GameObject TFFrameObj;
    public GameObject TFFrameGhost;

    private float flipping;
    private static float flippingThreshold = 0;
    private Coroutine flippingCoroutine;
    private Quaternion lastRotation;

    void Start() =>
        DespawnGhost();

    void Update() =>
        PerformFlippingDetection();

    private bool IsFrameSet() =>
        frame != null;

    private void PerformFlippingDetection()
    {
        if(!IsFrameSet()) return;
        DetectFlipping();
        ApplyFlippingWarning();
    }

    private void DetectFlipping()
    {
        List<Quaternion> rotations = frame.GetHistRotation();
        Quaternion currentRot = frame.Rotation;
        lastRotation = rotations.Last();

        flipping = Quaternion.Dot(currentRot, lastRotation);
    }

    private bool IsFlipping() =>
        flipping < flippingThreshold;

    private void ApplyFlippingWarning()
    {
        if(!IsFlipping()) StopFlipping();
        if(IsFlipping()) TriggerFlipping();
    }

    private void SpawnGhost()
    {
        TFFrameGhost.SetActive(true);     
        Quaternion lastRot = frame.GetHistRotation().Last();

        TFFrameGhost.transform.rotation = lastRot;
    }

    private void DespawnGhost() =>
        TFFrameGhost.SetActive(false);

    private bool IsGhostSet() =>
        TFFrameGhost != null;

    private void TriggerFlipping()
    {
        if(!IsGhostSet()) return;
        if(IsFlippingActive()) return;
        SpawnGhost();
        StartFlipping();
    }

    private void StartFlipping() =>
        flippingCoroutine = StartCoroutine(FadeAndDestroyGhost(TFFrameGhost));

    private void StopFlipping()
    {
        if(!IsFlippingActive()) return;
        StopCoroutine(flippingCoroutine);
        DespawnGhost();
    }

    private bool IsFlippingActive() =>
        flippingCoroutine != null;

    IEnumerator FadeAndDestroyGhost(GameObject ghost)
    {
        Renderer[] renderers = ghost.GetComponentsInChildren<Renderer>();
        Debug.Log($"Renderers count: {renderers.Length}");

        while (true)
        {  
            float t = 0f;
            float duration = 1f;
            while (t < duration)
            {
                float k = t / duration;

                ghost.transform.rotation = 
                    Quaternion.Slerp(frame.Rotation, lastRotation, k);
                float alpha = Mathf.Lerp(1f, 0f, k);

                foreach (var r in renderers)
                {
                    Color c = r.material.color;
                    c.a = alpha;
                    r.material.color = c;
                }

                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}
