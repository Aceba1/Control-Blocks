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
            HoverMod = 4f; JetMod = 10f; TurbineMod = 4f;
        }

        private void OnPool()
        {
            base.block.serializeEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
            base.block.serializeTextEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
        }

        private void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
        {
            if (saving)
            {
                ModuleSteeringRegulator.SerialData serialData = new ModuleSteeringRegulator.SerialData()
                {
                    dist = MaxDist,
                    jet = JetMod,
                    hover = HoverMod,
                    turbine = TurbineMod
                };
                serialData.Store(blockSpec.saveState);
            }
            else
            {
                ModuleSteeringRegulator.SerialData serialData2 = Module.SerialData<ModuleSteeringRegulator.SerialData>.Retrieve(blockSpec.saveState);
                if (serialData2 != null)
                {
                    HoverMod = serialData2.hover;
                    JetMod = serialData2.jet;
                    TurbineMod = serialData2.turbine;
                    MaxDist = serialData2.dist;
                }
            }
        }

        [Serializable]
        private new class SerialData : Module.SerialData<ModuleSteeringRegulator.SerialData>
        {
            public float hover, jet, turbine, dist;
        }

        public Vector3 StabilizerTarget { get; private set; } = Vector3.zero;
        public Rigidbody rbody => block.tank.rbody;
        public float MaxDist = 2f;
        public float HoverMod = 4f, JetMod = 10f, TurbineMod = 4f;
        public Vector3 lhs { get; private set; } = Vector3.zero;

        private void LHS()
        {
            if (MaxDist <= 0f)
            {
                lhs = Vector3.zero;
            }
            lhs = (rbody.worldCenterOfMass - this.StabilizerTarget) / MaxDist;
        }

        public void FixedUpdate()
        {
            if (block.tank == null || !block.IsAttached)
            {
                return;
            }
            if (block.tank.ShouldAutoStabilise || MaxDist <= 0f)
            {
                lhs = Vector3.zero;
                return;
            }
            Vector3 com = this.rbody.worldCenterOfMass;
            if (Vector3.Distance(this.StabilizerTarget, com) > MaxDist)
            {
                this.StabilizerTarget = com + (this.StabilizerTarget - com).normalized * MaxDist;
            }
            LHS();
        }
    }
}
