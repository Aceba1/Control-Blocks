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
        Collider[] checkCount;
        public Vector3 Effector;
        public Vector3 CheckCenter;
        public float CheckRadius;
        void OnPool()
        {
            checkCount = new Collider[8];
        }

        void FixedUpdate()
        {
            if (block.IsAttached && block.tank != null)
            {
                int count = Physics.OverlapSphereNonAlloc(CheckCenter, CheckRadius, checkCount, Globals.inst.layerTank.mask, QueryTriggerInteraction.Ignore);
            }
        }
    }
}
