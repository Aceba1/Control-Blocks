using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Control_Block
{
#warning Could try to fake dynamic bodies later
    class ClusterBody : MonoBehaviour /*, IWorldTreadmill */
    {
        /* 
        public const float MaxSpringForce = 2000f;

        public void OnMoveWorldOrigin(IntVector3 amountToMove)
        {
            if (rbody != null)
                rbody.position += amountToMove;
        }

        public Rigidbody rbody;
        public ConfigurableJoint Joint; 
        
        void Update()
        {
            if (Dynamics && rbody != null)
            {
                rbody.drag = coreTank.rbody.drag;
                rbody.angularDrag = coreTank.rbody.angularDrag;
            }
        }
        
        public ClusterBody()
        {
            ManWorldTreadmill.inst.AddListener(this);
        } 
        */

        public List<TankBlock> blocks = new List<TankBlock>();
        public Dictionary<IntVector3, byte> ClusterAPBitField = new Dictionary<IntVector3, byte>();
        public List<ModuleWeapon> blockWeapons = new List<ModuleWeapon>();
        public List<ModuleDrill> blockDrills = new List<ModuleDrill>();

        public ModuleBlockMover moduleBlockMover;
        public Tank coreTank;
        public Rigidbody parentBody;
        public bool Dirty;
        /*public bool Dynamics;*/

        //Vector3 CoG;
        public float rbody_mass;
        public Vector3 rbody_centerOfMass;
        public Vector3 rbody_inertiaTensor;

        /// <summary>
        /// Modify this after returning CoM to offset it, for locked joints
        /// </summary>
        public Vector3 rbody_centerOfMass_mask;

        public ClusterBody Destroy()
        {
            moduleBlockMover.block.tank.control.driveControlEvent.Unsubscribe(GetDriveControl);
            Clear(true);
            Destroy(gameObject);
            /* ManWorldTreadmill.inst.RemoveListener(this); */
            return null;
        }

        public bool ForceFireNextFrame;
        public bool ForceNoFireNextFrame;

        private static System.Reflection.FieldInfo ModuleDrill_m_Spinning = typeof(ModuleDrill).GetField("m_Spinning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

        public void GetDriveControl(TankControl.ControlState state)
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

        /*
        private void CreateCustomJoint()
        {
            RemoveJoint();
            parentBody = moduleBlockMover.ownerBody;

            Quaternion blockRot = moduleBlockMover.transform.localRotation;
            Vector3 anchor1 = parentBody.transform.InverseTransformPoint(moduleBlockMover.HolderPart.position);
            transform.rotation = transform.parent.rotation;
            Joint = parentBody.gameObject.AddComponent<ConfigurableJoint>();
            Joint.configuredInWorldSpace = false;
            Joint.autoConfigureConnectedAnchor = false; // Do NOT allow Unity to auto-configure anchor, or blocks will become "phantom"
            Joint.axis = blockRot * Vector3.up;
            Joint.secondaryAxis = blockRot * Vector3.forward;
            Joint.connectedBody = rbody;
            Joint.anchor = anchor1;
            Joint.connectedAnchor = transform.parent.InverseTransformPoint(moduleBlockMover.HolderPart.position); // Fix bug caused by static holders
            Joint.xMotion = ConfigurableJointMotion.Locked;
            Joint.yMotion = ConfigurableJointMotion.Locked;
            Joint.zMotion = ConfigurableJointMotion.Locked;
            Joint.angularXMotion = ConfigurableJointMotion.Locked;
            Joint.angularYMotion = ConfigurableJointMotion.Locked;
            Joint.angularZMotion = ConfigurableJointMotion.Locked;
            Joint.enableCollision = false;

            Joint.enablePreprocessing = false;//true;

#warning Create own projection solution
            //Joint.projectionMode = JointProjectionMode.PositionAndRotation;
            //Joint.projectionDistance = 10f;
            //Joint.projectionAngle = 0f;

            Joint.rotationDriveMode = RotationDriveMode.XYAndZ;

            // Testings...
            Joint.targetPosition = Vector3.zero;
            Joint.targetRotation = Quaternion.Euler(0f, 90f, 0f);  

            SetJointDrive(moduleBlockMover.SPRDAM, moduleBlockMover.SPRSTR);

            SetJointLimits(moduleBlockMover.MINVALUELIMIT, moduleBlockMover.MAXVALUELIMIT);
        }

        public void SetJointLimitLow(float limit) => Joint.lowAngularXLimit = new SoftJointLimit { bounciness = 0, contactDistance = 0, limit = limit };
        public void SetJointLimitHigh(float limit) => Joint.highAngularXLimit = new SoftJointLimit { bounciness = 0, contactDistance = 0, limit = limit };

        public void SetJointLimits(float low, float high)
        {
            SetJointLimitLow(low);
            SetJointLimitHigh(high);
        }

        public void SetJointDrive(float positionDamper, float positionSpring)
        {
            var drive = new JointDrive { positionDamper = positionDamper, positionSpring = positionSpring, maximumForce = MaxSpringForce };
            Joint.angularXDrive = drive;
            Joint.angularYZDrive = drive;
            Joint.xDrive = drive;
            Joint.yDrive = drive;
            Joint.zDrive = drive;
        }
        */
        public void RemoveJoint()
        {
            /*
            if (Joint != null)
            {
                //if (Joint.gameObject == parentBody.gameObject && Joint.connectedBody == rbody) return;
                DestroyImmediate(Joint);
            }
            */
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
                //CoG = Vector3.zero;
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
                    //float num2 = tankBlock.CurrentMass * coreTank.massScaleFactor * tankBlock.AverageGravityScaleFactor;
                    a += currentInertiaTensor + tankBlock.CurrentMass * new Vector3(localCoM.y * localCoM.y + localCoM.z * localCoM.z, localCoM.z * localCoM.z + localCoM.x * localCoM.x, localCoM.x * localCoM.x + localCoM.y * localCoM.y);

                    //Vector3 a2 = tankBlock.cachedLocalPosition + tankBlock.cachedLocalRotation * tankBlock.CentreOfGravity;
                    //CoG += num2 * a2;
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
            /* if (Dynamics)
            {
                if (rbody_mass != 0f)
                {
                    if (rbody == null)
                    {
                        rbody = gameObject.AddComponent<Rigidbody>();
                        rbody.detectCollisions = true;
                    }
                    SyncRigidbody();
                    moduleBlockMover.Print($"> Cluster mass: {rbody_mass}, com: {rbody_centerOfMass}, tank mass: {coreTank.rbody.mass} {coreTank.rbody.centerOfMass}");
                    RemoveCOMFromTank();
                    moduleBlockMover.Print($"> New tank mass: {coreTank.rbody.mass}, tank com: {coreTank.rbody.centerOfMass}");
                    CreateCustomJoint();
                }
                else
                {
                    PurgeComponents();
                }
            }
            else */
            {
                var paRBody = transform.GetComponentInParent<Rigidbody>();
                if (paRBody != coreTank.rbody) // If the parent is another cluster and NOT the tank
                {
                    ReturnCOMToRigidbody(paRBody);
                }
                /* PurgeComponents(); */
            }

            #region TankBlock slip-in
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
            #endregion
        }
        //static System.Reflection.PropertyInfo CurrentMass = typeof(TankBlock).GetProperty("CurrentMass", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //static System.Reflection.FieldInfo m_CentreOfMass = typeof(TankBlock).GetField("m_CentreOfMass", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //static System.Reflection.PropertyInfo CurrentInertiaTensor = typeof(TankBlock).GetProperty("CurrentInertiaTensor", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //static System.Reflection.FieldInfo m_TotalWeightScale = typeof(Tank).GetField("m_TotalWeightScale", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //static System.Reflection.FieldInfo m_CogPos = typeof(Tank).GetField("m_CogPos", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // COM = (com1 * m1 + com2 * m2) / (m1 + m2)
        // com1 = ((COM * m1) + (COM * m2) - (com2 * m2)) / m1

        /*
        private void SyncRigidbody()
        {
            rbody.mass = rbody_mass;
            rbody.centerOfMass = rbody_centerOfMass;
            rbody.inertiaTensor = rbody_inertiaTensor;
            rbody.inertiaTensorRotation = Quaternion.identity;
        }

        private void PurgeComponents()
        {
            if (rbody != null)
                Component.DestroyImmediate(rbody);
            if (Joint != null)
                Component.DestroyImmediate(Joint);
        } 
        */

        /*
        public void SetDynamics(bool state, bool FixCOM = true)
        {
            if (state != Dynamics)
            {
                coreTank.RequestPhysicsReset();
            }
            Dynamics = state;
        }
        */

        private void ReturnCOMToTank()
        {
            //var TWR = coreTank.GetGravityScale();

            ReturnCOMToRigidbody(coreTank.rbody);

            //m_TotalWeightScale.SetValue(coreTank, Mathf.Clamp01(TWR) * coreTank.rbody.mass);
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
            //other.mass = m1 + m2; // other.mass will have all mass in lockjoint only
        }

        private void RemoveCOMFromTank()
        {
            //var TWR = coreTank.GetGravityScale();

            RemoveCOMFromRigidbody(coreTank.rbody);

            //m_TotalWeightScale.SetValue(coreTank, Mathf.Clamp01(TWR) * coreTank.rbody.mass);
        }

        public void RemoveCOMFromRigidbody(Rigidbody other)
        {
            Vector3 COM = other.centerOfMass, com2 = rbody_centerOfMass_mask;
            float m2 = rbody_mass, m1 = other.mass - rbody_mass; // other.mass will have all mass in lockjoint only

            //other.mass = m1; // other.mass will have all mass in lockjoint only
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
            if (weapon) blockWeapons.Add(weapon);
            ModuleDrill drill = block.GetComponent<ModuleDrill>();
            if (drill) blockDrills.Add(drill);



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

                //block.trans.position += transform.TransformPoint(block.cachedLocalPosition) - coreTank.transform.TransformPoint(block.cachedLocalPosition);

                Dirty = true;
                return true;
            }
            return false;
        }

        internal void Clear(bool FixPositions)
        {
            /* PurgeComponents(); */
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
