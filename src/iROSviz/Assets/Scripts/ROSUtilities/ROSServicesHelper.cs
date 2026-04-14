using UnityEngine;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using Unity.Robotics.ROSTCPConnector;


using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System.Net.Mail;

#nullable enable

namespace App.ROSUtilities
{
    public static class ROSServicesHelper
    {
        private static ROSConnection _ros = ROSConnection.GetOrCreateInstance();


        public static async Task InvokeService(string serviceName, 
            Message requestMessage, 
            Message responseMessage,
            Action<Message>? callback)
        {
            _ros.RegisterRosService(
                serviceName, 
                requestMessage.RosMessageName, 
                responseMessage.RosMessageName
            );
            
            await SendServiceMessage(serviceName, 
                requestMessage, 
                responseMessage, 
                callback
            );
        }

        private static async Task SendServiceMessage(
            string serviceName, 
            Message requestMessage, 
            Message responseMessage, 
            Action<Message>? callback
        )
        {
            Type responseType = responseMessage.GetType();

            IEnumerable<MethodInfo>? methods = typeof(ROSConnection)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.Name == "SendServiceMessage");

            MethodInfo? method = methods.Where(m => m.ReturnType != typeof(void)).FirstOrDefault();
            
            if (method == null) return;

            method = method.MakeGenericMethod(responseType);
    
            object[] paramsObj = 
                new object[] 
                { 
                    serviceName, 
                    requestMessage
                };

            Message? response = (Message?)await InvokeAsync(method, _ros, paramsObj) ?? 
                (Message)Activator.CreateInstance(responseType);

            if(callback != null && response != null) callback(response);
        }


        private static async Task<object?> InvokeAsync(
            MethodInfo? method, 
            object obj, 
            object[] parameters
        )
        {
            if (method == null) return null;

            var result = method.Invoke(obj, parameters);

            if (result is Task task)
            {
                await task.ConfigureAwait(false);

                var taskType = task.GetType();
                if (taskType.IsGenericType)
                    return taskType.GetProperty("Result")!.GetValue(task);

                return null;
            }

            return result;
        }

    }   
}
