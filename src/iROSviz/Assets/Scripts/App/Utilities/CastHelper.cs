using UnityEngine;

using App.Exceptions;
using System.Text.RegularExpressions;

namespace App.Utilities
{
    public static class CastHelper
    {
        /// <summary>
        /// Function casts string to float
        /// </summary>
        /// <param name="input">string to cast</param>
        /// <returns>Casted float</returns>
        /// <exception cref="FloatParseException">If cast has not beeen successful</exception>
        public static float CastStringToFloat(string input)
        {
            float castedFloat;
            bool success = float.TryParse(input, out castedFloat);
            if(!success) throw new FloatCastException("Input should be a floating point number");

            return castedFloat;
        }

        /// <summary>
        /// Function casts string to int
        /// </summary>
        /// <param name="input">string to cast</param>
        /// <returns>Casted integer</returns>
        /// <exception cref="IntegerParseException">If cast has not been successful</exception>
        public static int CastStringToInt(string input)
        {
            int castedInt;
            bool success = int.TryParse(input, out castedInt);
            
            if(!success) throw new IntegerCastException("Input should be an integer number");

            return castedInt;
        }

        /// <summary>
        /// Function casts string to double
        /// </summary>
        /// <param name="input">string to cast</param>
        /// <returns>Casted double</returns>
        /// <exception cref="DoubleParseException">If cast has not been successful</exception>
        public static double CastStringToDouble(string input)
        {
            double castedDouble;
            bool success = double.TryParse(input, out castedDouble);

            if(!success) throw new DoubleCastException("Input should be a double number");

            return castedDouble;
        }

        /// <summary>
        /// Function casts string to long number
        /// </summary>
        /// <param name="input">string to cast</param>
        /// <returns>Casted long</returns>
        /// <exception cref="LongCastException">If cast has not been successful</exception>
        public static long CastStringToLong(string input)
        {
            long castedLong;
            bool success = long.TryParse(input, out castedLong);

            if(!success) throw new LongCastException("Input should be a long number");

            return castedLong;
        }

        /// <summary>
        /// Function casts string to boolean
        /// </summary>
        /// <param name="input">string to cast</param>
        /// <returns>Casted boolean</returns>
        /// <exception cref="BoolCastException">If cas has not been successful</exception>
        public static bool CastStringToBool(string input)
        {
            bool castedBool;
            bool success = bool.TryParse(input, out castedBool);

            if (!success) throw new BoolCastException("Input should be a boolean");

            return castedBool;
        }

        /// <summary>
        /// Function casts string to byte
        /// </summary>
        /// <param name="input">string to cast</param>
        /// <returns>Casted byte</returns>
        /// <exception cref="ByteCastException">If cast has not been successful</exception>
        public static byte CastStringToByte(string input)
        {
            byte castedByte;
            bool success = byte.TryParse(input, out castedByte);

            if(!success) throw new ByteCastException("Input should be a byte");

            return castedByte;
        }

        /// <summary>
        /// Function casts string to signed byte
        /// </summary>
        /// <param name="input">string to cast</param>
        /// <returns>Casted signed byte</returns>
        /// <exception cref="SByteCastException">If cast has not been successful</exception>
        public static sbyte CastStringToSByte(string input)
        {
            sbyte castedByte;
            bool success = sbyte.TryParse(input, out castedByte);

            if(!success) throw new SByteCastException("Input should be a byte");

            return castedByte;
        }

        /// <summary>
        /// Function casts string to byte array
        /// </summary>
        /// <param name="input">string to cast</param>
        /// <returns>Casted byte array</returns>
        /// <exception cref="ByteArrayCastException">If byte array is malformed</exception>
        public static byte[] CastStringToByteArray(string input)
        {
            try
            {
                string[] arrayPart = GetArrayParts(input);
                byte[] castedArray = new byte[arrayPart.Length];

                for (int i = 0; i < castedArray.Length; i++)
                {
                    castedArray[i] = CastStringToByte(arrayPart[i]);
                }

                return castedArray;
            }
            catch(CastException)
            {
                throw new ByteArrayCastException("Byte array malformed");    
            }
            
        }

        /// <summary>
        /// Function casts string to signed byte array
        /// </summary>
        /// <param name="input">Casted signed byte array</param>
        /// <returns></returns>
        /// <exception cref="SByteArrayCastException">If signed byte is malformed</exception>
        public static sbyte[] CastStringToSByteArray(string input)
        {
            try
            {
                string[] arrayPart = GetArrayParts(input);
                sbyte[] castedArray = new sbyte[arrayPart.Length];

                for (int i = 0; i < castedArray.Length; i++)
                {
                    castedArray[i] = CastStringToSByte(arrayPart[i]);
                }

                return castedArray;
            }
            catch(CastException)
            {
                throw new SByteArrayCastException("Byte array malformed");    
            }
            
        }

        /// <summary>
        /// Function casts string to boolean array
        /// </summary>
        /// <param name="input">string to cast</param>
        /// <returns>Casted boolean array</returns>
        /// <exception cref="BoolArrayCastException">If boolean array is malformed</exception>
        public static bool[] CastStringToBoolArray(string input)
        {
            try
            {
                string[] arrayPart = GetArrayParts(input);
                bool[] castedArray = new bool[arrayPart.Length];

                for (int i = 0; i < castedArray.Length; i++)
                {
                    castedArray[i] = CastStringToBool(arrayPart[i]);
                }

                return castedArray;
            }
            catch (CastException)
            {
                throw new BoolArrayCastException("Boolean array malformed");
            }
        }

        /// <summary>
        /// Function casts string to double array
        /// </summary>
        /// <param name="input">string to cast</param>
        /// <returns>Casted double array</returns>
        /// <exception cref="DoubleArrayCastException">If double array is malformed</exception>
        public static double[] CastStringToDoubleArray(string input)
        {
            try
            {
                string[] arrayPart = GetArrayParts(input);
                double[] castedArray = new double[arrayPart.Length];

                for (int i = 0; i < castedArray.Length; i++)
                {
                    castedArray[i] = CastStringToDouble(arrayPart[i]);
                }

                return castedArray;
            }
            catch (CastException)
            {
                throw new DoubleArrayCastException("Double array malformed");
            }
        }

        /// <summary>
        /// Function casts string to float array
        /// </summary>
        /// <param name="input">string to cast</param>
        /// <returns>Casted float array</returns>
        /// <exception cref="FloatArrayCastException">If float array is malformed</exception>
        public static float[] CastStringToFloatArray(string input)
        {
            try
            {
                string[] arrayPart = GetArrayParts(input);
                float[] castedArray = new float[arrayPart.Length];

                for (int i = 0; i < castedArray.Length; i++)
                {
                    castedArray[i] = CastStringToFloat(arrayPart[i]);
                }

                return castedArray;
            }
            catch (CastException)
            {
                throw new FloatArrayCastException("Float array malformed");
            }
        }

        /// <summary>
        /// Function casts string to long array
        /// </summary>
        /// <param name="input">string to cast</param>
        /// <returns>Casted long array</returns>
        /// <exception cref="LongArrayCastException">If long array is malformed</exception>
        public static long[] CastStringToLongArray(string input)
        {
            try
            {
                string[] arrayPart = GetArrayParts(input);
                long[] castedArray = new long[arrayPart.Length];

                for (int i = 0; i < castedArray.Length; i++)
                {
                    castedArray[i] = CastStringToLong(arrayPart[i]);
                }

                return castedArray;
            }
            catch (CastException)
            {
                throw new LongArrayCastException("Float array malformed");
            }
        }

        /// <summary>
        /// Function casts string to string array
        /// </summary>
        /// <param name="input">string to cast</param>
        /// <returns>Casted string array</returns>
        /// <exception cref="ArrayMalformed">If string array is malformed</exception>
        public static string[] CastStringToStringArray(string input) =>
            GetArrayParts(input);

        private static string[] GetArrayParts(string input)
        {
            string pattern = @"^\[[^{}]*\]$";
            Regex regEx = new Regex(pattern);
            if(regEx.Match(input).Success) throw new ArrayMalformed("Array malformed");

            return input.Replace(" ", "").Replace("[", "").Replace("]", "").Split(",");
        }

    }    
}

