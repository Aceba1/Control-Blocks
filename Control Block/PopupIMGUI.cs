// Popup list with multi-instance support, originally created by Xiaohang Miao. (xmiao2@ncsu.edu)

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Control_Block
{
    // Possible change: Make a generic for value types
    public class PopupBase
    {
        public static Popup<InputOperator.InputType> CreateFromInputCategories()
        {
            string[] items = InputOperator.InputCategoryNames;
            var subItems = new List<string[]>();
            var subValues = new List<InputOperator.InputType[]>();
            foreach (InputOperator.InputType[] list in InputOperator.InputCategoryLists)
            {
                subItems.Add(Array.ConvertAll(list, item => item.ToString()));
                subValues.Add(list);
            }
            return new Popup<InputOperator.InputType>(items, subItems, subValues);
        }

        public static Popup<InputOperator.OperationType> CreateFromOperatorCategories()
        {
            string[] items = InputOperator.OperationCategoryNames;
            var subItems = new List<string[]>();
            var subValues = new List<InputOperator.OperationType[]>();
            foreach (InputOperator.OperationType[] list in InputOperator.OperationCategoryLists)
            {
                subItems.Add(Array.ConvertAll(list, item => item.ToString()));
                subValues.Add(list);
            }
            return new Popup<InputOperator.OperationType>(items, subItems, subValues);
        }
    }
    public class Popup<T> : PopupBase
    {
        const float MaxLength = 200;

        public Popup(string[] items)
        {
            this.items = items;
        }

        public Popup(string[] items, List<string[]> subItems, List<T[]> subValues)
        {
            this.items = items;
            this.subItems = subItems;
            this.subValues = subValues;
            hasSubItems = true;
        }

        private string[] items;
        private bool hasSubItems = false;
        private List<string[]> subItems;
        private List<T[]> subValues;

        private Vector2 scroll = Vector2.zero,
            subScroll = Vector2.zero;

        // Represents the selected index of the popup list, the default selected index is 0, or the first item
        public int SelectedIndex = 0;
        public T SelectedValue;
        public string SelectedName = "List...";

        // Represents whether the popup selections are visible (active)

        public bool isVisible { get; set; }
        bool isSubVisible = false;

        // Represents whether the popup button is clicked once to expand the popup selections
        private bool isClicked = false;

        // If multiple Popup objects exist, this static variable represents the active instance, or a Popup object whose selection is currently expanded
        private static PopupBase currentPopup;

        private Rect box;

        public void Hide()
        {
            if (currentPopup == this)
            {
                isVisible = false;
                isSubVisible = false;
                currentPopup = null;
            }
        }

        public void Button()
        {
            if (GUILayout.Button(SelectedName))
            {
                // If the button was not clicked before, set the current instance to be the active instance
                if (!isClicked)
                {
                    currentPopup = this;
                    isClicked = true;
                }
                // If the button was clicked before (it was the active instance), reset the isClicked boolean
                else
                {
                    isClicked = false;
                    if (currentPopup == this) currentPopup = null;
                    isSubVisible = false;
                    isVisible = false;
                }
            }
            box = GUILayoutUtility.GetLastRect();
        }

        public void Show(float CornerOffsetX, float CornerOffsetY)
        {
            if (isVisible)
            {
                Rect inputBox = new Rect(box);
                inputBox.center += new Vector2(CornerOffsetX, CornerOffsetY);
                SelectedIndex = InputPopup(inputBox, false, SelectedIndex, items, ref scroll, out bool changed);

                if (changed)
                {
                    if (hasSubItems)
                    {
                        isSubVisible = true;
                        subScroll = Vector2.zero;
                    }
                    else
                    {
                        currentPopup = null;
                    }
                }

                if (isSubVisible)
                {
                    int SelectedSubIndex = InputPopup(inputBox, true, -1, subItems[SelectedIndex], ref subScroll, out changed);
                    
                    if (changed)
                    {
                        SelectedValue = subValues[SelectedIndex][SelectedSubIndex]; // Set the index to the Enum type
                        SelectedName = subItems[SelectedIndex][SelectedSubIndex]; // Set the render name to the Enum name
                        currentPopup = null;
                    }
                }
            }
        }

        private static int InputPopup(Rect box, bool listOnSide, int selectedItemIndex, string[] items, ref Vector2 scroll, out bool Changed)
        {
            Rect listRect = new Rect(0, 0, box.width - 20, box.height * items.Length);
            Rect scrollRect = listOnSide ?
                new Rect(box.x + box.width, box.y + box.height, box.width, Mathf.Min(box.height * items.Length, MaxLength)) :
                new Rect(box.x, box.y + box.height, box.width, Mathf.Min(box.height * items.Length, MaxLength));

            scroll = GUI.BeginScrollView(scrollRect, scroll, listRect, false, true, GUIStyle.none, GUIStyle.none);

            GUI.changed = false;

            int result = GUI.SelectionGrid(listRect, selectedItemIndex, items, 1, GUIStyle.none);
            Changed = GUI.changed || result != selectedItemIndex;

            GUI.EndScrollView();

            return result;
        }

        private static void DrawPopup(Rect box, bool listOnSide, int selectedItemIndex, string[] items, Vector2 scroll)
        {
            Rect listRect = new Rect(0, 0, box.width - 20, box.height * items.Length);
            Rect scrollRect = listOnSide ?
                new Rect(box.x + box.width, box.y + box.height, box.width, Mathf.Min(box.height * items.Length, MaxLength)) :
                new Rect(box.x, box.y + box.height, box.width, Mathf.Min(box.height * items.Length, MaxLength));
            GUI.Box(scrollRect, "");

            GUI.BeginScrollView(scrollRect, scroll, listRect, false, true);
            
            GUI.SelectionGrid(listRect, selectedItemIndex, items, 1);
            
            GUI.EndScrollView();
        }

        public void List(float CornerOffsetX, float CornerOffsetY)
        {
            // If the instance's popup selection is visible
            if (isVisible)
            {
                Rect drawBox = new Rect(box);
                drawBox.center += new Vector2(CornerOffsetX, CornerOffsetY);
                DrawPopup(drawBox, false, SelectedIndex, items, scroll);
                if (isSubVisible)
                {
                    DrawPopup(drawBox, true, -1, subItems[SelectedIndex], subScroll);
                }
            }

            // Get the control ID
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            // Listen for controls
            switch (Event.current.GetTypeForControl(controlID))
            {
                // If mouse button is clicked, set all Popup selections to be retracted
                case EventType.MouseUp:
                    {
                        currentPopup = null;
                        break;
                    }
            }

            // If the instance is the active instance, set its popup selections to be visible
            if (currentPopup == this)
            {
                isVisible = true;
            }
            else // These resets are here to do some cleanup work for OnGUI() updates
            {
                isVisible = false;
                isSubVisible = false;
                isClicked = false;
            }
        }
    }

}

//http://wiki.unity3d.com/index.php?title=PopupList#C.23_-_Popup.cs