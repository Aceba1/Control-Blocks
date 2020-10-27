using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Control_Block
{
    internal class OptionMenuHoverPID : MonoBehaviour
    {
        public OptionMenuHoverPID()
        {
            inst = this;
        }
        public static OptionMenuHoverPID inst;

        private readonly int ID = 9184;

        private bool visible = false;

        private ModulePID module;

        private Rect win;

        private float mouseX, mouseY;

        private string targetHeightStr, targetChangeRateStr,
            targetPitchStr, targetRollStr,
            hoverKP, hoverKI, hoverKD,
            strafeKP, strafeKI, strafeKD,
            accelKP, accelKI, accelKD,
            pitchKP, pitchKI, pitchKD,
            rollKP, rollKI, rollKD,
            yawKP, yawKI, yawKD;

        private bool hoverEnabled, hoverPresent,
            strafeEnabled, strafePresent,
            accelEnabled, accelPresent,
            pitchEnabled, pitchPresent,
            rollEnabled, rollPresent,
            yawEnabled, yawPresent;

        private bool[] enableBools = new bool[6];

        private int rowTiling = 3;

        private void Update()
        {
            if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetMouseButtonDown(1))
            {
                this.mouseX = Input.mousePosition.x;
                this.mouseY = Input.mousePosition.y;
                this.win = new Rect(this.mouseX, Screen.height - this.mouseY - 75f, 300f, 425f);
                try
                {
                    this.module = Singleton.Manager<ManPointer>.inst.targetVisible.block.GetComponent<ModulePID>();
                }
                catch
                {
                    this.module = null;
                }
                this.visible = this.module;
                if (this.visible)
                {
                    this.targetHeightStr = this.module.targetHeight.ToString();
                    this.targetChangeRateStr = this.module.manualTargetChangeRate.ToString();

                    if (this.module.m_HoverParameters != null)
                    {
                        this.hoverKP = this.module.m_HoverParameters.kP.ToString();
                        this.hoverKI = this.module.m_HoverParameters.kI.ToString();
                        this.hoverKD = this.module.m_HoverParameters.kD.ToString();
                    }
                    if (this.module.m_AccelParameters != null)
                    {
                        this.accelKP = this.module.m_AccelParameters.kP.ToString();
                        this.accelKI = this.module.m_AccelParameters.kI.ToString();
                        this.accelKD = this.module.m_AccelParameters.kD.ToString();
                    }
                    if (this.module.m_StrafeParameters != null)
                    {
                        this.strafeKP = this.module.m_StrafeParameters.kP.ToString();
                        this.strafeKI = this.module.m_StrafeParameters.kI.ToString();
                        this.strafeKD = this.module.m_StrafeParameters.kD.ToString();
                    }
                    if (this.module.m_PitchParameters != null)
                    {
                        this.pitchKP = this.module.m_PitchParameters.kP.ToString();
                        this.pitchKI = this.module.m_PitchParameters.kI.ToString();
                        this.pitchKD = this.module.m_PitchParameters.kD.ToString();
                    }
                    if (this.module.m_RollParameters != null)
                    {
                        this.rollKP = this.module.m_RollParameters.kP.ToString();
                        this.rollKI = this.module.m_RollParameters.kI.ToString();
                        this.rollKD = this.module.m_RollParameters.kD.ToString();
                    }
                    if (this.module.m_YawParameters != null)
                    {
                        this.yawKP = this.module.m_YawParameters.kP.ToString();
                        this.yawKI = this.module.m_YawParameters.kI.ToString();
                        this.yawKD = this.module.m_YawParameters.kD.ToString();
                    }

                    this.win = this.UpdateWinSize();
                }
            }
        }

        public bool check_OnGUI()
        {
            return this.visible && this.module;
        }

        private Rect UpdateWinSize()
        {
            int panels = 0;
            if (this.module.m_HoverParameters != null)
            {
                this.hoverEnabled = this.module.m_HoverParameters.enabled;
                this.enableBools[panels] = this.hoverEnabled;
                panels += 1;
            }
            if (this.module.m_AccelParameters != null)
            {
                this.accelEnabled = this.module.m_AccelParameters.enabled;
                this.enableBools[panels] = this.accelEnabled;
                panels += 1;
            }
            if (this.module.m_StrafeParameters != null)
            {
                this.strafeEnabled = this.module.m_StrafeParameters.enabled;
                this.enableBools[panels] = this.strafeEnabled;
                panels += 1;
            }
            if (this.module.m_RollParameters != null)
            {
                this.rollEnabled = this.module.m_RollParameters.enabled;
                this.enableBools[panels] = this.rollEnabled;
                panels += 1;
            }
            if (this.module.m_PitchParameters != null)
            {
                this.pitchEnabled = this.module.m_PitchParameters.enabled;
                this.enableBools[panels] = this.pitchEnabled;
                panels += 1;
            }
            if (this.module.m_YawParameters != null)
            {
                this.yawEnabled = this.module.m_YawParameters.enabled;
                this.enableBools[panels] = this.yawEnabled;
                panels += 1;
            }

            if (panels < 4)
            {
                this.rowTiling = panels;
            }
            else if (panels == 4)
            {
                this.rowTiling = 2;
            }
            else
            {
                this.rowTiling = 3;
            }
            bool row1_enable = false;
            bool row2_enable = false;
            for (int i = 0; i < panels; i++)
            {
                if (this.enableBools[i])
                {
                    if (i < this.rowTiling)
                    {
                        row1_enable = true;
                    }
                    else
                    {
                        row2_enable = true;
                    }
                }
            }
            float xSize = 300f * this.rowTiling;
            float ySize = Mathf.Max((row1_enable ? 400f : 80f) + (panels > 3 ? (row2_enable ? 300f : 80f) + 16f : 0f) + 40f + 60f, 100f);
            return new Rect(mouseX, Screen.height - mouseY - 75f, xSize, ySize);
        }

        public void stack_OnGUI()
        {
            if (!this.visible || !this.module)
            {
                return;
            }

            try
            {
                this.win = GUI.Window(ID, this.win, new GUI.WindowFunction(DoWindow), "<size=40>PID Control</size>");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void DoWindow(int id)
        {
            if (this.module == null)
            {
                this.visible = false;
                return;
            }

            int currPanelsCount = 0;
            GUILayout.Space(35);
            GUILayout.BeginHorizontal();
            if (this.module.m_HoverParameters != null)
            {
                GUILayout.BeginVertical(GUILayout.Width(260f), GUILayout.MaxWidth(260f), GUILayout.MinHeight(50f), GUILayout.MaxHeight(425f));
                // reset target height here, since it may change elsewhere
                this.targetHeightStr = module.targetHeight.ToString();
                GUILayout.Label("<size=20>Hover PID</size>");

                if (this.hoverEnabled)
                {
                    if (GUIOverseer.TextSliderPair("Target Height: ", ref this.targetHeightStr, ref this.module.targetHeight, 0f, 200f, false))
                    {
                        module.OnGUIUpdateHover();
                    }

                    if (GUIOverseer.TextSliderPair("Proportional Gain: ", ref this.hoverKP, ref this.module.m_HoverParameters.kP, 0f, 100f, false, 0.05f))
                    {
                        module.OnGUIUpdateHover();
                    }

                    if (GUIOverseer.TextSliderPair("Integral Gain: ", ref this.hoverKI, ref this.module.m_HoverParameters.kI, 0f, 10f, false, 0.05f))
                    {
                        module.OnGUIUpdateHover();
                    }

                    if (GUIOverseer.TextSliderPair("Derivative Gain: ", ref this.hoverKD, ref this.module.m_HoverParameters.kD, 0f, 100f, false, 0.05f))
                    {
                        module.OnGUIUpdateHover();
                    }

                    if (GUIOverseer.TextSliderPair("Manual Target Change Rate: ", ref this.targetChangeRateStr, ref this.module.manualTargetChangeRate, 10f, 100f, true, 0.05f))
                    {
                        module.OnGUIUpdateHover();
                    }

                    bool toggleStaticHeight = GUILayout.Toggle(this.module.staticHeight, " Use static altitude");
                    if (toggleStaticHeight != this.module.staticHeight)
                    {
                        this.module.staticHeight = toggleStaticHeight;
                        this.module.OnGUIUpdateHover();
                    }
                    if (GUILayout.Button("Target Current Height"))
                    {
                        this.module.staticHeight = true;
                        this.module.targetHeight = this.module.block.centreOfMassWorld.y;
                        this.module.OnGUIUpdateHover();
                        this.module.OnResetHoverError();
                    }

                    bool toggleDebug = GUILayout.Toggle(this.module.m_HoverParameters.debug, " Enable debug mode");
                    if (toggleDebug != this.module.m_HoverParameters.debug)
                    {
                        this.module.m_HoverParameters.debug = toggleDebug;
                        this.module.OnGUIUpdateHover();
                    }

                    if (GUILayout.Button("Reset PID Error"))
                    {
                        this.module.OnResetHoverError();
                    }

                    bool toggleFixedHeight = GUILayout.Toggle(this.module.useTargetHeight, " Target Fixed Height");
                    if (toggleFixedHeight != this.module.useTargetHeight)
                    {
                        this.module.useTargetHeight = toggleFixedHeight;
                        this.module.OnGUIUpdateHover();
                        this.module.OnResetHoverError();
                    }

                    if (!toggleFixedHeight)
                    {
                        bool toggleHoldPosition = GUILayout.Toggle(this.module.enableHoldPosition, " Enable Position Holding");
                        if (toggleHoldPosition != this.module.enableHoldPosition)
                        {
                            this.module.enableHoldPosition = toggleHoldPosition;
                            if (!this.module.useTargetHeight)
                            {
                                this.module.OnResetHoverError();
                            }
                            this.module.OnResetAccelError();
                            this.module.OnResetStrafeError();
                            this.module.OnGUIUpdateHover();
                        }
                    }
                    else
                    {
                        GUILayout.Space(16);
                    }

                    bool toggleEnabled = GUILayout.Toggle(this.module.m_HoverParameters.enabled, " Enable PID");
                    if (toggleEnabled != this.module.m_HoverParameters.enabled)
                    {
                        this.hoverEnabled = toggleEnabled;
                        this.module.m_HoverParameters.enabled = toggleEnabled;
                        this.module.OnGUIUpdateHover();
                    }
                }
                else
                {
                    bool toggleEnabled = GUILayout.Toggle(this.module.m_HoverParameters.enabled, " Enable PID");
                    if (toggleEnabled != this.module.m_HoverParameters.enabled)
                    {
                        this.hoverEnabled = toggleEnabled;
                        this.module.m_HoverParameters.enabled = toggleEnabled;
                        this.module.OnGUIUpdateHover();
                    }
                }
                GUILayout.EndVertical();
                currPanelsCount += 1;
            }
            if (this.module.m_AccelParameters != null)
            {
                GUILayout.BeginVertical(GUILayout.Width(260f), GUILayout.MaxWidth(260f), GUILayout.MinHeight(50f), GUILayout.MaxHeight(425f));
                GUILayout.Label("<size=20>Accel PID</size>");

                if (this.accelEnabled)
                {
                    if (GUIOverseer.TextSliderPair("Proportional Gain: ", ref this.accelKP, ref this.module.m_AccelParameters.kP, 0f, 100f, false, 0.05f))
                    {
                        module.OnGUIUpdateAccel();
                    }

                    if (GUIOverseer.TextSliderPair("Integral Gain: ", ref this.accelKI, ref this.module.m_AccelParameters.kI, 0f, 10f, false, 0.05f))
                    {
                        module.OnGUIUpdateAccel();
                    }

                    if (GUIOverseer.TextSliderPair("Derivative Gain: ", ref this.accelKD, ref this.module.m_AccelParameters.kD, 0f, 100f, false, 0.05f))
                    {
                        module.OnGUIUpdateAccel();
                    }

                    bool toggleDebug = GUILayout.Toggle(this.module.m_AccelParameters.debug, " Enable debug mode");
                    if (toggleDebug != this.module.m_AccelParameters.debug)
                    {
                        this.module.m_AccelParameters.debug = toggleDebug;
                        this.module.OnGUIUpdateAccel();
                    }

                    if (GUILayout.Button("Reset PID Error"))
                    {
                        this.module.OnResetAccelError();
                    }

                    bool toggleHoldPosition = GUILayout.Toggle(this.module.enableHoldPosition, " Enable Position Holding");
                    if (toggleHoldPosition != this.module.enableHoldPosition)
                    {
                        this.module.enableHoldPosition = toggleHoldPosition;
                        if (!this.module.useTargetHeight)
                        {
                            this.module.OnResetHoverError();
                        }
                        this.module.OnResetAccelError();
                        this.module.OnResetStrafeError();
                        this.module.OnGUIUpdateAccel();
                    }

                    bool toggleEnabled = GUILayout.Toggle(this.module.m_AccelParameters.enabled, " Enable PID");
                    if (toggleEnabled != this.module.m_AccelParameters.enabled)
                    {
                        this.accelEnabled = toggleEnabled;
                        this.module.m_AccelParameters.enabled = toggleEnabled;
                        this.module.OnGUIUpdateAccel();
                    }
                }
                else
                {
                    bool toggleEnabled = GUILayout.Toggle(this.module.m_AccelParameters.enabled, " Enable PID");
                    if (toggleEnabled != this.module.m_AccelParameters.enabled)
                    {
                        this.accelEnabled = toggleEnabled;
                        this.module.m_AccelParameters.enabled = toggleEnabled;
                        this.module.OnGUIUpdateAccel();
                    }
                }
                GUILayout.EndVertical();
                currPanelsCount += 1;
            }
            if (this.module.m_StrafeParameters != null)
            {
                GUILayout.BeginVertical(GUILayout.Width(260f), GUILayout.MaxWidth(260f), GUILayout.MinHeight(50f), GUILayout.MaxHeight(425f));
                GUILayout.Label("<size=20>Strafe PID</size>");

                if (this.strafeEnabled)
                {
                    if (GUIOverseer.TextSliderPair("Proportional Gain: ", ref this.strafeKP, ref this.module.m_StrafeParameters.kP, 0f, 100f, false, 0.05f))
                    {
                        module.OnGUIUpdateStrafe();
                    }

                    if (GUIOverseer.TextSliderPair("Integral Gain: ", ref this.strafeKI, ref this.module.m_StrafeParameters.kI, 0f, 10f, false, 0.05f))
                    {
                        module.OnGUIUpdateStrafe();
                    }

                    if (GUIOverseer.TextSliderPair("Derivative Gain: ", ref this.strafeKD, ref this.module.m_StrafeParameters.kD, 0f, 100f, false, 0.05f))
                    {
                        module.OnGUIUpdateStrafe();
                    }

                    bool toggleDebug = GUILayout.Toggle(this.module.m_StrafeParameters.debug, " Enable debug mode");
                    if (toggleDebug != this.module.m_StrafeParameters.debug)
                    {
                        this.module.m_StrafeParameters.debug = toggleDebug;
                        this.module.OnGUIUpdateStrafe();
                    }

                    if (GUILayout.Button("Reset PID Error"))
                    {
                        this.module.OnResetStrafeError();
                    }

                    bool toggleHoldPosition = GUILayout.Toggle(this.module.enableHoldPosition, " Enable Position Holding");
                    if (toggleHoldPosition != this.module.enableHoldPosition)
                    {
                        this.module.enableHoldPosition = toggleHoldPosition;
                        if (!this.module.useTargetHeight)
                        {
                            this.module.OnResetHoverError();
                        }
                        this.module.OnResetAccelError();
                        this.module.OnResetStrafeError();
                        this.module.OnGUIUpdateStrafe();
                    }

                    bool toggleEnabled = GUILayout.Toggle(this.module.m_StrafeParameters.enabled, " Enable PID");
                    if (toggleEnabled != this.module.m_StrafeParameters.enabled)
                    {
                        this.strafeEnabled = toggleEnabled;
                        this.module.m_StrafeParameters.enabled = toggleEnabled;
                        this.module.OnGUIUpdateStrafe();
                    }
                }
                else
                {
                    bool toggleEnabled = GUILayout.Toggle(this.module.m_StrafeParameters.enabled, " Enable PID");
                    if (toggleEnabled != this.module.m_StrafeParameters.enabled)
                    {
                        this.strafeEnabled = toggleEnabled;
                        this.module.m_StrafeParameters.enabled = toggleEnabled;
                        this.module.OnGUIUpdateStrafe();
                    }
                }
                GUILayout.EndVertical();
                currPanelsCount += 1;
            }
            // check for new horizontal
            if (currPanelsCount == this.rowTiling)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUILayout.EndHorizontal();
            }
            if (this.module.m_PitchParameters != null)
            {
                GUILayout.BeginVertical(GUILayout.Width(260f), GUILayout.MaxWidth(260f), GUILayout.MinHeight(50f), GUILayout.MaxHeight(425f));
                GUILayout.Label("<size=20>Strafe PID</size>");
                // reset target pitch here, since it may change elsewhere
                this.targetPitchStr = module.targetPitch.ToString();
                if (this.pitchEnabled)
                {
                    if (GUIOverseer.TextSliderPair("Proportional Gain: ", ref this.pitchKP, ref this.module.m_PitchParameters.kP, 0f, 1f, false, 0.05f))
                    {
                        module.OnGUIUpdatePitch();
                    }

                    if (GUIOverseer.TextSliderPair("Integral Gain: ", ref this.pitchKI, ref this.module.m_PitchParameters.kI, 0f, 1f, false, 0.05f))
                    {
                        module.OnGUIUpdatePitch();
                    }

                    if (GUIOverseer.TextSliderPair("Derivative Gain: ", ref this.pitchKD, ref this.module.m_PitchParameters.kD, 0f, 1f, false, 0.05f))
                    {
                        module.OnGUIUpdatePitch();
                    }

                    if (GUIOverseer.TextSliderPair("Manual Target Change Rate: ", ref this.targetChangeRateStr, ref this.module.manualTargetChangeRate, 10f, 100f, true, 0.05f))
                    {
                        module.OnGUIUpdatePitch();
                    }

                    bool toggleDebug = GUILayout.Toggle(this.module.m_PitchParameters.debug, " Enable debug mode");
                    if (toggleDebug != this.module.m_PitchParameters.debug)
                    {
                        this.module.m_PitchParameters.debug = toggleDebug;
                        this.module.OnGUIUpdatePitch();
                    }

                    if (GUILayout.Button("Reset PID Error"))
                    {
                        this.module.OnResetPitchError();
                    }

                    bool toggleEnabled = GUILayout.Toggle(this.module.m_PitchParameters.enabled, " Enable PID");
                    if (toggleEnabled != this.module.m_PitchParameters.enabled)
                    {
                        this.pitchEnabled = toggleEnabled;
                        this.module.m_PitchParameters.enabled = toggleEnabled;
                        this.module.OnGUIUpdatePitch();
                    }
                }
                else
                {
                    bool toggleEnabled = GUILayout.Toggle(this.module.m_PitchParameters.enabled, " Enable PID");
                    if (toggleEnabled != this.module.m_PitchParameters.enabled)
                    {
                        this.pitchEnabled = toggleEnabled;
                        this.module.m_PitchParameters.enabled = toggleEnabled;
                        this.module.OnGUIUpdatePitch();
                    }
                }
                GUILayout.EndVertical();
                currPanelsCount += 1;
            }
            if (currPanelsCount == this.rowTiling)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUILayout.EndHorizontal();
            }
            if (this.module.m_RollParameters != null)
            {
                GUILayout.BeginVertical(GUILayout.Width(260f), GUILayout.MaxWidth(260f), GUILayout.MinHeight(50f), GUILayout.MaxHeight(425f));
                GUILayout.Label("<size=20>Roll PID</size>");
                // reset target roll here, since it may change elsewhere
                this.targetRollStr = module.targetRoll.ToString();
                if (this.rollEnabled)
                {
                    if (GUIOverseer.TextSliderPair("Proportional Gain: ", ref this.rollKP, ref this.module.m_RollParameters.kP, 0f, 10f, false, 0.05f))
                    {
                        module.OnGUIUpdateRoll();
                    }

                    if (GUIOverseer.TextSliderPair("Integral Gain: ", ref this.rollKI, ref this.module.m_RollParameters.kI, 0f, 10f, false, 0.05f))
                    {
                        module.OnGUIUpdateRoll();
                    }

                    if (GUIOverseer.TextSliderPair("Derivative Gain: ", ref this.rollKD, ref this.module.m_RollParameters.kD, 0f, 10f, false, 0.05f))
                    {
                        module.OnGUIUpdateRoll();
                    }

                    if (GUIOverseer.TextSliderPair("Manual Target Change Rate: ", ref this.targetChangeRateStr, ref this.module.manualTargetChangeRate, 10f, 100f, true, 0.05f))
                    {
                        module.OnGUIUpdateRoll();
                    }

                    bool toggleDebug = GUILayout.Toggle(this.module.m_RollParameters.debug, " Enable debug mode");
                    if (toggleDebug != this.module.m_RollParameters.debug)
                    {
                        this.module.m_RollParameters.debug = toggleDebug;
                        this.module.OnGUIUpdateRoll();
                    }

                    if (GUILayout.Button("Reset PID Error"))
                    {
                        this.module.OnResetRollError();
                    }

                    bool toggleEnabled = GUILayout.Toggle(this.module.m_RollParameters.enabled, " Enable PID");
                    if (toggleEnabled != this.module.m_RollParameters.enabled)
                    {
                        this.rollEnabled = toggleEnabled;
                        this.module.m_RollParameters.enabled = toggleEnabled;
                        this.module.OnGUIUpdateRoll();
                    }
                }
                else
                {
                    bool toggleEnabled = GUILayout.Toggle(this.module.m_RollParameters.enabled, " Enable PID");
                    if (toggleEnabled != this.module.m_RollParameters.enabled)
                    {
                        this.rollEnabled = toggleEnabled;
                        this.module.m_RollParameters.enabled = toggleEnabled;
                        this.module.OnGUIUpdateRoll();
                    }
                }
                GUILayout.EndVertical();
                currPanelsCount += 1;
            }
            if (currPanelsCount == this.rowTiling)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUILayout.EndHorizontal();
            }
            if (this.module.m_YawParameters != null)
            {
                GUILayout.BeginVertical(GUILayout.Width(260f), GUILayout.MaxWidth(260f), GUILayout.MinHeight(50f), GUILayout.MaxHeight(425f));
                GUILayout.Label("<size=20>Yaw PID</size>");

                if (this.yawEnabled)
                {
                    if (GUIOverseer.TextSliderPair("Proportional Gain: ", ref this.yawKP, ref this.module.m_YawParameters.kP, 0f, 10f, false, 0.05f))
                    {
                        module.OnGUIUpdateYaw();
                    }

                    if (GUIOverseer.TextSliderPair("Integral Gain: ", ref this.yawKI, ref this.module.m_YawParameters.kI, 0f, 10f, false, 0.05f))
                    {
                        module.OnGUIUpdateYaw();
                    }

                    if (GUIOverseer.TextSliderPair("Derivative Gain: ", ref this.yawKD, ref this.module.m_YawParameters.kD, 0f, 10f, false, 0.05f))
                    {
                        module.OnGUIUpdateYaw();
                    }

                    bool toggleDebug = GUILayout.Toggle(this.module.m_YawParameters.debug, " Enable debug mode");
                    if (toggleDebug != this.module.m_YawParameters.debug)
                    {
                        this.module.m_YawParameters.debug = toggleDebug;
                        this.module.OnGUIUpdateYaw();
                    }

                    if (GUILayout.Button("Reset PID Error"))
                    {
                        this.module.OnResetYawError();
                    }

                    bool toggleEnabled = GUILayout.Toggle(this.module.m_YawParameters.enabled, " Enable PID");
                    if (toggleEnabled != this.module.m_YawParameters.enabled)
                    {
                        this.yawEnabled = toggleEnabled;
                        this.module.m_YawParameters.enabled = toggleEnabled;
                        this.module.OnGUIUpdateYaw();
                    }
                }
                else
                {
                    bool toggleEnabled = GUILayout.Toggle(this.module.m_YawParameters.enabled, " Enable PID");
                    if (toggleEnabled != this.module.m_YawParameters.enabled)
                    {
                        this.yawEnabled = toggleEnabled;
                        this.module.m_YawParameters.enabled = toggleEnabled;
                        this.module.OnGUIUpdateYaw();
                    }
                }
                GUILayout.EndVertical();
                currPanelsCount += 1;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(16);
            if (GUILayout.Button("Close"))
            {
                this.visible = false;
                this.module = null;
            }
            GUI.DragWindow();
        }
    }
}
