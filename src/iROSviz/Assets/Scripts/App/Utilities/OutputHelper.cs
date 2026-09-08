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

        
        public static double Similarity(string a, string b)
        {
            int distance = Levenshtein(a, b);
            int maxLen = Math.Max(a.Length, b.Length);
            return maxLen == 0 ? 1.0 : 1.0 - (double)distance / maxLen;
        }

        public static double PathSimilarity(string refPath, string candidate)
        {
            var refParts = refPath.Trim('/').Split('/');
            var candParts = candidate.Trim('/').Split('/');

            double score = 0;

            int len = Math.Min(refParts.Length, candParts.Length);

            for (int i = 0; i < len; i++)
            {
                score += Similarity(refParts[i], candParts[i]);
            }

            return score;
        }

        private static int Levenshtein(string s, string t)
        {
            int[,] d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s.Length, t.Length];
        }

    }   
}
