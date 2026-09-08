using System.ComponentModel;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class OptionsModal : MonoBehaviour
{
    public GameObject TopicEchoUI;
    public GameObject ParamChangerUI;
    public GameObject TopicPublisherUI;
    public GameObject ConfigUI;
    public GameObject TFAdjusterUI;
    public GameObject PathPlannerUI;
    public GameObject TFFrameMeasurerUI;
    public GameObject ToggleTFFrameUI;
    public GameObject TeleopUI;
    public GameObject PointCloudConfigUI;
    public GameObject XROrigin;
    public GameObject DebugMenuUI;

    public GameObject ARDebugMenuSlider;
    public GameObject ARTrackablesSlider;

    public void OnClickTopicEcho()
    {
        if(TopicEchoUI.activeSelf) TopicEchoUI.SetActive(false);
        else TopicEchoUI.SetActive(true);

        ParamChangerUI.SetActive(false);
        TopicPublisherUI.SetActive(false);
        ConfigUI.SetActive(false);
        TFAdjusterUI.SetActive(false);
        PathPlannerUI.SetActive(false);
        TFFrameMeasurerUI.SetActive(false);
        ToggleTFFrameUI.SetActive(false);
        TeleopUI.SetActive(false);
        PointCloudConfigUI.SetActive(false);
    }

    public void OnClickParamChanger()
    {
        TopicEchoUI.SetActive(false);

        if(ParamChangerUI.activeSelf) ParamChangerUI.SetActive(false);
        else ParamChangerUI.SetActive(true);

        TopicPublisherUI.SetActive(false);
        ConfigUI.SetActive(false);
        TFAdjusterUI.SetActive(false);
        PathPlannerUI.SetActive(false);
        TFFrameMeasurerUI.SetActive(false);
        ToggleTFFrameUI.SetActive(false);
        TeleopUI.SetActive(false);
        PointCloudConfigUI.SetActive(false);
    }

    public void OnClickTopicPublisher()
    {
        TopicEchoUI.SetActive(false);
        ParamChangerUI.SetActive(false);
        
        if(TopicPublisherUI.activeSelf) TopicPublisherUI.SetActive(false);
        else TopicPublisherUI.SetActive(true);

        ConfigUI.SetActive(false);
        TFAdjusterUI.SetActive(false);
        PathPlannerUI.SetActive(false);
        TFFrameMeasurerUI.SetActive(false);
        ToggleTFFrameUI.SetActive(false);
        TeleopUI.SetActive(false);
        PointCloudConfigUI.SetActive(false);
    }

    public void OnClickConfig()
    {
        TopicEchoUI.SetActive(false);
        ParamChangerUI.SetActive(false);
        TopicPublisherUI.SetActive(false);

        if(ConfigUI.activeSelf) ConfigUI.SetActive(false);
        else ConfigUI.SetActive(true);
        
        TFAdjusterUI.SetActive(false);
        PathPlannerUI.SetActive(false);
        TFFrameMeasurerUI.SetActive(false);
        ToggleTFFrameUI.SetActive(false);
        TeleopUI.SetActive(false);
        PointCloudConfigUI.SetActive(false);
    }

    public void OnClickTFAdjuster()
    {
        TopicEchoUI.SetActive(false);
        ParamChangerUI.SetActive(false);
        TopicPublisherUI.SetActive(false);
        ConfigUI.SetActive(false);

        if(TFAdjusterUI.activeSelf) TFAdjusterUI.SetActive(false);
        else TFAdjusterUI.SetActive(true);

        PathPlannerUI.SetActive(false);
        TFFrameMeasurerUI.SetActive(false);
        ToggleTFFrameUI.SetActive(false);
        TeleopUI.SetActive(false);
        PointCloudConfigUI.SetActive(false);
    }

    public void OnClickPathPlanner()
    {
        TopicEchoUI.SetActive(false);
        ParamChangerUI.SetActive(false);
        TopicPublisherUI.SetActive(false);
        ConfigUI.SetActive(false);
        TFAdjusterUI.SetActive(false);

        if(PathPlannerUI.activeSelf) PathPlannerUI.SetActive(false);
        else PathPlannerUI.SetActive(true);

        TFFrameMeasurerUI.SetActive(false);
        ToggleTFFrameUI.SetActive(false);
        TeleopUI.SetActive(false);
        PointCloudConfigUI.SetActive(false);
    }

    public void OnClickTFFrameMeasurer()
    {
        TopicEchoUI.SetActive(false);
        ParamChangerUI.SetActive(false);
        TopicPublisherUI.SetActive(false);
        ConfigUI.SetActive(false);
        TFAdjusterUI.SetActive(false);
        PathPlannerUI.SetActive(false);

        if(TFFrameMeasurerUI.activeSelf) TFFrameMeasurerUI.SetActive(false);
        else TFFrameMeasurerUI.SetActive(true);

        ToggleTFFrameUI.SetActive(false);
        TeleopUI.SetActive(false);
        PointCloudConfigUI.SetActive(false);
    }

    public void OnClickToggleTFFrame()
    {
        TopicEchoUI.SetActive(false);
        ParamChangerUI.SetActive(false);
        TopicPublisherUI.SetActive(false);
        ConfigUI.SetActive(false);
        TFAdjusterUI.SetActive(false);
        PathPlannerUI.SetActive(false);
        TFFrameMeasurerUI.SetActive(false);

        if(ToggleTFFrameUI.activeSelf) ToggleTFFrameUI.SetActive(false);
        else ToggleTFFrameUI.SetActive(true);

        TeleopUI.SetActive(false);
        PointCloudConfigUI.SetActive(false);
    }

    public void OnClickTeleop()
    {
        TopicEchoUI.SetActive(false);
        ParamChangerUI.SetActive(false);
        TopicPublisherUI.SetActive(false);
        ConfigUI.SetActive(false);
        TFAdjusterUI.SetActive(false);
        PathPlannerUI.SetActive(false);
        TFFrameMeasurerUI.SetActive(false);
        ToggleTFFrameUI.SetActive(false);

        if(TeleopUI.activeSelf) TeleopUI.SetActive(false);
        else TeleopUI.SetActive(true);

        PointCloudConfigUI.SetActive(false);
    }

    public void OnClickPointCloudConfig()
    {
        TopicEchoUI.SetActive(false);
        ParamChangerUI.SetActive(false);
        TopicPublisherUI.SetActive(false);
        ConfigUI.SetActive(false);
        TFAdjusterUI.SetActive(false);
        PathPlannerUI.SetActive(false);
        TFFrameMeasurerUI.SetActive(false);
        ToggleTFFrameUI.SetActive(false);
        TeleopUI.SetActive(false);

        if(PointCloudConfigUI.activeSelf) PointCloudConfigUI.SetActive(false);
        else PointCloudConfigUI.SetActive(true);
    }

    public void OnClickVisualizeSurfaces()
    {
        GameObject trackables = XROrigin.transform.Find("Trackables").gameObject;
        DebugSlider slider = ARTrackablesSlider.GetComponent<DebugSlider>();
        if(slider == null) return;
        if(slider.value == 0) 
        {
            trackables.SetActive(true);
            slider.value = 1;
            return;
        }
        if(slider.value == 1) 
        {
            trackables.SetActive(false);
            slider.value = 0;
            return;
        }
    }

    public void OnClickARDebugMenu()
    {
        DebugSlider slider = ARDebugMenuSlider.GetComponent<DebugSlider>();
        if(slider == null) return;
        if(slider.value == 0) 
        {
            DebugMenuUI.SetActive(true);
            slider.value = 1;
            return;
        }

        if(slider.value == 1) 
        {
            DebugMenuUI.SetActive(false);
            slider.value = 0;
            return;
        }
    }

}
