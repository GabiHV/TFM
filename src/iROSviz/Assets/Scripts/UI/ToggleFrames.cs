using UnityEngine;
using TMPro;

public class ToggleFrames : MonoBehaviour
{
    public GameObject scrollViewContent;
    public TFRoot tFRoot;
    public GameObject frameTogglerPrefab;

    void Start() =>
        InvokeRepeating(nameof(PopulateScrollView), 0.0f, 5.0f);

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
        foreach (var (topic, frames) in tFRoot.GetFramesByTopic())
        {
            foreach (var (frameName, frame) in frames)
            {
                GameObject frameGO = GetItemByName($"{topic}_{frameName}");
                if (frameGO != null) {
                    frameGO.SetActive(true);
                    continue;
                }

                frameGO = Instantiate(frameTogglerPrefab, scrollViewContent.transform, false);

                frameGO.name = $"{topic}_{frameName}";
                var textGO = frameGO.transform.Find("Text").gameObject;
                var text = textGO.GetComponent<TextMeshProUGUI>();
                text.text = $"{topic}: {frameName}";
                
                var button = frameGO.transform.Find("Toggle").GetComponent<UnityEngine.UI.Toggle>();

                // Add toggle functionality
                button.onValueChanged.AddListener(
                    (isOn) => OnToggleValueChanged(isOn, frame)
                );
            }
        }
    }

    private void OnToggleValueChanged(bool isOn, TFRoot.FrameNode frameNode)
    {
        if (frameNode.GO != null)
            frameNode.GO.transform.Find(TFFrameCreator.FrameMarker)?.gameObject.SetActive(isOn);
    }
}
