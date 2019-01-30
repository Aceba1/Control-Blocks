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
        public float EvaluatedBlockCurve = 0f;
        public int StretchModifier = 1;
        public float MaxStr = 1;
        public bool CanModifyStretch = false;
        public float StretchSpeed = 0.1f;

        internal bool OVERRIDE = false;
        public float alphaOpen { get; private set; } = 0f;
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
            if (alphaOpen != 0f)
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
            if (alphaOpen == 1f)
            {
                ForceMove = true;
                ResetRenderState(true);
            }
            else
            {
                ResetRenderState(false);
            }
            base.BlockRemoved(block, tank);
        }

        bool VInput { get => !LocalControl || (LocalControl && (tankcache == Singleton.playerTank)); }
        bool ButtonIsValid = true;
        void FixedUpdate()
        {
            if (ForceMove)
            {
                open = 1f;
                alphaOpen = 1f;
                Move(true);
                //SetRenderState();
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
            if ((Dirty || CanMove) && open == alphaOpen)
            {
                if (IsToggle)
                {
                    switch (InverseTrigger)
                    {
                        case 0:
                            if (VInput && Input.GetKeyDown(trigger))
                                alphaOpen = 1f - alphaOpen;
                            break;
                        case 1:
                            if (VInput && Input.GetKeyUp(trigger))
                                alphaOpen = 1f - alphaOpen;
                            break;
                        case 2:
                            if ((alphaOpen == 0f && VInput && Input.GetKeyDown(trigger)) ||
                                (alphaOpen == 1f && VInput && Input.GetKeyUp(trigger)))
                                if (ButtonIsValid)
                                {
                                    ButtonIsValid = false;
                                    alphaOpen = 1f - alphaOpen;
                                }
                            break;
                        case 3:
                            if ((alphaOpen == 1f && VInput && Input.GetKeyDown(trigger)) ||
                                (alphaOpen == 0f && VInput && Input.GetKeyUp(trigger)))
                                if (ButtonIsValid)
                                {
                                    ButtonIsValid = false;
                                    alphaOpen = 1f - alphaOpen;
                                }
                            break;
                    }
                }
                else
                {
                    if ((VInput && Input.GetKey(trigger)) != (InverseTrigger == 1))
                    {
                        alphaOpen = 1f;
                    }
                    else
                    {
                        alphaOpen = 0f;
                    }
                }
                if (ForceOpen)
                {
                    alphaOpen = 1f;
                    ForceOpen = false;
                }
                if (open != alphaOpen)
                {
                    Move(alphaOpen != 0f);
                }
            }
            if (OVERRIDE)
            {
                OVERRIDE = false;
                SnapRender = true;
                if (alphaOpen != 0f)
                {
                    Move(true);
                }
                SetRenderState();
            }
            if (open != alphaOpen)
            {
                float oldOpen = blockcurve.Evaluate(open * (StretchModifier / MaxStr));
                open = Mathf.Clamp01((open - (StretchSpeed * 0.5f * (MaxStr / StretchModifier))) + alphaOpen * (StretchSpeed * (MaxStr / StretchModifier)));
                if (Mathf.Abs(open - alphaOpen) < 0.01f) open = alphaOpen;
                EvaluatedBlockCurve = blockcurve.Evaluate(open * (StretchModifier / MaxStr));
                if (Class1.PistonHeart == Heart)
                {
                    //if (UpdateCOM)
                    //{
                    //    UpdateCOM = false;
                    //    CacheCOM = tankcache.rbody.worldCenterOfMass - LoadCOM.position * (MassPushing / tankcache.rbody.mass);
                    //    CacheCOM = tankcache.rbody.transform.InverseTransformVector(CacheCOM);
                    //    tankcache.rbody.mass -= MassPushing;
                    //}
                    var offs = SetRenderState();
                    if (block.tank != null && !block.tank.IsAnchored && block.tank.rbody.mass > 0f)
                    {
                        float th = (MassPushing / block.tank.rbody.mass);
                        var thing = (EvaluatedBlockCurve - oldOpen) * th;
                        tankcache.transform.position -= block.transform.rotation * Vector3.up * StretchModifier * thing;
#warning Fix COM
                        //if (open == alphaOpen)
                        //{
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
            if (UpdateFix)
            {
                UpdateFix = false;
                Move(true);
                SetRenderState();
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
        private void Move(bool Expand)
        {
            CleanDirty();
            if (CanMove)
            {
                if (parts.Length != 0)
                {
                    for (int I = 0; I < parts.Length; I++)
                        parts[I].localPosition = Vector3.up * curves[I].Evaluate(Expand ? (StretchModifier / MaxStr) : 0f);
                }
                Vector3 modifier = block.transform.localRotation * ((Expand ? Vector3.up : Vector3.down) * StretchModifier);
                foreach (var pair in GrabbedBlocks)
                {
                    var val = pair.Key;
                    val.transform.localPosition += modifier;
                }
            }
            else
            {
                alphaOpen = open;
            }
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
        
        public Vector3 SetRenderState()
        {
            if (SnapRender)
            {
                open = alphaOpen;
                SnapRender = false;
                EvaluatedBlockCurve = blockcurve.Evaluate(open * (StretchModifier / MaxStr));
            }
            for (int I = 0; I < parts.Length; I++)
            {
                parts[I].localPosition = Vector3.up * curves[I].Evaluate(open * (StretchModifier / MaxStr));
            }
            var rawOfs = EvaluatedBlockCurve - alphaOpen * StretchModifier;
            Vector3 offs = (block.transform.localRotation * Vector3.up) * (rawOfs - gOfs);
            gOfs = rawOfs;
            foreach (var pair in GrabbedBlocks)
            {
                var block = pair.Key;
                if (block.tank == base.block.tank)
                    block.transform.localPosition += offs;
            }
            return offs;
        }

        private void OnSpawn()
        {
            SetRenderState();
        }

        internal override void Attach()
        {
            alphaOpen = 0;
            open = 0;
            gOfs = 0;
            
            tankcache?.AttachEvent.Unsubscribe(a_action);
            tankcache?.DetachEvent.Unsubscribe(d_action);
            tankcache = block.tank;
            tankcache.AttachEvent.Subscribe(a_action);
            tankcache.DetachEvent.Subscribe(d_action);
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
            alphaOpen = 0f;
            open = 0f;

        }

        internal override void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
        {
            if (saving)
            {
                if (alphaOpen == 1f)
                {
                    ForceMove = true;
                    ResetRenderState(true);
                    open = 1f;
                }

                ModulePiston.SerialData serialData = new ModulePiston.SerialData()
                {
                    IsOpen = this.alphaOpen != 0f,
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