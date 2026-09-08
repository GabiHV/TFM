using UnityEngine;
using RosMessageTypes.Std;
using RosMessageTypes.RclInterfaces;

using System.Linq;
using System.Collections.Generic;

using App.ROSUtilities.Subscribers;

namespace App.ROSUtilities.Services
{
    public class ROSParameterListService : 
        ROSService<Dictionary<string, Dictionary<string, (object, byte)>>, KeyValuePair<string, Dictionary<string, (object, byte)>>>
    {
        private static ROSParameterListService _instance;
        private object lockObj = new();

        private ROSParameterListService()
        {
            base.result = new();
        }

        public static Dictionary<string, Dictionary<string, (object, byte)>> GetParameters() =>
            GetOrCreateInstance().GetResult();

        private static ROSParameterListService GetOrCreateInstance()
        {
            if(_instance == null) _instance = new();
            return _instance;
        }

        protected override void InvokeServices()
        {
            HashSet<string> nodes = ROSNodeInfoSubscriber.GetOrLoadNodeList();
            foreach(string node in nodes)
                GetOrCreateInstance().InvokeService(
                    $"/{node}/list_parameters",
                    new ListParametersRequest(),
                    new ListParametersResponse(),
                    msg => LoadParametersCallback((ListParametersResponse)msg, node)
                );
        }

        private void LoadParametersCallback(ListParametersResponse res, string node)
        {
           string[] parameters =  res.result.names;
           Debug.Log($"Response from {node}");
           
           GetParametersValues(node, parameters);
        }

        private void GetParametersValues(string node, string[] parameters)
        {
            base.InvokeService(
                $"/{node}/get_parameters",
                new GetParametersRequest(parameters),
                new GetParametersResponse(),
                msg => LoadParametersValuesCallback(
                    (GetParametersResponse)msg, 
                    node, 
                    parameters)
            );
        }
        
        private void LoadParametersValuesCallback(
            GetParametersResponse res, 
            string node, 
            string[] parameters
        )
        {
            ParameterValueMsg[] values = res.values;
            if(parameters.Length != values.Length) return;

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterValueMsg value = values[i];
                string parameter = parameters[i];

                lock (lockObj)
                {
                    if(!base.result.ContainsKey(node))
                        base.result.Add(node, new());

                    if(!base.result[node].ContainsKey(parameter))
                        base.result[node].Add(parameter, new());

                    base.result[node][parameter] = (GetField(value.type, value), value.type);
                }
            }
        }

        private object GetField(byte type, ParameterValueMsg value)
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
    }    
}

