using UnityEngine;

using System.Collections.Generic;

public class TFFrameTrailVisualizer : MonoBehaviour
{
    public TFRoot.FrameNode frame;
    public GameObject TFFrameObj;

    private static readonly string TrailObjLabel = "Trail";
    private Transform TrailObj;

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
        List<Vector3> positions = frame.GetHistPosition();

        for (int i = 0; i < positions.Count; i++)
        {
            UpdateOrCreatePathChild(i, positions[i]);
        }
    }

    private void UpdateOrCreatePathChild(int index, Vector3 newPos)
    {
        GameObject child = TrailObj.childCount <= index ?
            CreatePathChild() :
            TrailObj.GetChild(index)?.gameObject;
        if (child == null) return;

        child.transform.position = newPos;
    }

    private GameObject CreatePathChild()
    {
        GameObject child = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        child.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        child.transform.SetParent(TrailObj);
        child.GetComponent<Renderer>().material.color = Color.red;

        return child;
    }

        
}
