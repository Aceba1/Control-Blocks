using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace Control_Block
{
    class ModuleSwivel : ModuleBlockMover
    {
        public Vector3 localEffectorPos = Vector3.zero;
        public float EvaluatedBlockRotCurve = 0f;
        public bool LockAxis = false;
        public float MinAxis = -45f, MaxAxis = 45f;
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
            if (VInput && (Dirty || CanMove))
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
            CurrentAngle = Mathf.Repeat(CurrentAngle, 360);
            if (ForceMove || Dirty || oldAngle != CurrentAngle)
            {
                ForceMove = false;
                EvaluatedBlockRotCurve = blockrotcurve.Evaluate(CurrentAngle);
                if (Class1.PistonHeart == Heart)
                {
                    Move();

                    //if (block.tank != null && !block.tank.IsAnchored && block.tank.rbody.mass > 0f)
                    //{
                    //    float th = (MassPushing / block.tank.rbody.mass);
                    //    var thing = (EvaluatedBlockRotCurve - oldOpen) * th;
                    //    tankcache.transform.position -= block.transform.rotation * Vector3.up * thing;
                    //    tankcache.rbody.centerOfMass -= th * offs;
                    //    tankcache.dragSphere.transform.position = tankcache.rbody.worldCenterOfMass;

                    //}
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

        Vector3 RotateAroundPoint(Vector3 param, Vector3 pivot, Quaternion rot) => (rot * (param - pivot)) + pivot;

        private void Move()
        {
            CleanDirty();
            if (CanMove)
            {
                if (parts.Length != 0)
                {
                    for (int I = 0; I < parts.Length; I++)
                        parts[I].localRotation = Quaternion.Euler(0f, rotCurves[I].Evaluate(CurrentAngle), 0f);
                }
                var ofs = CurrentAngle;
                var thig = (block.transform.localRotation * Vector3.up) * (ofs - gOfs);
                var modifier = Quaternion.Euler(thig);
                gOfs = ofs;
                int iterate = GrabbedBlocks.Count;
                foreach (var pair in GrabbedBlocks)
                {
                    iterate--;
                    var val = pair.Key;
                    val.transform.localPosition = RotateAroundPoint(val.transform.localPosition, block.transform.localPosition + (block.transform.localRotation * localEffectorPos), modifier);
                    val.transform.localRotation = modifier * val.transform.localRotation;
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
                    Speed = RotateSpeed
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
        }
    }
}