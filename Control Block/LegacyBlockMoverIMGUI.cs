using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Control_Block
{
    static class LegacyBlockMoverIMGUI
    {
        public static string Log;
        static ModuleBlockMover lastBlockMover;
        static Rect window;

        public static bool DoWindow(ModuleBlockMover blockMover, Rect referencePos)
        {
            bool Changed = false;
            if (blockMover.IsPlanarVALUE)
            {
                //Swivel prefab list
            }
            else
            {
                //Piston prefab list
            }
            return Changed;
        }

        internal static void SetState(bool showUI, ModuleBlockMover blockMover, Rect refWindow)
        {
            if (showUI)
            {
                lastBlockMover = blockMover;
                float width = 400, height = 600;
                window = new Rect(refWindow.x - width, refWindow.y + refWindow.height - height, width, height);
            }
            else
            {
                lastBlockMover = null;
            }
        }

        /*
        internal static class OptionMenuPiston
        {
            private static Rect win;
            private static readonly string[] toggleOptions = new string[] { "Normal", "DelayedInput", "PreferOpen", "PreferClosed" };
            private static readonly string[] notToggleOptions = new string[] { "Normal", "InvertInput" };

            private static bool IsSettingKeybind;

            private static Vector2 Scroll = Vector2.zero;

            private class Modu
            {
                public float StretchModifier;
                public KeyCode trigger;
            }
            private static Modu module;

            private static void DoWindow(int id)
            {
                Scroll = GUILayout.BeginScrollView(Scroll);
                if (IsSettingKeybind)
                {
                    var e = Event.current;
                    if (e.isKey)
                    {
                        module.trigger = e.keyCode;
                        IsSettingKeybind = false;
                    }
                }

                //GUILayout.Label($"Noise Multiplier : {module.SFXVolume}, Current Noise : {module.LastCurveDiff * module.SFXVolume}");
                //module.SFXVolume = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.SFXVolume * 20, 1, 2000)) * .05f;

                GUILayout.Label("Keybind input");
                IsSettingKeybind = GUILayout.Button(IsSettingKeybind ? "Press a key for use" : module.trigger.ToString()) != IsSettingKeybind;

                if (lastBlockMover.TrueLimitVALUE > 1f)
                {
                    GUILayout.Label("Maximum Stretch : " + module.StretchModifier.ToString());
                    int temp = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.StretchModifier, 1, module.MaxStr));
                    if (module.open == 0)
                    {
                        module.StretchModifier = temp;
                    }
                }

                module.IsToggle = GUILayout.Toggle(module.IsToggle, "Is toggle");

                module.InverseTrigger = (byte)GUILayout.SelectionGrid(module.InverseTrigger, (module.IsToggle ? toggleOptions : notToggleOptions), 2);

                module.LocalControl = GUILayout.Toggle(module.LocalControl, "Local to tech");

                GUILayout.Label("Piston : " + module.block.cachedLocalPosition.ToString());

                if (module.CurrentCellPush > module.MaximumBlockPush)
                {
                    GUILayout.Label(" The piston is overburdened! (>" + module.MaximumBlockPush.ToString() + ")");
                }
                else if (module.CurrentCellPush == -1)
                {
                    GUILayout.Label(" The piston is structurally locked!");
                }
                else if (module.CurrentCellPush == -2)
                {
                    GUILayout.Label(" This piston cannot move any cabs!");
                }
                else
                {
                    GUILayout.Label(" Burden : " + module.CurrentCellPush.ToString());
                }

                if (GUILayout.Button("Apply"))
                {

                }
                if (GUILayout.Button("Close"))
                {
                    IsSettingKeybind = false;
                    module = null;
                }
                GUI.DragWindow();
                GUILayout.EndScrollView();
            }

            private static void SetPreset(ModuleBlockMover blockMover)
            {

            }
        }

        internal class OptionMenuSwivel : MonoBehaviour
        {
            public OptionMenuSwivel()
            {
                inst = this;
            }
            public static OptionMenuSwivel inst;
            private readonly int ID = 7782;

            private bool visible = false;

            private ModuleSwivel module;

            private Rect win;
            private readonly string[] modeOptions = new string[] { "Positional", "Directional", "Speed", "On/Off", "Target Aim", "Steering", "Player Aim", "Velocity Aim", "Cycle", "Throttle" };

            private void Update()
            {
                if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetMouseButtonDown(1))
                {
                    win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 175f, 450f, 600f);
                    try
                    {
                        module = Singleton.Manager<ManPointer>.inst.targetVisible.block.GetComponent<ModuleSwivel>();
                    }
                    catch
                    {
                        //Console.WriteLine(e);
                        module = null;
                    }
                    visible = module;
                    IsSettingKeybind = false;
                }
            }

            public bool check_OnGUI()
            {
                return visible && module;
            }

            public void stack_OnGUI()
            {
                if (!visible || !module)
                {
                    return;
                }

                try
                {
                    win = GUI.Window(ID, win, new GUI.WindowFunction(DoWindow), module.gameObject.name);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            private bool IsSettingKeybind;

            private int SetButton = -1;

            private Vector2 Scroll = Vector2.zero;

            private void DoWindow(int id)
            {
                Scroll = GUILayout.BeginScrollView(Scroll);
                if (module == null)
                {
                    visible = false;
                    return;
                }
                if (IsSettingKeybind)
                {
                    var e = Event.current;
                    if (e.isKey)
                    {
                        switch (SetButton)
                        {
                            case 0: module.trigger1 = e.keyCode; break;
                            case 1: module.trigger2 = e.keyCode; break;
                        }
                        IsSettingKeybind = false;
                        SetButton = -1;
                    }
                }

                //GUILayout.Label($"Noise Multiplier : {module.SFXVolume}, Current Noise : {module.LastCurveDiff * module.SFXVolume}");
                //module.SFXVolume = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.SFXVolume * 20, 1, 2000)) * .05f;

                GUILayout.Label("Clockwise Key");
                IsSettingKeybind = GUILayout.Button(IsSettingKeybind && SetButton == 0 ? "Press a key for use" : module.trigger1.ToString()) != IsSettingKeybind;
                if (IsSettingKeybind && SetButton == -1)
                {
                    SetButton = 0;
                }

                GUILayout.Label("CounterClockwise Key");
                IsSettingKeybind = GUILayout.Button(IsSettingKeybind && SetButton == 1 ? "Press a key for use" : module.trigger2.ToString()) != IsSettingKeybind;
                if (IsSettingKeybind && SetButton == -1)
                {
                    SetButton = 1;
                }

                if (!IsSettingKeybind && SetButton != -1)
                {
                    SetButton = -1;
                }

                module.LockAngle = GUILayout.Toggle(module.LockAngle, "Restrict Angle");
                float Angle = (Mathf.Repeat(module.AngleCenter + 180, 360) - 180);
                GUILayout.Label("Center of Limit: " + Angle.ToString());
                module.AngleCenter = Mathf.RoundToInt((GUILayout.HorizontalSlider(Angle, -180, 179) + 360) % 360);
                GUILayout.Label("Range of Limit: " + module.AngleRange.ToString());
                module.AngleRange = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.AngleRange, 0, 179 - module.RotateSpeed) % 360);

                module.mode = (ModuleSwivel.Mode)GUILayout.SelectionGrid((int)module.mode, modeOptions, 2);

                if (module.CanModifySpeed)
                {
                    GUILayout.Label("Rotation Speed : " + module.RotateSpeed.ToString());
                    module.RotateSpeed = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.RotateSpeed * 2, 1, module.MaxSpeed * 2)) * .5f;
                }
                Angle = (Mathf.Repeat(module.CurrentAngle + 180, 360) - 180);

                GUILayout.Label("Angle : " + ((int)Angle).ToString());
                var newAngle = GUILayout.HorizontalSlider(Angle, -180, 179);
                if (newAngle != Angle)
                {
                    module.CurrentAngle = Mathf.Clamp(newAngle, Angle - module.MaxSpeed, Angle + module.MaxSpeed) - 360;
                }

                module.LocalControl = GUILayout.Toggle(module.LocalControl, "Local to tech");

                if (module.mode != ModuleSwivel.Mode.AimAtPlayer && module.mode != ModuleSwivel.Mode.AimAtVelocity)
                {
                    if (module.mode != ModuleSwivel.Mode.Directional)
                    {
                        GUILayout.Label("Start Pause: " + module.StartDelay.ToString());
                        module.StartDelay = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.StartDelay, 0, 360));
                    }
                    if (module.mode == ModuleSwivel.Mode.Cycle || module.mode == ModuleSwivel.Mode.Directional || module.mode == ModuleSwivel.Mode.OnOff)
                    {
                        GUILayout.Label("CW Limiter Pause: " + module.CWDelay.ToString());
                        module.CWDelay = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.CWDelay, 0, 360));
                        GUILayout.Label("CCW Limiter Pause: " + module.CCWDelay.ToString());
                        module.CCWDelay = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.CCWDelay, 0, 360));
                    }
                }

                GUILayout.Label("Swivel : " + module.block.cachedLocalPosition.ToString());

                if (module.CurrentCellPush > module.MaximumBlockPush)
                {
                    GUILayout.Label(" The swivel is overburdened! (>" + module.MaximumBlockPush.ToString() + ")");
                }
                else if (module.CurrentCellPush == -1)
                {
                    GUILayout.Label(" The swivel is structurally locked!");
                }
                else if (module.CurrentCellPush == -2)
                {
                    GUILayout.Label(" This swivel cannot move any cabs!");
                }
                else
                {
                    GUILayout.Label(" Burden : " + module.CurrentCellPush.ToString());
                }

                if (GUILayout.Button("Close"))
                {
                    visible = false;
                    IsSettingKeybind = false;
                    module = null;
                }
                GUI.DragWindow();
                GUILayout.EndScrollView();
            }
        }
        */
    }
}
