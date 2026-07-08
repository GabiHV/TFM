using UnityEngine;

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

    public void OnClickTopicEcho()
    {
        TopicEchoUI.SetActive(true);
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
        ParamChangerUI.SetActive(true);
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
        TopicPublisherUI.SetActive(true);
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
        ConfigUI.SetActive(true);
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
        TFAdjusterUI.SetActive(true);
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
        PathPlannerUI.SetActive(true);
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
        TFFrameMeasurerUI.SetActive(true);
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
        ToggleTFFrameUI.SetActive(true);
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
        TeleopUI.SetActive(true);
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
        PointCloudConfigUI.SetActive(true);
    }
    
}
