using System;
using System.Collections.Generic;
using System.Text;
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
            public bool SliderHasNegative;
            public float SliderMax;
            public bool SliderMaxIsMaxVel;
            public float SliderPosFraction;
            public bool SliderMinOnPlanar;
            public string ToggleComment;
            public float ToggleMultiplier;
            public float DefaultValue;
            public string DummyNegative;
            //public float SliderStep;

            public UIDispOperation(string UIname, string UIdesc, bool lockInputTypes = false, InputType permittedInputType = InputType.AlwaysOn, bool hideStrength = false, bool strengthIsToggle = false, bool clampStrength = false, float sliderFraction = 0f, bool sliderHasNegative = false, float sliderMax = 0f, string toggleComment = "Invert", float toggleMultiplier = 1f, bool sliderMaxIsMaxVel = false, bool sliderMinOnPlanar = false, float defaultValue = 0f, string dummyNegative = null)
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
                SliderHasNegative = sliderHasNegative;
                ToggleComment = toggleComment;
                ToggleMultiplier = toggleMultiplier;
                //SliderStep = sliderStep;
                SliderMinOnPlanar = sliderMinOnPlanar;
                DefaultValue = defaultValue;
                DummyNegative = dummyNegative;
            }
        }
        public struct UIDispInput
        {
            public bool HideInputKey;
            public bool HideParam;
            public bool ParamIsToggle;
            public bool ParamIsTrueValue, SliderMaxIsMaxVal, SliderMaxIsMaxVel;
            public string UIName, UIDesc;
            public float SliderMax;
            public string ToggleComment;
            public float ToggleMultiplier;
            public float DefaultValue;
            public string DummyNegative;
            //public float SliderStep;

            public UIDispInput(string UIname, string UIdesc, bool hideInputKey = false, bool hideParam = false, bool paramIsToggle = false, bool paramIsTrueValue = false, float sliderMax = 0f, string toggleComment = "Invert", float toggleMultiplier = 1f, bool sliderMaxIsMaxVal = false, bool sliderMaxIsMaxVel = false, float defaultValue = 0f, string dummyNegative = null)
            {
                HideInputKey = hideInputKey;
                HideParam = hideParam;
                ParamIsToggle = paramIsToggle;
                UIName = UIname;
                UIDesc = UIdesc;
                ParamIsTrueValue = paramIsTrueValue;
                SliderMax = sliderMax;
                ToggleComment = toggleComment;
                ToggleMultiplier = toggleMultiplier;
                SliderMaxIsMaxVal = sliderMaxIsMaxVal;
                SliderMaxIsMaxVel = sliderMaxIsMaxVel;
                DefaultValue = defaultValue;
                DummyNegative = dummyNegative;
                //SliderStep = sliderStep;
            }
        }


        public static string[] OperationCategoryNames = new string[]
        {
            "Moving",
            "Pointing",
            //"Joint Type",
            "Conditions",
            "More"
        };
        public static List<OperationType[]> OperationCategoryLists = new List<OperationType[]>
        {
            new OperationType[] {
                OperationType.SetPos,
                OperationType.ShiftPos,
                OperationType.SetSpeed,
                OperationType.ShiftSpeed
            },
            new OperationType[] {
                OperationType.ArrowPoint,
                OperationType.TargetPoint,
                OperationType.TargetPointPredictive,
                OperationType.PlayerPoint,
                OperationType.CursorPoint,
                OperationType.CameraPoint,
                OperationType.GroundPoint,
                OperationType.NorthPoint
            },
            //new OperationType[] {
            //    OperationType.SetLockJoint,
            //    OperationType.SetBodyJoint,
            //    OperationType.SetFreeJoint
            //},
            new OperationType[] {
                OperationType.IfThen,
                OperationType.OrThen,
                OperationType.ElseThen,
                OperationType.EndIf
            },
            new OperationType[] {
                OperationType.Nothing,
                OperationType.FireWeapons
            }
        };

        public static string[] InputCategoryNames = new string[]
        {
            "Input",
            "Sensing",
            "Variables",
            "More"
        };
        public static List<InputType[]> InputCategoryLists = new List<InputType[]>
        {
            new InputType[] {
                InputType.OnPress,
                InputType.WhileHeld,
                InputType.OnRelease,
                InputType.Toggle,
            },
            new InputType[] {
                InputType.EnemyTechIsNear,
                InputType.PlayerTechIsNear,
                InputType.AboveSurfaceElev,
                InputType.AboveVelocity
            },
            new InputType[] {
                InputType.IfPosAbove,
                InputType.IfPosEqual,
                InputType.IfPosBelow,
                InputType.IfSpeedAbove,
                InputType.IfSpeedEqual,
                InputType.IfSpeedBelow
            },
            new InputType[] {
                InputType.AlwaysOn
            }
        };

        public static Dictionary<OperationType, UIDispOperation> UIOperationPairs = new Dictionary<OperationType, UIDispOperation>
        {
            {OperationType.ShiftPos, new UIDispOperation("Shift Position", "Move the position or angle by an amount", defaultValue:0f, sliderMaxIsMaxVel:true, sliderHasNegative:true) },
            {OperationType.SetPos, new UIDispOperation("Set Position", "Set the position or angle to Strength's value", defaultValue:0f, sliderFraction:1f, sliderMinOnPlanar:true) },
            {OperationType.ShiftSpeed, new UIDispOperation("Shift Speed", "Accelerate the velocity by an amount", defaultValue:0f, sliderMaxIsMaxVel:true, sliderHasNegative:true) },
            {OperationType.SetSpeed, new UIDispOperation("Set Speed", "Set the rate at which the position changes", defaultValue:0f, sliderMaxIsMaxVel:true, sliderHasNegative:true) },
            {OperationType.ArrowPoint, new UIDispOperation("Velocity Point", "Aim towards the direction the tech is moving", defaultValue:1f, sliderMax:1f, toggleComment:"Flip left & right") },
            {OperationType.GroundPoint, new UIDispOperation("Ground Point", "Aim towards the ground's surface", defaultValue:1f, clampStrength:true) },
            {OperationType.NorthPoint, new UIDispOperation("North Point", "Aim at a direction on a compass, determined by Strength", defaultValue:0f, sliderHasNegative:true, sliderMax:180f) },
            {OperationType.TargetPoint, new UIDispOperation("Target Point", "Aim towards the focused enemy (multiplied by Strength)\nThis will recenter if enemy is lost while running", defaultValue:1f, sliderMax:1f, toggleComment:"Flip left & right") },
            {OperationType.TargetPointPredictive, new UIDispOperation("Target Point (Predictive)", "Aim towards the focused enemy (with the following muzzle velocity)\nThis will recenter if enemy is lost while running", defaultValue:0f, sliderMax:500f, toggleComment:"Ignore Gravity") },
            {OperationType.PlayerPoint, new UIDispOperation("Player Point", "Aim towards the player's tech (multiplied by Strength)", defaultValue:1f, sliderMax:1f, toggleComment:"Flip left & right") },
            {OperationType.CursorPoint, new UIDispOperation("Cursor Point", "Aim towards the point the mouse goes to (multiplied by Strength)", defaultValue:1f, sliderMax:1f, toggleComment:"Flip left & right") },
            {OperationType.CameraPoint, new UIDispOperation("Camera Point", "Aim in the direction the camera is facing (multiplied by Strength)", defaultValue:1f, sliderMax:1f, toggleComment:"Flip left & right") },
            {OperationType.IfThen, new UIDispOperation("Start IF THEN", "Run everything up to EndIF or ELSE, if the condition is met (for Strength amount of time)", defaultValue:0f, sliderMax:5f, toggleComment:"False after time") },
            {OperationType.OrThen, new UIDispOperation("OR IF THEN", "Use this condition if the condition above is not met", strengthIsToggle:true, toggleComment:"Use timer from top") },
            {OperationType.ElseThen, new UIDispOperation("ELSE THEN", "If the above condition failed, do what is below", lockInputTypes:true, hideStrength:true) },
            {OperationType.EndIf, new UIDispOperation("End IF", "Close the condition branch above and proceed as normal", lockInputTypes:true, hideStrength:true) },
            {OperationType.Nothing, new UIDispOperation("Do Nothing", "It does nothing", hideStrength:true) },
#warning Might want to fix Deny Firing at some point
            {OperationType.FireWeapons, new UIDispOperation("Fire Weapons", "Any weapons on this cluster? Bam, unemployed", dummyNegative:"DenyFireWeapons", strengthIsToggle:true, toggleComment:"(Experimental) Deny firing") },
            {OperationType.SetLockJoint, new UIDispOperation("(Unavailable!) Set Lock-Joint", "(Unavailable!) Static state. Set the block-mover to use ghost-phasing", hideStrength:true) },
            {OperationType.SetBodyJoint, new UIDispOperation("(Unavailable!) Set Dynamic-Joint", "(Unavailable!) Physics state. Set the block-mover to use kinematics", hideStrength:true) },
            {OperationType.SetFreeJoint, new UIDispOperation("(Unavailable!) Set Free-Joint", "(Unavailable!) Suspension state. Set the block-mover to use loose kinematics", hideStrength:true) },
        };
        public static Dictionary<InputType, UIDispInput> UIInputPairs = new Dictionary<InputType, UIDispInput>
        {
            {InputType.AlwaysOn, new UIDispInput("Always On", "Just DOIT!", hideInputKey:true, hideParam:true) },
            {InputType.OnPress, new UIDispInput("On Key Press", "The moment this key is tapped", dummyNegative:"WhileNotPress", defaultValue:1f, paramIsToggle:true) },
            {InputType.WhileHeld, new UIDispInput("On Key Hold", "As long as this key is held", dummyNegative:"WhileNotHeld", defaultValue:1f, paramIsToggle:true) },
            {InputType.OnRelease, new UIDispInput("On Key Release", "Right when this key is released", dummyNegative:"WhileNotRelease", defaultValue:1f, paramIsToggle:true) },
            {InputType.Toggle, new UIDispInput("Toggle on Key", "Pressing this key turns this on or off", defaultValue:-1f, paramIsToggle:true, toggleComment:"State", toggleMultiplier:-1f) },
            {InputType.EnemyTechIsNear, new UIDispInput("If Enemy is Near", "While an enemy is within this range", dummyNegative:"EnemyTechIsFar", defaultValue:10f, hideInputKey:true, sliderMax:64) },
            {InputType.PlayerTechIsNear, new UIDispInput("If Player is Near", "While the 1st player is within this range", dummyNegative:"PlayerTechIsFar", defaultValue:10f, hideInputKey:true, sliderMax:64) },
            {InputType.AboveSurfaceElev, new UIDispInput("If Above Surface Elevation", "Checks how high it is from the surface directly below", dummyNegative:"BelowSurfaceElev", defaultValue:10f, hideInputKey:true, sliderMax:64) },
            {InputType.AboveVelocity, new UIDispInput("If Above Velocity", "Checks how many Meters-per-Second this block is moving (1 meter is 1 block unit)", dummyNegative:"BelowVelocity", defaultValue:10f, hideInputKey:true, sliderMax:60) },

            {InputType.IfPosAbove, new UIDispInput("If this Value Above", "Checks current position of this block", defaultValue:0f, hideInputKey:true, sliderMaxIsMaxVal:true) },
            {InputType.IfPosBelow, new UIDispInput("If this Value Below", "Checks current position of this block", defaultValue:0f, hideInputKey:true, sliderMaxIsMaxVal:true) },
            {InputType.IfPosEqual, new UIDispInput("If this Value Equals", "Checks current position of this block", defaultValue:0f, hideInputKey:true, sliderMaxIsMaxVal:true) },
            {InputType.IfSpeedAbove, new UIDispInput("If this Speed Above", "Checks current change-rate of this block", defaultValue:0f, hideInputKey:true, sliderMaxIsMaxVel:true) },
            {InputType.IfSpeedBelow, new UIDispInput("If this Speed Below", "Checks current change-rate of this block", defaultValue:0f, hideInputKey:true, sliderMaxIsMaxVel:true) },
            {InputType.IfSpeedEqual, new UIDispInput("If this Speed Equals", "Checks current change-rate of this block", defaultValue:0f, hideInputKey:true, sliderMaxIsMaxVel:true) },


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
            GroundPoint,
            NorthPoint,
            SetLockJoint,
            SetBodyJoint,
            SetFreeJoint,
            IfThen,
            OrThen,
            ElseThen,
            EndIf,
            CursorPoint,
            CameraPoint,
            Nothing,
            FireWeapons,
            TargetPointPredictive,
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
            IfSpeedEqual,
        }
        public OperationType m_OperationType = OperationType.SetPos;
        public InputType m_InputType = InputType.OnPress;
        public KeyCode m_InputKey = KeyCode.Space;

        public float m_InputParam; // Negative to invert condition
        public float m_Strength = 0;

        float timeSinceLastState;
        public bool LASTSTATE {
            get
            {
                return timeSinceLastState > Time.time;
            }
            set
            {
                if (value)
                {
                    timeSinceLastState = Time.time + 0.1f;
                }
            }
        }

        private static float SafePlanarPointAngle(Transform trans, Vector3 projectedVector, float originalValue) =>
                (Vector3.SignedAngle(trans.forward, projectedVector, trans.up) - originalValue + 900f) % 360f - 180f;

        private static float PointAtTarget(Transform trans, Vector3 relativeTarget, bool ProjectOnPlane, float OriginalValue)
        {
            if (ProjectOnPlane)
            {
                var planar = Vector3.ProjectOnPlane(relativeTarget, trans.up);
                return SafePlanarPointAngle(trans, planar, OriginalValue);
                //return (Vector3.SignedAngle(trans.forward, Vector3.ProjectOnPlane(localTarget, trans.up), trans.up));
            }
            return trans.InverseTransformDirection(relativeTarget).y - OriginalValue;
        }

        public float m_InternalTimer = 0f;
        public bool m_ResetTimer = false;


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
        public bool Calculate(ModuleBlockMover blockMover, bool LocalInput, bool ProjectDirToPlane, ref float Value, ref float Velocity, ref ModuleBlockMover.MoverType moverType, out int Skip)
        {
            Skip = 0;
            switch (m_OperationType)
            {
                case OperationType.OrThen: return false;
                case OperationType.ElseThen: Skip = 1; return false;
                default: break;
            }
            TankBlock block = blockMover.block;
            if (ConditionMatched(blockMover, LocalInput, Value, Velocity))
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
                        var vel = block.tank.rbody.GetPointVelocity(block.centreOfMassWorld);
                        if (blockMover.IsPlanarVALUE)
                        {
                            var planar = Vector3.ProjectOnPlane(vel * Mathf.Sign(m_Strength), block.trans.up);
                            Value += SafePlanarPointAngle(block.trans, planar, Value) * Mathf.Clamp01(m_Strength * m_Strength * planar.magnitude);
                            return true;
                        }
                        Value += block.trans.InverseTransformDirection(vel * Mathf.Sign(m_Strength)).y * Mathf.Clamp01(m_Strength * m_Strength);
                        return true;

                    case OperationType.TargetPoint:
                        Visible target = blockMover.GetTarget();
                        if (target == null)
                        {
                            if (m_ResetTimer)
                            {
                                Value = blockMover.UseLIMIT ? blockMover._CENTERLIMIT : 0f;
                                m_ResetTimer = false;
                            }
                            return false;
                        }
                        m_ResetTimer = true;
                        Value += PointAtTarget(block.trans, (target.GetAimPoint(block.trans.position) - block.centreOfMassWorld) * Mathf.Sign(m_Strength), ProjectDirToPlane, Value) * m_Strength * m_Strength;// * Mathf.Abs(m_Strength);
                        return true;

                    case OperationType.TargetPointPredictive:
                        Visible targetPred = blockMover.GetTarget();
                        if (targetPred == null)
                        {
                            if (m_ResetTimer)
                            {
                                Value = blockMover.UseLIMIT ? blockMover._CENTERLIMIT : 0f;
                                m_ResetTimer = false;
                            }
                            return false;
                        }
                        float muzzleVelocity = Mathf.Abs(m_Strength);
                        bool useGravity = m_Strength > 0;

                        Vector3 BlockCenter = block.trans.position;
                        Vector3 AimPointVector = targetPred.GetAimPoint(BlockCenter);
                        Vector3 vector = AimPointVector - BlockCenter;
                        if (muzzleVelocity > 0f) {
                            Vector3 dist = vector;
                            Rigidbody rbodyTank = block.tank.rbody;
                            Vector3 angularToggle = rbodyTank.angularVelocity;
                            Vector3 relativeVelocity = targetPred.rbody.velocity - (rbodyTank.velocity + angularToggle);

                            float time = dist.magnitude / muzzleVelocity;
                            if (useGravity)
                            {
                                // Console.WriteLine("enter Gravity");
                                float height = dist.y;
                                float land = Mathf.Sqrt((dist.x * dist.x) + (dist.z * dist.z));

                                float sqrVel = muzzleVelocity * muzzleVelocity;

                                if (sqrVel >= 30.0f * (height + dist.magnitude))
                                {
                                    // radians
                                    float theta = Mathf.Atan((sqrVel - Mathf.Sqrt((sqrVel * sqrVel) - ((900.0f * land * land) + (60.0f * height * sqrVel)))) / (30.0f * land));
                                    time = land / (Mathf.Cos(theta) * muzzleVelocity);

                                    // add elevation
                                    vector.y += (Mathf.Sin(theta) * muzzleVelocity * time) + BlockCenter.y;
                                }
                            }
                            // vector now represents where the enemy will be - still need elevation
                            vector += (time * relativeVelocity);
                        }
                        m_ResetTimer = true;
                        Value += PointAtTarget(block.trans, vector, ProjectDirToPlane, Value);// * Mathf.Abs(m_Strength);
                        return true;

                    case OperationType.PlayerPoint:
                        Tank playerTank = Singleton.playerTank;
                        if (playerTank == null)
                            return false;
                        Value += PointAtTarget(block.trans, (playerTank.WorldCenterOfMass - block.centreOfMassWorld) * Mathf.Sign(m_Strength), ProjectDirToPlane, Value) * m_Strength * m_Strength;// * Mathf.Abs(m_Strength);
                        return true;

                    case OperationType.GroundPoint:
                        var comw = block.centreOfMassWorld;
                        ManWorld.inst.GetTerrainHeight(comw, out float outHeight);
                        //return (comw.y > outHeight + m_InputParam);
                        float reducer = Mathf.Abs(Vector3.Dot(block.trans.up, Vector3.up));
                        if (blockMover.IsPlanarVALUE) reducer = 1f - reducer;
                        Value += PointAtTarget(block.trans, Vector3.up * (outHeight - comw.y) * Mathf.Sign(m_Strength), ProjectDirToPlane, Value) * m_Strength * m_Strength * reducer;
                        //Value += PointAtTarget(block.trans, Vector3.down * Mathf.Sign(m_Strength), ProjectDirToPlane, Value) * m_Strength * m_Strength;// * Mathf.Abs(m_Strength);
                        return true;

                    case OperationType.NorthPoint:

                        if (blockMover.IsPlanarVALUE)
                            Value = (Vector3.SignedAngle(block.trans.forward, Vector3.ProjectOnPlane(Vector3.forward, block.trans.up), block.trans.up) + m_Strength + 900f) % 360f - 180f;
                        else
                        {
                            float rad = m_Strength * Mathf.Deg2Rad;
                            Value = block.trans.InverseTransformDirection(new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad))).y;
                        }
                        return true;

                    case OperationType.SetFreeJoint:
                        if (blockMover.CanOnlyBeLockJoint)
                            return false;
                        if (blockMover.CannotBeFreeJoint)
                            moverType = ModuleBlockMover.MoverType.Dynamic;
                        else
                            moverType = ModuleBlockMover.MoverType.Physics;
                        return true;

                    case OperationType.SetBodyJoint:
                        if (blockMover.CanOnlyBeLockJoint)
                            return false;
                        moverType = ModuleBlockMover.MoverType.Dynamic;
                        return true;

                    case OperationType.SetLockJoint:
                        if (blockMover.CanOnlyBeLockJoint)
                            return false;
                        moverType = ModuleBlockMover.MoverType.Static;
                        return true;

                    case OperationType.CameraPoint:
                        var camTr = Singleton.cameraTrans;
                        if (camTr == null) return false;
                        Value += PointAtTarget(block.trans, camTr.forward * Mathf.Sign(m_Strength), ProjectDirToPlane, Value) * m_Strength * m_Strength;// * Mathf.Abs(m_Strength);
                        return true;

                    case OperationType.IfThen:
                        if (m_ResetTimer)
                        {
                            m_InternalTimer = 0f;
                            m_ResetTimer = false;
                        }
                        if (m_Strength == 0f) return true;

                        m_InternalTimer += Time.fixedDeltaTime;
                        // If time is satisfied and strength is positive, do not skip. Negative strength will only activate within that timeframe
                        bool met = (m_InternalTimer >= Mathf.Abs(m_Strength)) == (m_Strength >= 0f);
                        Skip = met ? 0 : 1;
                        return met;

                    case OperationType.CursorPoint:
                        Value += PointAtTarget(block.trans, (AdjustAttachPosition.PointerPos - block.centreOfMassWorld) * Mathf.Sign(m_Strength), ProjectDirToPlane, Value) * m_Strength * m_Strength;// * Mathf.Abs(m_Strength);
                        return true;

                    case OperationType.Nothing:
                        return true; // Light it up in the GUI. Technically, the task did not fail

                    case OperationType.FireWeapons:
                        if (blockMover.Holder == null) return false;
                        if (m_Strength < 0)
                            blockMover.Holder.ForceNoFireNextFrame = true;
                        else
                            blockMover.Holder.ForceFireNextFrame = true;
                        return true;

                    default:
                        return false;
                }
            }
            else if (m_OperationType == OperationType.IfThen)
            {
                if (m_Strength != 0)
                    m_InternalTimer += Time.fixedDeltaTime;
                if (m_ResetTimer)
                    m_InternalTimer = 0f;
                else
                    m_ResetTimer = true;
                Skip = 1;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CanDo">Permit input</param>
        /// <returns>1 if pressed, 2 if held, 3 if released</returns>
        int KeyState(bool CanDo = true)
        {
            if (CanDo && Input.GetKey(m_InputKey))
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

        static bool CanInput(ModuleBlockMover block, bool LocalInput)
        {
            return !LocalInput || block.block.tank == Singleton.playerTank;
        }

        public bool ConditionMatched(ModuleBlockMover block, bool localInput, float m_Val, float m_Vel)
        {
            bool Invert = m_InputParam < 0;
            switch (m_InputType)
            {
                case InputType.AlwaysOn:
                    return true;

                case InputType.OnPress:
                    m_InputParam = Mathf.Sign(m_InputParam);
                    return (KeyState(CanInput(block, localInput)) == 1) != Invert;

                case InputType.OnRelease:
                    m_InputParam = Mathf.Sign(m_InputParam);
                    return (KeyState(CanInput(block, localInput)) == 3) != Invert;

                case InputType.WhileHeld:
                    m_InputParam = Mathf.Sign(m_InputParam);
                    return (KeyState(CanInput(block, localInput)) != 0) != Invert;

                case InputType.Toggle:
                    m_InputParam = Mathf.Sign(m_InputParam - 0.001f);
                    if (KeyState(CanInput(block, localInput)) == 1)
                        m_InputParam = -m_InputParam;
                    return m_InputParam > 0;

                case InputType.EnemyTechIsNear:
                    Visible target = block.GetTarget();
                    if (target == null) return Invert;
                    return ((target.centrePosition - block.block.centreOfMassWorld).sqrMagnitude < m_InputParam * m_InputParam) != Invert;

                case InputType.PlayerTechIsNear:
                    if (Singleton.playerTank == null) return Invert;
                    if (Singleton.playerTank == block.block.tank) return !Invert;
                    return ((Singleton.playerTank.visible.centrePosition - block.block.centreOfMassWorld).sqrMagnitude < m_InputParam * m_InputParam) != Invert;

                case InputType.AboveSurfaceElev:
                    var comw = block.block.centreOfMassWorld;
                    ManWorld.inst.GetTerrainHeight(comw, out float outHeight);
                    return (comw.y > outHeight + m_InputParam) != Invert;

                case InputType.AboveVelocity:
                    return (block.block.tank.rbody.GetPointVelocity(block.block.centreOfMassWorld).sqrMagnitude > m_InputParam * m_InputParam) != Invert;

                case InputType.IfPosAbove:
                    if (block.IsPlanarVALUE)
                        return ((block.PVALUE + 900) % 360) - 180 > m_InputParam;
                    m_InputParam = Mathf.Max(m_InputParam, 0f);
                    return block.PVALUE > m_InputParam;
                case InputType.IfPosBelow:
                    if (block.IsPlanarVALUE)
                        return ((block.PVALUE + 900) % 360) - 180 < m_InputParam;
                    m_InputParam = Mathf.Max(m_InputParam, 0f);
                    return block.PVALUE < m_InputParam;
                case InputType.IfPosEqual:
                    if (block.IsPlanarVALUE)
                        return m_InputParam.Approximately(((block.PVALUE + 900) % 360) - 180);
                    m_InputParam = Mathf.Max(m_InputParam, 0f);
                    return block.PVALUE.Approximately(m_InputParam);

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

        public override string ToString() => ToString(false);

        string GetConditionString(bool dummyMode) => GetConditionString(dummyMode, UIInputPairs[m_InputType]);
        string GetConditionString(bool dummyMode, UIDispInput iT)
        {
            if (m_InputType == InputType.AlwaysOn) return "";
            if (dummyMode)
            {
                if (iT.HideInputKey)
                    return (m_InputParam < 0f && iT.DummyNegative != null ? 
                        iT.DummyNegative : m_InputType.ToString())
                        + " ";
                else
                    return (m_InputParam < 0f && iT.DummyNegative != null ? 
                        iT.DummyNegative : m_InputType.ToString()) 
                        + " " + m_InputKey.ToString() + " ";
            }
            return m_InputType.ToString() + " ( " +  // Condition and parenthesis
                (iT.HideInputKey ? "" : m_InputKey.ToString()) + // 1st parameter
                (!iT.HideInputKey && !iT.HideParam ? ", " : "") + // Put a comma in between if there are two parameters
                (iT.HideParam ? "" : m_InputParam.ToString()) + // 2nd parameter
                " ) "; // End parenthesis

            //Returns with a space at the end, unless empty!
        }
        
        string GetActionString(bool dummyMode)
        {
            var oT = UIOperationPairs[m_OperationType];
            return "DO " + (dummyMode && m_Strength < 0f && oT.DummyNegative != null ? oT.DummyNegative : m_OperationType.ToString()) + (dummyMode ? " " : " ( ") + (oT.HideStrength ? "" : m_Strength.ToString()) + (dummyMode ? "" : " )"); // 'Dummy mode' removes the parenthesis only
        }

        public string ToString(bool dummyMode)
        {
            switch (m_OperationType)
            {
                case OperationType.IfThen:
                    return (dummyMode && m_Strength != 0f ? "TIMER ( " : "IF ( ") + GetConditionString(dummyMode) + (dummyMode ? " )" : $", {m_Strength} )");
                case OperationType.OrThen:
                    return (dummyMode && m_Strength < 0f ? "OR TIMER ( " : "OR IF ( ") + GetConditionString(dummyMode) + (dummyMode ? " )" : $", {m_Strength} )");
                case OperationType.EndIf:
                    return "ENDIF";
                case OperationType.ElseThen:
                    return "ELSE";
                default:
                    return GetConditionString(dummyMode) + GetActionString(dummyMode);
            }
        }

        public static InputOperator FromString(string source)
        {
            var result = new InputOperator();
            source = source.ToLower().Replace("do ", "#do#").Replace(" ", "").Replace("\t", ""); // Highlight DO separator and remove spacing
            if (source == "endif") // ENDIF statement
            {
                result.m_InputType = InputType.AlwaysOn;
                result.m_OperationType = OperationType.EndIf;
            }
            else if (source == "else") // ENDIF statement
            {
                result.m_InputType = InputType.AlwaysOn;
                result.m_OperationType = OperationType.ElseThen;
            }
            else if (source.StartsWith("if(")) // IF statement
            {
                result.m_OperationType = OperationType.IfThen;
                source = source.Substring(3, source.Length - 4); //remove ')', compensate for `if(`
                string condition = source.Substring(0, source.LastIndexOf(','));
                ParseInputFromString(result, condition);
                string strength = source.Substring(condition.Length + 1);
                if (!float.TryParse(strength, out float fstrength)) throw new FormatException("Cannot parse strength parameter '" + strength + "'");
                result.m_Strength = fstrength;
            }
            else if (source.StartsWith("orif(")) // OR IF statement
            {
                result.m_OperationType = OperationType.OrThen;
                source = source.Substring(5, source.Length - 6); //remove ')', compensate for `orif(`
                string condition = source.Substring(0, source.LastIndexOf(','));
                ParseInputFromString(result, condition);
                string strength = source.Substring(condition.Length + 1);
                if (!float.TryParse(strength, out float fstrength)) throw new FormatException("Cannot parse strength parameter '" + strength + "'");
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

        public static string[] ProcessOperationsToStringArray(List<InputOperator> ProcessOperations, bool dummyMode = false, string[] StartingArray = null)
        {
            int startIndex;
            string[] result;
            if (StartingArray != null && StartingArray.Length != 0)
            {
                startIndex = StartingArray.Length;
                result = new string[ProcessOperations.Count + startIndex];
                StartingArray.CopyTo(result, 0);
            }
            else
            {
                startIndex = 0;
                result = new string[ProcessOperations.Count];
            }
            int c = 0;
            for (int i = 0; i < ProcessOperations.Count; i++)
            {
                var P = ProcessOperations[i];
                if (P.m_OperationType == OperationType.EndIf) c = Math.Max(c - 1, 0);
                result[i + startIndex] = "".PadRight(P.m_OperationType == OperationType.OrThen || P.m_OperationType == OperationType.ElseThen ? 
                    Math.Max(c - 1, 0) * 4 : c * 4) // Unpad or/else statements
                    + P.ToString(dummyMode);
                if (P.m_OperationType == OperationType.IfThen) c++;
            }
            return result;
        }
        public static string StringArrayToProcessOperations(string systemCopyBuffer, ref List<InputOperator> ProcessOperations)
        {
            int count = 0;
            try
            {
                var list = new List<InputOperator>(); // Make a new one, for in case there is an error
                foreach (string s in systemCopyBuffer.Replace("--", "")  // Remove double negatives (or just long even lines of --)
                    .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)) // Split by separator, keep empty entries for incrementing count only
                {
                    string source = s;
                    count++;
                    int comment = source.IndexOf('#'); // Comment character
                    if (comment != -1) source = source.Substring(0, comment);
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
