using UnityEngine;
using TMPro;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App.Utilities;
using App.ROSUtilities.Subscribers;

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
        }

        yield return new WaitWhile(() => _instance.result == Result.None);

        _instance.DismissWindow();
    }

    public static void DismissDialog()
    {
        if(_instance == null) return;
        _instance.DismissWindow();
    }

    public static string GetTagName() =>
        _instance == null ? string.Empty : _instance.GetSelectedTagName();

    public static string GetFrame()
    {
        if(_instance == null) return string.Empty;
        try
        {
            return _instance.GetSelectedTagName().Split(":")[1];
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    void Awake()
    {
        _instance = this;
        gameObject.SetActive(false);
    }

    private async Task RefreshTagNames()
    {
        HashSet<string> nodes = ROSRobotInfoSubscriber.GetOrLoadRobotList();

        if(nodes.Count == 0) return;
        List<string> newTagNames = nodes.ToList();

        tagNames = newTagNames;
        RefreshTagDropdown();
    }

    private void RefreshTagDropdown() =>
        DropdownHelper.ClearDropdownAndSetOption(
            namespaceDropdown,
            tagNames,
            GetSelectedTagName
        );

    private void ShowWindow() =>
        this.gameObject.SetActive(true);

    private void DismissWindow()
    {
        CancelInvoke(nameof(RefreshTagNames));

        this.result = Result.None;

        this.gameObject.SetActive(false);
        this.nodesAreBeingRetrieved = false;
    }

    private string GetSelectedTagName() =>
        DropdownHelper.GetDropdownSelectedText(namespaceDropdown);

    private int GetTagId() =>
        namespaceDropdown.value;

    public void Confirm() =>
        this.result = Result.Ok;

}