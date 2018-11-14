using System;
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

        protected short ReconnectEvents = 0;

        private void BlockAdded(TankBlock block, Tank tank)
        {
            float prealpha = alphaOpen, preopen = open;

            alphaOpen = 0f; open = 0f;
            SetRenderState();
            things.Clear();
            CanMove = GetBlocks();
            if (CanMove)
            {
                alphaOpen = prealpha; open = preopen;
                SetRenderState();
            }
        }
        private void BlockRemoved(TankBlock block, Tank tank)
        {
            CanMove = GetBlocks();
            //SetDirty()
        }
        private void SetDirty()
        {
            if (ReconnectEvents <= 0 && !Dirty)
            {
                Console.WriteLine("Piston " + base.transform.localPosition.ToString() + " is now  d i r t y");
                Dirty = true;
            }
        }
        void FixedUpdate()
        {
            try
            {
                if (ReconnectEvents <= 0)
                {
                    if (block.tank == null)
                    {
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
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
                Console.WriteLine(E.StackTrace);
            }
            if (ReconnectEvents > 0) ReconnectEvents--;
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
                        if (Input.GetKeyUp(trigger))
                        {
                            alphaOpen = 1f - alphaOpen;
                        }
                    }
                    else
                    {
                        if (Input.GetKeyDown(trigger))
                        {
                            alphaOpen = 1f - alphaOpen;
                        }
                    }
                }
                else
                {
                    if (Input.GetKey(trigger) != InverseTrigger)
                    {
                        alphaOpen = 1f;
                    }
                    else
                    {
                        alphaOpen = 0f;
                    }
                }
                if (open != alphaOpen)
                {
                    Move(alphaOpen != 0f);
                }
            }
            if (OVERRIDE)
            {
                OVERRIDE = false;
                Move(alphaOpen != 0f);
            }
            if (open != alphaOpen)
            {
                open = Mathf.Clamp01((open - .05f) + alphaOpen * .1f);
                SetRenderState();
            }
        }

        internal float open = 0f;
        public float Open
        {
            get
            {
                return open;
            }
        }

#warning Remove SetBlockState for ghost shifting
        //internal void SetBlockState()
        //{
        //    if (block.filledCells.Length < alphaOpen + 1f)
        //        block.filledCells = new IntVector3[] { IntVector3.zero, IntVector3.up };
        //    else if (block.filledCells.Length >= alphaOpen + 2f)
        //        block.filledCells = new IntVector3[] { IntVector3.zero };
        //    TopAP = alphaOpen == 0f ? Vector3.up * .5f : Vector3.up * 1.5f;
        //    var bounds = new Bounds(Vector3.zero, Vector3.zero);
        //    foreach (IntVector3 c in block.filledCells)
        //    {
        //        bounds.Encapsulate(c);
        //    }

        //    try
        //    {
        //        m_BlockCellBounds.SetValue(base.block, bounds);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //}

        #region Old
        /*      private void Move(bool Expand)
                {
                    CleanDirty(Expand);
                    if (CanMove)
                    {
                        ReconnectEvents = 2;
                        var blockman = block.tank.blockman;
                        bool FAILED = false;
                        foreach (var pair in things.Reverse())
                        {
                            blockman.RemoveBlock(pair.Key, false, false, true);
                        }
                        var thn = new blockDat(block);
                        blockman.RemoveBlock(block, false, false, true);
                        SetBlockState();
                        var temp = base.block;
                        blockman.AddBlock(ref temp, thn.pos, thn.ortho);
                        IntVector3 modifier = Expand != StartExtended ? block.transform.localRotation * (StartExtended? Vector3.down : Vector3.up) : Vector3.zero;
                        int iterate = things.Count;
                        foreach (var pair in things)
                        {
                            iterate--;
                            var val = pair.Key;
                            FAILED = !blockman.AddBlock(ref val, pair.Value.pos + modifier, pair.Value.ortho);
                            if (FAILED)
                                break;
                        }
                        if (FAILED)
                        {
                            Console.WriteLine("Piston move FAILED!");
                            foreach (var pair in things.Reverse().Skip(iterate))
                            {
                                try
                                {
                                    blockman.RemoveBlock(pair.Key, false, false, true);
                                }
                                catch(Exception E0)
                                {
                                    Console.WriteLine("Removing block failed! " + E0.Message);
                                }
                            }
                            alphaOpen = 1f - alphaOpen;
                            blockman.RemoveBlock(block, false, false, true);
                            SetBlockState();
                            var tempbaseblock = base.block;
                            blockman.AddBlock(ref tempbaseblock, thn.pos, thn.ortho);
                            foreach (var pair in things)
                            {
                                try
                                {
                                    var val = pair.Key;
                                    blockman.AddBlock(ref val, pair.Value.pos, pair.Value.ortho);
                                }
                                catch (Exception E0)
                                {
                                    Console.WriteLine("Adding block failed! " + E0.Message);
                                }
                            }
                            CanMove = false;

                        }
                    }
                    else
                    {
                        alphaOpen = open;
                    }
                }
        */
        #endregion

        private void Move(bool Expand)
        {
            CleanDirty(Expand);
            if (CanMove)
            {
                ReconnectEvents = 1;
                var blockman = block.tank.blockman;
                bool FAILED = false;
                Vector3 modifier = block.cachedLocalRotation * (Expand ? Vector3.up : Vector3.down);
                int iterate = things.Count;
                foreach (var pair in things)
                {
                    iterate--;
                    var val = pair.Key;
                    val.MoveLocalPositionWhileAttached(modifier);
                }
                    var thn = new blockDat(block);
                //blockman.RemoveBlock(block, false, false, true);
#warning Remove SetBlockState for ghost shifting
                //SetBlockState();
                var temp = base.block;
                    //blockman.AddBlock(ref temp, thn.pos, thn.ortho);
            }
            else
            {
                alphaOpen = open;
            }
        }

        private struct blockDat
        {
            public blockDat(TankBlock Block)
            {
                pos = Block.cachedLocalPosition;
                ortho = Block.cachedLocalRotation;
            }
            public IntVector3 pos;
            public OrthoRotation ortho;
        }

        private void CleanDirty(bool SetToExpand)
        {
            if (!Dirty)
                return;
            things.Clear();
            //StartExtended = !SetToExpand;
            CanMove = GetBlocks();
            Dirty = false;
            Console.WriteLine("Piston " + block.transform.localPosition.ToString() + " is now  c l e a n s e d");
        }

        public void SetRenderState()
        {
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

        public const ushort MaxBlockPush = 64;
        public int CurrentCellPush { get; private set; } = 0;
        internal bool Dirty = true;
        //internal bool StartExtended = false;
        bool CanMove = false;
        private Dictionary<TankBlock, blockDat> things;

        private bool GetBlocks(TankBlock Start = null, BlockManager blockman = null)
        {
            var _Start = Start;
            bool flag = things.Count == 0;
            if (flag)
            {
                Console.WriteLine("Starting blockgrab for Piston " + block.transform.localPosition.ToString());
                blockman = block.tank.blockman;
                _Start = blockman.GetBlockAtPosition((block.transform.localRotation * (/*StartExtended ? Vector3.up * 2 :*/ Vector3.up )) + block.transform.localPosition);
                if (_Start == null)
                {
                    Console.WriteLine("Piston is pushing nothing");
                    return true;
                }
                things.Add(_Start, new blockDat(_Start));
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
                                Console.WriteLine("Looped to self! Escaping blockgrab as false");
                                return false;
                            }
                            else
                            {
                                Console.WriteLine("Skipping self");
                                continue;
                            }
                        }
                        if (!things.ContainsKey(cb))
                        {
                            Console.WriteLine(cb.transform.localPosition.ToString());
                            CurrentCellPush += cb.filledCells.Length;
                            if (CurrentCellPush > MaxBlockPush)
                            {
                                return false;
                            }
                            things.Add(cb, new blockDat(cb));
                            if (!GetBlocks(cb, blockman))
                                return false;
                        }
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine(E.Message);
                        Console.WriteLine(E.StackTrace);
                        Console.WriteLine(cb == null ? "Cycled block is null!" : "");
                    }
                }
            }
            catch (Exception E2)
            {
                Console.WriteLine(E2.Message);
                Console.WriteLine(E2.StackTrace);
                Console.WriteLine(_Start == null ? "Scanned block is null!" : "");
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

        public void BeforeBlockAdded(IntVector3 localPos)
        {
            var serializationBuffer = (Array)s_BlockSerializationBuffer.GetValue(null);
            try
            {
                for (int i = 0; i < serializationBuffer.Length; i++)
                {
                    object sblock = serializationBuffer.GetValue(i);
                    var block = (TankBlock)SpawnContext_block.GetValue(sblock);
                    var blockSpec = (TankPreset.BlockSpec)SpawnContext_blockSpec.GetValue(sblock);

                    if (blockSpec.saveState.Count == 0) continue;
                    var data = Module.SerialData<ModulePiston.SerialData>.Retrieve(blockSpec.saveState);

                    if (base.block == block)
                    {
                        open = data.IsOpen ? 1f : 0f;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
            SetRenderState();
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
            things = new Dictionary<TankBlock, blockDat>();
            base.block.serializeEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
            base.block.serializeTextEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
        }

        private void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
        {
            if (saving)
            {
                ModulePiston.SerialData serialData = new ModulePiston.SerialData()
                {
                    IsOpen = this.open != 0f,
                    Input = this.trigger,
                    Toggle = this.IsToggle
                };
                serialData.Store(blockSpec.saveState);
            }
            else
            {
                ModulePiston.SerialData serialData2 = Module.SerialData<ModulePiston.SerialData>.Retrieve(blockSpec.saveState);
                if (serialData2 != null)
                {
                    alphaOpen = serialData2.IsOpen ? 1f : 0f;
                    OVERRIDE = serialData2.IsOpen;
                    trigger = serialData2.Input;
                    IsToggle = serialData2.Toggle;
                    
                    Dirty = true;
//                    StartExtended = serialData2.IsOpen;
#warning Remove SetBlockState for ghost shifting
                    //SetBlockState();
                }
            }
        }

        [Serializable]
        private new class SerialData : Module.SerialData<ModulePiston.SerialData>
        {
            public bool IsOpen;
            public KeyCode Input;
            public bool Toggle;
            public bool Local
        }
    }
}