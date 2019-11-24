using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Control_Block
{
    class ClusterBody : MonoBehaviour, IWorldTreadmill
    {
        public const float MaxSpringForce = 2000f;

        public void OnMoveWorldOrigin(IntVector3 amountToMove)
        {
            rbody.position += amountToMove;
        }

        public Rigidbody rbody;
        public ConfigurableJoint Joint;
        /// <summary>
        /// Isolate COM from coreTank and move the cluster with physics
        /// </summary>
        public List<TankBlock> blocks;
        public ModuleBlockMover moduleBlockMover;
        //public SphereCollider dragSphere;
        //! Can possibly be used for AP render manipulation
        public Tank coreTank;
        public Rigidbody parentBody;
        public bool Dirty;
        public bool Dynamics = true;



        public ClusterBody()
        {
            ManWorldTreadmill.inst.AddListener(this);
        }

        public ClusterBody Destroy()
        {
            Clear(true);
            Destroy(this.gameObject);
            ManWorldTreadmill.inst.RemoveListener(this);
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

        private void CreateCustomJoint()
        {
            RemoveJoint();
            parentBody = moduleBlockMover.ownerBody;
            Quaternion blockRot = moduleBlockMover.transform.localRotation;
            Vector3 anchor1 = parentBody.transform.InverseTransformPoint(moduleBlockMover.HolderPart.position);
            transform.rotation = parentBody.transform.rotation;
            Joint = parentBody.gameObject.AddComponent<ConfigurableJoint>();
            Joint.configuredInWorldSpace = false;
            Joint.autoConfigureConnectedAnchor = false; // Do NOT allow Unity to auto-configure anchor, or blocks will become "phantom"
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

            Joint.enablePreprocessing = true; //!CHANGED
            Joint.projectionMode = JointProjectionMode.PositionAndRotation;
            Joint.projectionDistance = 10f;
            Joint.projectionAngle = 0f;

            Joint.rotationDriveMode = RotationDriveMode.XYAndZ;
            //Joint.slerpDrive = new JointDrive { positionDamper = moduleBlockMover.SPRDAM, positionSpring = moduleBlockMover.SPRSTR, maximumForce = MaxSpringForce };
            Joint.angularXDrive = new JointDrive { positionDamper = moduleBlockMover.SPRDAM, positionSpring = moduleBlockMover.SPRSTR, maximumForce = MaxSpringForce, mode = JointDriveMode.Position };
            Joint.xDrive = new JointDrive { positionDamper = moduleBlockMover.SPRDAM, positionSpring = moduleBlockMover.SPRSTR, maximumForce = MaxSpringForce, mode = JointDriveMode.Position };
            //Joint.lowAngularXLimit = new SoftJointLimit { bounciness = 0, contactDistance = 0, limit = moduleBlockMover.MINVALUELIMIT };
            //Joint.highAngularXLimit = new SoftJointLimit { bounciness = 0, contactDistance = 0, limit = moduleBlockMover.MAXVALUELIMIT };
        }

        //Vector3 CoG;
        public float rbody_mass;
        public Vector3 rbody_centerOfMass;
        public Vector3 rbody_inertiaTensor;
        public void RemoveJoint()
        {
            if (Joint != null)
            {
                //if (Joint.gameObject == parentBody.gameObject && Joint.connectedBody == rbody) return;
                DestroyImmediate(Joint);
            }
        }
        public void ResetPhysics()
        {
            if (Dirty)
            {
                Dirty = false;
                float mass = 0f;
                Vector3 CoM = Vector3.zero;
                //CoG = Vector3.zero;
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
                    //float num2 = tankBlock.CurrentMass * coreTank.massScaleFactor * tankBlock.AverageGravityScaleFactor;
                    a += currentInertiaTensor + tankBlock.CurrentMass * new Vector3(localCoM.y * localCoM.y + localCoM.z * localCoM.z, localCoM.z * localCoM.z + localCoM.x * localCoM.x, localCoM.x * localCoM.x + localCoM.y * localCoM.y);
                    
                    //Vector3 a2 = tankBlock.cachedLocalPosition + tankBlock.cachedLocalRotation * tankBlock.CentreOfGravity;
                    //CoG += num2 * a2;
                }
                if (Null)
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
            if (Dynamics)
            {
                if (rbody == null)
                {
                    rbody = gameObject.AddComponent<Rigidbody>();
                    rbody.detectCollisions = true;
                }
                SyncRigidbody();
                moduleBlockMover.Print($"> Cluster mass: {rbody_mass}, com: {rbody_centerOfMass}, tank mass:{coreTank.rbody.mass} {coreTank.rbody.centerOfMass}");
                RemoveCOMFromTank();
                moduleBlockMover.Print($"> New tank mass: {coreTank.rbody.mass}, tank com: {coreTank.rbody.centerOfMass}");
                CreateCustomJoint();
            }
            else
            {
                if (transform.parent != coreTank.trans)
                {
                    ReturnCOMToRigidbody(transform.GetComponentInParent<Rigidbody>());
                }
                if (rbody != null)
                    Component.DestroyImmediate(rbody);
                if (Joint != null)
                    Component.DestroyImmediate(Joint);
            }

            //var target = moduleBlockMover.block;
            //target.trans.localPosition = coreTank.trans.InverseTransformPoint(transform.position);
            //target.trans.localRotation = Quaternion.identity;
            //var cm = target.CurrentMass;
            //CurrentMass.SetValue(target, rbody.mass, null);
            //var cit = target.CurrentInertiaTensor;
            //CurrentInertiaTensor.SetValue(target, rbody.inertiaTensor, null);
            //var com = target.CentreOfMass;
            //m_CentreOfMass.SetValue(target, rbody.centerOfMass);
            //coreTank.BlockMassChanged(target, 0f, Vector3.zero);
            //target.trans.localPosition = target.cachedLocalPosition;
            //target.trans.localRotation = target.cachedLocalRotation;
            //CurrentMass.SetValue(target, cm, null);
            //CurrentInertiaTensor.SetValue(target, cit, null);
            //m_CentreOfMass.SetValue(target, com);

            //if (Input.GetKey(KeyCode.Alpha0))
            //if (moduleBlockMover.transform.parent != coreTank.trans)
            //{
            //    var mask = moduleBlockMover.transform.parent.GetComponent<ClusterBody>();
            //    if (mask != null)
            //        CreateMaskJoint(mask.moduleBlockMover.transform.parent.GetComponent<Rigidbody>());
            //}
        }
        //static System.Reflection.PropertyInfo CurrentMass = typeof(TankBlock).GetProperty("CurrentMass", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //static System.Reflection.FieldInfo m_CentreOfMass = typeof(TankBlock).GetField("m_CentreOfMass", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //static System.Reflection.PropertyInfo CurrentInertiaTensor = typeof(TankBlock).GetProperty("CurrentInertiaTensor", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        static System.Reflection.FieldInfo m_TotalWeightScale = typeof(Tank).GetField("m_TotalWeightScale", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //static System.Reflection.FieldInfo m_CogPos = typeof(Tank).GetField("m_CogPos", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // COM = (com1 * m1 + com2 * m2) / (m1 + m2)
        // com1 = ((COM * m1) + (COM * m2) - (com2 * m2)) / m1

        private void SyncRigidbody()
        {
            rbody.mass = rbody_mass;
            rbody.centerOfMass = rbody_centerOfMass;
            rbody.inertiaTensor = rbody_inertiaTensor;
            rbody.inertiaTensorRotation = Quaternion.identity;
        }

        public void SetDynamics(bool state, bool FixCOM = true)
        {
            //if (state)
            //{
            //    //transform.parent = coreTank.trans.parent;
            //    if (!Dynamics)
            //    {
            //        if (FixCOM)
            //        {
            //            if (transform.parent == coreTank.trans)
            //            {
            //                RemoveCOMFromTank();
            //            }
            //            else
            //            {
            //                RemoveCOMFromRigidbody(transform.parent.GetComponent<Rigidbody>());
            //            }
            //            rbody = gameObject.AddComponent<Rigidbody>();
            //            rbody.detectCollisions = true;
            //            SyncRigidbody();
            //            CreateCustomJoint();
            //        }
            //    }
            //}
            //else
            //{
            //    //transform.parent = moduleBlockMover.transform.parent;
            //    if (Dynamics)
            //    {
            //        if (FixCOM)
            //        {
            //            if (transform.parent == coreTank.trans)
            //            {
            //                ReturnCOMToTank();
            //            }
            //            else
            //            {
            //                ReturnCOMToRigidbody(transform.GetComponentInParent<Rigidbody>());
            //            }
            //        }
            //        Component.DestroyImmediate(Joint);
            //        Component.DestroyImmediate(rbody);
            //    }
            //}
            if (state != Dynamics)
            {
                coreTank.RequestPhysicsReset();
            }
            Dynamics = state;
        }

        private void ReturnCOMToTank()
        {
            var TWR = coreTank.GetGravityScale();

            ReturnCOMToRigidbody(coreTank.rbody);

            m_TotalWeightScale.SetValue(coreTank, TWR * coreTank.rbody.mass);
        }

        private void ReturnCOMToRigidbody(Rigidbody other)
        {
            Vector3 com1 = other.centerOfMass, com2 = rbody_centerOfMass;
            float m2 = rbody_mass, m1 = other.mass;

            other.centerOfMass = ((com1 * m1) + (com2 * m2)) / (m1 + m2);
            other.mass = m1 + m2;
        }

        private void RemoveCOMFromTank()
        {
            var TWR = coreTank.GetGravityScale();

            RemoveCOMFromRigidbody(coreTank.rbody);

            m_TotalWeightScale.SetValue(coreTank, TWR * coreTank.rbody.mass);
        }

        private void RemoveCOMFromRigidbody(Rigidbody other)
        {
            Vector3 COM = other.centerOfMass, com2 = rbody_centerOfMass;
            float m2 = rbody_mass, m1 = other.mass - rbody_mass;

            other.mass = m1;
            other.centerOfMass = ((COM * m1) + (COM * m2) - (com2 * m2)) / m1;
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

        internal void Clear(bool FixPositions)
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
                    if (FixPositions)
                    {
                        block.trans.localPosition = block.cachedLocalPosition;
                        block.trans.localRotation = block.cachedLocalRotation;
                    }
                }
            }
            Dirty = true;
            blocks.Clear();
        }
    }
}
