using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony;
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
                            Singleton.Manager<ManPauseGame>.inst.TogglePauseMenu();
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

            /* public static void Postfix(ref TankControl __instance, ref Vector3 __state)
            {
                if (!__state.Equals(PatchTankControl.defaultState))
                {
                    Vector3 detectedThrottle = (Vector3)PatchTankControl.m_Throttle.GetValue(__instance);
                    TankControl.ControlState controlState = (TankControl.ControlState)PatchTankControl.m_ControlState.GetValue(__instance);

                    Console.WriteLine($"    POSTFIX THROTTLE START: {detectedThrottle}");
                    Console.WriteLine($"    POSTFIX INPUTMOVEMENT START: {controlState.m_State.m_InputMovement}");

                    float heightChange = detectedThrottle.y;
                    float strafeChange = detectedThrottle.x;
                    float accelChange = detectedThrottle.z;


                    detectedThrottle.x = __state.x;
                    detectedThrottle.z = __state.z;

                    Vector3 oldInput = (Vector3)PatchTankControl.m_ThrottleInput.GetValue(__instance);
                    Vector3 oldTiming = (Vector3)PatchTankControl.m_ThrottleTiming.GetValue(__instance);
                    PIDController pid = __instance.Tech.gameObject.GetComponent<PIDController>();

                    // if detect input, change target height
                    if (heightChange != 0f)
                    {
                        if (pid.HoverPID != null)
                        {
                            if (pid.useTargetHeight)
                            {
                                pid.targetHeight += Mathf.Sign(heightChange) * Time.deltaTime * pid.manualTargetChangeRate;
                                pid.PropagateUpdatedHoverParameters();
                            }
                            else
                            {
                                controlState.m_State.m_InputMovement.y += heightChange;
                            }
                            detectedThrottle.y = __state.y;
                        }
                        else
                        {
                            detectedThrottle.y = Mathf.Clamp(__state.y + detectedThrottle.y, -1f, 1f);
                        }

                        oldInput.y = 0f;
                        oldTiming.y = 0f;

                        PatchTankControl.m_ThrottleInput.SetValue(__instance, new Vector3(oldInput.x, 0f, oldInput.z));
                        PatchTankControl.m_ThrottleTiming.SetValue(__instance, new Vector3(oldTiming.x, 0f, oldTiming.z));
                    }

                    // if detect input, shunt it to m_Input
                    if (strafeChange != 0f)
                    {
                        if (pid.StrafePID != null)
                        {
                            controlState.m_State.m_InputMovement.x += strafeChange;
                            detectedThrottle.x = __state.x;
                        }

                        oldInput.x = 0f;
                        oldTiming.x = 0f;

                        PatchTankControl.m_ThrottleInput.SetValue(__instance, new Vector3(0f, oldInput.y, oldInput.z));
                        PatchTankControl.m_ThrottleTiming.SetValue(__instance, new Vector3(0f, oldTiming.y, oldTiming.z));
                    }
                    if (accelChange != 0f)
                    {
                        if (pid.AccelPID != null)
                        {
                            __instance.DriveControl += accelChange;
                            detectedThrottle.z = __state.z;
                        }

                        PatchTankControl.m_ThrottleInput.SetValue(__instance, new Vector3(oldInput.x, oldInput.y, 0f));
                        PatchTankControl.m_ThrottleTiming.SetValue(__instance, new Vector3(oldTiming.x, oldTiming.y, 0f));
                    }

                    Console.WriteLine($"    POSTFIX THROTTLE END: {detectedThrottle}");
                    PatchTankControl.m_Throttle.SetValue(__instance, detectedThrottle);
                    controlState.m_State.m_ThrottleValues.x = detectedThrottle.x;
                    controlState.m_State.m_ThrottleValues.y = detectedThrottle.y;
                    controlState.m_State.m_ThrottleValues.z = detectedThrottle.z;
                    Console.WriteLine($"    POSTFIX INPUTMOVEMENT END: {controlState.m_State.m_InputMovement}");
                    Console.WriteLine("");
                }
            } */
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
                    if (pidController != null && pidController.HoverPID != null && pidController.useTargetHeight)
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
                                    // insertion: check if there's a tank with a HoverPID
                                    PIDController hoverPID = applicationRigidbody.gameObject.GetComponent<PIDController>();
                                    if (hoverPID && !hoverPID.AttachedTank.beam.IsActive)
                                    {
                                        // num is the new gravity force, replaces default gravity
                                        // gravity is effectively nullified for the block mass now, uses this isntead
                                        hoverPID.nonGravityThrust += new Vector3(0, num - normalGravity, 0);
                                    }
                                    applicationRigidbody.AddForceAtPosition(new Vector3(0f, num, 0f), gravityApplicationTarget.GetWorldCentreOfGravity());
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
                    PIDController hoverPID = __instance.block.tank.gameObject.GetComponent<PIDController>();
                    if (hoverPID)
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
                        hoverPID.nonGravityThrust += force;
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

                        PIDController hoverPID = tank.gameObject.GetComponent<PIDController>();
                        if (hoverPID && !hoverPID.AttachedTank.beam.IsActive)
                        {
                            hoverPID.nonGravityThrust -= vector2;
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
                        }
                        float num8 = Vector3.Dot(-_EffectorDir, rbody.GetPointVelocity(vector));
                        num3 -= num8 * (float)PatchHoverJet.m_DampingScale.GetValue(__instance);
                        if (num3 > 0f)
                        {
                            Vector3 force3 = -_EffectorDir * num3 * __instance.forceMax;
                            rbody.AddForceAtPosition(force3, vector);
                            hoverPID.nonGravityThrust += force3;
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

                    #region BoosterY
                    {
                        float vert = jet.LocalBoostDirection.y;
                        if (vert > 0)
                        {
                            float force = (float)PatchBooster.m_Force.GetValue(jet);
                            pid.calculatedThrustPositive.y += sign * force;
                        }
                        else if (vert < 0)
                        {
                            float force = (float)PatchBooster.m_Force.GetValue(jet);
                            pid.calculatedThrustNegative.y += sign * force;
                        }
                    }
                    #endregion BoosterY

                    #region BoosterX
                    {
                        float strafe = jet.LocalBoostDirection.x;
                        if (strafe > 0)
                        {
                            float force = (float)PatchBooster.m_Force.GetValue(jet);
                            pid.calculatedThrustPositive.x += sign * force;
                        }
                        else if (strafe < 0)
                        {
                            float force = (float)PatchBooster.m_Force.GetValue(jet);
                            pid.calculatedThrustNegative.x += sign * force;
                        }
                    }
                    #endregion BoosterX

                    #region BoosterZ
                    {
                        float accel = jet.LocalBoostDirection.z;
                        if (accel > 0)
                        {
                            float force = (float)PatchBooster.m_Force.GetValue(jet);
                            pid.calculatedThrustPositive.z += sign * force;
                        }
                        else if (accel < 0)
                        {
                            float force = (float)PatchBooster.m_Force.GetValue(jet);
                            pid.calculatedThrustNegative.z += sign * force;
                        }
                    }
                    #endregion BoosterZ
                }
                foreach (FanJet fan in fanList)
                {
                    PIDController.GlobalDebugPrint("Fan: " + fan.LocalBoostDirection.ToString());

                    #region FanY
                    {
                        float vert = fan.LocalBoostDirection.y;
                        if (vert > 0)
                        {
                            pid.calculatedThrustPositive.y += sign * fan.force;
                            pid.calculatedThrustNegative.y += sign * fan.backForce;
                        }
                        else if (vert < 0)
                        {
                            pid.calculatedThrustPositive.y += sign * fan.backForce;
                            pid.calculatedThrustNegative.y += sign * fan.force;
                        }
                    }
                    #endregion FanY

                    #region FanX
                    {
                        float strafe = fan.LocalBoostDirection.x;
                        if (strafe > 0)
                        {
                            pid.calculatedThrustPositive.x += sign * fan.force;
                            pid.calculatedThrustNegative.x += sign * fan.backForce;
                        }
                        else if (strafe < 0)
                        {
                            pid.calculatedThrustPositive.x += sign * fan.backForce;
                            pid.calculatedThrustNegative.x += sign * fan.force;
                        }
                    }
                    #endregion FanX

                    #region FanZ
                    {
                        float accel = fan.LocalBoostDirection.z;
                        if (accel > 0)
                        {
                            pid.calculatedThrustPositive.z += sign * fan.force;
                            pid.calculatedThrustNegative.z += sign * fan.backForce;
                        }
                        else if (accel < 0)
                        {
                            pid.calculatedThrustPositive.z += sign * fan.backForce;
                            pid.calculatedThrustNegative.z += sign * fan.force;
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
                    PIDController hoverPID = __instance.block.tank.GetComponent<PIDController>();
                    if (hoverPID)
                    {
                        PatchBooster.OnResetTechPhysics.Invoke(__instance, null);
                        PatchBooster.GetThrustComponents(__instance, hoverPID, true);
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
                    PIDController hoverPID = __instance.block.tank.GetComponent<PIDController>();
                    if (hoverPID)
                    {
                        PatchBooster.GetThrustComponents(__instance, hoverPID, false);
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
                                        fireStrengthCurrent = 0f;
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

            // Patch FixedUpdate to use throttle
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
