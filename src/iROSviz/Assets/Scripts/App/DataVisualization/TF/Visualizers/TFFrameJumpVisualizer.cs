using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TFFrameJumpVisualizer : MonoBehaviour
{
    public TFRoot.FrameNode frame;
    public GameObject TFFrameObj;
    public GameObject JumpGO;
    public float distanceThreshold;
    public float rotationThreshold;

    private float deltaDistance;
    private float deltaRotation;
    private Coroutine jitterCoroutine;

    void Start() =>
        JumpGO.SetActive(false);

    void Update() =>
        PerformJumpDetection();

    private bool IsFrameSet() =>
        frame != null;

    private void PerformJumpDetection()
    {
        if(!IsFrameSet()) return;
        JumpDetection();
        RotationJumpDetection();
        ApplyJumpWarning();
    }

    private void JumpDetection()
    {
        Vector3 currentPos = frame.Position;
        List<Vector3> histPos = frame.GetHistPosition();
        Vector3 lastPos = histPos.Last();

        deltaDistance = Vector3.Distance(currentPos, lastPos);
    }

    private void RotationJumpDetection()
    {
        Quaternion currentRotation = frame.Rotation;
        List<Quaternion> histRot = frame.GetHistRotation();
        Quaternion lastRot = histRot.Last();

        deltaRotation = Quaternion.Angle(currentRotation, lastRot);
    }

    private bool IsJump() =>
        deltaDistance > distanceThreshold;

    private bool IsRotationJump() =>
        deltaRotation > rotationThreshold;

    private void ApplyJumpWarning()
    {
        if(!IsJump() && !IsRotationJump()) 
        {
            StopJitterEffect();
            return;
        };
        TriggerJump();
    }


    private void TriggerJump()
    {
        if(!IsJumpGOSet()) return;
        if(IsJitterActive()) return;
        StartJitterEfect();
    }

    private bool IsJitterActive() =>
        jitterCoroutine != null;
    
    private void StopJitterEffect()
    {
        if(!IsJitterActive()) return;
        StopCoroutine(jitterCoroutine);

        JumpGO.SetActive(false);
        jitterCoroutine = null;
    }

    private void StartJitterEfect() =>
        jitterCoroutine = StartCoroutine(JumpEffect(JumpGO.transform));

    private IEnumerator JumpEffect(Transform jumpMarkerT)
    {
        jumpMarkerT.gameObject.SetActive(true);

        Vector3 startScale = Vector3.one * 1f;
        Vector3 endScale = Vector3.one * 2f;

        Renderer rend = jumpMarkerT.GetComponent<Renderer>();

        while (true)
        {
            float duration = 1f;
            float t = 0;
            while (t < duration)
            {
                float k = t / duration;

                jumpMarkerT.localScale = Vector3.Lerp(startScale, endScale, k);

                Color c = Color.Lerp(Color.yellow, Color.red, k);
                c.a = 1f - k;

                rend.material.color = c;

                t += Time.deltaTime;
                yield return null;
            }
        }
    }

    private bool IsJumpGOSet() =>
        JumpGO != null;
}
