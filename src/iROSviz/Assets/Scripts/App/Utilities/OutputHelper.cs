using UnityEngine;

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
    }   
}
