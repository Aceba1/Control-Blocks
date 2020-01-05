using System;
using System.Collections.Generic;
using UnityEngine;

namespace Control_Block
{
    public class InputOperator // Stackable list of operations to peform
    {
        public struct UIDispOperation
        {
            public bool LockInputTypes;
            public InputType PermittedInputType;
            public bool HideStrength;
            public bool StrengthIsToggle;
            public bool ClampStrength;
            public string UIName, UIDesc;
            public bool SliderMin;
            public float SliderMax;
            public bool SliderMaxIsMaxVel;
            public float SliderPosFraction;
            public string ToggleComment;
            public float ToggleMultiplier;

            public UIDispOperation(string UIname, string UIdesc, bool lockInputTypes = false, InputType permittedInputType = InputType.AlwaysOn, bool hideStrength = false, bool strengthIsToggle = false, bool clampStrength = false, float sliderFraction = 0f, bool sliderHasNegative = false, float sliderMax = 0f, string toggleComment = "Invert", float toggleMultiplier = 1f, bool sliderMaxIsMaxVel = false)
            {
                LockInputTypes = lockInputTypes;
                PermittedInputType = permittedInputType;
                HideStrength = hideStrength;
                StrengthIsToggle = strengthIsToggle;
                UIName = UIname;
                UIDesc = UIdesc;
                ClampStrength = clampStrength;
                SliderPosFraction = sliderFraction;
                SliderMax = sliderMax;
                SliderMaxIsMaxVel = sliderMaxIsMaxVel;
                SliderMin = sliderHasNegative;
                ToggleComment = toggleComment;
                ToggleMultiplier = toggleMultiplier;
            }
        }
        public struct UIDispInput
        {
            public bool HideInputKey;
            public bool HideParam;
            public bool ParamIsToggle;
            public bool ParamIsTrueValue, SliderMaxIsMaxVal, SliderMaxIsMaxVel;
            public string UIName;
            public float SliderMax;
            public string ToggleComment;
            public float ToggleMultiplier;

            public UIDispInput(string UIname, bool hideInputKey = false, bool hideParam = false, bool paramIsToggle = false, bool paramIsTrueValue = false, float sliderMax = 0f, string toggleComment = "Invert", float toggleMultiplier = 1f, bool sliderMaxIsMaxVal = false, bool sliderMaxIsMaxVel = false)
            {
                HideInputKey = hideInputKey;
                HideParam = hideParam;
                ParamIsToggle = paramIsToggle;
                UIName = UIname;
                ParamIsTrueValue = paramIsTrueValue;
                SliderMax = sliderMax;
                ToggleComment = toggleComment;
                ToggleMultiplier = toggleMultiplier;
                SliderMaxIsMaxVal = sliderMaxIsMaxVal;
                SliderMaxIsMaxVel = sliderMaxIsMaxVel;
            }
        }

        public static Dictionary<OperationType, UIDispOperation> UIOperationPairs = new Dictionary<OperationType, UIDispOperation>
        {
            {OperationType.ShiftPos, new UIDispOperation("Shift Position", "Move the position or angle by Strength", sliderMaxIsMaxVel:true, sliderHasNegative:true) },
            {OperationType.SetPos, new UIDispOperation("Set Position", "Set the position or angle to Strength", sliderFraction:1f) },
            {OperationType.ShiftSpeed, new UIDispOperation("Shift Speed", "Accelerate the velocity by Strength", sliderMaxIsMaxVel:true, sliderHasNegative:true) },
            {OperationType.SetSpeed, new UIDispOperation("Set Speed", "Set the velocity to Strength", sliderMaxIsMaxVel:true, sliderHasNegative:true) },
            {OperationType.ArrowPoint, new UIDispOperation("Arrow Point", "Aim towards velocity, multiplied by Strength", clampStrength:true) },
            {OperationType.TargetPoint, new UIDispOperation("Target Point", "Aim towards the focused enemy, multiplied by Strength", clampStrength:true) },
            {OperationType.PlayerPoint, new UIDispOperation("Player Point", "Aim towards the player's tech, multiplied by Strength", clampStrength:true) },
            {OperationType.GravityPoint, new UIDispOperation("Gravity Point", "Aim towards the normal of gravity, multiplied by Strength", clampStrength:true) },
            {OperationType.FreeJoint, new UIDispOperation("Free-Joint", "Suspension state. Set loose kinematics on or off", strengthIsToggle:true, toggleComment:"Set Off") },
            {OperationType.LockJoint, new UIDispOperation("Lock-Joint", "Static state. Set ghost-phasing on or off", strengthIsToggle:true, toggleComment:"Set Off") },
            {OperationType.ConditionIfThen, new UIDispOperation("IF Condition", "Run everything up to EndIF, if Condition is met (for Strength amount of seconds)", sliderMax:5f, strengthIsToggle:true, toggleComment:"False after time") },
            {OperationType.ConditionEndIf, new UIDispOperation("End IF", "Close the IF Condition and proceed as normal", lockInputTypes:true, hideStrength:true) },
            {OperationType.CursorPoint, new UIDispOperation("Cursor Point", "Aim towards mouse end, multiplied by Strength", clampStrength:true) }
        };
        public static Dictionary<InputType, UIDispInput> UIInputPairs = new Dictionary<InputType, UIDispInput>
        {
            {InputType.AlwaysOn, new UIDispInput("Always On", hideInputKey:true, hideParam:true) },
            {InputType.OnPress, new UIDispInput("On Key Press", paramIsToggle:true) },
            {InputType.WhileHeld, new UIDispInput("On Key Hold", paramIsToggle:true) },
            {InputType.OnRelease, new UIDispInput("On Key Release", paramIsToggle:true) },
            {InputType.Toggle, new UIDispInput("Toggle Key", paramIsToggle:true, toggleComment:"State", toggleMultiplier:-1f) },
            {InputType.EnemyTechIsNear, new UIDispInput("Enemy is Near", hideInputKey:true, sliderMax:64) },
            {InputType.PlayerTechIsNear, new UIDispInput("Player is Near", hideInputKey:true, sliderMax:64) },
            {InputType.AboveSurfaceElev, new UIDispInput("Above Surface Elevation", hideInputKey:true, sliderMax:64) },
            {InputType.AboveVelocity, new UIDispInput("Above Velocity", hideInputKey:true, sliderMax:10) },

            {InputType.IfPosAbove, new UIDispInput("If Position Above", hideInputKey:true, sliderMaxIsMaxVal:true) },
            {InputType.IfPosBelow, new UIDispInput("If Position Below", hideInputKey:true, sliderMaxIsMaxVal:true) },
            {InputType.IfPosEqual, new UIDispInput("If Position Equal", hideInputKey:true, sliderMaxIsMaxVal:true) },
            {InputType.IfSpeedAbove, new UIDispInput("If Speed Above", hideInputKey:true, sliderMaxIsMaxVel:true) },
            {InputType.IfSpeedBelow, new UIDispInput("If Speed Below", hideInputKey:true, sliderMaxIsMaxVel:true) },
            {InputType.IfSpeedEqual, new UIDispInput("If Speed Equal", hideInputKey:true, sliderMaxIsMaxVel:true) },
            //{InputType.IfSprStrengthAbove, new UIDispInput("If Spring strength Above", hideInputKey:true, paramIsTrueValue:true) },
            //{InputType.IfSprStrengthBelow, new UIDispInput("If Spring strength Below", hideInputKey:true, paramIsTrueValue:true) },
            //{InputType.IfSprStrengthEqual, new UIDispInput("If Spring strength Equal", hideInputKey:true, paramIsTrueValue:true) },
            //{InputType.IfSprDampenAbove, new UIDispInput("If Spring dampen Above", hideInputKey:true, paramIsTrueValue:true) },
            //{InputType.IfSprDampenBelow, new UIDispInput("If Spring dampen Below", hideInputKey:true, paramIsTrueValue:true) },
            //{InputType.IfSprDampenEqual, new UIDispInput("If Spring dampen Equal", hideInputKey:true, paramIsTrueValue:true) },

        };
        public enum OperationType : byte
        {
            ShiftPos,
            SetPos,
            ShiftSpeed,
            SetSpeed,
            ArrowPoint,
            TargetPoint,
            PlayerPoint,
            GravityPoint,
            FreeJoint,
            ConditionIfThen,
            ConditionEndIf,
            CursorPoint,
            LockJoint
        }
        public enum InputType : byte
        {
            AlwaysOn,
            OnPress,
            OnRelease,
            WhileHeld,
            Toggle,
            EnemyTechIsNear,
            PlayerTechIsNear,
            AboveSurfaceElev,
            AboveVelocity,
            IfPosAbove,
            IfPosBelow,
            IfPosEqual,
            IfSpeedAbove,
            IfSpeedBelow,
            IfSpeedEqual
        }
        public OperationType m_OperationType = OperationType.SetPos;
        public InputType m_InputType = InputType.OnPress;
        public KeyCode m_InputKey = KeyCode.Space;

        public float m_InputParam; // Negative to invert condition
        public float m_Strength = 1;

        public bool LASTSTATE;

        private static float PointAtTarget(Transform trans, Vector3 localTarget, bool ProjectOnPlane, float Strength)
        {
            if (ProjectOnPlane)
            {
                return Vector3.SignedAngle(trans.forward, Vector3.ProjectOnPlane(localTarget, trans.up).normalized + (trans.forward * (1f - Strength)), trans.up);
            }
            return trans.InverseTransformDirection(localTarget).y * Strength;
        }

        float m_InternalTimer = 0f;


        /// <summary>
        /// Process this operation, checking if it's active and then modifying values based on its function
        /// </summary>
        /// <param name="block">The ModuleBlockMover to use for calculations</param>
        /// <param name="ProjectDirToPlane">Is the value used on a plane, or on an axis</param>
        /// <param name="Value">Positional value to modify</param>
        /// <param name="Velocity">Positional velocity to modify</param>
        /// <param name="FreeJoint">Allow free-moving in the attached body</param>
        /// <param name="LockJoint">Ghost-phasing</param>
        /// <returns>Returns true if satisfied</returns>
        public bool Calculate(TankBlock block, bool ProjectDirToPlane, ref float Value, ref float Velocity, ref bool FreeJoint, ref bool LockJoint, out int Skip)
        {
            Skip = 0;
            if (ConditionMatched(block, Value, Velocity))
            {
                switch (m_OperationType)
                {
                    case OperationType.ShiftPos:
                        Value += m_Strength;
                        return true;

                    case OperationType.SetPos:
                        Value = m_Strength;
                        return true;

                    case OperationType.ShiftSpeed:
                        Velocity += m_Strength;
                        return true;

                    case OperationType.SetSpeed:
                        Velocity = m_Strength;
                        return true;

                    case OperationType.ArrowPoint:
                        Value += PointAtTarget(block.trans, block.tank.rbody.GetPointVelocity(block.centreOfMassWorld) * Mathf.Sign(m_Strength), ProjectDirToPlane, Mathf.Abs(m_Strength)) - Value;
                        return true;

                    case OperationType.TargetPoint:
                        Visible target = block.tank.Weapons.GetManualTarget();
                        if (target == null)
                            return false;
                        Value += PointAtTarget(block.trans, (target.centrePosition - block.centreOfMassWorld) * Mathf.Sign(m_Strength), ProjectDirToPlane, Mathf.Abs(m_Strength)) - Value;
                        return true;

                    case OperationType.PlayerPoint:
                        Tank playerTank = Singleton.playerTank;
                        if (playerTank == null)
                            return false;
                        Value += PointAtTarget(block.trans, (playerTank.WorldCenterOfMass - block.centreOfMassWorld) * Mathf.Sign(m_Strength), ProjectDirToPlane, Mathf.Abs(m_Strength)) - Value;
                        return true;

                    case OperationType.GravityPoint:
                        Value += PointAtTarget(block.trans, Vector3.down * Mathf.Sign(m_Strength), ProjectDirToPlane, Mathf.Abs(m_Strength)) - Value;
                        return true;

                    case OperationType.FreeJoint:
                        FreeJoint = m_Strength >= 0;
                        return true;

                    case OperationType.LockJoint:
                        LockJoint = m_Strength >= 0;
                        return true;

                    case OperationType.ConditionIfThen:
                        if (m_Strength == 0) return true;
                        m_InternalTimer += Time.deltaTime;
                        bool met = (m_InternalTimer > m_Strength) != (m_Strength < 0);
                        // If time is satisfied and strength is positive, do not skip. Negative strength will only activate within that timeframe
                        Skip = met ? 0 : 1;
                        return met;

                    case OperationType.CursorPoint:
                        Value += PointAtTarget(block.trans, (AdjustAttachPosition.PointerPos - block.centreOfMassWorld) * Mathf.Sign(m_Strength), ProjectDirToPlane, Mathf.Abs(m_Strength)) - Value;
                        return true;

                    default:
                        return false;
                }
            }
            else if (m_OperationType == OperationType.ConditionIfThen)
            {
                m_InternalTimer = 0;
                Skip = 1;
            }
            return false;
        }

        int KeyState()
        {
            if (Input.GetKey(m_InputKey))
            {
                if (!KeyHold)
                {
                    KeyHold = true;
                    return 1;
                }
                return 2;
            }
            else
            {
                if (KeyHold)
                {
                    KeyHold = false;
                    return 3;
                }
                return 0;
            }
        }
        bool KeyHold = false;

        private bool ConditionMatched(TankBlock block, float m_Val, float m_Vel)
        {
            bool Invert = (m_InputParam < 0);
            switch (m_InputType)
            {
                case InputType.AlwaysOn:
                    return true;//Invert;

                case InputType.OnPress:
                    m_InputParam = Mathf.Sign(m_InputParam);
                    return (KeyState() == 1) != Invert;

                case InputType.OnRelease:
                    m_InputParam = Mathf.Sign(m_InputParam);
                    return (KeyState() == 3) != Invert;

                case InputType.WhileHeld:
                    m_InputParam = Mathf.Sign(m_InputParam);
                    return (KeyState() == 2) != Invert;

                case InputType.Toggle:
                    m_InputParam = Mathf.Sign(m_InputParam - 0.001f);
                    if (KeyState() == 1)
                    {
                        m_InputParam = -m_InputParam;
                    }
                    return m_InputParam > 0;

                case InputType.EnemyTechIsNear:
                    Visible target = block.tank.Weapons.GetManualTarget();
                    if (target == null) return Invert;
                    return ((target.centrePosition - block.centreOfMassWorld).sqrMagnitude < m_InputParam * m_InputParam) != Invert;

                case InputType.PlayerTechIsNear:
                    if (Singleton.playerTank == null) return Invert;
                    if (Singleton.playerTank == block.tank) return !Invert;
                    return ((Singleton.playerTank.visible.centrePosition - block.centreOfMassWorld).sqrMagnitude < m_InputParam * m_InputParam) != Invert;

                case InputType.AboveSurfaceElev:
                    var comw = block.centreOfMassWorld;
                    ManWorld.inst.GetTerrainHeight(comw, out float outHeight);
                    return (comw.y > outHeight + m_InputParam) != Invert;

                case InputType.AboveVelocity:
                    return (block.tank.rbody.GetPointVelocity(block.centreOfMassWorld).sqrMagnitude > m_InputParam * m_InputParam) != Invert;

                case InputType.IfPosAbove:
                    return m_Val > m_InputParam;
                case InputType.IfPosBelow:
                    return m_Val < m_InputParam;
                case InputType.IfPosEqual:
                    return m_Val.Approximately(m_InputParam);

                case InputType.IfSpeedAbove:
                    return m_Vel > m_InputParam;
                case InputType.IfSpeedBelow:
                    return m_Vel < m_InputParam;
                case InputType.IfSpeedEqual:
                    return m_Vel.Approximately(m_InputParam);

                default:
                    return false;
            }
        }

        public override string ToString()
        {
            var oT = UIOperationPairs[m_OperationType];
            var iT = UIInputPairs[m_InputType];
            string action = $"DO {m_OperationType.ToString()} ( {(oT.HideStrength ? "" : m_Strength.ToString())} )";
            string condition = (m_InputType != InputType.AlwaysOn ? $"{m_InputType.ToString()} ( " +
                (iT.HideInputKey ? "" : m_InputKey.ToString()) + (!iT.HideInputKey && !iT.HideParam ? ", " : "") + // Hide comma if only one
                $"{(iT.HideParam ? "" : " " + m_InputParam.ToString())} ) " : ""); // If AlwaysOn, it will not have
            switch (m_OperationType)
            {
                case OperationType.ConditionIfThen:
                    return $"IF ( {condition}, {m_Strength} ) THEN";
                case OperationType.ConditionEndIf:
                    return "ENDIF";
                default:
                    return condition + action;
            }
        }
        public static InputOperator FromString(string source)
        {
            var result = new InputOperator();
            source = source.ToLower().Replace("do ", "#do#").Replace(" ", "").Replace("\t", ""); // Highlight DO separator and remove spacing
            if (source == "endif") // ENDIF statement
            {
                result.m_InputType = InputType.AlwaysOn;
                result.m_OperationType = OperationType.ConditionEndIf;
            }
            else if (source.StartsWith("if(")) // IF statement
            {
                result.m_OperationType = OperationType.ConditionIfThen;
                source = source.Substring(3, source.Length - 8); //remove ') THEN', compensate for IF (
                string strength = source.Substring(0, source.LastIndexOf(','));
                ParseInputFromString(result, strength);
                if (!float.TryParse(source.Substring(strength.Length + 1), out float fstrength)) throw new FormatException("Cannot parse parameter '" + strength + "'");
                result.m_Strength = fstrength;
            }
            else
            {
                int dosep = source.IndexOf("#do#");
                if (dosep == 0) // No condition defined, always on
                {
                    result.m_InputType = InputType.AlwaysOn;
                }
                else
                {
                    ParseInputFromString(result, source.Substring(0, dosep)); // Get input type and parameters
                }
                ParseOperationFromString(result, source.Substring(dosep + 4)); // Cut down to action
            }
            return result;
        }

        static void ParseInputFromString(InputOperator obj, string source)
        {
            int pindex = source.IndexOf('(');
            string key = source.Substring(0, pindex);
            if (!Enum.TryParse(key, true, out InputType inputType)) throw new ArgumentException("Condition '" + key + "' does not exist in InputType");
            obj.m_InputType = inputType;
            var parameters = source.Substring(pindex + 1).TrimEnd(')').Split(',');
            pindex = 0; // Repurpose as parameter
            var iT = UIInputPairs[inputType];
            if (!iT.HideInputKey)
            {
                if (parameters.Length <= pindex || string.IsNullOrEmpty(parameters[0])) throw new FormatException("Expected Key parameter");
                if (!Enum.TryParse(parameters[0], true, out KeyCode keyCode)) throw new ArgumentException("Key Parameter '" + parameters[0] + "' does not exist in KeyCode");
                obj.m_InputKey = keyCode;
                pindex++;
            }
            if (!iT.HideParam)
            {
                if (parameters.Length <= pindex || string.IsNullOrEmpty(parameters[pindex])) throw new FormatException("Expected Value parameter");
                if (!float.TryParse(parameters[pindex], out float param)) throw new FormatException("Cannot parse parameter '" + parameters[pindex] + "'");
                obj.m_InputParam = param;
            }
        }

        static void ParseOperationFromString(InputOperator obj, string source)
        {
            int pindex = source.IndexOf('(');
            string key = source.Substring(0, pindex);
            if (!Enum.TryParse(key, true, out OperationType operationType)) throw new ArgumentException("Operation '" + key + "' does not exist in OperationType");
            obj.m_OperationType = operationType;
            source = source.Substring(pindex + 1).TrimEnd(')');
            if (source.Length != 0)
            {
                if (!float.TryParse(source, out float param)) throw new FormatException("Cannot parse parameter '" + source + "'");
                obj.m_Strength = param;
            }
        }

        public static string[] ProcessOperationsToStringArray(List<InputOperator> ProcessOperations)
        {
            var result = new string[ProcessOperations.Count];
            int c = 0;
            for (int i = 0; i < ProcessOperations.Count; i++)
            {
                var P = ProcessOperations[i];
                if (P.m_OperationType == InputOperator.OperationType.ConditionEndIf) c = Math.Max(c - 1, 0);
                result[i] = $"{"".PadRight(c * 4)}{P.ToString()}";
                if (P.m_OperationType == InputOperator.OperationType.ConditionIfThen) c++;
            }
            return result;
        }
        public static string StringArrayToProcessOperations(string systemCopyBuffer, ref List<InputOperator> ProcessOperations)
        {
            int count = 0;
            try
            {
                var list = new List<InputOperator>(); // Make a new one, for in case there is an error
                foreach (string s in systemCopyBuffer.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
                {
                    string source = s;
                    count++;
                    int comment = source.IndexOf('#');
                    if (comment != -1) source = source.Substring(comment);
                    if (string.IsNullOrWhiteSpace(source)) continue;
                    list.Add(InputOperator.FromString(source));
                }
                ProcessOperations.Clear(); // No error, clear and 
                ProcessOperations = list;
                return "Pasted";
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
                return "Line " + count.ToString() + ":\n" + E.Message;
            }
        }
    }
}
