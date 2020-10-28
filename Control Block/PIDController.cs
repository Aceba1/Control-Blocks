using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;

// compile with: -doc:DocFileName.xml

namespace Control_Block
{
    /// <summary>
    /// Class that does the actual PID calculations for the throttle.
    /// </summary>
    /// <remarks> <para> Limited to one per <see cref="Tank"/>. On changing parameters through the UI, it overwrites the parameters here, and on all <see cref="ModulePID"/> modules on the tech. </para></remarks>
    [DefaultExecutionOrder(Int32.MaxValue)]
    public class PIDController : TechComponent
    {
        #region PatchFields
        /// <value>FieldInfo to fetch the list of all <see cref="ModuleBooster"/> modules attached to a <see cref="Tank"/> via reflection</value>
        private static FieldInfo m_BoosterModules = typeof(TechBooster).GetField("m_BoosterModules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        /// <value>FieldInfo to fetch the list of all <see cref="ModuleLinearMotionEngine"/> modules attached to a <see cref="Tank"/> via reflection</value>
        private static FieldInfo m_LinearMotionEngines = typeof(TechAudio).GetField("m_LinearMotionEngines", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        /// <value>FieldInfo to fetch the value of a <see cref="Tank"/>'s throttle via reflection</value>
        private static FieldInfo m_Throttle = typeof(TankControl).GetField("m_ThrottleValues", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        /// <value>FieldInfo to fetch the value of a <see cref="Tank"/>'s <see cref="TankControl.ControlState"/> via reflection</value>
        private static FieldInfo m_ControlState = typeof(TankControl).GetField("m_ControlState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        #endregion PatchFields

        #region PIDManagement

        #region PID_List
        /// <value>List to contain all Hover <see cref="ModulePID"/> associated with the parent <see cref="Tank"/></value>
        private HashSet<ModulePID> m_PIDModules = new HashSet<ModulePID>();
        private HashSet<ModulePID> m_HoverPIDModules = new HashSet<ModulePID>();
        private HashSet<ModulePID> m_StrafePIDModules = new HashSet<ModulePID>();
        private HashSet<ModulePID> m_AccelPIDModules = new HashSet<ModulePID>();
        private HashSet<ModulePID> m_PitchPIDModules = new HashSet<ModulePID>();
        private HashSet<ModulePID> m_RollPIDModules = new HashSet<ModulePID>();
        private HashSet<ModulePID> m_YawPIDModules = new HashSet<ModulePID>();
        #endregion PID_List

        #region PID_Parameters
        public PIDParameters HoverPID;
        public PIDParameters StrafePID;
        public PIDParameters AccelPID;
        public PIDParameters PitchPID;
        public PIDParameters RollPID;
        public PIDParameters YawPID;

        public float targetHeight, manualTargetChangeRate, targetPitch, targetRoll;
        public bool staticHeight, useTargetHeight, enableHoldPosition;

        [Serializable]
        public class PIDParameters : ScriptableObject
        {
            [SerializeField]
            /// <value>which axis does this PID control</value>
            public PIDAxis pidAxis;

            [SerializeField]
            /// <value>contains the observed error from the last FixedUpdate</value>
            public float lastError;

            [SerializeField]
            /// <value>contains the cumulative error since the last error reset</value>
            public float cumulativeError;

            [SerializeField]
            /// <value>contains the Proportional error coefficient</value>
            public float kP;

            [SerializeField]
            /// <value>contains the Integrative error coefficient</value>
            public float kI;

            [SerializeField]
            /// <value>contains the Derivative error coefficient</value>
            public float kD;

            [SerializeField]
            /// <value>flag to enable debug printing or not</value>
            public bool debug;

            [SerializeField]
            /// <value>flag for disabling this particular pid</value>
            public bool enabled;

            public enum PIDAxis
            {
                Strafe = 0,
                Hover = 1,
                Accel = 2,
                Pitch = 3,
                Yaw = 4,
                Roll = 5
            }

            public static int AxisMask(PIDAxis axis)
            {
                return 1 << ((int)axis);
            }

            public void ResetError()
            {
                // PIDController.GlobalDebugPrint($"PIDParameters.ResetError {this.pidAxis}");
                this.lastError = 0f;
                this.cumulativeError = 0f;
            }

            public void DebugPrint(String text)
            {
                if (this.debug)
                {
                    Console.WriteLine(text);
                }
            }

            public float UpdateStep(float newError, ref string overridePrint, string prefix="", string postfix="")
            {
                float dt = Time.fixedDeltaTime;
                this.cumulativeError += newError;

                float _kP = this.kP * newError;
                float _kI = this.kI * this.cumulativeError * dt;
                float _kD = this.kD * (newError - this.lastError) / dt;
                this.lastError = newError;
                float _out = _kP + _kI + _kD;

                string mainContent = $"K_P: ({this.kP}), P: [{_kP}], K_I: ({this.kI}), I: [{_kI}], K_D: ({this.kD}), D: [{_kD}], PID_OUT: [{_out}]";
                string printStr = $" | {this.pidAxis} PID |  {prefix}{mainContent}{postfix}";
                if (overridePrint != null)
                {
                    this.DebugPrint(printStr);
                }
                else
                {
                    overridePrint = printStr;
                }
                return _out;
            }
        }

        public static PIDParameters GenerateParameterInstance(PIDParameters.PIDAxis axis, float kP, float kI, float kD, bool debug, bool enabled)
        {
            PIDController.PIDParameters parameters = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
            parameters.pidAxis = axis;
            parameters.kP = kP;
            parameters.kI = kI;
            parameters.kD = kD;
            parameters.lastError = 0f;
            parameters.cumulativeError = 0f;
            parameters.debug = debug;
            parameters.enabled = enabled;
            return parameters;
        }

        #endregion PID_Parameters

        private int currAxisMask = 0;

        private void NullPIDByAxis(PIDController.PIDParameters.PIDAxis axis)
        {
            if (axis == PIDController.PIDParameters.PIDAxis.Accel)
            {
                Destroy(this.AccelPID);
                this.AccelPID = null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Strafe)
            {
                Destroy(this.StrafePID);
                this.StrafePID = null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Hover)
            {
                Destroy(this.HoverPID);
                this.HoverPID = null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Pitch)
            {
                Destroy(this.PitchPID);
                this.PitchPID = null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Roll)
            {
                Destroy(this.RollPID);
                this.RollPID = null;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Yaw)
            {
                Destroy(this.YawPID);
                this.YawPID = null;
            }
        }
        private HashSet<ModulePID> FetchModulesByAxis(PIDController.PIDParameters.PIDAxis axis)
        {
            if (axis == PIDController.PIDParameters.PIDAxis.Accel)
            {
                return this.m_AccelPIDModules;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Strafe)
            {
                return this.m_StrafePIDModules;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Hover)
            {
                return this.m_HoverPIDModules;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Pitch)
            {
                return this.m_PitchPIDModules;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Roll)
            {
                return this.m_RollPIDModules;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Yaw)
            {
                return this.m_YawPIDModules;
            }
            return null;
        }
        private void AddModulesByAxis(ModulePID pid, PIDController.PIDParameters.PIDAxis axis)
        {
            if (axis == PIDController.PIDParameters.PIDAxis.Accel)
            {
                this.m_AccelPIDModules.Add(pid);
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Strafe)
            {
                this.m_StrafePIDModules.Add(pid);
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Hover)
            {
                this.m_HoverPIDModules.Add(pid);
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Pitch)
            {
                this.m_PitchPIDModules.Add(pid);
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Roll)
            {
                this.m_RollPIDModules.Add(pid);
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Yaw)
            {
                this.m_YawPIDModules.Add(pid);
            }
        }
        private HashSet<ModulePID> RemoveModulesByAxis(ModulePID pid, PIDController.PIDParameters.PIDAxis axis)
        {
            if (axis == PIDController.PIDParameters.PIDAxis.Accel)
            {
                this.m_AccelPIDModules.Remove(pid);
                return this.m_AccelPIDModules;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Strafe)
            {
                this.m_StrafePIDModules.Remove(pid);
                return this.m_StrafePIDModules;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Hover)
            {
                this.m_HoverPIDModules.Remove(pid);
                return this.m_HoverPIDModules;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Pitch)
            {
                this.m_PitchPIDModules.Remove(pid);
                return this.m_PitchPIDModules;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Roll)
            {
                this.m_RollPIDModules.Remove(pid);
                return this.m_RollPIDModules;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Yaw)
            {
                this.m_YawPIDModules.Remove(pid);
                return this.m_YawPIDModules;
            }
            return null;
        }
        private PIDController.PIDParameters FetchParametersByAxis(PIDController.PIDParameters.PIDAxis axis)
        {
            if (axis == PIDController.PIDParameters.PIDAxis.Accel)
            {
                return this.AccelPID;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Strafe)
            {
                return this.StrafePID;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Hover)
            {
                return this.HoverPID;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Pitch)
            {
                return this.PitchPID;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Roll)
            {
                return this.RollPID;
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Yaw)
            {
                return this.YawPID;
            }
            return null;
        }
        private void InitializeParametersByAxis(PIDController.PIDParameters.PIDAxis axis)
        {
            if (axis == PIDController.PIDParameters.PIDAxis.Accel)
            {
                if (this.AccelPID == null) {
                    this.AccelPID = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                    this.AccelPID.pidAxis = axis;
                }
                else
                {
                    PIDController.GlobalDebugPrint("Accel PID Present");
                }
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Strafe)
            {
                if (this.StrafePID == null)
                {
                    this.StrafePID = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                    this.StrafePID.pidAxis = axis;
                }
                else
                {
                    PIDController.GlobalDebugPrint("Strafe PID Present");
                }
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Hover)
            {
                if (this.HoverPID == null)
                {
                    this.HoverPID = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                    this.HoverPID.pidAxis = axis;
                }
                else
                {
                    PIDController.GlobalDebugPrint("Hover PID Present");
                }
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Pitch)
            {
                if (this.PitchPID == null)
                {
                    this.PitchPID = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                    this.PitchPID.pidAxis = axis;
                }
                else
                {
                    PIDController.GlobalDebugPrint("Pitch PID Present");
                }
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Roll)
            {
                if (this.RollPID == null)
                {
                    this.RollPID = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                    this.RollPID.pidAxis = axis;
                }
                else
                {
                    PIDController.GlobalDebugPrint("Roll PID Present");
                }
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Yaw)
            {
                if (this.YawPID == null)
                {
                    this.YawPID = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                    this.YawPID.pidAxis = axis;
                }
                else
                {
                    PIDController.GlobalDebugPrint("Yaw PID Present");
                }
            }
        }
        private void RegisterPIDAxis(ModulePID pid, PIDController.PIDParameters.PIDAxis axis)
        {
            // fetch first pid
            HashSet<ModulePID> pidList = this.FetchModulesByAxis(axis);
            ModulePID oldPID = null;
            if (pidList != null && pidList.Count > 0)
            {
                HashSet<ModulePID>.Enumerator em = pidList.GetEnumerator();
                em.MoveNext();
                oldPID = em.Current;
            }
            else
            {
                oldPID = pid;
            }
            // add module to axis set
            this.AddModulesByAxis(pid, axis);
            // initialize parameters for axes
            this.InitializeParametersByAxis(axis);
            this.currAxisMask |= PIDController.PIDParameters.AxisMask(axis);
            this.OnUpdateParametersByAxis(oldPID, axis);
        }
        public void RegisterPID(ModulePID pid)
        {
            this.m_PIDModules.Add(pid);
            foreach (PIDParameters.PIDAxis axis in Enum.GetValues(typeof(PIDParameters.PIDAxis)))
            {
                // Register changes for the pid's axes to all other pids
                if (pid.MatchesAxis(axis))
                {
                    PIDController.GlobalDebugPrint($"REGISTER PID - matches axis {axis}");
                    this.RegisterPIDAxis(pid, axis);
                }
                else
                {
                    this.PropagateUpdatedParametersByAxis(axis);
                }
            }
            // Register params from all other pids onto this one
            // pid.OnUpdateAllParameters();
        }
        public void UnregisterPID(ModulePID pid)
        {
            #region SetClean
            this.m_PIDModules.Remove(pid);
            PIDController.GlobalDebugPrint($"UNREGISTER PID {pid.block.name}");
            foreach (PIDParameters.PIDAxis axis in Enum.GetValues(typeof(PIDParameters.PIDAxis)))
            {
                if (pid.MatchesAxis(axis))
                {
                    PIDController.GlobalDebugPrint($"    matches axis {axis}");
                    HashSet<ModulePID> modules = this.RemoveModulesByAxis(pid, axis);
                    if (modules.Count == 0)
                    {
                        PIDController.GlobalDebugPrint($"    NO PID REMAINING - clear axis {axis}");
                        this.NullPIDByAxis(axis);
                        this.currAxisMask &= ~PIDParameters.AxisMask(axis);
                        this.PropagateUpdatedParameters(null, axis);
                    }
                }
            }
            PIDController.GlobalDebugPrint($"PID {pid.block.name} UNREGISTERED");
            #endregion SetClean

            if (this.currAxisMask == 0)
            {
                Destroy(this);
            }
        }
        #endregion PIDManagement

        /// <value>The <see cref="Tank"/> associated with this <see cref="PIDController"/></value>
        public Tank AttachedTank
        {
            get
            {
                return this._tech;
            }
        }
        private Tank _tech;

        #region Globals
        private static readonly RaycastHit[] s_Hits = new RaycastHit[32];
        private static readonly Vector3 m_EffectorDir = new Vector3(0, -1, 0);
        private static readonly bool GlobalDebug = false;
        private static string dummyStr = "";
        private static float degreesPerRadian = 180f / Mathf.PI;

        public static void GlobalDebugPrint(string str)
        {
            if (PIDController.GlobalDebug)
            {
                Console.WriteLine($"[CB-PID] - {str}");
            }
        }
        #endregion Globals

        // calculated thrust force
        #region FixedUpdateParameters
        public Vector3 swiveledThrustPositive = Vector3.zero;
        public Vector3 swiveledThrustNegative = Vector3.zero;

        public Vector3 calculatedThrustPositive = Vector3.zero;
        public Vector3 calculatedThrustNegative = Vector3.zero;

        public Vector3 calculatedTorquePositive = Vector3.zero;
        public Vector3 calculatedTorqueNegative = Vector3.zero;

        public Vector3 nonGravityThrust = Vector3.zero;
        public Vector3 nonManagedTorque = Vector3.zero;
        private Vector3 baseGravity
        {
            get {
                return this.AttachedTank.rbody.mass * Physics.gravity;
            }
        }

        public Vector3 targetPosition = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        #endregion FixedUpdateParameters

        #region UpdateParameters
        // this one only called from ModulePID, force synchronizes everything
        public void OnUpdateParameters(ModulePID pid)
        {
            PIDController.GlobalDebugPrint($"PIDController.OnUpdateParameters {pid.block.name}");
            if (this.AccelPID != null)
            {
                if (pid.m_AccelParameters == null)
                {
                    pid.m_AccelParameters = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                    pid.m_AccelParameters.pidAxis = PIDParameters.PIDAxis.Accel;
                }
                this.OnUpdateAccelParameters(pid);
            }
            if (this.HoverPID != null)
            {
                if (pid.m_HoverParameters == null)
                {
                    pid.m_HoverParameters = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                    pid.m_HoverParameters.pidAxis = PIDParameters.PIDAxis.Hover;
                }
                this.OnUpdateHoverParameters(pid);
            }
            if (this.StrafePID != null)
            {
                if (pid.m_StrafeParameters == null)
                {
                    pid.m_StrafeParameters = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                    pid.m_StrafeParameters.pidAxis = PIDParameters.PIDAxis.Strafe;
                }
                this.OnUpdateStrafeParameters(pid);
            }
            if (this.PitchPID)
            {
                if (pid.m_PitchParameters == null)
                {
                    pid.m_PitchParameters = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                    pid.m_PitchParameters.pidAxis = PIDParameters.PIDAxis.Pitch;
                }
                this.OnUpdatePitchParameters(pid);
            }
            if (this.RollPID)
            {
                if (pid.m_RollParameters == null)
                {
                    pid.m_RollParameters = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                    pid.m_RollParameters.pidAxis = PIDParameters.PIDAxis.Roll;
                }
                this.OnUpdateRollParameters(pid);
            }
            if (this.YawPID)
            {
                if (pid.m_YawParameters == null)
                {
                    pid.m_YawParameters = (PIDController.PIDParameters)ScriptableObject.CreateInstance(typeof(PIDController.PIDParameters));
                    pid.m_YawParameters.pidAxis = PIDParameters.PIDAxis.Yaw;
                }
                this.OnUpdateYawParameters(pid);
            }
        }
        public void OnUpdateParametersByAxis(ModulePID pid, PIDController.PIDParameters.PIDAxis axis)
        {
            PIDController.GlobalDebugPrint($"PIDController.OnUpdateParametersByAxis {pid.block.name}, {axis}");
            if (axis == PIDController.PIDParameters.PIDAxis.Accel)
            {
                this.OnUpdateAccelParameters(pid);
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Strafe)
            {
                this.OnUpdateStrafeParameters(pid);
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Hover)
            {
                this.OnUpdateHoverParameters(pid);
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Pitch)
            {
                this.OnUpdatePitchParameters(pid);
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Roll)
            {
                this.OnUpdateRollParameters(pid);
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Yaw)
            {
                this.OnUpdateYawParameters(pid);
            }
        }
        public void OnUpdateStrafeParameters(ModulePID pid)
        {
            PIDController.GlobalDebugPrint($"PIDController.OnUpdateStrafeParameters {pid.block.name}");
            this.StrafePID.kI = pid.m_StrafeParameters.kI;
            this.StrafePID.kP = pid.m_StrafeParameters.kP;
            this.StrafePID.kD = pid.m_StrafeParameters.kD;
            this.StrafePID.debug = pid.m_StrafeParameters.debug;
            this.StrafePID.enabled = pid.m_StrafeParameters.enabled;

            this.enableHoldPosition = pid.enableHoldPosition;
            this.PropagateUpdatedStrafeParameters();
        }
        public void OnUpdateHoverParameters(ModulePID pid)
        {
            PIDController.GlobalDebugPrint($"PIDController.OnUpdateHoverParameters {pid.block.name}");
            this.HoverPID.kI = pid.m_HoverParameters.kI;
            this.HoverPID.kP = pid.m_HoverParameters.kP;
            this.HoverPID.kD = pid.m_HoverParameters.kD;
            this.HoverPID.debug = pid.m_HoverParameters.debug;
            this.HoverPID.enabled = pid.m_HoverParameters.enabled;

            this.targetHeight = pid.targetHeight;
            this.staticHeight = pid.staticHeight;
            this.useTargetHeight = pid.useTargetHeight;
            this.manualTargetChangeRate = pid.manualTargetChangeRate;
            this.enableHoldPosition = pid.enableHoldPosition;
            this.PropagateUpdatedHoverParameters();
        }
        public void OnUpdateAccelParameters(ModulePID pid)
        {
            PIDController.GlobalDebugPrint($"PIDController.OnUpdateAccelParameters {pid.block.name}");
            this.AccelPID.kI = pid.m_AccelParameters.kI;
            this.AccelPID.kP = pid.m_AccelParameters.kP;
            this.AccelPID.kD = pid.m_AccelParameters.kD;
            this.AccelPID.debug = pid.m_AccelParameters.debug;
            this.AccelPID.enabled = pid.m_AccelParameters.enabled;

            this.enableHoldPosition = pid.enableHoldPosition;
            this.PropagateUpdatedAccelParameters();
        }
        public void OnUpdatePitchParameters(ModulePID pid)
        {
            PIDController.GlobalDebugPrint($"PIDController.OnUpdatePitchParameters {pid.block.name}");
            this.PitchPID.kI = pid.m_PitchParameters.kI;
            this.PitchPID.kP = pid.m_PitchParameters.kP;
            this.PitchPID.kD = pid.m_PitchParameters.kD;
            this.PitchPID.debug = pid.m_PitchParameters.debug;
            this.PitchPID.enabled = pid.m_PitchParameters.enabled;
            
            this.targetPitch = pid.targetPitch;
            this.manualTargetChangeRate = pid.manualTargetChangeRate;
            this.PropagateUpdatedPitchParameters();
        }
        public void OnUpdateRollParameters(ModulePID pid)
        {
            PIDController.GlobalDebugPrint($"PIDController.OnUpdateRollParameters {pid.block.name}");
            this.RollPID.kI = pid.m_RollParameters.kI;
            this.RollPID.kP = pid.m_RollParameters.kP;
            this.RollPID.kD = pid.m_RollParameters.kD;
            this.RollPID.debug = pid.m_RollParameters.debug;
            this.RollPID.enabled = pid.m_RollParameters.enabled;

            this.targetRoll = pid.targetRoll;
            this.manualTargetChangeRate = pid.manualTargetChangeRate;
            this.PropagateUpdatedRollParameters();
        }
        public void OnUpdateYawParameters(ModulePID pid)
        {
            PIDController.GlobalDebugPrint($"PIDController.OnUpdateYawParameters {pid.block.name}");
            this.YawPID.kI = pid.m_YawParameters.kI;
            this.YawPID.kP = pid.m_YawParameters.kP;
            this.YawPID.kD = pid.m_YawParameters.kD;
            this.YawPID.debug = pid.m_YawParameters.debug;
            this.YawPID.enabled = pid.m_YawParameters.enabled;
            this.PropagateUpdatedParameters(this.YawPID, PIDParameters.PIDAxis.Yaw);
        }
        private void PropagateUpdatedParametersByAxis(PIDParameters.PIDAxis axis)
        {
            PIDController.GlobalDebugPrint($"PropagateUpdatedParametersByAxis {axis}");
            if (axis == PIDController.PIDParameters.PIDAxis.Accel)
            {
                this.PropagateUpdatedAccelParameters();
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Strafe)
            {
                this.PropagateUpdatedStrafeParameters();
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Hover)
            {
                this.PropagateUpdatedHoverParameters();
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Pitch)
            {
                this.PropagateUpdatedPitchParameters();
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Roll)
            {
                this.PropagateUpdatedRollParameters();
            }
            else if (axis == PIDController.PIDParameters.PIDAxis.Yaw)
            {
                this.PropagateUpdatedParameters(this.YawPID, PIDParameters.PIDAxis.Yaw);
            }
        }
        public void PropagateUpdatedHoverParameters()
        {
            PIDController.GlobalDebugPrint($"PIDController.PropagateUpdatedHoverParameters");
            foreach (ModulePID pidRef in this.m_PIDModules)
            {
                pidRef.targetHeight = this.targetHeight;
                pidRef.staticHeight = this.staticHeight;
                pidRef.manualTargetChangeRate = this.manualTargetChangeRate;
                pidRef.useTargetHeight = this.useTargetHeight;
                pidRef.enableHoldPosition = this.enableHoldPosition;
                pidRef.OnUpdateParameters(this.HoverPID, PIDParameters.PIDAxis.Hover);
            }
        }
        private void PropagateUpdatedStrafeParameters()
        {
            PIDController.GlobalDebugPrint($"PIDController.PropagateUpdatedStrafeParameters");
            foreach (ModulePID pidRef in this.m_PIDModules)
            {
                pidRef.enableHoldPosition = this.enableHoldPosition;
                pidRef.OnUpdateParameters(this.StrafePID, PIDParameters.PIDAxis.Strafe);
            }
        }
        private void PropagateUpdatedAccelParameters()
        {
            PIDController.GlobalDebugPrint($"PIDController.PropagateUpdatedAccelParameters");
            foreach (ModulePID pidRef in this.m_PIDModules)
            {
                pidRef.enableHoldPosition = this.enableHoldPosition;
                pidRef.OnUpdateParameters(this.AccelPID, PIDParameters.PIDAxis.Accel);
            }
        }
        private void PropagateUpdatedPitchParameters()
        {
            PIDController.GlobalDebugPrint($"PIDController.PropagateUpdatedPitchParameters");
            foreach (ModulePID pidRef in this.m_PIDModules)
            {
                pidRef.manualTargetChangeRate = this.manualTargetChangeRate;
                pidRef.targetPitch = this.targetPitch;
                pidRef.OnUpdateParameters(this.PitchPID, PIDParameters.PIDAxis.Pitch);
            }
        }
        private void PropagateUpdatedRollParameters()
        {
            PIDController.GlobalDebugPrint($"PIDController.PropagateUpdatedRollParameters");
            foreach (ModulePID pidRef in this.m_PIDModules)
            {
                pidRef.manualTargetChangeRate = this.manualTargetChangeRate;
                pidRef.targetRoll = this.targetRoll;
                pidRef.OnUpdateParameters(this.RollPID, PIDParameters.PIDAxis.Roll);
            }
        }
        private void PropagateUpdatedParameters(PIDController.PIDParameters parameters, PIDParameters.PIDAxis axis)
        {
            PIDController.GlobalDebugPrint($"PIDController.PropagateUpdatedParameters {axis}: {ModulePID.ConvertOnSerialize(parameters)}");
            foreach (ModulePID pidRef in this.m_PIDModules)
            {
                pidRef.OnUpdateParameters(parameters, axis);
            }
        }
        #endregion UpdateParameters

        #region Reset_Error
        public void ResetError()
        {
            PIDController.GlobalDebugPrint($"PIDController.ResetError");
            this.ResetAccelError();
            this.ResetHoverError();
            this.ResetStrafeError();
            this.ResetPitchError();
            this.ResetRollError();
            this.ResetYawError();
        }
        public void ResetHoverError()
        {
            PIDController.GlobalDebugPrint($"PIDController.ResetHoverError");
            if (this.HoverPID != null)
            {
                this.HoverPID.ResetError();
            }
        }
        public void ResetStrafeError()
        {
            PIDController.GlobalDebugPrint($"PIDController.ResetStrafeError");
            if (this.StrafePID != null)
            {
                this.StrafePID.ResetError();
            }
        }
        public void ResetAccelError()
        {
            PIDController.GlobalDebugPrint($"PIDController.ResetAccelError");
            if (this.AccelPID != null)
            {
                this.AccelPID.ResetError();
            }
        }
        public void ResetPitchError()
        {
            PIDController.GlobalDebugPrint($"PIDController.ResetPitchError");
            if (this.PitchPID != null)
            {
                this.PitchPID.ResetError();
            }
        }
        public void ResetRollError()
        {
            PIDController.GlobalDebugPrint($"PIDController.ResetRollError");
            if (this.RollPID != null)
            {
                this.RollPID.ResetError();
            }
        }
        public void ResetYawError()
        {
            PIDController.GlobalDebugPrint($"PIDController.ResetYawError");
            if (this.YawPID != null)
            {
                this.YawPID.ResetError();
            }
        }
        #endregion Reset_Error

        public float GetCurrentHeight()
        {
            if (this.staticHeight)
            {
                return this.AttachedTank.WorldCenterOfMass.y;
            }
            else
            {
                float height;
                Vector3 currentPosition = this.AttachedTank.WorldCenterOfMass;
                if (ManWorld.inst.GetTerrainHeight(currentPosition, out height))
                {
                    return currentPosition.y - height;
                }
                return Mathf.Infinity;
            }
        }

        private void FixedUpdate() {
            // get input into desired throttle changes

            if (!this.AttachedTank.beam.IsActive)
            {
                Vector3 currentVelocity = this.AttachedTank.transform.InverseTransformVector(this.AttachedTank.rbody.velocity);
                Vector3 currentAngularVelocity = this.AttachedTank.transform.InverseTransformVector(this.AttachedTank.rbody.angularVelocity);
                Vector3 currentRotation = this.AttachedTank.transform.eulerAngles;

                if (currentRotation.x > 180f)
                {
                    currentRotation.x -= 360f;
                }
                else if (currentRotation.x < -180f)
                {
                    currentRotation.x += 360f;
                }

                if (currentRotation.y > 180f)
                {
                    currentRotation.y -= 360f;
                }
                else if (currentRotation.y < -180f)
                {
                    currentRotation.y += 360f;
                }

                if (currentRotation.z > 180f)
                {
                    currentRotation.z -= 360f;
                }
                else if (currentRotation.z < -180f)
                {
                    currentRotation.z += 360f;
                }

                TankControl control = this.AttachedTank.control;

                TankControl.ControlState controlState = (TankControl.ControlState) PIDControllerPatches.PatchTankControl.m_ControlState.GetValue(control);
                Vector3 inputCommand = (Vector3) controlState.m_State.m_InputMovement;
                Vector3 inputRotation = controlState.m_State.m_InputRotation;
                
                Vector3 newThrottle = Vector3.zero;
                Vector3 newRotation = Vector3.zero;
                Vector3 standardForce = -this.nonGravityThrust;
                Vector3 standardTorque = -this.nonManagedTorque;
                Vector3 relativeTargetPosition = this.AttachedTank.transform.InverseTransformVector(this.targetPosition - this.AttachedTank.WorldCenterOfMass);
                // PIDController.GlobalDebugPrint($"FixedUpdate Commanded Action: {inputCommand}, CalculatedThrustPos: {this.calculatedThrustNegative}, CalculatedThrustNeg: {this.calculatedThrustPositive}");

                if (this.StrafePID != null && this.StrafePID.enabled)
                {
                    if (this.calculatedThrustNegative.x != 0f || this.calculatedThrustPositive.x != 0f)
                    {
                        if (inputCommand.x == 0f)
                        {
                            float error;
                            float targetForce;
                            if (inputCommand.z == 0f)
                            {
                                // have horizontal targetPosition
                                if (this.targetPosition.x == Mathf.Infinity || this.targetPosition.z == Mathf.Infinity)
                                {
                                    this.targetPosition = this.AttachedTank.WorldCenterOfMass;
                                    relativeTargetPosition = this.AttachedTank.transform.InverseTransformPoint(this.targetPosition);
                                    this.AccelPID.ResetError();
                                    this.StrafePID.ResetError();
                                }
                            }
                            // Minimize velocity, set target once minimized
                            if ((this.targetPosition.x != Mathf.Infinity) && this.enableHoldPosition)
                            {
                                error = relativeTargetPosition.x;
                                // get force needed to bring to standstill
                                targetForce = 2 * this.AttachedTank.rbody.mass * (error - currentVelocity.x);
                            }
                            else
                            {
                                // this.targetPosition.z = Mathf.Infinity;
                                error = -currentVelocity.x;
                                // get force needed to bring to standstill
                                targetForce = -(this.AttachedTank.rbody.mass * currentVelocity.x);
                            }

                            float flatCalculatedThrust = targetForce < 0 ? this.calculatedThrustNegative.x : this.calculatedThrustPositive.x;
                            // get current available thrust based on tech angle
                            Vector3 worldTechRight = this.AttachedTank.transform.rotation * new Vector3(1, 0, 0);
                            float calculatedThrust = Mathf.Abs(flatCalculatedThrust * worldTechRight.x);
                            float standardThrottle = standardForce.x / calculatedThrust;

                            string prefixStr = $"Vel: {currentVelocity}, Target: {this.targetPosition}, Position: {this.AttachedTank.WorldCenterOfMass}, RelTarget: {relativeTargetPosition} ";
                            float pidForce = this.StrafePID.UpdateStep(error, ref PIDController.dummyStr, prefixStr);
                            float clampedPIDForce = targetForce > 0 ? Mathf.Min(targetForce, pidForce) : Mathf.Max(targetForce, pidForce);
                            float clampThrottle = Mathf.Clamp(standardThrottle + (clampedPIDForce / calculatedThrust), -1f, 1f);
                            if (this.StrafePID.debug)
                            {
                                Console.WriteLine($" | Strafe PID | Set Throttle: {clampThrottle} for Force: {clampedPIDForce}");
                            }
                            newThrottle.x = clampThrottle;
                        }
                        else
                        {
                            this.targetPosition.x = Mathf.Infinity;
                            this.targetPosition.z = Mathf.Infinity;
                            this.StrafePID.ResetError();
                            this.AccelPID.ResetError();
                        }
                    }
                }
                if (this.AccelPID != null && this.AccelPID.enabled)
                {
                    if (this.calculatedThrustNegative.z != 0f || this.calculatedThrustPositive.z != 0f)
                    {
                        if (inputCommand.z == 0f)
                        {
                            float error;
                            float targetForce;
                            if (inputCommand.x == 0f)
                            {
                                // have horizontal targetPosition
                                if (this.targetPosition.x == Mathf.Infinity || this.targetPosition.z == Mathf.Infinity)
                                {
                                    this.targetPosition = this.AttachedTank.WorldCenterOfMass;
                                    relativeTargetPosition = this.AttachedTank.transform.InverseTransformPoint(this.targetPosition);
                                    this.AccelPID.ResetError();
                                    this.StrafePID.ResetError();
                                }
                            }
                            // Minimize velocity, set target once minimized
                            if ((this.targetPosition.z != Mathf.Infinity) && this.enableHoldPosition)
                            {
                                error = relativeTargetPosition.z;
                                // get force needed to bring to standstill
                                targetForce = 2 * this.AttachedTank.rbody.mass * (error - currentVelocity.z);
                            }
                            else
                            {
                                // this.targetPosition.z = Mathf.Infinity;
                                error = -currentVelocity.z;
                                // get force needed to bring to standstill
                                targetForce = -(this.AttachedTank.rbody.mass * currentVelocity.z);
                            }

                            float flatCalculatedThrust = targetForce < 0 ? this.calculatedThrustNegative.z : this.calculatedThrustPositive.z;
                            // get current available thrust based on tech angle
                            Vector3 worldTechForward = this.AttachedTank.transform.rotation * new Vector3(0, 0, 1);
                            float calculatedThrust = Mathf.Abs(flatCalculatedThrust * worldTechForward.z);
                            float standardThrottle = standardForce.z / calculatedThrust;

                            string prefixStr = $"Vel: {currentVelocity}, Target: {this.targetPosition}, Position: {this.AttachedTank.WorldCenterOfMass}, RelTarget: {relativeTargetPosition} ";
                            float pidForce = this.AccelPID.UpdateStep(error, ref PIDController.dummyStr, prefixStr);
                            float clampedPIDForce = targetForce > 0 ? Mathf.Min(targetForce, pidForce) : Mathf.Max(targetForce, pidForce);
                            float clampThrottle = Mathf.Clamp(standardThrottle + (clampedPIDForce / calculatedThrust), -1f, 1f);
                            if (this.AccelPID.debug)
                            {
                                Console.WriteLine($" | Accel PID | Set Throttle: {clampThrottle} for Force: {clampedPIDForce}");
                            }
                            newThrottle.z = clampThrottle;
                        }
                        else
                        {
                            this.targetPosition.x = Mathf.Infinity;
                            this.targetPosition.z = Mathf.Infinity;
                            this.AccelPID.ResetError();
                            this.StrafePID.ResetError();
                        }
                    }
                }
                if (this.PitchPID != null && this.PitchPID.enabled)
                {
                    if (this.calculatedTorquePositive.x != 0f || this.calculatedTorqueNegative.x != 0f)
                    {
                        if (inputRotation.x == 0f)
                        {
                            float pitchVelocity = currentAngularVelocity.x * degreesPerRadian;
                            float error = currentRotation.x;
                            if (this.targetPitch > error)
                            {
                                float testClock = this.targetPitch - error;
                                if (testClock <= 180f)
                                {
                                    error = testClock;
                                }
                                else
                                {
                                    error = testClock - 360f;
                                }
                            }
                            else
                            {
                                float testCounter = error - this.targetPitch;
                                if (testCounter <= 180f)
                                {
                                    error = -testCounter;
                                }
                                else
                                {
                                    error = 360f - testCounter;
                                }
                            }

                            // get torque needed to bring to standstill
                            float targetTorque = 2 * this.AttachedTank.rbody.inertiaTensor.x * (error - pitchVelocity);

                            float calculatedTorque = targetTorque < 0 ? this.calculatedTorqueNegative.x : this.calculatedTorquePositive.x;
                            float standardRotation = standardTorque.z / calculatedTorque;

                            string prefixStr = $"Vel: {pitchVelocity}, Full Angular Vel: {currentAngularVelocity}, ";
                            float pidTorque = this.PitchPID.UpdateStep(error, ref PIDController.dummyStr, prefixStr);
                            float clampedPIDTorque = targetTorque > 0 ? Mathf.Min(targetTorque, pidTorque) : Mathf.Max(targetTorque, pidTorque);
                            float clampRotation = Mathf.Clamp(standardRotation + (clampedPIDTorque / calculatedTorque), -1f, 1f);
                            if (this.PitchPID.debug)
                            {
                                Console.WriteLine($" | Pitch PID | Set Rotation: {clampRotation} for Torque: {clampedPIDTorque}");
                            }
                            newRotation.x = clampRotation;
                        }
                        else
                        {
                            this.PitchPID.ResetError();
                        }
                    }
                }
                if (this.RollPID != null && this.RollPID.enabled)
                {
                    if (this.calculatedTorquePositive.z != 0f || this.calculatedTorqueNegative.z != 0f)
                    {
                        if (inputRotation.z == 0f)
                        {
                            float rollVelocity = currentAngularVelocity.z * degreesPerRadian;
                            float error = currentRotation.z;
                            if (this.targetRoll > error)
                            {
                                float testClock = this.targetRoll - error;
                                if (testClock <= 180f)
                                {
                                    error = testClock;
                                }
                                else
                                {
                                    error = testClock - 360f;
                                }
                            }
                            else
                            {
                                float testCounter = error - this.targetRoll;
                                if (testCounter <= 180f)
                                {
                                    error = -testCounter;
                                }
                                else
                                {
                                    error = 360f - testCounter;
                                }
                            }

                            // get torque needed to bring to standstill
                            float targetTorque = 2 * this.AttachedTank.rbody.inertiaTensor.z * (error - rollVelocity);

                            float calculatedTorque = targetTorque < 0 ? this.calculatedTorqueNegative.z : this.calculatedTorquePositive.z;
                            float standardRotation = standardTorque.z / calculatedTorque;

                            string prefixStr = $"Vel: {rollVelocity}, Full Angular Vel: {currentAngularVelocity}, ";
                            float pidTorque = this.RollPID.UpdateStep(error, ref PIDController.dummyStr, prefixStr);
                            float clampedPIDTorque = targetTorque > 0 ? Mathf.Min(targetTorque, pidTorque) : Mathf.Max(targetTorque, pidTorque);
                            float clampRotation = Mathf.Clamp(standardRotation + (clampedPIDTorque / calculatedTorque), -1f, 1f);
                            if (this.RollPID.debug)
                            {
                                Console.WriteLine($" | Pitch PID | Set Rotation: {clampRotation} for Torque: {clampedPIDTorque}");
                            }
                            newRotation.z = clampRotation;
                        }
                        else
                        {
                            this.RollPID.ResetError();
                        }
                    }
                }
                if (this.YawPID != null && this.YawPID.enabled)
                {
                    if (this.calculatedTorquePositive.y != 0f || this.calculatedTorqueNegative.y != 0f)
                    {
                        if (inputRotation.y == 0f)
                        {
                            float yawVelocity = currentRotation.y * degreesPerRadian;
                            float error = yawVelocity;

                            // get torque needed to bring to standstill
                            float targetTorque = - this.AttachedTank.rbody.inertiaTensor.y * yawVelocity;

                            float calculatedTorque = targetTorque < 0 ? this.calculatedTorqueNegative.y : this.calculatedTorquePositive.y;
                            float standardRotation = standardTorque.y / calculatedTorque;

                            string prefixStr = $"Vel: {yawVelocity}, Full Angular Vel: {currentAngularVelocity}, ";
                            float pidTorque = this.YawPID.UpdateStep(error, ref PIDController.dummyStr, prefixStr);
                            float clampedPIDTorque = targetTorque > 0 ? Mathf.Min(targetTorque, pidTorque) : Mathf.Max(targetTorque, pidTorque);
                            float clampRotation = Mathf.Clamp(standardRotation + (clampedPIDTorque / calculatedTorque), -1f, 1f);
                            if (this.YawPID.debug)
                            {
                                Console.WriteLine($" | Yaw PID | Set Rotation: {clampRotation} for Torque: {clampedPIDTorque}");
                            }
                            newRotation.y = clampRotation;
                        }
                        else
                        {
                            this.YawPID.ResetError();
                        }
                    }
                }
                if (this.HoverPID != null && this.HoverPID.enabled)
                {
                    standardForce -= this.baseGravity;
                    float flatCalculatedThrust = standardForce.y < 0 ? this.calculatedThrustNegative.y : this.calculatedThrustPositive.y;
                    // get current available thrust based on tech angle
                    Vector3 worldTechUp = this.AttachedTank.transform.rotation * new Vector3(0, 1, 0);
                    float calculatedThrust = Mathf.Abs(flatCalculatedThrust * worldTechUp.y);
                    if (calculatedThrust != 0f)
                    {
                        float standardThrottle = standardForce.y / calculatedThrust;

                        // hover at altitude
                        // useTargetHeight ==> always hover
                        if (this.useTargetHeight)
                        {
                            // with target height updated, update other stuff
                            float height = this.GetCurrentHeight();
                            float newError = this.targetHeight - height;

                            if (this.staticHeight)
                            {
                                newError = this.targetHeight - this.AttachedTank.WorldCenterOfMass.y;
                            }

                            if (newError < 1f && newError > -1f)
                            {
                                newError = 0.0f;
                            }

                            // calculate new throttle values
                            float targetForce = 2 * this.AttachedTank.rbody.mass * (newError - currentVelocity.y);
                            string outStr = null;
                            float pidForce = this.HoverPID.UpdateStep(newError, ref outStr, $"H: ({height}), GS: {{{-standardForce}}}, ");

                            float clampedPIDForce = targetForce > 0 ? Mathf.Min(targetForce, pidForce) : Mathf.Max(targetForce, pidForce);
                            float clampThrottle = Mathf.Clamp(standardThrottle + (clampedPIDForce / calculatedThrust), -1f, 1f);
                            newThrottle.y = clampThrottle;

                            bool down = clampThrottle < 0f;
                            string postfix = $"T: {{{clampThrottle}}},  TF: {{{(down ? clampThrottle * this.calculatedThrustNegative.y : clampThrottle * this.calculatedThrustPositive.y)}}},  {(down ? "" : "[")}F_U{(down ? "" : "]")}: <{this.calculatedThrustPositive.y}>, {(down ? "[" : "")}F_D{(down ? "]" : "")}: <{this.calculatedThrustNegative.y}>, KCF:[{ targetForce}]";

                            // do overriden print
                            this.HoverPID.DebugPrint($"{outStr}, {postfix}");
                        }
                        else if (inputCommand.y == 0f)
                        {
                            float error;
                            float targetForce;

                            if (this.targetHeight == Mathf.Infinity)
                            {
                                this.targetHeight = this.AttachedTank.WorldCenterOfMass.y;
                                this.HoverPID.ResetError();
                            }
                            error = this.targetHeight - this.AttachedTank.WorldCenterOfMass.y;
                            // get force needed to bring to standstill
                            targetForce = 2 * this.AttachedTank.rbody.mass * (error - currentVelocity.y);

                            string prefixStr = $"Vel: {currentVelocity}";
                            float pidForce = this.HoverPID.UpdateStep(error, ref PIDController.dummyStr, prefixStr);
                            float clampedPIDForce = targetForce > 0 ? Mathf.Min(targetForce, pidForce) : Mathf.Max(targetForce, pidForce);
                            float clampThrottle = Mathf.Clamp(standardThrottle + (clampedPIDForce / calculatedThrust), -1f, 1f);
                            newThrottle.y = clampThrottle;
                        }
                        // this.useTargetHeight = false, (not enableHoldPosition, or velocity high)
                        else
                        {
                            this.targetHeight = Mathf.Infinity;
                            this.HoverPID.ResetError();
                        }
                    }
                }
                this.UpdateThrottle(newThrottle);
            }
            else
            {
                this.ResetError();
            }
            this.nonGravityThrust = Vector3.zero;
            this.nonManagedTorque = Vector3.zero;
        }

        private void UpdateThrottle(Vector3 throttle)
        {
            TankControl control = this.AttachedTank.control;
            Vector3 newThrottle = (Vector3) PIDController.m_Throttle.GetValue(control);

            newThrottle.x = throttle.x;
            newThrottle.y = throttle.y;
            newThrottle.z = throttle.z;

            PIDController.m_Throttle.SetValue(control, newThrottle);

            TankControl.ControlState controlState = (TankControl.ControlState)PIDController.m_ControlState.GetValue(control);
            
            if (throttle.x != 0f)
            {
                controlState.m_State.m_ThrottleValues.x = throttle.x;
            }
            if (throttle.y != 0f)
            {
                controlState.m_State.m_ThrottleValues.y = throttle.y;
            }
            if (throttle.z != 0f)
            {
                controlState.m_State.m_ThrottleValues.z = throttle.z;
            }
            return;
        }

        public void ForceSpawn(Tank tech)
        {
            PIDController.GlobalDebugPrint("Force Spawn PIDController");
            this.PrePool();
            this._tech = tech;
            this.AttachedTank.ResetPhysicsEvent.Send();
            this.CalculateTechThrottleThrust();
        }

        // calculates the boosters currently on the tech. There's a separate patch that does it for each new fan added/deleted
        public void CalculateTechThrottleThrust()
        {
            List<ModuleBooster> boosterList = (List<ModuleBooster>) PIDController.m_BoosterModules.GetValue(this.AttachedTank.Boosters);
            if (boosterList != null)
            {
                foreach (ModuleBooster booster in boosterList)
                {
                    PIDController.GlobalDebugPrint("found booster: " + booster.block.name);
                    PIDControllerPatches.PatchBooster.GetThrustComponents(booster, this, true);
                    PIDControllerPatches.PatchBooster.GetTorqueComponents(booster, this, true);
                }
            }

            HashSet<ModuleLinearMotionEngine> lmeSet = (HashSet<ModuleLinearMotionEngine>) PIDController.m_LinearMotionEngines.GetValue(this.AttachedTank.TechAudio);
            if (lmeSet != null)
            {
                foreach (ModuleLinearMotionEngine engine in lmeSet)
                {
                    PIDController.GlobalDebugPrint("found lme: " + engine.block.name);
                    PIDControllerPatches.PatchLinearMotionEngine.GetThrustComponents(engine, this, true);
                }
            }
        }

        #region HookedFuncs
        private void PrePool()
        {
            PIDController.GlobalDebugPrint("PIDController Pre Pool");
        }

        private void OnPool()
        {
            PIDController.GlobalDebugPrint("PIDController On Pool");
            this._tech = base.Tech;
            this.AttachedTank.ResetPhysicsEvent.Send();
            this.CalculateTechThrottleThrust();
        }

        #endregion HookedFuncs
    }
}
