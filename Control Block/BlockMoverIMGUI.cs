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
            inst.gameObject.SetActive(OptionMenuMover.inst.check_OnGUI() || OptionMenuSteeringRegulator.inst.check_OnGUI() || LogGUI.inst.check_OnGUI());
        }
        public void OnGUI()
        {
            OptionMenuMover.inst.stack_OnGUI();
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
                win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 100f, 650f, 350f);
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
        GUIStyle pvOn, pvOff, pvSelOn, pvSelOff, noWrap, bigLabel, paddedBigLabel;

        public static OptionMenuMover inst;

        private readonly int EditID = 7788, ListID = 7789;

        private bool visible = false, showTechList = false, showListTools;

        private ModuleBlockMover module;

        //private ModuleBMSegment segment;

        private Rect win, listwin;

        public bool queueResetTextCache;

        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.C))
            {
                listhighlight?.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.CustomSkinHighlight);
                listhighlight = null;
                lastlisthighlight = -1;
                showTechList = !showTechList;
                if (showTechList)
                {
                    if (Singleton.playerTank == null)
                        showTechList = false;
                    else
                    {
                        Singleton.playerTank.GetComponentsInChildren<ModuleBlockMover>(cachedBlockList);
                        if (cachedBlockList.Count == 0)
                            showTechList = false;
                        else
                        {
                            cachedBlockList.Sort((x, y) => String.Compare(x.UIName, y.UIName));
                            lastBlockCount = Singleton.playerTank.blockman.blockCount;
                            int X = Screen.width, Y = Screen.height;
                            listwin = new Rect(X * 0.96f - 400f, Y * 0.2f, 400f, Mathf.Max(550, Y * 0.6f));
                            Scrolls[2] = Vector2.zero;
                            Scrolls[3] = Vector2.zero;
                            showListTools = false;
                            filterCache = "";
                            toolsLogLocalInput = "";
                            toolsLogSpeed = "";
                            toolsLogPaste = "";
                            speedPistonCache = null;
                            speedSwivelCache = null;
                        }
                    }
                }
                else
                {
                    foreach (var pair in cachedInputs) pair.Value.Clear();
                    cachedInputs.Clear();
                    foreach (var pair in cachedInputBlocks) pair.Value.Clear();
                    cachedBlockList.Clear();
                    ClearListLightUp();
                }
            }
            if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetMouseButtonDown(1))
            {
                ModuleBlockMover lastModule = module;
                try
                {
                    var block = Singleton.Manager<ManPointer>.inst.targetVisible.block;
                    if (block != null && block.tank != null)
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
                win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 200f, 700f, 450f);
                SetupEditWindow(module, lastModule);
            }
            if (queueResetTextCache) ResetTextCache();
        }

        void SetupEditWindow(ModuleBlockMover newModule, ModuleBlockMover lastModule)
        {
            module = newModule;
            visible = newModule;
            if (visible)
            {
                if (newModule != lastModule)
                {
                    lastModule?.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                    newModule.block.visible.EnableOutlineGlow(true, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                    SelectedIndex = -1;
                    ResetTextCache();
                    Log = "";
                    Texts = null;
                    Scrolls[0] = Vector2.zero;
                    Scrolls[1] = Vector2.zero;
                    IsSettingKeybind = false;
                    GUI.FocusWindow(EditID);
                }
            }
            else lastModule?.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
        }

        void VerifyListWindow()
        {
            Tank playerTank = Singleton.playerTank;
            if (playerTank == null)
            {
                listhighlight?.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.CustomSkinHighlight);
                listhighlight = null;
                lastlisthighlight = -1;
                lastBlockCount = -1;
                showTechList = false;
                foreach (var pair in cachedInputs) pair.Value.Clear();
                cachedInputs.Clear();
                foreach (var pair in cachedInputBlocks) pair.Value.Clear();
                cachedInputBlocks.Clear();
                cachedBlockList.Clear();
                ClearListLightUp();
                return;
            }
            if (lastBlockCount != playerTank.blockman.blockCount)
            {
                cachedBlockList.Clear();
                playerTank.GetComponentsInChildren<ModuleBlockMover>(cachedBlockList);
                if (cachedBlockList.Count == 0)
                {
                    listhighlight?.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.CustomSkinHighlight);
                    listhighlight = null;
                    lastlisthighlight = -1;
                    lastBlockCount = -1;
                    showTechList = false;
                    foreach (var pair in cachedInputs) pair.Value.Clear();
                    cachedInputs.Clear();
                    foreach (var pair in cachedInputBlocks) pair.Value.Clear();
                    cachedInputBlocks.Clear();
                    cachedBlockList.Clear();
                    ClearListLightUp();
                    return;
                }
                cachedBlockList.Sort((x, y) => String.Compare(x.UIName, y.UIName));
                lastBlockCount = playerTank.blockman.blockCount;
                if (showListTools)
                    RegenerateListKeyCodeCache();
            }
        }

        internal void ResetTextCache()
        {
            queueResetTextCache = false;
            maxCache = null; //module.MAXVALUELIMIT.ToString();
            minCache = null; //module.MINVALUELIMIT.ToString();
            maxVelCache = module.MAXVELOCITY.ToString();
            //springCache = module.SPRSTR.ToString();
            //dampCache = module.SPRDAM.ToString();
            valueCache = module.VALUE.ToString();
            velocityCache = module.VELOCITY.ToString();
        }

        public bool check_OnGUI()
        {
            return showTechList || visible && module;
        }

        public void stack_OnGUI()
        {
            if (showTechList)
            {
                try
                {
                    VerifyListWindow();
                    if (showTechList)
                        listwin = GUI.Window(ListID, listwin, DoListWindow, "Blockmover List : " +Singleton.playerTank?.name);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            if (visible && module)
            {
                if (module.IsControlledByNet)
                {
                    visible = false;
                    module?.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                }
                else
                {
                    try
                    {
                        win = GUI.Window(EditID, win, DoEditWindow, module.UIName + (module.Valid || module.startblockpos.Length == 0 ? "" : " [" + module.InvalidReason + "]"));
                        if (showPresetsUI)
                            PresetsIMGUI.DoWindow();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        private bool IsSettingKeybind;
        internal static int SelectedIndex;

        private Popup<InputOperator.InputType> UIInputPopup;
        private Popup<InputOperator.OperationType> UIOperatorPopup;
        private Vector2[] Scrolls = new Vector2[4];
        internal string[] Texts;
        private string paramCache, strengthCache, valueCache, velocityCache, minCache, maxVelCache, maxCache, filterCache, speedPistonCache, speedSwivelCache;
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

            bigLabel = new GUIStyle(GUI.skin.label)
            {
                font = GUI.skin.button.font,
                fontSize = GUI.skin.button.fontSize,
                fontStyle = GUI.skin.button.fontStyle,
            };
            paddedBigLabel = new GUIStyle(bigLabel)
            {
                margin = new RectOffset(8, 4, 8, 4),
                alignment = TextAnchor.MiddleLeft,
            };
        }

        bool UpdateSelection;

        private int lastBlockCount = -1;
        private List<ModuleBlockMover> cachedBlockList = new List<ModuleBlockMover>();
        private Dictionary<KeyCode, List<InputOperator>> cachedInputs = new Dictionary<KeyCode, List<InputOperator>>();
        private Dictionary<KeyCode, List<Visible>> cachedInputBlocks = new Dictionary<KeyCode, List<Visible>>();
        private List<Visible> listLightUpVisRef;
        private List<ModuleBlockMover> listLightUpMBMRef;
        private List<ModuleBlockMover> filteredBlockList = new List<ModuleBlockMover>();
        private Visible listhighlight;
        private int lastlisthighlight;
        private bool IsListSettingKeybind;
        private KeyCode ListSelectedIndex;
        private bool CanHighlightList, CanHighlightTools;
        private string toolsLogLocalInput = "", toolsLogSpeed = "", toolsLogPaste;
        private float toolsPSH, toolsPSL, toolsPSM, toolsSSH, toolsSSL, toolsSSM;

        void UpdateListLightUp(List<ModuleBlockMover> newList)
        {
            if (newList == listLightUpMBMRef) return;
            ClearListLightUp();
            listLightUpMBMRef = newList;
            cakeslice.OutlineEffect.Instance.SetSkinPaintingColour(true);
            foreach (ModuleBlockMover mover in newList)
            {
                mover.block.visible.EnableOutlineGlow(true, cakeslice.Outline.OutlineEnableReason.CustomSkinHighlight);
            }
        }

        void UpdateListLightUp(List<ModuleBlockMover> newList, bool IsPlanarValue)
        {
            if (newList == listLightUpMBMRef) return;
            ClearListLightUp();
            listLightUpMBMRef = newList;
            cakeslice.OutlineEffect.Instance.SetSkinPaintingColour(true);
            foreach (ModuleBlockMover mover in newList)
            {
                mover.block.visible.EnableOutlineGlow(mover.IsPlanarVALUE == IsPlanarValue, cakeslice.Outline.OutlineEnableReason.CustomSkinHighlight);
            }
        }

        void UpdateListLightUp(List<Visible> newList)
        {
            if (newList == listLightUpVisRef) return;
            ClearListLightUp();
            listLightUpVisRef = newList;
            cakeslice.OutlineEffect.Instance.SetSkinPaintingColour(true);
            foreach (Visible mover in newList)
            {
                mover.EnableOutlineGlow(true, cakeslice.Outline.OutlineEnableReason.CustomSkinHighlight);
            }
        }

        void ClearListLightUp()
        {
            if (listLightUpVisRef != null)
            {
                foreach (Visible mover in listLightUpVisRef)
                {
                    mover.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.CustomSkinHighlight);
                }
                listLightUpVisRef = null;
            }
            if (listLightUpMBMRef != null)
            {
                foreach (ModuleBlockMover mover in listLightUpMBMRef)
                {
                    mover.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.CustomSkinHighlight);
                }
                listLightUpMBMRef = null;
            }
        }


        private void DoListWindow(int id)
        {
            try
            {
                if (IsListSettingKeybind)
                {
                    var e = Event.current;
                    if (e.isKey)
                    {
                        var newkey = e.keyCode;
                        if (newkey == KeyCode.Escape || newkey == KeyCode.Delete) newkey = KeyCode.None;
                        foreach (var oper in cachedInputs[ListSelectedIndex])
                            oper.m_InputKey = newkey;
                        IsListSettingKeybind = false;
                    }
                }

                GUILayout.BeginVertical();
                {
                    if (paddedBigLabel == null)
                        CreateGUIStyles();

                    GUILayout.BeginVertical();
                    var BlockList = showListTools ? filteredBlockList : cachedBlockList;
                    Scrolls[2] = GUILayout.BeginScrollView(Scrolls[2]);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Search: ", bigLabel, GUILayout.MaxWidth(80));
                            GUI.changed = false;
                            filterCache = GUILayout.TextField(filterCache).ToLower();
                            if (GUI.changed && showListTools) RegenerateListKeyCodeCache();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(8);

                        GUILayout.Label("Hover over an item to show where it is on the tech.\nClick on an item's name to rename the block.");
                        //bool filterFlag = !string.IsNullOrEmpty(filterCache);
                        for (int i = 0; i < BlockList.Count; i++)
                        {
                            ModuleBlockMover mover = BlockList[i];
                            //if (filterFlag && !mover.UIName.ToLower().Contains(filterCache))
                            //    continue;

                            GUILayout.BeginHorizontal(GUI.skin.button);
                            {
                                mover.UIName = GUILayout.TextField(mover.UIName, paddedBigLabel);
                                if (GUILayout.Button("Edit", GUILayout.MaxWidth(80)))
                                {
                                    win = new Rect(Screen.width * 0.5f - 450, Screen.height * 0.5f - 200f, 700f, 450f);
                                    SetupEditWindow(mover, module);
                                }
                            }
                            GUILayout.EndHorizontal();
                            //if (GUILayoutUtility.GetLastRect().Contains(Input.mousePosition))
                            if (CanHighlightList && Event.current.type == EventType.Repaint &&
                                GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                            {
                                if (i != lastlisthighlight)
                                {
                                    if (listhighlight) listhighlight.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.CustomSkinHighlight);
                                    lastlisthighlight = i;
                                    listhighlight = mover.block.visible;
                                    cakeslice.OutlineEffect.Instance.SetSkinPaintingColour(mover.Valid || mover.startblockpos.Length == 0);
                                    listhighlight.EnableOutlineGlow(true, cakeslice.Outline.OutlineEnableReason.CustomSkinHighlight);
                                }
                            }
                            else if (i == lastlisthighlight)
                            {
                                if (listhighlight) listhighlight.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.CustomSkinHighlight);
                                listhighlight = null;
                                lastlisthighlight = -1;
                            }
                        }

                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                    if (Event.current.type == EventType.Repaint) CanHighlightList = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);

                    if (showListTools)
                    {
                        bool highlightFlag = false;
                        GUILayout.BeginVertical("Tools", GUI.skin.window, GUILayout.MinHeight(250), GUILayout.MaxHeight(Mathf.Max(listwin.height * 0.4f, 300)));
                        Scrolls[3] = GUILayout.BeginScrollView(Scrolls[3]);
                        {
                            GUILayout.Label("You might want to save your tech before you slip up.\nKeep in mind that these obey the Search filter above");

                            GUILayout.Space(8);

                            GUILayout.Label("LocalInput Rewriter", bigLabel);
                            GUILayout.Label("  Set all the LocalInput toggles on the tech.");
                            GUILayout.BeginHorizontal(/*"LocalInput Rewriter", GUI.skin.window*/);
                            {
                                if (GUILayout.Button("Set Local"))
                                {
                                    int counter = 0;
                                    foreach (ModuleBlockMover mover in BlockList)
                                    {
                                        counter += mover.LOCALINPUT ? 0 : 1;
                                        mover.LOCALINPUT = true;
                                    }
                                    toolsLogLocalInput = "    Set " + counter.ToString() + " block" + (counter != 1 ? "s" : "") + " to Local input" + (counter != BlockList.Count ? " (" + BlockList.Count + " total)" : "");
                                }
                                if (GUILayout.Button("Set Global"))
                                {
                                    int counter = 0;
                                    foreach (ModuleBlockMover mover in BlockList)
                                    {
                                        counter += mover.LOCALINPUT ? 1 : 0;
                                        mover.LOCALINPUT = false;
                                    }
                                    toolsLogLocalInput = "    Set " + counter.ToString() + " block" + (counter != 1 ? "s" : "") + " to Global input" + (counter != BlockList.Count ? " (" + BlockList.Count + " total)" : "");
                                }

                            }
                            GUILayout.EndHorizontal();
                            if (CanHighlightTools && Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                            {
                                highlightFlag = true; UpdateListLightUp(filteredBlockList);
                            }

                            if (!string.IsNullOrEmpty(toolsLogLocalInput)) GUILayout.Label(toolsLogLocalInput);
                            
                            GUILayout.Space(8);

                            GUILayout.Label("Velocity Rewriter", bigLabel);
                            GUILayout.Label("  Set how fast all the movers should move things.\n  Use the search filter to avoid setting the wrong ones.");

                            if (!float.IsNegativeInfinity(toolsPSM))
                            {
                                GUILayout.Space(4);

                                GUILayout.Label(toolsPSL != toolsPSH ? "  (Lowest Piston speed: " + toolsPSL.ToString() + ")" : "");
                                GUILayout.BeginVertical();
                                if (GUIOverseer.TextSliderPair("  Max Piston speed: ", ref speedPistonCache, ref toolsPSH, 0f, toolsPSM, true, 0.005f))
                                {
                                    float newSpeed = toolsPSH;
                                    toolsPSL = float.PositiveInfinity;
                                    toolsPSH = float.NegativeInfinity;
                                    int counter = 0;
                                    foreach (ModuleBlockMover mover in BlockList)
                                    {
                                        if (mover.IsPlanarVALUE) continue;
                                        counter++;
                                        mover.MAXVELOCITY = Mathf.Min(newSpeed, mover.TrueMaxVELOCITY);
                                        if (mover.MAXVELOCITY < toolsPSL) toolsPSL = mover.MAXVELOCITY;
                                        if (mover.MAXVELOCITY > toolsPSH) toolsPSH = mover.MAXVELOCITY;
                                    }
                                    toolsLogSpeed = "    Set " + counter.ToString() + " piston" + (counter != 1 ? "s" : "") + " to speed: " + newSpeed.ToString() + (toolsPSL != toolsPSH ? "\n     - Some were clamped" : "");
                                }
                                GUILayout.EndVertical();
                                if (CanHighlightTools && Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                                {
                                    highlightFlag = true; UpdateListLightUp(filteredBlockList, false);
                                }
                            }
                            if (!float.IsNegativeInfinity(toolsSSM))
                            {
                                GUILayout.Space(4);

                                GUILayout.Label(toolsSSL != toolsSSH ? "  (Lowest Swivel speed: " + toolsSSL.ToString() + ")" : "");
                                GUILayout.BeginVertical();
                                if (GUIOverseer.TextSliderPair("  Max Swivel speed: ", ref speedSwivelCache, ref toolsSSH, 0f, toolsSSM, true, 0.005f))
                                {
                                    float newSpeed = toolsSSH;
                                    toolsSSL = float.PositiveInfinity;
                                    toolsSSH = float.NegativeInfinity;
                                    int counter = 0;
                                    foreach (ModuleBlockMover mover in BlockList)
                                    {
                                        if (!mover.IsPlanarVALUE) continue;
                                        counter++;
                                        mover.MAXVELOCITY = Mathf.Min(newSpeed, mover.TrueMaxVELOCITY);
                                        if (mover.MAXVELOCITY < toolsSSL) toolsSSL = mover.MAXVELOCITY;
                                        if (mover.MAXVELOCITY > toolsSSH) toolsSSH = mover.MAXVELOCITY;
                                    }
                                    toolsLogSpeed = "    Set " + counter.ToString() + " swivel" + (counter != 1 ? "s" : "") + " to speed: " + newSpeed.ToString() + (toolsPSL != toolsPSH ? "\n     - Some were clamped" : "");
                                }
                                GUILayout.EndVertical();
                                if (CanHighlightTools && Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                                {
                                    highlightFlag = true; UpdateListLightUp(filteredBlockList, true);
                                }
                            }
                            if (!string.IsNullOrEmpty(toolsLogSpeed)) GUILayout.Label(toolsLogSpeed);

                            GUILayout.Space(8);

                            GUILayout.Label("Keybinder", bigLabel);
                            GUILayout.Label("  Very dangerous!\n  Input ESC or DEL to set to None.");
                            if (cachedInputs.Count == 0)
                            {
                                GUILayout.Label("\n  Huh, there are no input keys...");
                            }
                            else
                            {
                                GUILayout.BeginVertical(/*"Keybind Rewriter", GUI.skin.window*/);
                                foreach (var pair in cachedInputs)
                                {
                                    GUILayout.BeginHorizontal();
                                    {
                                        GUI.changed = false;
                                        IsListSettingKeybind = GUILayout.Button((IsListSettingKeybind && pair.Key == ListSelectedIndex ? "Press a key" : pair.Value[0].m_InputKey.ToString()) + " (" + pair.Value.Count.ToString() + ")") != IsListSettingKeybind;
                                        if (GUI.changed)
                                            ListSelectedIndex = pair.Key;
                                        if (pair.Value[0].m_InputKey == KeyCode.None && GUILayout.Button("Delete", GUILayout.MaxWidth(80)))
                                        {
                                            foreach (ModuleBlockMover mover in cachedBlockList)
                                            {
                                                int i = 0;
                                                var operList = mover.ProcessOperations;
                                                while (i < operList.Count)
                                                {
                                                    InputOperator oper = operList[i++];
                                                    var uiInput = InputOperator.UIInputPairs[oper.m_InputType];
                                                    if (uiInput.HideInputKey || oper.m_InputKey != pair.Value[0].m_InputKey) continue;

                                                    if ((oper.m_InputParam > 0) == (oper.m_InputType == InputOperator.InputType.Toggle)) oper.m_InputType = InputOperator.InputType.AlwaysOn;
                                                    else if (oper.m_OperationType == InputOperator.OperationType.IfThen
                                                          || oper.m_OperationType == InputOperator.OperationType.OrThen) oper.m_InputKey = KeyCode.None;
                                                    else operList.RemoveAt(--i);
                                                }
                                            }

                                            cachedInputs.Remove(pair.Key);
                                            GUILayout.EndHorizontal();
                                            break;
                                        }
                                    }
                                    GUILayout.EndHorizontal();
                                    if (CanHighlightTools && Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                                    {
                                        highlightFlag = true; UpdateListLightUp(cachedInputBlocks[pair.Key]);
                                    }
                                }
                                GUILayout.EndVertical();
                            }

                            GUILayout.Space(12);

                            GUILayout.Label("Paste Text Over All", bigLabel);
                            GUILayout.Label("  DO NOT POKE UNLESS YOU KNOW THE RISKs!\n  Copy a block's values in their GUI, and press this\n  to overwrite the values on all the other blocks");
                            if (GUILayout.Button("Paste Text on " + BlockList.Count + " block" + (BlockList.Count != 1 ? "s" : "")))
                            {
                                bool flag = true;
                                foreach(var module in BlockList)
                                {
                                    string buffer = GUIUtility.systemCopyBuffer;
                                    module.SetValuesFromCommentedString(buffer);
                                    if (flag)
                                    {
                                        flag = false;
                                        toolsLogPaste = InputOperator.StringArrayToProcessOperations(buffer, ref module.ProcessOperations);
                                    }
                                    else InputOperator.StringArrayToProcessOperations(buffer, ref module.ProcessOperations);
                                }
                                RegenerateListKeyCodeCache();
                            }
                            if (CanHighlightTools && Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                            {
                                highlightFlag = true; UpdateListLightUp(BlockList);
                            }
                            GUILayout.Label(toolsLogPaste);
                        }
                        GUILayout.EndScrollView();
                        GUILayout.EndVertical();
                        if (Event.current.type == EventType.Repaint)
                            if (!(CanHighlightTools = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) || !highlightFlag)
                                ClearListLightUp();
                    }
                    else if (showListTools = GUILayout.Button("Show Tools"))
                    {
                        RegenerateListKeyCodeCache();
                    }
                }
                GUILayout.EndVertical();
                GUI.DragWindow();
            }
            catch { } // Weird GUI bugs
        }

        private void RegenerateListKeyCodeCache()
        {
            ClearListLightUp();
            foreach (var pair in cachedInputs) pair.Value.Clear();
            cachedInputs.Clear();
            foreach (var pair in cachedInputBlocks) pair.Value.Clear();
            cachedInputBlocks.Clear();
            bool filterFlag = !string.IsNullOrEmpty(filterCache);
            toolsPSL = float.PositiveInfinity;
            toolsSSL = float.PositiveInfinity;
            toolsPSH = float.NegativeInfinity;
            toolsSSH = float.NegativeInfinity;
            toolsPSM = float.NegativeInfinity;
            toolsSSM = float.NegativeInfinity;
            int movercount = 0;
            filteredBlockList.Clear();
            speedPistonCache = null;
            speedSwivelCache = null;
            foreach (ModuleBlockMover mover in cachedBlockList)
            {
                if (filterFlag && !mover.UIName.ToLower().Contains(filterCache))
                    continue;

                filteredBlockList.Add(mover);
                movercount++;

                if (mover.IsPlanarVALUE)
                {
                    if (mover.MAXVELOCITY < toolsSSL) toolsSSL = mover.MAXVELOCITY;
                    if (mover.MAXVELOCITY > toolsSSH) toolsSSH = mover.MAXVELOCITY;
                    if (mover.TrueMaxVELOCITY > toolsSSM) toolsSSM = mover.TrueMaxVELOCITY;
                }
                else
                {
                    if (mover.MAXVELOCITY < toolsPSL) toolsPSL = mover.MAXVELOCITY;
                    if (mover.MAXVELOCITY > toolsPSH) toolsPSH = mover.MAXVELOCITY;
                    if (mover.TrueMaxVELOCITY > toolsPSM) toolsPSM = mover.TrueMaxVELOCITY;
                }

                foreach (var oper in mover.ProcessOperations)
                {
                    if (InputOperator.UIInputPairs[oper.m_InputType].HideInputKey) continue;
                    if (cachedInputs.TryGetValue(oper.m_InputKey, out List<InputOperator> list))
                        list.Add(oper);
                    else
                        cachedInputs.Add(oper.m_InputKey, new List<InputOperator>() { oper });

                    if (cachedInputBlocks.TryGetValue(oper.m_InputKey, out List<Visible> list2))
                        list2.Add(mover.block.visible);
                    else
                        cachedInputBlocks.Add(oper.m_InputKey, new List<Visible>() { mover.block.visible });
                }
            }
        }

        private static float ConditionalRound(float value, bool round, float step) => round && step != 0f ? Mathf.Round(value / step) * step : value;

        private void DoEditWindow(int id)
        {
            try
            {
                if (module == null || module.block.tank == null)
                {
                    module?.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
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
                            var newkey = e.keyCode;
                            if (newkey == KeyCode.Escape || newkey == KeyCode.Delete) newkey = KeyCode.None;
                            var io = module.ProcessOperations[SelectedIndex];
                            io.m_InputKey = newkey;
                            Texts = null;
                            IsSettingKeybind = false;
                        }
                    }
                }

                if (pvOn == null)
                    CreateGUIStyles();

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
                                        UIInputPopup.Button();
                                        GUILayout.Label(uii.UIDesc);
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
                                                                if (module.IsPlanarVALUE)
                                                                    temp = GUILayout.HorizontalSlider(temp, -module.HalfLimitVALUE, module.HalfLimitVALUE);
                                                                else
                                                                    temp = GUILayout.HorizontalSlider(Mathf.Abs(temp), 0, module.TrueLimitVALUE) * Mathf.Sign(temp);
                                                                temp = ConditionalRound(temp, GUI.changed, module.IsPlanarVALUE ? 2.5f : 0.25f);
                                                            }
                                                            else if (uii.SliderMaxIsMaxVel)
                                                            {
                                                                temp = GUILayout.HorizontalSlider(Mathf.Abs(temp), -module.TrueMaxVELOCITY, module.TrueMaxVELOCITY) * Mathf.Sign(temp);
                                                                temp = ConditionalRound(temp, GUI.changed, module.IsPlanarVALUE ? 0.25f : 0.1f);
                                                            }
                                                            else
                                                            {
                                                                if (uii.SliderMax != 0)
                                                                {
                                                                    temp = GUILayout.HorizontalSlider(Mathf.Abs(temp), 0, uii.SliderMax) * Mathf.Sign(temp);
                                                                    temp = ConditionalRound(temp, GUI.changed, 0.5f);
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
                                            temp = ConditionalRound(temp, GUI.changed, 0.05f);
                                        }
                                        else if (uio.SliderMaxIsMaxVel)
                                        {
                                            float vel = module.TrueMaxVELOCITY;
                                            temp = GUILayout.HorizontalSlider(temp, uio.SliderHasNegative ? -vel : 0f, vel);
                                            temp = ConditionalRound(temp, GUI.changed, module.IsPlanarVALUE ? 0.25f : 0.1f);
                                        }
                                        else if (uio.SliderPosFraction != 0f)
                                        {
                                            float frac = module.MAXVALUELIMIT * uio.SliderPosFraction;
                                            temp = GUILayout.HorizontalSlider(temp, (uio.SliderHasNegative || uio.SliderMinOnPlanar && module.IsPlanarVALUE) ? -frac : 0f, frac);
                                            temp = ConditionalRound(temp, GUI.changed, frac * 0.05f);
                                        }
                                        else if (uio.SliderMax != 0f)
                                        {
                                            if (uio.SliderHasNegative)
                                                temp = GUILayout.HorizontalSlider(temp, -uio.SliderMax, uio.SliderMax);
                                            else
                                                temp = GUILayout.HorizontalSlider(Mathf.Abs(temp), 0f, uio.SliderMax) * Mathf.Sign(temp);
                                            temp = ConditionalRound(temp, GUI.changed, 0.1f);
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

                            if (SelectedIndex != -1)
                            {
                                GUILayout.Label("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -", noWrap);

                                GUILayout.Space(16);
                            }

                            GUILayout.BeginVertical("Properties", GUI.skin.window);
                            {
                                module.LOCALINPUT = GUILayout.Toggle(module.LOCALINPUT, "Input local to tech");

                                GUILayout.Space(12);
                                
                                GUILayout.Label($"Current Value: {((module.PVALUE + 180f) % 360f) - 180f}");
                                //GUILayout.Label($"Target Value: {module.VALUE}");

                                float round = module.IsPlanarVALUE ? 2.5f : 0.25f;
                                float min, max;
                                float value = module.VALUE;
                                if (module.IsPlanarVALUE)
                                {
                                    min = ((module.MINVALUELIMIT + 181f) % 360f) - 181f;
                                    max = ((module.MAXVALUELIMIT + 179f) % 360f) - 179f;
                                    value = ((value + 180f) % 360.001f) - 180f;
                                    if (module.UseLIMIT)
                                    {
                                        if (GUIOverseer.TextSliderPair("Target Value (position): ", ref valueCache, ref value, module.MINVALUELIMIT, module.MAXVALUELIMIT, true, round))
                                            module.VALUE = (value + 360) % 360;
                                    }
                                    else
                                    {
                                        if (GUIOverseer.TextSliderPair("Target Value (position): ", ref valueCache, ref value, -180f, 180f, true, round))
                                            module.VALUE = (value + 360) % 360;
                                    }
                                }
                                else
                                {
                                    min = module.MINVALUELIMIT;
                                    max = module.MAXVALUELIMIT;
                                    if (module.UseLIMIT)
                                    {
                                        if (GUIOverseer.TextSliderPair("Target Value (position): ", ref valueCache, ref value, min, max, true, round))
                                            module.VALUE = value;
                                    }
                                    else
                                    {
                                        if (GUIOverseer.TextSliderPair("Target Value (position): ", ref valueCache, ref value, 0f, module.TrueLimitVALUE, true, round))
                                            module.VALUE = value;
                                    }
                                }

                                GUIOverseer.TextSliderPair("Current Velocity: ", ref velocityCache, ref module.VELOCITY, -module.MAXVELOCITY, module.MAXVELOCITY, true, module.TrueMaxVELOCITY / 20f);
                                GUIOverseer.TextSliderPair("Max Velocity (change rate): ", ref maxVelCache, ref module.MAXVELOCITY, 0f, module.TrueMaxVELOCITY, true, module.TrueMaxVELOCITY / 10f);

                                GUILayout.Space(12);

                                if (!module.HardLIMIT)
                                {
                                    GUI.changed = false;
                                    module.UseLIMIT = GUILayout.Toggle(module.UseLIMIT, "Use limits");
                                    /* if (GUI.changed && module.IsFreeJoint)
                                    {
                                        module.SetupFreeJoint();
                                    } */
                                }

                                GUILayout.BeginHorizontal();
                                {
                                    if (!module.HardLIMIT) GUILayout.Space(12);
                                    GUILayout.BeginVertical();
                                    {
                                        if (module.IsPlanarVALUE)
                                        {
                                            GUILayout.Label("Min Value Limit: " + min);
                                            GUILayout.Label("Max Value Limit: " + max);
                                            float cen = module._CENTERLIMIT, ext = module._EXTENTLIMIT;
                                            GUILayout.Label("\nNote, Center & Extent are used for swivels");
                                            if (GUIOverseer.TextSliderPair("Center of Limit: ", ref minCache, ref cen, 0f, module.TrueLimitVALUE, true, round) |
                                                GUIOverseer.TextSliderPair("Extent of Limit: ", ref maxCache, ref ext, 0f, module.HalfLimitVALUE, true, round)) // | instead of ||, so it doesn't skip drawing both
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
                                            if (GUIOverseer.TextSliderPair("Min Value Limit: ", ref minCache, ref min, 0f, module.TrueLimitVALUE, true, round))
                                            {
                                                module.SetMinLimit(min);
                                                queueResetTextCache = true;
                                            }
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

                                /* if (module.CanOnlyBeLockJoint)
                                { */
                                if (module.startblockpos.Length != 0)
                                {
                                    GUILayout.Space(12);
                                    module.LockJointBackPush = GUILayout.Toggle(module.LockJointBackPush, "Back-push movement");
                                }
                                /* }
                                else
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
                                */

                                GUILayout.Space(12f);

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label("Name");
                                    module.UIName = GUILayout.TextField(module.UIName, GUILayout.MaxWidth(240), GUILayout.MaxWidth(240));
                                }
                                GUILayout.EndVertical();
                            }
                            GUILayout.EndVertical();

                            GUILayout.Space(16);

                            GUILayout.Label("Tip: Pressing ALT C will open up a list of all blockmovers on this tech");

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
                            GUILayout.Label($"Tank Gravity: {module.block.tank.GetGravityScale()}");

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
                                    Scrolls[1] = Vector2.zero;
                                }
                            }

                            UpdateSelection = GUI.changed;

                            GUILayout.FlexibleSpace(); // Inflate scrollview
                        }
                        GUILayout.EndScrollView();

                        GUILayout.BeginVertical("Edit", GUI.skin.window);
                        {
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
                                if (GUILayout.Button("Copy all"))
                                {
                                    GUIUtility.systemCopyBuffer = module.GetValuesAsCommentedString() + "\n\n" + string.Join("\n", InputOperator.ProcessOperationsToStringArray(module.ProcessOperations));
                                    Log = "Copied w/ Property values";
                                }
                                if (GUILayout.Button("Paste text"))
                                {
                                    string buffer = GUIUtility.systemCopyBuffer;
                                    module.SetValuesFromCommentedString(buffer);
                                    Log = InputOperator.StringArrayToProcessOperations(buffer, ref module.ProcessOperations);
                                    UpdateSelection = true;
                                    Texts = null;
                                    queueResetTextCache = true;
                                }
                                // Copy as Text, Paste from Text
                            }
                            GUILayout.EndHorizontal();

                            if (!string.IsNullOrEmpty(Log))
                                GUILayout.Label(Log);
                        }
                        GUILayout.EndVertical();
                        //bool _showPresetsUI = GUILayout.Toggle(showPresetsUI, "Function Presets", GUI.skin.button);
                        //if (_showPresetsUI != showPresetsUI)
                        //{
                        //    PresetsIMGUI.SetState(_showPresetsUI, module, win);
                        //    showPresetsUI = _showPresetsUI;
                        //}
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
                            paramCache = io.m_InputParam.ToString();
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
                            strengthCache = io.m_Strength.ToString();
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
                module?.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
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

        private readonly int ID = 7787;

        private bool visible = false;

        private ModuleSteeringRegulator module;

        private Rect win;

        private string c0, c1, c2, c3;

        public void Update()
        {
            if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetMouseButtonDown(1))
            {
                win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 100f, 200f, 250f);
                try
                {
                    module = Singleton.Manager<ManPointer>.inst.targetVisible.block.GetComponent<ModuleSteeringRegulator>();
                    c0 = null;
                    c1 = null;
                    c2 = null;
                    c3 = null;
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
            GUIOverseer.TextSliderPair("Sensitivity RADIUS: ", ref c0, ref module.MaxDist, 0f, 20f, false, 0.25f);
            GUIOverseer.TextSliderPair("Drive PiD: ", ref c1, ref module.DriveMod, 0f, 1f, false, 0.1f);
            GUIOverseer.TextSliderPair("Throttle PiD: ", ref c2, ref module.ThrottleMod, 0f, 1f, false, 0.1f);
            GUIOverseer.TextSliderPair("Velcoity Dampen: ", ref c3, ref module.VelocityDampen, 0.1f, 1f, false, 0.05f);

            GUILayout.Space(16);

            if (GUILayout.Button("Close"))
            {
                visible = false;
                module = null;
            }
            GUI.DragWindow();
        }
    }
}