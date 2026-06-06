using UnityEngine;
using TMPro;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App.Utilities;
using App.ROSUtilities;

public class ModalTagNameSelector : MonoBehaviour
{
    private static ModalTagNameSelector _instance;
    private static List<string> tagNames = new();

    public TMP_Dropdown namespaceDropdown;
    public TMP_Dropdown visualizationDropdown;
    private bool nodesAreBeingRetrieved = false;
    public TFRoot tFRoot;

    private enum Result {None, Ok};
    private Result result;

    public static IEnumerator ShowDialog()
    {
        if(_instance == null) yield break;
        _instance.ShowWindow();

        if(!_instance.nodesAreBeingRetrieved)
        {
            _instance.nodesAreBeingRetrieved = true;
            _instance.InvokeRepeating(nameof(_instance.RefreshTagNames), 0f, 5f);
            _instance.InvokeRepeating(nameof(_instance.RefreshVisualizationDropdown), 0f, 5f);
        }

        yield return new WaitWhile(() => _instance.result == Result.None);

        _instance.DismissWindow();
    }

    public static void DismissDialog()
    {
        if(_instance == null) return;
        _instance.DismissWindow();
    }

    public static string GetNamespaceResult() =>
        _instance == null ? string.Empty : _instance.GetTagName();

    public static string GetVisualizationResult() =>
        _instance == null ? string.Empty : _instance.GetVisualization();

    void Awake()
    {
        _instance = this;
        gameObject.SetActive(false);
    }

    private async Task RefreshTagNames()
    {
        HashSet<string> nodes = await ROSRobotListService.RefreshRobotList();

        if(nodes.Count == 0) return;
        List<string> newTagNames = nodes.ToList();

        tagNames = newTagNames;
        RefreshTagDropdown();
    }

    private void RefreshTagDropdown() =>
        DropdownHelper.ClearDropdownAndSetOption(
            namespaceDropdown, 
            tagNames, 
            GetTagName
        );
    
    private void RefreshVisualizationDropdown() =>
    DropdownHelper.ClearDropdownAndSetOption(
        visualizationDropdown, 
        GetViusalizationOptions(), 
        GetVisualization
    );

    private List<string> GetViusalizationOptions()
    {
        if(tFRoot == null) return new List<string>();
        var options = tFRoot.frames[GetTagName()]?.Values.Select(frame => frame.Name).ToList();
        return options ?? new List<string>();
    }

    private void ShowWindow() =>
        this.gameObject.SetActive(true);
    
    private void DismissWindow() 
    {
        CancelInvoke(nameof(RefreshTagNames));
        CancelInvoke(nameof(RefreshVisualizationDropdown));

        this.result = Result.None;

        this.gameObject.SetActive(false);
        this.nodesAreBeingRetrieved = false;
    }

    private string GetTagName() =>
        DropdownHelper.GetDropdownSelectedText(namespaceDropdown);

    private int GetTagId() =>
        namespaceDropdown.value;
    
    private string GetVisualization() =>
        DropdownHelper.GetDropdownSelectedText(visualizationDropdown);
    
    private int GetVisualizationId() =>
        visualizationDropdown.value;

    public void Confirm() =>
        this.result = Result.Ok;
    
}