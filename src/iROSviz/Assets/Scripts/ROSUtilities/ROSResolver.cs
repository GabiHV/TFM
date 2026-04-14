using UnityEngine;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

using RosMessageTypes.RclInterfaces;

using System;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using App.Utilities;
using App.Exceptions;

namespace App.ROSUtilities
{
    public static class ROSResolver
    {
        private static readonly string _msg = "_msgs"; // Messages folder name
        private static readonly string _srvs = "_srvs"; // Server messages folder name 
        private static readonly string _msgTerminator = "Msg";
        private static readonly string _srvsTerminatorResponse = "Response";
        private static readonly string _srvsTerminatorRequest = "Request";

        public static Type GetMessageType(string rosType) =>
            GetType(rosType, _msg, _msgTerminator, 0, 2);

        public static Type GetServiceRequestType(string rosType) =>
            GetType(rosType, _srvs, _srvsTerminatorRequest, 0, 1);
        
        public static Type GetServiceResponseType(string rosType) =>
            GetType(rosType, _srvs, _srvsTerminatorResponse, 0, 1);

        public static Message InstantiateMessage(string rosType, string message)
        {
            try
            {
                Type messageType = GetMessageType(rosType);
                Message messageObj = (Message)JsonUtility.FromJson(
                    JsonConvert.DeserializeObject(message).ToString(), 
                    messageType
                );

                return messageObj;
            }
            catch (JsonException ex)
            {
                throw new JSONParseException("JSON malformed", ex);
            }
        }

        public static string GetMessageNameWithoutType(string rosType)
        {
            // ROS message type consists of package, folder and name
            // Folder can be "msg" or "srv".
            return rosType.Replace("/msg", "").Replace("/srv", "");
        }

        public static object GetCastedParameterValue(byte type, string value)
        {
            object valueObj = null;
            switch (type)
            {
                case ParameterTypeMsg.PARAMETER_BOOL:
                    valueObj = CastHelper.CastStringToBool(value);
                    break;
                case ParameterTypeMsg.PARAMETER_INTEGER:
                    valueObj = CastHelper.CastStringToLong(value);
                    break;
                case ParameterTypeMsg.PARAMETER_DOUBLE:
                    valueObj = CastHelper.CastStringToDouble(value);
                    break;
                case ParameterTypeMsg.PARAMETER_STRING:
                    valueObj = value;
                    break;
                case ParameterTypeMsg.PARAMETER_BYTE_ARRAY:
                    valueObj = CastHelper.CastStringToSByteArray(value);
                    break;
                case ParameterTypeMsg.PARAMETER_BOOL_ARRAY:
                    valueObj = CastHelper.CastStringToBoolArray(value);
                    break;
                case ParameterTypeMsg.PARAMETER_INTEGER_ARRAY:
                    valueObj = CastHelper.CastStringToLongArray(value);
                    break;
                case ParameterTypeMsg.PARAMETER_DOUBLE_ARRAY:
                    valueObj = CastHelper.CastStringToDoubleArray(value);
                    break;
                case ParameterTypeMsg.PARAMETER_STRING_ARRAY:
                    valueObj = CastHelper.CastStringToStringArray(value);
                    break;
                case ParameterTypeMsg.PARAMETER_NOT_SET:
                    break;
                default:
                    break;
            }
            return valueObj;
        }

        private static Type GetType(string rosType, 
            string packageTerminator, 
            string classTerminator,
            int packagePosition,
            int classPosition)
        {
            // ROS message type consists of different parts divided by slash
            string[] parts = rosType.Split("/");
            if (parts.Length < packagePosition) return null;
            if (parts.Length < classPosition) return null;
            
            // Package part consists of multiple strings separated by underscores
            // "msg" and "srv" is not included in Uity package namespaces
            string package = parts[packagePosition];
            string[] unitySubNamespaceParts = package.Replace(packageTerminator, "").
                Split("_");
            
            string unitySubNamespace = string.Empty;
            // Concat subnamespace parts capitalizing the first letter
            foreach (string stringPart in unitySubNamespaceParts)
            {
                unitySubNamespace += Char.ToUpper(stringPart[0]) + 
                    stringPart.Substring(1);
            }

            // Name classes are composed by ROS type name part and terminator (Msg, Request or Response)
            // i.e. if we have a Twist message type, Unity message type will be TwistMsg
            string name = parts[classPosition];
            string className = $"{name}{classTerminator}";

            // The complete resource route will be composed by 
            // RosMessageTypes.<subnamespace>.<class_name>
            string fullTypeName = $"RosMessageTypes.{unitySubNamespace}.{className}";

            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType(fullTypeName))
                .FirstOrDefault(t => t != null);
        } 
    }   
}
