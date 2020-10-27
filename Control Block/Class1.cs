using Harmony;
using Nuterra.BlockInjector;
using System;
using System.Reflection;
using UnityEngine;

namespace Control_Block
{
    public class Class1
    {
        const string MoverText = "\n Right click to configure this block.\n\n" +
            "This is a BlockMover. Blocks attached to the head of this will have their own physics separate from the body they are on, yet still restrained to the same tech. Like a multi-tech, but a single tech. ClusterTech.";
        const string FakeMoverText = "\n Right click to configure this block.\n\n" +
            "This is a decorative BlockMover. It does not separate physics bodies or move blocks at all, but it still posesses the programmability of a standard BlockMover.";
        public static void CreateBlocks()
        {

            #region Blocks
            {
                #region Materials
                Material bf_mat = GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main");
                #endregion Materials

                #region Pistons
                {
                    #region GSO Piston
                    {
                        var ControlBlock = new BlockPrefabBuilder(BlockTypes.GSOBlock_111)
                            .SetName("GSO Piston")
                            .SetDescription("A configurable piston that can push and pull blocks on a tech. Constructed from the poorly-fabricated concept of a child, by his own future being. Because nothing is impossible." + MoverText)
                            .SetBlockID(1293838)
                            .SetFaction(FactionSubTypes.GSO)
                            .SetCategory(BlockCategories.Control)
                            .SetGrade(2)
                            .SetPrice(4527)
                            .SetHP(1000)
                            .SetMass(2.5f)
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.piston_icon_png)));

                        var mat = GameObjectJSON.GetObjectFromGameResources<Material>("GSO_Main");
                        var par = ControlBlock.Prefab.transform;

                        AddMeshToBlockMover(mat, new Vector3(.95f, .725f, .95f), new Vector3(0f, -.125f, 0f), par, Properties.Resources.piston_base);
                        AddMeshToBlockMover(mat, new Vector3(.75f, .8f, .75f), Vector3.zero, par, Properties.Resources.piston_shaft);
                        AddMeshToBlockMover(mat, new Vector3(.8f, .9f, .8f), Vector3.zero, par, Properties.Resources.piston_head);

                        ControlBlock.SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[]{
                                Vector3.up * 0.5f,
                                Vector3.down * 0.5f,
                                Vector3.left * 0.5f,
                                Vector3.right * 0.5f,
                                Vector3.forward * 0.5f,
                                Vector3.back * 0.5f })
                            .AddComponent<ModuleBlockMover>(SetGSOPiston)
                            .SetRecipe(ChunkTypes.FuelInjector, ChunkTypes.SensoryTransmitter, ChunkTypes.PlubonicAlloy)
                            .RegisterLater();
                    }
                    #endregion GSO Piston

                    #region GeoCorp Piston
                    {
                        var ControlBlock = new BlockPrefabBuilder(BlockTypes.GCBlock_222)
                            .SetName("GeoCorp Large Piston")
                            .SetDescription("This is a bulky piston. Slower, smoother, and sturdy as heck.\nForged in the valleys of Uberkartoffel potatoes" + MoverText)
                            .SetBlockID(129380)
                            .SetFaction(FactionSubTypes.GC)
                            .SetCategory(BlockCategories.Control)
                            .SetGrade(2)
                            .SetPrice(7323)
                            .SetHP(8000)
                            .SetMass(10f)
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.GEOp_icon_png)));

                        var mat = GameObjectJSON.GetObjectFromGameResources<Material>("GC_Main");
                        var par = ControlBlock.Prefab.transform;

                        AddMeshToBlockMover(mat, new Vector3(1.99f, .95f, 1.99f), new Vector3(.5f, 0f, .5f), par, Properties.Resources.GEOp_blockbottom);
                        AddMeshToBlockMover(mat, new Vector3(1.6f, 1f, 1.6f), new Vector3(.5f, .5f, .5f), par, Properties.Resources.GEOp_shaftbottom);
                        AddMeshToBlockMover(mat, new Vector3(1.3f, 1f, 1.3f), new Vector3(.5f, .5f, .5f), par, Properties.Resources.GEOp_shafttop);
                        AddMeshToBlockMover(mat, new Vector3(1.99f, .95f, 1.99f), new Vector3(.5f, 1f, .5f), par, Properties.Resources.GEOp_blocktop);

                        ControlBlock.SetSizeManual(new IntVector3[] {
                                new IntVector3(0,0,0),
                                new IntVector3(0,0,1),
                                new IntVector3(0,1,0),
                                new IntVector3(0,1,1),
                                new IntVector3(1,0,0),
                                new IntVector3(1,0,1),
                                new IntVector3(1,1,0),
                                new IntVector3(1,1,1)
                            }, new Vector3[]{
                                new Vector3(0f,-.5f,0f),
                                new Vector3(1f,-.5f,0f),
                                new Vector3(0f,-.5f,1f),
                                new Vector3(1f,-.5f,1f),
                                new Vector3(0f,1.5f,0f),
                                new Vector3(1f,1.5f,0f),
                                new Vector3(0f,1.5f,1f),
                                new Vector3(1f,1.5f,1f)
                            }).AddComponent<ModuleBlockMover>(SetGeoCorpPiston)
                            .SetRecipe(ChunkTypes.FuelInjector, ChunkTypes.SensoryTransmitter, ChunkTypes.PlubonicAlloy,
                                       ChunkTypes.FuelInjector, ChunkTypes.FuelInjector, ChunkTypes.TitanicAlloy)
                            .RegisterLater();
                    }
                    #endregion GeoCorp Piston

                    #region Hawkeye Piston
                    {
                        var ControlBlock = new BlockPrefabBuilder(BlockTypes.HE_StdBlock_Alt_1_02_111)
                            .SetName("Hawkeye Telescopic Piston")
                            .SetDescription("A set of enforced interlocked plates composing a piston that can extend to 4 blocks from its compressed state." + MoverText)
                            .SetBlockID(129381)
                            .SetFaction(FactionSubTypes.HE)
                            .SetCategory(BlockCategories.Control)
                            .SetGrade(2)
                            .SetPrice(6993)
                            .SetHP(2250)
                            .SetMass(5f)
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.HEp_icon_png)));

                        var mat = GameObjectJSON.GetObjectFromGameResources<Material>("HE_Main");
                        var par = ControlBlock.Prefab.transform;

                        AddMeshToBlockMover(mat, new Vector3(.95f, .95f, .95f), Vector3.zero, par, Properties.Resources.HEp_blockbottom);
                        AddMeshToBlockMover(mat, new Vector3(.875f, .47f, .875f), Vector3.down * .25f, par, Properties.Resources.HEp_shaftbottom);
                        AddMeshToBlockMover(mat, new Vector3(.75f, .95f, .75f), Vector3.zero, par, Properties.Resources.HEp_shaftmidb);
                        AddMeshToBlockMover(mat, new Vector3(.75f, .95f, .75f), Vector3.zero, par, Properties.Resources.HEp_shaftmidt);
                        AddMeshToBlockMover(mat, new Vector3(.875f, .47f, .875f), Vector3.up * .25f, par, Properties.Resources.HEp_shafttop);
                        AddMeshToBlockMover(mat, new Vector3(.95f, .95f, .95f), Vector3.zero, par, Properties.Resources.HEp_blocktop);

                        ControlBlock.SetSizeManual(new IntVector3[] {
                    new IntVector3(0,0,0)
                }, new Vector3[]{
                    new Vector3(0f,-.5f,0f),
                    new Vector3(0f,0f,-.5f),
                    new Vector3(0f,.5f, 0f),
                    new Vector3(0f, 0f,.5f),
                }).AddComponent<ModuleBlockMover>(SetHawkeyePiston)
                            .SetRecipe(ChunkTypes.FuelInjector, ChunkTypes.FuelInjector, ChunkTypes.SensoryTransmitter, ChunkTypes.TitanicAlloy, ChunkTypes.PlubonicAlloy, ChunkTypes.PlubonicAlloy)
                            .RegisterLater();
                    }

                    {
                        var ControlBlock = new BlockPrefabBuilder(BlockTypes.HE_ArmouredBlock_10_111)
                            .SetName("Hawkeye Armoured Panel Piston")
                            .SetDescription("Another set of enforced interlocked plates composing a piston that can extend to 4 blocks from its relatively less compressed state of 2. Works pretty well as armor. Armour? What region is this?" + MoverText)
                            .SetBlockID(6194710)
                            .SetFaction(FactionSubTypes.HE)
                            .SetCategory(BlockCategories.Control)
                            .SetGrade(2)
                            .SetPrice(5283)
                            .SetHP(2250)
                            .SetMass(2f)
                            .SetCenterOfMass(new Vector3(0f, 0.5f, -0.4f))
                            .SetDamageableType(ManDamage.DamageableType.Armour)
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.pistonblock_he_panel)));

                        var mat = GameObjectJSON.GetObjectFromGameResources<Material>("HE_Main");
                        var par = ControlBlock.Prefab.transform;

                        AddMeshToBlockMover(mat, new Vector3(1f, 1f, .2f), new Vector3(0f, 0f, -0.4f), par, Properties.Resources.HEps_base);
                        AddMeshToBlockMover(mat, Vector3.zero, par, Properties.Resources.HEps_shaft_1);
                        AddMeshToBlockMover(mat, new Vector3(1f, 2f, .1f), new Vector3(0f, .5f, -0.4f), par, Properties.Resources.HEps_shaft_2);
                        AddMeshToBlockMover(mat, Vector3.zero, par, Properties.Resources.HEps_shaft_3);
                        AddMeshToBlockMover(mat, new Vector3(1f, 1f, .2f), new Vector3(0f, 1f, -0.4f), par, Properties.Resources.HEps_head);

                        ControlBlock.SetSizeManual(new IntVector3[] {
                    new IntVector3(0,0,0), new IntVector3(0,1,0)
                }, new Vector3[]{
                    new Vector3(0f,0f,-.5f),
                    new Vector3(0f,1f,-.5f),
                }).AddComponent<ModuleBlockMover>(SetHawkeyePanelPiston)
                            .SetRecipe(ChunkTypes.FuelInjector, ChunkTypes.SensoryTransmitter, ChunkTypes.PlubonicAlloy, ChunkTypes.TitanicAlloy)
                            .RegisterLater();
                    }
                    #endregion Hawkeye Piston

                    #region BetterFuture Piston
                    {
                        var ControlBlock = new BlockPrefabBuilder(BlockTypes.BF_Block_111)
                            .SetName("Better Piston")
                            .SetDescription("This piston started the revolution for all pistons and swivels. Replacing the technology of ghost-phasing and uniting swivels and pistons as one in a series of events that this piston was not aware was happening." + MoverText)
                            .SetBlockID(1293834)
                            .SetFaction(FactionSubTypes.BF)
                            .SetCategory(BlockCategories.Control)
                            .SetGrade(0)
                            .SetPrice(7587)
                            .SetHP(2000)
                            .SetMass(3f)
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.BFp_png)));

                        var mat = GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main");
                        var par = ControlBlock.Prefab.transform;

                        AddMeshToBlockMover(mat, new Vector3(.95f, .9f, .95f), Vector3.down * 0.05f, par, Properties.Resources.BFp_blockbottom);
                        AddMeshToBlockMover(mat, new Vector3(1f, .9f, 1f), Vector3.up * 0.05f, par, Properties.Resources.BFp_blocktop);

                        ControlBlock.SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[]{
                    Vector3.up * 0.5f,
                    Vector3.down * 0.5f })
                            .AddComponent<ModuleBlockMover>(SetBFPiston)
                            .SetRecipe(ChunkTypes.HardenedTitanic, ChunkTypes.FuelInjector, ChunkTypes.HeatCoil, ChunkTypes.SensoryTransmitter, ChunkTypes.PlubonicAlloy)
                            .RegisterLater();
                    }
                    #endregion BetterFuture Piston

                    #region Venture Piston
                    {
                        var ControlBlock = new BlockPrefabBuilder(BlockTypes.VENBlock_111)
                            .SetName("Venture Twist Piston")
                            .SetDescription("Venture wanted to get their own piston, but with a twist. That's literally it. It could be used quite well as a shock absorber..." + MoverText)
                            .SetBlockID(1293837)
                            .SetFaction(FactionSubTypes.VEN)
                            .SetCategory(BlockCategories.Control)
                            .SetGrade(0)
                            .SetPrice(6075)
                            .SetHP(1250)
                            .SetMass(1f)
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.ven_twist_piston_png)));

                        var mat = GameObjectJSON.GetObjectFromGameResources<Material>("VEN_Main");
                        var par = ControlBlock.Prefab.transform;

                        AddMeshToBlockMover(mat, new Vector3(.5f, .5f, .5f), Vector3.down * 0.5f, par, Properties.Resources.ven_twist_piston_base);
                        AddMeshToBlockMover(mat, new Vector3(.3f, .95f, .95f), Vector3.zero, par, Properties.Resources.ven_twist_piston_shaft_1);
                        AddMeshToBlockMover(mat, new Vector3(.3f, .95f, .95f), Vector3.zero, par, Properties.Resources.ven_twist_piston_shaft_2);
                        AddMeshToBlockMover(mat, new Vector3(.5f, .5f, .5f), Vector3.up * 0.5f, par, Properties.Resources.ven_twist_piston_head);

                        ControlBlock.SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[]{
                        Vector3.up * 0.5f,
                        Vector3.down * 0.5f })
                            .AddComponent<ModuleBlockMover>(SetVENPiston)
                            .SetRecipe(ChunkTypes.PlubonicGreebles, ChunkTypes.PlubonicGreebles, ChunkTypes.BlastCaps, ChunkTypes.FuelInjector, ChunkTypes.SensoryTransmitter, ChunkTypes.PlubonicAlloy)
                            .RegisterLater();
                    }
                    #endregion BetterFuture Piston

                    #region Reticule Research Piston
                    {
                        var Piston = new BlockPrefabBuilder(BlockTypes.EXP_Block_111)
                            .SetName("R.R. Floating Piston")
                            .SetDescription("The Floating Piston is an experimental block-mover developed at A.C.E. laboratories, in an effort to make a floating table which dogs wouldn’t bump into while playing around the lab (the facility is famous for the large cloned dog population).\n" +
                            "It uses the new R- AF ionic engine to move a tiny platform up and down. One can even pass their hand between both blocks, as the ionic beam is completely harmless on short term." + MoverText + "\n\n<b>Aceba1</b>: And so King Rafs pronounced, 'Let it do be so', with two pitchforks in each hand; And piercing the sky at all 4 corners, raised the man hole cover from the grasp of the pothole pathways for the first time since two days ago. Then, proceeded to fire at it with grace and pitchforks to propel it to the heavens, gifting it with the imprinted psychological trauma of flight.\n<b>Rafs</b>: What the-")
                            .SetBlockID(1980325)
                            .SetFaction(FactionSubTypes.EXP)
                            .SetCategory(BlockCategories.Control)
                            .SetGrade(0)
                            .SetPrice(10000) // Fix later
                            .SetRarity(BlockRarity.Rare)
                            .SetHP(600)
                            .SetMass(4f)
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.rr_floating_piston)))
                            .AddComponent(out SimpleConnectLineRenderer lineRenderer);

                        var mat = GameObjectJSON.GetObjectFromGameResources<Material>("RR_Main");
                        var par = Piston.Prefab.transform;

                        AddMeshToBlockMover(mat, new Vector3(1f, 0.8f, 1f), Vector3.down * 0.1f, par, Properties.Resources.rr_floating_piston_base);
                        lineRenderer.refObj = AddMeshToBlockMover(mat, new Vector3(1f, 0.2f, 1f), Vector3.up * 0.4f, par, Properties.Resources.rr_floating_piston_head);
                        lineRenderer.strPos = new Vector3(0f, 0.3f, 0f);
                        lineRenderer.refPos = new Vector3(0f, 0.4f, 0f);
                        lineRenderer.width = 0.6f;
                        lineRenderer.material = GameObjectJSON.GetObjectFromGameResources<Material>("MAT_BF_SkyAnchor_Beam");

                        Piston.SetSize(IntVector3.one)
                            .SetAPsManual(new Vector3[] { Vector3.down * 0.5f, Vector3.up * 0.5f })
                            .SetCustomEmissionMode(BlockPrefabBuilder.EmissionMode.Active)
                            .AddComponent<ModuleBlockMover>(SetRRFloatingPiston)
                            .RegisterLater();
                    }
                    #endregion Reticule Research Piston

                    #region Special Pistons
                    {
                        #region BetterFuture Rail Piston
                        var bfmat = GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main");
                        {
                            var ControlBlock = new BlockPrefabBuilder(BlockTypes.BF_Block_111)
                                .SetName("Better Future Rail Piston")
                                .SetDescription("An extendable rail, with a small cart that can move blocks attached to it. Add rail segment blocks to the end to make it longer!" + MoverText)
                                .SetBlockID(1293835)//, "f63931ef3e14ba8e")
                                .SetFaction(FactionSubTypes.BF)
                                .SetCategory(BlockCategories.Control)
                                .SetGrade(0)
                                .SetPrice(6984)
                                .SetHP(600)
                                .SetDetachFragility(0.5f)
                                .SetMass(1.5f)
                                .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.bf_rail_piston)))
                                .SetRecipe(ChunkTypes.HardenedTitanic, ChunkTypes.FuelInjector, ChunkTypes.HardLightDrive, ChunkTypes.SensoryTransmitter);

                            var par = ControlBlock.Prefab.transform;

                            AddMeshToBlockMover(bfmat, new Vector3(1f, 1f, 1f), Vector3.zero, par, Properties.Resources.bf_rail_piston_base);
                            AddMeshToBlockMover(bfmat, new Vector3(0.2f, 0.4f, 0.2f), new Vector3(0f, 0f, 0.5f), par, Properties.Resources.bf_rail_piston_head);

                            ControlBlock.SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.All)
                                .AddComponent<ModuleBMRail>(SetBFRailPiston)
                                .RegisterLater();
                        }
                        BlockTypes railID = (BlockTypes)1293835;
                        {
                            var ControlBlock = new BlockPrefabBuilder(BlockTypes.BF_Block_111)
                                .SetName("Better Future Rail segment")
                                .SetDescription("(Travel length: 1) A segment for the Better Future Rail Piston, add it to the end of the line to make it go farther.")
                                .SetBlockID(1293836)//, "f63931ef3e14ba8e")
                                .SetFaction(FactionSubTypes.BF)
                                .SetCategory(BlockCategories.Control)
                                .SetGrade(0)
                                .SetPrice(2106)
                                .SetHP(500)
                                .SetDetachFragility(0.5f)
                                .SetMass(1.5f)
                                .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.bf_rail_segment)))
                                .SetRecipe(ChunkTypes.HardenedTitanic, ChunkTypes.HardenedTitanic, ChunkTypes.CelestianCrystal, ChunkTypes.CelestianCrystal)
                                .SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[]{
                                    Vector3.up * 0.5f,
                                    Vector3.down * 0.5f,
                                    Vector3.left * 0.5f,
                                    Vector3.right * 0.5f,
                                    Vector3.back * 0.5f })
                                .AddComponent<ModuleBMSegment>(out ModuleBMSegment segment);

                            var par = ControlBlock.Prefab.transform;

                            AddMeshToBlockMover(bfmat, new Vector3(1f, 1f, 1f), Vector3.zero, par, Properties.Resources.bf_rail_piston_extension);

                            segment.blockMoverHeadType = railID;
                            segment.APs = new AttachPoint[2]
                            {
                                new AttachPoint()
                                {
                                    AnimLength = 1f,
                                    AnimPosChange = Vector3.up,
                                    Tangent = Vector3.up,
                                    apPos = new Vector3(0f, -0.5f, 0f),
                                    blockPos = new IntVector3(0, -1, 0),
                                    apDirForward = new IntVector3(0, -1, 0),
                                    apDirUp = IntVector3.forward,
                                },
                                new AttachPoint()
                                {
                                    AnimLength = 1f,
                                    AnimPosChange = Vector3.down,
                                    Tangent = Vector3.down,
                                    apPos = new Vector3(0f, 0.5f, 0f),
                                    blockPos = new IntVector3(0, 1, 0),
                                    apDirForward = new IntVector3(0, 1, 0),
                                    apDirUp = IntVector3.forward,
                                }
                            };

                            ControlBlock.RegisterLater();
                        }

                        {
                            var ControlBlock = new BlockPrefabBuilder(BlockTypes.BF_Block_111)
                                .SetName("Better Future Rail long segment")
                                .SetDescription("(Trevel length: 2) A longer segment for the Better Future Rail Piston, add it to the end of the line to make it go even farther.")
                                .SetBlockID(1393800)//, "f63931ef3e14ba8e")
                                .SetFaction(FactionSubTypes.BF)
                                .SetCategory(BlockCategories.Control)
                                .SetGrade(0)
                                .SetPrice(3879)
                                .SetHP(800)
                                .SetDetachFragility(0.5f)
                                .SetMass(3f)
                                .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.bf_rail_segment_long)))
                                .SetRecipe(ChunkTypes.HardenedTitanic, ChunkTypes.HardenedTitanic, ChunkTypes.HardenedTitanic, ChunkTypes.CelestianCrystal, ChunkTypes.CelestianCrystal, ChunkTypes.CelestianCrystal, ChunkTypes.CelestianCrystal)
                                .SetSize(new IntVector3(1, 2, 1))
                                .SetAPsManual(new Vector3[] {
                                    Vector3.up * 1.5f,
                                    Vector3.down * 0.5f,
                                    Vector3.left * 0.5f,
                                    Vector3.right * 0.5f,
                                    Vector3.back * 0.5f,
                                    new Vector3(-0.5f, 1f, 0f),
                                    new Vector3(0.5f, 1f, 0f),
                                    new Vector3(0f, 1f, -0.5f) })
                                .AddComponent<ModuleBMSegment>(out ModuleBMSegment segment);

                            var par = ControlBlock.Prefab.transform;

                            AddMeshToBlockMover(bfmat, new Vector3(1f, 2f, 1f), new Vector3(0f, 0.5f, 0f), par, Properties.Resources.bf_rail_piston_extension_2);

                            segment.blockMoverHeadType = railID;
                            segment.APs = new AttachPoint[2]
                            {
                                new AttachPoint()
                                {
                                    AnimLength = 2f,
                                    AnimPosChange = Vector3.up * 2f,
                                    Tangent = Vector3.up,
                                    apPos = new Vector3(0f, -0.5f, 0f),
                                    blockPos = new IntVector3(0, -1, 0),
                                    apDirForward = new IntVector3(0, -1, 0),
                                    apDirUp = IntVector3.forward
                                },
                                new AttachPoint()
                                {
                                    AnimLength = 2f,
                                    AnimPosChange = Vector3.down * 2f,
                                    Tangent = Vector3.down,
                                    apPos = new Vector3(0f, 1.5f, 0f),
                                    blockPos = new IntVector3(0, 2, 0),
                                    apDirForward = new IntVector3(0, 1, 0),
                                    apDirUp = IntVector3.forward
                                }
                            };

                            ControlBlock.RegisterLater();
                        }

                        {
                            var ControlBlock = new BlockPrefabBuilder(BlockTypes.BF_Block_111)
                                .SetName("Better Future Rail wedge segment")
                                .SetDescription("(Travel length: 2) A curved segment for the Better Future Rail Piston, add it to the end of the line to make it bend!")
                                .SetBlockID(1393801)//, "f63931ef3e14ba8e")
                                .SetFaction(FactionSubTypes.BF)
                                .SetCategory(BlockCategories.Control)
                                .SetGrade(0)
                                .SetPrice(2826)
                                .SetHP(800)
                                .SetDetachFragility(0.5f)
                                .SetMass(1f)
                                .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.bf_rail_segment_streamline)))
                                .SetRecipe(ChunkTypes.HardenedTitanic, ChunkTypes.HardenedTitanic, ChunkTypes.CelestianCrystal, ChunkTypes.CelestianCrystal, ChunkTypes.CelestianCrystal)
                                .SetSize(new IntVector3(1, 1, 1))
                                .SetAPsManual(new Vector3[]{
                                    Vector3.down * 0.5f,
                                    Vector3.left * 0.5f,
                                    Vector3.right * 0.5f,
                                    Vector3.back * 0.5f })
                                .AddComponent<ModuleBMSegment>(out ModuleBMSegment segment);

                            var par = ControlBlock.Prefab.transform;

                            AddMeshToBlockMover(bfmat, new Vector3(1f, 1f, 1f), Vector3.zero, par, Properties.Resources.bf_rail_piston_wedge);

                            segment.blockMoverHeadType = railID;
                            segment.AnimWeight = 0.13f;
                            segment.APs = new AttachPoint[2]
                            {
                                new AttachPoint()
                                {
                                    AnimLength = 2f,
                                    AnimPosChange = new Vector3(0f, 0.5f, -0.5f),
                                    DisableFreeJoint = true,
                                    Tangent = Vector3.back,
                                    apPos = new Vector3(0f, -0.5f, 0f),
                                    blockPos = new IntVector3(0, -1, 0),
                                    apDirForward = new IntVector3(0, -1, 0),
                                    apDirUp = IntVector3.forward
                                },
                                new AttachPoint()
                                {
                                    AnimLength = 2f,
                                    AnimPosChange = new Vector3(0f, -0.5f, 0.5f),
                                    DisableFreeJoint = true,
                                    Tangent = Vector3.down,
                                    apPos = new Vector3(0f, 0f, -0.5f),
                                    blockPos = new IntVector3(0, 0, -1),
                                    apDirForward = new IntVector3(0, 0, -1),
                                    apDirUp = IntVector3.up
                                }
                            };

                            ControlBlock.RegisterLater();
                        }

                        {
                            var ControlBlock = new BlockPrefabBuilder(BlockTypes.BF_Block_111)
                                .SetName("Better Future Rail large wedge segment")
                                .SetDescription("(Travel length: 4) A curved segment for the Better Future Rail Piston, add it to the end of the line to make it bend!")
                                .SetBlockID(1393802)//, "f63931ef3e14ba8e")
                                .SetFaction(FactionSubTypes.BF)
                                .SetCategory(BlockCategories.Control)
                                .SetGrade(0)
                                .SetPrice(4212)
                                .SetHP(1200)
                                .SetDetachFragility(0.5f)
                                .SetMass(4.5f)
                                .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.bf_rail_segment_streamline_large)))
                                .SetRecipe(ChunkTypes.HardenedTitanic, ChunkTypes.HardenedTitanic, ChunkTypes.HardenedTitanic, ChunkTypes.HardenedTitanic, ChunkTypes.CelestianCrystal, ChunkTypes.CelestianCrystal, ChunkTypes.CelestianCrystal, ChunkTypes.CelestianCrystal)
                                .SetSize(new IntVector3(1, 2, 2))
                                .SetAPsManual(new Vector3[]{
                                    new Vector3(0f, -0.5f, 0f), new Vector3(0f, -0.5f, 1f),
                                    new Vector3(-0.5f, 0f, 0f), new Vector3(-0.5f, 0f, 1f), new Vector3(-0.5f, 1f, 0f),
                                    new Vector3(0.5f, 0f, 0f),  new Vector3(0.5f, 0f, 1f),  new Vector3(0.5f, 1f, 0f),
                                    new Vector3(0f, 0f, -0.5f), new Vector3(0f, 1f, -0.5f), })
                                .AddComponent<ModuleBMSegment>(out ModuleBMSegment segment);

                            var par = ControlBlock.Prefab.transform;

                            AddMeshToBlockMover(bfmat, new Vector3(1f, 2f, 2f), new Vector3(0f, 0.5f, 0.5f), par, Properties.Resources.bf_rail_piston_wedge_2);

                            segment.blockMoverHeadType = railID;
                            segment.AnimWeight = 0.2f;
                            segment.APs = new AttachPoint[2]
                            {
                                new AttachPoint()
                                {
                                    AnimLength = 4f,
                                    AnimPosChange = new Vector3(0f, 1.5f, -1.5f),
                                    DisableFreeJoint = true,
                                    Tangent = Vector3.back,
                                    apPos = new Vector3(0f, -0.5f, 1f),
                                    blockPos = new IntVector3(0, -1, 1),
                                    apDirForward = new IntVector3(0, -1, 0),
                                    apDirUp = IntVector3.forward
                                },
                                new AttachPoint()
                                {
                                    AnimLength = 4f,
                                    AnimPosChange = new Vector3(0f, -1.5f, 1.5f),
                                    DisableFreeJoint = true,
                                    Tangent = Vector3.down,
                                    apPos = new Vector3(0f, 1f, -0.5f),
                                    blockPos = new IntVector3(0, 1, -1),
                                    apDirForward = new IntVector3(0, 0, -1),
                                    apDirUp = IntVector3.up
                                }
                            };

                            ControlBlock.RegisterLater();
                        }

                        {
                            var ControlBlock = new BlockPrefabBuilder(BlockTypes.BF_Block_111)
                                .SetName("Better Future Rail corner segment")
                                .SetDescription("(Travel length: 1) A curved segment for the Better Future Rail Piston, add it to the end of the line to make it twist!")
                                .SetBlockID(1393803)//, "f63931ef3e14ba8e")
                                .SetFaction(FactionSubTypes.BF)
                                .SetCategory(BlockCategories.Control)
                                .SetGrade(0)
                                .SetPrice(2106)
                                .SetHP(400)
                                .SetDetachFragility(0.5f)
                                .SetMass(1.5f)
                                .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.bf_rail_segment_corner)))
                                .SetRecipe(ChunkTypes.HardenedTitanic, ChunkTypes.HardenedTitanic, ChunkTypes.CelestianCrystal, ChunkTypes.CelestianCrystal)
                                .SetSize(new IntVector3(1, 1, 1))
                                .SetAPsManual(new Vector3[]{
                                    Vector3.down * 0.5f,
                                    Vector3.right * 0.5f,
                                    Vector3.left * 0.5f,
                                    Vector3.up * 0.5f,
                                    Vector3.back * 0.5f
                                })
                                .AddComponent<ModuleBMSegment>(out ModuleBMSegment segment);

                            var par = ControlBlock.Prefab.transform;

                            AddMeshToBlockMover(bfmat, new Vector3(1f, 1f, 1f), Vector3.zero, par, Properties.Resources.bf_rail_piston_corner);

                            segment.blockMoverHeadType = railID;
                            segment.AnimWeight = 0.25f;
                            segment.APs = new AttachPoint[2]
                            {
                                new AttachPoint()
                                {
                                    AnimLength = 1f,
                                    AnimPosChange = new Vector3(0.5f, 0.5f, 0f),
                                    DisableFreeJoint = true,
                                    Tangent = Vector3.right,
                                    apPos = new Vector3(0f, -0.5f, 0f),
                                    blockPos = new IntVector3(0, -1, 0),
                                    apDirForward = new IntVector3(0, -1, 0),
                                    apDirUp = IntVector3.forward
                                },
                                new AttachPoint()
                                {
                                    AnimLength = 1f,
                                    AnimPosChange = new Vector3(-0.5f, -0.5f, 0f),
                                    DisableFreeJoint = true,
                                    Tangent = Vector3.down,
                                    apPos = new Vector3(0.5f, 0f, 0f),
                                    blockPos = new IntVector3(1, 0, 0),
                                    apDirForward = new IntVector3(1, 0, 0),
                                    apDirUp = IntVector3.forward
                                }
                            };

                            ControlBlock.RegisterLater();
                        }

                        {
                            var ControlBlock = new BlockPrefabBuilder(BlockTypes.BF_Block_111)
                                .SetName("Better Future Rail large corner segment")
                                .SetDescription("(Travel length: 3) A curved segment for the Better Future Rail Piston, add it to the end of the line to make it twist!")
                                .SetBlockID(1393804)//, "f63931ef3e14ba8e")
                                .SetFaction(FactionSubTypes.BF)
                                .SetCategory(BlockCategories.Control)
                                .SetGrade(0)
                                .SetPrice(3879)
                                .SetHP(1200)
                                .SetDetachFragility(0.5f)
                                .SetMass(4.5f)
                                .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.bf_rail_segment_corner_large)))
                                .SetRecipe(ChunkTypes.HardenedTitanic, ChunkTypes.HardenedTitanic, ChunkTypes.HardenedTitanic, ChunkTypes.CelestianCrystal, ChunkTypes.CelestianCrystal, ChunkTypes.CelestianCrystal, ChunkTypes.CelestianCrystal)
                                .SetSize(new IntVector3(2, 2, 1))
                                .SetAPsManual(new Vector3[]{
                                    new Vector3(0f, -0.5f, 0f), new Vector3(1f, -0.5f, 0f), // bottom
                                    new Vector3(-0.5f, 0f, 0f), new Vector3(-0.5f, 1f, 0f), // left
                                    new Vector3(1.5f, 0f, 0f),  new Vector3(1.5f, 1f, 0f), // right
                                    new Vector3(0f, 1.5f, 0f), new Vector3(1f, 1.5f, 0f),
                                    new Vector3(0f, 0f, -0.5f), new Vector3(0f, 1f, -0.5f), new Vector3(1f, 0f, -0.5f), new Vector3(1f, 1f, -0.5f), })
                                .AddComponent<ModuleBMSegment>(out ModuleBMSegment segment);

                            var par = ControlBlock.Prefab.transform;

                            AddMeshToBlockMover(bfmat, new Vector3(2f, 2f, 1f), new Vector3(0.5f, 0.5f, 0f), par, Properties.Resources.bf_rail_piston_corner_2);

                            segment.blockMoverHeadType = railID;
                            segment.AnimWeight = 0.27f;
                            segment.APs = new AttachPoint[2]
                            {
                                new AttachPoint()
                                {
                                    AnimLength = 3f,
                                    AnimPosChange = new Vector3(1.5f, 1.5f, 0f),
                                    DisableFreeJoint = true,
                                    Tangent = Vector3.right,
                                    apPos = new Vector3(0f, -0.5f, 0f),
                                    blockPos = new IntVector3(0, -1, 0),
                                    apDirForward = new IntVector3(0, -1, 0),
                                    apDirUp = IntVector3.forward
                                },
                                new AttachPoint()
                                {
                                    AnimLength = 3f,
                                    AnimPosChange = new Vector3(-1.5f, -1.5f, 0f),
                                    DisableFreeJoint = true,
                                    Tangent = Vector3.down,
                                    apPos = new Vector3(1.5f, 1f, 0f),
                                    blockPos = new IntVector3(2, 1, 0),
                                    apDirForward = new IntVector3(1, 0, 0),
                                    apDirUp = IntVector3.forward
                                }
                            };

                            ControlBlock.RegisterLater();
                        }
                        #endregion BetterFuture Rail Piston

                        #region Decorational Pistons
                        {
                            var ControlBlock = new BlockPrefabBuilder(BlockTypes.HE_ArmouredBlock_10_111)
                                .SetName("Hawkeye Armoured Panel Gate")
                                .SetDescription("A conveniently compactable set of extendable panels for armoring whatever you want to hide behind a series of whatever conditions are most convenient. There's not much else to it" + FakeMoverText)
                                .SetBlockID(6194711)
                                .SetFaction(FactionSubTypes.HE)
                                .SetCategory(BlockCategories.Accessories)
                                .SetGrade(2)
                                .SetPrice(3513)
                                .SetHP(3500)
                                .SetMass(2f)
                                .SetCenterOfMass(new Vector3(0f, 0f, -0.4f))
                                .SetDamageableType(ManDamage.DamageableType.Armour)
                                .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.pistonblock_he_panel_deco)));

                            var mat = GameObjectJSON.GetObjectFromGameResources<Material>("HE_Main");
                            var par = ControlBlock.Prefab.transform;

                            AddMeshToBlockMover(mat, new Vector3(1f, 1f, .2f), new Vector3(0f, 0f, -0.4f), par, Properties.Resources.HEps_deco_base);
                            AddMeshToBlockMover(mat, new Vector3(1f, 1f, .1f), new Vector3(0f, 0f, -0.4f), par, Properties.Resources.HEps_deco_shaft);
                            AddMeshToBlockMover(mat, new Vector3(1f, 1f, .2f), new Vector3(0f, 0f, -0.4f), par, Properties.Resources.HEps_deco_head);

                            ControlBlock.SetSizeManual(new IntVector3[] {
                                    new IntVector3(0,0,0)
                                }, new Vector3[]{
                                    new Vector3(0f,0f,-.5f)
                                }).AddComponent<ModuleBlockMover>(SetHawkeyePanelDecoPiston)
                                .SetRecipe(ChunkTypes.RubberBrick, ChunkTypes.SensoryTransmitter, ChunkTypes.PlubonicGreebles, ChunkTypes.HardenedTitanic)
                                .RegisterLater();
                        }
                        #endregion
                    }
                    #endregion Special Pistons
                }
                #endregion Pistons

                #region Swivels
                {
                    #region GSO Medium Swivel
                    {
                        var ControlBlock = new BlockPrefabBuilder(BlockTypes.GSOBlock_111)
                            .SetName("GSO Medium Embedded Swivel")
                            .SetDescription("A cheap & light configurable swivel that can rotate blocks on a tech. It's like the small embedded swivel, but medium. The head for this one rests on the top, safely inside the base." + MoverText)
                            .SetBlockID(1393838)
                            .SetFaction(FactionSubTypes.GSO)
                            .SetCategory(BlockCategories.Control)
                            .SetGrade(2)
                            .SetPrice(4470)
                            .SetHP(2000)
                            .SetMass(2.5f)
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.swivel_png)));

                        var mat = GameObjectJSON.GetObjectFromGameResources<Material>("GSO_Main");
                        var par = ControlBlock.Prefab.transform;

                        AddMeshToBlockMover(mat, new Vector3(2f, .9f, 2f), new Vector3(.5f, -.05f, .5f), par, Properties.Resources.swivel_base);
                        AddMeshToBlockMover(mat, new Vector3(1.4f, .1f, 1.4f), new Vector3(0f, .45f, 0f), par, Properties.Resources.swivel_head, new Vector3(.5f, 0f, .5f));
                        //gimbal.aimClampMaxPercent = 360;
                        //gimbal.rotationAxis = GimbalAimer.AxisConstraint.Y;

                        ControlBlock.SetSize(new IntVector3(2, 1, 2), BlockPrefabBuilder.AttachmentPoints.All)
                            .AddComponent<ModuleBlockMover>(SetMediumSwivel)
                            .RegisterLater();

                        CustomRecipe.RegisterRecipe(
                            new CustomRecipe.RecipeInput[]
                            {
                    new CustomRecipe.RecipeInput((int)ChunkTypes.FuelInjector, 1),
                    new CustomRecipe.RecipeInput((int)ChunkTypes.SensoryTransmitter, 1),
                    new CustomRecipe.RecipeInput((int)ChunkTypes.PlubonicAlloy, 1),
                            },
                            new CustomRecipe.RecipeOutput[]
                            {
                    new CustomRecipe.RecipeOutput(1393838)
                            });
                    }
                    #endregion GSO Medium Swivel

                    #region VEN Inline Swivel
                    {
                        var ControlBlock = new BlockPrefabBuilder(BlockTypes.VENBlock_111)
                            .SetName("Venture Inline Swivel")
                            .SetDescription("A swivel which's center disk rotates blocks, while it is held by the top or bottom. And it's fast, too. That's because it's packed full of dense planetary motors, that have the incredible capability of breaking all known laws of thermodynamics" + MoverText)
                            .SetBlockID(1393837)
                            .SetFaction(FactionSubTypes.VEN)
                            .SetCategory(BlockCategories.Control)
                            .SetGrade(2)
                            .SetPrice(6237)
                            .SetHP(2000)
                            .SetMass(3f)
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.swivel_ven_png)))
                            .SetRecipe(ChunkTypes.FuelInjector, ChunkTypes.FuelInjector, ChunkTypes.SensoryTransmitter, ChunkTypes.PlubonicAlloy, ChunkTypes.PlubonicAlloy);

                        var mat = GameObjectJSON.GetObjectFromGameResources<Material>("VEN_Main");
                        var par = ControlBlock.Prefab.transform;

                        AddColliderToGameObject(
                            AddMeshToBlockMover(mat, new Vector3(.95f, .3f, .95f), new Vector3(0f, -.35f, 0f), par, Properties.Resources.swivel_ven_base),
                            new Vector3(.95f, .3f, .95f), new Vector3(0f, .35f, 0f), false
                        );
                        AddColliderToGameObject(
                            AddMeshToBlockMover(mat, Vector3.zero, par, Properties.Resources.swivel_ven_head),
                            Vector3.one * .9f, Vector3.zero, true
                        );
                        //gimbal.aimClampMaxPercent = 360;
                        //gimbal.rotationAxis = GimbalAimer.AxisConstraint.Y;

                        ControlBlock.SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.All)
                            .AddComponent<ModuleBlockMover>(SetInlineSwivel)
                            .RegisterLater();
                    }
                    #endregion VEN Inline Swivel

                    #region GSO Small Swivel
                    {
                        var ControlBlock = new BlockPrefabBuilder(BlockTypes.GSOBlock_111)
                            .SetName("GSO Small Embedded Swivel")
                            .SetDescription("It's like the medium embedded swivel, but smaller. Look at how small it is, it's like a macaroon cheeseburger cupcake! Good thing it isn't, or there'd be food all over." + MoverText)
                            .SetBlockID(1393836)
                            .SetFaction(FactionSubTypes.GSO)
                            .SetCategory(BlockCategories.Control)
                            .SetGrade(2)
                            .SetPrice(4455)
                            .SetHP(2000)
                            .SetMass(1.5f)
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.swivel_small_png)))
                            .SetRecipe(ChunkTypes.PlubonicGreebles, ChunkTypes.PlubonicGreebles, ChunkTypes.FuelInjector, ChunkTypes.SensoryTransmitter);

                        var mat = GameObjectJSON.GetObjectFromGameResources<Material>("GSO_Main");
                        var par = ControlBlock.Prefab.transform;

                        AddMeshToBlockMover(mat, new Vector3(.9f, .5f, .9f), new Vector3(0f, -.25f, 0f), par, Properties.Resources.swivel_small_base);
                        AddMeshToBlockMover(mat, new Vector3(.9f, .5f, .9f), new Vector3(0f, 0.25f, 0f), par, Properties.Resources.swivel_small_head);//.AddComponent<GimbalAimer>();
                                                                                                                                                      //gimbal.aimClampMaxPercent = 360;
                                                                                                                                                      //gimbal.rotationAxis = GimbalAimer.AxisConstraint.Y;

                        ControlBlock.SetSizeManual(new IntVector3[] { IntVector3.zero },
                            new Vector3[] { Vector3.down * .5f, Vector3.up * .5f })
                            .AddComponent<ModuleBlockMover>(SetSmallSwivel)
                            .RegisterLater();
                    }
                    #endregion GSO Small Swivel

                    #region HE Double Swivel
                    {
                        var ControlBlock = new BlockPrefabBuilder(BlockTypes.HE_StdBlock_Alt_1_02_111)
                            .SetName("Hawkeye Dual Rotor Swivel")
                            .SetDescription("A swivel with two heads at the top and bottom. They, uh, don't move separately, they're on the same axle. If one gets stuck, you will smell burning rubber.\n(Designed by Rafs!)" + MoverText)
                            .SetBlockID(1393835)//, "f74931ef3e14ba8e")
                            .SetFaction(FactionSubTypes.HE)
                            .SetCategory(BlockCategories.Control)
                            .SetGrade(2)
                            .SetPrice(5526)
                            .SetHP(2000)
                            .SetMass(2f)
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.swivelblock_he)))
                            .SetRecipe(ChunkTypes.PlubonicAlloy, ChunkTypes.PlubonicAlloy, ChunkTypes.FuelInjector, ChunkTypes.SensoryTransmitter);

                        var mat = GameObjectJSON.GetObjectFromGameResources<Material>("HE_Main");
                        var par = ControlBlock.Prefab.transform;

                        AddMeshToBlockMover(mat, new Vector3(1f, .8f, 1f), Vector3.zero, par, Properties.Resources.swivel_double_base);
                        AddMeshToBlockMover(mat, new Vector3(.5f, 1f, .5f), Vector3.zero, par, Properties.Resources.swivel_double_head);//.AddComponent<GimbalAimer>();
                                                                                                                                        //gimbal.aimClampMaxPercent = 360;
                                                                                                                                        //gimbal.rotationAxis = GimbalAimer.AxisConstraint.Y;

                        ControlBlock.SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.All)
                            .AddComponent<ModuleBlockMover>(SetDoubleSwivel)
                            .RegisterLater();
                    }
                    #endregion GSO Medium Swivel

                    #region RR Medium Inline Swivel
                    {
                        var ControlBlock = new BlockPrefabBuilder(BlockTypes.GSOBlock_111)
                            .SetName("R.R. Medium Inline Swivel")
                            .SetDescription("Instead of a massive stepper motor, we lined the inside of this swivel with hovers. No one really knows why it works, but it's an incredible breakthrough nonetheless.\n(Designed by Rafs!)" + MoverText)
                            .SetBlockID(29571436)
                            .SetFaction(FactionSubTypes.EXP)
                            .SetCategory(BlockCategories.Control)
                            .SetGrade(0)
                            .SetPrice(10000)
                            .SetHP(2000)
                            .SetMass(4f)
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.rr_inline_swivel)));

                        var mat = GameObjectJSON.GetObjectFromGameResources<Material>("RR_Main");
                        var par = ControlBlock.Prefab.transform;

                        AddMeshToBlockMover(mat, new Vector3(1.5f, 1f, 1.5f), new Vector3(.5f, 0f, .5f), par, Properties.Resources.swivel_rr_base);
                        AddMeshToBlockMover(mat, new Vector3(2f, .5f, 2f), new Vector3(0f, 0f, 0f), par, Properties.Resources.swivel_rr_head, new Vector3(.5f, 0f, .5f));
                        //gimbal.aimClampMaxPercent = 360;
                        //gimbal.rotationAxis = GimbalAimer.AxisConstraint.Y;

                        ControlBlock.SetSize(new IntVector3(2, 1, 2), BlockPrefabBuilder.AttachmentPoints.All)
                            .AddComponent<ModuleBlockMover>(SetMediumInlineSwivel)
                            .RegisterLater();
                    }
                    #endregion GSO Medium Swivel
                }
                #endregion Swivels

                #region Steering Regulator
                {
                    var SteeringRegulator = new BlockPrefabBuilder(BlockTypes.BF_Block_111)
                        .SetName("Inverse Drive Regulator")
                        .SetDescription("This block got tired of being pushed around by the fancy new BF stabilizer computer, so it decided to 'take the wheel' in a more literal sense...")
                        .SetBlockID(1293839)
                        .SetFaction(FactionSubTypes.BF)
                        .SetCategory(BlockCategories.Accessories)
                        .SetGrade(0)
                        .SetPrice(3467)
                        .SetHP(200)
                        .SetMass(3.5f)
                        .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.sr), true, GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main", true))
                        .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.sr_png)))
                        .SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[]{
                            Vector3.down * 0.5f,
                            Vector3.left * 0.5f,
                            Vector3.right * 0.5f,
                            Vector3.forward * 0.5f,
                            Vector3.back * 0.5f }
                        );
                        // .AddComponent<ModuleSteeringRegulator>();

                    TankBlock baseBlock = SteeringRegulator.TankBlock;
                    ModulePID addedPID = baseBlock.gameObject.AddComponent<ModulePID>() as ModulePID;

                    addedPID.AddParameters(PIDController.GenerateParameterInstance(PIDController.PIDParameters.PIDAxis.Accel, 2f, 0.1f, 5f, false, true));
                    addedPID.AddParameters(PIDController.GenerateParameterInstance(PIDController.PIDParameters.PIDAxis.Strafe, 2f, 0.1f, 5f, false, true));
                    addedPID.enableHoldPosition = true;

                    SteeringRegulator.RegisterLater();

                    CustomRecipe.RegisterRecipe(
                        new CustomRecipe.RecipeInput[]
                        {
                            new CustomRecipe.RecipeInput((int)ChunkTypes.MotherBrain, 1),
                            new CustomRecipe.RecipeInput((int)ChunkTypes.ThermoJet, 1),
                            new CustomRecipe.RecipeInput((int)ChunkTypes.FibrePlating, 2),
                        },
                        new CustomRecipe.RecipeOutput[]
                        {
                            new CustomRecipe.RecipeOutput(1293839)
                        }
                    );
                }
                #endregion Steering Regulator

                #region Pads
                {
                    #region GC Small Pad
                    {
                        var FrictionPad = new BlockPrefabBuilder(BlockTypes.GCBlock_222)
                            .SetName("Small Friction Pad")
                            .SetDescription("Nice and grippy. Little sticky. Will break reality if used improperly")
                            .SetBlockID(1293831)
                            .SetDetachFragility(0f)
                            .SetDamageableType(ManDamage.DamageableType.Rubber)
                            .SetFaction(FactionSubTypes.GC)
                            .SetCategory(BlockCategories.Wheels)
                            .SetGrade(1)
                            .SetPrice(500)
                            .SetHP(625)
                            .SetMass(0.5f)
                            .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.GCfrictionpadsmall), true, GameObjectJSON.GetObjectFromGameResources<Material>("GC_Main", true))
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.friction_pad_gc_small_png)))
                            .SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[] { Vector3.up * 0.5f })
                            .AddComponent<ModuleFrictionPad>(SetGCSmallPad);

                        var trigger = FrictionPad.Prefab.gameObject.AddComponent<BoxCollider>();
                        trigger.isTrigger = true;

                        trigger.size = new Vector3(0.925f, 0.5f, 0.925f);
                        trigger.center = Vector3.up * 0.25f;

                        FrictionPad.RegisterLater();

                        CustomRecipe.RegisterRecipe(
                            new CustomRecipe.RecipeInput[]
                            {
                    new CustomRecipe.RecipeInput((int)ChunkTypes.RubberBrick, 4),
                            },
                            new CustomRecipe.RecipeOutput[]
                            {
                    new CustomRecipe.RecipeOutput(1293831)
                            });
                    }
                    #endregion GC Small Pad

                    #region GC Large Pad
                    {
                        var FrictionPad = new BlockPrefabBuilder(BlockTypes.GCBlock_222)
                            .SetName("Non Slip-A-Tron 3000")
                            .SetDescription("'Name by /-Shido, Shido named this block. Who, what?\n<i>What where am I-I need an adult</i>")
                            .SetBlockID(1293830)
                            .SetDetachFragility(0f)
                            .SetDamageableType(ManDamage.DamageableType.Rubber)
                            .SetFaction(FactionSubTypes.GC)
                            .SetCategory(BlockCategories.Wheels)
                            .SetGrade(1)
                            .SetPrice(2000)
                            .SetHP(2500)
                            .SetMass(2f)
                            .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.GCfrictionpadbig), true, GameObjectJSON.GetObjectFromGameResources<Material>("GC_Main", true))
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.friction_pad_gc_big_png)))
                            .SetSizeManual(new IntVector3[] { IntVector3.zero, IntVector3.forward, IntVector3.right, new IntVector3(1, 0, 1) }, new Vector3[] { Vector3.up * 0.5f, new Vector3(1f, 0.5f, 0f), new Vector3(1f, 0.5f, 1f), new Vector3(0f, 0.5f, 1f), })
                            .AddComponent<ModuleFrictionPad>(SetGCBigPad);

                        var trigger = FrictionPad.Prefab.gameObject.AddComponent<BoxCollider>();
                        trigger.isTrigger = true;

                        trigger.size = new Vector3(1.9f, 1f, 1.9f);
                        trigger.center = new Vector3(1f, 0f, 1f);

                        FrictionPad.RegisterLater();

                        CustomRecipe.RegisterRecipe(
                            new CustomRecipe.RecipeInput[]
                            {
                    new CustomRecipe.RecipeInput((int)ChunkTypes.RubberBrick, 16),
                            },
                            new CustomRecipe.RecipeOutput[]
                            {
                    new CustomRecipe.RecipeOutput(1293831)
                            });
                    }
                    #endregion GC Large Pad
                }
                #endregion Pads

                #region MultiTech Magnets
                {
                    #region Fixed MTMag
                    {
                        var mtmag = new BlockPrefabBuilder(BlockTypes.BF_Block_111)
                            .SetName("FixedJoint MTMag")
                            .SetDescription("Use this with another of its kind on a separate tech to lock them together through the power of PHYSICS!\n\n")
                            .SetBlockID(1293700)
                            .SetFaction(FactionSubTypes.BF)
                            .SetCategory(BlockCategories.Accessories)
                            .SetGrade(2)
                            .SetPrice(500)
                            .SetHP(600)
                            .SetMass(1f)
                            .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.mtmag_fixed), false, GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main", false))
                            //.SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.mtmag_fixed_png)))
                            .SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[] { Vector3.up * -0.5f })
                            .AddComponent<ModuleMTMagnet>(SetFixedMTMag);

                        AddFrictionlessCollider(new Vector3(1f, 0.5f, 1f), Vector3.down * 0.25f, mtmag.Prefab.transform);

                        var trigger = mtmag.Prefab.gameObject.AddComponent<BoxCollider>();
                        trigger.isTrigger = true;

                        trigger.size = new Vector3(0.5f, 0.4f, 0.5f);
                        trigger.center = Vector3.zero;

                        mtmag.RegisterLater();

                        //CustomRecipe.RegisterRecipe(
                        //    new CustomRecipe.RecipeInput[]
                        //    {
                        //    new CustomRecipe.RecipeInput((int)ChunkTypes.RubberBrick, 4),
                        //    },
                        //    new CustomRecipe.RecipeOutput[]
                        //    {
                        //    new CustomRecipe.RecipeOutput(/*Edit*/)
                        //    });
                    }
                    #endregion Fixed MTMag

                    #region Ball MTMag
                    {
                        var mtmag = new BlockPrefabBuilder(BlockTypes.BF_Block_111)
                            .SetName("BallJoint MTMag")
                            .SetDescription("Use this with another of its kind on a separate tech to lock them together through the power of PHYSICS!\nThis is literally the best joint. In terms of stability and reliability. Now if only it were bigger...")
                            .SetBlockID(1293701)
                            .SetFaction(FactionSubTypes.BF)
                            .SetCategory(BlockCategories.Accessories)
                            .SetGrade(1)
                            .SetPrice(800)
                            .SetHP(600)
                            .SetMass(1f)
                            .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.mtmag_ball), GameObjectJSON.MeshFromData(Properties.Resources.mtmag_ball_collider), true, GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main", false))
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.mtmag_ball_png)))
                            .SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[] { Vector3.up * -0.5f })
                            .AddComponent<ModuleMTMagnet>(SetBallMTMag);

                        var trigger = mtmag.Prefab.gameObject.AddComponent<BoxCollider>();
                        trigger.isTrigger = true;

                        trigger.size = new Vector3(.8f, .7f, .8f);
                        trigger.center = Vector3.up * 0.2f;

                        mtmag.RegisterLater();

                        CustomRecipe.RegisterRecipe(
                            new CustomRecipe.RecipeInput[]
                            {
                    new CustomRecipe.RecipeInput((int)ChunkTypes.PlubonicGreebles, 3),
                            },
                            new CustomRecipe.RecipeOutput[]
                            {
                    new CustomRecipe.RecipeOutput(1293701)
                            });
                    }
                    #endregion Ball MTMag

                    #region Large Ball MTMag
                    {
                        var mtmag = new BlockPrefabBuilder(BlockTypes.BF_Block_111)
                            .SetName("BallJoint Large MTMag")
                            .SetDescription("Use this with another of its kind on a separate tech to lock them together through the power of PHYSICS!\nA bigger version of the best joint in all of existence!")
                            .SetBlockID(1293703)
                            .SetFaction(FactionSubTypes.BF)
                            .SetCategory(BlockCategories.Accessories)
                            .SetGrade(1)
                            .SetPrice(1500)
                            .SetHP(600)
                            .SetMass(1f)
                            .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.mtmag_ball_large), GameObjectJSON.MeshFromData(Properties.Resources.mtmag_ball_collider_large), true, GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main", false))
                            .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.mtmag_ball_large_png)))
                            .SetSize(new IntVector3(2, 2, 2), BlockPrefabBuilder.AttachmentPoints.Bottom)
                            .AddComponent<ModuleMTMagnet>(SetLargeBallMTMag);

                        var trigger = mtmag.Prefab.gameObject.AddComponent<BoxCollider>();
                        trigger.isTrigger = true;

                        trigger.size = new Vector3(1.5f, 1.5f, 1.5f);
                        trigger.center = new Vector3(0.5f, 0.5f, 0.5f);

                        mtmag.RegisterLater();

                        CustomRecipe.RegisterRecipe(
                            new CustomRecipe.RecipeInput[]
                            {
                    new CustomRecipe.RecipeInput((int)ChunkTypes.PlubonicAlloy, 4),
                            },
                            new CustomRecipe.RecipeOutput[]
                            {
                    new CustomRecipe.RecipeOutput(1293703)
                            });
                    }
                    #endregion Large Ball MTMag

                    #region Large Swivel MTMag
                    {
                        var mtmag = new BlockPrefabBuilder(BlockTypes.BF_Block_111)
                            .SetName("SwivelJoint Large MTMag")
                            .SetDescription("Use this with another of its kind on a separate tech to lock them together through the power of PHYSICS!")
                            .SetBlockID(1293702)
                            .SetFaction(FactionSubTypes.BF)
                            .SetCategory(BlockCategories.Accessories)
                            .SetGrade(2)
                            .SetPrice(500)
                            .SetHP(600)
                            .SetMass(3f)
                            .SetSize(new IntVector3(2, 1, 2), BlockPrefabBuilder.AttachmentPoints.Bottom)
                            .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.mtmag_swivel_large), false, GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main", false))
                            //.SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.mtmag_fixed_png)))
                            .AddComponent<ModuleMTMagnet>(SetLargeSwivelMTMag);

                        AddFrictionlessCollider(new Vector3(2f, 1f, 2f), new Vector3(0.5f, 0f, 0.5f), mtmag.Prefab.transform);

                        var trigger = mtmag.Prefab.gameObject.AddComponent<BoxCollider>();
                        trigger.isTrigger = true;

                        trigger.size = new Vector3(1f, 1f, 1f);
                        trigger.center = new Vector3(0.5f, 0.5f, 0.5f);

                        mtmag.RegisterLater();

                        //CustomRecipe.RegisterRecipe(
                        //    new CustomRecipe.RecipeInput[]
                        //    {
                        //    new CustomRecipe.RecipeInput((int)ChunkTypes.RubberBrick, 4),
                        //    },
                        //    new CustomRecipe.RecipeOutput[]
                        //    {
                        //    new CustomRecipe.RecipeOutput(/*Edit*/)
                        //    });
                    }
                    #endregion Large Swivel MTMag
                }
                #endregion MultiTech Magnets

                #region HoverPID
                {
                    var bf_PID_controller = new BlockPrefabBuilder("BF_Stabiliser_Computer_111")
                        .SetName("BF Hover PID 2.0")
                        .SetDescription("New desc.")
                        .SetBlockID(10998)
                        .SetFaction(FactionSubTypes.BF)
                        .SetCategory(BlockCategories.Accessories)
                        .SetGrade(0)
                        .SetPrice(3467)
                        .SetHP(200)
                        .SetMass(3.5f)
                        .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.BF_Flight_Computer), true, bf_mat)
                        .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.BF_Hover_PID_png)))
                        .SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[]{
                        Vector3.down * 0.5f});

                    TankBlock baseBlock = bf_PID_controller.TankBlock;
                    ModulePID addedPID = baseBlock.gameObject.AddComponent<ModulePID>() as ModulePID;

                    addedPID.AddParameters(PIDController.GenerateParameterInstance(PIDController.PIDParameters.PIDAxis.Hover, 300f, 10f, 600f, false, true));
                    addedPID.enableHoldPosition = true;
                    addedPID.manualTargetChangeRate = 1.0f;
                    addedPID.useTargetHeight = true;

                    Mesh spinnerMesh = GameObjectJSON.MeshFromData(Properties.Resources.BF_Flight_Computer_Spinner);
                    GameObject spindleChild = baseBlock.gameObject.FindChildGameObject("_spindle");
                    GameObject ringObject = spindleChild.FindChildGameObject("m_BF_Stabiliser_Computer_111_Ring");
                    ringObject.AddComponent<MeshFilter>().sharedMesh = spinnerMesh;
                    ringObject.AddComponent<MeshRenderer>().sharedMaterial = bf_mat;

                    CustomRecipe.RegisterRecipe(
                        new CustomRecipe.RecipeInput[]
                        {
                            new CustomRecipe.RecipeInput((int)ChunkTypes.MotherBrain, 1),
                            new CustomRecipe.RecipeInput((int)ChunkTypes.ThermoJet, 1),
                            new CustomRecipe.RecipeInput((int)ChunkTypes.FibrePlating, 2),
                        },
                        new CustomRecipe.RecipeOutput[]
                        {
                            new CustomRecipe.RecipeOutput(10998)
                        }
                    );

                    bf_PID_controller.RegisterLater();
                }
                #endregion HoverPID
            }
            #endregion Blocks

            GameObject _holder = new GameObject();
            //_holder.AddComponent<OptionMenuPiston>();
            //_holder.AddComponent<OptionMenuSwivel>();
            _holder.AddComponent<OptionMenuSteeringRegulator>();
            _holder.AddComponent<OptionMenuMover>();
            _holder.AddComponent<OptionMenuHoverPID>();
            _holder.AddComponent<LogGUI>();
            _holder.AddComponent<AdjustAttachPosition>();
            new GameObject().AddComponent<GUIOverseer>();
            ManWorldTreadmill.inst.OnBeforeWorldOriginMove.Subscribe(WorldShift);
            UnityEngine.Object.DontDestroyOnLoad(_holder);

            ModuleBlockMover.InitiateNetworking();

            var harmony = HarmonyInstance.Create("aceba1.controlblocks");
            harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
        }


        internal static bool PistonHeart = false;

        internal static void WorldShift()
        {
            PistonHeart = !PistonHeart;
        }

        #region PrefabBaker

        static PhysicMaterial Frictionless = new PhysicMaterial() { dynamicFriction = 0, frictionCombine = PhysicMaterialCombine.Maximum, staticFriction = 0.3f },
                              Normal = new PhysicMaterial();

        internal static GameObject AddFrictionlessCollider(Vector3 colliderSize, Vector3 colliderOffset, Transform par)
        {
            GameObject sub = new GameObject("Frictionless Collider") { layer = Globals.inst.layerTank };

            var mhc = sub.AddComponent<BoxCollider>();
            mhc.size = colliderSize;
            mhc.center = colliderOffset;
            mhc.sharedMaterial = Frictionless;
            sub.transform.SetParent(par);
            sub.transform.localPosition = Vector3.zero;
            sub.transform.localRotation = Quaternion.identity;
            return sub;
        }

        internal static void AddColliderToGameObject(GameObject toAddTo, Vector3 colliderSize, Vector3 colliderOffset, bool sphere)
        {
            if (sphere)
            {
                var mhc = toAddTo.AddComponent<SphereCollider>();
                mhc.radius = colliderSize.x / 2f;
                mhc.center = colliderOffset;
                mhc.sharedMaterial = Normal;
            }
            else
            {
                var mhc = toAddTo.AddComponent<BoxCollider>();
                mhc.size = colliderSize;
                mhc.center = colliderOffset;
                mhc.sharedMaterial = Normal;
            }
        }

        internal static GameObject AddMeshToBlockMover(Material mat, Vector3 colliderSize, Vector3 colliderOffset, Transform par, string Mesh, Vector3 actualPosition = default)
        {
            GameObject sub = new GameObject("BlockMover Part") { layer = Globals.inst.layerTank };
            sub.AddComponent<MeshFilter>().sharedMesh = GameObjectJSON.MeshFromData(Mesh);
            sub.AddComponent<MeshRenderer>().sharedMaterial = mat;

            var mhc = sub.AddComponent<BoxCollider>();
            mhc.size = colliderSize;
            mhc.center = colliderOffset;
            mhc.sharedMaterial = Normal;
            sub.transform.SetParent(par);
            sub.transform.localPosition = actualPosition;
            sub.transform.localRotation = Quaternion.identity;
            return sub;
        }

        internal static GameObject AddMeshToBlockMover(Material mat, Vector3 objPos, Transform par, string Mesh)
        {
            GameObject sub = new GameObject("BlockMover Part")
            {
                layer = Globals.inst.layerTank
            };
            sub.AddComponent<MeshFilter>().sharedMesh = GameObjectJSON.MeshFromData(Mesh);
            sub.AddComponent<MeshRenderer>().sharedMaterial = mat;
            sub.transform.SetParent(par);
            sub.transform.localPosition = objPos;
            sub.transform.localRotation = Quaternion.identity;
            return sub;
        }

        #endregion PrefabBaker

        #region SetBlockData

        #region MultiMagnets

        internal static void SetFixedMTMag(ModuleMTMagnet mtmag)
        {
            mtmag.Identity = ModuleMTMagnet.MTMagTypes.Fixed;
            mtmag.TransformCorrection = 0.5f;
            mtmag.VelocityCorrection = 0.5f;
            mtmag.ConfigureNewJoint = new Action<ModuleMTMagnet, ModuleMTMagnet>(CFixedJoint);
        }
        internal static void CFixedJoint(ModuleMTMagnet origin, ModuleMTMagnet body)
        {
            var Joint = origin.block.tank.gameObject.AddComponent<ConfigurableJoint>();
            Joint.autoConfigureConnectedAnchor = false;
            Joint.anchor = origin.LocalPosWithEffector;
            Joint.connectedAnchor = body.LocalPosWithEffector;
            Joint.enableCollision = true;
            Joint.connectedBody = body.block.tank.rbody;
            Joint.xMotion = ConfigurableJointMotion.Locked;
            Joint.yMotion = ConfigurableJointMotion.Locked;
            Joint.zMotion = ConfigurableJointMotion.Locked;
            Joint.angularXMotion = ConfigurableJointMotion.Locked;
            Joint.angularYMotion = ConfigurableJointMotion.Locked;
            Joint.angularZMotion = ConfigurableJointMotion.Locked;
            origin.joint = Joint;
            //var thing = Joint.angularYZLimitSpring;
            //thing.damper = 8f;
            //thing.spring = 130;
            //var thing2 = Joint.angularXLimitSpring;
            //thing2.damper = 8f;
            //thing2.spring = 130;
            //var thing3 = Joint.angularYLimit;
        }
        internal static void SetLargeSwivelMTMag(ModuleMTMagnet mtmag)
        {
            mtmag.Identity = ModuleMTMagnet.MTMagTypes.Swivel;
            mtmag.TransformCorrection = 0.9f;
            mtmag.VelocityCorrection = 0.8f;
            mtmag.Effector = new Vector3(0.5f, 0.5f, 0.5f);
            mtmag.ConfigureNewJoint = new Action<ModuleMTMagnet, ModuleMTMagnet>(CSwivelJoint);
        }
        internal static void CSwivelJoint(ModuleMTMagnet origin, ModuleMTMagnet body)
        {
            var Joint = origin.block.tank.gameObject.AddComponent<ConfigurableJoint>();
            Joint.autoConfigureConnectedAnchor = false;
            Joint.anchor = origin.LocalPosWithEffector;
            Joint.axis = origin.transform.up;
            Joint.secondaryAxis = -body.transform.up;
            Joint.connectedAnchor = body.LocalPosWithEffector;
            Joint.enableCollision = true;
            Joint.connectedBody = body.block.tank.rbody;
            Joint.xMotion = ConfigurableJointMotion.Locked;
            Joint.yMotion = ConfigurableJointMotion.Locked;
            Joint.zMotion = ConfigurableJointMotion.Locked;
            Joint.angularXMotion = ConfigurableJointMotion.Free;
            Joint.angularYMotion = ConfigurableJointMotion.Locked;
            Joint.angularZMotion = ConfigurableJointMotion.Locked;
            origin.joint = Joint;
            //var thing = Joint.angularYZLimitSpring;
            //thing.damper = 8f;
            //thing.spring = 130;
            //var thing2 = Joint.angularXLimitSpring;
            //thing2.damper = 8f;
            //thing2.spring = 130;
            //var thing3 = Joint.angularYLimit;
        }
        internal static void SetLargeBallMTMag(ModuleMTMagnet mtmag)
        {
            mtmag.Identity = ModuleMTMagnet.MTMagTypes.LargeBall;
            mtmag.TransformCorrection = 0.9f;
            mtmag.VelocityCorrection = 0.9f;
            mtmag.Effector = new Vector3(0.5f, 0.5f, 0.5f);
            mtmag.ConfigureNewJoint = new Action<ModuleMTMagnet, ModuleMTMagnet>(CBallJoint);
        }
        internal static void SetBallMTMag(ModuleMTMagnet mtmag)
        {
            mtmag.Identity = ModuleMTMagnet.MTMagTypes.Ball;
            mtmag.TransformCorrection = 0.3f;
            mtmag.VelocityCorrection = 0.6f;
            mtmag.ConfigureNewJoint = new Action<ModuleMTMagnet, ModuleMTMagnet>(CBallJoint);
        }
        internal static void CBallJoint(ModuleMTMagnet origin, ModuleMTMagnet body)
        {
            var Joint = origin.block.tank.gameObject.AddComponent<ConfigurableJoint>();
            Joint.autoConfigureConnectedAnchor = false;
            Joint.anchor = origin.LocalPosWithEffector;
            Joint.connectedAnchor = body.LocalPosWithEffector;
            Joint.enableCollision = true;
            Joint.connectedBody = body.block.tank.rbody;
            Joint.xMotion = ConfigurableJointMotion.Locked;
            Joint.yMotion = ConfigurableJointMotion.Locked;
            Joint.zMotion = ConfigurableJointMotion.Locked;
            Joint.angularXMotion = ConfigurableJointMotion.Free;
            Joint.angularYMotion = ConfigurableJointMotion.Free;
            Joint.angularZMotion = ConfigurableJointMotion.Free;
            origin.joint = Joint;
        }

        #endregion

        #region Pistons

        internal static void SetGSOPiston(ModuleBlockMover piston)
        {
            piston.usePosCurves = true;
            piston.posCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, .375f, 0f, 0f)), //shaft
                new AnimationCurve(),

                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f)), //block top
                new AnimationCurve()
            };
            piston.PartCount = 2;
            piston.TrueMaxVELOCITY = 0.08f;
            piston.TrueLimitVALUE = 1f;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0)
            };
            piston.SFX = TechAudio.SFXType.GSODrillSmall;
            piston.SFXVolume = 1f;
        }
        internal static void SetGeoCorpPiston(ModuleBlockMover piston)
        {
            piston.usePosCurves = true;
            piston.posCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(2f, .5f, 0f, 0f)),
                new AnimationCurve(),
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(2f, 1.1f, 0f, 0f)),
                new AnimationCurve(),

                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(2f, 2f, 0f, 0f)),
                new AnimationCurve()
            };
            piston.PartCount = 3;
            piston.TrueMaxVELOCITY = 0.06f;
            piston.TrueLimitVALUE = 2;
            //piston.StretchModifier = 2; piston.MaxStr = 2;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,2,0),
                new IntVector3(0,2,1),
                new IntVector3(1,2,0),
                new IntVector3(1,2,1)
            };
            piston.SFX = TechAudio.SFXType.GCPlasmaCutter;
            piston.SFXVolume = 1f;
        }
        internal static void SetHawkeyePiston(ModuleBlockMover piston)
        {
            piston.usePosCurves = true;
            piston.posCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, .5f), new Keyframe(1f, .5f, .5f,  0f), new Keyframe(2f,  .5f,  0f,  0f), new Keyframe(3f,  .5f, 0f, 0f)), //shaft bottom
                new AnimationCurve(),
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, .5f), new Keyframe(1f, .5f, .5f, .5f), new Keyframe(2f,   1f, .5f,  0f), new Keyframe(3f,   1f, 0f, 0f)), //shaft mid bottom
                new AnimationCurve(),
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, .5f), new Keyframe(1f, .5f, .5f, .5f), new Keyframe(2f,   1f, .5f,  1f), new Keyframe(3f,   2f, 1f, 0f)), //shaft mid top
                new AnimationCurve(),
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, .5f), new Keyframe(1f, .5f, .5f,  1f), new Keyframe(2f, 1.5f,  1f,  1f), new Keyframe(3f, 2.5f, 1f, 0f)), //shaft top
                new AnimationCurve(),

                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 1f), new Keyframe(2f, 2f, 1f, 1f), new Keyframe(3f, 3f, 1f, 0f)), //block top
                new AnimationCurve()
            };
            piston.PartCount = 5;
            piston.TrueMaxVELOCITY = 0.075f;
            piston.TrueLimitVALUE = 3;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0),
                new IntVector3(0,0,-1)
            };
            piston.SFX = TechAudio.SFXType.GCTripleBore;
            piston.SFXVolume = 0.9f;
        }
        internal static void SetHawkeyePanelPiston(ModuleBlockMover piston)
        {
            piston.usePosCurves = true;
            piston.posCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,  0f,  0f, .5f), new Keyframe(1f, .5f, .5f,  0f), new Keyframe(2f, .5f,  0f,  0f)), //shaft bottom
                new AnimationCurve(),
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,  0f,  0f, .5f), new Keyframe(1f, .5f, .5f, .5f), new Keyframe(2f,  1f, .5f,  0f)), //shaft mid bottom
                new AnimationCurve(),
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,  0f,  0f, .5f), new Keyframe(1f, .5f, .5f,  1f), new Keyframe(2f,1.5f,  1f,  0f)), //shaft mid top
                new AnimationCurve(),

                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,  0f,  0f,  1f), new Keyframe(1f,  1f,  1f,  1f), new Keyframe(2f,  2f,  1f,  0f)), //block top
                new AnimationCurve()
            };
            piston.PartCount = 4;
            piston.TrueMaxVELOCITY = 0.075f;
            piston.TrueLimitVALUE = 2;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,-1)
            };
            piston.SFX = TechAudio.SFXType.GCTripleBore;
            piston.SFXVolume = 1f;
        }
        internal static void SetHawkeyePanelDecoPiston(ModuleBlockMover piston)
        {
            piston.CanOnlyBeLockJoint = true;
            piston.usePosCurves = true;
            piston.posCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,  0f,  0f, .6f), new Keyframe(1f, 1f, .6f,  0f), new Keyframe(2f, 1f,  0f,  0f)), //shaft bottom
                new AnimationCurve(),
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f,  0f, .6f), new Keyframe(1f, 1f, .6f, .6f), new Keyframe(2f,  2f, .6f,  0f)), //shaft mid bottom
                new AnimationCurve(),
            };
            piston.PartCount = 2;
            piston.TrueMaxVELOCITY = 0.075f;
            piston.TrueLimitVALUE = 2;
            piston.startblockpos = new IntVector3[0];
            piston.SFX = TechAudio.SFXType.GCTripleBore;
            piston.SFXVolume = 1f;
        }
        internal static void SetBFPiston(ModuleBlockMover piston)
        {
            piston.usePosCurves = true;
            piston.posCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f)), //block top
                new AnimationCurve()
            };
            piston.PartCount = 1;
            piston.TrueMaxVELOCITY = 0.15f;
            piston.TrueLimitVALUE = 1f;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0)
            };
            piston.SFX = TechAudio.SFXType.FlameThrowerPlasma;
            piston.SFXVolume = 1f;
        }
        internal static void SetVENPiston(ModuleBlockMover piston)
        {
            piston.usePosCurves = true;
            piston.posCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 0.3f, 0.3f, 0.3f), new Keyframe(2f, 0.6f, 0f, 0f)), //shaft_1
                new AnimationCurve(),

                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 0.6f, 0.65f, 0.65f), new Keyframe(2f, 1.5f, 0f, 0f)), //shaft_2
                new AnimationCurve(),

                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 1f, 1f), new Keyframe(2f, 2f, 0f, 0f)), //head
                new AnimationCurve(),
            };
            piston.useRotCurves = true;
            piston.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 22.5f, 0f, 0f), new Keyframe(1f, 10f, -15f, -15f), new Keyframe(2f, -17.5f, 0f, 0f)), //shaft_1
                new AnimationCurve(),

                new AnimationCurve(),
                new AnimationCurve(), //shaft_2
                new AnimationCurve(),

                new AnimationCurve(),
                new AnimationCurve(), //head
                new AnimationCurve(),
            };
            piston.PartCount = 3;
            piston.TrueMaxVELOCITY = 0.12f;
            piston.TrueLimitVALUE = 2f;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0)
            };
            piston.SFX = TechAudio.SFXType.VENFlameThrower;
            piston.SFXVolume = 1f;
        }
        internal static void SetRRFloatingPiston(ModuleBlockMover piston)
        {
            piston.usePosCurves = true;
            piston.posCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(5f, 5f, 1f, 0f)), //block top
                new AnimationCurve()
            };
            piston.PartCount = 1;
            piston.TrueMaxVELOCITY = 0.07f;
            piston.InvPointWeightRatio = 0.12f;
            piston.TrueLimitVALUE = 5f;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0)
            };
            piston.SFX = TechAudio.SFXType.GCBuzzSaw;
            piston.SFXVolume = 0f;
        }
        internal static void SetBFRailPiston(ModuleBMRail piston)
        {
            piston.usePosCurves = true;
            piston.useRotCurves = true;
            piston.posCurves = new AnimationCurve[]
            {
                new AnimationCurve(), new AnimationCurve(), new AnimationCurve()
            };
            piston.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(), new AnimationCurve(), new AnimationCurve(), new AnimationCurve()
            };
            piston.PartCount = 1;
            piston.TrueMaxVELOCITY = 0.3f;
            piston.TrueLimitVALUE = 64f;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,0,1) // Pickup-AP
            };
            piston.SFX = TechAudio.SFXType.GSODrillSmall;
            piston.SFXVolume = 1f;

            piston.starterAnim = new AttachPoint()
            {
                apPos = Vector3.up * 0.5f,
                blockPos = IntVector3.up,
                apDirForward = Vector3.up,
                apDirUp = Vector3.forward,
                AnimLength = 0.5f,
                AnimPosChange = Vector3.up * 0.5f,
                Tangent = Vector3.up
            };
        }

        #endregion

        #region Swivels

        internal static void SetMediumSwivel(ModuleBlockMover swivel)
        {
            swivel.IsPlanarVALUE = true;
            swivel.useRotCurves = true;
            swivel.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,0f,0f,1f), new Keyframe(360f,360f,1f,0f)),
                new AnimationCurve()
            };
            swivel.PartCount = 1;
            swivel.TrueMaxVELOCITY = 9;
            swivel.TrueLimitVALUE = 360;
            swivel.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0),
                new IntVector3(1,1,0),
                new IntVector3(0,1,1),
                new IntVector3(1,1,1)
            };
            swivel.SFX = TechAudio.SFXType.GCTripleBore;
            swivel.SFXVolume = 0.1f;
        }
        internal static void SetInlineSwivel(ModuleBlockMover swivel)
        {
            swivel.IsPlanarVALUE = true;
            swivel.useRotCurves = true;
            swivel.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,0f,0f,1f), new Keyframe(360f,360f,1f,0f)),
                new AnimationCurve()
            };
            swivel.PartCount = 1;
            swivel.TrueMaxVELOCITY = 12;
            swivel.TrueLimitVALUE = 360;
            swivel.startblockpos = new IntVector3[]
            {
                new IntVector3(0,0,-1),
                new IntVector3(1,0,0),
                new IntVector3(0,0,1),
                new IntVector3(-1,0,0)
            };
            swivel.SFX = TechAudio.SFXType.GSODrillSmall;
            swivel.SFXVolume = 0.1f;
        }
        internal static void SetMediumInlineSwivel(ModuleBlockMover swivel)
        {
            swivel.IsPlanarVALUE = true;
            swivel.useRotCurves = true;
            swivel.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,0f,0f,1f), new Keyframe(360f,360f,1f,0f)),
                new AnimationCurve()
            };
            swivel.PartCount = 1;
            swivel.TrueMaxVELOCITY = 6;
            swivel.TrueLimitVALUE = 360;
            swivel.InvPointWeightRatio = 0.12f;
            swivel.startblockpos = new IntVector3[]
            {
                new IntVector3(-1,0,0),
                new IntVector3(-1,0,1),
                new IntVector3(0,0,-1),
                new IntVector3(1,0,-1),
                new IntVector3(2,0,0),
                new IntVector3(2,0,1),
                new IntVector3(0,0,2),
                new IntVector3(1,0,2),
            };
            swivel.SFX = TechAudio.SFXType.GCBuzzSaw;
            swivel.SFXVolume = 0f;
        }
        internal static void SetSmallSwivel(ModuleBlockMover swivel)
        {
            swivel.IsPlanarVALUE = true;
            swivel.useRotCurves = true;
            swivel.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,0f,0f,1f), new Keyframe(360f,360f,1f,0f)),
                new AnimationCurve()
            };
            swivel.PartCount = 1;
            swivel.TrueMaxVELOCITY = 6;
            swivel.TrueLimitVALUE = 360;
            swivel.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0)
            };
            swivel.SFX = TechAudio.SFXType.GSODrillSmall;
            swivel.SFXVolume = 0.2f;
        }
        internal static void SetDoubleSwivel(ModuleBlockMover swivel)
        {
            swivel.IsPlanarVALUE = true;
            swivel.useRotCurves = true;
            swivel.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,0f,0f,1f), new Keyframe(360f,360f,1f,0f)),
                new AnimationCurve()
            };
            swivel.PartCount = 1;
            swivel.TrueMaxVELOCITY = 9;
            swivel.TrueLimitVALUE = 360;
            swivel.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0),
                new IntVector3(0,-1,0)
            };
            swivel.SFX = TechAudio.SFXType.GSODrillSmall;
            swivel.SFXVolume = 1f;
        }

        #endregion

        #region FrictionPads

        private static void SetGCSmallPad(ModuleFrictionPad obj)
        {
            obj.strength = 1f;//.75f;
            obj.threshold = 400f;
        }
        private static void SetGCBigPad(ModuleFrictionPad obj)
        {
            obj.strength = 1f;
            obj.threshold = 400f;
            obj.effector = new Vector3(0.5f, 0.5f, 0.5f);
        }

        #endregion

        #endregion SetBlockData

        public static string LogAllComponents(Transform SearchIn, bool Reflection = false, string Indenting = "")
        {
            string result = "";
            Component[] c = SearchIn.GetComponents<Component>();
            foreach (Component comp in c)
            {
                result += "\n" + Indenting + comp.name + " : " + comp.GetType().Name;
                if (comp is MeshRenderer renderer) result += " : Material (" + renderer.material.name + ")";
                if (comp is Animation anim)
                {
                    var clipc = anim.GetClipCount();
                    result += $" : Animation ({clipc} clips)";
                }
            }
            for (int i = SearchIn.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = SearchIn.transform.GetChild(i);
                result += LogAllComponents(child, Reflection, Indenting + "  ");
            }
            return result;
        }
    }
    internal class AdjustAttachPosition : MonoBehaviour
    {
        //public static Vector3 oLocalPos;
        public static Vector3 PointerPos;
        static readonly int PointerLayerMask = Globals.inst.layerTank.mask | Globals.inst.layerTankIgnoreTerrain.mask | Globals.inst.layerScenery.mask | Globals.inst.layerPickup.mask | Globals.inst.layerTerrain.mask;
        const float PointerDistance = 512f;
        private int lastBlockCount = -1;

        void LateUpdate()
        {
            try // Must put it in a try catch block because sometimes the camera dies internally
            {
                var ray = Singleton.camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, PointerDistance, PointerLayerMask, QueryTriggerInteraction.Ignore))
                {
                    PointerPos = hitInfo.point;
                }
                else
                {
                    PointerPos = ray.GetPoint(PointerDistance);
                }
                if (ManPointer.inst.DraggingItem != null && ManPointer.inst.DraggingFocusTech != null)
                {
                    float num = float.MaxValue;
                    Tank tank = ManPointer.inst.DraggingFocusTech;
                    ClusterBody tankbody = null;
                    var cast = PhysicsUtils.RaycastAllNonAlloc(Singleton.camera.ScreenPointToRay(Input.mousePosition), ManPointer.inst.PickupRange, Globals.inst.layerTank.mask, QueryTriggerInteraction.Ignore);
                    foreach (var hit in cast)
                    {
                        float distance = hit.distance;
                        if (distance < num)
                        {
                            num = distance;
                            ClusterBody component = hit.collider.GetComponent<ModuleBlockMover.ModuleBMPart>()?.parent.Holder;
                            if (component == null) component = hit.collider.GetComponentInParent<ClusterBody>();
                            if (component != null && (/*tank == null || */component.coreTank == tank))
                                tankbody = component;
                            else
                                tankbody = null;
                        }
                    }
                    if (tankbody != null)
                    {
                        if (Patches.DoOffsetAttachParticles == 0 || Patches.FocusedBody != tankbody || lastBlockCount != tank.blockman.blockCount)
                            ManTechBuilder.inst.ResetAPCollection();
                        lastBlockCount = tank.blockman.blockCount;

                        Patches.FocusedBody = tankbody;
                        Patches.FocusedTech = tankbody.coreTank.trans;
                        //if (tank == null)
                        //    ManPointer.inst.DraggingFocusTech = tankbody.coreTank;

                        Patches.DoOffsetAttachParticles = 4;
                    }
                    else if (Patches.DoOffsetAttachParticles == 0 && Patches.FocusedBody != null)
                    {
                        Patches.FocusedBody = null;
                        ManTechBuilder.inst.ResetAPCollection();
                        lastBlockCount = -1;
                    }
                }
            }
            catch { /* fail silently */ }
        }
    }

    internal class Patches
    {
        static Vector3 oP;
        static Quaternion oQ;
        public static Transform FocusedTech;
        public static ClusterBody FocusedBody;
        public static byte DoOffsetAttachParticles = 0;

        /// <summary>
        /// <see cref="BlockManager.TableCache"/>
        /// </summary>
        static FieldInfo BlockPlacementCollector_m_BlockTableCache = typeof(BlockPlacementCollector).GetField("m_BlockTableCache", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPatch(typeof(BlockPlacementCollector), "ResetState")]
        static class SeepInCustomAPTable
        {
            static void Postfix(BlockPlacementCollector __instance)
            {
                if (DoOffsetAttachParticles != 0 && FocusedBody != null && __instance.PlacementsValid)
                {
                    var tableCache = (BlockManager.TableCache)BlockPlacementCollector_m_BlockTableCache.GetValue(__instance);
                    byte[,,] newTable = new byte[BlockManager.MaxBlockLimit, BlockManager.MaxBlockLimit, BlockManager.MaxBlockLimit];
                    foreach (var pair in FocusedBody.ClusterAPBitField)
                    {
                        var i = pair.Key + tableCache.blockTableCentre;
                        newTable[i.x, i.y, i.z] = pair.Value;
                    }
                    tableCache.apTable = newTable;
                    BlockPlacementCollector_m_BlockTableCache.SetValue(__instance, tableCache);
                }
            }
        }

        [HarmonyPatch(typeof(ManTechBuilder), "Update")]
        static class UpdateAttachParticles_Offset
        {
            static void Prefix()
            {
                if (DoOffsetAttachParticles != 0)
                _Offset();
            }
            static void Postfix()
            {
                if (DoOffsetAttachParticles != 0)
                _undoOffset();
            }
        }

        [HarmonyPatch(typeof(ManPointer), "Update")]
        static class ManPointer_Offset
        {
            static void Prefix()
            {
                if (DoOffsetAttachParticles != 0)
                    _Offset();
            }
            static void Postfix()
            {
                if (DoOffsetAttachParticles != 0)
                    _undoOffset();
            }
        }

        static void _Offset()
        {
                if (FocusedBody == null || FocusedTech == null)
                {
                    DoOffsetAttachParticles = 0;
                    return;
                }
                oP = FocusedTech.position;
                oQ = FocusedTech.rotation;
                FocusedTech.position = FocusedBody.transform.position;
                FocusedTech.rotation = FocusedBody.transform.rotation;
        }

        static void _undoOffset()
        {
            DoOffsetAttachParticles--;
            FocusedTech.position = oP;
            FocusedTech.rotation = oQ;
        }
    }
}