using System;
using UnityEngine;

namespace Control_Block
{
    //     [RequireComponent(typeof(TargetAimer))]
    internal class ModuleSwivel : ModuleBlockMover
    {
        //         public TargetAimer aimer;
        //         public GimbalAimer gimbal;

        //         public float EvaluatedBlockRotCurve = 0f, oldEvaluatedBlockCurve = 0f;
        //         public bool LockAngle = false;
        //         public float AngleCenter = 0f, AngleRange = 45f;

        //         public float StartDelay, CWDelay, CCWDelay, CurrentDelay;

        //         public float Direction = 0f;
        //         public Mode mode = Mode.Positional;

        public static void ConvertSerialToBlockMover(SerialData serialData, ModuleBlockMover blockMover)
        {
            string ProcessList;
            blockMover.SetDirty();
            blockMover.moverType = ModuleBlockMover.MoverType.Static;
            blockMover.LockJointBackPush = true;
            blockMover.LOCALINPUT = serialData.Local;
            blockMover._CENTERLIMIT = serialData.minRestrict;
            blockMover._EXTENTLIMIT = serialData.rangeRestrict;
            blockMover.VELOCITY = serialData.Direction;
            switch(serialData.mode)
            {
                case Mode.Positional:
                    {
                        if (serialData.StartDelay != 0f)
                        {

                        }
                        else // No nonsense
                        {

                        }
                        return;
                    }
            }
        }

        public enum Mode : byte
        {
            /// <summary>
            /// Input, StartDelay
            /// </summary>
            Positional,

            /// <summary>
            /// Input, SideDelay
            /// </summary>
            Directional,

            /// <summary>
            /// Input, StartDelay
            /// </summary>
            Speed,

            /// <summary>
            /// Input, StartDelay, SideDelay
            /// </summary>
            OnOff,

            /// <summary>
            /// When no target, Positional
            /// </summary>
            Aim,

            /// <summary>
            /// Input, StartDelay
            /// </summary>
            Turning,

            /// <summary>
            /// No Input
            /// </summary>
            AimAtPlayer,

            /// <summary>
            /// No Input
            /// </summary>
            AimAtVelocity,

            /// <summary>
            /// Input, StartDelay, SideDelay
            /// </summary>
            Cycle,

            /// <summary>
            /// Input, StartDelay
            /// </summary>
            Throttle,
        }

        //switch (mode)
        //{
        //case Mode.Aim:
        //    aimer.AimAtWorldPos(parts[parts.Length - 1].rotation* Vector3.forward + parts[parts.Length - 1].transform.position, 100000000);
        //aimer.UpdateAndAimAtTarget(RotateSpeed / Time.deltaTime);
        //    if (aimer.HasTarget)
        //    {
        //        CurrentAngle = parts[parts.Length - 1].localRotation.eulerAngles.y;
        //        WasAiming = true;
        //    }
        //    else if (WasAiming)
        //    {
        //        Direction = Mathf.Clamp(-(Mathf.Repeat(CurrentAngle + 180, 360) - 180) / RotateSpeed, -1f, 1f);
        //        CurrentAngle += Direction* RotateSpeed;
        //        if (CurrentAngle == 0)
        //        {
        //            WasAiming = false;
        //            gimbal.ResetAngles();
        //            CurrentAngle = 0;
        //        }
        //    }
        //    else
        //    {
        //        goto Positional;
        //    }
        //    break;

        //case Mode.Positional:
        //    Positional:
        //    if (VInput)
        //    {
        //        if (Input.GetKey(trigger1))
        //        {
        //            if (CurrentDelay > 0)
        //            {
        //                CurrentDelay -= RotateSpeed;
        //                break;
        //            }
        //            CurrentAngle += RotateSpeed;
        //        }
        //        else if (Input.GetKey(trigger2))
        //        {
        //            if (CurrentDelay > 0)
        //            {
        //                CurrentDelay -= RotateSpeed;
        //                break;
        //            }
        //            CurrentAngle -= RotateSpeed;
        //        }
        //        else
        //        {
        //            CurrentDelay = StartDelay;
        //        }
        //    }
        //    break;

        //case Mode.AimAtPlayer:
        //    if (Singleton.playerTank != null)
        //    {
        //        gimbal.Aim(Singleton.playerTank.rbody.worldCenterOfMass, (RotateSpeed / Time.deltaTime));
        //    }
        //    else
        //    {
        //        gimbal.AimDefault((RotateSpeed / Time.deltaTime));
        //    }

        //    CurrentAngle = parts[parts.Length - 1].localRotation.eulerAngles.y;
        //    break;

        //case Mode.AimAtVelocity:
        //    gimbal.Aim(parts[parts.Length - 1].transform.position + tankcache.rbody.GetPointVelocity(parts[parts.Length - 1].transform.position) + (((LockAngle? block.transform.forward : Vector3.down) / Time.deltaTime* 1f) * (Vector3.ProjectOnPlane(block.transform.up, Vector3.up).magnitude + 0.1f)), (RotateSpeed / Time.deltaTime));
        //    CurrentAngle = parts[parts.Length - 1].localRotation.eulerAngles.y;
        //    break;

        //case Mode.Directional:
        //    if (Direction == 0 && CurrentDelay <= 0)
        //    {
        //        CurrentDelay = StartDelay;
        //    }
        //    if (VInput)
        //    {
        //        if (Input.GetKey(trigger1))
        //        {
        //            Direction = 1f;
        //        }
        //        else if (Input.GetKey(trigger2))
        //        {
        //            Direction = -1f;
        //        }
        //    }
        //    if (Direction != 0 && CurrentDelay > 0)
        //    {
        //        CurrentDelay -= RotateSpeed;
        //        break;
        //    }
        //    CurrentAngle += Direction* RotateSpeed;
        //    break;

        //case Mode.Throttle:
        //case Mode.Speed:
        //    if (Direction == 0 && ButtonNotPressed && CurrentDelay <= 0)
        //    {
        //        CurrentDelay = StartDelay;
        //    }
        //    if (VInput)
        //    {
        //        if (Input.GetKey(trigger1))
        //        {
        //            Direction += 0.025f;
        //        }
        //        else if (Input.GetKey(trigger2))
        //        {
        //            Direction -= 0.025f;
        //        }
        //        else if (mode == Mode.Throttle)
        //        {
        //            Direction = Mathf.Clamp(0, Direction - 0.025f, Direction + 0.025f);
        //        }
        //        Direction = Mathf.Clamp(Direction, -1f, 1f);
        //    }
        //    if (Direction != 0 && CurrentDelay > 0)
        //    {
        //        CurrentDelay -= RotateSpeed;
        //        if (CurrentDelay <= 0)
        //        {
        //            Direction = 0;
        //        }

        //        break;
        //    }
        //    CurrentAngle += Direction* RotateSpeed;
        //    break;

        //case Mode.OnOff:
        //    if (Direction == 0 && CurrentDelay <= 0)
        //    {
        //        CurrentDelay = StartDelay;
        //    }
        //    if (VInput && ButtonNotPressed)
        //    {
        //        if (Input.GetKey(trigger1))
        //        {
        //            Direction += 1f;
        //        }
        //        else if (Input.GetKey(trigger2))
        //        {
        //            Direction -= 1f;
        //        }
        //        Direction = Mathf.Clamp(Direction, -1f, 1f);
        //    }
        //    if (Direction != 0 && CurrentDelay > 0)
        //    {
        //        CurrentDelay -= RotateSpeed;
        //        break;
        //    }
        //    CurrentAngle += Direction* RotateSpeed;
        //    break;

        //case Mode.Cycle:
        //    if (Direction == 0 && CurrentDelay <= 0)
        //    {
        //        CurrentDelay = StartDelay;
        //    }
        //    if (VInput && ButtonNotPressed)
        //    {
        //        if (Input.GetKey(trigger1))
        //        {
        //            if (Direction == 0) // Forward
        //            {
        //                Direction = 1;
        //            }
        //            else
        //            {
        //                Direction = 0;
        //            }
        //        }
        //        else if (Input.GetKey(trigger2))
        //        {
        //            if (Direction == 0) // Reverse
        //            {
        //                Direction = -1;
        //            }
        //            else
        //            {
        //                Direction = 0;
        //            }
        //        }
        //    }
        //    if (Direction != 0)
        //    {
        //        if (CurrentDelay > 0)
        //        {
        //            CurrentDelay -= RotateSpeed;
        //        }
        //        else
        //        {
        //            CurrentAngle += Direction* RotateSpeed;
        //        }
        //    }
        //    else
        //    {
        //        CurrentAngle += Mathf.Clamp(-(Mathf.Repeat(CurrentAngle + 180, 360) - 180) / RotateSpeed, -1f, 1f) * RotateSpeed;
        //        if (CurrentAngle == 0)
        //        {
        //            CurrentDelay = 0;
        //        }
        //    }
        //    break;

        //case Mode.Turning:

        //    if (Direction == 0 && CurrentAngle == AngleCenter && CurrentDelay <= 0)
        //    {
        //        CurrentDelay = StartDelay;
        //    }
        //    if (VInput)
        //    {
        //        if (Input.GetKey(trigger1))
        //        {
        //            Direction = +1;
        //        }
        //        else if (Input.GetKey(trigger2))
        //        {
        //            Direction = -1;
        //        }
        //        else
        //        {
        //            Direction = -(Mathf.Repeat(CurrentAngle - AngleCenter + 180, 360) - 180) / RotateSpeed;
        //        }
        //    }
        //    else
        //    {
        //        Direction = -(Mathf.Repeat(CurrentAngle - AngleCenter + 180, 360) - 180) / RotateSpeed;
        //    }
        //    Direction = Mathf.Clamp(Direction, -1f, 1f);

        //    if (Direction != 0 && CurrentDelay > 0)
        //    {
        //        CurrentDelay -= RotateSpeed;
        //        break;
        //    }
        //    CurrentAngle += Direction* RotateSpeed;
        //    break;
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