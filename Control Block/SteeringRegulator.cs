using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using UnityEngine;
using System.Reflection;

namespace Control_Block
{
    class ModuleSteeringRegulator : Module
    {
        public void OnSpawn()
        {
            //HoverMod = 4f; JetMod = 6f; TurbineMod = 2f;
            DriveMod = 1f;
            ThrottleMod = 1f;
            MaxDist = 2f;
            VelocityDampen = 0.4f;
        }

        private void OnPool()
        {
            base.block.serializeEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
            base.block.serializeTextEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
            base.block.AttachEvent.Subscribe(OnAttach);
            base.block.DetachEvent.Subscribe(OnDetach);
        }

        private void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
        {
            if (saving)
            {
                ModuleSteeringRegulator.SerialData serialData = new ModuleSteeringRegulator.SerialData()
                {
                    dist = MaxDist,
                    drive = DriveMod,
                    throttle = ThrottleMod,
                    dampen = VelocityDampen
                };
                serialData.Store(blockSpec.saveState);
            }
            else
            {
                ModuleSteeringRegulator.SerialData serialData2 = Module.SerialData<ModuleSteeringRegulator.SerialData>.Retrieve(blockSpec.saveState);
                if (serialData2 != null)
                {
                    DriveMod = serialData2.drive;
                    ThrottleMod = serialData2.throttle;
                    MaxDist = serialData2.dist;
                    VelocityDampen = serialData2.dampen;
                }
            }
        }

        [Serializable]
        private new class SerialData : Module.SerialData<ModuleSteeringRegulator.SerialData>
        {
            public float dist, drive, throttle, dampen;
            public float? hover, jet, turbine;
        }

        public Vector3 StabilizerTarget { get; private set; } = Vector3.zero;
        public Rigidbody rbody => block.tank.rbody;
        public float MaxDist = 2f;
        public float DriveMod = 1f, ThrottleMod = 1f, VelocityDampen = 0.4f;
        //public float HoverMod = 4f, JetMod = 10f, TurbineMod = 4f;
        public Vector3 lhs { get; private set; } = Vector3.zero;

        MeshRenderer _mr;
        MeshRenderer mr
        {
            get
            {
                if (_mr == null)
                {
                    _mr = block.GetComponentInChildren<MeshRenderer>();
                }
                return _mr;
            }
        }
        public void SetColor(Color color)
        {
            mr.material.SetColor("_EmissionColor", color);
        }

        private void LHS()
        {
            if (!CanWork || MaxDist <= 0f)
            {
                lhs = Vector3.zero;
                SetColor(Color.red);
                return;
            }
            lhs = (StabilizerTarget - rbody.worldCenterOfMass) / MaxDist - rbody.velocity * VelocityDampen;
        }



        private void OnDetach()
        {
            //block.tank.control.driveControlEvent.Unsubscribe(Control_driveControlEvent);
            block.tank.control.manualAimFireEvent.Unsubscribe(Control_manualAimFireEvent);
            SetColor(Color.white);
        }
        private void OnAttach()
        {
            //block.tank.control.driveControlEvent.Subscribe(Control_driveControlEvent);
            block.tank.control.manualAimFireEvent.Subscribe(Control_manualAimFireEvent);
            StabilizerTarget = rbody.worldCenterOfMass;
        }

        public bool CanWork = true;

        private void Control_manualAimFireEvent(int _, bool Fire)
        {
            TankControl.State state = block.tank.control.CurState;
            if (CanWork = (state.m_ThrottleValues + state.m_InputMovement).Approximately(Vector3.zero, 0.01f)) // Set CanWork, while also checking CanWork
            {
                state.m_InputMovement = lhs * DriveMod;
                state.m_ThrottleValues = lhs * ThrottleMod;
                block.tank.control.CurState = state;
            }
            else
                StabilizerTarget = rbody.worldCenterOfMass;
        }

        public void FixedUpdate()
        {
            if (block.tank == null || !block.IsAttached)
            {
                return;
            }
            if (MaxDist <= 0f)
            {
                lhs = Vector3.zero;

                SetColor(Color.red);
                return;
            }
            Vector3 com = rbody.worldCenterOfMass;
            var dist = Quaternion.Inverse(rbody.rotation) * (StabilizerTarget - com);
            dist.x = Mathf.Clamp(dist.x, -MaxDist, MaxDist);
            dist.y = Mathf.Clamp(dist.y, -MaxDist, MaxDist);
            dist.z = Mathf.Clamp(dist.z, -MaxDist, MaxDist);
            StabilizerTarget = com + (rbody.rotation * dist);
            var col = Mathf.Clamp01(dist.sqrMagnitude / (MaxDist * MaxDist));

            SetColor(new Color(col, col, col));
            LHS();
        }
    }
}
