using UnityEngine;
using TMPro;

using System.Collections.Generic;

using App.Utilities;
using App.ROSUtilities;

public class TFRender : MonoBehaviour
{
    void Update() =>
        Render();

    private void Render()
    {
        foreach (var robotFrames in this.GetComponent<TFRoot>().frames)
        {
            Dictionary<string, TFRoot.FrameNode> robotFramesDict = robotFrames.Value;
            foreach (var frame in robotFramesDict.Values)
                VisulizeFrame(frame);    
        }
    }

    private void VisulizeFrame(TFRoot.FrameNode frame)
    {
        if (frame == null) return;

        this.GetComponent<TFFrameCreator>().CreateTFFrame(frame);
    }
}
