using UnityEngine;
using TMPro;

using System.Collections.Generic;

using App.ROSUtilities.Subscribers;

public class PointCloudConfig : MonoBehaviour
{

    public GameObject TFRootGO;
    public GameObject pointCloudConfigPrefab;
    public GameObject scrollViewContent;
    
    private List<GameObject> pointCloudConfigObjects = new List<GameObject>();
    private List<string> pointCloudTopics = new List<string>();

    // Update is called once per frame
    void Update()
    {
        UpdatePointCloudTopicList();
        LoadPointCloudConfig();
    }

    private void UpdatePointCloudTopicList() =>
        pointCloudTopics = ROSTopicInfoSubscriber.GetTopicsByType(new RosMessageTypes.Sensor.PointCloud2Msg().RosMessageName);

    private void LoadPointCloudConfig()
    {
        Dictionary<string, Dictionary<string, TFRoot.FrameNode>> frames = TFRootGO.GetComponent<TFRoot>().GetFramesByTopic();
        foreach (var (root, frameDict) in frames)
        {
            foreach (var (_, frame) in frameDict)
            {
                string frameName = $"{frame.Root}: {frame.Name}";
                if (pointCloudConfigObjects.Exists(obj => obj.name == frameName)) continue;

                GameObject newConfigObj = Instantiate(pointCloudConfigPrefab, scrollViewContent.transform);
                newConfigObj.name = frameName;
                newConfigObj.GetComponent<PointCloudConfigItem>().SetFrameName(frameName);
                newConfigObj.GetComponent<PointCloudConfigItem>().SetPointCloudTopics(pointCloudTopics);
                pointCloudConfigObjects.Add(newConfigObj);
            }
        }
    }

    public void Close() =>
        gameObject.SetActive(false);
}
