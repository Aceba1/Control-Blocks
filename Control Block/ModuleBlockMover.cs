using Control_Block;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

abstract class ModuleBlockMover : Module, TechAudio.IModuleAudioProvider
{
    public TechAudio.SFXType SFX;
    public bool UpdateCOM = false;
    public bool BreakOnCab = false;
    public Transform LoadCOM;
    public Vector3 CacheCOM;
    public Transform holder;
    public int PartCount = 1;
    public bool Heart;
    public bool useVectorsForCurves = false;
    public AnimationCurve[] curves;
    public bool useRotCurves = false;
    public AnimationCurve[] rotCurves;
    public AnimationCurve BlockCurve => curves[curves.Length - 1];
    public AnimationCurve BlockRotCurve => rotCurves[rotCurves.Length - 1];
    public Transform[] parts;
    public IntVector3[] startblockpos;
    public int MaximumBlockPush = 64;
    public int CurrentCellPush { get; private set; } = 0;
    public float MassPushing { get; private set; } = 0f;
    public bool Dirty = true;
    public bool CanMove { get; internal set; } = false;
    public Dictionary<TankBlock, BlockDat> GrabbedBlocks;
    public Action<TankBlock, Tank> a_action, d_action;

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
        bool on = !(Speed * SFXVolume).Approximately(0f, 0.1f);
        PlaySFX(on, Mathf.Abs(Speed) * SFXVolume);
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
        return new Vector3(curves[Mod].Evaluate(Position), curves[Mod + 1].Evaluate(Position), curves[Mod + 2].Evaluate(Position));
    }

    private void OnPool() //Creation
    {
        GrabbedBlocks = new Dictionary<TankBlock, BlockDat>();
        base.block.serializeEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
        base.block.serializeTextEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));

        parts = new Transform[PartCount];
        for (int I = 0; I < PartCount; I++)
            parts[I] = block.transform.GetChild(I + 2);

        holder = new GameObject("Holding Agent").transform;
        holder.parent = parts[PartCount - 1].transform;

        LoadCOM = new GameObject("Load CenterOfMass").transform;
        LoadCOM.parent = holder;

        a_action = new Action<TankBlock, Tank>(this.BlockAdded);
        d_action = new Action<TankBlock, Tank>(this.BlockRemoved);
        block.AttachEvent.Subscribe(Attach);
        block.DetachEvent.Subscribe(Detatch);
    }

    internal abstract void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec);

    private void OnSpawn() //Pull from Object Pool
    {
        Heart = Control_Block.Class1.PistonHeart;
        GrabbedBlocks.Clear();
        Dirty = true;
    }

    internal virtual void Detatch()
    {
        //SFXIsOn = false;
        base.block.tank.TechAudio.RemoveModule<ModuleBlockMover>(this);
        ResetBlocks();
    }

    internal virtual void Attach()
    {
        block.tank.TechAudio.AddModule<ModuleBlockMover>(this);
        holder.position = block.tank.transform.position;
        holder.rotation = block.tank.transform.rotation;
        SetDirty();
    }

    internal virtual void BlockAdded(TankBlock block, Tank tank)
    {
        SetDirty();
    }

    internal virtual void BlockRemoved(TankBlock block, Tank tank)
    {
        if (GrabbedBlocks.ContainsKey(block))
        {
            GrabbedBlocks.Remove(block);
        }
        SetDirty();
    }

    internal struct BlockDat
    {
        public BlockDat(TankBlock Block)
        {
            pos = Block.cachedLocalPosition;
            ortho = Block.cachedLocalRotation;
        }
        public IntVector3 pos;
        public OrthoRotation ortho;
    }

    internal abstract void BeforeBlockAdded(TankBlock block);

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
        //Console.WriteLine(GetDateTime("CB(", "): ") + Message);
    }

    internal void CleanDirty()
    {
        if (!Dirty || !block.IsAttached || block.tank == null)
            return;
        ResetBlocks();
        //StartExtended = !SetToExpand;
        CanMove = GetBlocks(null, true);
        if (CanMove)
        {
            LoadCOM.transform.position = cacheCOM / GrabbedBlocks.Count;
            //foreach (var b in GrabbedBlocks)
            //{
            //    b.Key.transform.parent = holder;
            //}
        }
        Dirty = false;
        //Print("Piston " + block.transform.localPosition.ToString() + " is now  c l e a n s e d");
    }

    internal void ResetBlocks()
    {
        foreach (var b in GrabbedBlocks)
        {
            b.Key.transform.parent = block.transform.parent;
        }

        GrabbedBlocks.Clear();
        CurrentCellPush = 0;
        MassPushing = block.CurrentMass * 0.5f;
    }

    internal void SetDirty()
    {
        if (!Dirty)
        {
            //Print("Piston " + base.block.cachedLocalPosition.ToString() + " is now  d i r t y");
            Dirty = true;
        }
    }

    Vector3 cacheCOM = Vector3.zero;

    internal bool GetBlocks(TankBlock Start = null, bool BeginGrab = false, bool IsStarter = false)
    {
        if (BeginGrab)
        {
            Print("Starting blockgrab for BlockMover " + block.cachedLocalPosition.ToString());

            cacheCOM = Vector3.zero;
            UpdateCOM = true;

            var StarterBlocks = new List<TankBlock>();

            try
            {
                var blockman = block.tank.blockman;

                foreach (IntVector3 sbp in startblockpos)
                {
                    var _Start = blockman.GetBlockAtPosition((block.cachedLocalRotation * sbp) + block.cachedLocalPosition);
                    if (_Start == null)
                    {
                        continue;
                    }
                    if (GrabbedBlocks.ContainsKey(_Start))
                    {
                        continue;
                    }
                    bool isAttached = false;
                    foreach (var block in _Start.ConnectedBlocksByAP)
                    {
                        if (block != null && block == this.block)
                        {
                            isAttached = true;
                            break;
                        }
                    }
                    if (isAttached)
                    {
                        GrabbedBlocks.Add(_Start, new BlockDat(_Start));
                        CurrentCellPush += _Start.filledCells.Length;
                        MassPushing += _Start.CurrentMass;
                        cacheCOM += _Start.centreOfMassWorld;
                        StarterBlocks.Add(_Start);
                    }
                }
            }
            catch
            {
                Print("Something is VERY wrong:");
                Print(block == null ? "BLOCK IS NULL" : (block.tank == null ? "TANK IS NULL" : (block.tank.blockman == null ? "BLOCKMAN IS NULL" : "i don't even know")));
                return false;
            }
            bool result = true;
            foreach (var b in StarterBlocks)
            {
                result = GetBlocks(b, false, true);
                if (!result)
                    return false;
            }
            //do the stuff here

            return true;
        }

        try
        {
            foreach (TankBlock cb in Start.ConnectedBlocksByAP)
            {
                if (cb == null)
                {
                    continue;
                }
                try
                {
                    if (cb == block)
                    {
                        if (!IsStarter)
                        {
                            Print("Looped to self! Escaping blockgrab as false");
                            CurrentCellPush = -1;
                            return false;
                        }
                        else
                        {
                            Print("Skipping self");
                            continue;
                        }
                    }
                    if (BreakOnCab && cb.BlockCategory == BlockCategories.Control)
                    {
                        Print("Encountered cab! Escaping blockgrab as false");
                        CurrentCellPush = -2;
                        return false;
                    }
                    if (!GrabbedBlocks.ContainsKey(cb))
                    {
                        Print("Found " + cb.cachedLocalPosition.ToString());
                        CurrentCellPush += cb.filledCells.Length;
                        MassPushing += cb.CurrentMass;
                        cacheCOM += cb.centreOfMassWorld;
                        //if (CurrentCellPush > MaximumBlockPush)
                        //{
                        //    return false;
                        //}
                        GrabbedBlocks.Add(cb, new BlockDat(cb));
                        if (!GetBlocks(cb))
                            return false;
                    }
                }
                catch (Exception E)
                {
                    Print(E.Message + "\n" + E.StackTrace + "\n" + (cb == null ? "Cycled block is null!" : ""));
                }
            }
        }
        catch (Exception E2)
        {
            Print(E2.Message + "\n" + E2.StackTrace + "\n" + (Start == null ? "Scanned block is null!" : ""));
        }
        return true;
    }

    public const TTMsgType NetMsgPistonID = (TTMsgType)32113, NetMsgSwivelID = (TTMsgType)32114;
    internal static bool IsNetworkingInitiated = false;
    public static void InitiateNetworking()
    {
        if (IsNetworkingInitiated)
        {
            throw new Exception("Something tried to initiate the networking component of BlockMovers twice!\n" + System.Reflection.Assembly.GetCallingAssembly().FullName);
        }
        IsNetworkingInitiated = true;
        Nuterra.NetHandler.Subscribe<BlockMoverPistonMessage>(NetMsgPistonID, ReceivePistonChange);//, RequestMoverChange);
        Nuterra.NetHandler.Subscribe<BlockMoverSwivelMessage>(NetMsgSwivelID, ReceiveSwivelChange);//, RequestMoverChange);
    }

    private static void ReceivePistonChange(BlockMoverPistonMessage obj) => obj.block.GetComponent<ModulePiston>().ReceiveFromNet(obj);
    private static void ReceiveSwivelChange(BlockMoverSwivelMessage obj) => obj.block.GetComponent<ModuleSwivel>().ReceiveFromNet(obj);

    //private static void RequestMoverChange(BlockMoverMessage obj)
    //{

    //}

    //public abstract void SendToNet();

    public class BlockMoverMessage : UnityEngine.Networking.MessageBase
    {
        public BlockMoverMessage() { }
        public override void Deserialize(UnityEngine.Networking.NetworkReader reader)
        {
            tank = ClientScene.FindLocalObject(new NetworkInstanceId(reader.ReadUInt32())).GetComponent<Tank>();
            block = tank.blockman.GetBlockWithID(reader.ReadPackedUInt32());
        }

        public override void Serialize(UnityEngine.Networking.NetworkWriter writer)
        {
            writer.Write(tank.netTech.netId.Value);
            writer.Write(block.blockPoolID);
        }
        public TankBlock block;
        public Tank tank;
    }

    public class BlockMoverPistonMessage : BlockMoverMessage
    {
        public BlockMoverPistonMessage() { }
        public BlockMoverPistonMessage(TankBlock Block, byte CurrentPosition, byte TargetPosition)
        {
            tank = Block.tank;
            block = Block;
            currentPosition = CurrentPosition;
            targetPosition = TargetPosition;
        }
        public override void Deserialize(UnityEngine.Networking.NetworkReader reader)
        {
            base.Deserialize(reader);
            currentPosition = reader.ReadByte();
            targetPosition = reader.ReadByte();
        }

        public override void Serialize(UnityEngine.Networking.NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(currentPosition);
            writer.Write(targetPosition);
        }
        public byte currentPosition, targetPosition;
    }

    public class BlockMoverSwivelMessage : BlockMoverMessage
    {
        public BlockMoverSwivelMessage() { }
        public BlockMoverSwivelMessage(TankBlock Block, float CurrentAngle, float CurrentVelocity)
        {
            tank = Block.tank;
            block = Block;
            currentAngle = CurrentAngle;
            currentVelocity = CurrentVelocity;
        }
        public override void Deserialize(UnityEngine.Networking.NetworkReader reader)
        {
            base.Deserialize(reader);
            currentAngle = reader.ReadInt16() / 4f;
            currentVelocity = reader.ReadSingle();
        }

        public override void Serialize(UnityEngine.Networking.NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write((short)Mathf.RoundToInt(currentAngle * 4));
            writer.Write(currentVelocity);
        }
        public float currentAngle;
        public float currentVelocity;
    }
}