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

        public static GameObject m_AttachParticlesGo;
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
        public bool Dirty { get; private set; }

        public ClusterBody Destroy()
        {
            if (Joint != null)
            {
                DestroyImmediate(Joint);
            }
            Clear();
            Destroy(this.gameObject);
            return null;
        }

        private void CreateCustomJoint(ModuleBlockMover moduleBlockMover)
        {
            parentBody = moduleBlockMover.GetComponentInParent<Rigidbody>();
            if (Joint != null)
            {
                if (Joint.gameObject == parentBody.gameObject && Joint.connectedBody == rbody) return;
                DestroyImmediate(Joint);
            }
            Quaternion blockRot = moduleBlockMover.block.cachedLocalRotation;
            var oldRot = transform.rotation;
            transform.rotation = parentBody.transform.rotation;
            Joint = parentBody.gameObject.AddComponent<ConfigurableJoint>();
            Joint.configuredInWorldSpace = false;
            Joint.autoConfigureConnectedAnchor = false;
            Joint.anchor = parentBody.transform.InverseTransformPoint(moduleBlockMover.HolderPart.position);
            Joint.axis = blockRot * Vector3.up;
            Joint.secondaryAxis = blockRot * Vector3.forward;
            Joint.enableCollision = false;
            Joint.connectedBody = rbody;
            Joint.connectedAnchor = transform.InverseTransformPoint(moduleBlockMover.HolderPart.position);
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
            transform.rotation = oldRot;
        }

        public void ResetPhysics(ModuleBlockMover moduleBlockMover)
        {
            if (Dirty)
            {
                Dirty = false;
                float mass = 0f;
                float m_TotalWeightScale = 0f;
                Vector3 CoM = Vector3.zero;
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
                    Vector3 centreOfMass = tankBlock.CentreOfMass;
                    Vector3 localCoM = tankBlock.trans.localPosition + tankBlock.trans.localRotation * centreOfMass;
                    mass += tankBlock.CurrentMass;
                    CoM += tankBlock.CurrentMass * localCoM;
                    float num2 = tankBlock.CurrentMass * coreTank.massScaleFactor * tankBlock.AverageGravityScaleFactor;
                    m_TotalWeightScale += num2;
                    a += currentInertiaTensor + tankBlock.CurrentMass * new Vector3(localCoM.y * localCoM.y + localCoM.z * localCoM.z, localCoM.z * localCoM.z + localCoM.x * localCoM.x, localCoM.x * localCoM.x + localCoM.y * localCoM.y);
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
        }

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

        public void RemoveCOMFromTank()
        {
            Vector3 COM = coreTank.rbody.centerOfMass, com2 = rbody.centerOfMass;
            float m2 = rbody.mass, M = coreTank.rbody.mass, 
                m1 = M - m2;

            coreTank.rbody.centerOfMass = ((COM * m1) + (COM * m2) - (com2 * m2)) / m1;
            coreTank.rbody.mass = m1;
            coreTank.rbody.inertiaTensor = coreTank.rbody.inertiaTensor - rbody.inertiaTensor;
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
            bool removed = blocks.Remove(block);
            Dirty |= removed;
            return removed;
        }

        internal void Clear()
        {
            if (coreTank == null)
            {
                foreach (var block in blocks)
                {
                    if (block == null) continue;
                    block.trans.parent = null;
                }
            }
            else
            {
                foreach (var block in blocks)
                {
                    if (block == null) continue;
                    block.trans.parent = coreTank.trans;
                    //block.trans.localPosition = block.cachedLocalPosition;
                    //block.trans.localRotation = block.cachedLocalRotation;
                }
            }
            blocks.Clear();
        }
    }
}
