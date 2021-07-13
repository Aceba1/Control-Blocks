using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using Rewired;

namespace Control_Block
{
    public class PIDControllerPatches
    {
        // patch input so no player can change vertical throttle if there's a PID
        [HarmonyPatch(typeof(TankControl))]
        [HarmonyPatch("GetInput")]
        public class PatchTankControl
        {
            private static FieldInfo m_Throttle = typeof(TankControl).GetField("m_ThrottleValues", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_ThrottleInput = typeof(TankControl).GetField("m_ThrottleLastInput", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_ThrottleTiming = typeof(TankControl).GetField("m_ThrottleTiming", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public static FieldInfo m_ControlState = typeof(TankControl).GetField("m_ControlState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public static FieldInfo m_RewiredPlayer = typeof(TankControl).GetField("m_RewiredPlayer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public static FieldInfo m_ThrottleAxisEnableCount = typeof(TankControl).GetField("m_ThrottleAxisEnableCount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public static FieldInfo m_ExplosiveBoltActivationControl = typeof(TankControl).GetField("m_ExplosiveBoltActivationControl", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public static FieldInfo m_AnchorToggleControl = typeof(TankControl).GetField("m_AnchorToggleControl", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public static FieldInfo m_BeamToggled = typeof(TankControl).GetField("m_BeamToggled", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            private static readonly Vector3 defaultState = new Vector3(Mathf.NegativeInfinity, Mathf.NegativeInfinity, Mathf.NegativeInfinity);
            private static MethodInfo ApplyThrottle = typeof(TankControl).GetMethod("ApplyThrottle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public static bool Prefix(ref TankControl __instance, ref Vector3 __state)
            {
                __state = new Vector3(Mathf.NegativeInfinity, Mathf.NegativeInfinity, Mathf.NegativeInfinity);

                PIDController pidController = ((TechComponent)__instance).Tech.gameObject.GetComponent<PIDController>();
                if (pidController)
                {
                    ControlScheme activeScheme = __instance.ActiveScheme;
                    Player rewiredPlayer = (Player) PatchTankControl.m_RewiredPlayer.GetValue(__instance);

                    if (rewiredPlayer != null && activeScheme != null)
                    {
                        float dt = Time.deltaTime;

                        TankControl.ControlState controlState = (TankControl.ControlState)PatchTankControl.m_ControlState.GetValue(__instance);
                        if ((bool)PatchTankControl.m_BeamToggled.GetValue(__instance) || __instance.Tech.beam.IsActive)
                        {
                            __instance.UpdateAxesWarnings(__instance.Tech.beam.IsActive);
                        }
                        if (rewiredPlayer.GetButtonDown(26))
                        {
                            Singleton.Manager<ManPauseGame>.inst.Invoke("TogglePauseMenu", 0f);
                        }
                        if (rewiredPlayer.GetButtonDown(73) && Singleton.Manager<ManHUD>.inst.IsHudElementVisible(ManHUD.HUDElementType.ControlSchema))
                        {
                            __instance.CycleActiveScheme();
                            activeScheme = __instance.ActiveScheme;
                        }
                        Vector3 inputMovement = new Vector3(activeScheme.GetAxisMapping(MovementAxis.MoveX_MoveRight).ReadRewiredInput(rewiredPlayer), activeScheme.GetAxisMapping(MovementAxis.MoveY_MoveUp).ReadRewiredInput(rewiredPlayer), activeScheme.GetAxisMapping(MovementAxis.MoveZ_MoveForward).ReadRewiredInput(rewiredPlayer));
                        Vector3 inputRotation = new Vector3(activeScheme.GetAxisMapping(MovementAxis.RotateX_PitchUp).ReadRewiredInput(rewiredPlayer), activeScheme.GetAxisMapping(MovementAxis.RotateY_YawLeft).ReadRewiredInput(rewiredPlayer), activeScheme.GetAxisMapping(MovementAxis.RotateZ_RollRight).ReadRewiredInput(rewiredPlayer));
                        if (Singleton.Manager<ManInput>.inst.IsCurrentlyUsingGamepad())
                        {
                            Vector2 vector3 = new Vector2(inputRotation.y, inputMovement.z);
                            vector3 = Globals.inst.m_DriveStickInputInterpreter.InterpretAnalogStickInput(vector3);
                            inputRotation.y = vector3.x;
                            inputMovement.z = vector3.y;
                        }

                        int[] throttleAxisEnableCount = (int[])PatchTankControl.m_ThrottleAxisEnableCount.GetValue(__instance);
                        Vector3 oldThrottle = (Vector3)PatchTankControl.m_Throttle.GetValue(__instance);
                        Vector3 oldInput = (Vector3)PatchTankControl.m_ThrottleInput.GetValue(__instance);
                        Vector3 oldTiming = (Vector3)PatchTankControl.m_ThrottleTiming.GetValue(__instance);

                        object[] argsX = new object[] { throttleAxisEnableCount[0] > 0, inputMovement.x, oldThrottle.x, oldInput.x, oldTiming.x };
                        object[] argsY = new object[] { throttleAxisEnableCount[1] > 0, inputMovement.y, oldThrottle.y, oldInput.y, oldTiming.y };
                        object[] argsZ = new object[] { throttleAxisEnableCount[2] > 0, inputMovement.z, oldThrottle.z, oldInput.z, oldTiming.z };
                        if (pidController.StrafePID == null)
                        {
                            PatchTankControl.ApplyThrottle.Invoke(__instance, argsX);
                        }
                        if (pidController.HoverPID == null)
                        {
                            PatchTankControl.ApplyThrottle.Invoke(__instance, argsY);
                        }
                        else if (pidController.useTargetHeight && inputMovement.y != 0f)
                        {
                            pidController.targetHeight += Mathf.Sign(inputMovement.y) * pidController.manualTargetChangeRate * dt;
                            pidController.PropagateUpdatedHoverParameters();
                        }
                        if (pidController.AccelPID == null)
                        {
                            PatchTankControl.ApplyThrottle.Invoke(__instance, argsZ);
                        }

                        // fix stuff after the ref
                        Vector3 newThrottle = new Vector3((float) argsX[2], (float) argsY[2], (float) argsZ[2]);
                        Vector3 newInput = new Vector3((float)argsX[3], (float)argsY[3], (float)argsZ[3]);
                        Vector3 newTiming = new Vector3((float)argsX[4], (float)argsY[4], (float)argsZ[4]);

                        PatchTankControl.m_Throttle.SetValue(__instance, newThrottle);
                        PatchTankControl.m_ThrottleInput.SetValue(__instance, newInput);
                        PatchTankControl.m_ThrottleTiming.SetValue(__instance, newTiming);

                        inputMovement.x = (float) argsX[1];
                        inputMovement.y = (float) argsY[1];
                        inputMovement.z = (float) argsZ[1];

                        // pitch/roll always use target angle - ignore
                        if (pidController.PitchPID && pidController.PitchPID.enabled)
                        {
                            pidController.targetPitch = Mathf.Clamp(pidController.targetPitch + (inputRotation.x * pidController.manualTargetChangeRate * dt / 180), -1f, 1f);
                            inputRotation.x = 0f;
                        }
                        if (pidController.RollPID && pidController.RollPID.enabled)
                        {
                            pidController.targetRoll = Mathf.Clamp(pidController.targetRoll + (inputRotation.z * pidController.manualTargetChangeRate * dt / 180), -1f, 1f);
                            inputRotation.z = 0f;
                        }

                        // reverse steering - may need to move rotation control around
                        if (!__instance.ActiveScheme.ReverseSteering && (inputMovement.z < -0.01f || newThrottle[2] < -0.01f))
                        {
                            Vector3 forward = __instance.Tech.rootBlockTrans.forward;
                            if (Vector3.Dot(__instance.Tech.rbody.velocity, forward) < 0f)
                            {
                                inputRotation.y *= -1f;
                            }
                        }

                        controlState.m_State.m_InputMovement = inputMovement;
                        controlState.m_State.m_InputRotation = inputRotation;
                        controlState.m_State.m_ThrottleValues = newThrottle;
                        controlState.m_State.m_BoostProps = (activeScheme.GetAxisMapping(MovementAxis.BoostPropellers).ReadRewiredInput(rewiredPlayer) > 0.01f);
                        controlState.m_State.m_BoostJets = (activeScheme.GetAxisMapping(MovementAxis.BoostJets).ReadRewiredInput(rewiredPlayer) > 0.01f);
                        if (Singleton.Manager<ManNetwork>.inst.IsMultiplayerAndInvulnerable())
                        {
                            if (Singleton.Manager<ManNetwork>.inst.NetController.GameModeType == MultiplayerModeType.Deathmatch)
                            {
                                __instance.FireControl = rewiredPlayer.GetButton(2);
                            }
                            else
                            {
                                __instance.FireControl = false;
                            }
                        }
                        else
                        {
                            __instance.FireControl = rewiredPlayer.GetButton(2);
                        }
                        if (rewiredPlayer.GetButtonDown(3))
                        {
                            bool suppressInventory = Singleton.Manager<ManInput>.inst.IsCurrentInputSource(3, ControllerType.Joystick);
                            __instance.ToggleBeamActivated(suppressInventory);
                        }
                        else if (rewiredPlayer.GetButtonDown(25))
                        {
                            bool flag = true;
                            if (Singleton.Manager<ManNetwork>.inst.IsMultiplayer())
                            {
                                flag = Singleton.Manager<ManNetwork>.inst.InventoryAvailable;
                            }
                            if (flag)
                            {
                                Singleton.Manager<ManPurchases>.inst.TogglePalette();
                            }
                        }
                        else if (rewiredPlayer.GetButtonDown(74) && Singleton.Manager<ManHUD>.inst.IsHudElementVisible(ManHUD.HUDElementType.SkinsPalette))
                        {
                            Singleton.Manager<ManHUD>.inst.HideHudElement(ManHUD.HUDElementType.SkinsPalette, null);
                        }
                        PatchTankControl.m_ExplosiveBoltActivationControl.SetValue(__instance, rewiredPlayer.GetButtonDown(45));
                        PatchTankControl.m_AnchorToggleControl.SetValue(__instance, rewiredPlayer.GetButtonDown(78));
                        if (Singleton.Manager<ManInput>.inst.IsGamepadUseEnabled())
                        {
                            if (rewiredPlayer.GetButtonDown(5) && Singleton.Manager<ManInput>.inst.GetCurrentUIInputMode() != UIInputMode.UISkinsPalettePanel)
                            {
                                if (Singleton.Manager<ManPointer>.inst.DraggingItem == null || Singleton.Manager<ManPointer>.inst.BuildMode == ManPointer.BuildingMode.PaintBlock)
                                {
                                    bool enable = !Singleton.Manager<ManPointer>.inst.IsInteractionModeEnabled;
                                    Singleton.Manager<ManPointer>.inst.EnableInteractionMode(enable);
                                    return false;
                                }
                            }
                            else if (rewiredPlayer.GetButtonDown(48))
                            {
                                OpenMenuEventData openMenuEventData = default(OpenMenuEventData);
                                openMenuEventData.m_RadialInputController = ManInput.RadialInputController.Gamepad;
                                openMenuEventData.m_AllowRadialMenu = true;
                                openMenuEventData.m_AllowNonRadialMenu = false;
                                if (!Singleton.Manager<ManHUD>.inst.IsHudElementVisible(ManHUD.HUDElementType.InteractionMode))
                                {
                                    UIHUDElement hudElement = Singleton.Manager<ManHUD>.inst.GetHudElement(ManHUD.HUDElementType.InteractionMode);
                                    if (hudElement && Singleton.playerTank != null)
                                    {
                                        hudElement.GetComponent<UIInteractionHUD>().PointToPos(Singleton.playerTank.WorldCenterOfMass);
                                    }
                                }
                                if (Singleton.Manager<ManNetwork>.inst.IsMultiplayer() && Singleton.Manager<ManGameMode>.inst.GetCurrentGameType() == ManGameMode.GameType.Deathmatch)
                                {
                                    Singleton.Manager<ManHUD>.inst.ShowHudElement(ManHUD.HUDElementType.MPTechActions, openMenuEventData);
                                    return false;
                                }
                                Singleton.Manager<ManHUD>.inst.ShowHudElement(ManHUD.HUDElementType.TechAndBlockActions, openMenuEventData);
                                return false;
                            }
                            else if (rewiredPlayer.GetButtonDown(62) && !Singleton.Manager<ManUndo>.inst.UndoAvailable && Singleton.Manager<ManPointer>.inst.BuildMode != ManPointer.BuildingMode.PaintBlock)
                            {
                                __instance.BoostControlProps = true;
                            }
                        }
                    }
                    return false;
                }
                return true;
            }
        }

        #region UIPatches
        // Patch Altimeter UI to display the actual height
        [HarmonyPatch(typeof(UIAltimeter))]
        [HarmonyPatch("Show")]
        public class PatchAltimeter
        {
            private static FieldInfo m_SeaLevelYPos = typeof(UIAltimeter).GetField("m_SeaLevelYPos", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_Imperial = typeof(UIAltimeter).GetField("m_Imperial", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public static void Postfix(ref UIAltimeter __instance)
            {
                PatchAltimeter.m_SeaLevelYPos.SetValue(__instance, 0f);
                PatchAltimeter.m_Imperial.SetValue(__instance, false);
                return;
            }
        }

        // Patch GetThrottle to display target height
        [HarmonyPatch(typeof(UIThrottle))]
        [HarmonyPatch("UpdateAxis")]
        public class PatchThrottleVisual
        {
            private static FieldInfo m_TankControl = typeof(UIThrottle).GetField("m_TankControl", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_TextDistance = typeof(UIThrottle).GetField("m_TextDistance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            public static bool Prefix(ref UIThrottle __instance, ref UIThrottle.Axis axis, ref int axisIndex, ref Vector3 direction)
            {
                if (axisIndex != 1)
                {
                    return true;
                }
                else
                {
                    TankControl tankControl = (TankControl)PatchThrottleVisual.m_TankControl.GetValue(__instance);
                    PIDController pidController = tankControl.Tech.gameObject.GetComponent<PIDController>();
                    if (pidController != null && pidController.HoverPID != null && (pidController.useTargetHeight || pidController.targetHeight != Mathf.Infinity))
                    {
                        float num2 = Mathf.Round(pidController.targetHeight * 10f);
                        if (axis.m_NumericDisplay)
                        {
                            bool flag = (num2 != 0f);
                            if (flag)
                            {
                                if (axis.m_CurrentNumericValue != num2)
                                {
                                    axis.m_CurrentNumericValue = num2;
                                    axis.m_NumericDisplay.text = (num2 * 0.1f).ToString("+0.0;-0.0");
                                }
                                Vector2 vector = new Vector2(direction.x, direction.y).normalized;
                                float num3 = (float)PatchThrottleVisual.m_TextDistance.GetValue(__instance);
                                num3 += Mathf.Lerp(axis.m_NumericDisplay.preferredHeight, axis.m_NumericDisplay.preferredWidth, Mathf.Abs(vector.x)) * 0.5f;
                                vector *= num3;
                                if (num2 < 0f)
                                {
                                    vector = -vector;
                                }
                                vector.x = Mathf.Round(vector.x);
                                vector.y = Mathf.Round(vector.y);
                                if (axis.m_NumberLocation && axis.m_CurrentNumberPos != vector)
                                {
                                    axis.m_CurrentNumberPos = vector;
                                    axis.m_NumberLocation.anchoredPosition = vector;
                                }
                            }
                            axis.m_NumericDisplay.gameObject.SetActive(flag);
                        }
                        return false;
                    }
                    return true;
                }
            }
        }
        #endregion UIPatches

        #region PatchIndependentForces
        // Patch Gyros
        [HarmonyPatch(typeof(ModuleGyro))]
        [HarmonyPatch("FixedUpdate")]
        public class PatchModuleGyro
        {
            private static FieldInfo m_UseActive = typeof(ModuleGyro).GetField("m_UseActive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_UsePassive = typeof(ModuleGyro).GetField("m_UsePassive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_ActiveStability = typeof(ModuleGyro).GetField("m_ActiveStability", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_ActiveSpeed = typeof(ModuleGyro).GetField("m_ActiveSpeed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_ControlTrim = typeof(ModuleGyro).GetField("m_ControlTrim", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_ControlTrimTarget = typeof(ModuleGyro).GetField("m_ControlTrimTarget", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_TrimAdjustSpeed = typeof(ModuleGyro).GetField("m_TrimAdjustSpeed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_Trim = typeof(ModuleGyro).GetField("m_Trim", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_TrimPitchMax = typeof(ModuleGyro).GetField("m_TrimPitchMax", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_ActiveAxisScale = typeof(ModuleGyro).GetField("m_ActiveAxisScale", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_PrevAngularVel = typeof(ModuleGyro).GetField("m_PrevAngularVel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_PassiveStrength = typeof(ModuleGyro).GetField("m_PassiveStrength", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_RotationMinSpeed = typeof(ModuleGyro).GetField("m_RotationMinSpeed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_RotationMaxSpeed = typeof(ModuleGyro).GetField("m_RotationMaxSpeed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_RotationAnimTransform = typeof(ModuleGyro).GetField("m_RotationAnimTransform", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_RotationAnimSpeed = typeof(ModuleGyro).GetField("m_RotationAnimSpeed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            private static MethodInfo TestUseFilter = typeof(ModuleGyro).GetMethod("TestUseFilter", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            public static bool Prefix(ref ModuleGyro __instance)
            {
                if (__instance.block.tank)
                {
                    PIDController pidController = __instance.block.tank.gameObject.GetComponent<PIDController>();
                    if (pidController != null && !pidController.AttachedTank.beam.IsActive)
                    {
                        Rigidbody rbody = __instance.block.tank.rbody;
                        Vector3 vector = Vector3.zero;
                        if ((bool)PatchModuleGyro.TestUseFilter.Invoke(__instance, new object[] {PatchModuleGyro.m_UseActive.GetValue(__instance)}))
                        {
                            float activeSpeed = (float) PatchModuleGyro.m_ActiveSpeed.GetValue(__instance);

                            Vector3 vector2 = Quaternion.AngleAxis(rbody.angularVelocity.magnitude * 57.29578f * ((float)PatchModuleGyro.m_ActiveStability.GetValue(__instance) / activeSpeed), rbody.angularVelocity) * __instance.block.tank.rootBlockTrans.up;
                            Vector3 vector3 = Vector3.up;
                            Vector3 right = __instance.block.tank.rootBlockTrans.right;
                            if ((__instance.block.tank.BlockStateController == null || !__instance.block.tank.BlockStateController.IsKillswitched(BlockControllerModuleTypes.GyroTrim)) && Mathf.Abs(Vector3.Dot(vector3, right)) < 0.95f)
                            {
                                float controlTrim = (float)PatchModuleGyro.m_ControlTrim.GetValue(__instance);
                                controlTrim = Mathf.MoveTowards(controlTrim, (float)PatchModuleGyro.m_ControlTrimTarget.GetValue(__instance), Time.deltaTime * (float)PatchModuleGyro.m_TrimAdjustSpeed.GetValue(__instance));
                                PatchModuleGyro.m_ControlTrim.SetValue(__instance, controlTrim);
                                float num = Mathf.Clamp((float)PatchModuleGyro.m_Trim.GetValue(__instance) + controlTrim, -1f, 1f);
                                vector3 = Quaternion.AngleAxis((float)PatchModuleGyro.m_TrimPitchMax.GetValue(__instance) * num, right) * vector3;
                            }
                            Vector3 vector4 = Vector3.Cross(vector2, vector3);
                            Vector3 vector5 = __instance.transform.InverseTransformVector(vector4);
                            Vector3 activeAxisScale = (Vector3) PatchModuleGyro.m_ActiveAxisScale.GetValue(__instance);
                            vector5.x *= activeAxisScale.x;
                            vector5.y *= activeAxisScale.y;
                            vector5.z *= activeAxisScale.z;
                            vector4 = __instance.transform.TransformVector(vector5);
                            vector += vector4 * activeSpeed * activeSpeed;
                            Debug.DrawLine(rbody.worldCenterOfMass, rbody.worldCenterOfMass + rbody.transform.up * 10f, Color.green);
                            Debug.DrawLine(rbody.worldCenterOfMass, rbody.worldCenterOfMass + vector2 * 10f, Color.cyan);
                            Debug.DrawLine(rbody.worldCenterOfMass, rbody.worldCenterOfMass + vector4 * 10f, Color.red);
                            Debug.DrawLine(__instance.block.centreOfMassWorld, __instance.block.centreOfMassWorld + vector4 * 10f, Color.red);
                        }
                        if ((bool)PatchModuleGyro.TestUseFilter.Invoke(__instance, new object[] { PatchModuleGyro.m_UsePassive.GetValue(__instance) }))
                        {
                            Vector3 vector6 = rbody.position + (__instance.block.transform.position - rbody.transform.position);
                            Vector3 vector7 = rbody.GetPointVelocity(vector6) - rbody.velocity;
                            Vector3 vector8 = __instance.transform.InverseTransformVector(vector7);

                            Vector3 passiveStrength = (Vector3)PatchModuleGyro.m_PassiveStrength.GetValue(__instance);

                            vector8.x *= passiveStrength.x;
                            vector8.y *= passiveStrength.y;
                            vector8.z *= passiveStrength.z;
                            vector7 = __instance.transform.TransformVector(vector8);
                            vector += Vector3.Cross(vector7, vector6 - rbody.worldCenterOfMass);
                            if (Vector3.Dot(rbody.angularVelocity, (Vector3) PatchModuleGyro.m_PrevAngularVel.GetValue(__instance)) < -1f)
                            {
                                vector *= 0.5f;
                            }
                        }
                        PatchModuleGyro.m_PrevAngularVel.SetValue(__instance, rbody.angularVelocity);

                        pidController.nonManagedTorque += vector;
                        rbody.AddTorque(vector);

                        Transform rotationAnimTransform = (Transform)PatchModuleGyro.m_RotationAnimTransform.GetValue(__instance);

                        if (rotationAnimTransform)
                        {
                            float num2 = Mathf.Min(vector.magnitude, (float)PatchModuleGyro.m_RotationMaxSpeed.GetValue(__instance));
                            if (num2 > (float)PatchModuleGyro.m_RotationMinSpeed.GetValue(__instance))
                            {
                                rotationAnimTransform.localRotation *= Quaternion.AngleAxis(num2 * (float)PatchModuleGyro.m_RotationAnimSpeed.GetValue(__instance) * Time.fixedDeltaTime, Vector3.forward);
                            }
                        }
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }

        // Patch ManGravity Thrust
        [HarmonyPatch(typeof(ManGravity))]
        [HarmonyPatch("FixedUpdate")]
        public class PatchManGravity
        {
            private static FieldInfo m_ApplicationTargets = typeof(ManGravity).GetField("m_ApplicationTargets", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public static bool Prefix(ref ManGravity __instance)
            {
                float y = Physics.gravity.y;
                List<IGravityApplicationTarget> applicationTargets = (List<IGravityApplicationTarget>)PatchManGravity.m_ApplicationTargets.GetValue(__instance);
                foreach (IGravityApplicationTarget gravityApplicationTarget in applicationTargets)
                {
                    if (gravityApplicationTarget.CanApplyGravity())
                    {
                        Rigidbody applicationRigidbody = gravityApplicationTarget.GetApplicationRigidbody();
                        if (applicationRigidbody.IsNotNull())
                        {
                            float gravityScale = gravityApplicationTarget.GetGravityScale();
                            bool flag = Mathf.Approximately(gravityScale, 1f);
                            applicationRigidbody.useGravity = flag;
                            if (!flag)
                            {
                                float normalGravity = y * applicationRigidbody.mass;
                                float num = gravityScale * normalGravity;
                                if (num != 0f)
                                {
                                    Vector3 forcePosition = gravityApplicationTarget.GetWorldCentreOfGravity();
                                    Vector3 force = new Vector3(0, num, 0);
                                    applicationRigidbody.AddForceAtPosition(force, forcePosition);

                                    // insertion: check if there's a tank with a HoverPID
                                    PIDController pidController = applicationRigidbody.gameObject.GetComponent<PIDController>();
                                    if (pidController && !pidController.AttachedTank.beam.IsActive)
                                    {
                                        force.y -= normalGravity;
                                        // num is the new gravity force, replaces default gravity
                                        // gravity is effectively nullified for the block mass now, uses this isntead
                                        pidController.nonGravityThrust += force;

                                        Vector3 localVector = pidController.AttachedTank.transform.InverseTransformVector(forcePosition);
                                        pidController.nonManagedTorque += Vector3.Cross(localVector, force);
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            }
        }

        // Patch floater thrust
        [HarmonyPatch(typeof(MotionBlocks.ModuleFloater))]
        [HarmonyPatch("FixedUpdate")]
        public class PatchModuleFloater
        {
            public static bool Prefix(ref MotionBlocks.ModuleFloater __instance)
            {
                if (__instance.block.IsAttached && __instance.block.tank != null && !__instance.block.tank.beam.IsActive)
                {
                    PIDController pidController = __instance.block.tank.gameObject.GetComponent<PIDController>();
                    if (pidController)
                    {
                        Vector3 blockCenter = __instance.block.centreOfMassWorld;
                        float blockForce = (__instance.MaxStrength / __instance.MaxHeight) * (__instance.MaxHeight - blockCenter.y)
                              - __instance.block.tank.rbody.GetPointVelocity(blockCenter).y * __instance.VelocityDampen;
                        Vector3 force = Vector3.up;
                        if (__instance.MaxStrength > 0)
                        {
                            force *= Mathf.Clamp(blockForce, 0f, __instance.MaxStrength * 1.25f);
                        }
                        else
                        {
                            force *= Mathf.Clamp(blockForce, __instance.MaxStrength * 1.25f, 0f);
                        }
                        __instance.block.tank.rbody.AddForceAtPosition(force, blockCenter, ForceMode.Impulse);
                        pidController.nonGravityThrust += force;

                        Vector3 localVector = __instance.block.tank.transform.InverseTransformVector(blockCenter);
                        pidController.nonManagedTorque += Vector3.Cross(localVector, force);
                        return false;
                    }
                }
                return true;
            }
        }

        // Patch ModuleWing thrust
        [HarmonyPatch(typeof(ModuleWing))]
        [HarmonyPatch("FixedUpdate")]
        public class PatchModuleWing
        {
            public static bool Prefix(ref ModuleWing __instance)
            {
                return true;
            }
        }

        // Patch ModuleAirBrake thrust
        [HarmonyPatch(typeof(ModuleAirBrake))]
        [HarmonyPatch("FixedUpdate")]
        public class PatchModuleAirBrake
        {
            private static FieldInfo m_Effector = typeof(ModuleAirBrake).GetField("m_Effector", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_Deployed = typeof(ModuleAirBrake).GetField("m_Deployed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_Strength = typeof(ModuleAirBrake).GetField("m_Strength", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_MinForceThreshold = typeof(ModuleAirBrake).GetField("m_MinForceThreshold", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            private static MethodInfo SetHasVelocity = typeof(ModuleAirBrake).GetMethod("SetHasVelocity", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public static bool Prefix(ref ModuleAirBrake __instance)
            {
                Transform effector = (Transform)PatchModuleAirBrake.m_Effector.GetValue(__instance);
                Tank tank = ((Module)__instance).block.tank;
                if (tank && effector)
                {
                    Rigidbody rbody = tank.rbody;
                    Vector3 vector = rbody.position + (effector.position - rbody.transform.position);
                    Vector3 vector2 = rbody.GetPointVelocity(vector);
                    vector2 = tank.control.ZeroWorldVelocityInThrottledAxes(tank, vector2);
                    Vector3 vector3 = ((Module)__instance).transform.InverseTransformVector(vector2);
                    Vector3 strengthVector = (Vector3)PatchModuleAirBrake.m_Strength.GetValue(__instance);
                    vector3.x = vector3.x * vector3.x * Mathf.Sign(vector3.x) * strengthVector.x;
                    vector3.y = vector3.y * vector3.y * Mathf.Sign(vector3.y) * strengthVector.y;
                    vector3.z = vector3.z * vector3.z * Mathf.Sign(vector3.z) * strengthVector.z;

                    PatchModuleAirBrake.SetHasVelocity.Invoke(__instance, new object[] { vector3.magnitude > (float)PatchModuleAirBrake.m_MinForceThreshold.GetValue(__instance) });
                    if ((bool)PatchModuleAirBrake.m_Deployed.GetValue(__instance))
                    {
                        vector2 = ((Module)__instance).transform.TransformVector(vector3);
                        rbody.AddForceAtPosition(-vector2, vector);
                        PIDController pidController = tank.gameObject.GetComponent<PIDController>();
                        if (pidController && !pidController.AttachedTank.beam.IsActive)
                        {
                            Vector3 localVector = tank.transform.InverseTransformVector(vector);
                            pidController.nonManagedTorque += Vector3.Cross(localVector, -vector2);
                            pidController.nonGravityThrust -= vector2;
                        }
                    }
                }
                return false;
            }
        }

        // Patch Hover thrust
        [HarmonyPatch(typeof(HoverJet))]
        [HarmonyPatch("FixedUpdate")]
        public class PatchHoverJet
        {
            private static FieldInfo grounded = typeof(HoverJet).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(field =>
                        field.CustomAttributes.Any(attr => attr.AttributeType == typeof(CompilerGeneratedAttribute)) &&
                        (field.DeclaringType == typeof(HoverJet).GetProperty("grounded").DeclaringType) &&
                        field.FieldType.IsAssignableFrom(typeof(HoverJet).GetProperty("grounded").PropertyType) &&
                        field.Name.StartsWith("<" + typeof(HoverJet).GetProperty("grounded").Name + ">")
                    );
            private static FieldInfo m_Hover = typeof(HoverJet).GetField("m_Hover", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo parentBlock = typeof(HoverJet).GetField("parentBlock", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_EffectorDir = typeof(HoverJet).GetField("m_EffectorDir", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_CosGroundMaxSlopeAngle = typeof(HoverJet).GetField("m_CosGroundMaxSlopeAngle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_MaxClimbDistance = typeof(HoverJet).GetField("m_MaxClimbDistance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_NormalizedPushForce = typeof(HoverJet).GetField("m_NormalizedPushForce", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_VectoredThrustTransform = typeof(HoverJet).GetField("m_VectoredThrustTransform", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_VectoredThrustMaxForceAngle = typeof(HoverJet).GetField("m_VectoredThrustMaxForceAngle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_ThrustDirUp = typeof(HoverJet).GetField("m_ThrustDirUp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_ThrustDirRight = typeof(HoverJet).GetField("m_ThrustDirRight", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_Drive = typeof(HoverJet).GetField("m_Drive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_Turn = typeof(HoverJet).GetField("m_Turn", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_DampingScale = typeof(HoverJet).GetField("m_DampingScale", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            private static FieldInfo k_LayerIgnoreMask = typeof(HoverJet).GetField("k_LayerIgnoreMask", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            private static FieldInfo s_Hits = typeof(HoverJet).GetField("s_Hits", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            public static bool Prefix(ref HoverJet __instance)
            {
                PatchHoverJet.grounded.SetValue(__instance, false);
                float _Hover = (float)PatchHoverJet.m_Hover.GetValue(__instance);
                if (_Hover == 0f)
                {
                    return false;
                }
                TankBlock _parentBlock = (TankBlock)PatchHoverJet.parentBlock.GetValue(__instance);
                PIDController hoverPID = _parentBlock.tank.gameObject.GetComponent<PIDController>();
                if (hoverPID)
                {
                    Rigidbody rbody = _parentBlock.tank.rbody;
                    Vector3 vector = rbody.position + (__instance.effector.position - rbody.transform.position);
                    float num = __instance.forceRangeMax * _Hover;

                    Vector3 _EffectorDir = (Vector3)PatchHoverJet.m_EffectorDir.GetValue(__instance);

                    RaycastHit[] hits = (RaycastHit[])PatchHoverJet.s_Hits.GetValue(null);
                    int _LayerIgnoreMask = (int)PatchHoverJet.k_LayerIgnoreMask.GetValue(null);

                    int num2 = Physics.SphereCastNonAlloc(new Ray(vector - _EffectorDir * __instance.jetRadius, _EffectorDir), __instance.jetRadius, hits, num, _LayerIgnoreMask, QueryTriggerInteraction.Ignore);
                    float num3 = 0f;
                    float num4 = 0f;
                    for (int i = 0; i < num2; i++)
                    {
                        RaycastHit raycastHit = hits[i];
                        if (raycastHit.distance != 0f && Vector3.Dot(raycastHit.normal, -_EffectorDir) >= (float)PatchHoverJet.m_CosGroundMaxSlopeAngle.GetValue(__instance))
                        {
                            float num5 = Vector3.Dot(vector - raycastHit.point, _EffectorDir);
                            if (num5 <= (float)PatchHoverJet.m_MaxClimbDistance.GetValue(__instance))
                            {
                                float num6 = raycastHit.distance / num;
                                float num7 = __instance.forceFunction.Evaluate(num6);
                                PhysicsModifier component = raycastHit.collider.gameObject.GetComponent<PhysicsModifier>();
                                if (component)
                                {
                                    num7 *= component.HoverForceScale;
                                    if (num5 > component.HoverMaxClimbDistance)
                                    {
                                        goto GOTO_TARGET;
                                    }
                                }
                                num4 = Mathf.Max(1f - num6, num4);
                                num3 = Mathf.Max(num7, num3);
                            }
                        }
                    GOTO_TARGET:;
                    }
                    PatchHoverJet.m_NormalizedPushForce.SetValue(__instance, num4);
                    if (num3 > 0f)
                    {
                        if (PatchHoverJet.m_VectoredThrustTransform.GetValue(__instance) != null)
                        {
                            float d = num3 * __instance.forceMax * Mathf.Sin((float)PatchHoverJet.m_VectoredThrustMaxForceAngle.GetValue(__instance) * 0.0174532924f);

                            Vector3 force1 = (Vector3)PatchHoverJet.m_ThrustDirUp.GetValue(__instance) * (float)PatchHoverJet.m_Drive.GetValue(__instance) * d;
                            Vector3 force2 = (Vector3)PatchHoverJet.m_ThrustDirRight.GetValue(__instance) * (float)PatchHoverJet.m_Turn.GetValue(__instance) * d;
                            rbody.AddForceAtPosition(force1, vector);
                            rbody.AddForceAtPosition(force2, vector);

                            hoverPID.nonGravityThrust += force1;
                            hoverPID.nonGravityThrust += force2;

                            Vector3 localVector = _parentBlock.tank.transform.InverseTransformVector(vector);
                            hoverPID.nonManagedTorque += Vector3.Cross(localVector, force1);
                            hoverPID.nonManagedTorque += Vector3.Cross(localVector, force2);
                        }
                        float num8 = Vector3.Dot(-_EffectorDir, rbody.GetPointVelocity(vector));
                        num3 -= num8 * (float)PatchHoverJet.m_DampingScale.GetValue(__instance);
                        if (num3 > 0f)
                        {
                            Vector3 force3 = -_EffectorDir * num3 * __instance.forceMax;
                            rbody.AddForceAtPosition(force3, vector);
                            hoverPID.nonGravityThrust += force3;

                            Vector3 localVector = _parentBlock.tank.transform.InverseTransformVector(vector);
                            hoverPID.nonManagedTorque += Vector3.Cross(localVector, force3);
                        }
                        PatchHoverJet.grounded.SetValue(__instance, true);
                        _parentBlock.tank.grounded = true;
                    }
                    return false;
                }
                return true;
            }
        }
        #endregion PatchIndependetForces

        #region PatchThrottleThrustAccumulators
        // Patch ModuleLinearMotionEngine Thrust (responds to throttle)
        public class PatchLinearMotionEngine
        {
            private static FieldInfo m_ForcePerEffector = typeof(ModuleLinearMotionEngine).GetField("m_ForcePerEffector", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_Effectors = typeof(ModuleLinearMotionEngine).GetField("m_Effectors", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static MethodInfo OnResetTechPhysics = typeof(ModuleLinearMotionEngine).GetMethod("OnResetTechPhysics", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            public static void GetThrustComponents(ModuleLinearMotionEngine lme, PIDController pid, bool add)
            {
                List<ModuleLinearMotionEngine.Effector> effectorList = (List<ModuleLinearMotionEngine.Effector>)PatchLinearMotionEngine.m_Effectors.GetValue(lme);

                PIDController.GlobalDebugPrint("Finding thrust for: " + lme.block.name);

                float sign = add ? 1f : -1f;
                float effectorForce = (float)PatchLinearMotionEngine.m_ForcePerEffector.GetValue(lme);
                foreach (ModuleLinearMotionEngine.Effector effector in effectorList)
                {
                    PIDController.GlobalDebugPrint("Effector: " + effector.TankLocalBoostDirection.ToString());

                    #region MLE_Y
                    {
                        float vert = effector.TankLocalBoostDirection.y;
                        if (vert > 0)
                        {
                            pid.calculatedThrustPositive.y += sign * effectorForce;
                        }
                        else if (vert < 0)
                        {
                            pid.calculatedThrustNegative.y += sign * effectorForce;
                        }
                    }
                    #endregion MLE_Y

                    #region MLE_X
                    {
                        float strafe = effector.TankLocalBoostDirection.x;
                        if (strafe > 0)
                        {
                            pid.calculatedThrustPositive.x += sign * effectorForce;
                        }
                        else if (strafe < 0)
                        {
                            pid.calculatedThrustNegative.x += sign * effectorForce;
                        }
                    }
                    #endregion MLE_X

                    #region MLE_Z
                    {
                        float accel = effector.TankLocalBoostDirection.z;
                        if (accel > 0)
                        {
                            pid.calculatedThrustPositive.z += sign * effectorForce;
                        }
                        else if (accel < 0)
                        {
                            pid.calculatedThrustNegative.z += sign * effectorForce;
                        }
                    }
                    #endregion MLE_Z
                }
            }

            [HarmonyPatch(typeof(ModuleLinearMotionEngine))]
            [HarmonyPatch("OnAttach")]
            public class PatchMLEAttach
            {
                public static void Postfix(ref ModuleLinearMotionEngine __instance)
                {
                    PIDController hoverPID = __instance.block.tank.GetComponent<PIDController>();
                    if (hoverPID)
                    {
                        PatchLinearMotionEngine.OnResetTechPhysics.Invoke(__instance, null);
                        PatchLinearMotionEngine.GetThrustComponents(__instance, hoverPID, true);
                    }
                }
            }

            [HarmonyPatch(typeof(ModuleLinearMotionEngine))]
            [HarmonyPatch("OnDetach")]
            public class PatchMLEDetach
            {
                public static void Postfix(ref ModuleLinearMotionEngine __instance)
                {
                    PIDController hoverPID = __instance.block.tank.GetComponent<PIDController>();
                    if (hoverPID)
                    {
                        PatchLinearMotionEngine.GetThrustComponents(__instance, hoverPID, false);
                    }
                }
            }
        }

        // Patch ModuleBooster Thrust (responds to throttle)
        public class PatchBooster
        {
            private static FieldInfo jets = typeof(ModuleBooster).GetField("jets", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo fans = typeof(ModuleBooster).GetField("fans", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_Force = typeof(BoosterJet).GetField("m_Force", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo boosterEffector = typeof(BoosterJet).GetField("m_Effector", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo fanEffector = typeof(FanJet).GetField("m_Effector", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_FireStrengthCurrent = typeof(BoosterJet).GetField("m_FireStrengthCurrent", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_ParentBlock = typeof(BoosterJet).GetField("m_ParentBlock", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_FireControl = typeof(BoosterJet).GetField("m_FireControl", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_BurnRate = typeof(BoosterJet).GetField("m_BurnRate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_JetConsumesFuel = typeof(BoosterJet).GetField("m_ConsumesFuel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            private static FieldInfo m_IsFiringSteer = typeof(ModuleBooster).GetField("m_IsFiringSteer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_IsFiringBoost = typeof(ModuleBooster).GetField("m_IsFiringBoost", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_UseDriveControls = typeof(ModuleBooster).GetField("m_UseDriveControls", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_UseBoostControls = typeof(ModuleBooster).GetField("m_UseBoostControls", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_EnablesThrottleControl = typeof(ModuleBooster).GetField("m_EnablesThrottleControl", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo m_ConsumesFuel = typeof(ModuleBooster).GetField("m_ConsumesFuel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            private static MethodInfo OnResetTechPhysics = typeof(ModuleBooster).GetMethod("OnResetTechPhysics", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            public static void GetThrustComponents(ModuleBooster booster, PIDController pid, bool add)
            {
                List<BoosterJet> jetList = (List<BoosterJet>)PatchBooster.jets.GetValue(booster);
                List<FanJet> fanList = (List<FanJet>)PatchBooster.fans.GetValue(booster);

                PIDController.GlobalDebugPrint("Finding thrust for: " + booster.block.name);

                float sign = add ? 1f : -1f;

                foreach (BoosterJet jet in jetList)
                {
                    PIDController.GlobalDebugPrint("Jet: " + jet.LocalBoostDirection.ToString());
                    float force = (float)PatchBooster.m_Force.GetValue(jet);

                    #region BoosterY
                    {
                        float vert = jet.LocalBoostDirection.y;
                        if (vert > 0)
                        {
                            pid.calculatedThrustPositive.y += sign * force;
                        }
                        else if (vert < 0)
                        {
                            pid.calculatedThrustNegative.y += sign * force;
                        }
                    }
                    #endregion BoosterY

                    #region BoosterX
                    {
                        float strafe = jet.LocalBoostDirection.x;
                        if (strafe > 0)
                        {
                            pid.calculatedThrustPositive.x += sign * force;
                        }
                        else if (strafe < 0)
                        {
                            pid.calculatedThrustNegative.x += sign * force;
                        }
                    }
                    #endregion BoosterX

                    #region BoosterZ
                    {
                        float accel = jet.LocalBoostDirection.z;
                        if (accel > 0)
                        {
                            
                            pid.calculatedThrustPositive.z += sign * force;
                        }
                        else if (accel < 0)
                        {
                            pid.calculatedThrustNegative.z += sign * force;
                        }
                    }
                    #endregion BoosterZ
                }
                foreach (FanJet fan in fanList)
                {
                    PIDController.GlobalDebugPrint("Fan: " + fan.LocalBoostDirection.ToString());

                    float force = fan.force;
                    float backForce = fan.backForce;

                    #region FanY
                    {
                        float vert = fan.LocalBoostDirection.y;
                        if (vert > 0)
                        {
                            pid.calculatedThrustPositive.y += sign * force;
                            pid.calculatedThrustNegative.y += sign * backForce;
                        }
                        else if (vert < 0)
                        {
                            pid.calculatedThrustPositive.y += sign * backForce;
                            pid.calculatedThrustNegative.y += sign * force;
                        }
                    }
                    #endregion FanY

                    #region FanX
                    {
                        float strafe = fan.LocalBoostDirection.x;
                        if (strafe > 0)
                        {
                            pid.calculatedThrustPositive.x += sign * force;
                            pid.calculatedThrustNegative.x += sign * backForce;
                        }
                        else if (strafe < 0)
                        {
                            pid.calculatedThrustPositive.x += sign * backForce;
                            pid.calculatedThrustNegative.x += sign * force;
                        }
                    }
                    #endregion FanX

                    #region FanZ
                    {
                        float accel = fan.LocalBoostDirection.z;
                        if (accel > 0)
                        {
                            pid.calculatedThrustPositive.z += sign * force;
                            pid.calculatedThrustNegative.z += sign * backForce;
                        }
                        else if (accel < 0)
                        {
                            pid.calculatedThrustPositive.z += sign * backForce;
                            pid.calculatedThrustNegative.z += sign * force;
                        }
                    }
                    #endregion FanZ
                }
            }

            public static void GetTorqueComponents(ModuleBooster booster, PIDController pid, bool add)
            {
                List<BoosterJet> jetList = (List<BoosterJet>)PatchBooster.jets.GetValue(booster);
                List<FanJet> fanList = (List<FanJet>)PatchBooster.fans.GetValue(booster);

                PIDController.GlobalDebugPrint("Finding torque for: " + booster.block.name);

                float sign = add ? 1f : -1f;

                foreach (BoosterJet jet in jetList)
                {
                    PIDController.GlobalDebugPrint("Jet: " + jet.RotationContribution.ToString());
                    float force = (float)PatchBooster.m_Force.GetValue(jet);
                    Transform effector = (Transform)PatchBooster.boosterEffector.GetValue(jet);
                    Vector3 localDirection = pid.AttachedTank.transform.InverseTransformVector(effector.forward);
                    Vector3 localPosition = pid.AttachedTank.transform.InverseTransformVector(effector.position - pid.AttachedTank.WorldCenterOfMass);

                    Vector3 torque = Vector3.Cross(localPosition, force * localDirection);
                    PIDController.GlobalDebugPrint($"    Torque: {torque}");

                    #region BoosterY
                    {
                        float yaw = jet.RotationContribution.y;
                        if (yaw > 0)
                        {
                            pid.calculatedTorquePositive.y += sign * torque.y;
                        }
                        else if (yaw < 0)
                        {
                            pid.calculatedTorqueNegative.y += -sign * torque.y;
                        }
                    }
                    #endregion BoosterY

                    #region BoosterX
                    {
                        float pitch = jet.RotationContribution.x;
                        if (pitch > 0)
                        {
                            pid.calculatedTorquePositive.x += sign * torque.x;
                        }
                        else if (pitch < 0)
                        {
                            pid.calculatedTorqueNegative.x += -sign * torque.x;
                        }
                    }
                    #endregion BoosterX

                    #region BoosterZ
                    {
                        float roll = jet.RotationContribution.z;
                        if (roll > 0)
                        {
                            pid.calculatedTorquePositive.z += sign * torque.z;
                        }
                        else if (roll < 0)
                        {
                            pid.calculatedTorqueNegative.z += -sign * torque.z;
                        }
                    }
                    #endregion BoosterZ
                }
                foreach (FanJet fan in fanList)
                {
                    PIDController.GlobalDebugPrint("Fan: " + fan.RotationContribution.ToString());

                    float force = fan.force;
                    float backForce = fan.backForce;

                    Transform effector = (Transform)PatchBooster.fanEffector.GetValue(fan);
                    Vector3 localDirection = pid.AttachedTank.transform.InverseTransformVector(effector.forward);
                    Vector3 localPosition = pid.AttachedTank.transform.InverseTransformVector(effector.position - pid.AttachedTank.WorldCenterOfMass);

                    Vector3 torque = Vector3.Cross(localPosition, force * localDirection);
                    Vector3 backTorque = Vector3.Cross(localPosition, backForce * -localDirection);
                    PIDController.GlobalDebugPrint($"    Torque: {torque}");
                    PIDController.GlobalDebugPrint($"    Neg Torque: {backTorque}");

                    #region FanY
                    {
                        float yaw = fan.RotationContribution.y;
                        if (yaw > 0)
                        {
                            pid.calculatedTorquePositive.y += sign * torque.y;
                            pid.calculatedTorqueNegative.y += -sign * backTorque.y;
                        }
                        else if (yaw < 0)
                        {
                            pid.calculatedTorquePositive.y += sign * backTorque.y;
                            pid.calculatedTorqueNegative.y += -sign * torque.y;
                        }
                    }
                    #endregion FanY

                    #region FanX
                    {
                        float pitch = fan.RotationContribution.x;
                        if (pitch > 0)
                        {
                            pid.calculatedTorquePositive.x += sign * torque.x;
                            pid.calculatedTorqueNegative.x += -sign * backTorque.x;
                        }
                        else if (pitch < 0)
                        {
                            pid.calculatedTorquePositive.x += sign * backTorque.x;
                            pid.calculatedTorqueNegative.x += -sign * torque.x;
                        }
                    }
                    #endregion FanX

                    #region FanZ
                    {
                        float roll = fan.RotationContribution.z;
                        if (roll > 0)
                        {
                            pid.calculatedTorquePositive.z += sign * torque.z;
                            pid.calculatedTorqueNegative.z += -sign * backTorque.z;
                        }
                        else if (roll < 0)
                        {
                            pid.calculatedTorquePositive.z += sign * backTorque.z;
                            pid.calculatedTorqueNegative.z += -sign * torque.z;
                        }
                    }
                    #endregion FanZ
                }
            }

            // Patch FanJet Thrust to add to thrust count
            [HarmonyPatch(typeof(ModuleBooster))]
            [HarmonyPatch("OnAttach")]
            public class PatchBoosterAttach
            {
                public static void Postfix(ref ModuleBooster __instance)
                {
                    PIDController pidController = __instance.block.tank.GetComponent<PIDController>();
                    if (pidController)
                    {
                        PatchBooster.OnResetTechPhysics.Invoke(__instance, null);
                        PatchBooster.GetThrustComponents(__instance, pidController, true);
                        PatchBooster.GetTorqueComponents(__instance, pidController, true);
                    }
                }
            }

            // Patch BoosterJet Thrust to add to thrust count
            [HarmonyPatch(typeof(ModuleBooster))]
            [HarmonyPatch("OnDetach")]
            public class PatchBoosterDetach
            {
                public static void Postfix(ref ModuleBooster __instance)
                {
                    PIDController pidController = __instance.block.tank.GetComponent<PIDController>();
                    if (pidController)
                    {
                        PatchBooster.GetThrustComponents(__instance, pidController, false);
                        PatchBooster.GetTorqueComponents(__instance, pidController, false);
                    }
                }
            }

            // Patch ModuleBooster DriveControlInput to set BoosterJet's individual throttles
            [HarmonyPatch(typeof(ModuleBooster))]
            [HarmonyPatch("DriveControlInput")]
            public class PatchDriveControl
            {
                public static bool Prefix(ref ModuleBooster __instance, ref TankControl.ControlState driveData)
                {
                    PIDController pidController = __instance.block.tank.GetComponent<PIDController>();
                    if (pidController)
                    {
                        if ((pidController.HoverPID == null || !pidController.HoverPID.enabled)
                            && (pidController.AccelPID == null || !pidController.AccelPID.enabled)
                            && (pidController.StrafePID == null || !pidController.StrafePID.enabled)
                            && (pidController.PitchPID == null || !pidController.PitchPID.enabled)
                            && (pidController.RollPID == null || !pidController.RollPID.enabled)
                            && (pidController.YawPID == null || !pidController.YawPID.enabled)
                        )
                        {
                            return true;
                        }
                        else
                        {
                            if (!__instance.enabled)
                            {
                                return false;
                            }
                            PatchBooster.m_IsFiringSteer.SetValue(__instance, false);
                            PatchBooster.m_IsFiringBoost.SetValue(__instance, false);
                            bool useDriveControls = (bool)PatchBooster.m_UseDriveControls.GetValue(__instance);
                            bool useBoostControls = (bool)PatchBooster.m_UseBoostControls.GetValue(__instance);

                            bool isFiringBoost = false;

                            List<BoosterJet> jetList = (List<BoosterJet>)PatchBooster.jets.GetValue(__instance);
                            List<FanJet> fanList = (List<FanJet>)PatchBooster.fans.GetValue(__instance);

                            // Leave as is, with minor changes to minimize reflection calls
                            bool shouldBeAutoStabilised = __instance.block.tank.ShouldAutoStabilise && useDriveControls && !(bool)PatchBooster.m_EnablesThrottleControl.GetValue(__instance) && !driveData.AnyMovementOrBoostControl;
                            foreach (FanJet fanJet in fanList)
                            {
                                float num = 0f;
                                if (useDriveControls)
                                {
                                    float num2 = Vector3.Dot(driveData.InputRotation, fanJet.RotationContribution);
                                    float num3 = Vector3.Dot(driveData.InputMovement + driveData.Throttle, fanJet.LocalBoostDirection);
                                    num = Mathf.Clamp(num2 + num3, -1f, 1f);
                                    if (num != 0f)
                                    {
                                        PatchBooster.m_IsFiringSteer.SetValue(__instance, true);
                                    }
                                    if (driveData.BoostProps && num3 >= 0f && useBoostControls)
                                    {
                                        PatchBooster.m_IsFiringBoost.SetValue(__instance, true);
                                        num = 1f;
                                        isFiringBoost = true;
                                    }
                                }
                                else if (useBoostControls)
                                {
                                    num = (driveData.BoostProps ? 1f : 0f);
                                    isFiringBoost = (isFiringBoost || driveData.BoostProps);
                                }
                                fanJet.SetSpin(num);
                                if (shouldBeAutoStabilised)
                                {
                                    fanJet.AutoStabiliseTank();
                                }
                            }
                            PatchBooster.m_IsFiringBoost.SetValue(__instance, isFiringBoost);

                            // If there is a throttle in the current direction, then forcibly set firing strength
                            foreach (BoosterJet boosterJet in jetList)
                            {
                                bool isRequestedByDriveControl = false;
                                if (!(bool) PatchBooster.m_ConsumesFuel.GetValue(__instance) || !__instance.block.tank.Boosters.FuelBurnedOut)
                                {
                                    if (useDriveControls)
                                    {
                                        float rotationalContribution = Vector3.Dot(driveData.InputRotation, boosterJet.RotationContribution);
                                        float linearContribution = Vector3.Dot(driveData.InputMovement + driveData.Throttle, boosterJet.LocalBoostDirection);
                                        float totalContribution = Mathf.Clamp(rotationalContribution + linearContribution, 0f, 1f);
                                        if (useBoostControls && driveData.BoostJets)
                                        {
                                            isRequestedByDriveControl = (linearContribution >= 0f);
                                            if (isRequestedByDriveControl)
                                            {
                                                PatchBooster.m_FireStrengthCurrent.SetValue(boosterJet, 1f);
                                            }
                                        }
                                        else
                                        {
                                            // flag2 = (num6 > 0.1f);
                                            if (totalContribution > 0f)
                                            {
                                                isRequestedByDriveControl = true;
                                                PatchBooster.m_FireStrengthCurrent.SetValue(boosterJet, totalContribution);
                                            }
                                        }
                                    }
                                    else if (useBoostControls)
                                    {
                                        isRequestedByDriveControl = driveData.BoostJets;
                                        isFiringBoost = (isFiringBoost || isRequestedByDriveControl);
                                        if (isFiringBoost)
                                        {
                                            PatchBooster.m_FireStrengthCurrent.SetValue(boosterJet, 1f);
                                        }
                                    }
                                }
                                boosterJet.SetFiring(isRequestedByDriveControl);
                                if (shouldBeAutoStabilised)
                                {
                                    boosterJet.AutoStabiliseTank();
                                }
                            }
                            PatchBooster.m_IsFiringBoost.SetValue(__instance, isFiringBoost);
                            return false;
                        }
                    }
                    return true;
                }
            }

            // Patch BoosterJet to use throttle
            [HarmonyPatch(typeof(BoosterJet))]
            [HarmonyPatch("FixedUpdate")]
            public class PatchBoosterJet
            {
                public static bool Prefix(ref BoosterJet __instance)
                {
                    TankBlock parentBlock = (TankBlock)PatchBooster.m_ParentBlock.GetValue(__instance);
                    if (parentBlock != null) {
                    Tank parentTank = parentBlock.tank;
                        if (parentTank != null)
                        {
                            PIDController pidController = parentTank.GetComponent<PIDController>();
                            if (pidController)
                            {
                                Vector3 localBoost = __instance.LocalBoostDirection;
                                Vector3 localRot = __instance.RotationContribution;

                                if ((localBoost.y != 0f && pidController.HoverPID != null && pidController.HoverPID.enabled)
                                    || (localBoost.z != 0f && pidController.AccelPID != null && pidController.AccelPID.enabled)
                                    || (localBoost.x != 0f && pidController.StrafePID != null && pidController.StrafePID.enabled)
                                    || (localRot.x != 0f && pidController.PitchPID != null && pidController.PitchPID.enabled)
                                    || (localRot.z != 0f && pidController.RollPID != null && pidController.RollPID.enabled)
                                    || (localRot.y != 0f && pidController.YawPID != null && pidController.YawPID.enabled)
                                )
                                {
                                    float fireStrengthCurrent = (float)PatchBooster.m_FireStrengthCurrent.GetValue(__instance);
                                    if (!(bool) PatchBooster.m_FireControl.GetValue(__instance)) {
                                        PatchBooster.m_FireStrengthCurrent.SetValue(__instance, 0f);
                                        return false;
                                    }
                                    if (fireStrengthCurrent == 0f)
                                    {
                                        return false;
                                    }
                                    float num = (float) PatchBooster.m_Force.GetValue(__instance) * fireStrengthCurrent;
                                    Rigidbody rigidbody;

                                    TechBooster techBooster = parentTank.Boosters;
                                    if ((bool) PatchBooster.m_JetConsumesFuel.GetValue(__instance))
                                    {
                                        techBooster.Burn(fireStrengthCurrent * (float) PatchBooster.m_BurnRate.GetValue(__instance) * Time.deltaTime);
                                    }
                                    rigidbody = parentBlock.tank.rbody;

                                    if (rigidbody != null && num != 0f)
                                    {
                                        Transform effector = (Transform) PatchBooster.boosterEffector.GetValue(__instance);
                                        Vector3 position = rigidbody.position + (effector.position - rigidbody.transform.position);
                                        rigidbody.AddForceAtPosition(-effector.forward * num, position);
                                    }
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                }
            }

            // Patch FanJet to use throttle
            [HarmonyPatch(typeof(FanJet))]
            [HarmonyPatch("FixedUpdate")]
            public class PatchFanJet
            {
                public static bool Prefix(ref FanJet __instance)
                {
                    return true;
                }
            }
        }
        #endregion PatchThrottleThrustAccumulators
    }
}
