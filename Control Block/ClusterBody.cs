using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Control_Block
{
    class ClusterBody : MonoBehaviour
    {
        public const float MaxSpringForce = 2000f;

        public Rigidbody rbody;
        public ConfigurableJoint Joint;
        /// <summary>
        /// Isolate COM from coreTank and move the cluster with physics
        /// </summary>
        public List<TankBlock> blocks;
        //public SphereCollider dragSphere;
        //! Can possibly be used for AP render manipulation
        public Tank coreTank;
        public Rigidbody parentBody;
        public bool Dirty;

        public ClusterBody Destroy()
        {
            Clear();
            Destroy(this.gameObject);
            return null;
        }

        private List<ConfigurableJoint> masks = new List<ConfigurableJoint>();

        private bool CreateMaskJoint(Rigidbody bodyToMask)
        {
            if (masks.Count != 0)
            {
                for (int i = masks.Count - 1; i >= 0; i--)
                {
                    if (masks == null) masks.RemoveAt(i);
                    else if (masks[i].connectedBody == bodyToMask) return false;
                }
            }
            var joint = gameObject.AddComponent<ConfigurableJoint>();
            masks.Add(joint);
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Free;
            joint.connectedBody = bodyToMask;
            joint.enableCollision = false;
            joint.enablePreprocessing = false;
            return true;
        }

        private void CreateCustomJoint(ModuleBlockMover moduleBlockMover)
        {
            parentBody = moduleBlockMover.transform.parent.GetComponent<Rigidbody>();
            if (Joint != null)
            {
                if (Joint.gameObject == parentBody.gameObject && Joint.connectedBody == rbody) return;
                DestroyImmediate(Joint);
            }
            Quaternion blockRot = moduleBlockMover.transform.localRotation;
            Vector3 anchor1 = parentBody.transform.InverseTransformPoint(moduleBlockMover.HolderPart.position);
            transform.rotation = parentBody.transform.rotation;
            Joint = parentBody.gameObject.AddComponent<ConfigurableJoint>();
            Joint.configuredInWorldSpace = false;
            Joint.autoConfigureConnectedAnchor = false;
            Joint.axis = blockRot * Vector3.up;
            Joint.secondaryAxis = blockRot * Vector3.forward;
            Joint.enableCollision = false;
            Joint.connectedBody = rbody;
            Joint.anchor = anchor1;
            Joint.connectedAnchor = anchor1;
            Joint.xMotion = ConfigurableJointMotion.Locked;
            Joint.yMotion = ConfigurableJointMotion.Locked;
            Joint.zMotion = ConfigurableJointMotion.Locked;
            Joint.angularXMotion = ConfigurableJointMotion.Locked;
            Joint.angularYMotion = ConfigurableJointMotion.Locked;
            Joint.angularZMotion = ConfigurableJointMotion.Locked;
            Joint.enableCollision = false;
            Joint.enablePreprocessing = false;
            Joint.projectionMode = JointProjectionMode.PositionAndRotation;
            Joint.projectionDistance = 0;
            Joint.projectionAngle = 0;

            //Joint.rotationDriveMode = RotationDriveMode.XYAndZ;
            Joint.angularXDrive = new JointDrive { positionDamper = moduleBlockMover.SPRDAM, positionSpring = moduleBlockMover.SPRSTR, maximumForce = MaxSpringForce };
            Joint.xDrive = new JointDrive { positionDamper = moduleBlockMover.SPRDAM, positionSpring = moduleBlockMover.SPRSTR, maximumForce = MaxSpringForce };
            Joint.lowAngularXLimit = new SoftJointLimit { bounciness = 0, contactDistance = 0, limit = moduleBlockMover.MINVALUELIMIT };
            Joint.highAngularXLimit = new SoftJointLimit { bounciness = 0, contactDistance = 0, limit = moduleBlockMover.MAXVALUELIMIT };
        }
        Vector3 CoG;
        public void ResetPhysics(ModuleBlockMover moduleBlockMover)
        {
            if (Dirty)
            {
                Dirty = false;
                float mass = 0f;
                Vector3 CoM = Vector3.zero;
                CoG = Vector3.zero;
                Vector3 a = Vector3.zero;
                bool Null = false;
                foreach (TankBlock tankBlock in blocks)
                {
                    if (tankBlock == null)
                    {
                        Null = true;
                        continue;
                    }
                    Vector3 currentInertiaTensor = tankBlock.CurrentInertiaTensor;
                    Vector3 localCoM = tankBlock.cachedLocalPosition + tankBlock.cachedLocalRotation * tankBlock.CentreOfMass;
                    mass += tankBlock.CurrentMass;
                    CoM += tankBlock.CurrentMass * localCoM;
                    float num2 = tankBlock.CurrentMass * coreTank.massScaleFactor * tankBlock.AverageGravityScaleFactor;
                    a += currentInertiaTensor + tankBlock.CurrentMass * new Vector3(localCoM.y * localCoM.y + localCoM.z * localCoM.z, localCoM.z * localCoM.z + localCoM.x * localCoM.x, localCoM.x * localCoM.x + localCoM.y * localCoM.y);
                    
                    Vector3 a2 = tankBlock.cachedLocalPosition + tankBlock.cachedLocalRotation * tankBlock.CentreOfGravity;
                    CoG += num2 * a2;
                }
                if (Null)
                {
                    Console.WriteLine("There were null blocks in a ClusterBody! " + moduleBlockMover.transform.localPosition);
                    blocks.RemoveAll(b => b == null);
                }
                rbody.mass = mass * coreTank.massScaleFactor;
                if (mass == 0f)
                {
                    CoM = Vector3.zero;
                }
                else
                {
                    CoM /= mass;
                }
                this.rbody.centerOfMass = CoM;
                a -= mass * new Vector3(CoM.y * CoM.y + CoM.z * CoM.z, CoM.z * CoM.z + CoM.x * CoM.x, CoM.x * CoM.x + CoM.y * CoM.y);
                rbody.inertiaTensor = a * coreTank.massScaleFactor * coreTank.inertiaTensorScaleFactor;
                rbody.inertiaTensorRotation = Quaternion.identity;
            }
            CreateCustomJoint(moduleBlockMover);
            var TWR = coreTank.GetGravityScale();
            moduleBlockMover.Print($"cluster mass: {rbody.mass} {rbody.centerOfMass} , tank mass: {coreTank.rbody.mass} {coreTank.rbody.centerOfMass}");
            RemoveCOMFromTank();
            if (coreTank.EnableGravity)
            {
                m_TotalWeightScale.SetValue(coreTank, TWR * coreTank.rbody.mass);
            }
            //if (moduleBlockMover.transform.parent != coreTank.trans)
            //{
            //    var mask = moduleBlockMover.transform.parent.parent.GetComponentInParent<Rigidbody>();
            //    if (mask != null) // which it shouldn't
            //        CreateMaskJoint(mask);
            //    else // and a pretty strong else
            //        Console.WriteLine("SubCluster " + moduleBlockMover.block.cachedLocalPosition.ToString() + ": No rigidbody found to mask!");
            //}
            moduleBlockMover.Print($"New tank mass: {coreTank.rbody.mass} {coreTank.rbody.centerOfMass}");
        }

        static System.Reflection.FieldInfo m_TotalWeightScale = typeof(Tank).GetField("m_TotalWeightScale", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //static System.Reflection.FieldInfo m_CogPos = typeof(Tank).GetField("m_CogPos", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // COM = (com1 * m1 + com2 * m2) / (m1 + m2)
        // com1 = ((COM * m1) + (COM * m2) - (com2 * m2)) / m1

        //public void ReturnCOMToTank()
        //{
        //    Console.WriteLine("RETURNED COM TO TANK AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        //    Vector3 com1 = coreTank.rbody.centerOfMass, com2 = rbody.centerOfMass;
        //    float m2 = rbody.mass, m1 = coreTank.rbody.mass;
        //    coreTank.rbody.centerOfMass = ((com1 * m1) + (com2 * m2)) / (m1 + m2);

        //    coreTank.rbody.mass = m1 + m2;
        //}

        private void RemoveCOMFromTank()
        {
            Vector3 COM = coreTank.rbody.centerOfMass, com2 = rbody.centerOfMass;
            float m2 = rbody.mass, m1 = coreTank.rbody.mass - rbody.mass;

            coreTank.rbody.mass = m1;
            coreTank.rbody.centerOfMass = ((COM * m1) + (COM * m2) - (com2 * m2)) / m1;

            //coreTank.rbody.inertiaTensor = coreTank.rbody.inertiaTensor - rbody.inertiaTensor;
            //+ M * Vector3.Scale(COM, COM) 
            //- m2 * Vector3.Scale(com2, com2);
            //coreTank.rbody.inertiaTensor -= rbody.inertiaTensor;
        }

        private void OnCollisionEnter(Collision collision)
        {
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

        public void AddBlock(TankBlock block, Vector3 pos, Quaternion rot)
        {
            blocks.Add(block);
            block.trans.parent = transform;
            block.trans.localPosition = pos;
            block.trans.localRotation = rot;
            Dirty = true;
            return;
        }

        public bool TryRemoveBlock(TankBlock block)
        {
            if (blocks.Contains(block))
            {
                blocks.Remove(block);
                Dirty = true;
                return true;
            }
            return false;
        }

        internal void Clear()
        {
            if (Joint != null)
            {
                DestroyImmediate(Joint);
            }
            //if (masks.Count != 0)
            //{
            //    for (int i = 0; i < masks.Count; i++)
            //    {
            //        if (masks[i] != null)
            //            Component.DestroyImmediate(masks[i]);
            //    }
            //    masks.Clear();
            //}
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
                    //block.trans.localPosition = block.cachedLocalPosition;
                    //block.trans.localRotation = block.cachedLocalRotation;
                }
            }
            Dirty = true;
            blocks.Clear();
        }
    }
}
