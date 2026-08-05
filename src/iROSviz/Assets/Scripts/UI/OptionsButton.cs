using UnityEngine;

public class OptionsButton : MonoBehaviour
{
    public GameObject optionsPanel;

    private bool isOptionsVisible = false;

    public void OnClick()
    {
        isOptionsVisible = !isOptionsVisible;
        optionsPanel.SetActive(isOptionsVisible);
    }
}
