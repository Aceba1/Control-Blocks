using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
using UnityEngine;
// using System.Reflection;

namespace Control_Block
{
    class ModulePiston
    {
        //                 if (IsToggle)
        //                 {
        //                     switch (InverseTrigger)
        //                     {
        //                         case 0: //Change toggle on down
        //                             if (VInput && Input.GetKeyDown(trigger))
        //                                 AlphaOpen = 1f - AlphaOpen;
        //                             break;
        //                         case 1: //Change toggle on up
        //                             if (VInput && Input.GetKeyUp(trigger))
        //                                 AlphaOpen = 1f - AlphaOpen;
        //                             break;
        //                         case 2: //Prefer extent
        //                             if ((AlphaOpen == 0f && VInput && Input.GetKeyDown(trigger)) ||
        //                                 (AlphaOpen == 1f && VInput && Input.GetKeyUp(trigger)))
        //                                 if (ButtonIsValid)
        //                                 {
        //                                     ButtonIsValid = false;
        //                                     AlphaOpen = 1f - AlphaOpen;
        //                                 }
        //                             break;
        //                         case 3: //Prefer contract
        //                             if ((AlphaOpen == 1f && VInput && Input.GetKeyDown(trigger)) ||
        //                                 (AlphaOpen == 0f && VInput && Input.GetKeyUp(trigger)))
        //                                 if (ButtonIsValid)
        //                                 {
        //                                     ButtonIsValid = false;
        //                                     AlphaOpen = 1f - AlphaOpen;
        //                                 }
        //                             break;
        //                     }
        //                 }
        //                 else // Not Toggle
        //                 {
        //                     if ((VInput && Input.GetKey(trigger)) != (InverseTrigger == 1)) // If pressed, * Invert
        //                     {
        //                         AlphaOpen = 1f; // Open 
        //                     }
        //                     else
        //                     {
        //                         AlphaOpen = 0f; // Close
        //                     }
        //                 }

        //InverseTrigger = (byte)((serialData2.Invert ? 1 : 0) + (serialData2.PreferState ? 2 : 0));

        public const string WhileHeldScript = @"# Toggle:false Invert:false Prefer:false
OnPress(<Input>,0) DO SetPos(<Extent>)
OnRelease(<Input>,0) DO SetPos(0)", // 
            WhileNotHeldScript = @"# Toggle:false Invert:true Prefer:false
OnPress(<Input>,0) DO SetPos(0)
OnRelease(<Input>,0) DO SetPos(<Extent>)", //
            Toggle = @"# Toggle:true Invert:false Prefer:false
DO SetPos(0)
Toggle(<Input>,0) DO SetPos(<Extent>)",
            ToggleDelayed = @"# Toggle:true Invert:true Prefer:false
Toggle(<Input>,0) DO ";
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