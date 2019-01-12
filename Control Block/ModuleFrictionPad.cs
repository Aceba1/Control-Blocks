using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Control_Block
{
    class ModuleFrictionPad : MonoBehaviour
    {
        LayerMask layer = LayerMask.GetMask("Terrain", "Scenery", "Landmarks", "Cosmetic");
        bool DoIt;
        void OnTriggerStay(Collider other)
        {
            if (DoIt && ((layer.value | (1<<other.gameObject.layer)) == layer.value))
            {
                DoIt = false;
                var block = gameObject.GetComponent<TankBlock>();
                if (block != null && block.tank != null)
                {
                    //    collisionData = "COLLIDING";
                    var force = Vector3.ProjectOnPlane(LastPos - (block.transform.position + (block.transform.rotation * Vector3.up * 0.5f)), block.transform.rotation * Vector3.up) * 1f;
                    if (force.magnitude < .25f)
                    {
                        block.tank.transform.position += force;
                        var pos = block.transform.position + (block.transform.rotation * Vector3.up * 0.5f);
                        var rbody = block.tank.rbody;
                        block.tank.rbody.AddForceAtPosition(Vector3.ProjectOnPlane(-rbody.GetPointVelocity(pos)+force, block.transform.rotation * Vector3.up) * 0.35f + block.transform.rotation * Vector3.down * 0.1f, pos, ForceMode.VelocityChange);
                    }
                    //else force = force.normalized;
                    //block.tank.rbody.AddForceAtPosition(force * 30f, block.transform.position + (block.transform.rotation * Vector3.up * 0.5f), ForceMode.VelocityChange);
                }
                LastPos = block.transform.position + (block.transform.rotation * Vector3.up * 0.5f);
            }
        }
        Vector3 LastPos;
        void FixedUpdate()
        {
            var block = gameObject.GetComponent<TankBlock>();
            DoIt = true;
        }
    }
}
