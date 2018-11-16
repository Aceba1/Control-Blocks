﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace Control_Block
{
    class ModulePiston : Module
    {
        public Transform head, shaft;
        protected bool deserializing = false;
        internal bool OVERRIDE = false;
        private float alphaOpen = 0f, gOfs = 0f;
        public bool IsToggle;
        public bool InverseTrigger;
        public bool LocalControl = true;
        public KeyCode trigger = KeyCode.Space;

        protected static PropertyInfo ConnectedBlocksByAP;
        protected static FieldInfo m_BlockCellBounds;
        protected static MethodInfo CalculateDefaultPhysicsConstants;
        protected static FieldInfo s_BlockSerializationBuffer;
        protected static FieldInfo bufferLength;
        protected static MethodInfo GetValue;

        protected static FieldInfo SpawnContext_block;
        protected static FieldInfo SpawnContext_blockSpec;

        Action<TankBlock, Tank> a_action, d_action;

        protected Tank tankcache;

        /// <summary>
        /// Ignore animating
        /// </summary>
        bool SnapRender = true; 
        /// <summary>
        /// Invoke activate input
        /// </summary>
        bool ForceOpen = false;
        /// <summary>
        /// Force ghost-push as opened
        /// </summary>
        bool ForceMove = false;

        public void Print(string Message)
        {
            Console.WriteLine("CB(" + DateTime.Now.ToString("T", System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) + "): " + Message);
        }

        public void BeforeBlockAdded(TankBlock block)
        {
            //Print("Block was added");
            if (alphaOpen != 0f)
            {
                alphaOpen = 0f;
                if (IsToggle)
                {
                    ForceOpen = true;
                }
                ResetRenderState(true);
            }
            else
            {
                ResetRenderState(false);
            }
        }

        private void BlockAdded(TankBlock block, Tank tank)
        {
            //Print("BlockAdded()");
            /*
            //Only run if extended, refresh blocks, check if new block is part of, apply offset to new block if is
            if (SnapRender && alphaOpen == 0f)
            {
                things.Clear();
                CanMove = GetBlocks();
                if (CanMove)
                {
                    Move(true);
                    alphaOpen = 1f;
                    open = 1f;
                    SetRenderState();
                    if (things.ContainsKey(block))
                    {
                        block.transform.localPosition += this.block.cachedLocalRotation * Vector3.one;
                    }
                    SnapRender = true;
                }
            }
            else
            {
            */
            SetDirty();
            /*}*/
        }
        private void BlockRemoved(TankBlock block, Tank tank)
        {
            if (alphaOpen == 1f)
            {
                ForceMove = true;
                ResetRenderState(true);
            }
            else
            {
                ResetRenderState(false);
            }
            //Print("BlockRemoved()");
            things.Remove(block);
            SetDirty();
        }
        private void SetDirty()
        {
            if (!Dirty)
            {
                //Print("Piston " + base.block.cachedLocalPosition.ToString() + " is now  d i r t y");
                Dirty = true;
            }
        }

        bool VInput { get => !LocalControl || (LocalControl && (tankcache == Singleton.playerTank)); }

        void FixedUpdate()
        {
            if (ForceMove)
            {
                open = 1f;
                alphaOpen = 1f;
                Move(true);
                //SetRenderState();
                ForceMove = false;
            }
            try
            {
                if (block.tank == null)
                {
                    SetDirty();
                    tankcache?.AttachEvent.Unsubscribe(a_action);
                    tankcache?.DetachEvent.Unsubscribe(d_action);
                }
                else if (block.tank != tankcache)
                {
                    tankcache?.AttachEvent.Unsubscribe(a_action);
                    tankcache?.DetachEvent.Unsubscribe(d_action);
                    tankcache = block.tank;
                    tankcache.AttachEvent.Subscribe(a_action);
                    tankcache.DetachEvent.Subscribe(d_action);
                }
            }
            catch (Exception E)
            {
                Print(E.Message);
                Print(E.StackTrace);
            }
            if (!block.IsAttached || block.tank == null)
            {
                return;
            }
            if ((Dirty || CanMove) && open == alphaOpen)
            {
                if (IsToggle)
                {
                    if (InverseTrigger)
                    {
                        if (VInput && Input.GetKeyUp(trigger))
                        {
                            alphaOpen = 1f - alphaOpen;
                        }
                    }
                    else
                    {
                        if (VInput && Input.GetKeyDown(trigger))
                        {
                            alphaOpen = 1f - alphaOpen;
                        }
                    }
                }
                else
                {
                    if (VInput && Input.GetKey(trigger) != InverseTrigger)
                    {
                        alphaOpen = 1f;
                    }
                    else
                    {
                        alphaOpen = 0f;
                    }
                }
                if (ForceOpen)
                {
                    alphaOpen = 1f;
                    ForceOpen = false;
                }
                if (open != alphaOpen)
                {
                    Move(alphaOpen != 0f);
                }
            }
            if (OVERRIDE)
            {
                OVERRIDE = false;
                SnapRender = true;
                if (alphaOpen != 0f)
                {
                    Move(true);
                }
                SetRenderState();
            }
            if (open != alphaOpen)
            {
                open = Mathf.Clamp01((open - .05f) + alphaOpen * .1f);
                SetRenderState();
                //if (open == alphaOpen)
                //{
                //    if (alphaOpen == 1f)
                //    {

                //    }
                //    else
                //    {

                //    }
                //}
            }
            SnapRender = false;
        }

        internal float open = 0f;
        public float Open
        {
            get
            {
                return open;
            }
        }
        private void Move(bool Expand)
        {
            CleanDirty();
            if (CanMove)
            {
                var blockman = block.tank.blockman;
                Vector3 modifier = block.cachedLocalRotation * (Expand ? Vector3.up : Vector3.down);
                int iterate = things.Count;
                foreach (var pair in things)
                {
                    iterate--;
                    var val = pair.Key;
                    val.transform.localPosition += modifier;
                }
                    var thn = new BlockDat(block);
                var temp = base.block;
            }
            else
            {
                alphaOpen = open;
            }
        }

        private struct BlockDat
        {
            public BlockDat(TankBlock Block)
            {
                pos = Block.cachedLocalPosition;
                ortho = Block.cachedLocalRotation;
            }
            public IntVector3 pos;
            public OrthoRotation ortho;
        }

        private void CleanDirty()
        {
            if (!Dirty)
                return;
            things.Clear();
            //StartExtended = !SetToExpand;
            CanMove = GetBlocks();
            Dirty = false;
            //Print("Piston " + block.transform.localPosition.ToString() + " is now  c l e a n s e d");
        }

        /// <summary>
        /// ONLY USE IF ALL PISTONS ARE TO BE RESET
        /// </summary>
        /// <param name="ImmediatelySetAfter">Set SnapRender true</param>
        public void ResetRenderState(bool ImmediatelySetAfter = false) 
        {
            head.localPosition = Vector3.zero;
            shaft.localPosition = Vector3.zero;
            open = 0f;
            //alphaOpen = 0f;
            SnapRender = ImmediatelySetAfter;
            gOfs = 0;
            foreach (var pair in things)
            {
                var block = pair.Key;
                if (block.tank == base.block.tank)
                    block.transform.localPosition = block.cachedLocalPosition;
            }
        }

        public void SetRenderState()
        {
            if (SnapRender)
            {
                open = alphaOpen;
                SnapRender = false;
            }
            head.localPosition = Vector3.up * open;
            shaft.localPosition = Vector3.up * open * 0.375f;
            var rawOfs = open - alphaOpen;
            Vector3 offs = (block.transform.localRotation * Vector3.up) * (rawOfs-gOfs);
            gOfs = rawOfs;
            foreach (var pair in things)
            {
                var block = pair.Key;
                if (block.tank == base.block.tank)
                    block.transform.localPosition += offs;
            }
        }

        public const int MaxBlockPush = 64;
        public int CurrentCellPush { get; private set; } = 0;
        internal bool Dirty = true;
        //internal bool StartExtended = false;
        bool CanMove = false;
        private Dictionary<TankBlock, BlockDat> things;

        private bool GetBlocks(TankBlock Start = null, BlockManager blockman = null)
        {
            var _Start = Start;
            bool flag = things.Count == 0;
            if (flag)
            {
                Print("Starting blockgrab for Piston " + block.cachedLocalPosition.ToString());
                try
                {
                    blockman = block.tank.blockman;
                    _Start = blockman.GetBlockAtPosition((block.cachedLocalRotation * (/*StartExtended ? Vector3.up * 2 :*/ Vector3.up)) + block.cachedLocalPosition);
                    if (_Start == null)
                    {
                        CurrentCellPush = 0;
                        Print("Piston is pushing nothing");
                        return true;
                    }
                    things.Add(_Start, new BlockDat(_Start));
                    CurrentCellPush = _Start.filledCells.Length;
                }
                catch
                {
                    Print("Something is VERY wrong:");
                    Print(block == null ? "BLOCK IS NULL" : (block.tank == null ? "TANK IS NULL" : (block.tank.blockman == null ? "BLOCKMAN IS NULL" : "i don't even know")));
                    return false;
                }
            }

            try
            {
                foreach (TankBlock cb in _Start.ConnectedBlocksByAP)
                {
                    if (cb == null)
                    {
                        continue;
                    }
                    try
                    {
                        if (cb == block)
                        {
                            if (!flag)
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
                        if (!things.ContainsKey(cb))
                        {
                            Print("Found " + cb.cachedLocalPosition.ToString());
                            CurrentCellPush += cb.filledCells.Length;
                            if (CurrentCellPush > MaxBlockPush)
                            {
                                return false;
                            }
                            things.Add(cb, new BlockDat(cb));
                            if (!GetBlocks(cb, blockman))
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
                Print(E2.Message + "\n" + E2.StackTrace + "\n" + (_Start == null ? "Scanned block is null!" : ""));
            }
            return true;
        }

        static ModulePiston()
        {
            ConnectedBlocksByAP = typeof(TankBlock).GetProperty("ConnectedBlocksByAP");
            m_BlockCellBounds = typeof(TankBlock).GetField("m_BlockCellBounds", BindingFlags.Instance | BindingFlags.NonPublic);//.First(f => f.Name.Contains("m_BlockCellBounds"));
            CalculateDefaultPhysicsConstants = typeof(TankBlock).GetMethod("CalculateDefaultPhysicsConstants", BindingFlags.Instance | BindingFlags.NonPublic);
            s_BlockSerializationBuffer = typeof(ManSpawn).GetField("s_BlockSerializationBuffer", BindingFlags.NonPublic | BindingFlags.Static);
            var t = s_BlockSerializationBuffer.FieldType;
            bufferLength = t.GetField("Length", BindingFlags.Public | BindingFlags.Instance);
            GetValue = t.GetMethod("GetValue", new Type[] { typeof(int) });

            t = typeof(ManSpawn).GetNestedType("SpawnContext", BindingFlags.NonPublic);
            SpawnContext_block = t.GetField("block");
            SpawnContext_blockSpec = t.GetField("blockSpec");
        }

        public List<IntVector3> Cells
        {
            get
            {
                var c = new List<IntVector3>(1) { IntVector3.zero };
                if (open != 0f) c.Add(IntVector3.up);
                return c;
            }
        }

        public Vector3 TopAP
        {
            get
            {
                return block.attachPoints[0];
            }
            set
            {
                block.attachPoints[0] = value;
            }
        }

        private void OnSpawn()
        {
            things.Clear();
            Dirty = true;
            shaft = block.transform.GetChild(2);
            head = block.transform.GetChild(3);
            a_action = new Action<TankBlock, Tank>(this.BlockAdded);
            d_action = new Action<TankBlock, Tank>(this.BlockRemoved);
            block.AttachEvent += Attach;
            SetRenderState();
        }

        void Attach()
        {
            alphaOpen = 0;
            open = 0;
            gOfs = 0;
            SetDirty();
            tankcache?.AttachEvent.Unsubscribe(a_action);
            tankcache?.DetachEvent.Unsubscribe(d_action);
            tankcache = block.tank;
            tankcache.AttachEvent.Subscribe(a_action);
            tankcache.DetachEvent.Subscribe(d_action);
        }

        private void OnDisable()
        {
            things.Clear();
            Dirty = true;
            tankcache?.AttachEvent.Unsubscribe(a_action);
            tankcache?.DetachEvent.Unsubscribe(d_action);
            trigger = KeyCode.Space;
            IsToggle = false;
            InverseTrigger = false;
            alphaOpen = 0f;
            open = 0f;

        }

        private void OnPool()
        {
            things = new Dictionary<TankBlock, BlockDat>();
            base.block.serializeEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
            base.block.serializeTextEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
        }

        private void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
        {
            if (saving)
            {
                if (alphaOpen == 1f)
                {
                    ForceMove = true;
                    ResetRenderState(true);
                    open = 1f;
                }

                ModulePiston.SerialData serialData = new ModulePiston.SerialData()
                {
                    IsOpen = this.alphaOpen != 0f,
                    Input = this.trigger,
                    Toggle = this.IsToggle,
                    Local = this.LocalControl,
                    Invert = this.InverseTrigger
                };
                serialData.Store(blockSpec.saveState);
            }
            else
            {
                ModulePiston.SerialData serialData2 = Module.SerialData<ModulePiston.SerialData>.Retrieve(blockSpec.saveState);
                if (serialData2 != null)
                {
                    if (serialData2.IsOpen)
                    {
                        ForceMove = true;
                        SnapRender = true;
                    }
                    InverseTrigger = serialData2.Invert;
                    trigger = serialData2.Input;
                    IsToggle = serialData2.Toggle;
                    LocalControl = serialData2.Local;
                    Dirty = true;
                }
            }
        }

        [Serializable]
        private new class SerialData : Module.SerialData<ModulePiston.SerialData>
        {
            public bool IsOpen;
            public KeyCode Input;
            public bool Toggle;
            public bool Local;
            public bool Invert;
        }
    }
}