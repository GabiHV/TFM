using UnityEngine;
using TMPro;

using System.Collections.Generic;

using App.Utilities;
using App.ROSUtilities;

public class TFRender : MonoBehaviour
{
    private HashSet<string> renderedObjs = new();
    private GameObject TFRootGO;

    void Start() =>
        SetTFRoot();

    void Update() =>
        Render();

    private void Render()
    {
        foreach (var (topic, frames) in this.GetComponent<TFRoot>().GetFramesByTopic())
        {
            RenderRoot(topic);
            foreach (var (frameName, frame) in frames)
                VisulizeFrame(frame);    
        }
    }

    private void SetTFRoot() =>
        TFRootGO = this.gameObject;

    private void VisulizeFrame(TFRoot.FrameNode frame)
    {
        if (frame == null) return;

        this.GetComponent<TFFrameCreator>().CreateTFFrame(frame);
    }

    private void RenderRoot(string topic)
    {
        if(renderedObjs.Contains(topic)) return;

        GameObject rootObj = new GameObject();
        rootObj.name = topic;
        rootObj.transform.position = Vector3.zero;
        rootObj.transform.rotation = Quaternion.identity;
        rootObj.transform.SetParent(TFRootGO.transform);

        renderedObjs.Add(topic);
    }
}
