using System;
using UnityEngine;

namespace Control_Block
{
    [RequireComponent(typeof(TargetAimer))]
    internal class ModuleSwivel : ModuleBlockMover
    {
        public TargetAimer aimer;
        public GimbalAimer gimbal;
        
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
            Turning,
            AimAtPlayer,
            AimAtVelocity
        }

        public float RotateSpeed = 1f;
        public float MaxSpeed = 15f;
        public bool CanModifySpeed = false;
        public float CurrentAngle = 0f;
        private float gOfs = 0f;

        public bool LocalControl = true;
        public KeyCode trigger1 = KeyCode.RightArrow, trigger2 = KeyCode.LeftArrow;

        private bool ForceMove = false;

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

        private void Update()
        {
            if (ForceMove)
            {
                Move();
            }
        }

        private bool VInput { get => !LocalControl || (LocalControl && (tankcache == Singleton.playerTank)); }
        private bool ButtonNotPressed = true, Moved = false;
        private void FixedUpdate()
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
                if (!tankcache.beam.IsActive)
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

                        case Mode.AimAtPlayer:
                            if (Singleton.playerTank != null)
                                gimbal.Aim(Singleton.playerTank.rbody.worldCenterOfMass, (RotateSpeed / Time.deltaTime));
                            else
                                gimbal.AimDefault((RotateSpeed / Time.deltaTime));
                            CurrentAngle = parts[parts.Length - 1].localRotation.eulerAngles.y;
                            break;
                        case Mode.AimAtVelocity:
                            gimbal.Aim(block.transform.position - tankcache.rbody.velocity + block.transform.forward * 0.15f, (RotateSpeed / Time.deltaTime));
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
                            if (VInput && ButtonNotPressed)
                            {
                                if (Input.GetKey(trigger1))
                                {
                                    Direction += 1f;
                                }
                                else if (Input.GetKey(trigger2))
                                {
                                    Direction -= 1f;
                                }
                                Direction = Mathf.Clamp(Direction, -1f, 1f);
                            }
                            CurrentAngle += Direction * RotateSpeed;
                            break;

                        case Mode.Turning:
                            if (VInput)
                            {
                                Direction = +1;
                                if (Input.GetKey(trigger1))
                                {
                                    Direction = +1;
                                }
                                else if (Input.GetKey(trigger2))
                                {
                                    Direction = -1;
                                }
                                else
                                {
                                    Direction = -(Mathf.Repeat(CurrentAngle - AngleCenter + 180, 360) - 180) / RotateSpeed;
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
                    Moved = true;
                    Move();
                    if (CanMove)
                    {
                        if ((oldOpen != EvaluatedBlockRotCurve) && block.tank != null && !block.tank.IsAnchored && block.tank.rbody.mass > 0f && MassPushing > block.CurrentMass)
                        {
                            float th = (MassPushing / block.tank.rbody.mass);
                            var thing = (Mathf.Repeat(EvaluatedBlockRotCurve - oldOpen + 180, 360) - 180) * th;
                            tankcache.transform.RotateAround(parts[parts.Length - 1].position, block.transform.rotation * Vector3.up, -thing);
#warning Fix COM
                            //tankcache.RequestPhysicsReset();
                        }
                    }
                    else
                    {
                        CurrentAngle = oldAngle;
                    }
                }
                else
                {
                    Heart = Class1.PistonHeart;
                }
            }
            else if (Moved && Class1.PistonHeart == Heart)
            {
                Moved = false;
                tankcache.RequestPhysicsReset();
            }
            if (mode == Mode.OnOff)
                ButtonNotPressed = !Input.GetKey(trigger1) && !Input.GetKey(trigger2);
        }

        private MeshRenderer[] _mr;

        private MeshRenderer[] mr
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
            foreach (var t in mr)
            {
                t.material.color = color;
            }
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
                    {
                        parts[I].localRotation = Quaternion.Euler(0f, rotCurves[I].Evaluate(CurrentAngle), 0f);
                    }
                }
                var ofs = CurrentAngle;
                var axis = (block.transform.rotation * Vector3.up);
                var angle = (Mathf.Repeat(ofs - gOfs + 180, 360) - 180);
                gOfs = ofs;
                foreach (var pair in GrabbedBlocks)
                {
                    var val = pair.Key;
                    val.transform.RotateAround(parts[parts.Length - 1].position, axis, angle);
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
            EvaluatedBlockRotCurve = 0f;
            LockAngle = false;
            AngleCenter = 0f; AngleRange = 45f;
            Direction = 0f;
            CurrentAngle = 0f;
            mode = Mode.Positional;
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
                SerialData sd = SerialData<ModuleSwivel.SerialData>.Retrieve(blockSpec.saveState);
                if (sd != null)
                {
                    CurrentAngle = sd.Angle;
                    if (CurrentAngle != 0f)
                    {
                        ForceMove = true;
                    }
                    trigger1 = sd.Input1;
                    trigger2 = sd.Input2;
                    LocalControl = sd.Local;
                    RotateSpeed = (int)Mathf.Clamp(sd.Speed, 0.5f, MaxSpeed);
                    Direction = sd.Direction;
                    LockAngle = sd.Restrict;
                    AngleCenter = sd.minRestrict;
                    AngleRange = sd.rangeRestrict;
                    mode = sd.mode;
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