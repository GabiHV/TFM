using UnityEngine;

public class TFFrameSpeedVisualizer : MonoBehaviour
{
    public TFRoot.FrameNode frame;
    public float speedThreshold = 0.01f;
    public float angularSpeedThreshold = 0.1f;

    public GameObject TFFrameObj;
    public Transform ringObj;
    public LineRenderer speedVector;

    void Update() =>
        UpdateSpeedVisualizing();
    
    private void UpdateSpeedVisualizing() =>
        UpdateSpeed();

    private void UpdateSpeed()
    {
        if(!IsFrameSet() || IsPaused()) return;

        UpdateLinealVelocity();
        UpdateAngularVelocity();
    }
    
    private bool IsFrameSet() =>
        frame != null;

    private void UpdateLinealVelocity()
    {
        if(!IsFrameSet()) return;

        float mag = frame.Velocity.magnitude;

        if (mag < speedThreshold)
        {
            speedVector.enabled = false;
            return;
        }

        speedVector.enabled = true;

        speedVector.SetPosition(0, frame.GO.transform.position);
        speedVector.SetPosition(1, frame.GO.transform.position + frame.Velocity * 0.5f);
    }

    private void UpdateAngularVelocity()
    {
        if(!IsFrameSet()) return;

        float speed = frame.AngularVelocity.magnitude;

        if (speed < angularSpeedThreshold)
        {
            ringObj.gameObject.SetActive(false);
            return;
        }

        Vector3 axis = frame.AngularVelocity.normalized;
        if (axis == Vector3.zero) axis = Vector3.up; 

        // Ring position and orientation
        ringObj.gameObject.SetActive(true);
        ringObj.position = frame.Position;
        ringObj.rotation = Quaternion.LookRotation(axis);

        // Color
        Color c = GetAngularColor(speed);
        ringObj.GetComponent<Renderer>().material.color = c;
    }

    private bool IsPaused() =>
        TFRuntimeController.IsPaused();

    private Color GetAngularColor(float speed)
    { 
        if (speed < 0.5f) return Color.green;
        if (speed < 2f) return Color.yellow;
        return Color.red;
    }
}
