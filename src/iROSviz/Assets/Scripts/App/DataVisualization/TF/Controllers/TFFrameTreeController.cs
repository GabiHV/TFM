using UnityEngine;

public class TFFrameTreeController : MonoBehaviour
{
    public TFRoot.FrameNode frame;

    void Update() =>
        UpdateLineToParent();

    private bool IsFrameSet() =>
        frame != null;

    private void UpdateLineToParent()
    {
        if(!IsFrameSet()) return;
        RenderLineToParent();
    }

    private void RenderLineToParent()
    {
        if(frame.Parent == null) return;
        Vector3 parentPos = frame.Parent.GO.transform.position;
        Vector3 childPos = frame.GO.transform.position;
        if(Vector3.Distance(parentPos, childPos) < 0.01f) return; // Skip if too close

        LineRenderer lr = frame.GO.GetComponent<LineRenderer>();
        if(lr == null)
            lr = frame.GO.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, parentPos);
        lr.SetPosition(1, childPos);
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.red;
        lr.endColor = Color.red;
        lr.widthMultiplier = 0.1f;
    }
}
