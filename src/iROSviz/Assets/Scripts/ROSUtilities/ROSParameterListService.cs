using UnityEngine;
using TMPro;
using RosMessageTypes.Std;
using RosMessageTypes.RclInterfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace App.ROSUtilities
{
    public static class ROSParameterListService
    {
        private static Dictionary<string, Dictionary<string, (object, byte)>> _parametersDict = new();
        private static Task currentServerInvoke;
        public static Dictionary<string, Dictionary<string, (object, byte)>> GetOrLoadParameterList()
        {
            if (!IsParameterListEmpty())  return _parametersDict;

            LoadNodeList();
            return _parametersDict;
        }

        public static Dictionary<string, Dictionary<string, (object, byte)>> RefreshParameterList()
        {
            LoadNodeList();

            return _parametersDict;
        }

        public static async void SetParameter(
            string nodeName, 
            string paramName, 
            byte paramType, 
            object paramValue
        )
        {
            ParameterValueMsg paramValueMsg = GetParameterValueMsg(paramType, paramValue);
            ParameterMsg parameter = new(paramName, paramValueMsg);
            await ROSServicesHelper.InvokeService(
                $"/{nodeName}/set_parameters",
                new SetParametersRequest(new[] {parameter}),
                new SetParametersResponse(),
                null
            );
        }

        private static bool IsParameterListEmpty() =>
            _parametersDict == null || _parametersDict.Count <= 0;

        private static void LoadNodeList()
        {
            if (currentServerInvoke == null || currentServerInvoke.IsCompleted)
                currentServerInvoke = ROSServicesHelper.InvokeService(
                    "/get_nodes",
                    new SetBoolRequest(),
                    new SetBoolResponse(),
                    async msg => await LoadNodes((SetBoolResponse)msg)
                );
        }

        private static async Task LoadParameterList(string node) => 
            await ROSServicesHelper.InvokeService(
                $"/{node}/list_parameters",
                new ListParametersRequest(),
                new ListParametersResponse(),
                async msg => await LoadParameters((ListParametersResponse)msg, node)
            );
        
        private static async Task LoadParameterValues(string node, string[] parameters) => 
            await ROSServicesHelper.InvokeService(
                $"/{node}/get_parameters",
                new GetParametersRequest(parameters),
                new GetParametersResponse(),
                msg => LoadParametersValues((GetParametersResponse)msg, node, parameters)
            );
        
        private static async Task LoadNodes(SetBoolResponse res)
        {
            string[] nodes = res.message.Split("\n");
            IEnumerable<string> oldKeys = _parametersDict.Keys.Where(k => !nodes.Any(n => n != k));
            foreach (string key in oldKeys)
            {
                _parametersDict.Remove(key);
            }

            foreach (string node in nodes)
            {

                if(string.IsNullOrEmpty(node)) continue;
                if(node.Contains("_RosService") || 
                    node.Contains("_RosSubscriber") || 
                    node.Contains("_RosPublisher")) continue;

                if(!_parametersDict.ContainsKey(node))
                    _parametersDict.Add(node, new Dictionary<string, (object, byte)>());
                await LoadParameterList(node);
            }

        }
        
        private static async Task LoadParameters(ListParametersResponse res, string node)
        {
           string[] parameters =  res.result.names;
           
           await LoadParameterValues(node, parameters);
        }

        private static void LoadParametersValues(
            GetParametersResponse res, 
            string node, 
            string[] parameters
        )
        {
            ParameterValueMsg[] values = res.values;

            if(parameters.Length != values.Length) return;

            IEnumerable<string> oldKeys = 
                _parametersDict[node].Keys.ToList().Where(k => !parameters.Any(p => p != k));
            foreach (string key in oldKeys)
            {
                _parametersDict[node].Remove(key);
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterValueMsg value = values[i];
                string parameter = parameters[i];

                if(!_parametersDict[node].ContainsKey(parameter))
                    _parametersDict[node].Add(parameter, new());

                _parametersDict[node][parameter] = (GetField(value.type, value), value.type);
            }
        }

        private static object GetField(byte type, ParameterValueMsg value)
        {
            object field = new();
            switch (type)
            {                        
                case ParameterTypeMsg.PARAMETER_BOOL:
                    field = value.bool_value;
                    break;
                case ParameterTypeMsg.PARAMETER_INTEGER:
                    field = value.integer_value;
                    break;
                case ParameterTypeMsg.PARAMETER_DOUBLE:
                    field = value.double_value;
                    break;
                case ParameterTypeMsg.PARAMETER_STRING:
                    field = value.string_value;
                    break;
                case ParameterTypeMsg.PARAMETER_BYTE_ARRAY:
                    field = value.byte_array_value;
                    break;
                case ParameterTypeMsg.PARAMETER_BOOL_ARRAY:
                    field = value.bool_array_value;
                    break;
                case ParameterTypeMsg.PARAMETER_INTEGER_ARRAY:
                    field = value.integer_array_value;
                    break;
                case ParameterTypeMsg.PARAMETER_DOUBLE_ARRAY:
                    field = value.double_array_value;
                    break;
                case ParameterTypeMsg.PARAMETER_STRING_ARRAY:
                    field = value.string_array_value;
                    break;
                case ParameterTypeMsg.PARAMETER_NOT_SET:
                    break;
                default:
                    break;
            }
            return field;
        }

        private static ParameterValueMsg GetParameterValueMsg(byte type, object value)
        {
            ParameterValueMsg paramValue = new();
            paramValue.type = type;
            switch (type)
            {
                case ParameterTypeMsg.PARAMETER_BOOL:
                    paramValue.bool_value = (bool)value;
                    break;
                case ParameterTypeMsg.PARAMETER_INTEGER:
                    paramValue.integer_value = (long)value;
                    break;
                case ParameterTypeMsg.PARAMETER_DOUBLE:
                    paramValue.double_value = (double)value;
                    break;
                case ParameterTypeMsg.PARAMETER_STRING:
                    paramValue.string_value = (string)value;
                    break;
                case ParameterTypeMsg.PARAMETER_BYTE_ARRAY:
                    paramValue.byte_array_value = (sbyte[])value;
                    break;
                case ParameterTypeMsg.PARAMETER_BOOL_ARRAY:
                    paramValue.bool_array_value = (bool[])value;
                    break;
                case ParameterTypeMsg.PARAMETER_INTEGER_ARRAY:
                    paramValue.integer_array_value = (long[])value;
                    break;
                case ParameterTypeMsg.PARAMETER_DOUBLE_ARRAY:
                    paramValue.double_array_value = (double[])value;
                    break;
                case ParameterTypeMsg.PARAMETER_STRING_ARRAY:
                    paramValue.string_array_value = (string[])value;
                    break;
                case ParameterTypeMsg.PARAMETER_NOT_SET:
                    break;
                default:
                    break;
            }
            return paramValue;
        }
    }    
}

