using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Control_Block
{
    static class PreConverter
    {
        #region Legacy Presets
        public static Dictionary<string, Preset> PreOverhaulSwivels = new Dictionary<string, Preset>
        {
            { "Positional", new Preset(true, "Legacy_Positional", "Converter for old swivel Positional mode",
                "<FuncForUseTimer>",
                new List<Preset.Variable> {
                    new Preset.StringConditionV() {
                        VarToReplace = "FuncForUseTimer", ValueName = "Use Start Delay", Description = "If there should be a delay between pressing a key and having the joint move",
                        ReplacementIfTrue = "IF(WhileHeld(<KeyRight>,0),<Time>)\nDO ShiftPos(<Amount>)\nENDIF\nIF(WhileHeld(<KeyLeft>,0),<Time>)\nDO ShiftPos(-<Amount>)\nENDIF",
                        ReplacementIfFalse = "WhileHeld(<KeyRight>,0) DO ShiftPos(<Amount>)\nWhileHeld(<KeyLeft>,0) DO ShiftPos(-<Amount>)",
                        KeysToHideIfFalse = 1
                    }, new Preset.FloatV() {
                        VarToReplace = "Time", ValueName = "Start Delay", Description = "(Only used if above is true) How long to wait before input is accepted",
                        DefaultValue = 1f,
                        MinValue = 0f,
                        MaxValue = 5f,
                        RestrictValue = false
                    }, new Preset.KeyCodeV() {
                        VarToReplace = "KeyRight", ValueName = "Right Key", Description = "Button to press for turning right",
                        DefaultValue = KeyCode.RightArrow
                    }, new Preset.KeyCodeV() {
                        VarToReplace = "KeyLeft", ValueName = "Left Key", Description = "Button to press for turning left",
                        DefaultValue = KeyCode.LeftArrow
                    }, new Preset.FloatV() {
                        VarToReplace = "Amount", ValueName = "Move Speed", Description = "How many degrees to move in a time frame",
                        DefaultValue = 5f,
                        LimitsAreVelocity = true,
                        RestrictValue = true
                    }
                }) },
        };
        public static Dictionary<string, Preset> PreOverhaulPistons = new Dictionary<string, Preset>
        {

        };
        #endregion
    }

    class Preset
    {
        public Preset(bool LockJoint, string Name, string Description, string Script, List<Variable> Variables)
        {
            this.Name = Name;
            this.Description = Description;
            this.Script = Script;
            this.Variables = Variables;
            if (LockJoint)
            {
                lockOffsetParent = true;
                moverType = ModuleBlockMover.MoverType.Static;
            }
        }

        public string Name, Description;

        public string Script;
        public List<Variable> Variables;

        public bool ExpandLimitsToBlock, ExpandVelocityToBlock;
        public float? minValueLimit, maxValueLimit,
            targetValue, velocity, maxVelocity, 
            jointStrength, jointDampen;
        public ModuleBlockMover.MoverType? moverType;
        public bool? lockOffsetParent, UseLimits;

        public override string ToString()
        {
            string nStr = Script;
            foreach (Variable v in Variables)
            {
                nStr = nStr.Replace("<" + v.VarToReplace + ">", v.Value);
            }
            return nStr;
        }

        public string ToJSONString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });
        }

        public static Preset FromJSONString(string json)
        {
            return JsonConvert.DeserializeObject(json, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore }) as Preset;
        }

        public void ResetValues(ModuleBlockMover block)
        {
            foreach (Variable v in Variables)
            {
                v.ResetValue(block);
            }
            if (ExpandLimitsToBlock)
            {
                minValueLimit = 0f;
                maxValueLimit = block.TrueLimitVALUE;
            }
            if (ExpandVelocityToBlock)
            {
                maxVelocity = block.TrueMaxVELOCITY;
            }
        }

        public string SetToBlockMover(ModuleBlockMover blockMover)
        {
            blockMover.SetDirty();
            blockMover.SetMinLimit(minValueLimit ?? 0f);
            blockMover.SetMaxLimit(maxValueLimit ?? blockMover.TrueLimitVALUE);
            blockMover.MAXVELOCITY = maxVelocity.HasValue ? velocity.Value : blockMover.MAXVELOCITY;
            if (targetValue.HasValue) blockMover.VALUE = targetValue.Value;
            if (velocity.HasValue) blockMover.VELOCITY = velocity.Value;
            if (jointStrength.HasValue) blockMover.SPRSTR = jointStrength.Value;
            if (jointDampen.HasValue) blockMover.SPRDAM = jointDampen.Value;
            if (moverType.HasValue) blockMover.moverType = moverType.Value;
            blockMover.CannotBeFreeJoint = blockMover.CannotBeFreeJoint;
            if (lockOffsetParent.HasValue) blockMover.LockJointBackPush = lockOffsetParent.Value;
            return InputOperator.StringArrayToProcessOperations(ToString(), ref blockMover.ProcessOperations);
        }

        public abstract class Variable
        {
            public string VarToReplace;
            public string ValueName;
            public string Description;
            public bool VisibleInGUI = true;

            public abstract void ResetValue(ModuleBlockMover block);
            public abstract void DrawGUI(ref int index);
            [JsonIgnore] public abstract string Value { get; }
        }
        public class KeyCodeV : Variable
        {
            public KeyCode DefaultValue;

            [JsonIgnore]
            public KeyCode value;

            public override void ResetValue(ModuleBlockMover block)
            {
                value = DefaultValue;
            }
            public override void DrawGUI(ref int index)
            {
                GUILayout.BeginVertical(ValueName, GUI.skin.window);
                {
                    GUILayout.Label(Description);
                    if (GUILayout.Button(PresetsIMGUI.IsSettingKeybind && PresetsIMGUI.IndexOfKeybinder == index ? "Press a key" : value.ToString()))
                    {
                        if (PresetsIMGUI.IsSettingKeybind)
                        {
                            if (PresetsIMGUI.IndexOfKeybinder == index)
                                PresetsIMGUI.IsSettingKeybind = false;
                            else
                                PresetsIMGUI.IndexOfKeybinder = index;
                        }
                        else
                        {
                            PresetsIMGUI.IndexOfKeybinder = index;
                            PresetsIMGUI.IsSettingKeybind = true;
                        }
                    }
                    if (PresetsIMGUI.IndexOfKeybinder == index && !PresetsIMGUI.IsSettingKeybind)
                    {
                        PresetsIMGUI.IndexOfKeybinder = -1;
                        value = PresetsIMGUI.SetKey;
                    }
                }
                GUILayout.EndVertical();
            }
            public override string Value => value.ToString();
        }

        public class FloatV : Variable
        {
            public float DefaultValue;
            public float MinValue;
            public float MaxValue;
            public bool LimitsAreValue;
            public bool LimitsAreVelocity;
            public bool RestrictValue;

            [JsonIgnore]
            public float value;
            [JsonIgnore]
            string cache;
            public override void ResetValue(ModuleBlockMover block)
            {
                value = DefaultValue;
                cache = null;

                if (LimitsAreValue)
                {
                    MinValue = 0f;
                    MaxValue = block.TrueLimitVALUE;
                }
                if (LimitsAreVelocity)
                {
                    MinValue = -block.TrueMaxVELOCITY;
                    MaxValue = block.TrueMaxVELOCITY;
                }
                value = Mathf.Clamp(value, MinValue, MaxValue);
            }
            public override void DrawGUI(ref int index)
            {
                GUILayout.BeginVertical(ValueName, GUI.skin.window);
                {
                    GUILayout.Label(Description);
                    GUIOverseer.TextSliderPair(ValueName + ": ", ref cache, ref value, MinValue, MaxValue, RestrictValue);
                }
                GUILayout.EndVertical();
            }
            public override string Value => value.ToString();
        }

        public class StringConditionV : Variable
        {
            public bool DefaultValue;
            public string ReplacementIfTrue;
            public string ReplacementIfFalse;
            public int KeysToHideIfTrue;
            public int KeysToHideIfFalse;
            [JsonIgnore]
            public bool value;

            public override void ResetValue(ModuleBlockMover block)
            {
                value = DefaultValue;
            }
            public override void DrawGUI(ref int index)
            {
                GUILayout.BeginVertical(ValueName, GUI.skin.window);
                {
                    GUILayout.Label(Description);
                    if (GUILayout.Toggle(value, ValueName) != value)
                    {
                        value = !value;
                    }
                }
                GUILayout.EndVertical();
                if (value) index += KeysToHideIfTrue;
                else index += KeysToHideIfFalse;
            }
            public override string Value => value ? ReplacementIfTrue : ReplacementIfFalse;
        }
    }
}
