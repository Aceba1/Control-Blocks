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
            if (input == null) input = value.ToString();
            input = GUILayout.TextField(input, GUILayout.MaxWidth(80));
            if (GUI.changed && float.TryParse(input, out float sValue))
            {
                if (clampText)
                    sValue = Mathf.Clamp(sValue, min, max);
                Changed = sValue != value;
                value = sValue;
            }

            GUI.changed = false;
            var tValue = Mathf.Round(GUILayout.HorizontalSlider(value, min, max) / round) * round;
            if (GUI.changed)
            {
                input = tValue.ToString();
                Changed |= tValue != value;
                value = tValue;
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
        public void OnGUI()
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

        public void Update()
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
            UIInputPopup = PopupBase.CreateFromInputCategories();
            UIOperatorPopup = PopupBase.CreateFromOperatorCategories();
            inst = this;
        }
        GUIStyle pvOn, pvOff, pvSelOn, pvSelOff, noWrap;

        public static OptionMenuMover inst;

        private readonly int ID = 7787;

        private bool visible = false;

        private ModuleBlockMover module;

        //private ModuleBMSegment segment;

        private Rect win;

        public bool queueResetTextCache;

        public void Update()
        {
            if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetMouseButtonDown(1))
            {
                win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 200f, 700f, 400f);
                try
                {
                    var block = Singleton.Manager<ManPointer>.inst.targetVisible.block;
                    if (block != null)
                    {
                        module = block.GetComponent<ModuleBlockMover>();
                        if (module == null)
                        {
                            var segment = block.GetComponent<ModuleBMSegment>();
                            if (segment != null) module = segment.VerifyBlockMover;
                        }
                    }
                }
                catch
                {
                    //Console.WriteLine(e);
                    module = null;
                    //segment = null;
                    PresetsIMGUI.SetState(false, null, win);
                    showPresetsUI = false;
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

        internal void ResetTextCache()
        {
            queueResetTextCache = false;
            maxCache = null; //module.MAXVALUELIMIT.ToString();
            minCache = null; //module.MINVALUELIMIT.ToString();
            maxVelCache = module.MAXVELOCITY.ToString();
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

            if (module.IsControlledByNet)
            {
                visible = false;
                return;
            }

            try
            {
                win = GUI.Window(ID, win, new GUI.WindowFunction(DoWindow), StringLookup.GetItemName(module.block.visible.m_ItemType));
                if (showPresetsUI) 
                    PresetsIMGUI.DoWindow();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private bool IsSettingKeybind;
        internal static int SelectedIndex;

        private Popup<InputOperator.InputType> UIInputPopup;
        private Popup<InputOperator.OperationType> UIOperatorPopup;
        private Vector2[] Scrolls = new Vector2[2];
        internal string[] Texts;
        private string paramCache, strengthCache, valueCache, velocityCache, minCache, maxVelCache, maxCache, springCache, dampCache;
        internal string Log = "";
        internal static bool showPresetsUI;

        void CreateGUIStyles()
        {
            var normalColor = new Color(0.8f, 0.8f, 0.8f);
            var hoverColor = Color.white;
            var activeColor = new Color(0.6f, 0.8f, 1f);

            pvOff = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft
            };
            pvOff.active.textColor = activeColor;
            pvOff.hover.textColor = hoverColor;
            pvOff.normal.textColor = normalColor;

            //pvOn.stretchHeight = false;
            //pvOn.fontSize += 2;
            pvOff.margin = new RectOffset(3, 3, 3, 3);
            //pvOn.clipping = TextClipping.Overflow;
            pvOff.wordWrap = false;

            pvOn = new GUIStyle(pvOff)
            {
                fontStyle = FontStyle.Bold
            };

            pvSelOff = new GUIStyle(pvOff);
            pvSelOff.active.textColor = activeColor;
            pvSelOff.hover.textColor = activeColor;
            pvSelOff.normal.textColor = activeColor;

            pvSelOn = new GUIStyle(pvSelOff)
            {
                fontStyle = FontStyle.Bold
            };

            noWrap = new GUIStyle(GUI.skin.label);
            noWrap.wordWrap = false;
            noWrap.fontStyle = FontStyle.Bold;
        }

        bool UpdateSelection;

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

                if (pvOn == null)
                {
                    CreateGUIStyles();
                }

                GUILayout.BeginHorizontal();
                { // Splitter

                    bool allowGUI = !UIInputPopup.isVisible && !UIOperatorPopup.isVisible;
                    GUI.enabled = allowGUI;

                    GUILayout.BeginVertical(GUILayout.MaxWidth(380)); // Parameters for processes
                    {
                        Scrolls[1] = GUILayout.BeginScrollView(Scrolls[1]);
                        {
                            bool PairEdit = IsItemSelected;

                            if (PairEdit)
                            {
                                var io = module.ProcessOperations[SelectedIndex];
                                if (UpdateSelection)
                                {
                                    paramCache = io.m_InputParam.ToString();
                                    strengthCache = io.m_Strength.ToString();
                                    UIInputPopup.SelectedValue = io.m_InputType;
                                    UIInputPopup.SelectedName = io.m_InputType.ToString();
                                    UIOperatorPopup.SelectedValue = io.m_OperationType;
                                    UIOperatorPopup.SelectedName = io.m_OperationType.ToString();
                                    UpdateSelection = false;
                                }
                                var uii = InputOperator.UIInputPairs[io.m_InputType];
                                var uio = InputOperator.UIOperationPairs[io.m_OperationType];

                                if (!uio.LockInputTypes)
                                {
                                    GUILayout.BeginVertical("Condition " + uii.UIName, GUI.skin.window);
                                    {
                                        //GUILayout.Space(8);
                                        UIInputPopup.Button();
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
                                                                temp = GUILayout.HorizontalSlider(Mathf.Abs(temp), -module.TrueMaxVELOCITY, module.TrueMaxVELOCITY) * Mathf.Sign(temp);
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
                                    UIOperatorPopup.Button();
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
                                            float vel = module.TrueMaxVELOCITY;
                                            temp = GUILayout.HorizontalSlider(temp, uio.SliderHasNegative ? -vel : 0f, vel);
                                        }
                                        else if (uio.SliderPosFraction != 0f)
                                        {
                                            float frac = module.MAXVALUELIMIT * uio.SliderPosFraction;
                                            temp = GUILayout.HorizontalSlider(temp, (uio.SliderHasNegative || uio.SliderMinOnPlanar && module.IsPlanarVALUE) ? -frac : 0f, frac);
                                        }
                                        else if (uio.SliderMax != 0f)
                                        {
                                            if (uio.SliderHasNegative)
                                                temp = GUILayout.HorizontalSlider(temp, -uio.SliderMax, uio.SliderMax);
                                            else
                                                temp = GUILayout.HorizontalSlider(Mathf.Abs(temp), 0f, uio.SliderMax) * Mathf.Sign(temp);
                                            if (!string.IsNullOrEmpty(uio.ToggleComment))
                                            {
                                                temp = (GUILayout.Toggle(temp < 0, uio.ToggleComment) ? -1f : 1f) * Mathf.Abs(temp);
                                            }

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

                            GUILayout.Label("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -", noWrap);

                            GUILayout.Space(16);

                            GUILayout.BeginVertical("Properties", GUI.skin.window);
                            {
                                module.LOCALINPUT = GUILayout.Toggle(module.LOCALINPUT, "Input local to tech");

                                GUILayout.Space(12);
                                
                                GUILayout.Label($"Current Value: {module.PVALUE}");
                                //GUILayout.Label($"Target Value: {module.VALUE}");

                                float round = module.IsPlanarVALUE ? 2.5f : 0.25f;

                                float value = module.VALUE;
                                if (module.IsPlanarVALUE)
                                {
                                    value = ((value + 180) % 360) - 180;
                                    if (GUIOverseer.TextSliderPair("Target Value: ", ref valueCache, ref value, -180f, 180f, true, round))
                                        module.VALUE = (value + 360) % 360;
                                }
                                else
                                {
                                    if (module.UseLIMIT)
                                    {
                                        if (GUIOverseer.TextSliderPair("Target Value: ", ref valueCache, ref value, module.MINVALUELIMIT, module.MAXVALUELIMIT, true, round))
                                            module.VALUE = value;
                                    }
                                    else
                                    {
                                        if (GUIOverseer.TextSliderPair("Target Value: ", ref valueCache, ref value, 0f, module.TrueLimitVALUE, true, round))
                                            module.VALUE = value;
                                    }
                                }

                                GUIOverseer.TextSliderPair("Current Velocity: ", ref velocityCache, ref module.VELOCITY, -module.TrueMaxVELOCITY, module.TrueMaxVELOCITY, true, module.TrueMaxVELOCITY / 20f);
                                GUIOverseer.TextSliderPair("Max Velocity: ", ref maxVelCache, ref module.MAXVELOCITY, 0f, module.TrueMaxVELOCITY, true, module.TrueMaxVELOCITY / 10f);

                                GUILayout.Space(12);

                                if (!module.HardLIMIT)
                                {
                                    GUI.changed = false;
                                    module.UseLIMIT = GUILayout.Toggle(module.UseLIMIT, "Use limits");
                                    if (GUI.changed && module.IsFreeJoint)
                                    {
                                        module.SetupFreeJoint();
                                    }
                                }

                                GUILayout.BeginHorizontal();
                                {
                                    if (!module.HardLIMIT) GUILayout.Space(12);
                                    GUILayout.BeginVertical();
                                    {
                                        if (module.IsPlanarVALUE)
                                        {
                                            float min = ((module.MINVALUELIMIT + 180f) % 450f) - 180f;
                                            float max = ((module.MAXVALUELIMIT + 180f) % 450f) - 180f;
                                            GUILayout.Label("Min Value Limit: " + min);
                                            GUILayout.Label("Max Value Limit: " + max);
                                            float cen = module._CENTERLIMIT, ext = module._EXTENTLIMIT;
                                            GUILayout.Label("Note, CENTER & EXTENT are used here for percision");
                                            if (GUIOverseer.TextSliderPair("Center of Limit: ", ref minCache, ref cen, 0f, module.TrueLimitVALUE, true, round)
                                                | GUIOverseer.TextSliderPair("Extent of Limit: ", ref maxCache, ref ext, 0f, module.TrueLimitVALUE / 0.5f, true, round)) // | instead of ||, so it doesn't skip
                                            {
                                                module._CENTERLIMIT = cen;
                                                module._EXTENTLIMIT = ext;
                                                module.SetMinLimit(module.MINVALUELIMIT, false);
                                                module.SetMaxLimit(module.MAXVALUELIMIT, false);
                                                queueResetTextCache = true;
                                            }
                                        }
                                        else
                                        {
                                            float min = module.MINVALUELIMIT;
                                            if (GUIOverseer.TextSliderPair("Min Value Limit: ", ref minCache, ref min, 0f, module.TrueLimitVALUE, true, round))
                                            {
                                                module.SetMinLimit(min);
                                                queueResetTextCache = true;
                                            }
                                            float max = module.MAXVALUELIMIT;
                                            if (GUIOverseer.TextSliderPair("Max Value Limit: ", ref maxCache, ref max, 0f, module.TrueLimitVALUE, true, round))
                                            {
                                                module.SetMaxLimit(max);
                                                queueResetTextCache = true;
                                            }
                                        }
                                    }
                                    GUILayout.EndVertical();
                                }
                                GUILayout.EndHorizontal();

                                if (!module.CanOnlyBeLockJoint)
                                {
                                    GUILayout.Space(12);

                                    GUILayout.BeginVertical(GUI.skin.window);
                                    {
                                        GUILayout.Label("Joint dynamics");
                                        if (GUILayout.Toggle(module.IsLockJoint, "Lock-Joint (Static state)"))
                                            module.moverType = ModuleBlockMover.MoverType.Static;
                                        GUILayout.Label("Static state fixates the position and removes physics from the body entirely. (Pre-overhaul)");
                                        GUILayout.BeginHorizontal();
                                        {
                                            GUILayout.Space(12);
                                            GUILayout.BeginVertical();
                                            {
                                                module.LockJointBackPush = GUILayout.Toggle(module.LockJointBackPush, "Back-push movement");
                                            }
                                            GUILayout.EndVertical();
                                        }
                                        GUILayout.EndHorizontal();
                                        GUILayout.Space(12);

                                        if (GUILayout.Toggle(module.IsBodyJoint, "Dynamic-Joint (Dynamic state)"))
                                            module.moverType = ModuleBlockMover.MoverType.Dynamic;
                                        GUILayout.Label("Dynamic state gives the body rigid physics, restricted to the joint.");

                                        GUILayout.Space(12);

                                        GUI.enabled = !module.CannotBeFreeJoint;

                                        if (GUILayout.Toggle(module.IsFreeJoint, "Free-Joint (Suspension state)"))
                                            module.moverType = ModuleBlockMover.MoverType.Physics;
                                        GUILayout.Label("Suspension state frees the physics joints to allow manipulation from influences, including the spring joint");
                                        GUILayout.BeginHorizontal();
                                        {
                                            GUILayout.Space(12);
                                            GUILayout.BeginVertical();
                                            {
                                                if (GUIOverseer.TextSliderPair("  Spring Strength: ", ref springCache, ref module.SPRSTR, 0, 2000, false, 5f))
                                                    module.UpdateSpringForce();
                                                if (GUIOverseer.TextSliderPair("  Spring Dampen: ", ref dampCache, ref module.SPRDAM, 0, 1000, false, 2.5f))
                                                    module.UpdateSpringForce();
                                            }
                                            GUILayout.EndVertical();
                                        }
                                        GUILayout.EndHorizontal();

                                        module.CannotBeFreeJoint = module.CannotBeFreeJoint; // Ensure GUI did not set it to free-joint if unpermitted
                                        GUI.enabled = allowGUI;
                                    }
                                    GUILayout.EndVertical();
                                }
                            }
                            GUILayout.EndVertical();

                            GUILayout.Space(16);

                            if (module.Holder != null)
                            {
                                GUILayout.Label($"Rigidbody mass: {module.Holder.rbody_mass}");
                                GUILayout.Label($"Rigidbody CoM: {module.Holder.rbody_centerOfMass}");
                                GUILayout.Label($"Blocks on body: {module.Holder.blocks.Count}");
                                if (module.HolderJoint != null)
                                {
                                    GUILayout.Label($"Target spring: {(module.IsPlanarVALUE ? module.HolderJoint.targetRotation * Vector3.forward : module.HolderJoint.targetPosition)}");
                                }
                            }
                            GUILayout.Label($"Tank mass: {module.block.tank.rbody.mass}");
                            GUILayout.Label($"Tank CoM: {module.block.tank.rbody.centerOfMass}");

                            //GUILayout.Space(16);

                            //if (GUILayout.Button("Close"))
                            //{
                            //    visible = false;
                            //    IsSettingKeybind = false;
                            //    module = null;
                            //}

                            GUILayout.Space(16);
                            GUILayout.Label("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -", noWrap);
                            GUILayout.Space(16);

                            GUILayout.Label("EXPERIMENTAL SOUND TEST, BEWARE");
                            GUILayout.Label("SFX: " + module.SFX.ToString());
                            if (int.TryParse(GUILayout.TextField(((int)module.SFX).ToString()), out int result))
                                module.SFX = (TechAudio.SFXType)result;
                            string v = module.SFXVolume.ToString();
                            GUIOverseer.TextSliderPair("Volume: ", ref v, ref module.SFXVolume, 0f, 2f, false, 0.1f);
                            module.SFXParam = GUILayout.TextField(module.SFXParam);

                            //if (segment)
                            //{
                            //    GUILayout.Space(16);
                            //    GUILayout.Label("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -", noWrap);
                            //    GUILayout.Space(16);
                            //    GUILayout.Label("Rail Segment Weights");
                            //    string c = segment.AnimWeight.ToString();
                            //    GUIOverseer.TextSliderPair("This Weight: ", ref c, ref segment.AnimWeight, 0f, 0.25f, false, 0.01f);
                            //}
                        }
                        GUILayout.EndScrollView();

                        GUI.enabled = true;
                        UIInputPopup.Show(32f, 16f - Scrolls[1].y);
                        UIOperatorPopup.Show(32f, 16f - Scrolls[1].y);
                        GUI.enabled = allowGUI;
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(GUILayout.MaxWidth(320));
                    {
                        Scrolls[0] = GUILayout.BeginScrollView(Scrolls[0]);
                        {
                            if (Texts == null)
                            {
                                Texts = InputOperator.ProcessOperationsToStringArray(module.ProcessOperations, true);
                            }

                            GUI.changed = false;

                            GUILayout.Space(16);

                            for (int index = 0; index < Texts.Length; index++)
                            {
                                bool state = module.ProcessOperations[index].LASTSTATE;
                                if (GUILayout.Button(Texts[index], SelectedIndex == index ? (state ? pvSelOn : pvSelOff) : (state ? pvOn : pvOff)))
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
                                if (GUILayout.Button("Insert"))
                                {
                                    module.ProcessOperations.Insert(++SelectedIndex, new InputOperator());
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
                                    if (GUILayout.Button("Remove"))
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
                                    GUIUtility.systemCopyBuffer = string.Join("\n", InputOperator.ProcessOperationsToStringArray(module.ProcessOperations));
                                    Log = "Copied";
                                }
                                if (GUILayout.Button("Paste text"))
                                {
                                    Log = InputOperator.StringArrayToProcessOperations(GUIUtility.systemCopyBuffer, ref module.ProcessOperations);
                                    Texts = null;
                                }
                                // Copy as Text, Paste from Text
                            }
                            GUILayout.EndHorizontal();
                            if (!string.IsNullOrEmpty(Log))
                                GUILayout.Label(Log);
                        }
                        GUILayout.EndVertical();
                        bool _showPresetsUI = GUILayout.Toggle(showPresetsUI, "Function Presets", GUI.skin.button);
                        if (_showPresetsUI != showPresetsUI)
                        {
                            PresetsIMGUI.SetState(_showPresetsUI, module, win);
                            showPresetsUI = _showPresetsUI;
                        }
                    }
                    GUILayout.EndVertical(); // Processes for <block>

                    GUI.enabled = true;

                    if (IsItemSelected && !UpdateSelection)
                    {
                        var io = module.ProcessOperations[SelectedIndex];

                        UIOperatorPopup.List(16f, 16f - Scrolls[1].y);
                        UIInputPopup.List(16f, 16f - Scrolls[1].y);

                        var InputType = UIInputPopup.SelectedValue;
                        if (InputType != io.m_InputType)
                        {
                            io.m_InputType = InputType;
                            io.m_InputParam = InputOperator.UIInputPairs[InputType].DefaultValue;
                            Texts = null;
                            ResetTextCache();
                        }
                        var OperationType = UIOperatorPopup.SelectedValue;
                        if (OperationType != io.m_OperationType)
                        {
                            io.m_OperationType = OperationType;
                            if (OperationType == InputOperator.OperationType.EndIf || OperationType == InputOperator.OperationType.ElseThen)
                                io.m_InputType = InputOperator.InputType.AlwaysOn;
                            io.m_Strength = InputOperator.UIOperationPairs[OperationType].DefaultValue;
                            io.m_InternalTimer = 0f;
                            Texts = null;
                            ResetTextCache();
                        }
                    }
                    else
                    {
                        if (UIInputPopup.isVisible) UIInputPopup.Hide();
                        if (UIOperatorPopup.isVisible) UIOperatorPopup.Hide();
                    }
                }
                GUILayout.EndHorizontal();

                GUI.DragWindow();
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
                visible = false;
                module = null;
                PresetsIMGUI.SetState(false, null, win);
                showPresetsUI = false;
            }
        }

        //string propertycache = "Press BACKSLASH";
        //private void rebuildpropertycache()
        //{
        //    propertycache = "Params: ";
        //    foreach (var mmmm in (typeof(FMODEventInstance).GetField("m_ParamDatabase", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null) as Dictionary<string, Dictionary<string, int>>))
        //    {
        //        propertycache += "\n- " + mmmm.Key;
        //        foreach (var mmmmm in mmmm.Value)
        //        {
        //            propertycache += "\n  - " + mmmmm.Value + " " + mmmmm.Key;
        //        }
        //    }
        //}

        private bool IsItemSelected => SelectedIndex != -1 && SelectedIndex < module.ProcessOperations.Count;
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

        public void Update()
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