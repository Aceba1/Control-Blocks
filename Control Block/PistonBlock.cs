using System;
using UnityEngine;

namespace Control_Block
{
    class ModulePiston
    {
        public static void ConvertSerialToBlockMover(SerialData serialData, ModuleBlockMover blockMover)
        {
            string ProcessList;
            blockMover.SetDirty();
            blockMover.moverType = ModuleBlockMover.MoverType.Static;
            blockMover.LockJointBackPush = true;
            blockMover.LOCALINPUT = serialData.Local;
            blockMover._CENTERLIMIT = serialData.Stretch != 0 ? serialData.Stretch / 2f : blockMover.HalfLimitVALUE;
            blockMover._EXTENTLIMIT = blockMover._CENTERLIMIT;
            blockMover.UseLIMIT = serialData.Stretch != 0 && serialData.Stretch != blockMover.TrueLimitVALUE;
            blockMover.VELOCITY = 0f;
            blockMover.VALUE = serialData.IsOpen ? serialData.Stretch : 0f;
            blockMover.PVALUE = blockMover.VALUE;

            var mode = GetMode(serialData.Toggle, serialData.Invert, serialData.PreferState);
            ProcessList = ModeToProcessFormat[(int)mode];
            ProcessList = ProcessList.Replace("<Input>", serialData.Input.ToString());
            ProcessList = ProcessList.Replace("<Extent>", (serialData.Stretch == 0 ? blockMover.TrueLimitVALUE : serialData.Stretch).ToString());
            ProcessList = ProcessList.Replace("<ToggleState>", serialData.IsOpen ? "1" : "-1");
            InputOperator.StringArrayToProcessOperations(ProcessList, ref blockMover.ProcessOperations);
        }

        public const string WhileHeldScript = @"# Toggle:false Invert:false Prefer:false
OnPress(<Input>,0) DO SetPos(<Extent>)
OnRelease(<Input>,0) DO SetPos(0)",

            WhileNotHeldScript = @"# Toggle:false Invert:true Prefer:false
OnPress(<Input>,0) DO SetPos(0)
OnRelease(<Input>,0) DO SetPos(<Extent>)",

            Toggle = @"# Toggle:true Invert:false Prefer:false
OnPress(<Input>,1) DO SetPos(0)
Toggle(<Input>,<ToggleState>) DO SetPos(<Extent>)",

            ToggleDelayed = @"# Toggle:true Invert:true Prefer:false
IF (Toggle(<Input>,<ToggleState>),0)
    OnRelease(<Input>,1) DO SetPos(<Extent>)
ELSE
    OnRelease(<Input>,1) DO SetPos(0)",

            TogglePreferExtended = @"# Toggle:true Invert:false Prefer:true
OnRelease(<Input>,1) DO SetPos(0)
Toggle(<Input>,<ToggleState>) DO SetPos(<Extent>)",
            
            TogglePreferContracted = @"# Toggle:true Invert:true Prefer:true
OnRelease(<Input>,1) DO SetPos(<Extent>)
Toggle(<Input>,<ToggleState>) DO SetPos(0)";

        public static string[] ModeToProcessFormat = new string[]
        {
            WhileHeldScript,
            WhileNotHeldScript,
            Toggle,
            ToggleDelayed,
            TogglePreferExtended,
            TogglePreferContracted
        };

        public enum Mode : byte
        {
            WhileHeld,
            WhileNotHeld,
            Toggle,
            ToggleDelayed,
            TogglePreferExtended,
            TogglePreferContracted
        }

        public static Mode GetMode(bool Toggle, bool Invert, bool Prefer)
        {
            return (Mode)((Invert ? 1 : 0) + (Toggle ? 2 + (Prefer ? 2 : 0) : 0));
        }

        [Serializable]
        public class SerialData : Module.SerialData<ModulePiston.SerialData>
        {
            public bool IsOpen;
            public KeyCode Input;
            public bool Toggle;
            public bool Local;
            public bool Invert;
            public bool PreferState;
            public int Stretch;
        }
    }
}