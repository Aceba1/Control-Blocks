using System;
using System.Collections.Generic;
using UnityEngine;

namespace Control_Block
{
    public class BlockMoverHead : Module
    {

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
        /// WorldTreadmill interference security measure
        /// </summary>
        [NonSerialized]
        public bool Heart;

        /// <summary>
        /// Relative location of where blocks should be if attached to the head of this block
        /// </summary>
        public IntVector3[] startblockpos;


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

        [NonSerialized]
        bool Dirty;

        void OnPool()
        {
            GrabbedBlocks = new List<TankBlock>();
            GrabbedBlockMovers = new List<ModuleBlockMover>();
            StarterBlocks = new List<TankBlock>();
            IgnoredBlocks = new List<TankBlock>();
        }

        private void CreateHolder()
        {
            if (Holder == null)
            {
                Holder = new GameObject("ClusterBody Holder").AddComponent<ClusterBody>();
                Holder.moduleMover = this;
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
    }
}
