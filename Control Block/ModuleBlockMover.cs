using Control_Block;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

internal class ModuleBlockMover : Module, TechAudio.IModuleAudioProvider
{
    public TechAudio.SFXType SFX;
    /// <summary>
    /// Body for holding blocks
    /// </summary>
    public ClusterBody Holder;
    /// <summary>
    /// Joint of which binds the two techs together
    /// </summary>
    public UnityEngine.ConfigurableJoint HolderJoint => Holder ? Holder.Joint : null;
    /// <summary>
    /// Preset number for quantity of models on block
    /// </summary>
    public int PartCount = 1;
    /// <summary>
    /// Models on block for animation purposes
    /// </summary>
    public Transform[] parts;
    /// <summary>
    /// The part who'se transform is responsible for the dummy tank's positioning
    /// </summary>
    public Transform HolderPart => parts[PartCount - 1];
    public Vector3 relativeCenter;
    /// <summary>
    /// WorldTreadmill interference security measure
    /// </summary>
    public bool Heart;
    /// <summary>
    /// Animation curves for determining position
    /// </summary>
    public AnimationCurve[] posCurves;
    public bool useRotCurves = false, usePosCurves = false;
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
    public bool Dirty = true;
    public bool Valid = true;
    /// <summary>
    /// Block-cache for blocks found after re-evaluation
    /// </summary>
    public List<TankBlock> GrabbedBlocks;
    public List<ModuleBlockMover> GrabbedBlockMovers;
    public List<TankBlock> StarterBlocks;
    public List<TankBlock> IgnoredBlocks;
    /// <summary>
    /// Event-cache for attaching and detaching to a tank
    /// </summary>
    public Action<TankBlock, Tank> tankAttachBlockAction, tankDetachBlockAction;
    //public Action tankResetPhysicsAction;

        /// <summary>
        /// Get the immediate rigidbody of the parent of this block. Tank if root, Cluster if grabbed.
        /// </summary>
    public Rigidbody ownerBody => transform.parent.GetComponent<Rigidbody>();
    private bool IsControlledByNet => ManGameMode.inst.IsCurrentModeMultiplayer() && block.tank != ManNetwork.inst.MyPlayer.CurTech.tech;

    internal float MINVALUELIMIT = 0f, MAXVALUELIMIT = 1f, MAXVELOCITY;
    public float TrueMaxVELOCITY = 1f, TrueLimitVALUE = 1f;
    public void SetMinValueLimit(float value)
    {
        if (FREEJOINT && HolderJoint != null)
        {
            if (IsPlanarVALUE)
            {
                var modify = HolderJoint.lowAngularXLimit;
                modify.limit = value;
                HolderJoint.lowAngularXLimit = modify;
            }
            else
            {
                SetLinearLimit();
            }
        }
        MINVALUELIMIT = value;
    }
    public void SetMaxValueLimit(float value)
    {
        if (FREEJOINT && HolderJoint != null)
        {
            if (IsPlanarVALUE)
            {
                var modify = HolderJoint.highAngularXLimit;
                modify.limit = value;
                HolderJoint.highAngularXLimit = modify;
            }
            else
            {
                SetLinearLimit();
            }
        }
        MAXVALUELIMIT = value;
    }
    public void SetLinearLimit()
    {
        var min = GetPosCurve(PartCount - 1, MINVALUELIMIT);
        var max = GetPosCurve(PartCount - 1, MAXVALUELIMIT);
        HolderJoint.anchor = transform.parent.InverseTransformPoint(transform.TransformPoint((max + min) * 0.5f));
        var ll = HolderJoint.linearLimit;
        ll.limit = Mathf.Abs((max - min).y) * 0.5f;
        HolderJoint.linearLimit = ll;
    }

    /// <summary>
    /// Target value
    /// </summary>
    public float VALUE;
    /// <summary>
    /// Spring Strength
    /// </summary>
    public float SPRSTR;
    /// <summary>
    /// Spring Dampening
    /// </summary>
    public float SPRDAM;
    public void UpdateSpringForce()
    {
        if (HolderJoint != null)
        {
            //HolderJoint.slerpDrive = new JointDrive { positionDamper = SPRDAM, positionSpring = SPRSTR, maximumForce = ClusterBody.MaxSpringForce };
            HolderJoint.angularXDrive = new JointDrive { positionDamper = SPRDAM, positionSpring = SPRSTR, maximumForce = ClusterBody.MaxSpringForce, mode = JointDriveMode.Position };
            HolderJoint.xDrive = new JointDrive { positionDamper = SPRDAM, positionSpring = SPRSTR, maximumForce = ClusterBody.MaxSpringForce, mode = JointDriveMode.Position };
        }
    }
    /// <summary>
    /// Current value
    /// </summary>
    public float PVALUE;
    public float VELOCITY;
    public bool IsPlanarVALUE, FREEJOINT, oldFREEJOINT, LOCKJOINT, oldLOCKJOINT;

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
        Speed = Speed / TrueMaxVELOCITY * SFXVolume;
        bool on = !(Speed).Approximately(0f, 0.05f);
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
        int Mod = Index * 3;
        return Quaternion.Euler(rotCurves[Mod].Evaluate(Position), rotCurves[Mod + 1].Evaluate(Position), rotCurves[Mod + 2].Evaluate(Position));
    }

    public Vector3 GetPosCurve(int Index, float Position)
    {
        int Mod = Index * 3;
        return new Vector3(posCurves[Mod].Evaluate(Position), posCurves[Mod + 1].Evaluate(Position), posCurves[Mod + 2].Evaluate(Position));
    }

    private void OnPool() //Creation
    {
        GrabbedBlocks = new List<TankBlock>();
        GrabbedBlockMovers = new List<ModuleBlockMover>();
        StarterBlocks = new List<TankBlock>();
        IgnoredBlocks = new List<TankBlock>();
        base.block.serializeEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
        //base.block.serializeTextEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize)); // Uncertain about this bit, need to study code
        ProcessOperations = new List<InputOperator>();
        //HolderDirty = new List<TankBlock>();

        parts = new Transform[PartCount];
        int offset = block.transform.childCount - PartCount;
        for (int I = 0; I < PartCount; I++)
        {
            parts[I] = block.transform.GetChild(I + offset);
        }
        relativeCenter = parts[PartCount - 1].localPosition;

        tankAttachBlockAction = new Action<TankBlock, Tank>(this.BlockAdded);
        tankDetachBlockAction = new Action<TankBlock, Tank>(this.BlockRemoved);
        //tankResetPhysicsAction = new Action(this.ResetPhysics);
        block.AttachEvent.Subscribe(Attach);
        block.DetachEvent.Subscribe(Detatch);
    }

    Vector3 cachePos, cacheLinVel, cacheAngVel;
    Quaternion cacheRot;
    bool HoldingVelocities = false;
    private void CacheHolderTr()
    {
        cachePos = Holder.transform.position;
        cacheRot = Holder.transform.rotation;
        if ((!LOCKJOINT || FREEJOINT) && Holder.rbody != null)
        {
            HoldingVelocities = true;
            cacheLinVel = Holder.rbody.velocity;
            cacheAngVel = Holder.rbody.angularVelocity;
        }
    }
    private void DefaultHolderTr(bool QueueRestore)
    {
        if (Holder != null)
        {
            queueRestoreHolderTr = QueueRestore;
            CacheHolderTr();
            //Holder.transform.position = transform.parent.position;
            //Holder.transform.rotation = transform.parent.rotation;
            Holder.transform.position = block.tank.trans.position;
            Holder.transform.rotation = block.tank.trans.rotation;
        }
    }

    private void DefaultPart()
    {
        //cachePos1 = HolderPart.localPosition;
        //cacheRot1 = HolderPart.localRotation;
        if (usePosCurves) HolderPart.localPosition = GetPosCurve(PartCount - 1, 0);
        if (useRotCurves) HolderPart.localRotation = GetRotCurve(PartCount - 1, 0);
    }

    //private void RestorePart()
    //{
    //    HolderPart.localPosition = cachePos1;
    //    HolderPart.localRotation = cacheRot1;
    //}

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
            if (IsPlanarVALUE)
                Holder.transform.rotation *= Quaternion.Euler(transform.localRotation * Vector3.up * PVALUE); // Apply rotation value
            Holder.transform.position += HolderPart.position - Holder.transform.TransformPoint(block.cachedLocalPosition + block.cachedLocalRotation * relativeCenter); // Restore position relative to holder part
            if (HoldingVelocities && Holder.rbody != null)
            {
                Holder.rbody.velocity = cacheLinVel;
                Holder.rbody.angularVelocity = cacheAngVel;
                HoldingVelocities = false;
            }
        }
    }

    private void CreateHolder()
    {
        if (Holder == null)
        {
            Holder = new GameObject("ClusterBody Holder").AddComponent<ClusterBody>();
            Holder.moduleBlockMover = this;
            //Holder.dragSphere = Holder.gameObject.AddComponent<SphereCollider>();
            Holder.blocks = new List<TankBlock>();
            Holder.gameObject.layer = block.tank.gameObject.layer;
            Holder.transform.parent = block.trans.parent;
            //Holder.transform.parent = LOCKJOINT ? transform.parent : block.tank.trans.parent;
        }
        Holder.transform.position = transform.parent.position;
        Holder.transform.rotation = transform.parent.rotation;
        Holder.coreTank = block.tank;
        Holder.Dynamics = !LOCKJOINT || FREEJOINT;
        //Holder.rbody.isKinematic = false;
        //ClusterTech.VerifyJoin(block.tank, Holder);
    }

    private void Update()
    {
        if (GrabbedBlocks.Count != 0) GrabbedBlocks.Clear();
        if (block.IsAttached)
        {
            UpdateSFX(LastSentVELOCITY);
        }
    }

    private void LateUpdate()
    {
        if (Dirty)
        {
            CleanDirty();
        }
        if (Holder != null)
        {
            if (queueRestoreHolderTr)
            {
                queueRestoreHolderTr = false;
                UpdatePartTransforms();
                RestoreHolderTr();
            }
            if (FREEJOINT)
            {
                if (IsPlanarVALUE)
                {
                    PVALUE = Vector3.SignedAngle(transform.parent.forward, Holder.transform.forward, transform.up) % 360;
                    HolderJoint.targetAngularVelocity = new Vector3(0f, VALUE - PVALUE, 0f);
                }
                else
                {
                    PVALUE = Mathf.Clamp(Vector3.Project(Holder.transform.position - transform.parent.position, transform.up).magnitude, MINVALUELIMIT, MAXVALUELIMIT);
                    HolderJoint.targetVelocity = new Vector3(0f, VALUE - PVALUE, 0f);
                }
                UpdatePartTransforms();
                //PVALUE = VALUE;
            }
        }
    }

    private void FixedUpdate()
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
            if (!Valid) return;

            bool Net = ManGameMode.inst.IsCurrentModeMultiplayer();
            bool IsControlledByNet = this.IsControlledByNet;

            float oldVALUE = PVALUE, oldVELOCITY = VELOCITY;
            int SKIP = 0;
            if (!IsControlledByNet)
            {
                foreach (var Processor in ProcessOperations)
                {
                    if (SKIP != 0)
                    {
                        if (Processor.m_OperationType == InputOperator.OperationType.ConditionIfThen) SKIP++;
                        else if (Processor.m_OperationType == InputOperator.OperationType.ConditionEndIf) SKIP--;
                        Processor.LASTSTATE = SKIP == 0;
                        continue;
                    }
                    Processor.LASTSTATE = Processor.Calculate(block, IsPlanarVALUE, ref VALUE, ref VELOCITY, ref FREEJOINT, ref LOCKJOINT, out SKIP);
                }
                VELOCITY = Mathf.Clamp(VELOCITY, -MAXVELOCITY, MAXVELOCITY);
                VALUE += VELOCITY;
                if (IsPlanarVALUE)
                {
                    VALUE = ((VALUE - oldVALUE + 540) % 360) - 180 + oldVALUE;
                }
                else
                {
                    VALUE = Mathf.Clamp(VALUE, 0, TrueLimitVALUE);
                }
                if (MINVALUELIMIT > 0 || MAXVALUELIMIT < TrueLimitVALUE)
                {
                    VALUE = Mathf.Clamp(VALUE, MINVALUELIMIT, MAXVALUELIMIT);
                }
                PVALUE = Mathf.Clamp(VALUE, oldVALUE - MAXVELOCITY, oldVALUE + MAXVELOCITY); //Fix to prevent passing value limits, if problem
                float DIFF = PVALUE - oldVALUE;
                if (!LastSentVELOCITY.Approximately(DIFF, 0.005f))
                {
                    LastSentVELOCITY = DIFF;
                    if (Net)
                        SendMoverChange(new BlockMoverMessage(block, PVALUE, DIFF));
                }
            }
            else // Is controlled by NET
            {
                VALUE += LastSentVELOCITY;
                PVALUE = VALUE;
                LOCKJOINT = true;
            }

            if (IsPlanarVALUE)
            {
                //VALUE = Mathf.Repeat(VALUE, 360);
                PVALUE = Mathf.Repeat(PVALUE, 360);
            }
            bool HolderExists = Holder != null;
            if (LOCKJOINT && !FREEJOINT)
            {
                if (HolderExists)
                {
                    if (!oldLOCKJOINT)
                    {
                        UpdatePartTransforms();
                        RestoreHolderTr();
                        Holder.SetDynamics(false);
                    }
                    else if (LastSentVELOCITY != 0f)
                    {
                        if (IsPlanarVALUE)
                        {

                        }
                        else
                        {
                            var orbody = ownerBody;
                            float th = (Holder.rbody_mass / orbody.mass);
                            var thing = LastSentVELOCITY * th;
                            orbody.position -= block.transform.rotation * Vector3.up * thing;
                        }

                        RestoreHolderTr();
                    }
                }
            }
            else
            {
                if (oldLOCKJOINT && HolderExists)
                {
                    Holder.SetDynamics(true);
                }

                if (!FREEJOINT)
                    UpdatePartTransforms();

                if (HolderExists && HolderJoint != null)
                {
                    if (FREEJOINT)
                    {
                        if (!oldFREEJOINT)
                        {
                            if (IsPlanarVALUE)
                            {
                                HolderJoint.angularXMotion = ConfigurableJointMotion.Free;
                                UpdateRotateAnchor(0f);
                                SetMinValueLimit(MINVALUELIMIT);
                                SetMaxValueLimit(MAXVALUELIMIT);
                            }
                            else
                            {
                                HolderJoint.xMotion = ConfigurableJointMotion.Limited;
                                //ClusterTech.SetOffset(block.tank, block.trans.up);
                                SetLinearLimit();
                            }
                            UpdateSpringForce();
                        }
                        if (IsPlanarVALUE)
                            HolderJoint.targetRotation = GetRotCurve(PartCount - 1, VALUE);
                        else
                            HolderJoint.targetPosition = GetPosCurve(PartCount - 1, VALUE);
                    }
                    else
                    {
                        if (oldFREEJOINT)
                        {
                            HolderJoint.xMotion = ConfigurableJointMotion.Locked;
                            HolderJoint.angularXMotion = ConfigurableJointMotion.Locked;
                            //VALUE = PVALUE;
                        }
                        if (IsPlanarVALUE)
                            UpdateRotateAnchor(PVALUE);
                        else
                            HolderJoint.anchor = transform.parent.InverseTransformPoint(HolderPart.position);
                    }
                }
            }
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
        oldFREEJOINT = FREEJOINT;
        oldLOCKJOINT = LOCKJOINT && !FREEJOINT;
    }

    void UpdatePartTransforms()
    {
        for (int i = 0; i < parts.Length; i++)
        {
            if (usePosCurves)
                parts[i].localPosition = GetPosCurve(i, PVALUE);
            if (useRotCurves)
                parts[i].localRotation = GetRotCurve(i, PVALUE);
        }
    }

    void UpdateRotateAnchor(float Angle)
    {
        var rot = Holder.transform.rotation;

        Holder.transform.rotation = transform.parent.rotation * Quaternion.Euler(transform.localRotation * Vector3.up * Angle);

        HolderJoint.axis = transform.localRotation * Vector3.up;
        HolderJoint.secondaryAxis = transform.localRotation * Vector3.forward;
        Holder.transform.rotation = rot;
    }

    bool queueRestoreHolderTr;

    internal void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
    {
       if (saving)
       {
            if (PVALUE != 0f)
            {
                DefaultHolderTr(true);
                Print("Serializing");
            }

            SerialData serialData = new SerialData()
            {
                minValueLimit = MINVALUELIMIT,
                maxValueLimit = MAXVALUELIMIT,
                currentValue = PVALUE,
                targetValue = VALUE,
                velocity = VELOCITY,
                maxVelocity = MAXVELOCITY,
                jointStrength = SPRSTR,
                jointDampen = SPRDAM,
                freeJoint = FREEJOINT,
                lockJoint = LOCKJOINT,
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
                MINVALUELIMIT = sd.minValueLimit;
                MAXVALUELIMIT = sd.maxValueLimit;
                PVALUE = sd.currentValue;
                VALUE = sd.targetValue;
                VELOCITY = sd.velocity;
                MAXVELOCITY = sd.maxVelocity;
                SPRSTR = sd.jointStrength;
                SPRDAM = sd.jointDampen;
                FREEJOINT = sd.freeJoint;
                LOCKJOINT = sd.lockJoint;
                InputOperator.StringArrayToProcessOperations(sd.processList, ref ProcessOperations);
           }
       }
    }


    [Serializable]
    public class SerialData : Module.SerialData<ModuleBlockMover.SerialData>
    {
        public float minValueLimit, maxValueLimit, currentValue, targetValue, velocity, jointStrength, jointDampen;
        public bool freeJoint, lockJoint;
        public string processList;
        public float maxVelocity;
    }

    private void OnSpawn() //Pull from Object Pool
    {
        ProcessOperations.Clear();
        if (IsPlanarVALUE)
        {
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.RightArrow, m_InputType = InputOperator.InputType.WhileHeld, m_InputParam = 0, m_OperationType = InputOperator.OperationType.ShiftPos, m_Strength = 1 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.LeftArrow, m_InputType = InputOperator.InputType.WhileHeld, m_InputParam = 0, m_OperationType = InputOperator.OperationType.ShiftPos, m_Strength = -1 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.I, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = 0 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.J, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = 270 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.K, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = 180 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.L, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = 90 });

            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.T, m_InputType = InputOperator.InputType.Toggle, m_InputParam = -1, m_OperationType = InputOperator.OperationType.ConditionIfThen, m_Strength = 3 });
            ProcessOperations.Add(new InputOperator() { m_InputType = InputOperator.InputType.PlayerTechIsNear, m_InputParam = -10, m_OperationType = InputOperator.OperationType.PlayerPoint, m_Strength = 1 });
            ProcessOperations.Add(new InputOperator() { m_InputType = InputOperator.InputType.PlayerTechIsNear, m_InputParam = 10, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = 0 });
            ProcessOperations.Add(new InputOperator() { m_InputType = InputOperator.InputType.AlwaysOn, m_OperationType = InputOperator.OperationType.ConditionEndIf });

            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.N, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.FreeJoint, m_Strength = 1 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.N, m_InputType = InputOperator.InputType.OnRelease, m_InputParam = 0, m_OperationType = InputOperator.OperationType.FreeJoint, m_Strength = -1 });
            TrueLimitVALUE = 360;
        }
        else
        {
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.Space, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = TrueLimitVALUE });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.Space, m_InputType = InputOperator.InputType.OnRelease, m_InputParam = 0, m_OperationType = InputOperator.OperationType.SetPos, m_Strength = 0 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.UpArrow, m_InputType = InputOperator.InputType.WhileHeld, m_InputParam = 0, m_OperationType = InputOperator.OperationType.ShiftPos, m_Strength = 0.05f });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.DownArrow, m_InputType = InputOperator.InputType.WhileHeld, m_InputParam = 0, m_OperationType = InputOperator.OperationType.ShiftPos, m_Strength = -0.05f });

            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.N, m_InputType = InputOperator.InputType.OnPress, m_InputParam = 0, m_OperationType = InputOperator.OperationType.FreeJoint, m_Strength = 1 });
            ProcessOperations.Add(new InputOperator() { m_InputKey = KeyCode.N, m_InputType = InputOperator.InputType.OnRelease, m_InputParam = 0, m_OperationType = InputOperator.OperationType.FreeJoint, m_Strength = -1 });
        }
        MINVALUELIMIT = 0;
        MAXVALUELIMIT = TrueLimitVALUE;
        FREEJOINT = false;
        oldFREEJOINT = false;
        LOCKJOINT = false;
        oldLOCKJOINT = false;
        SPRSTR = 0;
        SPRDAM = 0;
        VALUE = 0;
        PVALUE = 0;
        VELOCITY = 0;
        Heart = Control_Block.Class1.PistonHeart;
        Dirty = true;
        //restored = true;
    }

    private void OnRecycle() //Put back to Object Pool
    {

    }

    internal void Detatch()
    {
        //SFXIsOn = false;
        block.tank.AttachEvent.Unsubscribe(tankAttachBlockAction);
        block.tank.DetachEvent.Unsubscribe(tankDetachBlockAction);
        //block.tank.ResetPhysicsEvent.Unsubscribe(tankResetPhysicsAction);
        block.tank.TechAudio.RemoveModule<ModuleBlockMover>(this);
        if (Holder != null)
        {
            Holder.coreTank = block.tank;
            Holder = Holder.Destroy();
        }
        LastSentVELOCITY = 0f;
        PVALUE = 0f;
        UpdatePartTransforms();
        Valid = false;
    }

    internal void Attach()
    {
        block.tank.AttachEvent.Subscribe(tankAttachBlockAction);
        block.tank.DetachEvent.Subscribe(tankDetachBlockAction);
        //block.tank.ResetPhysicsEvent.Subscribe(tankResetPhysicsAction);
        block.tank.TechAudio.AddModule<ModuleBlockMover>(this);
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
        else
        {
            if (!Dirty)
                Print("REMOVE : set dirty " + block.cachedLocalPosition.ToString());
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
            Holder.Dynamics = !LOCKJOINT;
            Holder.ResetPhysics();

            UpdatePartTransforms();
            RestoreHolderTr();
            queueRestoreHolderTr = false;

            UpdateSpringForce();
            if (!LOCKJOINT)
            {
                //Holder.SetDynamics(false);
            //}
            //else
            //{
                oldFREEJOINT = false;
                if (!FREEJOINT && !LOCKJOINT) // FreeJoint needs the anchor positions to stay where they are
                {
                    if (IsPlanarVALUE)
                    {
                        UpdateRotateAnchor(PVALUE);
                    }
                    else
                    {
                        HolderJoint.anchor = transform.parent.InverseTransformPoint(HolderPart.position);
                    }
                }
            }
            //else
            //{
            //    if (IsPlanarVALUE)
            //    {
            //        SetMinValueLimit(MINVALUELIMIT);
            //        SetMaxValueLimit(MAXVALUELIMIT);
            //    }
            //    else
            //    {
            //        SetLinearLimit();
            //    }
            //}
        }
    }
    private string lastdatetime = "";

    private string GetDateTime(string Before, string After)
    {
        string newdatetime = DateTime.Now.ToString("T", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
        if (newdatetime != lastdatetime)
        {
            lastdatetime = newdatetime;
            return Before + lastdatetime + After;
        }
        return "";
    }

    public void Print(string Message)
    {
        Console.WriteLine(GetDateTime("CB(", "): ") + Message);
    }

    internal void CleanDirty()
    {
        if (!Dirty || !block.IsAttached || block.tank == null)
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
                    Holder = Holder.Destroy();
                    Print("> Purged holder, there were no blocks");
                }
            }
            else
            {
                bool MakeNew = Holder == null, Refill = !MakeNew && (Holder.Dirty || GrabbedBlocks.Count != Holder.blocks.Count);
                if (Refill)
                {
                    Print("> Clearing holder's blocks: " + (Holder.Dirty ? "mover was marked changed" : $"grabbed {GrabbedBlocks.Count} blocks, but holder had {Holder.blocks.Count}"));
                    Holder.Clear(false);
                    //Holder = Holder.Destroy();
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
            Holder = Holder.Destroy();
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
            return false;
        }

        GrabbedBlocks.Clear();
        GrabbedBlockMovers.Clear();

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
    /// <param name="Start">Block to search from</param>
    /// <param name="IsStarter">For recursive use, determines if crossing mover-block is illegal after a step</param>
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
                                //Print("Looped to self! Escaping blockgrab");
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

    bool CheckIfValid(TankBlock b, List<ModuleBlockMover> WatchDog)
    {
        ModuleBlockMover bm = b.GetComponent<ModuleBlockMover>();
        if (bm != null)
        {
            if (WatchDog != null && WatchDog.Contains(bm)) // If this block is actually a parent, take their knees and leave the scene
            {
                //Print("Parent encountered! Escaping blockgrab (Impossible structure)");
                for (int p = WatchDog.IndexOf(bm); p < WatchDog.Count; p++)
                    WatchDog[p].StarterBlocks.Clear();
                return false;
            }

            if (bm.Dirty && bm.GrabbedBlocks.Count == 0) // If they didn't do their thing yet guide them to watch out for parents
            {
                //Print("Triggering new blockgrab for child");
                List<ModuleBlockMover> nWD = new List<ModuleBlockMover>();
                if (WatchDog != null) nWD.AddRange(WatchDog);
                nWD.Add(this);
                bm.Valid = bm.StartGetBlocks(nWD);
                nWD.Clear();
                if (StarterBlocks.Count == 0)
                {
                    //Print("Impossible structure! Escaping blockgrab");
                    GrabbedBlockMovers.Clear();
                    return false; // They took our knees, also leave
                }
            }

            if (bm.Valid) // If that block did a good job leave its harvest alone
            {
                //Print("Child is valid, ignore blocks of");
                GrabbedBlockMovers.Add(bm);
                IgnoredBlocks.AddRange(bm.StarterBlocks);
            }
        }
        if (block.tank.blockman.IsRootBlock(b))
        {
            //Print("Encountered cab! Escaping blockgrab (false)");
            return false;
        }
        return true;
    }

    public const TTMsgType NetMsgMoverID = (TTMsgType)32115;
    internal static bool IsNetworkingInitiated = false;

#warning Disable "free-joint" when networked?
    public static void InitiateNetworking()
    {
        if (IsNetworkingInitiated)
        {
            throw new Exception("Something tried to initiate the networking component of BlockMovers twice!\n" + System.Reflection.Assembly.GetCallingAssembly().FullName);
        }
        IsNetworkingInitiated = true;
        Nuterra.NetHandler.Subscribe<BlockMoverMessage>(NetMsgMoverID, ReceiveMoverChange, PromptNewMoverChange);
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

    float LastSentVELOCITY = 0f;

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