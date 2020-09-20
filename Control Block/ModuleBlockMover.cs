using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Control_Block
{
    public class ModuleBlockMover : Module, TechAudio.IModuleAudioProvider
    {
        internal class ModuleBMPart : Module
        {
            public ModuleBlockMover parent;
        }

        public string UIName;

        public TechAudio.SFXType SFX;

        /// <summary>
        /// Body for holding blocks
        /// </summary>
        [NonSerialized]
        internal ClusterBody Holder;

        /// <summary>
        /// Joint of which binds the two techs together
        /// </summary>
        public UnityEngine.ConfigurableJoint HolderJoint => /* Holder ? Holder.Joint : */ null;

        /// <summary>
        /// Preset number for quantity of animation parts on block
        /// </summary>
        public int PartCount = 1;

        /// <summary>
        /// Models on block for animation purposes
        /// </summary>
        [NonSerialized]
        public Transform[] parts;

        /// <summary>
        /// The part who'se transform is responsible for the dummy tank's positioning
        /// </summary>
        public Transform HolderPart => parts[PartCount - 1];

        [NonSerialized]
        public Vector3 relativeCenter;

        /// <summary>
        /// WorldTreadmill interference security measure
        /// </summary>
        [NonSerialized]
        public bool Heart;

        /// <summary>
        /// Animation curves for determining position
        /// </summary>
        public AnimationCurve[] posCurves;

        public bool useRotCurves = false, usePosCurves = false;

        /// <summary>
        /// 0:Euler (X, Y, Z), 1:Quaternion (X, Y, Z, W), 2:Axis (X, Y, Z, A)
        /// </summary>
        public byte rotType = 0;

        /// <summary>
        /// Animation curves for determining rotation
        /// </summary>
        public AnimationCurve[] rotCurves;

        /// <summary>
        /// Relative location of where blocks should be if attached to the head of this block
        /// </summary>
        public IntVector3[] startblockpos;

        //public int MaximumBlockPush = 64;
        /// <summary>
        /// Trigger reevaluation of block
        /// </summary>
        [NonSerialized]
        public bool Dirty = true;

        [NonSerialized]
        public bool Valid = true;
        [NonSerialized]
        public string InvalidReason = "Corrupt";

        /// <summary>
        /// Block-cache for blocks found after re-evaluation
        /// </summary>
        [NonSerialized]
        public List<TankBlock> GrabbedBlocks;

        [NonSerialized]
        public List<ModuleBlockMover> GrabbedBlockMovers;

        [NonSerialized]
        public List<TankBlock> StarterBlocks;

        [NonSerialized]
        public List<TankBlock> IgnoredBlocks;

        /// <summary>
        /// Event-cache for attaching and detaching to a tank
        /// </summary>
        [NonSerialized]
        public Action<TankBlock, Tank> tankAttachBlockAction, tankDetachBlockAction;

        //public Action tankResetPhysicsAction;

        /// <summary>
        /// Get the immediate rigidbody of the parent of this block. Tank if root, Cluster if grabbed.
        /// </summary>
        public Rigidbody ownerBody => transform.parent.GetComponentInParent<Rigidbody>();

        public bool IsControlledByNet => ManGameMode.inst.IsCurrentModeMultiplayer() && block.tank != ManNetwork.inst.MyPlayer.CurTech.tech;

        internal float MINVALUELIMIT
        {
            get
            {
                //if (IsPlanarVALUE)
                //    return (_CENTERLIMIT - _EXTENTLIMIT + 720) % 360;
                //else
                return _CENTERLIMIT - _EXTENTLIMIT;
            }
            set
            {
                float o = MAXVALUELIMIT;
                if (IsPlanarVALUE)
                {
                    float v = (value + 360) % 360;
                    if (o < v)
                        o += 360;
                    _CENTERLIMIT = ((v + o) * 0.5f + 360) % 360;
                    _EXTENTLIMIT = (o - v) * 0.5f;
                    if (_EXTENTLIMIT.Approximately(0f)) _EXTENTLIMIT = HalfLimitVALUE;
                }
                else
                {
                    if (value > o)
                    {
                        _CENTERLIMIT = value;
                        _EXTENTLIMIT = 0f;
                    }
                    else
                    {
                        _CENTERLIMIT = (value + o) * 0.5f;
                        _EXTENTLIMIT = (o - value) * 0.5f;
                    }
                }
            }
        }

        internal float MAXVALUELIMIT
        {
            get
            {
                //if (IsPlanarVALUE)
                //    return (_CENTERLIMIT + _EXTENTLIMIT + 720) % 360;
                //else
                return _CENTERLIMIT + _EXTENTLIMIT;
            }
            set
            {
                float o = MINVALUELIMIT;
                if (IsPlanarVALUE)
                {
                    float v = (value + 360) % 360;
                    if (o > v)
                        v += 360;
                    _CENTERLIMIT = ((v + o) * 0.5f + 360) % 360;
                    _EXTENTLIMIT = (v - o) * 0.5f;
                    if (_EXTENTLIMIT.Approximately(0f)) _EXTENTLIMIT = HalfLimitVALUE;
                }
                else
                {
                    if (value < o)
                    {
                        _CENTERLIMIT = value;
                        _EXTENTLIMIT = 0f;
                    }
                    else
                    {
                        _CENTERLIMIT = (value + o) * 0.5f;
                        _EXTENTLIMIT = (value - o) * 0.5f;
                    }
                }
            }
        }

        internal float _CENTERLIMIT, _EXTENTLIMIT;

        public bool UseLIMIT
        {
            get => HardLIMIT || _useLIMIT;
            set => _useLIMIT = value;
        }

        [NonSerialized]
        private bool _useLIMIT = false;

        public bool HardLIMIT = false;

        public float MAXVELOCITY, TrueMaxVELOCITY = 1f, TrueLimitVALUE = 1f;
        public float HalfLimitVALUE => TrueLimitVALUE * 0.5f;

        [NonSerialized]
        public bool LOCALINPUT = true;

        public void SetMinLimit(float value, bool ChangeValue = true)
        {
            if (ChangeValue)
                MINVALUELIMIT = value;
            /* if (IsFreeJoint && HolderJoint != null)
            {
                if (IsPlanarVALUE)
                {
                    var modify = HolderJoint.lowAngularXLimit;
                    modify.limit = value;
                    HolderJoint.lowAngularXLimit = modify;
                }
                else
                    SetLinearLimit();
            } */
        }

        public void SetMaxLimit(float value, bool ChangeValue = true)
        {
            if (ChangeValue)
                MAXVALUELIMIT = value;
            /* if (IsFreeJoint && HolderJoint != null)
            {
                if (IsPlanarVALUE)
                {
                    var modify = HolderJoint.highAngularXLimit;
                    modify.limit = value;
                    HolderJoint.highAngularXLimit = modify;
                }
                else
                    SetLinearLimit();
            } */
        }

        /*
        public void SetLinearLimit()
        {
            Vector3 min = GetPosCurve(PartCount - 1, MINVALUELIMIT), max = GetPosCurve(PartCount - 1, MAXVALUELIMIT), cen = (min + max) * 0.5f; // Do not use _CENTERLIMIT, because some animations will have different centers at different positions
            HolderJoint.anchor = transform.parent.InverseTransformPoint(transform.TransformPoint(cen));
            var ll = HolderJoint.linearLimit;
            ll.limit = _EXTENTLIMIT; // Could use (max - min).magnitude * 0.5f instead...
            HolderJoint.linearLimit = ll;
        }
        */

        /// <summary>
        /// Target value
        /// </summary>
        public float VALUE;

        /// <summary>
        /// Spring Strength
        /// </summary>
        [NonSerialized]
        public float SPRSTR;

        /// <summary>
        /// Spring Dampening
        /// </summary>
        [NonSerialized]
        public float SPRDAM;

        public void UpdateSpringForce()
        {
            /*
            if (HolderJoint != null)
            {
                Holder.SetJointDrive(SPRDAM, SPRSTR);
            }
            */
        }

        /// <summary>
        /// Current value
        /// </summary>
        [NonSerialized]
        public float PVALUE;

        [NonSerialized]
        public float VELOCITY;

        /// <summary>
        /// The big question; Is it a piston, or is it a swivel?
        /// </summary>
        public bool IsPlanarVALUE;

        public float InvPointWeightRatio = 1f;
        public float PointWeightRatio => 1 - InvPointWeightRatio;

        /// <summary>
        /// Back-push, Offset the parent rigidbody by lockjoint movement
        /// </summary>
        public bool LockJointBackPush;

        [NonSerialized]
        public MoverType moverType, oldMoverType;

        public bool CannotBeFreeJoint
        {
            get => _cannotBeFreeJoint;
            set
            {
                _cannotBeFreeJoint = value;
                if (value && IsFreeJoint) moverType = MoverType.Dynamic;
            }
        }

        private bool _cannotBeFreeJoint;

        public bool CanOnlyBeLockJoint
        {
            get => true;
            set { }
        }

        public bool IsFreeJoint => false;//moverType == MoverType.Physics;
        public bool IsBodyJoint => false;//moverType == MoverType.Dynamic;
        public bool IsLockJoint => true;//moverType == MoverType.Static;
        public bool WasFreeJoint => false;//oldMoverType == MoverType.Physics;
        public bool WasBodyJoint => false;//oldMoverType == MoverType.Dynamic;
        public bool WasLockJoint => true;//oldMoverType == MoverType.Static;

        public enum MoverType : byte
        {
            /// <summary>
            /// Lock-Joint, moves blocks using transform manipulation. The old method
            /// </summary>
            Static,

            /// <summary>
            /// Moves blocks under a separate physics body, restricted to the joint.
            /// </summary>
            Dynamic,

            /// <summary>
            /// Free-Joint, like Dynamic, but is allowed to move freely according to limits and spring forces
            /// </summary>
            Physics
        }

        [NonSerialized]
        public List<InputOperator> ProcessOperations;

        public TechAudio.SFXType SFXType
        {
            get
            {
                return this.SFX;
            }
        }

        public event Action<TechAudio.AudioTickData, FMODEvent.FMODParams> OnAudioTickUpdate;

        public float SFXVolume = 1f;
        public string SFXParam = "Rate";

        public void UpdateSFX(float Speed)
        {
            if (IsFreeJoint) // No noise allowed on Freejoint
            {
                PlaySFX(false, 0f);
                return;
            }
            Speed = Speed / TrueMaxVELOCITY * SFXVolume;
            bool on = !(Speed).Approximately(0f, 0.01f);
            PlaySFX(on, Mathf.Abs(Speed));
        }

        internal void PlaySFX(bool On, float Speed)
        {
            if (this.OnAudioTickUpdate != null)
            {
                TechAudio.AudioTickData value = new TechAudio.AudioTickData
                {
                    module = this,
                    provider = this,
                    sfxType = SFX,
                    numTriggered = (On ? 1 : 0),
                    triggerCooldown = 0f,
                    isNoteOn = On,
                    adsrTime01 = 0f,
                };
                this.OnAudioTickUpdate(value, On ? new FMODEvent.FMODParams(SFXParam, Speed) : null);
            }
        }

        public Quaternion GetRotCurve(int Index, float Position)
        {
            int Mod;
            switch (rotType)
            {
                case 0:
                    Mod = Index * 3;
                    return Quaternion.Euler(rotCurves[Mod].Evaluate(Position), rotCurves[Mod + 1].Evaluate(Position), rotCurves[Mod + 2].Evaluate(Position));

                case 1:
                    Mod = Index * 4;
                    return new Quaternion(rotCurves[Mod].Evaluate(Position), rotCurves[Mod + 1].Evaluate(Position), rotCurves[Mod + 2].Evaluate(Position), rotCurves[Mod + 3].Evaluate(Position));

                case 2:
                    Mod = Index * 4;
                    return Quaternion.AngleAxis(rotCurves[Mod + 3].Evaluate(Position), new Vector3(rotCurves[Mod].Evaluate(Position), rotCurves[Mod + 1].Evaluate(Position), rotCurves[Mod + 2].Evaluate(Position)));

                default:
                    Invalidate();
                    throw new Exception(name + ".ModuleBlockMover.GetRotCurve() : Field 'rotType' cannot be of value " + rotType + "!");
            }
        }

        public Vector3 GetPosCurve(int Index, float Position)
        {
            int Mod = Index * 3;
            return new Vector3(posCurves[Mod].Evaluate(Position), posCurves[Mod + 1].Evaluate(Position), posCurves[Mod + 2].Evaluate(Position));
        }

        internal void OnPool() //Creation
        {
            GrabbedBlocks = new List<TankBlock>();
            GrabbedBlockMovers = new List<ModuleBlockMover>();
            StarterBlocks = new List<TankBlock>();
            IgnoredBlocks = new List<TankBlock>();
            base.block.serializeEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
            base.block.serializeTextEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize)); // Test later if serializing how it is wanted has any major change on it
            ProcessOperations = new List<InputOperator>();

            parts = new Transform[PartCount];
            int offset = block.transform.childCount - PartCount;
            for (int I = 0; I < PartCount; I++)
            {
                parts[I] = block.transform.GetChild(I + offset);
            }
            relativeCenter = parts[PartCount - 1].localPosition;
            if (startblockpos.Length != 0) HolderPart.gameObject.AddComponent<ModuleBMPart>().parent = this;

            tankAttachBlockAction = new Action<TankBlock, Tank>(this.BlockAdded);
            tankDetachBlockAction = new Action<TankBlock, Tank>(this.BlockRemoved);
            block.AttachEvent.Subscribe(Attach);
            block.DetachEvent.Subscribe(Detatch);
            m_TargetAimer = gameObject.AddComponent<TargetAimer>();
            m_TargetAimer.Init(base.block, 0.5f, null);
        }

        private TargetAimer m_TargetAimer;

        internal Visible GetTarget()
        {
            if (!UpdatedTargetAimer)
            {
                m_TargetAimer.UpdateAndCanAimAtTarget();
                UpdatedTargetAimer = true;
            }
            return m_TargetAimer.Target;
        }

        private bool UpdatedTargetAimer = false;

        /* Vector3 cacheLinVel, cacheAngVel;
        bool HoldingVelocities = false; */

        private void CacheHolderTr()
        {
            /*
            if (!IsLockJoint && Holder.rbody != null)
            {
                HoldingVelocities = true;
                cacheLinVel = Holder.rbody.velocity;
                cacheAngVel = Holder.rbody.angularVelocity;
            }
            */
        }

        private void DefaultHolderTr(bool QueueRestore)
        {
            if (Holder != null)
            {
                queueRestoreHolderTr = QueueRestore;
                CacheHolderTr();
                Holder.transform.position = block.tank.trans.position;
                Holder.transform.rotation = block.tank.trans.rotation;
            }
        }

        internal static Quaternion RotateRotationByRotatedRotation(Quaternion Target,
                                                                   Quaternion Rotation,
                                                                   Quaternion RotationTweak)
        {
            Target *= RotationTweak;
            Target *= Rotation;
            return Target * Quaternion.Inverse(RotationTweak);
        }

        private void DefaultPart()
        {
            if (usePosCurves) HolderPart.localPosition = GetPosCurve(PartCount - 1, 0);
            if (useRotCurves) HolderPart.localRotation = GetRotCurve(PartCount - 1, 0);
        }

        private void RestoreHolderTr()
        {
            if (Holder != null)
            {
                //if (FREEJOINT)
                //{
                //    Holder.transform.position = cachePos;
                //    Holder.transform.rotation = cacheRot;
                //}
                //else
                //{
                Holder.transform.rotation = transform.parent.rotation; // Restore rotation to blockmover's parent
                if (IsPlanarVALUE || useRotCurves)
                {
                    //Holder.transform.rotation *= Quaternion.Euler(transform.localRotation * Vector3.up * PVALUE); // transform.localRotation * GetRotCurve(PartCount - 1, PVALUE); // Apply rotation value
                    Holder.transform.rotation = RotateRotationByRotatedRotation(Holder.transform.rotation, GetRotCurve(PartCount - 1, PVALUE), transform.localRotation);
                }
                Holder.transform.position += HolderPart.position - Holder.transform.TransformPoint(block.cachedLocalPosition + block.cachedLocalRotation * relativeCenter); // Restore position relative to holder part

                /*
                if (HoldingVelocities && Holder.rbody != null) // Will not activate if lock joint, from the check above
                {
                    Holder.rbody.velocity = cacheLinVel;
                    Holder.rbody.angularVelocity = cacheAngVel;
                    HoldingVelocities = false;
                }
                */
            }
        }

        private void CreateHolder()
        {
            if (Holder == null)
            {
                Holder = new GameObject("ClusterBody Holder").AddComponent<ClusterBody>();
                Holder.moduleBlockMover = this;
                Holder.gameObject.layer = block.tank.gameObject.layer;
                Holder.transform.parent = block.trans.parent;
                //Holder.ClusterAPBitField;
                Holder.InitializeAPCache();
            }
            block.tank.control.driveControlEvent.Subscribe(Holder.GetDriveControl);
            Holder.transform.position = transform.parent.position;
            Holder.transform.rotation = transform.parent.rotation;
            Holder.coreTank = block.tank;
            /* Holder.Dynamics = !IsLockJoint; */
            //ClusterTech.VerifyJoin(block.tank, Holder);
        }

        internal void Update()
        {
            if (UpdatedTargetAimer) UpdatedTargetAimer = false;
            if (GrabbedBlocks.Count != 0) GrabbedBlocks.Clear();
            if (block.IsAttached)
            {
                UpdateSFX(LastSentVELOCITY);
            }
        }

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="ownerBody">this.ownerBody</param>
        ///// <returns></returns>
        //private bool CheckIfOnStatic(Rigidbody ownerBody)
        //{
        //    return ownerBody.transform != transform.parent;
        //}

        internal void LateUpdate()
        {
            if (Dirty)
            {
                CleanDirty();
            }
            if (Holder != null)
            {
                if (Holder.transform.parent != transform.parent)
                {
                    Holder.transform.parent = transform.parent;
                    RestoreHolderTr();
                }
                if (queueRestoreHolderTr)
                {
                    queueRestoreHolderTr = false;
                    UpdatePartTransforms();
                    RestoreHolderTr();
                }
                /* if (IsFreeJoint && HolderJoint != null)
                {
                    float oldPVALUE = PVALUE;
                    PValueFromFreeJoint();
                    UpdatePartTransforms();

                    VerifyNetState(PVALUE - oldPVALUE, ManGameMode.inst.IsCurrentModeMultiplayer());
                }
                else */
                if (PokedByParent)
                {
                    LockJointUpdateCOM();
                    PokedByParent = false;
                }
            }
        }

        /* private void PValueFromFreeJoint()
        {
            PVALUE = IsPlanarVALUE
                ? (Vector3.SignedAngle(transform.parent.forward, Holder.transform.forward, transform.up) + 360) % 360
                : Mathf.Clamp(Vector3.Dot(transform.up, Holder.transform.position - transform.parent.position), MINVALUELIMIT, MAXVALUELIMIT);
        } */

        [NonSerialized]
        private byte playerSyncByte;

        private static byte GlobalPlayerSyncByte;

        private void VerifyNetState(float Diff, bool Net)
        {
            if (IsPlanarVALUE) Diff = ((Diff + 540f) % 360f) - 180f;
            if (!LastSentVELOCITY.Approximately(Diff, 0.005f) || playerSyncByte != GlobalPlayerSyncByte)
            {
                playerSyncByte = GlobalPlayerSyncByte;
                LastSentVELOCITY = Diff;
                if (Net)
                    SendMoverChange(new BlockMoverMessage(block, PVALUE, Diff));
            }
        }

        internal void FixedUpdate()
        {
            if (Heart != Class1.PistonHeart)
            {
                Heart = Class1.PistonHeart;
                return;
            }
            try
            {
                if (block.tank == null)
                {
                    return;
                }
                if (!Valid && startblockpos.Length != 0) return;

                bool Net = ManGameMode.inst.IsCurrentModeMultiplayer();
                bool IsControlledByNet = this.IsControlledByNet;

                float oldPVALUE = PVALUE, oldVELOCITY = VELOCITY;
                if (!IsControlledByNet)
                {
                    InputOperator topIfOperator = null;
                    int SKIP = 0;
                    foreach (var Processor in ProcessOperations)
                    {
                        if (SKIP != 0)
                        {
                            switch (Processor.m_OperationType)
                            {
                                case InputOperator.OperationType.OrThen:
                                    if (SKIP == 1 && topIfOperator.m_ResetTimer // If on the same level, and if the top operator wants to clear
                                        && Processor.ConditionMatched(this, LOCALINPUT, VALUE, VELOCITY)) // Run the check on this processor
                                    {
                                        if (Processor.m_Strength < 0) // Use timer from topIfOperator
                                        {
                                            topIfOperator.m_ResetTimer = false; // Tell operator not to clear
                                            bool met = (topIfOperator.m_InternalTimer >= Mathf.Abs(topIfOperator.m_Strength)) != (topIfOperator.m_Strength < 0); // Check if should skip
                                            SKIP = met ? 0 : 1;
                                        }
                                        else
                                            SKIP = 0;
                                    }
                                    break;

                                case InputOperator.OperationType.IfThen:
                                    SKIP++;
                                    break;

                                case InputOperator.OperationType.EndIf:
                                    SKIP--;
                                    break;

                                case InputOperator.OperationType.ElseThen:
                                    if (SKIP == 1) SKIP = 0;
                                    break;
                            }
                            Processor.LASTSTATE = SKIP == 0;
                            continue;
                        }

                        Processor.LASTSTATE = Processor.Calculate(this, LOCALINPUT, IsPlanarVALUE, ref VALUE, ref VELOCITY, ref moverType, out SKIP);

                        if (SKIP == 1) topIfOperator = Processor;
                    }

                    VELOCITY = Mathf.Clamp(VELOCITY, -MAXVELOCITY, MAXVELOCITY);
                    VALUE += VELOCITY;

                    float ofst, pofst;
                    if (IsPlanarVALUE)
                    {
                        VALUE = ((VALUE - oldPVALUE + 540) % 360) - 180 + oldPVALUE; // im' ,math
                        ofst = ((VALUE - _CENTERLIMIT + 540f) % 360f) - 180f;
                        pofst = ((oldPVALUE - _CENTERLIMIT + 540f) % 360f) - 180f;
                    }
                    else // No need to loop the value
                    {
                        VALUE = Mathf.Clamp(VALUE, 0, TrueLimitVALUE);
                        ofst = VALUE - _CENTERLIMIT;
                        pofst = oldPVALUE - _CENTERLIMIT;
                    }

                    if (UseLIMIT)
                    {
                        if (ofst < -_EXTENTLIMIT)
                        {
                            VALUE = MINVALUELIMIT;
                            ofst = -_EXTENTLIMIT;
                        }
                        else if (ofst > _EXTENTLIMIT)
                        {
                            VALUE = MAXVALUELIMIT;
                            ofst = _EXTENTLIMIT;
                        }
                    }

                    /*if (!IsFreeJoint)
                    { */
                    if (UseLIMIT)
                        PVALUE += Mathf.Clamp((ofst - pofst) * InvPointWeightRatio, -MAXVELOCITY, MAXVELOCITY); // Use ofst from before
                    else
                        PVALUE = Mathf.MoveTowardsAngle(PVALUE, VALUE * InvPointWeightRatio + PVALUE * PointWeightRatio, MAXVELOCITY); //Mathf.Clamp(VALUE * InvPointWeightRatio + PVALUE * PointWeightRatio, oldPVALUE - MAXVELOCITY, oldPVALUE + MAXVELOCITY);

                    VerifyNetState(PVALUE - oldPVALUE, Net);
                    /* } */
                }
                else // Is controlled by NET
                {
                    VALUE += LastSentVELOCITY;
                    PVALUE = VALUE;
                    moverType = MoverType.Static;
                }

                if (IsPlanarVALUE)
                {
                    PVALUE = (PVALUE + 360) % 360;
                }
                bool HolderExists = Holder != null;

                /* if (!IsFreeJoint) */
                UpdatePartTransforms();
                /* if (IsLockJoint)
                { */
                if (HolderExists)
                {
                    /* if (!WasLockJoint)
                    {
                        RestoreHolderTr();
                        Holder.SetDynamics(false);
                    }
                    else */
                    if (LastSentVELOCITY != 0f)
                    {
                        LockJointUpdateCOM();
                        for (int i = 0; i < GrabbedBlockMovers.Count; i++) GrabbedBlockMovers[i].PokedByParent = true;
                        PokedByParent = false;
                    }
                }
                /*
                }
                else // Not LockJoint
                {
                    if (WasLockJoint && HolderExists)
                    {
                        Holder.SetDynamics(true);
                    }

                    if (HolderExists && HolderJoint != null)
                    {
                        var orbody = ownerBody;
                        if (IsFreeJoint)
                        {
                            if (!WasFreeJoint) // Set the anchors
                            {
                                SetupFreeJoint();
                                UpdateSpringForce();
                            }
                            if (IsPlanarVALUE)
                                HolderJoint.targetRotation = GetRotCurve(PartCount - 1, VALUE) * Quaternion.FromToRotation(Vector3.up, HolderJoint.axis);
                            else
                                HolderJoint.targetPosition = Quaternion.FromToRotation(Vector3.up, HolderJoint.axis) * GetPosCurve(PartCount - 1, VALUE + HalfLimitVALUE);

                            if (CheckIfOnStatic(orbody))
                            {
                                UpdateRotateAnchor(Quaternion.identity);//0f);
                                HolderJoint.anchor = orbody.transform.InverseTransformPoint(HolderPart.position);
                            }
                        }
                        else // IsBodyJoint
                        {
                            if (WasFreeJoint)
                            {
                                HolderJoint.xMotion = ConfigurableJointMotion.Locked;
                                HolderJoint.angularXMotion = ConfigurableJointMotion.Locked;
                            }

                            //if (CheckIfOnStatic(orbody))
                            //{
                            //    if (IsPlanarVALUE)
                            //        UpdateRotateAnchor(PVALUE);
                            //    else
                            //        UpdateRotateAnchor(0f);
                            //    HolderJoint.anchor = orbody.transform.InverseTransformPoint(HolderPart.position);
                            //}
                            //else
                            //{
                            //    if (IsPlanarVALUE)
                            //        UpdateRotateAnchor(PVALUE);
                            //    else
                            //        HolderJoint.anchor = orbody.transform.InverseTransformPoint(HolderPart.position);
                            //}
                            UpdateRotateAnchor(HolderPart.localRotation);
                            HolderJoint.anchor = orbody.transform.InverseTransformPoint(HolderPart.position);
                        }
                    }
                }
                */
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
                if (Holder == null)
                    Console.WriteLine("Holder is NULL!");
                foreach (var descriptor in typeof(ModuleBlockMover).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
                {
                    string name = descriptor.Name;
                    object value = descriptor.GetValue(this);
                    Console.WriteLine("{0}={1}", name, value);
                }
            }
            oldMoverType = moverType;
        }

        private bool PokedByParent;

        private void LockJointUpdateCOM()
        {
            var orbody = ownerBody;
            float massChangeRatio = (Holder.rbody_mass / orbody.mass) * LastSentVELOCITY;
            var pCOM = orbody.worldCenterOfMass;
            Holder.RemoveCOMFromRigidbody(orbody);
            RestoreHolderTr();
            Holder.FixMaskedCOM(orbody.transform);
            Holder.ReturnCOMToRigidbody(orbody);

            if (LockJointBackPush && !block.tank.Anchors.Fixed) // Is not a fixated anchor
            {
                if (IsPlanarVALUE)
                {
                    orbody.rotation *= Quaternion.Euler(transform.localRotation * Vector3.up * -massChangeRatio);
                    orbody.position -= orbody.worldCenterOfMass - pCOM;
                }
                else
                {
                    orbody.position -= transform.rotation * Vector3.up * massChangeRatio;
                }
            }
        }

        /*
        internal void SetupFreeJoint()
        {
            if (IsPlanarVALUE)
            {
                HolderJoint.angularXMotion = UseLIMIT ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Free;
                UpdateRotateAnchor(Quaternion.identity);
                SetMinLimit(MINVALUELIMIT, false);
                SetMaxLimit(MAXVALUELIMIT, false);
            }
            else
            {
                HolderJoint.xMotion = ConfigurableJointMotion.Limited;
                //ClusterTech.SetOffset(block.tank, block.trans.up);
                SetLinearLimit();
            }
        } */

        private void UpdatePartTransforms()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (usePosCurves)
                    parts[i].localPosition = GetPosCurve(i, PVALUE);
                if (useRotCurves)
                    parts[i].localRotation = GetRotCurve(i, PVALUE);
            }
        }

        /// <summary>
        /// <para>HolderPart.localRotation</para>
        /// <para>Quaternion.Euler(transform.localRotation * Vector3.up * Angle)</para>
        /// </summary>
        /// <param name="localRotationOffset"></param>
        private void UpdateRotateAnchor(Quaternion localRotationOffset)//float Angle)
        {
            var rot = Holder.transform.rotation;

            Holder.transform.rotation = RotateRotationByRotatedRotation(transform.parent.rotation, localRotationOffset, transform.localRotation);//HolderPart.localRotation;//Quaternion.Euler(transform.localRotation * Vector3.up * Angle);

            HolderJoint.axis = transform.localRotation * Vector3.up;
            HolderJoint.secondaryAxis = transform.localRotation * Vector3.forward;
            Holder.transform.rotation = rot;
        }

        private bool queueRestoreHolderTr;

        internal void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
        {
            if (saving)
            {
                //if (PVALUE != 0f)
                //{
                //    DefaultHolderTr(true);
                //}

                //Print("Serializing " + transform.localPosition);
                SerialData serialData = new SerialData()
                {
                    name = UIName,
                    currentValue = PVALUE,
                    targetValue = VALUE,
                    velocity = VELOCITY,
                    maxVelocity = MAXVELOCITY,
                    jointStrength = SPRSTR,
                    jointDampen = SPRDAM,
                    onlyLocalInput = LOCALINPUT,
                    moverType = moverType,
                    lockOffsetParent = LockJointBackPush,
                    limitCenter = _CENTERLIMIT,
                    limitExtent = _EXTENTLIMIT,
                    useLimits = UseLIMIT,
                    processList = string.Join("\n", InputOperator.ProcessOperationsToStringArray(ProcessOperations))
                };
                serialData.Store(blockSpec.saveState);
            }
            else
            {
                SerialData sd = SerialData<ModuleBlockMover.SerialData>.Retrieve(blockSpec.saveState);
                if (sd != null)
                {
                    SetDirty();
                    if (!string.IsNullOrWhiteSpace(sd.name))
                        UIName = sd.name;
                    PVALUE = sd.currentValue;
                    VALUE = sd.targetValue;
                    VELOCITY = sd.velocity;
                    MAXVELOCITY = sd.maxVelocity;
                    SPRSTR = sd.jointStrength;
                    SPRDAM = sd.jointDampen;
                    LOCALINPUT = sd.onlyLocalInput;
                    moverType = sd.moverType;
                    CannotBeFreeJoint = _cannotBeFreeJoint;
                    LockJointBackPush = sd.lockOffsetParent;
                    _CENTERLIMIT = sd.limitCenter;
                    _EXTENTLIMIT = sd.limitExtent;
                    UseLIMIT = sd.useLimits;
                    InputOperator.StringArrayToProcessOperations(sd.processList, ref ProcessOperations);
                    Deserialized = true;

                    //Print("Deserializing " + transform.localPosition);
                    //Print("Positional Value: " + VALUE);
                    //Print("Got " + ProcessOperations.Count + " oper. lines from JSON:\n" + sd.processList);
                }
                else
                {
                    if (IsPlanarVALUE)
                    {
                        var ssd = SerialData<ModuleSwivel.SerialData>.Retrieve(blockSpec.saveState);
                        if (ssd != null)
                        {
                            //Console.WriteLine("FOUND PRE-OVERHAUL SWIVEL");
                            ModuleSwivel.ConvertSerialToBlockMover(ssd, this);
                        }
                    }
                    else
                    {
                        var psd = SerialData<ModulePiston.SerialData>.Retrieve(blockSpec.saveState);
                        if (psd != null)
                        {
                            //Console.WriteLine("FOUND PRE-OVERHAUL PISTON");
                            ModulePiston.ConvertSerialToBlockMover(psd, this);
                        }
                    }
                }
            }
        }

        [Serializable]
        public class SerialData : Module.SerialData<ModuleBlockMover.SerialData>
        {
            public float limitCenter, limitExtent, currentValue, targetValue, velocity, jointStrength, jointDampen;
            public MoverType moverType;
            public bool lockOffsetParent, onlyLocalInput, useLimits;
            public string processList, name;
            public float maxVelocity;
        }

        private bool Deserialized = false;

        private void OnSpawn() //Pull from Object Pool
        {
            playerSyncByte = GlobalPlayerSyncByte;
            Heart = Control_Block.Class1.PistonHeart;
            Dirty = true;
            if (Deserialized) return;

            ProcessOperations.Clear();
            if (IsPlanarVALUE)
            {
                ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.RightArrow, m_InputType = InputOperator.InputType.WhileHeld, m_InputParam = 0, m_OperationType = InputOperator.OperationType.ShiftPos, m_Strength = 1 });
                ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.LeftArrow, m_InputType = InputOperator.InputType.WhileHeld, m_InputParam = 0, m_OperationType = InputOperator.OperationType.ShiftPos, m_Strength = -1 });
                //TrueLimitVALUE = 360;
                _CENTERLIMIT = 0f;
                _EXTENTLIMIT = HalfLimitVALUE;
            }
            else
            {
                ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.Space, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = TrueLimitVALUE });
                ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.Space, m_InputType = InputOperator.InputType.OnRelease, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = 0 });
                ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.UpArrow, m_InputType = InputOperator.InputType.WhileHeld, m_InputParam = 0, m_OperationType = InputOperator.OperationType.ShiftPos, m_Strength = 0.05f });
                ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.DownArrow, m_InputType = InputOperator.InputType.WhileHeld, m_InputParam = 0, m_OperationType = InputOperator.OperationType.ShiftPos, m_Strength = -0.05f });
                _CENTERLIMIT = HalfLimitVALUE;
                _EXTENTLIMIT = HalfLimitVALUE;
                UseLIMIT = false;
            }
            moverType = MoverType.Static;
            oldMoverType = MoverType.Static;
            SPRSTR = 0;
            SPRDAM = 0;
            VALUE = 0;
            PVALUE = 0;
            VELOCITY = 0;
            MAXVELOCITY = TrueMaxVELOCITY;
            LockJointBackPush = true;
            LOCALINPUT = true;
            ResetUIName();
            //restored = true;
        }

        public string ResetUIName() => UIName = StringLookup.GetItemName(block.visible.m_ItemType);

        private void OnRecycle() //Put back to Object Pool
        {
            Deserialized = false;
        }

        internal void Detatch()
        {
            //SFXIsOn = false;
            block.tank.AttachEvent.Unsubscribe(tankAttachBlockAction);
            block.tank.DetachEvent.Unsubscribe(tankDetachBlockAction);
            block.tank.GetComponent<TechPhysicsReset>().Unsubscribe(PreResetPhysics, PostResetPhysics);
            //block.tank.ResetPhysicsEvent.Unsubscribe(tankResetPhysicsAction);
            block.tank.TechAudio.RemoveModule<ModuleBlockMover>(this);
            if (Holder != null)
            {
                //Holder.coreTank = block.tank;
                Holder = Holder.Destroy();
            }
            LastSentVELOCITY = 0f;
            PVALUE = 0f;
            UpdatePartTransforms();
            Valid = false;
            InvalidReason = "Detached";
            SetDirty();
        }

        internal void Attach()
        {
            block.tank.AttachEvent.Subscribe(tankAttachBlockAction);
            block.tank.DetachEvent.Subscribe(tankDetachBlockAction);
            block.tank.GetComponent<TechPhysicsReset>().Subscribe(PreResetPhysics, PostResetPhysics);
            //block.tank.ResetPhysicsEvent.Subscribe(tankResetPhysicsAction);
            block.tank.TechAudio.AddModule<ModuleBlockMover>(this);
            if (startblockpos.Length != 0) CreateHolder();
            SetDirty();
        }

        internal void BlockAdded(TankBlock mkblock, Tank tank)
        {
            // This may actually have no effect...
            //if (Valid && Holder != null)
            //{
            //    DefaultHolderTr(true); //Default position so no AP problems occur
            //}
            if (!Dirty)
                Print("ADD : set dirty " + block.cachedLocalPosition.ToString());
            SetDirty(); //ResetPhysics may already be called
        }

        internal void BlockRemoved(TankBlock rmblock, Tank tank)
        {
            if (block.tank != null && block.tank.blockman != null) Valid &= CanStartGetBlocks(block.tank.blockman);
            if (!Valid || (Holder != null && Holder.TryRemoveBlock(rmblock)))
            {
                if (!Dirty)
                    Print("REMOVE : Block removed from blockmover, set dirty " + block.cachedLocalPosition.ToString());
            }
            SetDirty();
        }

        internal void PreResetPhysics()
        {
            if (Holder != null)
            {
                Holder.coreTank = block.tank;
                Holder.RemoveJoint();
                DefaultPart();
                DefaultHolderTr(false);
                Print("RESET_PRE : ResetPhysics called, fixing position of  " + block.cachedLocalPosition.ToString());
            }
        }

        internal void PostResetPhysics()
        {
            if (Holder != null)
            {
                Print("RESET_POST : Cleaning holder rbody");
                /* Holder.Dynamics = !IsLockJoint; */
                Holder.ResetPhysics();

                Rigidbody orbody = null;
                if (IsLockJoint)
                {
                    orbody = ownerBody;
                    Holder.RemoveCOMFromRigidbody(orbody);
                }

                UpdatePartTransforms();
                RestoreHolderTr();
                queueRestoreHolderTr = false;

                /* if (IsLockJoint)
                { */
                Holder.FixMaskedCOM(orbody.transform);
                Holder.ReturnCOMToRigidbody(orbody);
                /* }
                else if (HolderJoint != null)
                {
                    UpdateSpringForce();
                    if (IsFreeJoint)
                    {
                        SetupFreeJoint();
                    }
                    else if (IsBodyJoint) // FreeJoint needs the anchor positions to stay where they are
                    {
                        //if (IsPlanarVALUE)
                        //{
                        //    UpdateRotateAnchor(PVALUE);
                        //}
                        //else
                        //{
                        //    HolderJoint.anchor = transform.parent.InverseTransformPoint(HolderPart.position);
                        //}
                        UpdateRotateAnchor(HolderPart.localRotation);
                        HolderJoint.anchor = transform.parent.InverseTransformPoint(HolderPart.position);
                    }
                } */
            }
        }

        private string lastdatetime = "";

        private static System.Globalization.CultureInfo enUS = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");

        private string GetDateTime(string Before, string After)
        {
            string newdatetime = DateTime.Now.ToString("T", enUS);
            if (newdatetime != lastdatetime)
            {
                lastdatetime = newdatetime;
                return Before + lastdatetime + After;
            }
            return "";
        }

        public void Print(string Message)
        {
            if (Input.GetKey(KeyCode.RightControl)) Console.WriteLine(GetDateTime("CB(", "): ") + Message);
        }

        internal void CleanDirty()
        {
            if (!Dirty || !block.IsAttached || block.tank == null || startblockpos.Length == 0)
            {
                Dirty = false;
                return;
            }
            Dirty = false;
            Print("DIRTY : Reached CleanDirty " + block.cachedLocalPosition.ToString());

            if (GrabbedBlocks.Count == 0)
            {
                Valid = StartGetBlocks();
            }
            if (!Valid)
            {
                Print("> Invalid");
                Invalidate();
                //queueRestoreHolderTr = false;
            }
            else
            {
                if (GrabbedBlocks.Count == 0)
                {
                    if (Holder != null)
                    {
                        Holder.Clear(true);
                        Holder.InitializeAPCache();
                        //! Holder = Holder.Destroy();
                        Print("> Cleaned holder, there were no blocks");
                    }
                }
                else
                {
                    bool MakeNew = Holder == null, Refill = !MakeNew && (Holder.Dirty || GrabbedBlocks.Count != Holder.blocks.Count);
                    if (Refill)
                    {
                        Print("> Clearing holder's blocks: " + (Holder.Dirty ? "mover was marked changed" : $"grabbed {GrabbedBlocks.Count} blocks, but holder had {Holder.blocks.Count}"));
                        CacheHolderTr();
                        Holder.Clear(false);
                        Holder.InitializeAPCache();
                    }
                    DefaultPart();
                    CreateHolder();
                    if (MakeNew || Refill)
                    {
                        for (int i = 0; i < GrabbedBlocks.Count; i++)
                        {
                            var b = GrabbedBlocks[i];
                            Holder.AddBlock(b, b.cachedLocalPosition, b.cachedLocalRotation);
                        }
                        Print($"> Put {Holder.blocks.Count} blocks on holder");
                    }
                    else
                        Print($"> Kept current {Holder.blocks.Count} blocks on holder");
                    //Holder.ResetPhysics(this);

                    UpdatePartTransforms();
                    RestoreHolderTr();
                    queueRestoreHolderTr = false;
                }
            }
        }

        internal void Invalidate()
        {
            if (Holder != null)
            {
                Holder.Clear(true);
                Holder.InitializeAPCache();
                //! Holder = Holder.Destroy();
            }
            PVALUE = 0f;
            UpdatePartTransforms();
        }

        internal void SetDirty()
        {
            if (!Dirty)
            {
                //Print("Piston " + base.block.cachedLocalPosition.ToString() + " is now  d i r t y");
                Dirty = true;
            }
        }

        /// <summary>
        /// Use by derived classes to determine whether or not it is plausible to continue, or to do something before blocks are grabbed
        /// </summary>
        /// <returns>Continue?</returns>
        internal virtual bool CanStartGetBlocks(BlockManager blockMan) => true;

        /// <summary>
        /// Begin recursive grab of blocks connected to the block-mover head on the main tech
        /// </summary>
        /// <param name="WatchDog">List of parents, from oldest to newest, to watch out for in the case of an impossible structure</param>
        internal bool StartGetBlocks(List<ModuleBlockMover> WatchDog = null)
        {
            Print("GRAB : Starting blockgrab for BlockMover " + block.cachedLocalPosition.ToString());

            StarterBlocks.Clear();
            IgnoredBlocks.Clear();

            var blockman = block.tank.blockman;

            if (!CanStartGetBlocks(blockman))
            {
                Print("> Unique pre-blockgrab check failed!");
                InvalidReason = "Bad setup";
                return false;
            }

            GrabbedBlocks.Clear();
            GrabbedBlockMovers.Clear();

            if (startblockpos.Length == 0)
            {
                Print("> There are no starting positions to get blocks!");
                InvalidReason = "Corrupt";
                return false;
            }

            foreach (IntVector3 sbp in startblockpos)
            {
                var Starter = blockman.GetBlockAtPosition((block.cachedLocalRotation * sbp) + block.cachedLocalPosition);
                if (Starter == null)
                {
                    continue;
                }
                if (GrabbedBlocks.Contains(Starter) || IgnoredBlocks.Contains(Starter))
                {
                    continue;
                }
                bool isAttached = false;
                foreach (var block in Starter.ConnectedBlocksByAP)
                {
                    if (block != null && block == this.block)
                    {
                        isAttached = true;
                        break;
                    }
                }
                if (isAttached)
                {
                    //Print("Starter block " + Starter.cachedLocalPosition.ToString());
                    GrabbedBlocks.Add(Starter);
                    StarterBlocks.Add(Starter);
                }
            }
            foreach (var b in StarterBlocks)
            {
                if (!CheckIfValid(b, WatchDog) || !GetBlocks(b))
                {
                    StarterBlocks.Clear();
                    GrabbedBlockMovers.Clear();
                    return false;
                }
            }
            return true;
        }

        public int GetBlocksIterationCount;

        /// <summary>
        /// Get blocks connected directly to mover-block head on the main tech, recursively
        /// </summary>
        /// <param name="Start">Block to search from</param>=
        /// <param name="WatchDog">List of parents, from oldest to newest, to watch out for in the case of an impossible structure</param>
        /// <returns>If false, the grab has failed and it is pulling back from the proccess</returns>
        internal bool GetBlocks(TankBlock Start = null, List<ModuleBlockMover> WatchDog = null)
        {
            List<TankBlock> buffer = new List<TankBlock>();
            buffer.Add(Start);
            int iteration = 0;
            do
            {
                int bC = buffer.Count;
                for (int i = 0; i < bC; i++)
                {
                    foreach (TankBlock ConnectedBlock in buffer[i].ConnectedBlocksByAP)
                    {
                        if (ConnectedBlock != null && !GrabbedBlocks.Contains(ConnectedBlock))
                        {
                            //Print("Block " + ConnectedBlock.cachedLocalPosition.ToString());
                            if (IgnoredBlocks.Contains(ConnectedBlock))
                            {
                                //Print("Ignoring block");
                                continue; // Skip ignored block
                            }
                            if (ConnectedBlock == block)
                            {
                                if (iteration == 0)
                                {
                                    continue;
                                }
                                else
                                {
                                    Print("Looped to self! Escaping blockgrab");
                                    InvalidReason = "Stuck";
                                    return false;
                                }
                            }
                            if (!CheckIfValid(ConnectedBlock, WatchDog)) return false; // Check validity. If failed, cease
                            GrabbedBlocks.Add(ConnectedBlock);
                            buffer.Add(ConnectedBlock); // Add to buffer
                        }
                    }
                }
                buffer.RemoveRange(0, bC);
                iteration++;
            }
            while (buffer.Count != 0);
            GetBlocksIterationCount += iteration;
            return true;
        }

        private bool CheckIfValid(TankBlock b, List<ModuleBlockMover> WatchDog)
        {
            ModuleBlockMover bm = b.GetComponent<ModuleBlockMover>();
            if (bm != null)
            {
                if (WatchDog != null && WatchDog.Contains(bm)) // If this block is actually a parent, take their knees and leave
                {
                    //Print("Parent encountered! Escaping blockgrab (Impossible structure)");
                    for (int p = WatchDog.IndexOf(bm) + 1; p < WatchDog.Count; p++) // Add 1 to the index, and let the encountered parent decide if it could move
                        WatchDog[p].StarterBlocks.Clear();
                    InvalidReason = "Stuck in loop";
                    return false;
                }

                if (bm.Dirty && bm.GrabbedBlocks.Count == 0) // If they didn't do their thing yet, guide them to watch out for parents
                {
                    //Print("Triggering new blockgrab for child");
                    List<ModuleBlockMover> nWD = new List<ModuleBlockMover>();
                    if (WatchDog != null) nWD.AddRange(WatchDog);
                    nWD.Add(this);
                    bm.Valid = bm.StartGetBlocks(nWD);
                    nWD.Clear();
                    if (StarterBlocks.Count == 0) // Did they take our knees
                    {
                        //Print("Impossible structure! Escaping blockgrab");
                        GrabbedBlockMovers.Clear();
                        InvalidReason = "Stuck in loop";
                        return false; // They took our knees, also leave
                    }
                }

                if (bm.Valid) // If that block got blocks, leave it alone
                {
                    //Print("Child is valid, ignore blocks of");
                    GrabbedBlockMovers.Add(bm);
                    IgnoredBlocks.AddRange(bm.StarterBlocks);
                }
            }
            if (block.tank.blockman.IsRootBlock(b))
            {
                Print("Encountered cab! Escaping blockgrab (false)");
                InvalidReason = "Can't move Cab";
                return false;
            }
            return true;
        }

        public string GetValuesAsCommentedString()
        {
            string result =
                $"#SET POSITION {PVALUE}\n" +
                $"#SET TARGET {VALUE}\n" +
                $"#SET VELOCITY {VELOCITY}\n" +
                $"#SET MAXVELOCITY {MAXVELOCITY}\n" +
                $"#SET LOCALINPUT {LOCALINPUT}\n" +
                $"#SET BACKPUSH {LockJointBackPush}\n" +
                $"#SET USELIMITS {UseLIMIT}\n" +
                $"#SET CENTERLIMIT {_CENTERLIMIT}\n" +
                $"#SET EXTENTLIMIT {_EXTENTLIMIT}";
            if (!CanOnlyBeLockJoint)
            {
                result += $"\n#SET TYPEMOVER {moverType}";
                if (!CannotBeFreeJoint)
                {
                    result +=
                    $"\n#SET STRENGTHSPRING {SPRSTR}" +
                    $"\n#SET DAMPENSPRING {SPRDAM}";
                }
            }
            return result;
        }

        public void SetValuesFromCommentedString(string values)
        {
            foreach (string s in values.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)) // Split by separator
            {
                if (s[0] != '#') continue;

                string line = s.ToUpper();
                if (!line.StartsWith("#SET ")) continue;

                int index = line.IndexOf(' ', 8) + 1;
                if (index == 0) continue;

                string value;
                int index2 = line.IndexOf(' ', index);
                if (index2 == -1)
                    value = line.Substring(index);
                else
                    value = line.Substring(index, index2 - index);

                try
                {
                    switch (s.Substring(5, 3))
                    {
                        //    POSITION
                        case "POS": PVALUE = float.Parse(value); break;
                        //    TARGET
                        case "TAR": VALUE = float.Parse(value); break;
                        //    VELOCITY
                        case "VEL": VELOCITY = float.Parse(value); break;
                        //    MAXVELOCITY
                        case "MAX": MAXVELOCITY = float.Parse(value); break;
                        //    STRENGTHSPRING
                        case "STR": SPRSTR = float.Parse(value); break;
                        //    DAMPENSPRING
                        case "DAM": SPRDAM = float.Parse(value); break;
                        //    LOCALINPUT
                        case "LOC": LOCALINPUT = bool.Parse(value); break;
                        //    TYPE
                        case "TYP": moverType = (MoverType)Enum.Parse(typeof(MoverType), value, true); break;
                        //    BACKPUSH
                        case "BAC": LockJointBackPush = bool.Parse(value); break;
                        //    USELIMIT
                        case "USE": _useLIMIT = bool.Parse(value); break;
                        //    CENTERLIMIT
                        case "CEN": _CENTERLIMIT = float.Parse(value); break;
                        //    EXTENTLIMIT
                        case "EXT": _EXTENTLIMIT = float.Parse(value); break;

                        default: Console.WriteLine("SetValuesFromCommentedString : Unknown line " + s); break;
                    }
                }
                catch (Exception E)
                {
                    Console.WriteLine("SetValuesFromCommentedString : Failed to parse line " + s + " (" + value + ")\n" + E.Message);
                }
            }
            SetDirty();
        }

        public const TTMsgType NetMsgMoverID = (TTMsgType)32115;
        internal static bool IsNetworkingInitiated = false;

        public static void InitiateNetworking()
        {
            if (IsNetworkingInitiated)
            {
                throw new Exception("Something tried to initiate the networking component of BlockMovers twice!\n" + System.Reflection.Assembly.GetCallingAssembly().FullName);
            }
            IsNetworkingInitiated = true;
            Nuterra.NetHandler.Subscribe<BlockMoverMessage>(NetMsgMoverID, ReceiveMoverChange, PromptNewMoverChange);
            Nuterra.NetHandler.OnPlayerJoined += SyncPlayer;

            #warning USE TECH EVENTS TO SYNCHRONIZE
            //ManNetTechs.inst.event
        }

        private static void SyncPlayer(NetPlayer player)
        {
            Console.WriteLine("A PLAYER HAS JOINED : " + player.name + "\nIncrementing GlobalSyncByte");
            GlobalPlayerSyncByte++;
            //if (ManNetwork.IsHost)
            //{
            //    Console.WriteLine("A PLAYER HAS JOINED : " + player.name);
            //    var NetTechs = ManNetwork.inst.GetAllPlayerTechs();
            //    foreach (Tank netTech in NetTechs)
            //    {
            //        try
            //        {
            //            if (netTech == null) continue;
            //            Console.WriteLine("Iterating through tech " + netTech.name);
            //            foreach (var mover in netTech.GetComponentsInChildren<ModuleBlockMover>())
            //            {
            //                Console.WriteLine("Sending " + mover.name + mover.transform.localPosition.ToString());
            //                Nuterra.NetHandler.BroadcastMessageToClient(NetMsgMoverID, new BlockMoverMessage(mover.block, mover.PVALUE, mover.LastSentVELOCITY), player.connectionToClient.connectionId);
            //            }
            //        }
            //        catch { }
            //    }
            //}
        }

        public static void SendMoverChange(BlockMoverMessage message)
        {
            if (ManNetwork.IsHost)
            {
                Nuterra.NetHandler.BroadcastMessageToAllExcept(NetMsgMoverID, message, true);
                return;
            }
            Nuterra.NetHandler.BroadcastMessageToServer(NetMsgMoverID, message);
        }

        private static void PromptNewMoverChange(BlockMoverMessage obj, NetworkMessage netmsg)
        {
            Nuterra.NetHandler.BroadcastMessageToAllExcept(NetMsgMoverID, obj, true, netmsg.conn.connectionId);
            ReceiveMoverChange(obj, netmsg);
        }

        private static void ReceiveMoverChange(BlockMoverMessage obj, NetworkMessage netmsg) => obj.block.GetComponent<ModuleBlockMover>().ReceiveFromNet(obj);

        private float LastSentVELOCITY = 0f;

        public void ReceiveFromNet(BlockMoverMessage data)
        {
            VALUE = data.value;
            LastSentVELOCITY = data.velocity;
            //Console.WriteLine($"Received new blockmover change: {block.cachedLocalPosition} set to {VALUE} with velocity {LastSentVELOCITY}");
        }

        public class BlockMoverMessage : UnityEngine.Networking.MessageBase
        {
            public BlockMoverMessage()
            {
            }

            public BlockMoverMessage(TankBlock Block, float Value, float Velocity)
            {
                block = Block;
                tank = Block.tank;
                value = Value;
                velocity = Velocity;
            }

            public override void Deserialize(UnityEngine.Networking.NetworkReader reader)
            {
                tank = ClientScene.FindLocalObject(new NetworkInstanceId(reader.ReadUInt32())).GetComponent<Tank>();
                block = tank.blockman.GetBlockWithID(reader.ReadUInt32());
                value = reader.ReadSingle();
                velocity = reader.ReadSingle();
            }

            public override void Serialize(UnityEngine.Networking.NetworkWriter writer)
            {
                writer.Write(tank.netTech.netId.Value);
                writer.Write(block.blockPoolID);
                writer.Write(value);
                writer.Write(velocity);
            }

            public TankBlock block;
            public Tank tank;

            public float value, velocity;
        }
    }
}