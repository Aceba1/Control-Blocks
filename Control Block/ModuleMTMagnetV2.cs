using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Control_Block
{
    class ModuleMTMagnetV2 : Module
    {
        static Collider[] checkCount = new Collider[8];
        const float GRIEFTIME = 1.5f;
        ModuleMTMagnetV2 bond;
        float griefEnd;

        public float VelocityCorrection;
        public float TransformCorrection;
        public Vector3 Effector;
        //public Vector3 CheckCenter;
        public float CheckRadius;

        public Vector3 WorldEffector => transform.TransformPoint(Effector);
        //public Vector3 WorldCenter => transform.TransformPoint(CheckCenter);

        void OnPool()
        {
            block.DetachEvent.Subscribe(Break);
        }

        void OnRecycle()
        {
            if (bond != null)
                Break();
        }

        void Bond(ModuleMTMagnetV2 other)
        {
            bond = other;
            other.bond = this;
        }

        void Break()
        {
            bond.bond = null;
            bond.griefEnd = Time.time + GRIEFTIME;

            bond = null;
            griefEnd = Time.time + GRIEFTIME;
        }

        void FixedUpdate()
        {
            bool cannotBond = (Input.GetKeyDown(KeyCode.X) && block.tank.PlayerFocused) || griefEnd > Time.time || block.IsAttached || block.tank != null;
            if (bond != null)
            {
                if (cannotBond || (WorldEffector - bond.WorldEffector).sqrMagnitude > (CheckRadius * CheckRadius * 2))
                {
                    Break();
                }
                else
                {
                    var Bm = bond.block.tank.rbody.mass;
                    var Am = block.tank.rbody.mass;
                    var offset = (bond.WorldEffector - WorldEffector) * TransformCorrection;
                    var tension = bond.block.tank.rbody.GetPointVelocity(bond.WorldEffector) - block.tank.rbody.GetPointVelocity(WorldEffector);

                    if (!block.tank.IsAnchored && !block.tank.beam.IsActive)
                    {
                        block.tank.transform.position += offset * (Am / (Am + Bm));
                        block.tank.rbody.AddForceAtPosition((offset + tension) * VelocityCorrection, WorldEffector);
                    }
                    if (!bond.block.tank.IsAnchored && !bond.block.tank.beam.IsActive)
                    {
                        bond.block.tank.transform.position -= offset * (Bm / (Am + Bm));
                        bond.block.tank.rbody.AddForceAtPosition((offset + tension) * -VelocityCorrection, bond.WorldEffector);
                    }
                }
            }
            else if (!cannotBond)
            {
                int count = Physics.OverlapSphereNonAlloc(WorldEffector, CheckRadius, checkCount, Globals.inst.layerTank.mask, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count; i++)
                {
                    var other = checkCount[count].GetComponent<ModuleMTMagnetV2>();
                    if (other != null)
                    {
                        if (other.block.visible.ItemType == block.visible.ItemType && other.bond == null && other.griefEnd <= Time.time)
                        {
                            Bond(other);
                        }
                    }
                }
            }
        }
    }
}
