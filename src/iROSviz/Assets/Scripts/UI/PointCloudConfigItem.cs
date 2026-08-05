using UnityEngine;
using TMPro;

using System.Collections.Generic;
using System.Linq;

using App.Utilities;

public class PointCloudConfigItem : MonoBehaviour
{
    public TMP_Dropdown pointCloudDropdown;
    public TMP_Text frameNameText;

    private string frameName;
    private List<string> pointCloudTopics = new List<string>();
    private string previousSelectedTopic;

    // Update is called once per frame
    void Update() =>
        UpdateDropdownOptions();

    public void SetFrameName(string frameName)
    {
        this.frameName = frameName;
        frameNameText.text = frameName;
    }

    public void SetPointCloudTopics(List<string> pointCloudTopics) =>
        this.pointCloudTopics = pointCloudTopics;

    private void UpdateDropdownOptions()
    {
        List<string> currentOptions = pointCloudTopics.ToArray().ToList();
        currentOptions.Add("");
        DropdownHelper.ClearDropdownAndSetOption(
            pointCloudDropdown, 
            currentOptions, 
            GetSelectedPointCloudTopic
        );
    }

    private string GetSelectedPointCloudTopic() =>
        DropdownHelper.GetDropdownSelectedText(pointCloudDropdown);

    public void OnDropdownValueChanged()
    {
        string selectedTopic = GetSelectedPointCloudTopic();
        if(selectedTopic == previousSelectedTopic)
            return;
        previousSelectedTopic = selectedTopic;

        if(string.IsNullOrEmpty(selectedTopic) 
            && PointCloudDatabase.TryGetPointCloudTopic(frameName, out string existingTopic) 
            && !string.IsNullOrEmpty(existingTopic))
                DeletePointCloud();
        
        if(!string.IsNullOrEmpty(selectedTopic)) StorePointCloud();
    }

    private void StorePointCloud()
    {
        PointCloudDatabase.StorePointCloud(frameName, GetSelectedPointCloudTopic(), frameName);
        Debug.Log($"Stored point cloud for frame {frameName} with topic {GetSelectedPointCloudTopic()}");
        PointCloudDatabase.SaveDatabase();
    }

    private void DeletePointCloud()
    {
        PointCloudDatabase.DeletePointCloud(frameName);
        Debug.Log($"Deleted point cloud for frame {frameName}");
        PointCloudDatabase.SaveDatabase();
    }
}
