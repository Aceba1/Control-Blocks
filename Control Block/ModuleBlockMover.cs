using Control_Block;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

internal class ModuleBlockMover : Module, TechAudio.IModuleAudioProvider
{
    public TechAudio.SFXType SFX;
    /// <summary>
    /// Body for holding blocks
    /// </summary>
    public ClusterBody Holder;
    /// <summary>
    /// Joint of which binds the two techs together
    /// </summary>
    public UnityEngine.ConfigurableJoint HolderJoint => Holder ? Holder.Joint : null;
    /// <summary>
    /// Preset number for quantity of models on block
    /// </summary>
    public int PartCount = 1;
    /// <summary>
    /// Models on block for animation purposes
    /// </summary>
    public Transform[] parts;
    /// <summary>
    /// The part who'se transform is responsible for the dummy tank's positioning
    /// </summary>
    public Transform HolderPart => parts[PartCount - 1];
    public Vector3 relativeCenter;
    /// <summary>
    /// WorldTreadmill interference security measure
    /// </summary>
    public bool Heart;
    /// <summary>
    /// Animation curves for determining position
    /// </summary>
    public AnimationCurve[] posCurves;
    public bool useRotCurves = false, usePosCurves = false;
    /// <summary>
    /// Animation curves for determining rotation
    /// </summary>
    public AnimationCurve[] rotCurves;
    /// <summary>
    /// Relative location of where blocks should be if attached to the head of this block
    /// </summary>
    public IntVector3[] startblockpos;
    //public int MaximumBlockPush = 64;
    /// <summary>
    /// Trigger reevaluation of block
    /// </summary>
    public bool Dirty = true;
    public bool Valid = true;
    /// <summary>
    /// Block-cache for blocks found after re-evaluation
    /// </summary>
    public List<TankBlock> GrabbedBlocks;
    public List<TankBlock> StarterBlocks;
    public List<TankBlock> IgnoredBlocks;
    /// <summary>
    /// Event-cache for attaching and detaching to a tank
    /// </summary>
    public Action<TankBlock, Tank> tankAttachBlockAction, tankDetachBlockAction;
    public Action tankResetPhysicsAction;

    internal float MINVALUELIMIT = 0f, MAXVALUELIMIT = 1f;
    public float MaxVELOCITY = 1f, TrueLimitVALUE = 1f;
    public void SetMinValueLimit(float value)
    {
        if (DEACTIVATEMOTOR && HolderJoint != null)
        {
            if (IsPlanarVALUE)
            {
                var modify = HolderJoint.lowAngularXLimit;
                modify.limit = value;
                HolderJoint.lowAngularXLimit = modify;
            }
            else
            {
                SetLinearLimit();
            }
        }
        MINVALUELIMIT = value;
    }
    public void SetMaxValueLimit(float value)
    {
        if (DEACTIVATEMOTOR && HolderJoint != null)
        {
            if (IsPlanarVALUE)
            {
                var modify = HolderJoint.highAngularXLimit;
                modify.limit = value;
                HolderJoint.highAngularXLimit = modify;
            }
            else
            {
                SetLinearLimit();
            }
        }
        MAXVALUELIMIT = value;
    }
    public void SetLinearLimit()
    {
        var min = GetPosCurve(PartCount - 1, MINVALUELIMIT);
        var max = GetPosCurve(PartCount - 1, MAXVALUELIMIT);
        HolderJoint.anchor = transform.parent.InverseTransformPoint(transform.TransformPoint((max + min) * 0.5f));
        var ll = HolderJoint.linearLimit;
        ll.limit = Mathf.Abs((max - min).y) * 0.5f;
        HolderJoint.linearLimit = ll;
    }

    /// <summary>
    /// Target value
    /// </summary>
    public float VALUE;
    /// <summary>
    /// Spring Strength
    /// </summary>
    public float SPRSTR;
    /// <summary>
    /// Spring Dampening
    /// </summary>
    public float SPRDAM;
    public void UpdateSpringForce()
    {
        if (HolderJoint != null)
        {
            HolderJoint.angularXDrive = new JointDrive { positionDamper = SPRDAM, positionSpring = SPRSTR, maximumForce = ClusterBody.MaxSpringForce };
            HolderJoint.xDrive = new JointDrive { positionDamper = SPRDAM, positionSpring = SPRSTR, maximumForce = ClusterBody.MaxSpringForce };
        }
    }
    /// <summary>
    /// Current value
    /// </summary>
    public float PVALUE;
    public float VELOCITY;
    public bool IsPlanarVALUE, DEACTIVATEMOTOR, oldDEACTIVATE;

    public List<InputOperator> ProcessOperations;

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
            {OperationType.DeactivateMotor, new UIDispOperation("Deactivate Motor", "Free the joint. Kill or reactivate the motor", strengthIsToggle:true, toggleComment:"Reactivate") },
            {OperationType.ConditionIfThen, new UIDispOperation("IF Condition", "Run everything afterwards, if Condition is met (for Strength amount of seconds)", sliderMax:5f, strengthIsToggle:true, toggleComment:"False after time") },
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
            DeactivateMotor,
            ConditionIfThen,
            ConditionEndIf,
            CursorPoint
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
                return Vector3.SignedAngle(trans.forward, Vector3.ProjectOnPlane(localTarget, trans.up) + (trans.forward * (1f - Strength)), trans.up);
            }
            return trans.InverseTransformDirection(localTarget).y * Strength;
        }

        float m_InternalTimer = 0f;
        float m_Vel, m_Val;
        

        /// <summary>
        /// Process this operation, checking if it's active and then modifying values based on its function
        /// </summary>
        /// <param name="block">The ModuleBlockMover to use for calculations</param>
        /// <param name="ProjectDirToPlane">Is the value used on a plane, or on an axis</param>
        /// <param name="Value">Positional value to modify</param>
        /// <param name="Velocity">Positional velocity to modify</param>
        /// <param name="DeactivateMotor">Whether or not to allow free-moving in the attached body</param>
        /// <returns>Returns true if satisfied</returns>
        public bool Calculate(TankBlock block, bool ProjectDirToPlane, ref float Value, ref float Velocity, ref bool DeactivateMotor, out int Skip)
        {
            m_Val = Value; m_Vel = Velocity;
            Skip = 0;
            if (ConditionMatched(block))
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

                    case OperationType.DeactivateMotor:
                        DeactivateMotor = m_Strength >= 0;
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

        private bool ConditionMatched(TankBlock block)
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
                string strength = source.Substring(0, source.IndexOf(','));
                ParseInputFromString(result, strength);
                if (!float.TryParse(source.Substring(strength.Length), out float fstrength)) throw new FormatException("Cannot parse parameter '" + strength + "'");
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
    }

    internal string[] ProcessOperationsToStringArray()
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
    internal string StringArrayToProcessOperations(string systemCopyBuffer)
    {
        int count = 0;
        try
        {
            var list = new List<InputOperator>();
            foreach (string s in systemCopyBuffer.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
            {
                string source = s;
                count++;
                int comment = source.IndexOf('#');
                if (comment != -1) source = source.Substring(comment);
                if (string.IsNullOrWhiteSpace(source)) continue;
                list.Add(InputOperator.FromString(source));
            }
            ProcessOperations.Clear();
            ProcessOperations = list;
            return "Pasted";
        }
        catch(Exception E)
        {
            Console.WriteLine(E);
            return "Line "+count.ToString()+":\n" + E.Message;
        }
    }

    public TechAudio.SFXType SFXType
    {
        get
        {
            return this.SFX;
        }
    }

    public event Action<TechAudio.AudioTickData, FMODEvent.FMODParams> OnAudioTickUpdate;

    public float SFXVolume = 1f;
    public string SFXParam = "Rate";

    public void UpdateSFX(float Speed)
    {
        bool on = !(Speed * SFXVolume).Approximately(0f, 0.1f);
        PlaySFX(on, Mathf.Abs(Speed) * SFXVolume);
    }

    internal void PlaySFX(bool On, float Speed)
    {
        if (this.OnAudioTickUpdate != null)
        {
            TechAudio.AudioTickData value = new TechAudio.AudioTickData
            {
                module = this,
                provider = this,
                sfxType = SFX,
                numTriggered = (On ? 1 : 0),
                triggerCooldown = 0f,
                isNoteOn = On,
                adsrTime01 = 0f,
            };
            this.OnAudioTickUpdate(value, On ? new FMODEvent.FMODParams(SFXParam, Speed) : null);
        }
    }

    public Quaternion GetRotCurve(int Index, float Position)
    {
        int Mod = Index * 3;
        return Quaternion.Euler(rotCurves[Mod].Evaluate(Position), rotCurves[Mod + 1].Evaluate(Position), rotCurves[Mod + 2].Evaluate(Position));
    }

    public Vector3 GetPosCurve(int Index, float Position)
    {
        int Mod = Index * 3;
        return new Vector3(posCurves[Mod].Evaluate(Position), posCurves[Mod + 1].Evaluate(Position), posCurves[Mod + 2].Evaluate(Position));
    }

    private void OnPool() //Creation
    {
        GrabbedBlocks = new List<TankBlock>();
        StarterBlocks = new List<TankBlock>();
        IgnoredBlocks = new List<TankBlock>();
        base.block.serializeEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
        //base.block.serializeTextEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize)); // Uncertain about this bit, need to study code
        ProcessOperations = new List<InputOperator>();
        //HolderDirty = new List<TankBlock>();

        parts = new Transform[PartCount];
        int offset = block.transform.childCount - PartCount;
        for (int I = 0; I < PartCount; I++)
        {
            parts[I] = block.transform.GetChild(I + offset);
        }
        relativeCenter = parts[PartCount - 1].localPosition;

        tankAttachBlockAction = new Action<TankBlock, Tank>(this.BlockAdded);
        tankDetachBlockAction = new Action<TankBlock, Tank>(this.BlockRemoved);
        tankResetPhysicsAction = new Action(this.ResetPhysics);
        block.AttachEvent.Subscribe(Attach);
        block.DetachEvent.Subscribe(Detatch);
    }

    Vector3 cachePos, cacheLinVel, cacheAngVel;
    Quaternion cacheRot;
    bool Holding = false;
    private void CacheHolderTr()
    {
        Holding = true;
        cachePos = Holder.transform.position;
        cacheRot = Holder.transform.rotation;
        cacheLinVel = Holder.rbody.velocity;
        cacheAngVel = Holder.rbody.angularVelocity;
    }
    private void DefaultHolderTr(bool QueueRestore)
    {
        if (Holder != null)
        {
            queueRestoreHolderTr = QueueRestore;
            CacheHolderTr();
            //Holder.transform.position = transform.parent.position;
            //Holder.transform.rotation = transform.parent.rotation;
            Holder.transform.position = block.tank.trans.position;
            Holder.transform.rotation = block.tank.trans.rotation;
        }
    }

    private void DefaultPart()
    {
        //cachePos1 = HolderPart.localPosition;
        //cacheRot1 = HolderPart.localRotation;
        if (usePosCurves) HolderPart.localPosition = GetPosCurve(PartCount - 1, 0);
        if (useRotCurves) HolderPart.localRotation = GetRotCurve(PartCount - 1, 0);
    }

    //private void RestorePart()
    //{
    //    HolderPart.localPosition = cachePos1;
    //    HolderPart.localRotation = cacheRot1;
    //}

    private void RestoreHolderTr()
    {
        if (Holder != null)
        {
            //if (DEACTIVATEMOTOR)
            //{
            //    Holder.transform.position = cachePos;
            //    Holder.transform.rotation = cacheRot;
            //}
            //else
            //{
            if (IsPlanarVALUE)
                Holder.transform.rotation = transform.parent.rotation * Quaternion.Euler(transform.localRotation * Vector3.up * PVALUE);
            // This is not restricted to linear values because it also recenters swivels
            Holder.transform.position += HolderPart.position - Holder.transform.TransformPoint(block.cachedLocalPosition + block.cachedLocalRotation * relativeCenter);
            if (Holding)
            {
                Holder.rbody.velocity = cacheLinVel;
                Holder.rbody.angularVelocity = cacheAngVel;
                Holding = false;
            }
        }
    }

    private void CreateHolder()
    {
        if (Holder == null)
        {
            Holder = new GameObject("ClusterBody Holder").AddComponent<ClusterBody>();
            Holder.rbody = Holder.gameObject.AddComponent<Rigidbody>();
            Holder.rbody.detectCollisions = true;
            //Holder.dragSphere = Holder.gameObject.AddComponent<SphereCollider>();
            Holder.blocks = new List<TankBlock>();
            Holder.gameObject.layer = block.tank.gameObject.layer;
            Holder.transform.parent = transform;
        }
        Holder.transform.position = transform.parent.position;
        Holder.transform.rotation = transform.parent.rotation;
        Holder.coreTank = block.tank;
        //Holder.rbody.isKinematic = false;
        //ClusterTech.VerifyJoin(block.tank, Holder);
    }

    private void Update()
    {
        if (GrabbedBlocks.Count != 0) GrabbedBlocks.Clear();
    }

    private void LateUpdate()
    {
        if (Dirty)
        {
            CleanDirty();
        }
        if (Holder != null)
        {
            if (queueRestoreHolderTr)
            {
                queueRestoreHolderTr = false;
                UpdatePartTransforms();
                RestoreHolderTr();
            }
            if (DEACTIVATEMOTOR)
            {
                if (IsPlanarVALUE)
                {
                    PVALUE = Vector3.SignedAngle(transform.parent.forward, Holder.transform.forward, transform.up);
                }
                else
                {
                    PVALUE = Mathf.Clamp(Vector3.Project(Holder.transform.position - transform.parent.position, transform.up).magnitude, MINVALUELIMIT, MAXVALUELIMIT);
                }
                UpdatePartTransforms();
                //PVALUE = VALUE;
            }
        }
    }

    private void FixedUpdate()
    {
        if (Heart != Class1.PistonHeart)
        {
            Heart = Class1.PistonHeart;
            return;
        }
        try
        {
            if (block.tank == null)
            { 
                return;
            }
            if (!Valid) return;

            bool Net = ManGameMode.inst.IsCurrentModeMultiplayer();
            bool IsControlledByNet = Net && block.tank != ManNetwork.inst.MyPlayer.CurTech.tech;

            float oldVALUE = PVALUE, oldVELOCITY = VELOCITY;
            int SKIP = 0;
            if (!IsControlledByNet)
            {
                foreach (var Processor in ProcessOperations)
                {
                    if (SKIP != 0)
                    {
                        if (Processor.m_OperationType == InputOperator.OperationType.ConditionIfThen) SKIP++;
                        else if (Processor.m_OperationType == InputOperator.OperationType.ConditionEndIf) SKIP--;
                        Processor.LASTSTATE = SKIP == 0;
                        continue;
                    }
                    Processor.LASTSTATE = Processor.Calculate(block, IsPlanarVALUE, ref VALUE, ref VELOCITY, ref DEACTIVATEMOTOR, out SKIP);
                }
                VELOCITY = Mathf.Clamp(VELOCITY, -MaxVELOCITY, MaxVELOCITY);
                VALUE += VELOCITY;
                if (IsPlanarVALUE)
                {
                    VALUE = ((VALUE - oldVALUE + 540) % 360) - 180 + oldVALUE;
                }
                else
                {
                    VALUE = Mathf.Clamp(VALUE, 0, TrueLimitVALUE);
                }
                if (MINVALUELIMIT > 0 || MAXVALUELIMIT < TrueLimitVALUE)
                {
                    VALUE = Mathf.Clamp(VALUE, MINVALUELIMIT, MAXVALUELIMIT);
                }
                PVALUE = Mathf.Clamp(VALUE, oldVALUE - MaxVELOCITY, oldVALUE + MaxVELOCITY);
                float DIFF = PVALUE - oldVALUE;
                //if (Net && !LastSentVELOCITY.Approximately(DIFF, 0.005f))
                //{
                //    LastSentVELOCITY = DIFF;
                //    SendMoverChange(new BlockMoverMessage(block, PVALUE, DIFF));
                //}
            }
            else
            {
                VALUE += LastSentVELOCITY;
                PVALUE = VALUE;
            }

            if (IsPlanarVALUE)
            {
                //VALUE = Mathf.Repeat(VALUE, 360);
                PVALUE = Mathf.Repeat(PVALUE, 360);
            }

            if (!DEACTIVATEMOTOR)
                UpdatePartTransforms();

            if (Holder != null && HolderJoint != null)
            {
                if (DEACTIVATEMOTOR)
                {
                    if (!oldDEACTIVATE)
                    {
                        if (IsPlanarVALUE)
                        {
                            HolderJoint.angularXMotion = ConfigurableJointMotion.Free;
                            SetMinValueLimit(MINVALUELIMIT);
                            SetMaxValueLimit(MAXVALUELIMIT);
                        }
                        else
                        {
                            HolderJoint.xMotion = ConfigurableJointMotion.Limited;
                            //ClusterTech.SetOffset(block.tank, block.trans.up);
                            SetLinearLimit();
                        }
                        UpdateSpringForce();
                    }
                    if (IsPlanarVALUE)
                        HolderJoint.targetRotation = GetRotCurve(PartCount - 1, VALUE);
                    else
                        HolderJoint.targetPosition = GetPosCurve(PartCount - 1, VALUE);
                }
                else
                {
                    if (oldDEACTIVATE)
                    {
                        HolderJoint.xMotion = ConfigurableJointMotion.Locked;
                        HolderJoint.angularXMotion = ConfigurableJointMotion.Locked;
                        //VALUE = PVALUE;
                    }
                    if (IsPlanarVALUE)
                        UpdateRotateAnchor();
                    else
                        HolderJoint.anchor = transform.parent.InverseTransformPoint(HolderPart.position);
                }
            }
        }
        catch (Exception E)
        {
            Console.WriteLine(E);
            if (Holder == null)
                Console.WriteLine("Holder is NULL!");
            foreach (var descriptor in typeof(ModuleBlockMover).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(this);
                Console.WriteLine("{0}={1}", name, value);
            }
        }
        oldDEACTIVATE = DEACTIVATEMOTOR;
    }

    void UpdatePartTransforms()
    {
        for (int i = 0; i < parts.Length; i++)
        {
            if (usePosCurves)
                parts[i].localPosition = GetPosCurve(i, PVALUE);
            if (useRotCurves)
                parts[i].localRotation = GetRotCurve(i, PVALUE);
        }
    }

    void UpdateRotateAnchor()
    {
        var rot = Holder.transform.rotation;

        Holder.transform.rotation = transform.parent.rotation * Quaternion.Euler(transform.localRotation * Vector3.up * PVALUE);

        HolderJoint.axis = transform.localRotation * Vector3.up;
        HolderJoint.secondaryAxis = transform.localRotation * Vector3.forward;
        Holder.transform.rotation = rot;
        //var pos = ((rotCurves[PartCount * 3 - 2].Evaluate(PVALUE) + 180) % 360) - 180;
        //var ll = HolderJoint.lowAngularXLimit;
        //ll.limit = Mathf.Max(pos - 0.00002f, -180f);
        //HolderJoint.lowAngularXLimit = ll;
        //var hl = HolderJoint.highAngularXLimit;
        //hl.limit = Mathf.Min(pos + 0.00002f, 180f);
        //HolderJoint.highAngularXLimit = hl;
    }

    bool queueRestoreHolderTr;

    internal void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
    {
       if (saving)
       {
            if (PVALUE != 0f)
            {
                DefaultHolderTr(true);
                Print("Serializing");
            }

            SerialData serialData = new SerialData()
            {
                minValueLimit = MINVALUELIMIT,
                maxValueLimit = MAXVALUELIMIT,
                currentValue = PVALUE,
                targetValue = VALUE,
                velocity = VELOCITY,
                jointStrength = SPRSTR,
                jointDampen = SPRDAM,
                freeJoint = DEACTIVATEMOTOR,
                processList = string.Join("\n", ProcessOperationsToStringArray())
           };
           serialData.Store(blockSpec.saveState);
       }
       else
       {
           SerialData sd = SerialData<ModuleBlockMover.SerialData>.Retrieve(blockSpec.saveState);
           if (sd != null)
           {
                SetDirty();
                MINVALUELIMIT = sd.minValueLimit;
                MAXVALUELIMIT = sd.maxValueLimit;
                PVALUE = sd.currentValue;
                VALUE = sd.targetValue;
                VELOCITY = sd.velocity;
                SPRSTR = sd.jointStrength;
                SPRDAM = sd.jointDampen;
                DEACTIVATEMOTOR = sd.freeJoint;
                StringArrayToProcessOperations(sd.processList);
           }
       }
    }


    [Serializable]
        public class SerialData : Module.SerialData<ModuleBlockMover.SerialData>
        {
            public float minValueLimit, maxValueLimit, currentValue, targetValue, velocity, jointStrength, jointDampen;
            public bool freeJoint;
            public string processList;
        }

    private void OnSpawn() //Pull from Object Pool
    {
        ProcessOperations.Clear();
        if (IsPlanarVALUE)
        {
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.RightArrow, m_InputType = InputOperator.InputType.WhileHeld, m_InputParam = 0, m_OperationType = InputOperator.OperationType.ShiftPos, m_Strength = 1 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.LeftArrow, m_InputType = InputOperator.InputType.WhileHeld, m_InputParam = 0, m_OperationType = InputOperator.OperationType.ShiftPos, m_Strength = -1 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.I, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = 0 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.J, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = 270 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.K, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = 180 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.L, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = 90 });

            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.T, m_InputType = InputOperator.InputType.Toggle, m_InputParam = -1, m_OperationType = InputOperator.OperationType.ConditionIfThen, m_Strength = 3 });
            ProcessOperations.Add(new InputOperator() { m_InputType = InputOperator.InputType.PlayerTechIsNear, m_InputParam = -10, m_OperationType = InputOperator.OperationType.PlayerPoint, m_Strength = 1 });
            ProcessOperations.Add(new InputOperator() { m_InputType = InputOperator.InputType.PlayerTechIsNear, m_InputParam = 10, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = 0 });
            ProcessOperations.Add(new InputOperator() { m_InputType = InputOperator.InputType.AlwaysOn, m_OperationType = InputOperator.OperationType.ConditionEndIf });

            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.N, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.DeactivateMotor, m_Strength = 1 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.N, m_InputType = InputOperator.InputType.OnRelease, m_InputParam = 0, m_OperationType = InputOperator.OperationType.DeactivateMotor, m_Strength = -1 });
            TrueLimitVALUE = 360;
        }
        else
        {
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.Space, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = TrueLimitVALUE });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.Space, m_InputType = InputOperator.InputType.OnRelease, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = 0 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.UpArrow, m_InputType = InputOperator.InputType.WhileHeld, m_InputParam = 0, m_OperationType = InputOperator.OperationType.ShiftPos, m_Strength = 0.05f });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.DownArrow, m_InputType = InputOperator.InputType.WhileHeld, m_InputParam = 0, m_OperationType = InputOperator.OperationType.ShiftPos, m_Strength = -0.05f });

            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.N, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.DeactivateMotor, m_Strength = 1 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.N, m_InputType = InputOperator.InputType.OnRelease, m_InputParam = 0, m_OperationType = InputOperator.OperationType.DeactivateMotor, m_Strength = -1 });
        }
        MINVALUELIMIT = 0;
        MAXVALUELIMIT = TrueLimitVALUE;
        DEACTIVATEMOTOR = false;
        oldDEACTIVATE = false;
        SPRSTR = 0;
        SPRDAM = 0;
        VALUE = 0;
        PVALUE = 0;
        VELOCITY = 0;
        Heart = Control_Block.Class1.PistonHeart;
        Dirty = true;
        //restored = true;
    }

    private void OnRecycle() //Put back to Object Pool
    {

    }

    internal void Detatch()
    {
        //SFXIsOn = false;
        block.tank.AttachEvent.Unsubscribe(tankAttachBlockAction);
        block.tank.DetachEvent.Unsubscribe(tankDetachBlockAction);
        block.tank.ResetPhysicsEvent.Unsubscribe(tankResetPhysicsAction);
        block.tank.TechAudio.RemoveModule<ModuleBlockMover>(this);
        if (Holder != null)
        {
            Holder.coreTank = block.tank;
            Holder = Holder.Destroy();
        }
        PVALUE = 0f;
        UpdatePartTransforms();
        Valid = false;
    }

    internal void Attach()
    {
        block.tank.AttachEvent.Subscribe(tankAttachBlockAction);
        block.tank.DetachEvent.Subscribe(tankDetachBlockAction);
        block.tank.ResetPhysicsEvent.Subscribe(tankResetPhysicsAction);
        block.tank.TechAudio.AddModule<ModuleBlockMover>(this);
        SetDirty();
    }

    internal void BlockAdded(TankBlock mkblock, Tank tank)
    {
        // This may actually have no effect...
        //if (Valid && Holder != null)
        //{
        //    DefaultHolderTr(true); //Default position so no AP problems occur
        //}
        if (!Dirty)
            Print("Block added, set dirty " + block.cachedLocalPosition.ToString());
        SetDirty(); //ResetPhysics may already be called
    }

    internal void BlockRemoved(TankBlock rmblock, Tank tank)
    {
        if (block.tank != null && block.tank.blockman != null) Valid &= CanStartGetBlocks(block.tank.blockman);
        if (!Valid || (Holder != null && Holder.TryRemoveBlock(rmblock)))
        {
            if (!Dirty)
                Print("Block removed from blockmover, set dirty " + block.cachedLocalPosition.ToString());
            SetDirty(); //ResetPhysics may already be called
        }
    }

    internal void ResetPhysics()
    {
        if (Holder != null)
        {
            Holder.coreTank = block.tank;
            Holder.Dirty = true;
            DefaultHolderTr(false);
            Print("ResetPhysics called, cleaning " + block.cachedLocalPosition.ToString());
            //SetDirty();
            DefaultPart();
            Holder.ResetPhysics(this);

            UpdatePartTransforms();
            RestoreHolderTr();
            queueRestoreHolderTr = false;

            UpdateSpringForce();
            oldDEACTIVATE = false;
            if (!DEACTIVATEMOTOR)
            {
                if (IsPlanarVALUE)
                {
                    UpdateRotateAnchor();
                }
                else
                {
                    HolderJoint.anchor = transform.parent.InverseTransformPoint(HolderPart.position);
                }
            }
            //else
            //{
            //    if (IsPlanarVALUE)
            //    {
            //        SetMinValueLimit(MINVALUELIMIT);
            //        SetMaxValueLimit(MAXVALUELIMIT);
            //    }
            //    else
            //    {
            //        SetLinearLimit();
            //    }
            //}
        }
    }
    private string lastdatetime = "";

    private string GetDateTime(string Before, string After)
    {
        string newdatetime = DateTime.Now.ToString("T", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
        if (newdatetime != lastdatetime)
        {
            lastdatetime = newdatetime;
            return Before + lastdatetime + After;
        }
        return "";
    }

    public void Print(string Message)
    {
        Console.WriteLine(GetDateTime("CB(", "): ") + Message);
    }

    internal void CleanDirty()
    {
        if (!Dirty || !block.IsAttached || block.tank == null)
        {
            Dirty = false;
            return;
        }
        Dirty = false;
        Print("Reached CleanDirty " + block.cachedLocalPosition.ToString());

        if (GrabbedBlocks.Count == 0)
        {
            Valid = StartGetBlocks();
        }
        if (!Valid)
        {
            Print("Invalid");
            Invalidate();
            //queueRestoreHolderTr = false;
        }
        else
        {
            if (GrabbedBlocks.Count == 0)
            {
                if (Holder != null)
                {
                    Holder = Holder.Destroy();
                    Print("Purged holder, there were no blocks");
                }
            }
            else
            {
                bool MakeNew = Holder == null, Refill = !MakeNew && (Holder.Dirty || GrabbedBlocks.Count != Holder.blocks.Count);
                if (Refill)
                {
                    Print("Clearing holder's blocks: " + (Holder.Dirty ? "mover was marked changed" : $"grabbed {GrabbedBlocks.Count} blocks, but holder had {Holder.blocks.Count}"));
                    Holder.Clear();
                    //Holder = Holder.Destroy();
                }
                DefaultPart();
                CreateHolder();
                if (MakeNew || Refill)
                {
                    for (int i = 0; i < GrabbedBlocks.Count; i++)
                    {
                        var b = GrabbedBlocks[i];
                        Holder.AddBlock(b, b.cachedLocalPosition, b.cachedLocalRotation);
                    }
                    Print($"Put {Holder.blocks.Count} blocks on holder");
                }
                else
                    Print($"Kept current {Holder.blocks.Count} blocks on holder");
                //Holder.ResetPhysics(this);

                UpdatePartTransforms();
                RestoreHolderTr();
                queueRestoreHolderTr = false;
            }
        }
    }

    internal void Invalidate()
    {
        if (Holder != null)
        {
            Holder = Holder.Destroy();
        }
        PVALUE = 0f;
        UpdatePartTransforms();
    }

    internal void SetDirty()
    {
        if (!Dirty)
        {
            //Print("Piston " + base.block.cachedLocalPosition.ToString() + " is now  d i r t y");
            Dirty = true;
        }
    }
    
    /// <summary>
    /// Use by derived classes to determine whether or not it is plausible to continue, or to do something before blocks are grabbed
    /// </summary>
    /// <returns>Continue?</returns>
    internal virtual bool CanStartGetBlocks(BlockManager blockMan) => true;

    /// <summary>
    /// Begin recursive grab of blocks connected to the block-mover head on the main tech
    /// </summary>
    /// <param name="WatchDog">List of parents, from oldest to newest, to watch out for in the case of an impossible structure</param>
    internal bool StartGetBlocks(List<ModuleBlockMover> WatchDog = null)
    {
        Print("Starting blockgrab for BlockMover " + block.cachedLocalPosition.ToString());

        StarterBlocks.Clear();
        IgnoredBlocks.Clear();

        var blockman = block.tank.blockman;

        if (!CanStartGetBlocks(blockman))
        {
            Print("Unique pre-blockgrab check failed!");
            return false;
        }

        GrabbedBlocks.Clear();

        foreach (IntVector3 sbp in startblockpos)
        {
            var Starter = blockman.GetBlockAtPosition((block.cachedLocalRotation * sbp) + block.cachedLocalPosition);
            if (Starter == null)
            {
                continue;
            }
            if (GrabbedBlocks.Contains(Starter) || IgnoredBlocks.Contains(Starter))
            {
                continue;
            }
            bool isAttached = false;
            foreach (var block in Starter.ConnectedBlocksByAP)
            {
                if (block != null && block == this.block)
                {
                    isAttached = true;
                    break;
                }
            }
            if (isAttached)
            {
                //Print("Starter block " + Starter.cachedLocalPosition.ToString());
                GrabbedBlocks.Add(Starter);
                StarterBlocks.Add(Starter);
                if (!CheckIfValid(Starter, false, WatchDog)) return false;
            }
        }
        foreach (var b in StarterBlocks)
        {
            if (!GetBlocks(b, true))
            {
                StarterBlocks.Clear();
                return false;
            }
        }
        //do the stuff here

        return true;
    }

    /// <summary>
    /// Get blocks connected directly to mover-block head on the main tech, recursively
    /// </summary>
    /// <param name="Start">Block to search from</param>
    /// <param name="IsStarter">For recursive use, determines if crossing mover-block is illegal after a step</param>
    /// <param name="WatchDog">List of parents, from oldest to newest, to watch out for in the case of an impossible structure</param>
    /// <returns>If false, the grab has failed and it is pulling back from the proccess</returns>
    internal bool GetBlocks(TankBlock Start = null, bool IsStarter = false, List<ModuleBlockMover> WatchDog = null)
    {
        foreach (TankBlock ConnectedBlock in Start.ConnectedBlocksByAP)
        {
            if (ConnectedBlock != null && !GrabbedBlocks.Contains(ConnectedBlock))
            {
                //Print("Block " + ConnectedBlock.cachedLocalPosition.ToString());
                if (IgnoredBlocks.Contains(ConnectedBlock))
                {
                    //Print("Ignoring block");
                    continue; // Skip ignored block
                }
                if (ConnectedBlock == block)
                {
                    if (IsStarter)
                    {
                        continue;
                    }
                    else
                    {
                        //Print("Looped to self! Escaping blockgrab");
                        return false;
                    }
                }
                if (!CheckIfValid(ConnectedBlock, IsStarter, WatchDog)) return false; // Check validity. If failed, cease
                GrabbedBlocks.Add(ConnectedBlock);
                if (!GetBlocks(ConnectedBlock, false, WatchDog)) return false; // Iterate down. If failed, cease
            }
        }
        return true;
    }

    bool CheckIfValid(TankBlock b, bool IsStarter, List<ModuleBlockMover> WatchDog)
    {
        ModuleBlockMover bm = b.GetComponent<ModuleBlockMover>();
        if (bm != null)
        {
            if (WatchDog != null && WatchDog.Contains(bm)) // If this block is actually a parent, take their knees and leave the scene
            {
                //Print("Parent encountered! Escaping blockgrab (Impossible structure)");
                for (int p = WatchDog.IndexOf(bm); p < WatchDog.Count; p++)
                    WatchDog[p].StarterBlocks.Clear();
                return false;
            }

            if (bm.Dirty) // If they didn't do their thing yet guide them to watch out for parents
            {
                //Print("Triggering new blockgrab for child");
                List<ModuleBlockMover> nWD = new List<ModuleBlockMover>();
                if (WatchDog != null) nWD.AddRange(WatchDog);
                nWD.Add(this);
                bm.Valid = bm.StartGetBlocks(nWD);
                nWD.Clear();
                if (StarterBlocks.Count == 0)
                {
                    //Print("Impossible structure! Escaping blockgrab");
                    return false; // They took our knees, also leave
                }
            }

            if (bm.Valid) // If that block did a good job leave its harvest alone
            {
                //Print("Child is valid, ignore blocks of");
                IgnoredBlocks.AddRange(bm.StarterBlocks);
            }
        }
        if (block.tank.blockman.IsRootBlock(b))
        {
            //Print("Encountered cab! Escaping blockgrab (false)");
            return false;
        }
        return true;
    }

    public const TTMsgType NetMsgMoverID = (TTMsgType)32115;
    internal static bool IsNetworkingInitiated = false;

#warning Disable "free-joint" when networked?
    public static void InitiateNetworking()
    {
        if (IsNetworkingInitiated)
        {
            throw new Exception("Something tried to initiate the networking component of BlockMovers twice!\n" + System.Reflection.Assembly.GetCallingAssembly().FullName);
        }
        IsNetworkingInitiated = true;
        Nuterra.NetHandler.Subscribe<BlockMoverMessage>(NetMsgMoverID, ReceiveMoverChange, PromptNewMoverChange);
    }

    public static void SendMoverChange(BlockMoverMessage message)
    {
        if (ManNetwork.IsHost)
        {
            Nuterra.NetHandler.BroadcastMessageToAllExcept(NetMsgMoverID, message, true);
            return;
        }
        Nuterra.NetHandler.BroadcastMessageToServer(NetMsgMoverID, message);
    }

    private static void PromptNewMoverChange(BlockMoverMessage obj, NetworkMessage netmsg)
    {
        Nuterra.NetHandler.BroadcastMessageToAllExcept(NetMsgMoverID, obj, true, netmsg.conn.connectionId);
        ReceiveMoverChange(obj, netmsg);
    }

    private static void ReceiveMoverChange(BlockMoverMessage obj, NetworkMessage netmsg) => obj.block.GetComponent<ModuleBlockMover>().ReceiveFromNet(obj);

    float LastSentVELOCITY = 0f;

    public void ReceiveFromNet(BlockMoverMessage data)
    {
        VALUE = data.value;
        LastSentVELOCITY = data.velocity;
        //Console.WriteLine($"Received new blockmover change: {block.cachedLocalPosition} set to {VALUE} with velocity {LastSentVELOCITY}");
    }

    public class BlockMoverMessage : UnityEngine.Networking.MessageBase
    {
        public BlockMoverMessage()
        {
        }

        public BlockMoverMessage(TankBlock Block, float Value, float Velocity)
        {
            block = Block;
            tank = Block.tank;
            value = Value;
            velocity = Velocity;
        }

        public override void Deserialize(UnityEngine.Networking.NetworkReader reader)
        {
            tank = ClientScene.FindLocalObject(new NetworkInstanceId(reader.ReadUInt32())).GetComponent<Tank>();
            block = tank.blockman.GetBlockWithID(reader.ReadUInt32());
            value = reader.ReadSingle();
            velocity = reader.ReadSingle();
        }

        public override void Serialize(UnityEngine.Networking.NetworkWriter writer)
        {
            writer.Write(tank.netTech.netId.Value);
            writer.Write(block.blockPoolID);
            writer.Write(value);
            writer.Write(velocity);
        }

        public TankBlock block;
        public Tank tank;

        public float value, velocity;
    }
}