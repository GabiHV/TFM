using UnityEngine;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.ROSUtilities.Services
{
    public abstract class ROSService<TResult, TItem> : IROSService<TResult> 
        where TResult : ICollection<TItem>
    {
        protected TResult result;

        public TResult GetResult()
        {
            InvokeServices();
            return result;
        }

        public bool HasResult() =>
            result != null && result.Count > 0;

        protected async void InvokeService(
            string service, 
            Message request, 
            Message response,
            Action<Message> callback
        ) => await ROSServicesHelper.InvokeService(
                service,
                request,
                response,
                callback
            );

        protected abstract void InvokeServices();
    }   
}
    
