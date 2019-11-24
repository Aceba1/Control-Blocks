using Harmony;
using Nuterra.BlockInjector;
using System;
using System.Reflection;
using UnityEngine;

namespace Control_Block
{
    public class Class1
    {
        const string MoverText = "\n Right click to configure this block's programming.\n\n" +
            "This is a BlockMover. Blocks attached to the head of this will have their own physics separate from the body they are on, yet still restrained to the same tech. Like a multi-tech, but a single tech. ClusterTech.";
        public static void CreateBlocks()
        {

            #region Blocks

            #region Pistons

            #region GSO Piston

            {
                var ControlBlock = new BlockPrefabBuilder("GSOBlock(111)")
                    .SetName("GSO Piston")
                    .SetDescription("A configurable piston that can push and pull blocks on a tech." + MoverText)
                    .SetBlockID(1293838)//, "f53931ef3e14ba8e")
                    .SetFaction(FactionSubTypes.GSO)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(2)
                    .SetPrice(4470)
                    .SetHP(2000)
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
                    new CustomRecipe.RecipeOutput(1293838)
                    });
            }

            #endregion GSO Piston

            #region GeoCorp Piston

            {
                var ControlBlock = new BlockPrefabBuilder("GCBlock(222)")
                    .SetName("GeoCorp Large Piston")
                    .SetDescription("This is a bulky piston. Slower, and moves smoother.\nForged in the valleys of Uberkartoffel potatoes" + MoverText)
                    .SetBlockID(129380)//, "f5b931ef3e14ba8e")
                    .SetFaction(FactionSubTypes.GC)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(2)
                    .SetPrice(6462)
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
                    .RegisterLater();

                CustomRecipe.RegisterRecipe(
                    new CustomRecipe.RecipeInput[]
                    {
                    new CustomRecipe.RecipeInput((int)ChunkTypes.FuelInjector, 3),
                    new CustomRecipe.RecipeInput((int)ChunkTypes.SensoryTransmitter, 1),
                    new CustomRecipe.RecipeInput((int)ChunkTypes.PlubonicAlloy, 1),
                    new CustomRecipe.RecipeInput((int)ChunkTypes.TitanicAlloy, 1)
                    },
                    new CustomRecipe.RecipeOutput[]
                    {
                    new CustomRecipe.RecipeOutput(129380)
                    }, RecipeTable.Recipe.OutputType.Items, "gcfab");
            }

            #endregion GeoCorp Piston

            #region Hawkeye Piston

            {
                var ControlBlock = new BlockPrefabBuilder("HE_Block_Alt_01_(111)")
                    .SetName("Hawkeye Telescopic Piston")
                    .SetDescription("A set of enforced interlocked plates composing a piston that can extend to 4 blocks from its compressed state." + MoverText)
                    .SetBlockID(129381)//, "e5bc31ef3e14ba8e")
                    .SetFaction(FactionSubTypes.HE)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(2)
                    .SetPrice(6462)
                    .SetHP(8000)
                    .SetMass(6f)
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
                    .RegisterLater();

                CustomRecipe.RegisterRecipe(
                    new CustomRecipe.RecipeInput[]
                    {
                    new CustomRecipe.RecipeInput((int)ChunkTypes.FuelInjector, 3),
                    new CustomRecipe.RecipeInput((int)ChunkTypes.SensoryTransmitter, 1),
                    new CustomRecipe.RecipeInput((int)ChunkTypes.PlubonicAlloy, 1),
                    new CustomRecipe.RecipeInput((int)ChunkTypes.TitanicAlloy, 1)
                    },
                    new CustomRecipe.RecipeOutput[]
                    {
                    new CustomRecipe.RecipeOutput(129380)
                    }, RecipeTable.Recipe.OutputType.Items, "hefab");
            }

            #endregion Hawkeye Piston

            #region BetterFuture Piston

            {
                var ControlBlock = new BlockPrefabBuilder("BF_Block(111)")
                    .SetName("Better Piston")
                    .SetDescription("This piston started the revolution for all pistons and swivels. Replacing the technology of ghost-phasing and uniting swivels and pistons as one in a series of events that this piston was not aware was happening." + MoverText)
                    .SetBlockID(1293834)//, "f63931ef3e14ba8e")
                    .SetFaction(FactionSubTypes.BF)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(0)
                    .SetPrice(10000)
                    .SetHP(2000)
                    .SetMass(3f)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.BFp_png)));

                var mat = GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main");
                var par = ControlBlock.Prefab.transform;

                AddMeshToBlockMover(mat, new Vector3(.95f, .95f, .95f), Vector3.zero, par, Properties.Resources.BFp_blockbottom);
                AddMeshToBlockMover(mat, new Vector3(.95f, .95f, .95f), Vector3.zero, par, Properties.Resources.BFp_blocktop);

                ControlBlock.SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[]{
                    Vector3.up * 0.5f,
                    Vector3.down * 0.5f })
                    .AddComponent<ModuleBlockMover>(SetBFPiston)
                    .RegisterLater();

                //CustomRecipe.RegisterRecipe(
                //    new CustomRecipe.RecipeInput[]
                //    {
                //    new CustomRecipe.RecipeInput((int)ChunkTypes.FuelInjector, 1),
                //    new CustomRecipe.RecipeInput((int)ChunkTypes.SensoryTransmitter, 1),
                //    new CustomRecipe.RecipeInput((int)ChunkTypes.PlubonicAlloy, 1),
                //    },
                //    new CustomRecipe.RecipeOutput[]
                //    {
                //    new CustomRecipe.RecipeOutput(1293838)
                //    });
            }

            #endregion BetterFuture Piston

            #region BetterFuture Piston

            {
                var ControlBlock = new BlockPrefabBuilder("VENBlock(111)")
                    .SetName("Venture Twist Piston")
                    .SetDescription("This piston started the revolution for all pistons and swivels. Replacing the technology of ghost-phasing and uniting swivels and pistons as one in a series of events that this piston was not aware was happening." + MoverText)
                    .SetBlockID(1293837)
                    .SetFaction(FactionSubTypes.VEN)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(0)
                    .SetPrice(5000)
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
                    .RegisterLater();

                //CustomRecipe.RegisterRecipe(
                //    new CustomRecipe.RecipeInput[]
                //    {
                //    new CustomRecipe.RecipeInput((int)ChunkTypes.FuelInjector, 1),
                //    new CustomRecipe.RecipeInput((int)ChunkTypes.SensoryTransmitter, 1),
                //    new CustomRecipe.RecipeInput((int)ChunkTypes.PlubonicAlloy, 1),
                //    },
                //    new CustomRecipe.RecipeOutput[]
                //    {
                //    new CustomRecipe.RecipeOutput(1293838)
                //    });
            }

            #endregion BetterFuture Piston

            #region Special Pistons

            #region BetterFuture Rail Piston

            {
                var ControlBlock = new BlockPrefabBuilder("BF_Block(111)")
                    .SetName("Better Future Rail Piston")
                    .SetDescription("An extendable rail, with a small cart that can move blocks attached to it. Add rail segment blocks to the end to make it longer!" + MoverText)
                    .SetBlockID(1293835)//, "f63931ef3e14ba8e")
                    .SetFaction(FactionSubTypes.BF)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(0)
                    .SetPrice(10000)
                    .SetHP(1000)
                    .SetDetachFragility(0.5f)
                    .SetMass(1.5f);
                    //.SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.BFp_png)));

                var mat = GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main");
                var par = ControlBlock.Prefab.transform;

                AddMeshToBlockMover(mat, new Vector3(1f, 1f, 1f), Vector3.zero, par, Properties.Resources.bf_rail_piston_base);
                AddMeshToBlockMover(mat, Vector3.zero, par, Properties.Resources.bf_rail_piston_head);

                ControlBlock.SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.All)
                    .AddComponent<ModuleBlockMoverRail>(SetBFRailPiston)
                    .RegisterLater();
            }

            {
                var ControlBlock = new BlockPrefabBuilder("BF_Block(111)")
                    .SetName("Better Future Rail Segment")
                    .SetDescription("A segment for the Better Future Rail Piston, add it to the end of the line to make it go farther")
                    .SetBlockID(1293836)//, "f63931ef3e14ba8e")
                    .SetFaction(FactionSubTypes.BF)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(0)
                    .SetPrice(2000)
                    .SetHP(1000)
                    .SetDetachFragility(0.5f)
                    .SetMass(1.5f);
                //.SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.BFp_png)));

                var mat = GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main");
                var par = ControlBlock.Prefab.transform;

                AddMeshToBlockMover(mat, new Vector3(1f, 1f, 1f), Vector3.zero, par, Properties.Resources.bf_rail_piston_extension);

                ControlBlock.SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[]{
                    Vector3.up * 0.5f,
                    Vector3.down * 0.5f,
                    Vector3.left * 0.5f,
                    Vector3.right * 0.5f,
                    Vector3.back * 0.5f })
                    .AddComponent<ModuleBlockMoverRailSegment>()
                    .RegisterLater();
            }
            #endregion BetterFuture Rail Piston

            #endregion Special Pistons

            #endregion Pistons

            #region Swivels

            #region GSO Medium Swivel

            {
                var ControlBlock = new BlockPrefabBuilder("GSOBlock(111)")
                    .SetName("Medium Embedded Swivel")
                    .SetDescription("A configurable swivel that can rotate blocks on a tech." + MoverText)
                    .SetBlockID(1393838)//, "f64931ef3e14ba8e")
                    .SetFaction(FactionSubTypes.GSO)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(2)
                    .SetPrice(4470)
                    .SetHP(2000)
                    .SetMass(2.5f)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.swivel_png)));

                var mat = GameObjectJSON.GetObjectFromGameResources<Material>("GSO_Main");
                var par = ControlBlock.Prefab.transform;

                AddMeshToBlockMover(mat, new Vector3(1.9f, .95f, 1.9f), new Vector3(.5f, 0f, .5f), par, Properties.Resources.swivel_base);
                AddMeshToBlockMover(mat, new Vector3(.5f, 0f, .5f), par, Properties.Resources.swivel_head);//.AddComponent<GimbalAimer>();
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
                    new CustomRecipe.RecipeOutput(1293838)
                    });
            }

            #endregion GSO Medium Swivel

            #region VEN Inline Swivel
            {
                var ControlBlock = new BlockPrefabBuilder("VENBlock(111)")
                    .SetName("Inline Embedded Swivel")
                    .SetDescription("An inline swivel, which's center disk rotates blocks. And it's fast, too" + MoverText)
                    .SetBlockID(1393837)//, "f74931ef3e14ba8e")
                    .SetFaction(FactionSubTypes.VEN)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(2)
                    .SetPrice(4470)
                    .SetHP(2000)
                    .SetMass(1f)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.swivel_ven_png)));

                var mat = GameObjectJSON.GetObjectFromGameResources<Material>("VEN_Main");
                var par = ControlBlock.Prefab.transform;

                AddMeshToBlockMover(mat, new Vector3(.95f, .95f, .95f), Vector3.zero, par, Properties.Resources.swivel_ven_base);
                AddMeshToBlockMover(mat, Vector3.zero, par, Properties.Resources.swivel_ven_head);//.AddComponent<GimbalAimer>();
                //gimbal.aimClampMaxPercent = 360;
                //gimbal.rotationAxis = GimbalAimer.AxisConstraint.Y;

                ControlBlock.SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.All)
                    .AddComponent<ModuleBlockMover>(SetInlineSwivel)
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
                    new CustomRecipe.RecipeOutput(1293838)
                    });
            }

            #endregion VEN Inline Swivel

            #region GSO Small Swivel
            {
                var ControlBlock = new BlockPrefabBuilder("GSOBlock(111)")
                    .SetName("Small Embedded Swivel")
                    .SetDescription("A smaller swivel, operational hopefully." + MoverText)
                    .SetBlockID(1393836)//, "f84931ef3e14ba8e")
                    .SetFaction(FactionSubTypes.GSO)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(2)
                    .SetPrice(4470)
                    .SetHP(2000)
                    .SetMass(1.5f)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.swivel_small_png)));

                var mat = GameObjectJSON.GetObjectFromGameResources<Material>("GSO_Main");
                var par = ControlBlock.Prefab.transform;

                AddMeshToBlockMover(mat, new Vector3(.95f, .95f, .95f), Vector3.zero, par, Properties.Resources.swivel_small_base);
                AddMeshToBlockMover(mat, Vector3.zero, par, Properties.Resources.swivel_small_head);//.AddComponent<GimbalAimer>();
                //gimbal.aimClampMaxPercent = 360;
                //gimbal.rotationAxis = GimbalAimer.AxisConstraint.Y;

                ControlBlock.SetSizeManual(new IntVector3[] { IntVector3.zero },
                    new Vector3[] { Vector3.down * .5f, Vector3.up * .5f })
                    .AddComponent<ModuleBlockMover>(SetSmallSwivel)
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
                    new CustomRecipe.RecipeOutput(1293838)
                    });
            }

            #endregion GSO Small Swivel

            #region HE Double Swivel
            {
                var ControlBlock = new BlockPrefabBuilder("HE_Block_Alt_01_(111)")
                    .SetName("Dual Rotor Swivel")
                    .SetDescription("A swivel with two heads at the top and bottom. Designed by Rafs!" + MoverText)
                    .SetBlockID(1393835)//, "f74931ef3e14ba8e")
                    .SetFaction(FactionSubTypes.HE)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(2)
                    .SetPrice(4470)
                    .SetHP(2000)
                    .SetMass(2f);
                //.SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.swivel_ven_png)));

                var mat = GameObjectJSON.GetObjectFromGameResources<Material>("HE_Main");
                var par = ControlBlock.Prefab.transform;

                AddMeshToBlockMover(mat, new Vector3(.95f, .95f, .95f), Vector3.zero, par, Properties.Resources.swivel_double_base);
                AddMeshToBlockMover(mat, Vector3.zero, par, Properties.Resources.swivel_double_head);//.AddComponent<GimbalAimer>();
                //gimbal.aimClampMaxPercent = 360;
                //gimbal.rotationAxis = GimbalAimer.AxisConstraint.Y;

                ControlBlock.SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.All)
                    .AddComponent<ModuleBlockMover>(SetDoubleSwivel)
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
                    new CustomRecipe.RecipeOutput(1293838)
                    });
            }

            #endregion GSO Medium Swivel

            #endregion Swivels

            #region Steering Regulator

            {
                var SteeringRegulator = new BlockPrefabBuilder("BF_Block(111)")
                    .SetName("Stabilizer PiD S.Regulator Accessory")
                    .SetDescription("The PiD technology has been discontinued, Please refer to the BF stabilization computer guidelines")
                    .SetBlockID(1293839)//, "12ef3f7f30d4ba8e")
                    .SetFaction(FactionSubTypes.BF)
                    .SetCategory(BlockCategories.Accessories)
                    //.AddComponent<ModuleSteeringRegulator>()
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
                    Vector3.back * 0.5f })
                    .RegisterLater();
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
                    });
            }

            #endregion Steering Regulator

            #region Pads

            #region GC Small Pad

            {
                var FrictionPad = new BlockPrefabBuilder("GCBlock(222)")
                    .SetName("Small Friction Pad")
                    .SetDescription("Nice and grippy. Little sticky. Will break reality if used improperly")
                    .SetBlockID(1293831)//, "02ef3f7f30d4ba8e")
                    .SetFaction(FactionSubTypes.GC)
                    .SetCategory(BlockCategories.Wheels)
                    .SetGrade(1)
                    .SetPrice(500)
                    .SetHP(600)
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
                var FrictionPad = new BlockPrefabBuilder("GCBlock(222)")
                    .SetName("Non Slip-A-Tron 3000")
                    .SetDescription("'Name by Rasseru")
                    .SetBlockID(1293830)//, "03ef3f7f30d4ba8e")
                    .SetFaction(FactionSubTypes.GC)
                    .SetCategory(BlockCategories.Wheels)
                    .SetGrade(1)
                    .SetPrice(2000)
                    .SetHP(2000)
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

            #endregion Pads

            #region MultiTech Magnets

            #region Fixed MTMag

            {
                var mtmag = new BlockPrefabBuilder("BF_Block(111)")
                    .SetName("FixedJoint MTMag")
                    .SetDescription("Use this with another of its kind on a separate tech to lock them together through the power of PHYSICS!\n\n")
                    .SetBlockID(1293700)
                    .SetFaction(FactionSubTypes.BF)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(2)
                    .SetPrice(500)
                    .SetHP(600)
                    .SetMass(1f)
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.mtmag_fixed), false, GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main", false))
                    //.SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.mtmag_fixed_png)))
                    .SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[] { Vector3.up * -0.5f })
                    .AddComponent<ModuleMTMagnet>(SetFixedMTMag);

                AddCollider(new Vector3(1f, 0.5f, 1f), Vector3.down * 0.25f, mtmag.Prefab.transform);

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
                var mtmag = new BlockPrefabBuilder("BF_Block(111)")
                    .SetName("BallJoint MTMag")
                    .SetDescription("Use this with another of its kind on a separate tech to lock them together through the power of PHYSICS!\nThis is literally the best joint. In terms of stability and reliability. Now if only it were bigger...")
                    .SetBlockID(1293701)
                    .SetFaction(FactionSubTypes.BF)
                    .SetCategory(BlockCategories.Base)
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
                var mtmag = new BlockPrefabBuilder("BF_Block(111)")
                    .SetName("BallJoint Large MTMag")
                    .SetDescription("Use this with another of its kind on a separate tech to lock them together through the power of PHYSICS!\nA bigger version of the best joint in all of existence!")
                    .SetBlockID(1293703)
                    .SetFaction(FactionSubTypes.BF)
                    .SetCategory(BlockCategories.Base)
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
                var mtmag = new BlockPrefabBuilder("BF_Block(111)")
                    .SetName("SwivelJoint Large MTMag")
                    .SetDescription("Use this with another of its kind on a separate tech to lock them together through the power of PHYSICS!")
                    .SetBlockID(1293702)
                    .SetFaction(FactionSubTypes.BF)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(2)
                    .SetPrice(500)
                    .SetHP(600)
                    .SetMass(3f)
                    .SetSize(new IntVector3(2, 1, 2), BlockPrefabBuilder.AttachmentPoints.Bottom)
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.mtmag_swivel_large), false, GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main", false))
                    //.SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.mtmag_fixed_png)))
                    .AddComponent<ModuleMTMagnet>(SetLargeSwivelMTMag);

                AddCollider(new Vector3(2f, 1f, 2f), new Vector3(0.5f, 0f, 0.5f), mtmag.Prefab.transform);

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

            #endregion MultiTech Magnets

            #endregion Blocks

            GameObject _holder = new GameObject();
            //_holder.AddComponent<OptionMenuPiston>();
            //_holder.AddComponent<OptionMenuSwivel>();
            _holder.AddComponent<OptionMenuSteeringRegulator>();
            _holder.AddComponent<OptionMenuMover>();
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

        internal static GameObject AddCollider(Vector3 colliderSize, Vector3 colliderOffset, Transform par)
        {
            GameObject sub = new GameObject("Frictionless Collider") { layer = Globals.inst.layerTank };

            var mhc = sub.AddComponent<BoxCollider>();
            mhc.size = colliderSize;
            mhc.center = colliderOffset;
            mhc.material = new PhysicMaterial() { dynamicFriction = 0, frictionCombine = PhysicMaterialCombine.Maximum, staticFriction = 0.3f };
            sub.transform.SetParent(par);
            sub.transform.localPosition = Vector3.zero;
            sub.transform.localRotation = Quaternion.identity;
            return sub;
        }

        internal static GameObject AddMeshToBlockMover(Material mat, Vector3 colliderSize, Vector3 colliderOffset, Transform par, string Mesh)
        {
            GameObject sub = new GameObject("BlockMover Part") { layer = Globals.inst.layerTank };
            sub.AddComponent<MeshFilter>().sharedMesh = GameObjectJSON.MeshFromData(Mesh);
            sub.AddComponent<MeshRenderer>().sharedMaterial = mat;

            var mhc = sub.AddComponent<BoxCollider>();
            mhc.size = colliderSize;
            mhc.center = colliderOffset;
            sub.transform.SetParent(par);
            sub.transform.localPosition = Vector3.zero;
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
            //piston.MaximumBlockPush = 108;
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
            piston.MaxVELOCITY = 0.08f;
            //piston.StretchSpeed = 0.08f;
            //piston.CanModifyStretch = false;
            piston.TrueLimitVALUE = 1f;
            piston.MINVALUELIMIT = 0f;
            piston.MAXVALUELIMIT = 1f;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0)
            };
            piston.SFX = TechAudio.SFXType.GSODrillLarge;
            piston.SFXVolume = 17f;
        }
        internal static void SetGeoCorpPiston(ModuleBlockMover piston)
        {
            //piston.MaximumBlockPush = 384;
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
            //piston.StretchSpeed = 0.03f;
            piston.MaxVELOCITY = 0.06f;
            //piston.CanModifyStretch = false;
            piston.MINVALUELIMIT = 0;
            piston.TrueLimitVALUE = 2;
            piston.MAXVALUELIMIT = 2;
            //piston.StretchModifier = 2; piston.MaxStr = 2;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,2,0),
                new IntVector3(0,2,1),
                new IntVector3(1,2,0),
                new IntVector3(1,2,1)
            };
            piston.SFX = TechAudio.SFXType.GCPlasmaCutter;
            piston.SFXVolume = 20f;
        }
        internal static void SetHawkeyePiston(ModuleBlockMover piston)
        {
            //piston.MaximumBlockPush = 120;
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
            piston.MaxVELOCITY = 0.075f;
            //piston.StretchSpeed = 0.025f;
            //piston.CanModifyStretch = true;
            piston.MINVALUELIMIT = 0;
            piston.TrueLimitVALUE = 3;
            piston.MAXVALUELIMIT = 3;
            //piston.StretchModifier = 3; piston.MaxStr = 3;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0),
                new IntVector3(0,0,-1)
            };
            piston.SFX = TechAudio.SFXType.GCTripleBore;
            piston.SFXVolume = 16f;
        }
        internal static void SetBFPiston(ModuleBlockMover piston)
        {
            //piston.MaximumBlockPush = 65535;
            piston.usePosCurves = true;
            piston.posCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f)), //block top
                new AnimationCurve()
            };
            piston.PartCount = 1;
            piston.MaxVELOCITY = 0.12f;
            //piston.StretchSpeed = 0.12f;
            //piston.CanModifyStretch = false;
            piston.TrueLimitVALUE = 1f;
            piston.MINVALUELIMIT = 0f;
            piston.MAXVALUELIMIT = 1f;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0)
            };
            piston.SFX = TechAudio.SFXType.FlameThrowerPlasma;
            piston.SFXVolume = 5f;
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
            piston.MaxVELOCITY = 0.1f;
            //piston.StretchSpeed = 0.12f;
            //piston.CanModifyStretch = false;
            piston.TrueLimitVALUE = 2f;
            piston.MINVALUELIMIT = 0f;
            piston.MAXVALUELIMIT = 2f;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0)
            };
            piston.SFX = TechAudio.SFXType.VENFlameThrower;
            piston.SFXVolume = 5f;
        }

        internal static void SetBFRailPiston(ModuleBlockMoverRail piston)
        {
            //piston.MaximumBlockPush = 65535;
            piston.usePosCurves = true;
            piston.posCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(64f, 64f, 1f, 0f)), //block top
                new AnimationCurve()
            };
            piston.PartCount = 1;
            piston.MaxVELOCITY = 0.15f;
            //piston.StretchSpeed = 0.12f;
            //piston.CanModifyStretch = false;
            piston.TrueLimitVALUE = 64f;
            piston.MINVALUELIMIT = 0f;
            piston.MAXVALUELIMIT = 0.1f;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,0,1)
            };
            piston.SFX = TechAudio.SFXType.FlameThrowerPlasma;
            piston.SFXVolume = 5f;

            piston.railblockpos = new IntVector3(0, 1, 0);
        }

        #endregion

        #region Swivels

        internal static void SetMediumSwivel(ModuleBlockMover swivel)
        {
            //swivel.MaximumBlockPush = 196;
            swivel.IsPlanarVALUE = true;
            swivel.useRotCurves = true;
            swivel.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,0f,0f,1f), new Keyframe(360f,360f,1f,0f)),
                new AnimationCurve()
            };
            swivel.PartCount = 1;
            //swivel.CanModifySpeed = true;
            //swivel.RotateSpeed = 5;
            swivel.MaxVELOCITY = 9;
            //swivel.MaxSpeed = 12f;
            //swivel.MINVALUELIMIT = 0;
            //swivel.MAXVALUELIMIT = 360;
            swivel.TrueLimitVALUE = 360;
            //swivel.LockAngle = false;
            swivel.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0),
                new IntVector3(1,1,0),
                new IntVector3(0,1,1),
                new IntVector3(1,1,1)
            };
            swivel.SFX = TechAudio.SFXType.GCTripleBore;
            swivel.SFXVolume = 0.05f;
        }
        internal static void SetInlineSwivel(ModuleBlockMover swivel)
        {
            //swivel.BreakOnCab = true;
            //swivel.MaximumBlockPush = 92;
            swivel.IsPlanarVALUE = true;
            swivel.useRotCurves = true;
            swivel.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,0f,0f,1f), new Keyframe(360f,360f,1f,0f)),
                new AnimationCurve()
            };
            swivel.PartCount = 1;
            //swivel.CanModifySpeed = true;
            //swivel.RotateSpeed = 7.5f;
            swivel.MaxVELOCITY = 12;
            //swivel.MaxSpeed = 15;
            //swivel.LockAngle = false;
            //swivel.MINVALUELIMIT = 0;
            //swivel.MAXVALUELIMIT = 360;
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
        internal static void SetSmallSwivel(ModuleBlockMover swivel)
        {
            //swivel.BreakOnCab = true;
            //swivel.MaximumBlockPush = 128;
            swivel.IsPlanarVALUE = true;
            swivel.useRotCurves = true;
            swivel.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,0f,0f,1f), new Keyframe(360f,360f,1f,0f)),
                new AnimationCurve()
            };
            swivel.PartCount = 1;
            //swivel.CanModifySpeed = true;
            //swivel.RotateSpeed = 5f;
            swivel.MaxVELOCITY = 6;
            //swivel.MaxSpeed = 8;
            //swivel.LockAngle = false;
            //swivel.MINVALUELIMIT = 0;
            //swivel.MAXVALUELIMIT = 360;
            swivel.TrueLimitVALUE = 360;
            swivel.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0)
            };
            swivel.SFX = TechAudio.SFXType.GSODrillSmall;
            swivel.SFXVolume = 0.13f;
        }
        internal static void SetDoubleSwivel(ModuleBlockMover swivel)
        {
            //swivel.BreakOnCab = true;
            //swivel.MaximumBlockPush = 92;
            swivel.IsPlanarVALUE = true;
            swivel.useRotCurves = true;
            swivel.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(),
                new AnimationCurve(new Keyframe(0f,0f,0f,1f), new Keyframe(360f,360f,1f,0f)),
                new AnimationCurve()
            };
            swivel.PartCount = 1;
            //swivel.CanModifySpeed = true;
            //swivel.RotateSpeed = 7.5f;
            swivel.MaxVELOCITY = 6;
            //swivel.MaxSpeed = 15;
            //swivel.LockAngle = false;
            //swivel.MINVALUELIMIT = 0;
            //swivel.MAXVALUELIMIT = 360;
            swivel.TrueLimitVALUE = 360;
            swivel.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0),
                new IntVector3(0,-1,0)
            };
            swivel.SFX = TechAudio.SFXType.GSODrillLarge;
            swivel.SFXVolume = 0.1f;
        }

        #endregion

        #region FrictionPads

        private static void SetGCSmallPad(ModuleFrictionPad obj)
        {
            obj.strength = .5f;
            obj.threshold = 1f;
        }
        private static void SetGCBigPad(ModuleFrictionPad obj)
        {
            obj.strength = .76f;
            obj.threshold = 2f;
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
                if (comp is MeshRenderer) result += " : Material (" + ((MeshRenderer)comp).material.name + ")";
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
        public static Vector3 oLocalPos;
        public static Vector3 PointerPos;
        static readonly int PointerLayerMask = Globals.inst.layerTank.mask | Globals.inst.layerTankIgnoreTerrain.mask | Globals.inst.layerScenery.mask | Globals.inst.layerPickup.mask | Globals.inst.layerTerrain.mask;
        const float PointerDistance = 512f;

        void LateUpdate()
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
                    if (distance < num && hit.rigidbody != null)
                    {
                        ClusterBody component = hit.rigidbody.GetComponent<ClusterBody>();
                        if (component && (tank == null || component.coreTank == tank))
                        {
                            tankbody = component;
                            num = distance;
                        }
                    }
                }
                if (tankbody != null)
                {
                    Patches.ClusterBody = tankbody.transform;
                    Patches.FocusedTech = tankbody.coreTank.trans;
                    if (tank == null)
                    {
                        ManPointer.inst.DraggingFocusTech = tankbody.coreTank;
                    }
                    Patches.DoOffsetAttachParticles = 4;
                }
            }
        }
    }

    internal class Patches
    {
        //private static FieldInfo H_mASS, H_mTCU, H_mTCR, H_pB,
        //    F_mTSR, F_mASS, F_mE, F_mPB,
        //    B_mE, B_mASS, B_mFSC, B_mPB;

        //static Patches()
        //{
        //    try
        //    {
        //        BindingFlags b = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
        //        {
        //            Type T = typeof(HoverJet);
        //            H_mASS = T.GetField("m_AutoStabiliseStrength", b);
        //            H_mTCU = T.GetField("m_ThrustContributionUp", b);
        //            H_mTCR = T.GetField("m_ThrustContributionRight", b);
        //            H_pB = T.GetField("parentBlock", b);
        //        }
        //        {
        //            Type T = typeof(FanJet);
        //            F_mPB = T.GetField("m_ParentBlock", b);
        //            F_mASS = T.GetField("m_AutoStabiliseStrength", b);
        //            F_mTSR = T.GetField("m_TargetSpinRate", b);
        //            F_mE = T.GetField("m_Effector", b);
        //        }
        //        {
        //            Type T = typeof(BoosterJet);
        //            B_mE = T.GetField("m_Effector", b);
        //            B_mASS = T.GetField("m_AutoStabiliseStrength", b);
        //            B_mFSC = T.GetField("m_FireStrengthCurrent", b);
        //            B_mPB = T.GetField("m_ParentBlock", b);
        //        }
        //        {
        //            m_AwaitingPhysicsReset = typeof(Tank).GetField("m_AwaitingPhysicsReset", b);
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        //[HarmonyPatch(typeof(BlockManager), "AddBlockToTech")]
        //private static class BlockManagerFix
        //{
        //    private static void Prefix(ref BlockManager __instance, ref TankBlock block, IntVector3 localPos)
        //    {
        //        foreach (TankBlock _b in __instance.IterateBlocks())
        //        {
        //            var module = _b.GetComponent<ModuleBlockMover>();
        //            if (module)
        //            {
        //                module.BeforeBlockAdded(block);
        //            }
        //        }
        //    }
        //}

        //SR

        //[HarmonyPatch(typeof(HoverJet), "AutoStabiliseTank")]
        //private static class HoverJetStabilizePatch
        //{
        //    private static void Postfix(ref HoverJet __instance, ref float driveInput, ref float turnInput)
        //    {
        //        ModuleSteeringRegulator sr = ((TankBlock)H_pB.GetValue(__instance)).tank.gameObject.GetComponentInChildren<ModuleSteeringRegulator>();
        //        if (sr != null && sr.CanWork)
        //        {
        //            float ___m_AutoStabiliseStrength = (float)H_mASS.GetValue(__instance);
        //            Vector3 lhs = Quaternion.Inverse(sr.rbody.rotation) * sr.lhs * sr.HoverMod;
        //            float num = 1f;
        //            driveInput -= ___m_AutoStabiliseStrength * Vector3.Dot(lhs, (Vector3)H_mTCU.GetValue(__instance));
        //            driveInput = Mathf.Clamp(driveInput, -num, num);
        //            turnInput -= ___m_AutoStabiliseStrength * Vector3.Dot(lhs, (Vector3)H_mTCR.GetValue(__instance));
        //            turnInput = Mathf.Clamp(turnInput, -num, num);
        //        }
        //    }
        //}

        //public static bool AwaitingOverride;

        //[HarmonyPatch(typeof(TargetAimer), "GetManualTarget")] // Create new hook to TargetAimer.GetManualTarget
        //static class ChangeTarget
        //{
        //    private static bool Prefix(ref TargetAimer __instance, ref Visible __result)
        //    {
        //        if (!AwaitingOverride) return true;
        //        AwaitingOverride = false;
        //        var tank = __instance.GetComponentInParent<Tank>();
        //        var swivel = tank.GetComponentInChildren<ModuleSwivel>();
        //        if (swivel != null && swivel.mode == ModuleSwivel.Mode.AimAtPlayer)
        //        {
        //            __result = Singleton.playerTank.visible;
        //            return false;
        //        }
        //        return true;
        //    }
        //}

        static Vector3 oP;
        static Quaternion oQ;
        public static Transform FocusedTech;
        public static Transform ClusterBody;
        public static byte DoOffsetAttachParticles = 0;

        //[HarmonyPatch(typeof(ManTechBuilder), "OnDragItem")]
        //static class OnDragItem_Offset
        //{
        //    static void Prefix()
        //    {
        //        _Offset();
        //    }
        //    static void Postfix()
        //    {
        //        _undoOffset();
        //    }
        //}

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
                if (ClusterBody == null || FocusedTech == null)
                {
                    DoOffsetAttachParticles = 0;
                    return;
                }
                oP = FocusedTech.position;
                oQ = FocusedTech.rotation;
                FocusedTech.position = ClusterBody.position;
                FocusedTech.rotation = ClusterBody.rotation;
        }

        static void _undoOffset()
        {
                DoOffsetAttachParticles--;
                FocusedTech.position = oP;
                FocusedTech.rotation = oQ;
        }

        //private static FieldInfo m_AwaitingPhysicsReset;
        //private static Vector3 oldCOM;
        //private static bool CenterToOld = false;
        [HarmonyPatch(typeof(Tank), "ResetPhysics")]
        private static class ResetPhysicsHook
        {
            private static void Prefix(Tank __instance)
            {
                foreach (var blockmover in __instance.GetComponentsInChildren<ModuleBlockMover>())
                {
                    blockmover.PreResetPhysics();
                    //if (blockmover.Holder != null)
                    //{
                    //    _RecursivePre(blockmover.Holder);
                    //}
                }
            }
            //static void _RecursivePre(ClusterBody body)
            //{
            //    foreach (var blockmover in body.moduleBlockMover.GrabbedBlockMovers)
            //    {
            //        blockmover.PreResetPhysics();
            //        if (blockmover.Holder != null)
            //        {
            //            _RecursivePre(blockmover.Holder);
            //        }
            //    }
            //}
            //static void _RecursivePost(ClusterBody body)
            //{
            //    foreach (var blockmover in body.moduleBlockMover.GrabbedBlockMovers)
            //    {
            //        blockmover.PostResetPhysics();
            //        //if (blockmover.Holder != null)
            //        //{
            //        //    _RecursivePost(blockmover.Holder);
            //        //}
            //    }
            //}
            private static void Postfix(Tank __instance)
            {
                foreach (var blockmover in __instance.GetComponentsInChildren<ModuleBlockMover>())
                {
                    blockmover.PostResetPhysics();
                    //if (blockmover.Holder != null)
                    //{
                    //    _RecursivePost(blockmover.Holder);
                    //}
                }
            }
        }

        //[HarmonyPatch(typeof(FanJet), "AutoStabiliseTank")]
        //private static class FanJetStabilizePatch
        //{
        //    private static void Postfix(ref FanJet __instance)
        //    {
        //        ModuleSteeringRegulator sr = ((TankBlock)F_mPB.GetValue(__instance)).tank.gameObject.GetComponentInChildren<ModuleSteeringRegulator>();
        //        if (sr != null && sr.CanWork)
        //        {
        //            float ___m_AutoStabiliseStrength = (float)F_mASS.GetValue(__instance);
        //            if (___m_AutoStabiliseStrength > 0f)
        //            {
        //                Rigidbody rbody = sr.rbody;
        //                Vector3 forward = ((Transform)F_mE.GetValue(__instance)).forward;
        //                Vector3 pointVelocity = sr.lhs * sr.TurbineMod;
        //                float num = ___m_AutoStabiliseStrength * Vector3.Dot(pointVelocity, forward);
        //                //if (Mathf.Abs(num) < 0.0125f)
        //                //{
        //                //    num = 0f;
        //                //}
        //                //else
        //                //{
        //                //    num -= Mathf.Sign(num) * 0.0125f;
        //                //}
        //                __instance.SetSpin(num + (float)F_mTSR.GetValue(__instance));
        //            }
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(BoosterJet), "AutoStabiliseTank")]
        //private static class BoosterJetStabilizePatch
        //{
        //    private static void Postfix(ref BoosterJet __instance)
        //    {
        //        ModuleSteeringRegulator sr = ((TankBlock)B_mPB.GetValue(__instance)).tank.gameObject.GetComponentInChildren<ModuleSteeringRegulator>();
        //        if (sr != null && sr.CanWork)
        //        {
        //            var ___m_AutoStabiliseStrength = (float)B_mASS.GetValue(__instance);
        //            if (___m_AutoStabiliseStrength > 0f)
        //            {
        //                Rigidbody rbody = sr.rbody;
        //                Vector3 forward = ((Transform)B_mE.GetValue(__instance)).forward;
        //                Vector3 pointVelocity = sr.lhs * sr.JetMod;
        //                float num = ___m_AutoStabiliseStrength * Vector3.Dot(pointVelocity, forward) - .075f;
        //                if (num < 0f)
        //                {
        //                    num = 0f;
        //                }
        //                var cs = (float)B_mFSC.GetValue(__instance);
        //                B_mFSC.SetValue(__instance, Mathf.Clamp(cs + num, cs, 1f));
        //            }
        //        }
        //    }
        //}
    }
}