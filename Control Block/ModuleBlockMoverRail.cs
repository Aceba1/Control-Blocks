using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Control_Block
{
    public class ModuleBMRail : ModuleBlockMover
    {
        public AttachPoint starterAnim;
        public List<ModuleBMSegment> m_Segments;
        public BlockTypes m_thisHeadType;

        internal override bool CanStartGetBlocks(BlockManager blockMan)
        {
            ClearSegmentList(); // Remove pointers to this block

            foreach (var m in posCurves) // Clear position animation
                m.keys = new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f, 0f, 0f) };
            foreach (var m in rotCurves) // Clear rotation animation
                m.keys = new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f, 0f, 0f) };

            Quaternion TravelQuat = Quaternion.identity;
            Vector3 TravelRot = Vector3.zero;
            OrthoRotation OriginalRot = block.cachedLocalRotation;
            float Length = starterAnim.AddToAnimCurves(OrthoRotation.identity, this, 0f, ref TravelQuat, ref TravelRot);
            TrueLimitVALUE = 0f;

            TankBlock LastBlock = block;
            AttachPoint LastAP = starterAnim; //For grabbing the block at the next position

            TankBlock Segment = LastAP.GetBlockAtPos(LastBlock, blockMan);
            while (Segment != null)
            {
                Print(">> Found block " + Segment.cachedLocalPosition);
                ModuleBMSegment component = Segment.GetComponent<ModuleBMSegment>();
                if (component == null) // Not a rail segment
                {
                    Print("   Not a segment");
                    ModuleBMRail opposer = Segment.GetComponent<ModuleBMRail>();
                    if (opposer == null || !LastAP.CanConnect(LastBlock, Segment, opposer.starterAnim)) // Not an opposing rail, or not sharing 
                        break;
                    // Cut shared rail in half to prevent overlap
                    Print("   Is another head!");
                    CutSegmentListInHalf(); 
                    TrueLimitVALUE = HalfLimitVALUE - 0.5f; // Move back .5 for block-room on the AP
                    break;
                }

                if (component.blockMoverHeadType != m_thisHeadType)//component.blockMoverHeadType.Contains(m_thisHeadType))
                {
                    Print("   Wrong rail type!");
                    break; // A different rail system
                }

                var _Segment = Segment;
                Segment = null; // Nullify this, if not re-set it will break the loop

                for (int i = 0; i < 2; i++)
                {
                    if (LastAP.CanConnect(LastBlock, _Segment, component.APs[i]))
                    {
                        m_Segments.Add(component);
                        LastBlock = _Segment;
                        LastAP = component.APs[1 - i];

                        Length = component.APs[i].AddToAnimCurves(_Segment.cachedLocalRotation * Quaternion.Inverse(OriginalRot), this, Length, ref TravelQuat, ref TravelRot);
                        Segment = LastAP.GetBlockAtPos(_Segment, blockMan); // Set the new segment, continue

                        Print("   Connected!");
                        break; // Exit the foreach
                    }
                }
                if (Segment == null) Print(">> No more blocks");
            }

            //if (TrueLimitVALUE == 0f) TrueLimitVALUE = 0.25f;
            ValidateSegmentList(out bool DisableFreeJoint);

            CannotBeFreeJoint = true;//DisableFreeJoint;

            if (MAXVALUELIMIT >= TrueLimitVALUE)
            {
                SetMaxLimit(TrueLimitVALUE);
                if (PVALUE > TrueLimitVALUE) PVALUE = TrueLimitVALUE;
                if (VALUE > TrueLimitVALUE) VALUE = TrueLimitVALUE;
            }

            return true;
        }

        public bool CheckSegment(ModuleBMSegment segment)
        {
            return segment.blockMoverHeadType == m_thisHeadType;
        }

        void OnPool()
        {
            m_Segments = new List<ModuleBMSegment>();
            m_thisHeadType = (BlockTypes)block.visible.ItemType;
            block.DetachEvent.Subscribe(ClearSegmentList);
            block.AttachEvent.Subscribe(ClearSegmentList);

            starterAnim = ManSpawn.inst.GetBlockPrefab(m_thisHeadType).GetComponent<ModuleBMRail>().starterAnim;
            // Unity refused to serialize the type, despite efforts, so I'm using this workaround
        }

        public void ClearSegmentList()
        {
            foreach (var s in m_Segments)
                if (s != null) s.ClearPointer(this);
            m_Segments.Clear();
        }

        internal bool IsSegmentInList(ModuleBMSegment segment)
        {
            return m_Segments.Contains(segment);
        }

        public void ValidateSegmentList(out bool usingRot)
        {
            usingRot = false;
            foreach (var s in m_Segments)
            {
                s.UIPointer(this);
                if (s.APs[0].AnimRotChange != Vector3.zero) usingRot = true; // Check only the first, because the second should resemble it anyways
            }
        }

        public void CutSegmentListInHalf()
        {
            int rm = m_Segments.Count >> 1;
            m_Segments.RemoveRange(rm, m_Segments.Count - rm);
        }
    }

    public class ModuleBMSegment : Module
    {
        /// <summary>
        /// An array of 2 APs
        /// </summary>
        public AttachPoint[] APs;
        public BlockTypes blockMoverHeadType;
        public ModuleBMRail blockMoverPointer;
        public ModuleBlockMover VerifyBlockMover
        {
            get
            {
                if (blockMoverPointer == null)
                    return null;
                if (blockMoverPointer.IsSegmentInList(this))
                    return blockMoverPointer;
                ClearPointer();
                return null;
            }
        }

        public void UIPointer(ModuleBMRail moduleBlockMover)
        {
            blockMoverPointer = moduleBlockMover;
            if (blockMoverPointer != null) blockMoverPointer.block.DetachEvent.Subscribe(ClearPointer);
        }

        public void ClearPointer(ModuleBMRail moduleBlockMover)
        {
            if (blockMoverPointer == moduleBlockMover)
            {
                blockMoverPointer.block.DetachEvent.Unsubscribe(ClearPointer);
                blockMoverPointer = null;
            }
        }

        public void ClearPointer()
        {
            TryUnsubscribeFromPointer();
            blockMoverPointer = null;
        }

        private void TryUnsubscribeFromPointer()
        {
            if (blockMoverPointer != null)
            {
                blockMoverPointer.block.DetachEvent.Unsubscribe(ClearPointer);
            }
        }

        private void WakeupRailSystem()
        {
            ClearPointer();
            BlockManager blockman = block.tank.blockman;
            foreach(var ap in APs)
            {
                var other = ap.GetBlockAtPos(block, blockman);
                if (other == null) continue;
                var segment = other.GetComponent<ModuleBMSegment>();
                if (segment == null || segment.blockMoverPointer == null) continue;
                segment.blockMoverPointer.SetDirty(); // This is to fix the bug with rails not being picked up as they are placed
            }
        }

        void OnPool()
        {
            block.DetachEvent.Subscribe(ClearPointer);
           
            block.AttachEvent.Subscribe(WakeupRailSystem);

            APs = ManSpawn.inst.GetBlockPrefab((BlockTypes)block.visible.ItemType).GetComponent<ModuleBMSegment>().APs;
            // Unity refused to serialize the array, despite efforts, so I'm using this workaround
        }
    }

    /// <summary>
    /// An animation-appender in the format of 'From -> To'
    /// </summary>
    public class AttachPoint
    {
        /// <summary>
        /// The new position, moving from the apPos
        /// </summary>
        public Vector3 AnimPosChange;
        /// <summary>
        /// The new rotation after running through this block
        /// </summary>
        public Vector3 AnimRotChange;
        /// <summary>
        /// How long this block is, to the animation and to the max value
        /// </summary>
        public float AnimLength;
        /// <summary>
        /// The final direction, for smoothing
        /// </summary>
        public Vector3 Tangent;
        /// <summary>
        /// The block at apPos, for use with the BlockManager
        /// </summary>
        public IntVector3 blockPos;
        /// <summary>
        /// The center of the starter attach point
        /// </summary>
        public Vector3 apPos;
        /// <summary>
        /// The axis at which two APs should face eachother
        /// </summary>
        public IntVector3 apDirForward;
        /// <summary>
        /// The acis at which two APs should be coplanar
        /// </summary>
        public IntVector3 apDirUp;

        public TankBlock GetBlockAtPos(TankBlock thisBlock, BlockManager blockMan)
        {
            return blockMan.GetBlockAtPosition((thisBlock.cachedLocalRotation * blockPos) + thisBlock.cachedLocalPosition);
        }

        public bool CanConnect(TankBlock thisBlock, TankBlock otherBlock, AttachPoint otherAP)
        {
            return thisBlock.trans.TransformPoint(apPos).Approximately(otherBlock.trans.TransformPoint(otherAP.apPos), 0.5f) // Same AP
                && otherBlock.cachedLocalRotation * otherAP.apDirUp == thisBlock.cachedLocalRotation * apDirUp // Planar
                && Vector3.Dot(otherBlock.cachedLocalRotation * otherAP.apDirForward, thisBlock.cachedLocalRotation * apDirForward) < -0.9f; // Facing same direction
        }

        public int CanConnect(TankBlock thisBlock, TankBlock otherBlock, IList<AttachPoint> otherAttachPoints)
        {
            for (int i = 0; i < otherAttachPoints.Count; i++)
            {
                if (CanConnect(thisBlock, otherBlock, otherAttachPoints[i])) return i;
            }
            return -1;
        }

        private static Vector3 CycleToNearestEuler(Vector3 reference, Vector3 value)
        {
            return new Vector3(
                ((value.x - reference.x + 540) % 360) - 180 + reference.x, // mmmmm math
                ((value.y - reference.y + 540) % 360) - 180 + reference.y,
                ((value.z - reference.z + 540) % 360) - 180 + reference.z
                );
        }

        const float c1 = 0.19f, c2 = 0.02f; // or c1 = 0.24f, c2 = 0f;

        public float AddToAnimCurves(Quaternion cachedLocalRot, ModuleBMRail target, float length, ref Quaternion rotation, ref Vector3 vrot)//ref Quaternion rotation)
        {
            int Mod = target.PartCount * 3;
            var x = target.posCurves[Mod - 3];
            var y = target.posCurves[Mod - 2];
            var z = target.posCurves[Mod - 1];
            var rx = target.rotCurves[Mod - 3];
            var ry = target.rotCurves[Mod - 2];
            var rz = target.rotCurves[Mod - 1];
            int Ind = x.length - 1;
            var xLast = x[Ind];
            var yLast = y[Ind];
            var zLast = z[Ind];
            target.TrueLimitVALUE += AnimLength;
            length += AnimLength;

            var change = cachedLocalRot * AnimPosChange;
            var tangent = cachedLocalRot * Tangent;
            if (AnimRotChange != Vector3.zero)
            {
                rotation = ModuleBlockMover.RotateRotationByRotatedRotation(rotation, Quaternion.Euler(AnimRotChange), cachedLocalRot);
                vrot = CycleToNearestEuler(vrot, rotation.eulerAngles);
            }

            float weight = c1 + c2 * AnimLength;
            x.AddKey(new Keyframe(length, xLast.value + change.x, tangent.x, tangent.x, weight, weight));
            y.AddKey(new Keyframe(length, yLast.value + change.y, tangent.y, tangent.y, weight, weight));
            z.AddKey(new Keyframe(length, zLast.value + change.z, tangent.z, tangent.z, weight, weight));
            //xLast.outWeight = weight;
            //yLast.outWeight = weight;
            //zLast.outWeight = weight;

            rx.AddKey(new Keyframe(length, vrot.x, 0f, 0f, 0f, 0f));
            ry.AddKey(new Keyframe(length, vrot.y, 0f, 0f, 0f, 0f));
            rz.AddKey(new Keyframe(length, vrot.z, 0f, 0f, 0f, 0f));

            return length;
        }
    }
}
