using System;
using UnityEngine;

namespace Control_Block
{
    internal class ModuleSwivel
    {
        public static void ConvertSerialToBlockMover(SerialData serialData, ModuleBlockMover blockMover)
        {
            string ProcessList;
            blockMover.SetDirty();
            blockMover.moverType = ModuleBlockMover.MoverType.Static;
            blockMover.LockJointBackPush = true;
            blockMover.LOCALINPUT = serialData.Local;
            blockMover._CENTERLIMIT = serialData.minRestrict;
            blockMover._EXTENTLIMIT = serialData.rangeRestrict;// / 2f;
            blockMover.UseLIMIT = serialData.Restrict && serialData.rangeRestrict != 0f;
            blockMover.VELOCITY = 0f;
            blockMover.VALUE = serialData.Angle;
            blockMover.PVALUE = serialData.Angle;
            blockMover.MAXVELOCITY = serialData.Speed;
            switch (serialData.mode)
            {
                case Mode.Positional:
                    if (serialData.StartDelay != 0f)
                        ProcessList = PositionalDelay;
                    else
                        ProcessList = Positional;
                    break;

                case Mode.Directional:
                    blockMover.VELOCITY = serialData.Direction * serialData.Speed;
                    if (serialData.CWDelay + serialData.CCWDelay != 0f)
                        ProcessList = DirectionalDelay;
                    else
                        ProcessList = Directional;
                    break;

                case Mode.Speed:
                    blockMover.VELOCITY = serialData.Direction * serialData.Speed;
                    if (serialData.StartDelay != 0f)
                        ProcessList = SpeedDelay;
                    else
                        ProcessList = Speed;
                    break;

                case Mode.OnOff:
                    blockMover.VELOCITY = serialData.Direction * serialData.Speed;
                    if (serialData.Restrict)
                    {
                        if (serialData.CWDelay + serialData.CCWDelay != 0f)
                            ProcessList = OnOffLimitDelay;
                        else
                            ProcessList = OnOffLimit;
                    }
                    else
                    {
                        if (serialData.StartDelay != 0f)
                            ProcessList = OnOffDelay;
                        else
                            ProcessList = OnOff;
                    }
                    break;

                case Mode.Aim:
                    if (serialData.StartDelay != 0f)
                        ProcessList = TargetDelay;
                    else
                        ProcessList = Target;
                    break;

                case Mode.Turning:
                    blockMover.VELOCITY = serialData.Direction * serialData.Speed;
                    if (serialData.Restrict)
                    {
                        if (serialData.StartDelay != 0f)
                            ProcessList = TurningLimitDelay;
                        else
                            ProcessList = TurningLimit;
                    }
                    else
                    {
                        if (serialData.StartDelay != 0f)
                            ProcessList = TurningDelay;
                        else
                            ProcessList = Turning;
                    }
                    break;

                case Mode.AimAtPlayer:
                    ProcessList = AimAtPlayer;
                    break;

                case Mode.Throttle:
                    blockMover.VELOCITY = serialData.Direction * serialData.Speed;
                    if (serialData.StartDelay != 0f)
                        ProcessList = ThrottleDelay;
                    else
                        ProcessList = Throttle;
                    break;

                case Mode.AimAtVelocity:
                    blockMover.VELOCITY = serialData.Direction * serialData.Speed;
                    ProcessList = AimAtVelocity;
                    break;

                case Mode.Cycle:
                    blockMover.VELOCITY = serialData.Direction * serialData.Speed;
                    if (serialData.Restrict)
                    {
                        if (serialData.StartDelay != 0f || serialData.CCWDelay != 0f || serialData.CWDelay != 0f)
                            ProcessList = CycleLimitDelay;
                        else
                            ProcessList = CycleLimit;
                    }
                    else
                    {
                        if (serialData.StartDelay != 0f || serialData.CCWDelay != 0f || serialData.CWDelay != 0f)
                            ProcessList = CycleDelay;
                        else
                            ProcessList = Cycle;
                    }
                    break;

                default:
                    Console.WriteLine("ModuleSwivel.ConvertSerialToBlockMover() : Cannot deserialize " + serialData.mode.ToString() + ", missing conversion!");
                    ProcessList = "";
                    break;
            }
            ProcessList = ProcessList.Replace("<KeyR>", serialData.Input1.ToString());
            ProcessList = ProcessList.Replace("<KeyL>", serialData.Input2.ToString());
            ProcessList = ProcessList.Replace("<SD>", ((serialData.StartDelay / serialData.Speed) * Time.fixedDeltaTime).ToString());
            ProcessList = ProcessList.Replace("<CWD>", ((serialData.CWDelay / serialData.Speed) * Time.fixedDeltaTime).ToString());
            ProcessList = ProcessList.Replace("<CCWD>", ((serialData.CCWDelay / serialData.Speed) * Time.fixedDeltaTime).ToString());
            ProcessList = ProcessList.Replace("<Speed>", serialData.Speed.ToString());
            ProcessList = ProcessList.Replace("<Smooth>", (serialData.Speed * 0.025f).ToString()); // Position += (Direction +- 0.025f) * Speed
            ProcessList = ProcessList.Replace("<Smooth2>", (serialData.Speed * 0.05f).ToString());
            ProcessList = ProcessList.Replace("<LimitC>", blockMover._CENTERLIMIT.ToString());
            ProcessList = ProcessList.Replace("<LimitL>", (((blockMover.MINVALUELIMIT + 900) % 360) - 180).ToString());
            ProcessList = ProcessList.Replace("<LimitR>", (((blockMover.MAXVALUELIMIT + 900) % 360) - 180).ToString());
            InputOperator.StringArrayToProcessOperations(ProcessList, ref blockMover.ProcessOperations);
        }

        public const string Positional = @"# 
WhileHeld(<KeyR>,1) DO ShiftPos(<Speed>)
WhileHeld(<KeyL>,1) DO ShiftPos(-<Speed>)",

            PositionalDelay = @"# Delayed
IF(WhileHeld(<KeyR>,1),<SD>)
    DO ShiftPos(<Speed>)
ENDIF
IF(WhileHeld(<KeyL>,1),<SD>)
    DO ShiftPos(-<Speed>)
ENDIF",

            TurningLimit = @"#
DO SetPos(<LimitC>)
WhileHeld(<KeyR>,1) DO SetPos(<LimitR>)
WhileHeld(<KeyL>,1) DO SetPos(<LimitL>)",

            TurningLimitDelay = @"#
DO SetPos(<LimitC>)
IF(WhileHeld(<KeyR>,1),<SD>)
    DO SetPos(<LimitR>)
ENDIF
IF(WhileHeld(<KeyL>,1),<SD>)
    DO SetPos(<LimitL>)
ENDIF",

            Turning = @"#
IF(WhileHeld(<KeyR>,-1), 0)
    WhileHeld(<KeyL>,-1) DO SetPos(<LimitC>)
ENDIF
WhileHeld(<KeyR>,1) DO ShiftPos(<Speed>)
WhileHeld(<KeyL>,1) DO ShiftPos(-<Speed>)",

            TurningDelay = @"#
IF(WhileHeld(<KeyR>,-1), 0)
    WhileHeld(<KeyL>,-1) DO SetPos(<LimitC>)
ENDIF
IF(WhileHeld(<KeyR>,1),<SD>)
    DO ShiftPos(<Speed>)
ENDIF
IF(WhileHeld(<KeyL>,1),<SD>)
    DO ShiftPos(-<Speed>)
ENDIF",

            Target = @"# 
WhileHeld(<KeyR>,1) DO ShiftPos(<Speed>)
WhileHeld(<KeyL>,1) DO ShiftPos(-<Speed>)
DO TargetPoint(1)",

            TargetDelay = @"# Delayed
IF(WhileHeld(<KeyR>,1),<SD>)
    DO ShiftPos(<Speed>)
ENDIF
IF(WhileHeld(<KeyL>,1),<SD>)
    DO ShiftPos(-<Speed>)
ENDIF
DO TargetPoint(1)",

        Directional = @"# Set VEL
WhileHeld(<KeyR>,1) DO SetSpeed(<Speed>)
WhileHeld(<KeyL>,1) DO SetSpeed(-<Speed>)",

            DirectionalDelay = @"# Delayed, Set VEL
IF(WhileHeld(<KeyR>,1),<CCWD>)
    DO SetSpeed(<Speed>)
ENDIF
IF(WhileHeld(<KeyL>,1),<CWD>)
    DO SetSpeed(-<Speed>)
ENDIF",

            Speed = @"# Set VEL
WhileHeld(<KeyR>,1) DO ShiftSpeed(<Smooth>)
WhileHeld(<KeyL>,1) DO ShiftSpeed(-<Smooth>)",

            SpeedDelay = @"# Delayed, Set VEL
IF(WhileHeld(<KeyR>,1),<SD>)
    DO ShiftSpeed(<Smooth>)
ENDIF
IF(WhileHeld(<KeyL>,1),<SD>)
    DO ShiftSpeed(-<Smooth>)
ENDIF",

            OnOff = @"# Set VEL
OnPress(<KeyR>,1) DO ShiftSpeed(<Speed>)
OnPress(<KeyL>,1) DO ShiftSpeed(<Speed>)",

            OnOffDelay = @"# Delayed, Set VEL
IF(WhileHeld(<KeyR>,1),<SD>)
    OnPress(<KeyR>,1) DO ShiftSpeed(<Speed>)
ENDIF
IF(WhileHeld(<KeyL>,1),<SD>)
    OnPress(<KeyL>,1) DO ShiftSpeed(-<Speed>)
ENDIF",

            OnOffLimit = @"# Set VEL
IfPosEqual(<LimitL>) DO SetSpeed(0)
IfPosEqual(<LimitR>) DO SetSpeed(0)
OnPress(<KeyR>,1) DO ShiftSpeed(<Speed>)
OnPress(<KeyL>,1) DO ShiftSpeed(-<Speed>)",

            OnOffLimitDelay = @"# Delayed, Set VEL
IfPosEqual(<LimitL>) DO SetSpeed(0)
IfPosEqual(<LimitR>) DO SetSpeed(0)
IF(WhileHeld(<KeyR>,1),<CCWD>)
    OnPress(<KeyR>,1) DO ShiftSpeed(<Speed>)
ENDIF
IF(WhileHeld(<KeyL>,1),<CWD>)
    OnPress(<KeyL>,1) DO ShiftSpeed(-<Speed>)
ENDIF",

            Throttle = @"# Set VEL
WhileHeld(<KeyR>,1) DO ShiftSpeed(<Smooth2>)
WhileHeld(<KeyL>,1) DO ShiftSpeed(-<Smooth2>)
SpeedIsAbove(0) DO ShiftSpeed(-<Smooth>)
SpeedIsBelow(0) DO ShiftSpeed(<Smooth>)",

            ThrottleDelay = @"# Delayed, Set VEL
IF(WhileHeld(<KeyR>,1),<SD>)
    DO ShiftSpeed(<Smooth2>)
ENDIF
IF(WhileHeld(<KeyL>,1),<SD>)
    DO ShiftSpeed(-<Smooth2>)
ENDIF
SpeedIsAbove(0) DO ShiftSpeed(-<Smooth>)
SpeedIsBelow(0) DO ShiftSpeed(<Smooth>)",
            
            AimAtPlayer = @"#
DO PlayerPoint(1)",
            
            AimAtVelocity = @"#
DO GroundPoint(0.2)
DO ArrowPoint(1)",
            
            CycleLimitDelay = @"# Delayed, Set VEL
IF(IfSpeedAbove(0),-<SD>)
ORIF(IfSpeedBelow(0),-1)
    DO SetPos(<LimitC>)
    IfSpeedAbove(0) DO ShiftPos(-<Speed>)
    IfSpeedBelow(0) DO ShiftPos(<Speed>)
ELSE
    IF(IfPosEqual(<LimitR>),<CWD>)
        DO SetSpeed(-<Speed>)
    ENDIF
    IF (IfPosEqual(<LimitL>),<CCWD>)
        DO SetSpeed(<Speed>)
    ENDIF
ENDIF
IF(OnPress(<KeyR>,1),0)
    IF(IfSpeedEqual(0),0)
        DO SetSpeed(<Speed>)
        DO ShiftPos(-<Speed>)
    ELSE
        DO SetSpeed(0)
        DO SetPos(<LimitC>)
    ENDIF
ENDIF
IF(OnPress(<KeyL>,1),0)
    IF(IfSpeedEqual(0),0)
        DO SetSpeed(-<Speed>)
        DO ShiftPos(<Speed>)
    ELSE
        DO SetSpeed(0)
        DO SetPos(<LimitC>)",
            
            CycleDelay = @"# Delayed, Set VEL
IF(IfSpeedAbove(0),-<SD>)
ORIF(IfSpeedBelow(0),-1)
    DO SetPos(<LimitC>)
    IfSpeedAbove(0) DO ShiftPos(-<Speed>)
    IfSpeedBelow(0) DO ShiftPos(<Speed>)
ENDIF
IF(OnPress(<KeyR>,1),0)
    IF(IfSpeedEqual(0),0)
        DO SetSpeed(<Speed>)
        DO ShiftPos(-<Speed>)
    ELSE
        DO SetSpeed(0)
        DO SetPos(<LimitC>)
    ENDIF
ENDIF
IF(OnPress(<KeyL>,1),0)
    IF(IfSpeedEqual(0),0)
        DO SetSpeed(-<Speed>)
        DO ShiftPos(<Speed>)
    ELSE
        DO SetSpeed(0)
        DO SetPos(<LimitC>)",

            CycleLimit = @"# Set VEL
IF(IfSpeedEqual(0),0)
    DO SetPos(<LimitC>)
ELSE
    IfPosEqual(<LimitR>) DO SetSpeed(-<Speed>)
    IfPosEqual(<LimitL>) DO SetSpeed(<Speed>)
ENDIF
IF(OnPress(<KeyR>,1),0)
    IF(IfSpeedEqual(0),0)
        DO SetSpeed(<Speed>)
    ELSE
        DO SetSpeed(0)
    ENDIF
ENDIF
IF(OnPress(<KeyL>,1),0)
    IF (IfSpeedEqual(0),0)
        DO SetSpeed(-<Speed>)
    ELSE
        DO SetSpeed(0)",

            Cycle = @"# Set VEL
IF(OnPress(<KeyR>,1),0)
    IF(IfSpeedEqual(0),0)
        DO SetSpeed(<Speed>)
    ELSE
        DO SetSpeed(0)
        DO SetPos(<LimitC>)
    ENDIF
ENDIF
IF(OnPress(<KeyL>,1),0)
    IF (IfSpeedEqual(0),0)
        DO SetSpeed(-<Speed>)
    ELSE
        DO SetSpeed(0)
        DO SetPos(<LimitC>)";

        public enum Mode : byte
        {
            /// <summary> Input, StartDelay </summary>
            Positional,

            /// <summary> Input, SideDelay </summary>
            Directional,

            /// <summary> Input, StartDelay </summary>
            Speed,

            /// <summary> Input, StartDelay, SideDelay </summary>
            OnOff,

            /// <summary> When no target, Positional </summary>
            Aim,

            /// <summary> Input, StartDelay </summary>
            Turning,

            /// <summary> No Input </summary>
            AimAtPlayer,

            /// <summary> No Input </summary>
            AimAtVelocity,

            /// <summary> Input, StartDelay, SideDelay </summary>
            Cycle,

            /// <summary> Input, StartDelay </summary>
            Throttle,
        }

        [Serializable]
        public class SerialData : Module.SerialData<ModuleSwivel.SerialData>
        {
            public float Angle;
            public KeyCode Input1;
            public KeyCode Input2;
            public bool Local;
            public float Speed;
            public Mode mode;
            public bool Restrict;
            public float rangeRestrict;
            public float Direction;
            public float minRestrict;
            public float StartDelay;
            public float CWDelay;
            public float CCWDelay;
            public float CurrentDelay;
        }
    }
}