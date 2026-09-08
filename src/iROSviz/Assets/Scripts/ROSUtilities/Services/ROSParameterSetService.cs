using UnityEngine;
using RosMessageTypes.Std;
using RosMessageTypes.RclInterfaces;

using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace App.ROSUtilities.Services
{    
    public class ROSParameterSetService : ROSService<List<SetParametersResponse>, SetParametersResponse>
    {
        private static ROSParameterSetService _instance;
        private string targetNode;
        private string paramName;
        private byte paramType;
        private object paramValue;

        private ROSParameterSetService() =>
            base.result = new();

        private static ROSParameterSetService GetOrCreateInstance()
        {
            if(_instance == null) _instance = new();
            return _instance;
        }

        public static void SetParameter(
            string targetNode, 
            string paramName, 
            byte paramType, 
            object paramValue
        )
        {
            GetOrCreateInstance().SetNodeName(targetNode);
            GetOrCreateInstance().SetParamName(paramName);
            GetOrCreateInstance().SetParamType(paramType);
            GetOrCreateInstance().SetParamValue(paramValue);
            GetOrCreateInstance().GetResult();
        }

        protected override void InvokeServices()
        {
            ParameterValueMsg paramValueMsg = 
                GetParameterValueMsg(paramType, paramValue);
            ParameterMsg parameter = new(paramName, paramValueMsg);

            base.InvokeService(
                $"/{targetNode}/set_parameters",
                new SetParametersRequest(new[] {parameter}),
                new SetParametersResponse(),
                null
            );
        }
        
        private ParameterValueMsg GetParameterValueMsg(byte type, object value)
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

        private void SetNodeName(string targetNode) =>
            this.targetNode = targetNode;

        private void SetParamName(string paramName) =>
            this.paramName = paramName;

        private void SetParamType(byte paramType) =>
            this.paramType = paramType;

        private void SetParamValue(object paramValue) =>
            this.paramValue = paramValue;
    }
}
