using UnityEngine;
using TMPro;

using App.Exceptions;

using System;

namespace App.Utilities
{
    public static class BoundariesHelper
    {
        private static readonly float _minSFloatValue = float.MinValue / 2;
        private static readonly float _maxSFloatValue = float.MaxValue / 2;
        private static readonly int _minSIntValue = int.MinValue / 2;
        private static readonly int _maxSIntValue = int.MaxValue / 2;
        private static readonly float _minUFloatValue = 0;
        private static readonly float _maxUFloatValue = float.MaxValue / 2;
        private static readonly int _minUIntValue = 0;
        private static readonly int _maxUIntValue = int.MaxValue / 2;

        public static void CheckSFloatInputTextBoundaries(
            TMP_InputField inputField,
            float defaultValue,
            Func<float> getValue
        )
        {
            try
            {
                float value = getValue();
                if (value < _minSFloatValue) inputField.text = _minSFloatValue.ToString();
                if (value > _maxSFloatValue) inputField.text = _maxSFloatValue.ToString();
            }
            catch (FloatCastException)
            {
                inputField.text = defaultValue.ToString();
            }
        }

        public static void CheckUFloatInputTextBoundaries(
            TMP_InputField inputField,
            float defaultValue,
            Func<float> getValue
        )
        {
            try
            {
                float value = getValue();
                if (value < _minUFloatValue) inputField.text = _minUFloatValue.ToString();
                if (value > _maxUFloatValue) inputField.text = _maxUFloatValue.ToString();
            }
            catch (FloatCastException)
            {
                inputField.text = defaultValue.ToString();
            }
        }

        public static void CheckSIntegerInputTextBoundaries(
            TMP_InputField inputField,
            int defaultValue,
            Func<int> getValue
        )
        {
            try
            {
                int value = getValue();
                if (value < _minSIntValue) inputField.text = _minSIntValue.ToString();
                if (value > _maxSIntValue) inputField.text = _maxSIntValue.ToString();
            }
            catch (IntegerCastException)
            {
                inputField.text = defaultValue.ToString();
            }
        }

        public static void CheckUIntegerInputTextBoundaries(
            TMP_InputField inputField,
            int defaultValue,
            Func<int> getValue
        )
        {
            try
            {
                int value = getValue();
                if (value < _minUIntValue) inputField.text = _minUIntValue.ToString();
                if (value > _maxUIntValue) inputField.text = _maxUIntValue.ToString();
            }
            catch (IntegerCastException)
            {
                inputField.text = defaultValue.ToString();
            }
        }

        public static void CheckCustomNumberInputTextBoundaries(
            TMP_InputField inputField,
            int defaultValue,
            int minValue,
            int maxValue,
            Func<int> getValue
        )
        {
            try
            {
                int value = getValue();
                if(value < minValue) inputField.text = minValue.ToString();
                if(value > maxValue) inputField.text = maxValue.ToString();
            }
            catch (CastException)
            {
                inputField.text = defaultValue.ToString();
            }
        }

        public static void IncrementUIntegerField(
            TMP_InputField inputField,
            Func<int> getValue
        ) => IncrementIntegerField(inputField, _maxUIntValue, getValue);

        public static void IncrementSIntegerField(
            TMP_InputField inputField,
            Func<int> getValue
        ) => IncrementIntegerField(inputField, _maxSIntValue, getValue);

        private static void IncrementIntegerField(
            TMP_InputField inputField,
            int maxValue,
            Func<int> getValue
        )
        {
            try
            {
                int value = getValue();
                if (value != maxValue) value++;
                inputField.text = value.ToString();
            }
            catch (IntegerCastException)
            {
                inputField.text = maxValue.ToString();
            }
        }

        public static void DecrementUIntegerField(
            TMP_InputField inputField,
            Func<int> getValue
        ) => DecrementIntegerField(inputField, _minUIntValue, getValue);

        public static void DecrementSIntegerField(
            TMP_InputField inputField,
            Func<int> getValue
        ) => DecrementIntegerField(inputField, _minSIntValue, getValue);

        private static void DecrementIntegerField(
            TMP_InputField inputField,
            int minValue,
            Func<int> getValue
        )
        {
            try
            {
                int value = getValue();
                if (value != minValue) value--;
                inputField.text = value.ToString();
            }
            catch (IntegerCastException)
            {
                inputField.text = minValue.ToString();
            }
        }

        public static void IncrementUFloatField(
            TMP_InputField inputField,
            Func<float> getValue,
            float step = 1.0f
        ) => IncrementFloatField(inputField, _maxUFloatValue, getValue, step);

        public static void IncrementSFloatField(
            TMP_InputField inputField,
            Func<float> getValue,
            float step = 1.0f
        ) => IncrementFloatField(inputField, _maxSFloatValue, getValue, step);

        private static void IncrementFloatField(
            TMP_InputField inputField,
            float maxValue,
            Func<float> getValue,
            float step = 1.0f
        )
        {
            try
            {
                float value = getValue();
                if (value != maxValue) value += step;
                inputField.text = value.ToString();
            }
            catch (FloatCastException)
            {
                inputField.text = maxValue.ToString();
            }
        }

        public static void DecrementUFloatField(
            TMP_InputField inputField,
            Func<float> getValue,
            float step = 1.0f
        ) => DecrementFloatField(inputField, _minUFloatValue, getValue, step);

        public static void DecrementSFloatField(
            TMP_InputField inputField,
            Func<float> getValue,
            float step = 1.0f
        ) => DecrementFloatField(inputField, _minSFloatValue, getValue, step);

        private static void DecrementFloatField(
            TMP_InputField inputField,
            float minValue,
            Func<float> getValue,
            float step = 1.0f
        )
        {
            try
            {
                float value = getValue();
                if (value != minValue) value -= step;
                inputField.text = value.ToString();
            }
            catch (FloatCastException)
            {
                inputField.text = minValue.ToString();
            }
        }

    }
}

