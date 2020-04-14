using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Control_Block
{
    class ModuleFrictionPad : Module
    {
        public float threshold = 20f, strength = 0.75f;
        public Vector3 effector = Vector3.up * 0.5f;
        LayerMask layer = LayerMask.GetMask("Terrain", "Scenery", "Landmarks", "Cosmetic");
        bool DoIt;
        bool Heart = false;
        Vector3 LastPos;
        //Vector3 GetEffector(TankBlock b) => b.transform.rotation * effector;
        //void OnTriggerStay(Collider other)
        //{
        //    if (Heart != Class1.PistonHeart) return;
        //    if (DoIt && ((layer.value | (1<<other.gameObject.layer)) == layer.value))
        //    {
        //        DoIt = false;
        //        var block = gameObject.GetComponent<TankBlock>();
        //        if (block != null && block.tank != null && !block.tank.beam.IsActive)
        //        {
        //            var force = Vector3.ProjectOnPlane(LastPos - (block.transform.position + GetEffector(block)), block.transform.rotation * Vector3.up) * strength;
        //            if (force.magnitude < threshold*4f)
        //            {
        //                block.tank.transform.position += force;
        //                var pos = block.transform.position + GetEffector(block);
        //                var rbody = block.tank.rbody;
        //                block.tank.rbody.AddForceAtPosition((/*Vector3.ProjectOnPlane(-rbody.GetPointVelocity(pos), block.transform.rotation * Vector3.up) * strength +*/ force) * 8 + block.transform.rotation * Vector3.down * threshold * 8, pos, ForceMode.Impulse);
        //            }
        //            //else force = force.normalized;
        //            //block.tank.rbody.AddForceAtPosition(force * 30f, block.transform.position + (block.transform.rotation * Vector3.up * 0.5f), ForceMode.VelocityChange);
        //        }
        //        LastPos = block.transform.position + GetEffector(block);
        //    }
        //}
        //Vector3 LastPos;
        //void FixedUpdate()
        //{
        //    //var block = gameObject.GetComponent<TankBlock>();
        //    DoIt = true;
        //}

        void OnTriggerStay(Collider other)
        {
            if (!DoIt && ((layer.value | (1 << other.gameObject.layer)) == layer.value))
                DoIt = true;
        }

        void FixedUpdate()
        {
            if (Heart != Class1.PistonHeart) return;
            if (DoIt && block.tank != null && !block.tank.beam.IsActive && !block.tank.IsAnchored)
            {
                DoIt = false;
                var rbody = block.tank.rbody;
                var delta = LastPos - block.centreOfMassWorld;
                float cost = rbody.mass * rbody.mass * delta.sqrMagnitude;
                if (cost != 0f) delta *= Mathf.Min(threshold * threshold, cost) / cost;
                block.tank.transform.position += delta * strength;
                rbody.AddForceAtPosition(delta, block.centreOfMassWorld, ForceMode.VelocityChange);
            }
        }

        void Update()
        {
            Heart = Class1.PistonHeart;
            LastPos = block.centreOfMassWorld;
        }
    }
}
