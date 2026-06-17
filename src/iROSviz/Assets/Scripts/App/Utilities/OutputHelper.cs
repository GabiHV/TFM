using UnityEngine;

using System;
using System.Collections;

namespace App.Utilities
{
    public static class OutputHelper
    {
        /// <summary>
        /// Gets the object string representation
        /// </summary>
        /// <param name="obj">object to represent</param>
        /// <returns>object string representation</returns>
        public static string ObjectToString(object obj)
        {
            if(obj.GetType().IsArray) return GetArrayToString(obj);

            return obj.ToString();
        }

        private static string GetArrayToString(object obj)
        {
            ICollection collection = (ICollection)obj;
            int lastItemPos = collection.Count;
            int counter = 0;
            string objString = "[";
            foreach (var item in collection)
            {
                objString += $"{item}"; 
                if(counter != lastItemPos - 1) objString += ", ";

                counter++;
            }
            objString += "]";
            return objString;
        }

        public static string RemoveStarter(string target, string starter)
        {
            if(!target.StartsWith(starter)) return target;
            try
            {
                return target.Substring(starter.Length);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string AddStarter(string target, string starter) =>
            $"{starter}{target}";
        
        public static string RemoveTerminator(string target, string terminator)
        {
            if(!target.EndsWith(terminator)) return target;
            try
            {
                return target.Substring(0, target.Length - terminator.Length);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string AddTerminator(string target, string terminator) =>
            $"{target}{terminator}";
    }   
}
