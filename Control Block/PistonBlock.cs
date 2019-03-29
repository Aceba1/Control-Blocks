using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace Control_Block
{
    class ModulePiston : ModuleBlockMover
    {
        public float LastCurveDiff = 0f;
        public float EvaluatedBlockCurve = 0f;
        public int StretchModifier = 1;
        public float MaxStr = 1;
        public bool CanModifyStretch = false;
        public float StretchSpeed = 0.1f;
        
        public float AlphaOpen { get; private set; } = 0f;
        private float gOfs = 0f;
        public bool IsToggle;
        public byte InverseTrigger;
        public bool LocalControl = true;
        public KeyCode trigger = KeyCode.Space;
        

        protected Tank tankcache;

        /// <summary>
        /// Ignore animating
        /// </summary>
        bool SnapRender = true;
        /// <summary>
        /// Invoke activate input
        /// </summary>
        bool ForceOpen = false;
        /// <summary>
        /// Force ghost-push as opened
        /// </summary>
        bool ForceMove = false;
        /// <summary>
        /// Repair before render
        /// </summary>
        bool UpdateFix = false;

        internal override void BeforeBlockAdded(TankBlock block)
        {
            if (AlphaOpen != 0f)
            {
                if (IsToggle)
                {
                    ForceOpen = true;
                }
                UpdateFix = true;
                ResetRenderState(true);
            }
            else
            {
                ResetRenderState(false);
            }
        }

        internal override void BlockRemoved(TankBlock block, Tank tank)
        {
            if (AlphaOpen != 0f)
            {
                ForceMove = true;
                UpdateFix = true;
                ResetRenderState(true);
            }
            else
            {
                ResetRenderState(false);
            }
            base.BlockRemoved(block, tank);
        }

        private void LateUpdate()
        {
            LastCurveDiff = 100f;
        }

        bool VInput { get => !LocalControl || (LocalControl && (tankcache == Singleton.playerTank)); }
        bool ButtonIsValid = true;
        void FixedUpdate()
        {
            if (ForceMove)
            {
                open = 1f;
                AlphaOpen = 1f;
                Move();
                ForceMove = false;
            }
            try
            {
                if (block.tank == null)
                {
                    SetDirty();
                    tankcache?.AttachEvent.Unsubscribe(a_action);
                    tankcache?.DetachEvent.Unsubscribe(d_action);
                }
                else if (block.tank != tankcache)
                {
                    tankcache?.AttachEvent.Unsubscribe(a_action);
                    tankcache?.DetachEvent.Unsubscribe(d_action);
                    tankcache = block.tank;
                    tankcache.AttachEvent.Subscribe(a_action);
                    tankcache.DetachEvent.Subscribe(d_action);
                }
            }
            catch (Exception E)
            {
                Print(E.Message);
                Print(E.StackTrace);
            }
            if (!block.IsAttached || block.tank == null)
            {
                return;
            }
            if ((Dirty || CanMove) && open == AlphaOpen)
            {
                if (IsToggle)
                {
                    switch (InverseTrigger)
                    {
                        case 0:
                            if (VInput && Input.GetKeyDown(trigger))
                                AlphaOpen = 1f - AlphaOpen;
                            break;
                        case 1:
                            if (VInput && Input.GetKeyUp(trigger))
                                AlphaOpen = 1f - AlphaOpen;
                            break;
                        case 2:
                            if ((AlphaOpen == 0f && VInput && Input.GetKeyDown(trigger)) ||
                                (AlphaOpen == 1f && VInput && Input.GetKeyUp(trigger)))
                                if (ButtonIsValid)
                                {
                                    ButtonIsValid = false;
                                    AlphaOpen = 1f - AlphaOpen;
                                }
                            break;
                        case 3:
                            if ((AlphaOpen == 1f && VInput && Input.GetKeyDown(trigger)) ||
                                (AlphaOpen == 0f && VInput && Input.GetKeyUp(trigger)))
                                if (ButtonIsValid)
                                {
                                    ButtonIsValid = false;
                                    AlphaOpen = 1f - AlphaOpen;
                                }
                            break;
                    }
                }
                else
                {
                    if ((VInput && Input.GetKey(trigger)) != (InverseTrigger == 1))
                    {
                        AlphaOpen = 1f;
                    }
                    else
                    {
                        AlphaOpen = 0f;
                    }
                }
                if (ForceOpen)
                {
                    AlphaOpen = 1f;
                    ForceOpen = false;
                }
                //if (open != AlphaOpen)
                //{
                //    SFXIsOn = true;
                //}
            }
            if (open != AlphaOpen)
            {
                float oldOpen = BlockCurve.Evaluate(open * (StretchModifier / MaxStr));
                open = Mathf.Clamp01((open - (StretchSpeed * 0.5f * (MaxStr / StretchModifier))) + AlphaOpen * (StretchSpeed * (MaxStr / StretchModifier)));
                if (Mathf.Abs(open - AlphaOpen) < 0.01f) open = AlphaOpen;
                EvaluatedBlockCurve = BlockCurve.Evaluate(open * (StretchModifier / MaxStr));
                if (Class1.PistonHeart == Heart)
                {
                    //if (UpdateCOM)
                    //{
                    //    UpdateCOM = false;
                    //    CacheCOM = tankcache.rbody.worldCenterOfMass - LoadCOM.position * (MassPushing / tankcache.rbody.mass);
                    //    CacheCOM = tankcache.rbody.transform.InverseTransformVector(CacheCOM);
                    //    tankcache.rbody.mass -= MassPushing;
                    //}
                    var offs = Move();
                    if (block.tank != null && !block.tank.IsAnchored && block.tank.rbody.mass > 0f)
                    {
                        float th = (MassPushing / block.tank.rbody.mass);
                        LastCurveDiff = EvaluatedBlockCurve - oldOpen;
                        var thing = LastCurveDiff * th;
                        tankcache.rbody.position -= block.transform.rotation * Vector3.up * thing;
#warning Fix COM
                        //if (open == AlphaOpen)
                        //{
                        //    SFXIsOn = false;
                        tankcache.RequestPhysicsReset();
                        //}
                        //tankcache.rbody.centerOfMass = CacheCOM + tankcache.rbody.transform.InverseTransformVector(LoadCOM.position) * th;
                        //tankcache.dragSphere.transform.position = tankcache.rbody.worldCenterOfMass;
                    }
                }
                else Heart = Class1.PistonHeart;
            }
            SnapRender = false;
        }

        void Update()
        {
            UpdateSFX(LastCurveDiff);
            if (UpdateFix)
            {
                UpdateFix = false;
                Move();
            }
            if (InverseTrigger == 2 || InverseTrigger == 3)
                ButtonIsValid = ButtonIsValid || Input.GetKeyUp(trigger);
        }

        internal float open = 0f;
        public float Open
        {
            get
            {
                return open;
            }
        }
        private Vector3 Move()
        {
            CleanDirty();
            if (CanMove)
            {
                if (SnapRender)
                {
                    open = AlphaOpen;
                    SnapRender = false;
                    EvaluatedBlockCurve = BlockCurve.Evaluate(open * (StretchModifier / MaxStr));
                }
                for (int I = 0; I < parts.Length; I++)
                {
                    parts[I].localPosition = Vector3.up * curves[I].Evaluate(open * (StretchModifier / MaxStr));
                }
                var rawOfs = EvaluatedBlockCurve;
                Vector3 offs = (block.transform.localRotation * Vector3.up) * (rawOfs - gOfs);
                gOfs = rawOfs;
                foreach (var pair in GrabbedBlocks)
                {
                    var block = pair.Key;
                    block.transform.localPosition += offs;
                }
                return offs;
            }
            else
            {
                AlphaOpen = open;
            }
            return default(Vector3);
        }

        /// <summary>
        /// ONLY USE IF ALL PISTONS ARE TO BE RESET
        /// </summary>
        /// <param name="ImmediatelySetAfter">Set SnapRender true</param>
        public void ResetRenderState(bool ImmediatelySetAfter = false)
        {
            //ApplyPistonForce(0f - alphaOpen);
            foreach (var part in parts)
            {
                part.localPosition = Vector3.zero;
            }
            open = 0f;
            //alphaOpen = 0f;
            SnapRender = ImmediatelySetAfter;
            tankcache.rbody.centerOfMass += MassPushing * (block.transform.localRotation * Vector3.up) * - gOfs;
            gOfs = 0;
            EvaluatedBlockCurve = 0f;
            if (block.tank != null)
            {
                holder.position = block.tank.transform.position;
                holder.rotation = block.tank.transform.rotation;
            }
            foreach (var pair in GrabbedBlocks)
            {
                var block = pair.Key;
                if (block.tank == base.block.tank)
                    block.transform.localPosition = block.cachedLocalPosition;
            }
        }
        private void OnSpawn()
        {
            Move();
        }

        internal override void Attach()
        {
            AlphaOpen = 0;
            open = 0;
            gOfs = 0;
            SnapRender = true;
            Move();
            tankcache?.AttachEvent.Unsubscribe(a_action);
            tankcache?.DetachEvent.Unsubscribe(d_action);
            tankcache = block.tank;
            tankcache.AttachEvent.Subscribe(a_action);
            tankcache.DetachEvent.Subscribe(d_action);
            base.Attach();
        }

        private void OnDisable()
        {
            GrabbedBlocks.Clear();
            Dirty = true;
            tankcache?.AttachEvent.Unsubscribe(a_action);
            tankcache?.DetachEvent.Unsubscribe(d_action);
            trigger = KeyCode.Space;
            IsToggle = false;
            InverseTrigger = 0;
            AlphaOpen = 0f;
            open = 0f;

        }

        internal override void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
        {
            if (saving)
            {
                if (AlphaOpen == 1f)
                {
                    ForceMove = true;
                    ResetRenderState(true);
                    open = 1f;
                }

                ModulePiston.SerialData serialData = new ModulePiston.SerialData()
                {
                    IsOpen = this.AlphaOpen != 0f,
                    Input = this.trigger,
                    Toggle = this.IsToggle,
                    Local = this.LocalControl,
                    Invert = this.InverseTrigger % 2 == 1,
                    PreferState = this.InverseTrigger >= 2,
                    Stretch = this.StretchModifier
                };
                serialData.Store(blockSpec.saveState);
            }
            else
            {
                ModulePiston.SerialData serialData2 = Module.SerialData<ModulePiston.SerialData>.Retrieve(blockSpec.saveState);
                if (serialData2 != null)
                {
                    if (serialData2.IsOpen)
                    {
                        ForceMove = true;
                        SnapRender = true;
                    }
                    InverseTrigger = (byte)((serialData2.Invert ? 1 : 0) + (serialData2.PreferState ? 2 : 0));
                    trigger = serialData2.Input;
                    IsToggle = serialData2.Toggle;
                    LocalControl = serialData2.Local;
                    StretchModifier = (int)Mathf.Clamp(serialData2.Stretch, 1, MaxStr);
                    Dirty = true;
                    UpdateFix = true;
                }
            }
        }

        [Serializable]
        private new class SerialData : Module.SerialData<ModulePiston.SerialData>
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