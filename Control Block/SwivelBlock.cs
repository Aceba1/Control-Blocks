using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace Control_Block
{
    [RequireComponent(typeof(TargetAimer))]
    class ModuleSwivel : ModuleBlockMover
    {
        public TargetAimer aimer;
        public GimbalAimer gimbal;

        public Vector3 localEffectorPos = Vector3.zero;
        public float EvaluatedBlockRotCurve = 0f;
        public bool LockAngle = false;
        public float AngleCenter = 0f, AngleRange = 45f;
        public float Direction = 0f;
        public Mode mode = Mode.Positional;
        public enum Mode : byte
        {
            Positional,
            Directional,
            Speed,
            OnOff,
            Aim,
            Cycle
        }
        public float RotateSpeed = 1f;
        public float MaxSpeed = 15f;
        public bool CanModifySpeed = false;
        public float CurrentAngle = 0f;
        private float gOfs = 0f;

        public bool LocalControl = true;
        public KeyCode trigger1 = KeyCode.RightArrow, trigger2 = KeyCode.LeftArrow;

        bool ForceMove = false;

        protected Tank tankcache;

        internal override void BeforeBlockAdded(TankBlock block)
        {
            ResetRenderState();
        }

        internal override void BlockRemoved(TankBlock block, Tank tank)
        {
            ResetRenderState();
            base.BlockRemoved(block, tank);
        }

        void Update()
        {
            if (ForceMove)
            {
                Move();
            }
        }

        bool VInput { get => !LocalControl || (LocalControl && (tankcache == Singleton.playerTank)); }
        void FixedUpdate()
        {
            var oldAngle = CurrentAngle;
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
            if (Dirty || CanMove)
            {
                switch (mode)
                {
                    case Mode.Positional:
                        if (VInput)
                        {
                            if (Input.GetKey(trigger1))
                            {
                                CurrentAngle += RotateSpeed;
                            }
                            if (Input.GetKey(trigger2))
                            {
                                CurrentAngle -= RotateSpeed;
                            }
                        }
                        break;
                    case Mode.Aim:
                        aimer.UpdateAndAimAtTarget(RotateSpeed / Time.deltaTime);
                        CurrentAngle = parts[parts.Length - 1].localRotation.eulerAngles.y;
                        break;
                    case Mode.Directional:
                        if (VInput)
                        {
                            if (Input.GetKey(trigger1))
                            {
                                Direction = 1f;
                            }
                            if (Input.GetKey(trigger2))
                            {
                                Direction = -1f;
                            }
                        }
                        CurrentAngle += Direction * RotateSpeed;
                        break;
                    case Mode.Speed:
                        if (VInput)
                        {
                            if (Input.GetKey(trigger1))
                            {
                                Direction += 0.025f;
                            }
                            else if (Input.GetKey(trigger2))
                            {
                                Direction -= 0.025f;
                            }
                            Direction = Mathf.Clamp(Direction, -1f, 1f);
                        }
                        CurrentAngle += Direction * RotateSpeed;
                        break;
                    case Mode.OnOff:
                        if (VInput)
                        {
                            if (Input.GetKeyDown(trigger1))
                            {
                                Direction += 1f;
                            }
                            else if (Input.GetKeyDown(trigger2))
                            {
                                Direction -= 1f;
                            }
                            Direction = Mathf.Clamp(Direction, -1f, 1f);
                        }
                        CurrentAngle += Direction * RotateSpeed;
                        break;
                }
            }
            if (LockAngle)
            {
                float Diff = (CurrentAngle - AngleCenter + 900) % 360 - 180;
                if (Diff < -AngleRange)
                {
                    CurrentAngle += (AngleCenter - AngleRange) - CurrentAngle;
                }
                else if (Diff > AngleRange)
                {
                    CurrentAngle += (AngleCenter + AngleRange) - CurrentAngle;
                }
            }
            CurrentAngle = Mathf.Repeat(CurrentAngle, 360);
            if ((ForceMove || Dirty || CanMove) && oldAngle != CurrentAngle)
            {
                ForceMove = false;
                float oldOpen = EvaluatedBlockRotCurve;
                EvaluatedBlockRotCurve = blockrotcurve.Evaluate(CurrentAngle);
                if (Class1.PistonHeart == Heart)
                {
                    Move();
                    
                    if ((oldOpen != EvaluatedBlockRotCurve) && block.tank != null && !block.tank.IsAnchored && block.tank.rbody.mass > 0f && MassPushing > block.CurrentMass*4f)
                    {
                        float th = (MassPushing / block.tank.rbody.mass);
                        var thing = (Mathf.Repeat(EvaluatedBlockRotCurve - oldOpen+180, 360)-180) * th;
                        tankcache.transform.RotateAround(block.transform.position + (block.transform.rotation * localEffectorPos), block.transform.rotation * Vector3.up, -thing);
                    }
                }
                else Heart = Class1.PistonHeart;
            }
        }

        MeshRenderer[] _mr;
        MeshRenderer[] mr
        {
            get
            {
                if (_mr == null)
                {
                    _mr = block.GetComponentsInChildren<MeshRenderer>();
                }
                return _mr;
            }
        }

        public void SetColor(Color color)
        {
            foreach (var t in mr) t.material.color = color;
        }

        private void Move()
        {
            CleanDirty();
            if (CanMove)
            {
                var lastp = holder.position;
                var lastr = holder.rotation;
                if (parts.Length != 0)
                {
                    for (int I = 0; I < parts.Length; I++)
                        parts[I].localRotation = Quaternion.Euler(0f, rotCurves[I].Evaluate(CurrentAngle), 0f);
                }
                var ofs = CurrentAngle;
                var axis = (block.transform.rotation * Vector3.up);
                var angle = (Mathf.Repeat(ofs - gOfs + 180, 360) - 180);
                gOfs = ofs;
                foreach (var pair in GrabbedBlocks)
                {
                    var val = pair.Key;
                    val.transform.RotateAround(block.transform.position + (block.transform.rotation * localEffectorPos), axis, angle);
                }
            }
        }

        /// <summary>
        /// ONLY USE IF ALL PISTONS ARE TO BE RESET
        /// </summary>
        public void ResetRenderState()
        {
            ForceMove = true;
            foreach (var part in parts)
            {
                part.localRotation = Quaternion.identity;
            }
            gOfs = 0;
            EvaluatedBlockRotCurve = 0f;
            if (block.tank != null)
            {
                holder.position = block.tank.transform.position;
                holder.rotation = block.tank.transform.rotation;
            }
            foreach (var pair in GrabbedBlocks)
            {
                var block = pair.Key;
                if (block.tank == base.block.tank)
                {
                    block.transform.localPosition = block.cachedLocalPosition;
                    block.transform.localRotation = block.cachedLocalRotation;
                }
            }
        }

        private void OnPool()
        {
            aimer = GetComponent<TargetAimer>();
            aimer.Init(block, .75f, null);
            gimbal = GetComponentInChildren<GimbalAimer>();
        }

        private void OnSpawn()
        {
            CurrentAngle = 0f;
            Move();
        }

        internal override void Attach()
        {
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
        }

        internal override void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
        {
            if (saving)
            {
                if (CurrentAngle != 0f)
                {
                    ResetRenderState();
                }

                SerialData serialData = new SerialData()
                {
                    Angle = CurrentAngle,
                    Input2 = trigger2,
                    Input1 = trigger1,
                    Local = LocalControl,
                    Speed = RotateSpeed,
                    Direction = Direction,
                    minRestrict = AngleCenter,
                    mode = mode,
                    rangeRestrict = AngleRange,
                    Restrict = LockAngle
                };
                serialData.Store(blockSpec.saveState);
            }
            else
            {
                SerialData serialData2 = SerialData<ModuleSwivel.SerialData>.Retrieve(blockSpec.saveState);
                if (serialData2 != null)
                {
                    CurrentAngle = serialData2.Angle;
                    if (CurrentAngle != 0f)
                    {
                        ForceMove = true;
                    }
                    trigger1 = serialData2.Input1;
                    trigger2 = serialData2.Input2;
                    LocalControl = serialData2.Local;
                    RotateSpeed = (int)Mathf.Clamp(serialData2.Speed, 0.5f, MaxSpeed);
                    Direction = serialData2.Direction;
                    LockAngle = serialData2.Restrict;
                    AngleCenter = serialData2.minRestrict;
                    AngleRange = serialData2.rangeRestrict;
                    Dirty = true;
                }
            }
        }

        [Serializable]
        private new class SerialData : Module.SerialData<ModuleSwivel.SerialData>
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
        }
    }
}