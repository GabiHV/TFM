using UnityEngine;
using UnityEngine.Events;
using TMPro;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App.Utilities;
using App.ROSUtilities.Resolvers;
using App.ROSUtilities.Services;

public class ParamChanger : MonoBehaviour
{
    public TMP_Dropdown nodeDropdown;
    public TMP_Dropdown paramDropdown;
    public TMP_Text paramValueText;
    public TMP_InputField paramValueInput;

    private Dictionary<string, Dictionary<string, (object Value, byte Type)>> _nodesWithParams;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() =>  
        StartCoroutine(GetNodesWithParams());

    private IEnumerator GetNodesWithParams()
    {
        while (true)
        {
           _nodesWithParams = ROSParameterListService.GetParameters();

            AddNodesToDropdown();
            ChangeParamsDropdown();
            ChangeParamValue();

            yield return new WaitForSeconds(2);
        }
    }

    private void AddNodesToDropdown() =>
        DropdownHelper.ClearDropdownAndSetOption(
            nodeDropdown,
            new List<string>(_nodesWithParams.Keys),
            GetSelectedNodeText
        );

    public void ChangeParamsDropdown()
    {
        string selectedNode = GetSelectedNodeText();
        if (string.IsNullOrEmpty(selectedNode)) return;

        List<string> parameters = 
            new(_nodesWithParams[selectedNode].Keys);

        DropdownHelper.ClearDropdownAndSetOption(
            paramDropdown,
            parameters,
            GetSelectedParamText
        );
    }
    
    public void ChangeParamValue()
    {
        object value = GetSelectedParamValue();

        paramValueText.text = OutputHelper.ObjectToString(value);
    }

    public void Save()
    {
        string selectedNode = GetSelectedNodeText();
        string selectedParam = GetSelectedParamText();
        byte? type = GetSelectedParamType();
        string value = GetInputValue();

        if(type == null) return;

        object valueObj = ROSResolver.GetCastedParameterValue(type ?? 0, value);
        Debug.Log($"Selected node: {selectedNode}. Selected param: {selectedParam}. Value: {value}");
        ROSParameterSetService.SetParameter(selectedNode, selectedParam, type ?? 0, valueObj);
    }

    private object GetSelectedParamValue()
    {
        string selectedNode = GetSelectedNodeText();
        string selectedParam = GetSelectedParamText();

        object value = IsNodeAndParamInDict(selectedNode, selectedParam) ? 
            _nodesWithParams[selectedNode][selectedParam].Value:
            string.Empty;
        return value;
    }

    private byte? GetSelectedParamType()
    {
        string selectedNode = GetSelectedNodeText();
        string selectedParam = GetSelectedParamText();

        byte? type = IsNodeAndParamInDict(selectedNode, selectedParam) ? 
            _nodesWithParams[selectedNode][selectedParam].Type:
            null;
        return type;
    }

    private bool IsNodeAndParamInDict(string node, string param) =>
        _nodesWithParams.ContainsKey(node) &&
            _nodesWithParams[node].ContainsKey(param);

    private string GetSelectedNodeText() =>
        DropdownHelper.GetDropdownSelectedText(nodeDropdown);

    private string GetSelectedParamText() =>
        DropdownHelper.GetDropdownSelectedText(paramDropdown);

    private int GetSelectedNode() =>
        DropdownHelper.GetDropdownSelectedValue(nodeDropdown);

    private int GetSelectedParam() =>
        DropdownHelper.GetDropdownSelectedValue(paramDropdown);

    private string GetInputValue() =>
        paramValueInput.text;

    public void Close() =>
        gameObject.SetActive(false);
    
}
