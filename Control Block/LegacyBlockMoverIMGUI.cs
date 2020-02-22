using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Control_Block
{
    static class PresetsIMGUI
    {
        public static bool IsSettingKeybind;
        public static int IndexOfKeybinder;
        public static KeyCode SetKey;
        public static string Log;
        static ModuleBlockMover lastBlockMover;
        static Rect window;

        public static void DoWindow()
        {
            if (lastBlockMover == null) return;
            window = GUI.Window(978243, window, PresetsWindow, "Presets");
        }

        static void Changed()
        {
            OptionMenuMover.SelectedIndex = -1;
            var ui = OptionMenuMover.inst;
            ui.Log = Log;
            ui.Texts = null;
            ui.ResetTextCache();
            OptionMenuMover.showPresetsUI = false;
        }

        internal static void SetState(bool showUI, ModuleBlockMover blockMover, Rect refWindow)
        {
            if (showUI)
            {
                lastBlockMover = blockMover;
                float width = 600, height = 500;
                window = new Rect(refWindow.center.x - width * 0.5f, refWindow.center.y - height * 0.5f, width, height);
                GUI.FocusWindow(978243);
            }
            else
            {
                lastBlockMover = null;
            }
        }

        private static Popup<Preset> PresetsPopup = CreatePresetsPopup();

        static Popup<Preset> CreatePresetsPopup()
        {
            string[] items = new string[] { "Primitives", "Legacy Piston", "Legacy Swivel" };
            var subItems = new List<string[]>();
            var subValues = new List<Preset[]>();
            //foreach (var list in PreConverter.PreOverhaulSwivels)
            //{
            subItems.Add(new string[0]);
            subValues.Add(new Preset[0]);
            subItems.Add(PreConverter.PreOverhaulPistons.Keys.ToArray());
            subValues.Add(PreConverter.PreOverhaulPistons.Values.ToArray());
            subItems.Add(PreConverter.PreOverhaulSwivels.Keys.ToArray());
            subValues.Add(PreConverter.PreOverhaulSwivels.Values.ToArray());
            //}
            return new Popup<Preset>(items, subItems, subValues);
        }

        static bool HasPreset = false;
        static Vector2 scroll;
        private static void PresetsWindow(int ID)
        {
            if (IsSettingKeybind)
            {
                if (IndexOfKeybinder == -1)
                    IsSettingKeybind = false;
                else
                {
                    var e = Event.current;
                    if (e.isKey)
                    {
                        SetKey= e.keyCode;
                        IsSettingKeybind = false;
                    }
                }
            }

            PresetsPopup.Button(GUILayout.MaxWidth(260));
            PresetsPopup.Show(0f, 0f);

            bool allowGUI = !PresetsPopup.isVisible;
            GUI.enabled = allowGUI;

            var p = PresetsPopup.SelectedValue;
            if (p != null)
            {
                GUILayout.BeginVertical(p.Name);//, GUI.skin.window);
                {
                    GUILayout.Label(p.Description);
                    if (!HasPreset)
                    {
                        HasPreset = true;
                        p.ResetValues(lastBlockMover);
                    }
                    scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
                    {
                        GUILayout.BeginVertical();
                        {
                            for (int i = 0; i < p.Variables.Count; i++)
                            {
                                GUILayout.Space(8);
                                p.Variables[i].DrawGUI(ref i);
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndScrollView();

                    if (GUILayout.Button("Apply to Tech"))
                    {
                        Log = p.SetToBlockMover(lastBlockMover);
                        Changed();
                    }
                }
                GUILayout.EndVertical();
            }
            else if (HasPreset)
            {
                HasPreset = false;
                scroll = Vector2.zero;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Close"))
            {
                PresetsPopup.SelectedValue = null;
                OptionMenuMover.showPresetsUI = false;
            }

            GUI.enabled = true;

            PresetsPopup.List(0f, 0f);

            GUI.DragWindow();
        }
    }
}
