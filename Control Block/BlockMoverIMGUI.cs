using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Control_Block
{
    class GUIOverseer : MonoBehaviour
    {
        public static bool TextSliderPair(string label, ref string input, ref float value, float min, float max, bool clampText, float round = 0.02f)
        {
            GUILayout.Label(label + value.ToString());

            GUILayout.BeginHorizontal();
            GUI.changed = false;
            bool Changed = false;
            input = GUILayout.TextField(input, GUILayout.MaxWidth(80));
            if (GUI.changed && float.TryParse(input, out float sValue))
            {
                if (clampText)
                    value = Mathf.Clamp(sValue, min, max);
                else
                    value = sValue;
                Changed = true;
            }

            GUI.changed = false;
            var tValue = Mathf.Round(GUILayout.HorizontalSlider(value, min, max) / round) * round;
            if (GUI.changed)
            {
                input = tValue.ToString();
                value = tValue;
                Changed = true;
            }
            GUILayout.EndHorizontal();
            return Changed;
        }

        public GUIOverseer()
        {
            inst = this;
        }
        public static GUIOverseer inst;
        public static void CheckValid()
        {
            inst.gameObject.SetActive(OptionMenuMover.inst.check_OnGUI() || /*OptionMenuSwivel.inst.check_OnGUI() || */OptionMenuSteeringRegulator.inst.check_OnGUI() || LogGUI.inst.check_OnGUI());
        }
        void OnGUI()
        {
            OptionMenuMover.inst.stack_OnGUI();
            //OptionMenuSwivel.inst.stack_OnGUI();
            OptionMenuSteeringRegulator.inst.stack_OnGUI();
            LogGUI.inst.stack_OnGUI();
        }
    }

    internal class LogGUI : MonoBehaviour
    {
        public LogGUI()
        {
            inst = this;
        }

        private readonly int ID = 45925;

        public static LogGUI inst;

        private bool visible = false;

        private TankBlock module;

        private string Log = "";

        private Rect win;

        private void Update()
        {
            if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetKeyDown(KeyCode.Backslash))
            {
                win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 100f, 600f, 300f);
                try
                {
                    module = Singleton.Manager<ManPointer>.inst.targetVisible.block;
                    Log = Class1.LogAllComponents(module.transform);
                }
                catch
                {
                    //Console.WriteLine(e);
                    module = null;
                    Log = "";
                }
                visible = module;
            }
            if (module != null && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Semicolon))
            {
                Console.WriteLine(Class1.LogAllComponents(module.transform, true));
            }

            GUIOverseer.CheckValid();
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
                win = GUI.Window(ID, win, new GUI.WindowFunction(DoWindow), "Dump");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private Vector2 scroll = Vector2.zero;

        private void DoWindow(int id)
        {
            if (module == null)
            {
                visible = false;
                return;
            }
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label(Log);
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }
    }

    internal class OptionMenuMover : MonoBehaviour
    {
        public OptionMenuMover()
        {
            UIInputPopup = new Popup(Enum.GetNames(typeof(ModuleBlockMover.InputOperator.InputType)));
            UIOperatorPopup = new Popup(Enum.GetNames(typeof(ModuleBlockMover.InputOperator.OperationType)));
            inst = this;
        }
        GUIStyle pvOn, pvOff;

        public static OptionMenuMover inst;

        private readonly int ID = 7787;

        private bool visible = false;

        private ModuleBlockMover module;

        private Rect win;

        public bool queueResetTextCache;

        private void Update()
        {
            if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetMouseButtonDown(1))
            {
                win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 200f, 700f, 400f);
                try
                {
                    module = Singleton.Manager<ManPointer>.inst.targetVisible.block.GetComponent<ModuleBlockMover>();
                }
                catch
                {
                    //Console.WriteLine(e);
                    module = null;
                }
                visible = module;
                if (visible)
                {
                    SelectedIndex = -1;
                    ResetTextCache();
                    Log = "";
                    Texts = null;
                }
                IsSettingKeybind = false;
            }
            if (queueResetTextCache) ResetTextCache();
        }

        private void ResetTextCache()
        {
            queueResetTextCache = false;
            maxCache = module.MAXVALUELIMIT.ToString();
            minCache = module.MINVALUELIMIT.ToString();
            springCache = module.SPRSTR.ToString();
            dampCache = module.SPRDAM.ToString();
            valueCache = module.VALUE.ToString();
            velocityCache = module.VELOCITY.ToString();
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
        private int SelectedIndex;

        private Popup UIInputPopup, UIOperatorPopup;
        private Vector2[] Scrolls = new Vector2[2];
        private string[] Texts;
        private string paramCache, strengthCache, valueCache, velocityCache, minCache, maxCache, springCache, dampCache;
        private string Log = "";
        private bool showOldUI;

        private void DoWindow(int id)
        {
            try
            {
                if (module == null || module.block.tank == null)
                {
                    module = null;
                    visible = false;
                    return;
                }
                if (SelectedIndex >= module.ProcessOperations.Count) SelectedIndex = -1;
                if (IsSettingKeybind)
                {
                    if (SelectedIndex == -1)
                        IsSettingKeybind = false;
                    else
                    {
                        var e = Event.current;
                        if (e.isKey)
                        {
                            var io = module.ProcessOperations[SelectedIndex];
                            io.m_InputKey = e.keyCode;
                            Texts = null;
                            IsSettingKeybind = false;
                        }
                    }
                }


                bool UpdateSelection;
                GUILayout.BeginHorizontal();
                { // Splitter
                    GUILayout.BeginVertical(GUILayout.MaxWidth(250));
                    {
                        Scrolls[0] = GUILayout.BeginScrollView(Scrolls[0]);
                        {
                            if (Texts == null)
                            {
                                Texts = module.ProcessOperationsToStringArray();
                            }

                            if (pvOn == null)
                            {
                                pvOn = new GUIStyle(GUI.skin.label);
                                pvOn.alignment = TextAnchor.MiddleLeft;

                                pvOn.onActive.textColor = Color.white;
                                pvOn.active.textColor = Color.white;

                                var normalColor = new Color(0.85f, 0.85f, 0.85f);

                                pvOn.onFocused.textColor = normalColor;
                                pvOn.focused.textColor = normalColor;

                                pvOn.onNormal.textColor = normalColor;
                                pvOn.normal.textColor = normalColor;

                                pvOn.stretchHeight = false;
                                pvOn.fontStyle = FontStyle.Bold;
                                pvOn.fontSize += 2;
                                pvOn.padding = new RectOffset(8, 8, 2, 2);
                                pvOn.clipping = TextClipping.Overflow;
                                pvOn.wordWrap = false;
                                pvOff = new GUIStyle(pvOn);
                                pvOff.fontStyle = FontStyle.Normal;
                            }

                            GUI.changed = false;

                            GUILayout.Space(16);

                            for (int index = 0; index < Texts.Length; index++)
                            {
                                bool state = module.ProcessOperations[index].LASTSTATE;
                                if (GUILayout.Toggle(state, Texts[index], index == SelectedIndex ? pvOn : pvOff) != state)
                                {
                                    SelectedIndex = index;
                                }
                            }

                            UpdateSelection = GUI.changed;

                            GUILayout.FlexibleSpace(); // Inflate scrollview hopefully
                        }
                        GUILayout.EndScrollView();

                        GUILayout.BeginVertical("Edit", GUI.skin.window);
                        {
                            //GUILayout.Space(8);
                            GUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Button("Add"))
                                {
                                    module.ProcessOperations.Insert(++SelectedIndex, new ModuleBlockMover.InputOperator());
                                    Texts = null;
                                    UpdateSelection = true;
                                }
                                if (SelectedIndex > -1)
                                {
                                    if (GUILayout.Button("▲") && SelectedIndex > 0)
                                    {
                                        var current = module.ProcessOperations[SelectedIndex];
                                        var swap = module.ProcessOperations[SelectedIndex - 1];
                                        module.ProcessOperations[SelectedIndex] = swap;
                                        module.ProcessOperations[--SelectedIndex] = current;
                                        Texts = null;
                                        //UpdateSelection = true;
                                    }
                                    if (GUILayout.Button("▼") && SelectedIndex < module.ProcessOperations.Count - 1)
                                    {
                                        var current = module.ProcessOperations[SelectedIndex];
                                        var swap = module.ProcessOperations[SelectedIndex + 1];
                                        module.ProcessOperations[SelectedIndex] = swap;
                                        module.ProcessOperations[++SelectedIndex] = current;
                                        Texts = null;
                                        //UpdateSelection = true;
                                    }
                                    if (GUILayout.Button("Delete"))
                                    {
                                        module.ProcessOperations.RemoveAt(SelectedIndex--);
                                        Texts = null;
                                        UpdateSelection = true;
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Button("Copy text"))
                                {
                                    GUIUtility.systemCopyBuffer = string.Join("\n", Texts);
                                    Log = "Copied";
                                }
                                if (GUILayout.Button("Paste text"))
                                {
                                    Log = module.StringArrayToProcessOperations(GUIUtility.systemCopyBuffer);
                                    Texts = null;
                                }
                                // Copy as Text, Paste from Text
                            }
                            GUILayout.EndHorizontal();
                            if (!string.IsNullOrEmpty(Log))
                                GUILayout.Label(Log);
                        }
                        GUILayout.EndVertical();
                        bool _showOldUI = GUILayout.Toggle(showOldUI, "Function Presets (Old UI)");
                        if (_showOldUI != showOldUI)
                        {
                            LegacyBlockMoverIMGUI.SetState(_showOldUI, module, win);
                            showOldUI = _showOldUI;
                        }
                        if (showOldUI)
                        {
#warning                    REINCOROPRATE SIMPLE UI so cloud doesn't hate me
                            if (LegacyBlockMoverIMGUI.DoWindow(module, win))
                            {
                                Log = LegacyBlockMoverIMGUI.Log;
                            }
                        }
                    }
                    GUILayout.EndVertical(); // Processes for <block>

                    GUILayout.BeginVertical(); // Parameters for processes
                    {
                        Scrolls[1] = GUILayout.BeginScrollView(Scrolls[1]);
                        {
                            Rect rect1 = default, rect2 = default;
                            bool PairEdit = SelectedIndex != -1 && SelectedIndex < module.ProcessOperations.Count;

                            if (PairEdit)
                            {
                                var io = module.ProcessOperations[SelectedIndex];
                                if (UpdateSelection)
                                {
                                    paramCache = io.m_InputParam.ToString();
                                    strengthCache = io.m_Strength.ToString();
                                    UIInputPopup.selectedItemIndex = (int)io.m_InputType;
                                    UIOperatorPopup.selectedItemIndex = (int)io.m_OperationType;
                                }
                                var uii = ModuleBlockMover.InputOperator.UIInputPairs[io.m_InputType];
                                var uio = ModuleBlockMover.InputOperator.UIOperationPairs[io.m_OperationType];

                                if (!uio.LockInputTypes)
                                {
                                    GUILayout.BeginVertical("Condition " + uii.UIName, GUI.skin.window);
                                    {
                                        //GUILayout.Space(8);
                                        rect1 = UIInputPopup.Show();
                                        GUILayout.Label(uii.UIName);
                                        GUILayout.BeginHorizontal();
                                        {
                                            if (uii.HideInputKey)
                                            {
                                                IsSettingKeybind = false;
                                            }
                                            else
                                            {
                                                GUILayout.BeginVertical();
                                                {
                                                    GUILayout.Label("Input Key");
                                                    IsSettingKeybind = GUILayout.Button(IsSettingKeybind ? "Press a key" : io.m_InputKey.ToString()) != IsSettingKeybind;
                                                }
                                                GUILayout.EndVertical();
                                            }
                                            if (!uii.HideParam)
                                            {
                                                GUILayout.BeginVertical();
                                                {
                                                    GUILayout.Label("Input Parameter");
                                                    if (!uii.ParamIsToggle)
                                                    {
                                                        GUI.changed = false;
                                                        paramCache = GUILayout.TextField(paramCache);
                                                        if (GUI.changed && float.TryParse(paramCache, out float value))
                                                        {
                                                            io.m_InputParam = value;
                                                            Texts = null;
                                                        }
                                                    }
                                                    if (!uii.ParamIsTrueValue)
                                                    {
                                                        GUI.changed = false;
                                                        var temp = io.m_InputParam;
                                                        if (uii.ParamIsToggle)
                                                        {
                                                            temp = (GUILayout.Toggle(temp * uii.ToggleMultiplier < 0, uii.ToggleComment) ? -1f : 1f) * uii.ToggleMultiplier;
                                                        }
                                                        else
                                                        {
                                                            if (uii.SliderMaxIsMaxVal)
                                                            {
                                                                temp = GUILayout.HorizontalSlider(Mathf.Abs(temp), 0, module.TrueLimitVALUE) * Mathf.Sign(temp);
                                                            }
                                                            else if (uii.SliderMaxIsMaxVel)
                                                            {
                                                                temp = GUILayout.HorizontalSlider(Mathf.Abs(temp), -module.MaxVELOCITY, module.MaxVELOCITY) * Mathf.Sign(temp);
                                                            }
                                                            else
                                                            {
                                                                if (uii.SliderMax != 0)
                                                                {
                                                                    temp = GUILayout.HorizontalSlider(Mathf.Abs(temp), 0, uii.SliderMax) * Mathf.Sign(temp);
                                                                }
                                                                temp = GUILayout.Toggle(temp < 0, "Invert") ? -Mathf.Abs(temp) : Mathf.Abs(temp);
                                                            }
                                                        }
                                                        if (GUI.changed)
                                                        {
                                                            paramCache = temp.ToString();
                                                            io.m_InputParam = temp;
                                                            Texts = null;
                                                        }
                                                    }
                                                }
                                                GUILayout.EndVertical();
                                            }
                                        }
                                        GUILayout.EndHorizontal();
                                        GUILayout.Space(8);
                                    }
                                    GUILayout.EndVertical();
                                }

                                GUILayout.BeginVertical("Operator " + uio.UIName, GUI.skin.window);
                                {
                                    //GUILayout.Space(8);
                                    rect2 = UIOperatorPopup.Show();
                                    GUILayout.Label(uio.UIDesc);
                                    if (!uio.HideStrength)
                                    {
                                        GUILayout.Label("Strength Parameter");
                                        if (!uio.StrengthIsToggle)
                                        {
                                            GUI.changed = false;
                                            strengthCache = GUILayout.TextField(strengthCache);
                                            if (GUI.changed && float.TryParse(strengthCache, out float value))
                                            {
                                                io.m_Strength = value;
                                                Texts = null;
                                            }
                                        }
                                        GUI.changed = false;
                                        var temp = io.m_Strength;
                                        if (uio.StrengthIsToggle)
                                        {
                                            temp = (GUILayout.Toggle(temp * uio.ToggleMultiplier < 0, uio.ToggleComment) ? -1f : 1f) * uio.ToggleMultiplier;
                                        }
                                        else if (uio.ClampStrength)
                                        {
                                            temp = GUILayout.HorizontalSlider(temp, -1f, 1f);
                                        }
                                        else if (uio.SliderMaxIsMaxVel)
                                        {
                                            float vel = module.MaxVELOCITY;
                                            temp = GUILayout.HorizontalSlider(temp, uio.SliderMin ? -vel : 0f, vel);
                                        }
                                        else if (uio.SliderPosFraction != 0f)
                                        {
                                            float frac = module.MAXVALUELIMIT * uio.SliderPosFraction;
                                            temp = GUILayout.HorizontalSlider(temp, uio.SliderMin ? -frac : 0f, frac);
                                        }
                                        else if (uio.SliderMax != 0f)
                                        {
                                            temp = GUILayout.HorizontalSlider(temp, uio.SliderMin ? uio.SliderMax : 0f, uio.SliderMax);
                                        }
                                        if (GUI.changed)
                                        {
                                            strengthCache = temp.ToString();
                                            io.m_Strength = temp;
                                            Texts = null;
                                        }
                                    }
                                    GUILayout.Space(8);
                                }
                                GUILayout.EndVertical();
                                GUILayout.Space(16);
                            }

                            GUILayout.Space(16);

                            GUILayout.BeginVertical("Values", GUI.skin.window);
                            {
                                //GUILayout.Space(8);
                                GUILayout.Label($"Current Value: {module.PVALUE}");
                                //GUILayout.Label($"Target Value: {module.VALUE}");

                                GUIOverseer.TextSliderPair("Target Value: ", ref valueCache, ref module.VALUE, module.MINVALUELIMIT, module.MAXVALUELIMIT, true, 0.25f);
                                GUIOverseer.TextSliderPair("Current Velocity: ", ref velocityCache, ref module.VELOCITY, -module.MaxVELOCITY, module.MaxVELOCITY, true);

                                GUILayout.Space(4);

                                if (GUIOverseer.TextSliderPair("Min Value Limit: ", ref minCache, ref module.MINVALUELIMIT, 0, module.TrueLimitVALUE, true, 0.25f))
                                {
                                    module.SetMinValueLimit(module.MINVALUELIMIT);
                                    queueResetTextCache = true;
                                }
                                if (GUIOverseer.TextSliderPair("Max Value Limit: ", ref maxCache, ref module.MAXVALUELIMIT, module.MINVALUELIMIT, module.TrueLimitVALUE, true, 0.25f))
                                {
                                    module.SetMaxValueLimit(module.MAXVALUELIMIT);
                                    queueResetTextCache = true;
                                }

                                GUILayout.Space(4);

                                GUILayout.Label("Free-joint properties");
                                if (GUIOverseer.TextSliderPair("Spring Strength: ", ref springCache, ref module.SPRSTR, 0, 1000, false, 0.25f))
                                    module.UpdateSpringForce();
                                if (GUIOverseer.TextSliderPair("Spring Dampen: ", ref dampCache, ref module.SPRDAM, 0, 100, false, 0.25f))
                                    module.UpdateSpringForce();
                                module.DEACTIVATEMOTOR = GUILayout.Toggle(module.DEACTIVATEMOTOR, "Deactivate Motor");

                                GUILayout.Space(4);
                            }
                            GUILayout.EndVertical();
                            GUILayout.Space(16);

                            if (module.Holder != null)
                            {
                                GUILayout.Label($"Rigidbody mass: {module.Holder.rbody.mass}");
                                GUILayout.Label($"Rigidbody CoM: {module.Holder.rbody.centerOfMass}");
                                GUILayout.Label($"Blocks on body: {module.Holder.blocks.Count}");
                            }
                            GUILayout.Label($"Tank mass: {module.block.tank.rbody.mass}");
                            GUILayout.Label($"Tank CoM: {module.block.tank.rbody.centerOfMass}");

                            GUILayout.Space(16);

                            if (GUILayout.Button("Close"))
                            {
                                visible = false;
                                IsSettingKeybind = false;
                                module = null;
                            }
                            if (PairEdit)
                            {
                                var io = module.ProcessOperations[SelectedIndex];

                                var InputType = (ModuleBlockMover.InputOperator.InputType)UIInputPopup.List(rect1);
                                if (InputType != io.m_InputType)
                                {
                                    io.m_InputType = InputType;
                                    Texts = null;
                                }
                                var OperationType = (ModuleBlockMover.InputOperator.OperationType)UIOperatorPopup.List(rect2);
                                if (OperationType != io.m_OperationType)
                                {
                                    io.m_OperationType = OperationType;
                                    if (OperationType == ModuleBlockMover.InputOperator.OperationType.ConditionEndIf)
                                        io.m_InputType = ModuleBlockMover.InputOperator.InputType.AlwaysOn;
                                    Texts = null;
                                }
                            }
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();

                GUI.DragWindow();
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
                visible = false;
                module = null;
            }
        }
    }

    internal class OptionMenuSteeringRegulator : MonoBehaviour
    {
        public OptionMenuSteeringRegulator()
        {
            inst = this;
        }
        public static OptionMenuSteeringRegulator inst;

        private readonly int ID = 7788;

        private bool visible = false;

        private ModuleSteeringRegulator module;

        private Rect win;

        private void Update()
        {
            if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetMouseButtonDown(1))
            {
                win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 100f, 200f, 200f);
                try
                {
                    module = Singleton.Manager<ManPointer>.inst.targetVisible.block.GetComponent<ModuleSteeringRegulator>();
                }
                catch
                {
                    //Console.WriteLine(e);
                    module = null;
                }
                visible = module;
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
                win = GUI.Window(ID, win, new GUI.WindowFunction(DoWindow), "Stabiliser PiD");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void DoWindow(int id)
        {
            if (module == null)
            {
                visible = false;
                return;
            }
            GUILayout.Label("Sensitivity RADIUS : " + module.MaxDist.ToString());
            module.MaxDist = Mathf.Round(GUILayout.HorizontalSlider(module.MaxDist * 4, 0f, 20f)) * .25f;
            GUILayout.Label("Hover PiD Effect");
            module.HoverMod = Mathf.Round(GUILayout.HorizontalSlider(module.HoverMod * 2f, 0f, 15f)) * .5f;
            GUILayout.Label("Steering Jet PiD Effect");
            module.JetMod = Mathf.Round(GUILayout.HorizontalSlider(module.JetMod, 0f, 15f));
            GUILayout.Label("Turbine PiD Effect");
            module.TurbineMod = Mathf.Round(GUILayout.HorizontalSlider(module.TurbineMod * 2f, 0f, 15f)) * .5f;
            if (GUILayout.Button("Close"))
            {
                visible = false;
                module = null;
            }
            GUI.DragWindow();
        }
    }
}
