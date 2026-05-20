using UnityEngine;
using TMPro;

public class ToggleFrames : MonoBehaviour
{
    public GameObject scrollViewContent;
    public TFRoot tFRoot;
    public GameObject frameTogglerPrefab;

    void Start() =>
        PopulateScrollView();
    
    void Update() =>
         PopulateScrollView();

    private void PopulateScrollView()
    {
        // Disable existing buttons to avoid losing performance 
        // by destroying and recreating them every frame
        DisableToggleButtons();

        // Create or enable a button for each frame
        CreateOrEnableToggleButton();
    }

    private GameObject GetItemByName(string name) =>
        scrollViewContent.transform.Find(name)?.gameObject;
    
    private void DisableToggleButtons()
    {
        foreach (Transform child in scrollViewContent.transform)
            child.gameObject.SetActive(false);
    }

    private void CreateOrEnableToggleButton()
    {
        foreach (var robotEntry in tFRoot.frames)
        {
            string robotName = robotEntry.Key;
            foreach (var frameEntry in robotEntry.Value)
            {
                string frameName = frameEntry.Key;
                GameObject frameGO = GetItemByName($"{robotName}_{frameName}");
                if (frameGO != null) {
                    frameGO.SetActive(true);
                    continue;
                }

                frameGO = Instantiate(frameTogglerPrefab, scrollViewContent.transform, false);

                frameGO.name = $"{robotName}_{frameName}";
                var textGO = frameGO.transform.Find("Text").gameObject;
                var text = textGO.GetComponent<TextMeshProUGUI>();
                text.text = $"{robotName}: {frameName}";
                
                var button = frameGO.transform.Find("Toggle").GetComponent<UnityEngine.UI.Toggle>();

                // Add toggle functionality
                button.onValueChanged.AddListener(
                    (isOn) => OnToggleValueChanged(isOn, frameEntry.Value)
                );
            }
        }
    }

    private void OnToggleValueChanged(bool isOn, TFRoot.FrameNode frameNode)
    {
        if (frameNode.GO != null)
            frameNode.GO.transform.Find("JointMarker")?.gameObject.SetActive(isOn);
    }
}
