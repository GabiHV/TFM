using UnityEngine;
using TMPro;

using System;
using System.Linq;
using System.Collections.Generic;

namespace App.Utilities
{
    public static class DropdownHelper
    {
        /// <summary>
        /// Clear input dropdown and set last option if exists
        /// </summary>
        /// <param name="dropdown">Dropdown to modify</param>
        /// <param name="elements">New element list</param>
        /// <param name="getSelectedOption">Function that returns current selected dropdown option </param>
        public static void ClearDropdownAndSetOption<T>(
            TMP_Dropdown dropdown, 
            List<T> elements,
            Func<string> getSelectedOption
        )
        {
            string selectedText = getSelectedOption();
            List<string> castedElements = elements.Select(o => o.ToString()).ToList();
            
            dropdown.ClearOptions();
            dropdown.AddOptions(castedElements);
            dropdown.RefreshShownValue();

            SetDropdownOption(dropdown, selectedText);
        }

        /// <summary>
        /// Function searches in dropdown for the entered text.
        /// </summary>
        /// <param name="dropdown">Dropdown element</param>
        /// <param name="text">TextToSearch</param>
        /// <returns>-1 if element has not been found or element position</returns>
        public static int SearchPositionInDropdown(TMP_Dropdown dropdown, string text)
        {
            int position = 0;
            bool elementFound = false;
            foreach (TMP_Dropdown.OptionData item in dropdown.options)
            {
                if (item.text == text) 
                {
                    elementFound = true;
                    break;
                }
                position ++;
            }
            if(!elementFound) return -1; // If the element has not been found
            return position;
        }

        /// <summary>
        /// Set dropdown selection to specified text value if exists
        /// </summary>
        /// <param name="dropdown">dropdown to set</param>
        /// <param name="selectedText">text to select</param>
        public static void SetDropdownOption(TMP_Dropdown dropdown, string selectedText)
        {
            int possibleNewValue = SearchPositionInDropdown(dropdown, selectedText);
            dropdown.value =  possibleNewValue != -1 ? possibleNewValue : 0;
        }

        /// <summary>
        /// Gets dropdown selected text
        /// </summary>
        /// <param name="dropdown">Dropdown</param>
        /// <returns>Selected text</returns>
        public static string GetDropdownSelectedText(TMP_Dropdown dropdown)
        {
            if(dropdown.options.Count <= 0) return string.Empty;
            return dropdown.options[GetDropdownSelectedValue(dropdown)].text;
        }

        /// <summary>
        /// Gets dropdown selected value
        /// </summary>
        /// <param name="dropdown">Dropdown</param>
        /// <returns>Selected value</returns>
        public static int GetDropdownSelectedValue(TMP_Dropdown dropdown) =>
            dropdown.value;
    }    
}

