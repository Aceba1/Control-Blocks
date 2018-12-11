using System;
using System.Collections.Generic;
using UnityEngine;

abstract class ModuleBlockMover : Module
{
    public int PartCount = 1;
    public bool Heart;
    public AnimationCurve[] curves;
    public bool useRotCurves = false;
    public AnimationCurve[] rotCurves;
    public AnimationCurve blockcurve => curves[curves.Length - 1];
    public AnimationCurve blockrotcurve => rotCurves[rotCurves.Length - 1];
    public Transform[] parts;
    public IntVector3[] startblockpos;
    public int MaximumBlockPush = 64;
    public int CurrentCellPush { get; private set; } = 0;
    public float MassPushing { get; private set; } = 0f;
    public bool Dirty = true;
    public bool CanMove { get; internal set; } = false;
    public Dictionary<TankBlock, BlockDat> GrabbedBlocks;
    public Action<TankBlock, Tank> a_action, d_action;

    public Quaternion GetRotCurve(int Index, float Position)
    {
        int Mod = Index * 3;
        return Quaternion.Euler(rotCurves[Mod - 3].Evaluate(Position), rotCurves[Mod - 3].Evaluate(Position), rotCurves[Mod - 3].Evaluate(Position));
    }

    public Vector3 GetPosCurve(int Index, float Position)
    {
        int Mod = Index * 3;
        return new Vector3(curves[Mod - 3].Evaluate(Position), curves[Mod - 3].Evaluate(Position), curves[Mod - 3].Evaluate(Position));
    }

    private void OnPool()
    {
        GrabbedBlocks = new Dictionary<TankBlock, BlockDat>();
        base.block.serializeEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
        base.block.serializeTextEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
    }

    internal abstract void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec);

    private void OnSpawn()
    {
        Heart = Control_Block.Class1.PistonHeart;
        GrabbedBlocks.Clear();
        Dirty = true;
        parts = new Transform[PartCount];
        for (int I = 0; I < PartCount; I++)
            parts[I] = block.transform.GetChild(I + 2);
        a_action = new Action<TankBlock, Tank>(this.BlockAdded);
        d_action = new Action<TankBlock, Tank>(this.BlockRemoved);
        block.AttachEvent += Attach;
    }

    internal virtual void Attach()
    {
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
        Console.WriteLine(GetDateTime("CB(", "): ") + Message);
    }

    internal void CleanDirty()
    {
        if (!Dirty)
            return;
        ResetBlocks();
        //StartExtended = !SetToExpand;
        CanMove = GetBlocks(null, true);
        Dirty = false;
        //Print("Piston " + block.transform.localPosition.ToString() + " is now  c l e a n s e d");
    }

    internal void ResetBlocks()
    {
        GrabbedBlocks.Clear();
        CurrentCellPush = 0;
        MassPushing = block.CurrentMass;
    }

    internal void SetDirty()
    {
        if (!Dirty)
        {
            //Print("Piston " + base.block.cachedLocalPosition.ToString() + " is now  d i r t y");
            Dirty = true;
        }
    }

    internal bool GetBlocks(TankBlock Start = null, bool BeginGrab = false, bool IsStarter = false)
    {
        if (BeginGrab)
        {
            Print("Starting blockgrab for Piston " + block.cachedLocalPosition.ToString());

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
                    if (!GrabbedBlocks.ContainsKey(cb))
                    {
                        Print("Found " + cb.cachedLocalPosition.ToString());
                        CurrentCellPush += cb.filledCells.Length;
                        MassPushing += cb.CurrentMass;
                        if (CurrentCellPush > MaximumBlockPush)
                        {
                            return false;
                        }
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
}