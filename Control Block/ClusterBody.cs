using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Control_Block
{

    class ClusterBody : MonoBehaviour /*, IWorldTreadmill */
    {

        public List<TankBlock> blocks = new List<TankBlock>();
        public Dictionary<IntVector3, byte> ClusterAPBitField = new Dictionary<IntVector3, byte>();
        public List<ModuleWeapon> blockWeapons = new List<ModuleWeapon>();
        public List<ModuleDrill> blockDrills = new List<ModuleDrill>();

        public ModuleBlockMover moduleBlockMover;
        public Tank coreTank;
        public Rigidbody parentBody;
        public bool Dirty;

        public float rbody_mass;
        public Vector3 rbody_centerOfMass;
        public Vector3 rbody_inertiaTensor;

        /// <summary>
        /// Modify this after returning CoM to offset it, for locked joints
        /// </summary>
        public Vector3 rbody_centerOfMass_mask;

        public ClusterBody Destroy()
        {
            coreTank?.control.driveControlEvent.Unsubscribe(GetDriveControl);
            Clear(true);
            Destroy(gameObject);
            return null;
        }

        public bool ForceFireNextFrame;
        public bool ForceNoFireNextFrame;

        private static System.Reflection.FieldInfo ModuleDrill_m_Spinning = typeof(ModuleDrill).GetField("m_Spinning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

        public void GetDriveControl(TankControl.ControlState state)
        {
            try
            {
                if (ForceFireNextFrame)
                {
                    foreach (var weapon in blockWeapons) // Set every cached ModuleWeapon on
                        weapon.FireControl = true;

                    foreach (var drill in blockDrills)
                        ModuleDrill_m_Spinning.SetValue(drill, true); // why is this value private

                    ForceFireNextFrame = false;
                }
                if (ForceNoFireNextFrame)
                {
                    foreach (var weapon in blockWeapons) // Set every cached ModuleWeapon off
                        weapon.FireControl = false;

                    ForceNoFireNextFrame = false;
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Null weapon block on " + coreTank.name + ":" + moduleBlockMover.UIName + "!");
                blockWeapons.RemoveAll(v => v == null);
                blockDrills.RemoveAll(v => v == null);
            }
        }
        public void RemoveJoint()
        {

        }

        public bool IsAnchored()
        {
            foreach (var anchor in GetComponentsInChildren<ModuleAnchor>())
            {
                if (anchor.IsAnchored) return true;
            }
            return false;
        }

        public void ResetPhysics()
        {
            if (Dirty)
            {
                Dirty = false;
                float mass = 0f;
                Vector3 CoM = Vector3.zero;
                Vector3 a = Vector3.zero;
                bool hasNullBlocks = false;
                foreach (TankBlock tankBlock in blocks)
                {
                    if (tankBlock == null)
                    {
                        hasNullBlocks = true;
                        continue;
                    }
                    Vector3 currentInertiaTensor = tankBlock.CurrentInertiaTensor;
                    Vector3 localCoM = tankBlock.cachedLocalPosition + tankBlock.cachedLocalRotation * tankBlock.CentreOfMass;
                    mass += tankBlock.CurrentMass;
                    CoM += tankBlock.CurrentMass * localCoM;
                    a += currentInertiaTensor + tankBlock.CurrentMass * new Vector3(localCoM.y * localCoM.y + localCoM.z * localCoM.z, localCoM.z * localCoM.z + localCoM.x * localCoM.x, localCoM.x * localCoM.x + localCoM.y * localCoM.y);
                }
                if (hasNullBlocks)
                {
                    Console.WriteLine("There were null blocks in a ClusterBody! " + moduleBlockMover.transform.localPosition.ToString());
                    blocks.RemoveAll(b => b == null);
                }
                rbody_mass = mass * coreTank.massScaleFactor;
                if (mass == 0f)
                {
                    CoM = Vector3.zero;
                }
                else
                {
                    CoM /= mass;
                }
                rbody_centerOfMass = CoM;
                a -= mass * new Vector3(CoM.y * CoM.y + CoM.z * CoM.z, CoM.z * CoM.z + CoM.x * CoM.x, CoM.x * CoM.x + CoM.y * CoM.y);
                rbody_inertiaTensor = a * coreTank.massScaleFactor * coreTank.inertiaTensorScaleFactor;
            }
            rbody_centerOfMass_mask = rbody_centerOfMass;
            {
                var paRBody = transform.GetComponentInParent<Rigidbody>();
                if (paRBody != coreTank.rbody) // If the parent is another cluster and NOT the tank
                {
                    ReturnCOMToRigidbody(paRBody);
                }
            }
        }


        private void ReturnCOMToTank()
        {
            ReturnCOMToRigidbody(coreTank.rbody);
        }

        public void FixMaskedCOM(Transform other)
        {
            rbody_centerOfMass_mask = other.InverseTransformPoint(transform.TransformPoint(rbody_centerOfMass));
        }

        public void ReturnCOMToRigidbody(Rigidbody other)
        {
            Vector3 com1 = other.centerOfMass, com2 = rbody_centerOfMass_mask;
            float m2 = rbody_mass, m1 = other.mass - rbody_mass; // other.mass will have all mass in lockjoint only

            other.centerOfMass = ((com1 * m1) + (com2 * m2)) / (m1 + m2);
        }

        private void RemoveCOMFromTank()
        {
            RemoveCOMFromRigidbody(coreTank.rbody);
        }

        public void RemoveCOMFromRigidbody(Rigidbody other)
        {
            Vector3 COM = other.centerOfMass, com2 = rbody_centerOfMass_mask;
            float m2 = rbody_mass, m1 = other.mass - rbody_mass; // other.mass will have all mass in lockjoint only
            other.centerOfMass = ((COM * m1) + (COM * m2) - (com2 * m2)) / m1;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Console.WriteLine("Collision entered! " + coreTank.name + " & " + collision.gameObject.name);
            coreTank.HandleCollision(collision, false);
        }
        private void OnCollisionStay(Collision collision)
        {
            coreTank.HandleCollision(collision, true);
        }
        private void OnTriggerEnter(Collider otherCollider)
        {
            coreTank.TriggerEvent.Send(otherCollider, 0);
        }
        private void OnTriggerExit(Collider otherCollider)
        {
            coreTank.TriggerEvent.Send(otherCollider, 2);
        }

        public void AddBlock(TankBlock block, IntVector3 pos, OrthoRotation rot)
        {
            blocks.Add(block);
            block.trans.parent = transform;
            block.trans.localPosition = pos;
            block.trans.localRotation = rot;
            Dirty = true;

            for (int ap = 0; ap < block.attachPoints.Length; ap++)
            {
                IntVector3 filledCellForAPIndex = block.GetFilledCellForAPIndex(ap);
                IntVector3 v3 = block.attachPoints[ap] * 2f - filledCellForAPIndex - filledCellForAPIndex;
                IntVector3 index = pos + rot * filledCellForAPIndex;
                byte b = (rot * v3).APHalfBits();
                ClusterAPBitField.TryGetValue(index, out byte ptr);
                ptr |= b;
                ClusterAPBitField[index] = ptr;
            }

            ModuleWeapon weapon = block.GetComponent<ModuleWeapon>();
            if (weapon != null) blockWeapons.Add(weapon);
            ModuleDrill drill = block.GetComponent<ModuleDrill>();
            if (drill != null) blockDrills.Add(drill);

            return;
        }

        public bool TryRemoveBlock(TankBlock block)
        {
            if (blocks.Remove(block))
            {
                ClusterAPBitField.Remove(block.cachedLocalPosition);

                var weapon = block.GetComponent<ModuleWeapon>();
                if (weapon) blockWeapons.Remove(weapon);
                var drill = block.GetComponent<ModuleDrill>();
                if (drill) blockDrills.Remove(drill);
                Dirty = true;
                return true;
            }
            return false;
        }

        internal void Clear(bool FixPositions)
        {
            if (coreTank == null)
            {
                foreach (var block in blocks)
                {
                    if (block == null || block.trans.parent != transform) continue;
                    block.trans.parent = null;
                }
            }
            else
            {
                foreach (var block in blocks)
                {
                    if (block == null || block.trans.parent != transform) continue;
                    block.trans.parent = coreTank.trans;
                    if (FixPositions)
                    {
                        block.trans.localPosition = block.cachedLocalPosition;
                        block.trans.localRotation = block.cachedLocalRotation;
                    }
                }
            }
            Dirty = true;
            blocks.Clear();
            ClusterAPBitField.Clear();
        }

        public void InitializeAPCache()
        {
            var block = moduleBlockMover.block;
            for (int ap = 0; ap < block.attachPoints.Length; ap++)
            {
                foreach (var stp in moduleBlockMover.startblockpos)
                {
                    var atp = block.attachPoints[ap];
                    if (atp.Approximately(stp, 0.6f))
                    {
                        IntVector3 filledCellForAPIndex = block.GetFilledCellForAPIndex(ap);
                        IntVector3 v3 = atp * 2f - filledCellForAPIndex - filledCellForAPIndex;
                        IntVector3 index = block.cachedLocalPosition + block.cachedLocalRotation * filledCellForAPIndex;
                        byte b = (block.cachedLocalRotation * v3).APHalfBits();
                        ClusterAPBitField.TryGetValue(index, out byte ptr);
                        ptr |= b;
                        ClusterAPBitField[index] = ptr;
                        break;
                    }
                }
            }
        }
    }
}
