using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Control_Block
{
    // Idea: Rail with corners that allow for turns, with modifications to animation

    /// <summary>
    /// Piston-only block mover that can be extended by blocks with ModuleBlockMoverRailSegment
    /// </summary>
    internal class ModuleBlockMoverRail : ModuleBlockMover
    {
        public IntVector3 railblockpos; // Can only work with 1x1x1 blocks
        internal override bool CanStartGetBlocks(BlockManager blockMan)
        {
            var Rail = blockMan.GetBlockAtPosition((block.cachedLocalRotation * railblockpos) + block.cachedLocalPosition);
            int rails = 0;
            float extent = 0f;
            if (Rail)
            {
                var component = Rail.GetComponent<ModuleBlockMoverRailSegment>();
                while (component != null)
                {
                    if (component.block.cachedLocalRotation * Vector3.forward == block.cachedLocalRotation * Vector3.forward) // Planar
                    {
                        if (Mathf.Abs(Vector3.Dot(component.block.cachedLocalRotation * Vector3.up, block.cachedLocalRotation * Vector3.up)) > 0.9) // Facing same direction
                        {
                            rails++; // Append

                            Rail = blockMan.GetBlockAtPosition((block.cachedLocalRotation * railblockpos * rails) + block.cachedLocalPosition); // Continue block chain
                            if (Rail) // If block exists
                            {
                                var endCheck = Rail.GetComponent<ModuleBlockMoverRail>();
                                if (endCheck) // If block is rail starter
                                {
                                    if (endCheck.block.cachedLocalRotation * Vector3.forward == block.cachedLocalRotation * Vector3.forward && // Planar
                                        Vector3.Dot(endCheck.block.cachedLocalRotation * Vector3.up, block.cachedLocalRotation * Vector3.up) < -0.9) // Facing opposite direction
                                    {
                                        extent = rails * 0.5f - 0.5f; // Share half of rails, end
                                        break;
                                    }
                                }
                                component = Rail.GetComponent<ModuleBlockMoverRailSegment>();
                                if (component == null) // If block is not rail
                                {
                                    extent = --rails; // End
                                    break;
                                }
                                continue; // On to the next block
                            }
                        }
                    }
                    extent = --rails; // End
                    break;
                }
            }
            if (extent <= 0) extent = 0.25f;
            //var keyframe = posCurves[PartCount * 3 - 2].keys[PartCount - 1];
            //keyframe.time = extent;
            //keyframe.value = extent; // * extentScale
            
            //posCurves[PartCount * 3 - 2].keys[1] = new Keyframe(extent, extent /* * extentScale */, 1, 0);
            if (MAXVALUELIMIT >= TrueLimitVALUE || MAXVALUELIMIT >= extent)
            {
                SetMaxValueLimit(extent);
                if (PVALUE > extent) PVALUE = extent;
                if (VALUE > extent) VALUE = extent;
            }
            TrueLimitVALUE = extent;
            return true;
        }
    }
    internal class ModuleBlockMoverRailSegment : Module { }
}
