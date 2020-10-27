using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using UnityEngine;


namespace Control_Block
{
    /// <summary>
    /// Class that defines the module that activates <see cref="PIDController"/>.
    /// </summary>
    /// <remarks> If (the <see cref="PIDController"/>) not extant on a <see cref="Tank"/>, one (<see cref="PIDController"/>) will be created. On the removal of all <see cref="ModulePID"/>s from the <see cref="Tank"/>, the <see cref="Tank"/>'s <see cref="PIDController"/> will also be destroyed.</remarks>
    [Serializable()]
    public class ModulePID : Module
    {
        private PIDController attachedPID;

        public float targetHeight = 50f;
        public float manualTargetChangeRate = 0f;
        public bool useTargetHeight = false;
        public bool enableHoldPosition = false;

        public float targetPitch = 0f;
        public float targetRoll = 0f;

        private int availableAxesMask = 0;

        [SerializeField]
        public PIDController.PIDParameters m_AccelParameters;
        [SerializeField]
        public PIDController.PIDParameters m_HoverParameters;
        [SerializeField]
        public PIDController.PIDParameters m_StrafeParameters;

        [SerializeField]
        public PIDController.PIDParameters m_PitchParameters;
        [SerializeField]
        public PIDController.PIDParameters m_RollParameters;
        [SerializeField]
        public PIDController.PIDParameters m_YawParameters;

        public bool MatchesAxis(PIDController.PIDParameters.PIDAxis axis)
        {
            int mask = PIDController.PIDParameters.AxisMask(axis);
            return (this.availableAxesMask & mask) != 0;
        }
        public void AddAxis(PIDController.PIDParameters.PIDAxis axis)
        {
            int mask = PIDController.PIDParameters.AxisMask(axis);
            this.availableAxesMask |= mask;
        }
        public bool ContainsPidAxis(PIDController.PIDParameters.PIDAxis axis)
        {
            if (axis == PIDController.PIDParameters.PIDAxis.Accel)
            {
                return this.m_AccelParameters != null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Strafe)
            {
                return this.m_StrafeParameters != null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Hover)
            {
                return this.m_HoverParameters != null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Pitch)
            {
                return this.m_PitchParameters != null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Roll)
            {
                return this.m_RollParameters != null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Yaw)
            {
                return this.m_YawParameters != null;
            }
            return false;
        }

        private void ClearParametersByAxis(PIDController.PIDParameters.PIDAxis axis)
        {
            PIDController.GlobalDebugPrint($"ModulePID.ClearParametersByAxis {axis} {this.block.name}");
            if (axis == PIDController.PIDParameters.PIDAxis.Accel)
            {
                Destroy(this.m_AccelParameters);
			    this.m_AccelParameters = null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Strafe)
            {
                Destroy(this.m_StrafeParameters);
			    this.m_StrafeParameters = null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Hover)
            {
                Destroy(this.m_HoverParameters);
			    this.m_HoverParameters = null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Pitch)
            {
                Destroy(this.m_PitchParameters);
			    this.m_PitchParameters = null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Roll)
            {
                Destroy(this.m_RollParameters);
			    this.m_RollParameters = null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Yaw)
            {
                Destroy(this.m_YawParameters);
			    this.m_YawParameters = null;
            }
        }
        public bool AddParameters(PIDController.PIDParameters parameters)
        {
            string parameterStr = ModulePID.ConvertOnSerialize(parameters).ToString(CultureInfo.InvariantCulture);
            PIDController.GlobalDebugPrint($"ModulePID.AddParameters - {parameterStr}");
            // this.availableAxesMask |= PIDController.PIDParameters.AxisMask(parameters.pidAxis);
            if (parameters.pidAxis == PIDController.PIDParameters.PIDAxis.Accel)
            {
                if (this.m_AccelParameters != null)
                {
                    return false;
                }
                this.m_AccelParameters = parameters;
            }
            else if (parameters.pidAxis == PIDController.PIDParameters.PIDAxis.Strafe)
            {
                if (this.m_StrafeParameters != null)
                {
                    return false;
                }
                this.m_StrafeParameters = parameters;
            }
            else if (parameters.pidAxis == PIDController.PIDParameters.PIDAxis.Hover)
            {
                if (this.m_HoverParameters != null)
                {
                    return false;
                }
                this.m_HoverParameters = parameters;
            }
            else if (parameters.pidAxis == PIDController.PIDParameters.PIDAxis.Pitch)
            {
                if (this.m_PitchParameters != null)
                {
                    return false;
                }
                this.m_PitchParameters = parameters;
            }
            else if (parameters.pidAxis == PIDController.PIDParameters.PIDAxis.Roll)
            {
                if (this.m_RollParameters != null)
                {
                    return false;
                }
                this.m_RollParameters = parameters;
            }
            else if (parameters.pidAxis == PIDController.PIDParameters.PIDAxis.Yaw)
            {
                if (this.m_YawParameters != null)
                {
                    this.m_YawParameters = parameters;
                }
            }
            return true;
        }

        #region UpdateParameters
        public void OnUpdateParameters(PIDController.PIDParameters parameters, PIDController.PIDParameters.PIDAxis axis)
        {
            string parameterStr = ModulePID.ConvertOnSerialize(parameters).ToString(CultureInfo.InvariantCulture);
            PIDController.GlobalDebugPrint($"ModulePID.OnUpdateParameters {axis} - {parameterStr} {this.block.name}");
            if (axis == PIDController.PIDParameters.PIDAxis.Accel)
            {
                if (parameters != null)
                {
                    if (this.m_AccelParameters == null)
                    {
                        this.m_AccelParameters = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                        this.m_AccelParameters.pidAxis = axis;
                    }
                    this.m_AccelParameters.kP = parameters.kP;
                    this.m_AccelParameters.kI = parameters.kI;
                    this.m_AccelParameters.kD = parameters.kD;
                    this.m_AccelParameters.debug = parameters.debug;
                    this.m_AccelParameters.enabled = parameters.enabled;
                }
                else
                {
                    this.m_AccelParameters = null;
                }
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Strafe)
            {
                if (parameters != null)
                {
                    if (this.m_StrafeParameters == null)
                    {
                        this.m_StrafeParameters = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                        this.m_StrafeParameters.pidAxis = axis;
                    }
                    this.m_StrafeParameters.kP = parameters.kP;
                    this.m_StrafeParameters.kI = parameters.kI;
                    this.m_StrafeParameters.kD = parameters.kD;
                    this.m_StrafeParameters.debug = parameters.debug;
                    this.m_StrafeParameters.enabled = parameters.enabled;
                }
                else
                {
                    this.m_StrafeParameters = null;
                }
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Hover)
            {
                if (parameters != null)
                {
                    if (this.m_HoverParameters == null)
                    {
                        this.m_HoverParameters = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                        this.m_HoverParameters.pidAxis = axis;
                    }
                    this.m_HoverParameters.kP = parameters.kP;
                    this.m_HoverParameters.kI = parameters.kI;
                    this.m_HoverParameters.kD = parameters.kD;
                    this.m_HoverParameters.debug = parameters.debug;
                    this.m_HoverParameters.enabled = parameters.enabled;
                }
                else
                {
                    this.m_HoverParameters = null;
                }
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Pitch)
            {
                if (parameters != null)
                {
                    if (this.m_PitchParameters == null)
                    {
                        this.m_PitchParameters = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                        this.m_PitchParameters.pidAxis = axis;
                    }
                    this.m_PitchParameters.kP = parameters.kP;
                    this.m_PitchParameters.kI = parameters.kI;
                    this.m_PitchParameters.kD = parameters.kD;
                    this.m_PitchParameters.debug = parameters.debug;
                    this.m_PitchParameters.enabled = parameters.enabled;
                }
                else
                {
                    this.m_PitchParameters = null;
                }
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Roll)
            {
                if (parameters != null)
                {
                    if (this.m_RollParameters == null)
                    {
                        this.m_RollParameters = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                        this.m_RollParameters.pidAxis = axis;
                    }
                    this.m_RollParameters.kP = parameters.kP;
                    this.m_RollParameters.kI = parameters.kI;
                    this.m_RollParameters.kD = parameters.kD;
                    this.m_RollParameters.debug = parameters.debug;
                    this.m_RollParameters.enabled = parameters.enabled;
                }
                else
                {
                    this.m_RollParameters = null;
                }
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Yaw)
            {
                if (parameters != null)
                {
                    if (this.m_YawParameters == null)
                    {
                        this.m_YawParameters = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                        this.m_YawParameters.pidAxis = axis;
                    }
                    this.m_YawParameters.kP = parameters.kP;
                    this.m_YawParameters.kI = parameters.kI;
                    this.m_YawParameters.kD = parameters.kD;
                    this.m_YawParameters.debug = parameters.debug;
                    this.m_YawParameters.enabled = parameters.enabled;
                }
                else
                {
                    this.m_YawParameters = null;
                }
            }
        }
        // only called on deserialization, so overwriting is fine
        // ASSUMES OVERWRITE IS FINE
        private void OnUpdateAllParameters()
        {
            PIDController.GlobalDebugPrint($"ModulePID.OnUpdateAllParameters {this.block.name}");
            this.attachedPID.OnUpdateParameters(this);
        }

        public void OnGUIUpdateHover() {
            PIDController.GlobalDebugPrint($"ModulePID.OnGUIUpdateHover {this.block.name}");
            this.attachedPID.OnUpdateHoverParameters(this);
        }
        public void OnGUIUpdateStrafe()
        {
            PIDController.GlobalDebugPrint($"ModulePID.OnGUIUpdateStrafe {this.block.name}");
            this.attachedPID.OnUpdateStrafeParameters(this);
        }
        public void OnGUIUpdateAccel()
        {
            PIDController.GlobalDebugPrint($"ModulePID.OnGUIUpdateAccel {this.block.name}");
            this.attachedPID.OnUpdateAccelParameters(this);
        }
        public void OnGUIUpdatePitch()
        {
            PIDController.GlobalDebugPrint($"ModulePID.OnGUIUpdatePitch {this.block.name}");
            this.attachedPID.OnUpdatePitchParameters(this);
        }
        public void OnGUIUpdateRoll()
        {
            PIDController.GlobalDebugPrint($"ModulePID.OnGUIUpdateRoll {this.block.name}");
            this.attachedPID.OnUpdateRollParameters(this);
        }
        public void OnGUIUpdateYaw()
        {
            PIDController.GlobalDebugPrint($"ModulePID.OnGUIUpdateYaw {this.block.name}");
            this.attachedPID.OnUpdateYawParameters(this);
        }
        #endregion UpdateParameters

        #region Reset_Error
        public void OnResetHoverError()
        {
            PIDController.GlobalDebugPrint($"ModulePID.OnResetHoverError {this.block.name}");
            if (this.attachedPID != null)
            {
                this.attachedPID.ResetHoverError();
            }
        }
        public void OnResetStrafeError()
        {
            PIDController.GlobalDebugPrint($"ModulePID.OnResetStrafeError {this.block.name}");
            if (this.attachedPID != null)
            {
                this.attachedPID.ResetStrafeError();
            }
        }
        public void OnResetAccelError()
        {
            PIDController.GlobalDebugPrint($"ModulePID.OnResetAccelError {this.block.name}");
            if (this.attachedPID != null)
            {
                this.attachedPID.ResetAccelError();
            }
        }
        public void OnResetPitchError()
        {
            PIDController.GlobalDebugPrint($"ModulePID.OnResetPitchError {this.block.name}");
            if (this.attachedPID != null)
            {
                this.attachedPID.ResetPitchError();
            }
        }
        public void OnResetRollError()
        {
            PIDController.GlobalDebugPrint($"ModulePID.OnResetRollError {this.block.name}");
            if (this.attachedPID != null)
            {
                this.attachedPID.ResetRollError();
            }
        }
        public void OnResetYawError()
        {
            PIDController.GlobalDebugPrint($"ModulePID.OnResetYawError {this.block.name}");
            if (this.attachedPID != null)
            {
                this.attachedPID.ResetYawError();
            }
        }
        #endregion Reset_Error

        public void OnPool() {
            this.availableAxesMask = 0;
            foreach (PIDController.PIDParameters.PIDAxis axis in Enum.GetValues(typeof(PIDController.PIDParameters.PIDAxis)))
            {
                // Register changes for the pid's axes to all other pids
                if (this.ContainsPidAxis(axis))
                {
                    PIDController.GlobalDebugPrint($"ModulePID - register axis {axis} {this.block.name}");
                    this.AddAxis(axis);
                }
            }
            block.AttachEvent.Subscribe(new Action(this.OnAttach));
            block.DetachEvent.Subscribe(new Action(this.OnDetach));
            base.block.serializeEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
            base.block.serializeTextEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerializeText));
        }

        private void OnAttach()
        {
            this.attachedPID = base.block.tank.GetComponent<PIDController>();
            if (this.attachedPID == null)
            {
                this.attachedPID = base.block.tank.gameObject.AddComponent(typeof(PIDController)) as PIDController;
                this.attachedPID.ForceSpawn(base.block.tank);
            }
            this.attachedPID.RegisterPID(this);
            if ((this.availableAxesMask & PIDController.PIDParameters.AxisMask(PIDController.PIDParameters.PIDAxis.Strafe)) != 0)
            {
                base.block.tank.control.AddThrottleControlEnabler(new Vector3(1, 0, 0));
            }
            if ((this.availableAxesMask & PIDController.PIDParameters.AxisMask(PIDController.PIDParameters.PIDAxis.Hover)) != 0)
            {
                base.block.tank.control.AddThrottleControlEnabler(new Vector3(0, 1, 0));
            }
            if ((this.availableAxesMask & PIDController.PIDParameters.AxisMask(PIDController.PIDParameters.PIDAxis.Accel)) != 0)
            {
                base.block.tank.control.AddThrottleControlEnabler(new Vector3(0, 0, 1));
            }
        }

        private const string formatter = "{0,10}{1,13}";

        // Convert a short argument to a byte array and display it.
        private static void GetBytesInt32(int argument)
        {
            byte[] byteArray = BitConverter.GetBytes(argument);
            Console.WriteLine(formatter, argument,
                BitConverter.ToString(byteArray));
        }

        private void OnDetach()
        {
            this.attachedPID.UnregisterPID(this);
            this.attachedPID = null;

            GetBytesInt32(this.availableAxesMask);

            foreach (PIDController.PIDParameters.PIDAxis axis in Enum.GetValues(typeof(PIDController.PIDParameters.PIDAxis)))
            {
                if (!this.MatchesAxis(axis))
                {
                    PIDController.GlobalDebugPrint($"ModulePID Clearing {axis} {this.block.name}");
                    this.ClearParametersByAxis(axis);
                }
            }

            if (this.MatchesAxis(PIDController.PIDParameters.PIDAxis.Strafe))
            {
                base.block.tank.control.RemoveThrottleControlEnabler(new Vector3(1, 0, 0));
            }
            if (this.MatchesAxis(PIDController.PIDParameters.PIDAxis.Hover))
            {
                base.block.tank.control.RemoveThrottleControlEnabler(new Vector3(0, 1, 0));
            }
            if (this.MatchesAxis(PIDController.PIDParameters.PIDAxis.Accel))
            {
                base.block.tank.control.RemoveThrottleControlEnabler(new Vector3(0, 0, 1));
            }
        }

        public static ModulePID.SerialData.PIDParameters ConvertOnSerialize(PIDController.PIDParameters parameters)
        {
            if (parameters == null)
            {
                return new ModulePID.SerialData.PIDParameters
                {
                    hasData = false
                };
            }
            else
            {
                return new ModulePID.SerialData.PIDParameters
                {
                    kP = parameters.kP,
                    kI = parameters.kI,
                    kD = parameters.kD,
                    debug = parameters.debug,
                    enabled = parameters.enabled,
                    hasData = true
                };
            }
        }

        public static PIDController.PIDParameters ConvertOnDeserialize(ModulePID.SerialData.PIDParameters parameters, PIDController.PIDParameters.PIDAxis axis)
        {
            Console.WriteLine("Deserialization Output: " + parameters.ToString());
            if (parameters.hasData)
            {
                PIDController.PIDParameters controllerParams = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                controllerParams.pidAxis = axis;

                controllerParams.lastError = 0.0f;
                controllerParams.cumulativeError = 0.0f;

                controllerParams.kP = parameters.kP;
                controllerParams.kI = parameters.kI;
                controllerParams.kD = parameters.kD;

                controllerParams.debug = parameters.debug;
                controllerParams.enabled = parameters.enabled;
                return controllerParams;
            }
            return null;
        }

        private void AssignParametersByAxis(PIDController.PIDParameters parameters, PIDController.PIDParameters.PIDAxis axis, bool force)
        {
            if (axis == PIDController.PIDParameters.PIDAxis.Accel)
            {
                PIDController.GlobalDebugPrint("ModulePID - Force Replace Accel {this.block.name}");
                if (this.m_AccelParameters != null || force) this.m_AccelParameters = parameters;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Strafe)
            {
                PIDController.GlobalDebugPrint("ModulePID - Force Replace Strafe {this.block.name}");
                if (this.m_StrafeParameters != null || force) this.m_StrafeParameters = parameters;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Hover)
            {
                PIDController.GlobalDebugPrint("ModulePID - Force Replace Hover {this.block.name}");
                if (this.m_HoverParameters != null || force) this.m_HoverParameters = parameters;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Pitch)
            {
                PIDController.GlobalDebugPrint("ModulePID - Force Replace Pitch {this.block.name}");
                if (this.m_PitchParameters != null || force) this.m_PitchParameters = parameters;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Roll)
            {
                PIDController.GlobalDebugPrint("ModulePID - Force Replace Roll {this.block.name}");
                if (this.m_RollParameters != null || force) this.m_RollParameters = parameters;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Yaw)
            {
                PIDController.GlobalDebugPrint("ModulePID - Force Replace Yaw {this.block.name}");
                if (this.m_YawParameters != null || force) this.m_YawParameters = parameters;
            }
        }

        private void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
        {
            string saveTxt = saving ? "Save" : "Load";
            PIDController.GlobalDebugPrint($"ModulePID OnSerialize {saveTxt} {this.block.name}");
            if (saving)
            {
                new ModulePID.SerialData
                {
                    _targetHeight = this.targetHeight,
                    _manualChangeRate = this.manualTargetChangeRate,
                    _useTargetHeight = this.useTargetHeight,
                    _enableHoldPosition = this.enableHoldPosition,

                    _targetRoll = this.targetRoll,
                    _targetPitch = this.targetPitch,

                    _AccelParameters = ModulePID.ConvertOnSerialize(this.m_AccelParameters),
                    _HoverParameters = ModulePID.ConvertOnSerialize(this.m_HoverParameters),
                    _StrafeParameters = ModulePID.ConvertOnSerialize(this.m_StrafeParameters),
                    _PitchParameters = ModulePID.ConvertOnSerialize(this.m_PitchParameters),
                    _RollParameters = ModulePID.ConvertOnSerialize(this.m_RollParameters),
                    _YawParameters = ModulePID.ConvertOnSerialize(this.m_YawParameters)
                }.Store(blockSpec.saveState);
                return;
            }
            else
            {
                ModulePID.SerialData serialData = Module.SerialData<ModulePID.SerialData>.Retrieve(blockSpec.saveState);
                if (serialData != null)
                {
                    this.targetHeight = serialData._targetHeight;
                    this.manualTargetChangeRate = serialData._manualChangeRate;
                    this.enableHoldPosition = serialData._enableHoldPosition;

                    this.useTargetHeight = serialData._useTargetHeight;

                    this.targetPitch = serialData._targetPitch;
                    this.targetRoll = serialData._targetRoll;

                    this.AssignParametersByAxis(ModulePID.ConvertOnDeserialize(serialData._AccelParameters, PIDController.PIDParameters.PIDAxis.Accel), PIDController.PIDParameters.PIDAxis.Accel, false);
                    this.AssignParametersByAxis(ModulePID.ConvertOnDeserialize(serialData._StrafeParameters, PIDController.PIDParameters.PIDAxis.Strafe), PIDController.PIDParameters.PIDAxis.Strafe, false);
                    this.AssignParametersByAxis(ModulePID.ConvertOnDeserialize(serialData._HoverParameters, PIDController.PIDParameters.PIDAxis.Hover), PIDController.PIDParameters.PIDAxis.Hover, false);
                    this.AssignParametersByAxis(ModulePID.ConvertOnDeserialize(serialData._PitchParameters, PIDController.PIDParameters.PIDAxis.Pitch), PIDController.PIDParameters.PIDAxis.Pitch, false);
                    this.AssignParametersByAxis(ModulePID.ConvertOnDeserialize(serialData._RollParameters, PIDController.PIDParameters.PIDAxis.Roll), PIDController.PIDParameters.PIDAxis.Roll, false);
                    this.AssignParametersByAxis(ModulePID.ConvertOnDeserialize(serialData._YawParameters, PIDController.PIDParameters.PIDAxis.Yaw), PIDController.PIDParameters.PIDAxis.Yaw, false);

                    this.OnUpdateAllParameters();
                }
            }
        }

        private void OnSerializeText(bool saving, TankPreset.BlockSpec context)
        {
            string saveTxt = saving ? "Save" : "Load";
            PIDController.GlobalDebugPrint($"ModulePID OnSerializeText {saveTxt} {this.block.name}");
            if (saving)
            {
                context.Store(base.GetType(), "_targetHeight", this.targetHeight.ToString(CultureInfo.InvariantCulture));
                context.Store(base.GetType(), "_manualChangeRate", this.manualTargetChangeRate.ToString(CultureInfo.InvariantCulture));

                context.Store(base.GetType(), "_targetPitch", this.targetPitch.ToString(CultureInfo.InvariantCulture));
                context.Store(base.GetType(), "_targetRoll", this.targetRoll.ToString(CultureInfo.InvariantCulture));
                context.Store(base.GetType(), "_useTargetHeight", this.useTargetHeight.ToString(CultureInfo.InvariantCulture));
                context.Store(base.GetType(), "_enableHoldPosition", this.enableHoldPosition.ToString(CultureInfo.InvariantCulture));

                context.Store(base.GetType(), "_AccelParameters", ModulePID.ConvertOnSerialize(this.m_AccelParameters).ToString(CultureInfo.InvariantCulture));
                context.Store(base.GetType(), "_HoverParameters", ModulePID.ConvertOnSerialize(this.m_HoverParameters).ToString(CultureInfo.InvariantCulture));
                context.Store(base.GetType(), "_StrafeParameters", ModulePID.ConvertOnSerialize(this.m_StrafeParameters).ToString(CultureInfo.InvariantCulture));
                context.Store(base.GetType(), "_PitchParameters", ModulePID.ConvertOnSerialize(this.m_PitchParameters).ToString(CultureInfo.InvariantCulture));
                context.Store(base.GetType(), "_RollParameters", ModulePID.ConvertOnSerialize(this.m_RollParameters).ToString(CultureInfo.InvariantCulture));
                context.Store(base.GetType(), "_YawParameters", ModulePID.ConvertOnSerialize(this.m_YawParameters).ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                #region _targetHeight
                {
                    string text = context.Retrieve(base.GetType(), "_targetHeight");
                    if (!text.NullOrEmpty())
                    {
                        float value;
                        if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                        {
                            if (value > 0)
                            {
                                this.targetHeight = value;
                            }
                            else
                            {
                                d.LogError(string.Concat(new string[]
                                {
                                "ModuleHoverPID.OnSerializeText - Failed to parse _targetHeight setting from save data on block '",
                                base.block.name,
                                "'. Expected positive float value but got '",
                                text,
                                "'. Setting to default value of 50!"
                                }));
                                this.targetHeight = 50f;
                            }
                        }
                        else
                        {
                            d.LogError(string.Concat(new string[]
                            {
                            "ModuleHoverPID.OnSerializeText - Failed to parse _targetHeight setting from save data on block '",
                            base.block.name,
                            "'. Expected positive float value but got '",
                            text,
                            "'. Setting to default value of 50!"
                            }));
                            this.targetHeight = 50f;
                        }
                    }
                    else
                    {
                        this.targetHeight = 50f;
                    }
                }
                #endregion _targetHeight

                #region _manualChangeRate
                {
                    string manualChangeRateStr = context.Retrieve(base.GetType(), "_manualChangeRate");
                    if (!manualChangeRateStr.NullOrEmpty())
                    {
                        float value;
                        if (float.TryParse(manualChangeRateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                        {
                            if (value > 0)
                            {
                                this.manualTargetChangeRate = value;
                            }
                            else
                            {
                                d.LogError(string.Concat(new string[]
                                {
                                "ModuleHoverPID.OnSerializeText - Failed to parse _manualChangeRate setting from save data on block '",
                                base.block.name,
                                "'. Expected positive float value but got '",
                                manualChangeRateStr,
                                "'. Setting to default value of 20!"
                                }));
                                this.manualTargetChangeRate = 20f;
                            }
                        }
                        else
                        {
                            d.LogError(string.Concat(new string[]
                            {
                            "ModuleHoverPID.OnSerializeText - Failed to parse _manualChangeRate setting from save data on block '",
                            base.block.name,
                            "'. Expected positive float value but got '",
                            manualChangeRateStr,
                            "'. Setting to default value of 20!"
                            }));
                            this.manualTargetChangeRate = 20f;
                        }
                    }
                    else
                    {
                        this.manualTargetChangeRate = 20f;
                    }
                }
                #endregion _manualChangeRate

                #region _useTargetHeight
                {
                    string useTargetHeightStr = context.Retrieve(base.GetType(), "_useTargetHeight");
                    if (!useTargetHeightStr.NullOrEmpty())
                    {
                        bool value;
                        if (bool.TryParse(useTargetHeightStr, out value))
                        {
                            this.useTargetHeight = value;
                        }
                        else
                        {
                            d.LogError(string.Concat(new string[]
                            {
                            "ModuleHoverPID.OnSerializeText - Failed to parse _useTargetHeight setting from save data on block '",
                            base.block.name,
                            "'. Expected bool but got '",
                            useTargetHeightStr,
                            "'. Setting to default value of False"
                            }));
                            this.useTargetHeight = false;
                        }
                    }
                    else
                    {
                        this.useTargetHeight = false;
                    }
                }
                #endregion _useTargetHeight

                #region _enableHoldPosition
                {
                    string useTargetHeightStr = context.Retrieve(base.GetType(), "_enableHoldPosition");
                    if (!useTargetHeightStr.NullOrEmpty())
                    {
                        bool value;
                        if (bool.TryParse(useTargetHeightStr, out value))
                        {
                            this.enableHoldPosition = value;
                        }
                        else
                        {
                            d.LogError(string.Concat(new string[]
                            {
                            "ModuleHoverPID.OnSerializeText - Failed to parse _enableHoldPosition setting from save data on block '",
                            base.block.name,
                            "'. Expected bool but got '",
                            useTargetHeightStr,
                            "'. Setting to default value of False"
                            }));
                            this.enableHoldPosition = false;
                        }
                    }
                    else
                    {
                        this.enableHoldPosition = false;
                    }
                }
                #endregion _enableHoldPosition

                #region _targetPitch
                {
                    string text = context.Retrieve(base.GetType(), "_targetPitch");
                    if (!text.NullOrEmpty())
                    {
                        float value;
                        if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                        {
                            this.targetPitch = value;
                        }
                        else
                        {
                            d.LogError(string.Concat(new string[]
                            {
                            "ModuleHoverPID.OnSerializeText - Failed to parse _targetPitch setting from save data on block '",
                            base.block.name,
                            "'. Expected float value but got '",
                            text,
                            "'. Setting to default value of 0!"
                            }));
                            this.targetPitch = 0f;
                        }
                    }
                    else
                    {
                        this.targetPitch = 0f;
                    }
                }
                #endregion _targetPitch

                #region _targetRoll
                {
                    string text = context.Retrieve(base.GetType(), "_targetRoll");
                    if (!text.NullOrEmpty())
                    {
                        float value;
                        if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                        {
                            this.targetRoll = value;
                        }
                        else
                        {
                            d.LogError(string.Concat(new string[]
                            {
                            "ModuleHoverPID.OnSerializeText - Failed to parse _targetRoll setting from save data on block '",
                            base.block.name,
                            "'. Expected float value but got '",
                            text,
                            "'. Setting to default value of 0!"
                            }));
                            this.targetRoll = 0f;
                        }
                    }
                    else
                    {
                        this.targetRoll = 0f;
                    }
                }
                #endregion _targetRoll

                #region PIDParameters
                {
                    #region AccelParameters
                    {
                        string parameterStr = context.Retrieve(base.GetType(), "_AccelParameters");
                        ModulePID.SerialData.PIDParameters parameters = ModulePID.SerialData.PIDParameters.FromString(parameterStr);
                        this.AssignParametersByAxis(ModulePID.ConvertOnDeserialize(parameters, PIDController.PIDParameters.PIDAxis.Accel), PIDController.PIDParameters.PIDAxis.Accel, false);
                    }
                    #endregion AccelParameters

                    #region StrafeParameters
                    {
                        string parameterStr = context.Retrieve(base.GetType(), "_StrafeParameters");
                        ModulePID.SerialData.PIDParameters parameters = ModulePID.SerialData.PIDParameters.FromString(parameterStr);
                        this.AssignParametersByAxis(ModulePID.ConvertOnDeserialize(parameters, PIDController.PIDParameters.PIDAxis.Strafe), PIDController.PIDParameters.PIDAxis.Strafe, false);
                    }
                    #endregion StrafeParameters

                    #region PitchParameters
                    {
                        string parameterStr = context.Retrieve(base.GetType(), "_PitchParameters");
                        ModulePID.SerialData.PIDParameters parameters = ModulePID.SerialData.PIDParameters.FromString(parameterStr);
                        this.AssignParametersByAxis(ModulePID.ConvertOnDeserialize(parameters, PIDController.PIDParameters.PIDAxis.Pitch), PIDController.PIDParameters.PIDAxis.Pitch, false);
                    }
                    #endregion PitchParameters

                    #region RollParameters
                    {
                        string parameterStr = context.Retrieve(base.GetType(), "_RollParameters");
                        ModulePID.SerialData.PIDParameters parameters = ModulePID.SerialData.PIDParameters.FromString(parameterStr);
                        this.AssignParametersByAxis(ModulePID.ConvertOnDeserialize(parameters, PIDController.PIDParameters.PIDAxis.Roll), PIDController.PIDParameters.PIDAxis.Roll, false);
                    }
                    #endregion RollParameters

                    #region YawParameters
                    {
                        string parameterStr = context.Retrieve(base.GetType(), "_YawParameters");
                        ModulePID.SerialData.PIDParameters parameters = ModulePID.SerialData.PIDParameters.FromString(parameterStr);
                        this.AssignParametersByAxis(ModulePID.ConvertOnDeserialize(parameters, PIDController.PIDParameters.PIDAxis.Yaw), PIDController.PIDParameters.PIDAxis.Yaw, false);
                    }
                    #endregion YawParameters

                    #region HoverParameters
                    {
                        string parameterStr = context.Retrieve(base.GetType(), "_HoverParameters");
                        ModulePID.SerialData.PIDParameters parameters = ModulePID.SerialData.PIDParameters.FromString(parameterStr);
                        this.AssignParametersByAxis(ModulePID.ConvertOnDeserialize(parameters, PIDController.PIDParameters.PIDAxis.Hover), PIDController.PIDParameters.PIDAxis.Hover, false);
                    }
                    #endregion HoverParameters
                }
                #endregion PIDParameters

                this.OnUpdateAllParameters();
            }
        }

        [Serializable]
        public new class SerialData : Module.SerialData<ModulePID.SerialData>
        {
            // Token: 0x0400457C RID: 17788
            public float _targetHeight;
            public float _manualChangeRate;

            public bool _useTargetHeight;
            public bool _enableHoldPosition;

            public float _targetPitch;
            public float _targetRoll;

            public ModulePID.SerialData.PIDParameters _AccelParameters;
            public ModulePID.SerialData.PIDParameters _HoverParameters;
            public ModulePID.SerialData.PIDParameters _StrafeParameters;

            public ModulePID.SerialData.PIDParameters _PitchParameters;
            public ModulePID.SerialData.PIDParameters _RollParameters;
            public ModulePID.SerialData.PIDParameters _YawParameters;

            [Serializable]
            public struct PIDParameters
            {
                /// <value>contains the Proportional error coefficient</value>
                public float kP;
                /// <value>contains the Integrative error coefficient</value>
                public float kI;
                /// <value>contains the Derivative error coefficient</value>
                public float kD;
                /// <value>flag to enable debug printing or not</value>
                public bool debug;
                /// <value>flag for disabling this particular pid</value>
                public bool enabled;
                /// <value>flag for disabling this particular pid</value>
                public bool hasData;
                
                public string ToString(System.Globalization.CultureInfo _)
                {
                    return $"({kP.ToString(CultureInfo.InvariantCulture)}:{kI.ToString(CultureInfo.InvariantCulture)}:{kD.ToString(CultureInfo.InvariantCulture)}:{debug.ToString(CultureInfo.InvariantCulture)}:{enabled.ToString(CultureInfo.InvariantCulture)}:{hasData.ToString(CultureInfo.InvariantCulture)})";
                }

                public static ModulePID.SerialData.PIDParameters FromString(string inputStr)
                {
                    PIDController.GlobalDebugPrint("DESERIALIZE INPUT: " + inputStr);
                    string[] data = inputStr.Replace("(", "").Replace(")", "").Split(':');
                    Console.Write(data.ToString());
                    int length = data.Length;

                    #region DataValidityCheck
                    if (length != 6)
                    {
                        d.LogError("Data of bad length! Returning default (empty, disabled)");
                        return new PIDParameters { hasData = false };
                    }
                    #endregion DataValidityCheck

                    #region initialDataCheck
                    bool _hasData = false;
                    {
                        string hasDataStr = data[5];
                        if (!hasDataStr.NullOrEmpty())
                        {
                            bool value;
                            if (bool.TryParse(hasDataStr, out value))
                            {
                                _hasData = value;
                            }
                            else
                            {
                                d.LogError(string.Concat(new string[]
                                {
                                    "ModuleHoverPID.OnSerializeText - Failed to parse _hasData setting from save data on block. Expected bool but got '",
                                    hasDataStr,
                                    "'. Setting to default value of False"
                                }));
                            }
                        }
                    }
                    #endregion initialDataCheck

                    if (_hasData)
                    {
                        #region ErrorParams
                        float _kP = 0f;
                        {
                            string kPStr = data[0];
                            if (!kPStr.NullOrEmpty())
                            {
                                float value;
                                if (float.TryParse(kPStr, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                                {
                                    _kP = value;
                                }
                                else
                                {
                                    d.LogError(string.Concat(new string[]
                                    {
                                    "ModuleHoverPID.OnSerializeText - Failed to parse _kP setting from save data on block. Expected float but got '",
                                    kPStr,
                                    "'. Setting to default value of 0.0"
                                    }));
                                }
                            }
                        }
                        float _kI = 0f;
                        {
                            string kIStr = data[1];
                            if (!kIStr.NullOrEmpty())
                            {
                                float value;
                                if (float.TryParse(kIStr, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                                {
                                    _kI = value;
                                }
                                else
                                {
                                    d.LogError(string.Concat(new string[]
                                    {
                                    "ModuleHoverPID.OnSerializeText - Failed to parse _kI setting from save data on block. Expected float but got '",
                                    kIStr,
                                    "'. Setting to default value of 0.0"
                                    }));
                                }
                            }
                        }
                        float _kD = 0f;
                        {
                            string kDStr = data[2];
                            if (!kDStr.NullOrEmpty())
                            {
                                float value;
                                if (float.TryParse(kDStr, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                                {
                                    _kD = value;
                                }
                                else
                                {
                                    d.LogError(string.Concat(new string[]
                                    {
                                    "ModuleHoverPID.OnSerializeText - Failed to parse _kD setting from save data on block. Expected float but got '",
                                    kDStr,
                                    "'. Setting to default value of 0.0"
                                    }));
                                }
                            }
                        }
                        #endregion ErrorParams

                        #region BoolParams
                        bool _debug = false;
                        {
                            string constantStr = data[3];
                            if (!constantStr.NullOrEmpty())
                            {
                                bool value;
                                if (bool.TryParse(constantStr, out value))
                                {
                                    _debug = value;
                                }
                                else
                                {
                                    d.LogError(string.Concat(new string[]
                                    {
                                    "ModuleHoverPID.OnSerializeText - Failed to parse _debug setting from save data on block. Expected bool but got '",
                                    constantStr,
                                    "'. Setting to default value of False"
                                    }));
                                }
                            }
                        }
                        bool _enabled = false;
                        {
                            string constantStr = data[4];
                            if (!constantStr.NullOrEmpty())
                            {
                                bool value;
                                if (bool.TryParse(constantStr, out value))
                                {
                                    _enabled = value;
                                }
                                else
                                {
                                    d.LogError(string.Concat(new string[]
                                    {
                                    "ModuleHoverPID.OnSerializeText - Failed to parse _enabled setting from save data on block. Expected bool but got '",
                                    constantStr,
                                    "'. Setting to default value of False"
                                    }));
                                }
                            }
                        }
                        #endregion BoolParams

                        return new PIDParameters {
                            kP = _kP,
                            kI = _kI,
                            kD = _kD,
                            debug = _debug,
                            enabled = _enabled,
                            hasData = _hasData
                        };
                    }
                    return new PIDParameters { hasData = false };
                }
            }
        }
    }
}
