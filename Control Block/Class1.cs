using Harmony;
using Nuterra.BlockInjector;
using System;
using System.Reflection;
using UnityEngine;

namespace Control_Block
{
    public class Class1
    {
        public static void CreateBlocks()
        {
            var harmony = HarmonyInstance.Create("aceba1.controlblocks");
            harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

            #region Blocks

            #region BF FPV Cab

            {
                var cockpit = new BlockPrefabBuilder("GSOCockpit(111)", false)
                    .SetBlockID(9003, "aba82861496cfa13")
                    .SetName("BF Compact FPV Cab")
                    .SetDescription("A nice small BF cab, featuring a built in camera that you can look through. Forged by AstraTheDragon\n\nRight click and drag to look and Cycle views with R (and backwards with Shift held down)")
                    .SetPrice(2000)
                    .SetHP(600)
                    .SetFaction(FactionSubTypes.BF)
                    .SetCategory(BlockCategories.Control)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.BFCab_png)))
                    .SetMass(.875f)
                    .SetSizeManual(new IntVector3[] { IntVector3.zero },
                        new Vector3[]
                        {
                            Vector3.down*.5f,
                            Vector3.left*0.5f,
                            Vector3.right*0.5f,
                            Vector3.back*0.5f
                        });

                var bfmat = GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main", true);

                cockpit.RemoveChildrenWithComponent<BoxCollider>(true);

                foreach (MeshFilter mf in cockpit.Prefab.GetComponentsInChildren<MeshFilter>())
                {
                    if (mf.name == "m_GSO_Cab_111_Base")
                    {
                        GameObject.DestroyImmediate(mf);
                        cockpit.SetModel(GameObjectJSON.MeshFromFile(Properties.Resources.BFCab, "BFCab"), GameObjectJSON.GetObjectFromGameResources<GameObject>("BF_Streamline(111)").GetComponentInChildren<MeshCollider>().sharedMesh, true, bfmat);
                    }
                    else if (mf.name.StartsWith("m_GSO_Cab_111_Tyre_"))
                    {
                        mf.mesh = GameObjectJSON.MeshFromFile(Properties.Resources.BFCab_wheel, "BFCab_Wheel");
                        mf.GetComponent<MeshRenderer>().material = bfmat;
                    }
                    else if (mf.name.StartsWith("m_GSO_Cab_111_Gun_") || mf.name.StartsWith("m_GSO_Cab_111_Flap_"))
                    {
                        GameObject.DestroyImmediate(mf.GetComponent<MeshRenderer>());
                        GameObject.DestroyImmediate(mf);
                    }
                }

                var view = new GameObject("FirstPersonAnchor");
                view.AddComponent<ModuleFirstPerson>();
                view.transform.parent = cockpit.TankBlock.transform;
                view.transform.localPosition = new Vector3(0f, 0.175f, -0.1f);
                view.transform.localRotation = Quaternion.identity;

                CustomRecipe.RegisterRecipe(
                    new CustomRecipe.RecipeInput[]
                    {
                    new CustomRecipe.RecipeInput((int)ChunkTypes.OleiteJelly, 25),
                    },
                    new CustomRecipe.RecipeOutput[]
                    {
                    new CustomRecipe.RecipeOutput(9003)
                    });

                cockpit.RegisterLater();
            }

            #endregion BF FPV Cab

            #region Pistons

            #region GSO Piston

            {
                var ControlBlock = new BlockPrefabBuilder("GSOBlock(111)")
                    .SetName("GSO Piston")
                    .SetDescription("A configurable piston that can push and pull blocks on a tech.\n Right click to configure.\n\nThese pistons use ghost-phasing technology to move blocks. Side effects include shifting of realities, nausea, and phasing")
                    .SetBlockID(1293838, "f53931ef3e14ba8e")
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
                    Vector3.up*0.5f,
                    Vector3.down * 0.5f,
                    Vector3.left * 0.5f,
                    Vector3.right * 0.5f,
                    Vector3.forward * 0.5f,
                    Vector3.back * 0.5f })
                    .AddComponent<ModulePiston>(SetGSOPiston)
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
                    .SetDescription("This piston can push much, MUCH more than the GSO one... and is smoother.\nForged in the valleys of Uberkartoffel potatoes\n Right click to configure.\n\nThese pistons use ghost-phasing technology to move blocks. Side effects include shifting of realities, nausea, and phasing")
                    .SetBlockID(129380, "f5b931ef3e14ba8e")
                    .SetFaction(FactionSubTypes.GC)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(2)
                    .SetPrice(6462)
                    .SetHP(8000)
                    .SetMass(10f)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.GEOp_icon_png)));

                var mat = GameObjectJSON.GetObjectFromGameResources<Material>("GeoCorp_Main");
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
                }).AddComponent<ModulePiston>(SetGeoCorpPiston)
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
                    .SetDescription("A strange set of enforced kinetic plates to make a piston that can stretch to 4 times its compressed state. Size can be changed for whatever needs there are.\n Right click to configure.\n\nThese pistons use ghost-phasing technology to move blocks. Side effects include shifting of realities, nausea, and phasing")
                    .SetBlockID(129381, "e5bc31ef3e14ba8e")
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
                }).AddComponent<ModulePiston>(SetHawkeyePiston)
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
                    .SetDescription("This piston was designed to solve many problems risen from other pistons, however the prototype mover engine showed signs of tearing the fabric of the universe. So that was thrown out. However, this is the most slick & efficient piston there is on market. Sharp movement, and unjustifiable burden strength!\n Right click to configure.\n\nThese pistons use ghost-phasing technology to move blocks. Side effects include shifting of realities, nausea, and phasing")
                    .SetBlockID(1293834, "f63931ef3e14ba8e")
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
                    .AddComponent<ModulePiston>(SetBFPiston)
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

            #endregion Pistons

            #region Swivels

            #region GSO Medium Swivel

            {
                var ControlBlock = new BlockPrefabBuilder("GSOBlock(111)")
                    .SetName("Medium Embedded Swivel")
                    .SetDescription("A configurable swivel that can rotate blocks on a tech.\n Right click to configure.\n\nThese swivels share the same technology as their siblings, however apply it differently. These swivels can also cause identical symptoms under use. Including but not limited to quantum law fracturing, dizziness, and phasing")
                    .SetBlockID(1393838, "f64931ef3e14ba8e")
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
                var gimbal = AddMeshToBlockMover(mat, new Vector3(.5f, 0f, .5f), par, Properties.Resources.swivel_head).AddComponent<GimbalAimer>();
                gimbal.aimClampMaxPercent = 360;
                gimbal.rotationAxis = GimbalAimer.AxisConstraint.Y;

                ControlBlock.SetSize(new IntVector3(2, 1, 2), BlockPrefabBuilder.AttachmentPoints.All)
                    .AddComponent<ModuleSwivel>(SetMediumSwivel)
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
                    .SetDescription("An inline swivel, which's center disk rotates blocks. Just ignore the shell's corners, they're, uhh... squishy\n Right click to configure.\n\nThese swivels share the same technology as their siblings, however apply it differently. These swivels can also cause identical symptoms under use. Including but not limited to quantum law fracturing, dizziness, and phasing")
                    .SetBlockID(1393837, "f74931ef3e14ba8e")
                    .SetFaction(FactionSubTypes.VEN)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(2)
                    .SetPrice(4470)
                    .SetHP(2000)
                    .SetMass(1f)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.swivel_ven_png)));

                var mat = GameObjectJSON.GetObjectFromGameResources<Material>("Venture_Main");
                var par = ControlBlock.Prefab.transform;

                AddMeshToBlockMover(mat, new Vector3(.95f, .95f, .95f), Vector3.zero, par, Properties.Resources.swivel_ven_base);
                var gimbal = AddMeshToBlockMover(mat, Vector3.zero, par, Properties.Resources.swivel_ven_head).AddComponent<GimbalAimer>();
                gimbal.aimClampMaxPercent = 360;
                gimbal.rotationAxis = GimbalAimer.AxisConstraint.Y;

                ControlBlock.SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.All)
                    .AddComponent<ModuleSwivel>(SetInlineSwivel)
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

            #region GSO Small Swivel
            {
                var ControlBlock = new BlockPrefabBuilder("GSOBlock(111)")
                    .SetName("Small Embedded Swivel")
                    .SetDescription("A smaller swivel, quite rushed, but operational hopefully.\n Right click to configure.\n\nThese swivels share the same technology as their siblings, however apply it differently. These swivels can also cause identical symptoms under use. Including but not limited to quantum law fracturing, dizziness, and phasing")
                    .SetBlockID(1393836, "f84931ef3e14ba8e")
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
                var gimbal = AddMeshToBlockMover(mat, Vector3.zero, par, Properties.Resources.swivel_small_head).AddComponent<GimbalAimer>();
                gimbal.aimClampMaxPercent = 360;
                gimbal.rotationAxis = GimbalAimer.AxisConstraint.Y;

                ControlBlock.SetSizeManual(new IntVector3[] { IntVector3.zero },
                    new Vector3[] { Vector3.down * .5f, Vector3.up * .5f })
                    .AddComponent<ModuleSwivel>(SetSmallSwivel)
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
                    .SetName("Stabilizer PiD S.Regulator Dongle")
                    .SetDescription("Right click to configure.\nThis is an extension to the Better Future Stabilizer Computer, adding a form of PiD to fight against idle movement on top through the BFSC's access to hovers, hover jets, and turbines\n\nAfter the release of the Better Future Stabilizer, the Steering Regulator had to be pulled from stock due to fatal conflicts within its presence. However, with high hopes for this new BFSC prototype, this has been repurposed for providing one of its core modules to this block.\n...However they've adapted their own methods, so this is of little use anymore")
                    .SetBlockID(1293839, "12ef3f7f30d4ba8e")
                    .SetFaction(FactionSubTypes.BF)
                    .SetCategory(BlockCategories.Accessories)
                    .SetGrade(0)
                    .SetPrice(3467)
                    .SetHP(200)
                    .SetMass(3.5f)
                    .SetModel(GameObjectJSON.MeshFromFile(Properties.Resources.sr, "sr_base"), true, GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main", true))
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.sr_png)))
                    .SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[]{
                    Vector3.down * 0.5f,
                    Vector3.left * 0.5f,
                    Vector3.right * 0.5f,
                    Vector3.forward * 0.5f,
                    Vector3.back * 0.5f })
                    .AddComponent<ModuleSteeringRegulator>()
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
                    .SetBlockID(1293831, "02ef3f7f30d4ba8e")
                    .SetFaction(FactionSubTypes.GC)
                    .SetCategory(BlockCategories.Wheels)
                    .SetGrade(1)
                    .SetPrice(500)
                    .SetHP(600)
                    .SetMass(0.5f)
                    .SetModel(GameObjectJSON.MeshFromFile(Properties.Resources.GCfrictionpadsmall, "sr_base"), true, GameObjectJSON.GetObjectFromGameResources<Material>("GeoCorp_Main", true))
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
                    .SetBlockID(1293830, "03ef3f7f30d4ba8e")
                    .SetFaction(FactionSubTypes.GC)
                    .SetCategory(BlockCategories.Wheels)
                    .SetGrade(1)
                    .SetPrice(2000)
                    .SetHP(2000)
                    .SetMass(2f)
                    .SetModel(GameObjectJSON.MeshFromFile(Properties.Resources.GCfrictionpadbig, "sr_base"), true, GameObjectJSON.GetObjectFromGameResources<Material>("GeoCorp_Main", true))
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.friction_pad_gc_big_png)))
                    .SetSizeManual(new IntVector3[] { IntVector3.zero, IntVector3.forward, IntVector3.right, new IntVector3(1,0,1) }, new Vector3[] { Vector3.up * 0.5f, new Vector3(1f, 0.5f, 0f), new Vector3(1f, 0.5f, 1f), new Vector3(0f, 0.5f, 1f), })
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

            #endregion Blocks

            GameObject _holder = new GameObject();
            _holder.AddComponent<OptionMenuPiston>();
            _holder.AddComponent<OptionMenuSwivel>();
            _holder.AddComponent<LogGUI>();
            _holder.AddComponent<OptionMenuSteeringRegulator>();
            ManWorldTreadmill.inst.OnBeforeWorldOriginMove.Subscribe(WorldShift);
            UnityEngine.Object.DontDestroyOnLoad(_holder);
        }

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

        internal static bool PistonHeart = false;

        internal static void WorldShift()
        {
            PistonHeart = !PistonHeart;
        }

        internal static GameObject AddMeshToBlockMover(Material mat, Vector3 colliderSize, Vector3 colliderOffset, Transform par, string Mesh)
        {
            GameObject sub = new GameObject("BlockMover Part") { layer = Globals.inst.layerTank };
            sub.AddComponent<MeshFilter>().sharedMesh = GameObjectJSON.MeshFromFile(Mesh, "piston_submesh");
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
            sub.AddComponent<MeshFilter>().sharedMesh = GameObjectJSON.MeshFromFile(Mesh, "piston_submesh");
            sub.AddComponent<MeshRenderer>().sharedMaterial = mat;
            sub.transform.SetParent(par);
            sub.transform.localPosition = objPos;
            sub.transform.localRotation = Quaternion.identity;
            return sub;
        }

        #region SetBlockData

        internal static void SetGSOPiston(ModulePiston piston)
        {
            piston.MaximumBlockPush = 108;
            piston.curves = new AnimationCurve[]
            {
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, .375f, 0f, 0f)), //shaft

                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(.5f, .5f, .6f, .6f), new Keyframe(1f, 1f, 0f, 0f)) //block top
            };
            piston.PartCount = 2;
            piston.StretchSpeed = 0.08f;
            piston.CanModifyStretch = false;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0)
            };
        }

        internal static void SetGeoCorpPiston(ModulePiston piston)
        {
            piston.MaximumBlockPush = 384;
            piston.curves = new AnimationCurve[]
            {
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, .5f, 0f, 0f)),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1.1f, 0f, 0f)),

                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 2f, 0f, 0f))
            };
            piston.PartCount = 3;
            piston.StretchSpeed = 0.03f;
            piston.CanModifyStretch = false;
            piston.StretchModifier = 2; piston.MaxStr = 2;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,2,0),
                new IntVector3(0,2,1),
                new IntVector3(1,2,0),
                new IntVector3(1,2,1)
            };
        }

        internal static void SetHawkeyePiston(ModulePiston piston)
        {
            piston.MaximumBlockPush = 120;
            piston.curves = new AnimationCurve[]
            {
                new AnimationCurve(new Keyframe(0f, 0f, 0f, .5f), new Keyframe(.33333f, .5f, .5f,  0f), new Keyframe(.66667f,  .5f,  0f,  0f), new Keyframe(1f,  .5f, 0f, 0f)), //shaft bottom
                new AnimationCurve(new Keyframe(0f, 0f, 0f, .5f), new Keyframe(.33333f, .5f, .5f, .5f), new Keyframe(.66667f,   1f, .5f,  0f), new Keyframe(1f,   1f, 0f, 0f)), //shaft mid bottom
                new AnimationCurve(new Keyframe(0f, 0f, 0f, .5f), new Keyframe(.33333f, .5f, .5f, .5f), new Keyframe(.66667f,   1f, .5f,  1f), new Keyframe(1f,   2f, 1f, 0f)), //shaft mid top
                new AnimationCurve(new Keyframe(0f, 0f, 0f, .5f), new Keyframe(.33333f, .5f, .5f,  1f), new Keyframe(.66667f, 1.5f,  1f,  1f), new Keyframe(1f, 2.5f, 1f, 0f)), //shaft top

                new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(.33333f, 1f, 1f, 1f), new Keyframe(.66667f, 2f, 1f, 1f), new Keyframe(1f, 3f, 1f, 0f)), //block top
            };
            piston.PartCount = 5;
            piston.StretchSpeed = 0.025f;
            piston.CanModifyStretch = true;
            piston.StretchModifier = 3; piston.MaxStr = 3;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0),
                new IntVector3(0,0,-1)
            };
        }

        internal static void SetBFPiston(ModulePiston piston)
        {
            piston.MaximumBlockPush = 65535;
            piston.curves = new AnimationCurve[]
            {
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f)) //block top
            };
            piston.PartCount = 1;
            piston.StretchSpeed = 0.12f;
            piston.CanModifyStretch = false;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0)
            };
        }

        internal static void SetMediumSwivel(ModuleSwivel swivel)
        {
            swivel.MaximumBlockPush = 196;
            swivel.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(new Keyframe(0f,0f,0f,1f), new Keyframe(360f,360f,1f,0f))
            };
            swivel.PartCount = 1;
            swivel.CanModifySpeed = true;
            swivel.RotateSpeed = 5;
            swivel.MaxSpeed = 12f;
            swivel.LockAngle = false;
            swivel.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0),
                new IntVector3(1,1,0),
                new IntVector3(0,1,1),
                new IntVector3(1,1,1)
            };
        }
        internal static void SetInlineSwivel(ModuleSwivel swivel)
        {
            swivel.BreakOnCab = true;
            swivel.MaximumBlockPush = 92;
            swivel.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(new Keyframe(0f,0f,0f,1f), new Keyframe(360f,360f,1f,0f))
            };
            swivel.PartCount = 1;
            swivel.CanModifySpeed = true;
            swivel.RotateSpeed = 7.5f;
            swivel.MaxSpeed = 15;
            swivel.LockAngle = false;
            swivel.startblockpos = new IntVector3[]
            {
                new IntVector3(0,0,-1),
                new IntVector3(1,0,0),
                new IntVector3(0,0,1),
                new IntVector3(-1,0,0)
            };
        }
        internal static void SetSmallSwivel(ModuleSwivel swivel)
        {
            //swivel.BreakOnCab = true;
            swivel.MaximumBlockPush = 128;
            swivel.rotCurves = new AnimationCurve[]
            {
                new AnimationCurve(new Keyframe(0f,0f,0f,1f), new Keyframe(360f,360f,1f,0f))
            };
            swivel.PartCount = 1;
            swivel.CanModifySpeed = true;
            swivel.RotateSpeed = 5f;
            swivel.MaxSpeed = 8;
            swivel.LockAngle = false;
            swivel.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0)
            };
        }

        #endregion SetBlockData

        public static string LogAllComponents(Transform SearchIn, string Indenting = "")
        {
            string result = "";
            Component[] c = SearchIn.GetComponents<Component>();
            foreach (Component comp in c)
            {
                result += "\n" + Indenting + comp.name + " : " + comp.GetType().Name;
                if (comp is MeshRenderer) result += " : Material (" + ((MeshRenderer)comp).material.name + ")";
            }
            for (int i = SearchIn.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = SearchIn.transform.GetChild(i);
                result += LogAllComponents(child, Indenting + "  ");
            }
            return result;
        }
    }

    internal class LogGUI : MonoBehaviour
    {
        private readonly int ID = 45925;

        private bool visible = false;

        private TankBlock module;

        private string Log = "";

        private Rect win;

        private void Update()
        {
            if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetKeyDown(KeyCode.Backslash))
            {
                win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 100f, 600f, 300f);
                try
                {
                    module = Singleton.Manager<ManPointer>.inst.targetVisible.block;
                    Log = Class1.LogAllComponents(module.transform);
                }
                catch
                {
                    //Console.WriteLine(e);
                    module = null;
                    Log = "";
                }
                visible = module;
            }
        }

        private void OnGUI()
        {
            if (!visible || !module)
            {
                return;
            }

            try
            {
                win = GUI.Window(ID, win, new GUI.WindowFunction(DoWindow), "Dump");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private Vector2 scroll = Vector2.zero;

        private void DoWindow(int id)
        {
            if (module == null)
            {
                visible = false;
                return;
            }
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label(Log);
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }
    }

    internal class Patches
    {
        private static FieldInfo H_mASS, H_mTCU, H_mTCR, H_pB,
            F_mTSR, F_mASS, F_mE, F_mPB,
            B_mE, B_mASS, B_mFSC, B_mPB;

        static Patches()
        {
            try
            {
                BindingFlags b = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
                {
                    Type T = typeof(HoverJet);
                    H_mASS = T.GetField("m_AutoStabiliseStrength", b);
                    H_mTCU = T.GetField("m_ThrustContributionUp", b);
                    H_mTCR = T.GetField("m_ThrustContributionRight", b);
                    H_pB = T.GetField("parentBlock", b);
                }
                {
                    Type T = typeof(FanJet);
                    F_mPB = T.GetField("m_ParentBlock", b);
                    F_mASS = T.GetField("m_AutoStabiliseStrength", b);
                    F_mTSR = T.GetField("m_TargetSpinRate", b);
                    F_mE = T.GetField("m_Effector", b);
                }
                {
                    Type T = typeof(BoosterJet);
                    B_mE = T.GetField("m_Effector", b);
                    B_mASS = T.GetField("m_AutoStabiliseStrength", b);
                    B_mFSC = T.GetField("m_FireStrengthCurrent", b);
                    B_mPB = T.GetField("m_ParentBlock", b);
                }
                {
                    m_AwaitingPhysicsReset = typeof(Tank).GetField("m_AwaitingPhysicsReset", b);
                }
            }
            catch
            {
            }
        }

        [HarmonyPatch(typeof(BlockManager), "AddBlockToTech")]
        private static class BlockManagerFix
        {
            private static void Prefix(ref BlockManager __instance, ref TankBlock block, IntVector3 localPos)
            {
                foreach (TankBlock _b in __instance.IterateBlocks())
                {
                    var module = _b.GetComponent<ModuleBlockMover>();
                    if (module)
                    {
                        module.BeforeBlockAdded(block);
                    }
                }
            }
        }

        //SR

        [HarmonyPatch(typeof(HoverJet), "AutoStabiliseTank")]
        private static class HoverJetStabilizePatch
        {
            private static void Postfix(ref HoverJet __instance, ref float driveInput, ref float turnInput)
            {
                ModuleSteeringRegulator sr = ((TankBlock)H_pB.GetValue(__instance)).tank.gameObject.GetComponentInChildren<ModuleSteeringRegulator>();
                if (sr != null && sr.CanWork)
                {
                    float ___m_AutoStabiliseStrength = (float)H_mASS.GetValue(__instance);
                    Vector3 lhs = Quaternion.Inverse(sr.rbody.rotation) * sr.lhs * sr.HoverMod;
                    float num = 1f;
                    driveInput -= ___m_AutoStabiliseStrength * Vector3.Dot(lhs, (Vector3)H_mTCU.GetValue(__instance));
                    driveInput = Mathf.Clamp(driveInput, -num, num);
                    turnInput -= ___m_AutoStabiliseStrength * Vector3.Dot(lhs, (Vector3)H_mTCR.GetValue(__instance));
                    turnInput = Mathf.Clamp(turnInput, -num, num);
                }
            }
        }

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


        private static FieldInfo m_AwaitingPhysicsReset;
        private static Vector3 oldCOM;
        private static bool CenterToOld = false;
        [HarmonyPatch(typeof(Tank), "ResetPhysics")]
        private static class CenterCOM
        {
            private static void Prefix(Tank __instance)
            {
                if (__instance.IsAnchored||!(bool)m_AwaitingPhysicsReset.GetValue(__instance)) return;
                CenterToOld = true;
                oldCOM = __instance.rbody.worldCenterOfMass;
            }
            private static void Postfix(Tank __instance)
            {
                if (!CenterToOld) return;
                CenterToOld = false;
                __instance.transform.position = (__instance.rbody.position + oldCOM - __instance.rbody.worldCenterOfMass);
            }
        }

        [HarmonyPatch(typeof(FanJet), "AutoStabiliseTank")]
        private static class FanJetStabilizePatch
        {
            private static void Postfix(ref FanJet __instance)
            {
                ModuleSteeringRegulator sr = ((TankBlock)F_mPB.GetValue(__instance)).tank.gameObject.GetComponentInChildren<ModuleSteeringRegulator>();
                if (sr != null && sr.CanWork)
                {
                    float ___m_AutoStabiliseStrength = (float)F_mASS.GetValue(__instance);
                    if (___m_AutoStabiliseStrength > 0f)
                    {
                        Rigidbody rbody = sr.rbody;
                        Vector3 forward = ((Transform)F_mE.GetValue(__instance)).forward;
                        Vector3 pointVelocity = sr.lhs * sr.TurbineMod;
                        float num = ___m_AutoStabiliseStrength * Vector3.Dot(pointVelocity, forward);
                        //if (Mathf.Abs(num) < 0.0125f)
                        //{
                        //    num = 0f;
                        //}
                        //else
                        //{
                        //    num -= Mathf.Sign(num) * 0.0125f;
                        //}
                        __instance.SetSpin(num + (float)F_mTSR.GetValue(__instance));
                    }
                }
            }
        }

        [HarmonyPatch(typeof(BoosterJet), "AutoStabiliseTank")]
        private static class BoosterJetStabilizePatch
        {
            private static void Postfix(ref BoosterJet __instance)
            {
                ModuleSteeringRegulator sr = ((TankBlock)B_mPB.GetValue(__instance)).tank.gameObject.GetComponentInChildren<ModuleSteeringRegulator>();
                if (sr != null && sr.CanWork)
                {
                    var ___m_AutoStabiliseStrength = (float)B_mASS.GetValue(__instance);
                    if (___m_AutoStabiliseStrength > 0f)
                    {
                        Rigidbody rbody = sr.rbody;
                        Vector3 forward = ((Transform)B_mE.GetValue(__instance)).forward;
                        Vector3 pointVelocity = sr.lhs * sr.JetMod;
                        float num = ___m_AutoStabiliseStrength * Vector3.Dot(pointVelocity, forward) - .075f;
                        if (num < 0f)
                        {
                            num = 0f;
                        }
                        var cs = (float)B_mFSC.GetValue(__instance);
                        B_mFSC.SetValue(__instance, Mathf.Clamp(cs + num, cs, 1f));
                    }
                }
            }
        }
    }

    internal class OptionMenuPiston : MonoBehaviour
    {
        private readonly int ID = 7787;

        private bool visible = false;

        private ModulePiston module;

        private Rect win;
        private readonly string[] toggleOptions = new string[] { "Normal", "DelayedInput", "PreferOpen", "PreferClosed" };
        private readonly string[] notToggleOptions = new string[] { "Normal", "InvertInput" };

        private void Update()
        {
            if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetMouseButtonDown(1))
            {
                win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 175f, 250f, 350f);
                try
                {
                    module = Singleton.Manager<ManPointer>.inst.targetVisible.block.GetComponent<ModulePiston>();
                }
                catch
                {
                    //Console.WriteLine(e);
                    module = null;
                }
                visible = module;
                IsSettingKeybind = false;
            }
        }

        private void OnGUI()
        {
            if (!visible || !module)
            {
                return;
            }

            try
            {
                win = GUI.Window(ID, win, new GUI.WindowFunction(DoWindow), module.gameObject.name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private bool IsSettingKeybind;

        private Vector2 Scroll = Vector2.zero;

        private void DoWindow(int id)
        {
            Scroll = GUILayout.BeginScrollView(Scroll);
            if (module == null)
            {
                visible = false;
                return;
            }
            if (IsSettingKeybind)
            {
                var e = Event.current;
                if (e.isKey)
                {
                    module.trigger = e.keyCode;
                    IsSettingKeybind = false;
                }
            }
            GUILayout.Label("Keybind input");
            IsSettingKeybind = GUILayout.Button(IsSettingKeybind ? "Press a key for use" : module.trigger.ToString()) != IsSettingKeybind;

            if (module.CanModifyStretch)
            {
                GUILayout.Label("Maximum Stretch (change while closed) : " + module.StretchModifier.ToString());
                int temp = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.StretchModifier, 1, module.MaxStr));
                if (module.open == 0)
                {
                    module.StretchModifier = temp;
                }
            }

            module.IsToggle = GUILayout.Toggle(module.IsToggle, "Is toggle");

            module.InverseTrigger = (byte)GUILayout.SelectionGrid(module.InverseTrigger, (module.IsToggle ? toggleOptions : notToggleOptions), 2);

            module.LocalControl = GUILayout.Toggle(module.LocalControl, "Local to tech");

            GUILayout.Label("Piston : " + module.block.cachedLocalPosition.ToString());

            if (module.CurrentCellPush > module.MaximumBlockPush)
            {
                GUILayout.Label(" The piston is overburdened! (>" + module.MaximumBlockPush.ToString() + ")");
            }
            else if (module.CurrentCellPush == -1)
            {
                GUILayout.Label(" The piston is structurally locked!");
            }
            else if (module.CurrentCellPush == -2)
            {
                GUILayout.Label(" This piston cannot move any cabs!");
            }
            else
            {
                GUILayout.Label(" Burden : " + module.CurrentCellPush.ToString());
            }

            if (GUILayout.Button("Close"))
            {
                visible = false;
                IsSettingKeybind = false;
                module = null;
            }
            GUI.DragWindow();
            GUILayout.EndScrollView();
        }
    }

    internal class OptionMenuSwivel : MonoBehaviour
    {
        private readonly int ID = 7782;

        private bool visible = false;

        private ModuleSwivel module;

        private Rect win;
        private readonly string[] modeOptions = new string[] { "Positional", "Directional", "Speed", "On/Off", "Target Aim", "Steering", "Player Aim", "Velocity Aim", "Cycle", "Throttle" };

        private void Update()
        {
            if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetMouseButtonDown(1))
            {
                win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 175f, 450f, 600f);
                try
                {
                    module = Singleton.Manager<ManPointer>.inst.targetVisible.block.GetComponent<ModuleSwivel>();
                }
                catch
                {
                    //Console.WriteLine(e);
                    module = null;
                }
                visible = module;
                IsSettingKeybind = false;
            }
        }

        private void OnGUI()
        {
            if (!visible || !module)
            {
                return;
            }

            try
            {
                win = GUI.Window(ID, win, new GUI.WindowFunction(DoWindow), module.gameObject.name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private bool IsSettingKeybind;

        private int SetButton = -1;

        private Vector2 Scroll = Vector2.zero;

        private void DoWindow(int id)
        {
            Scroll = GUILayout.BeginScrollView(Scroll);
            if (module == null)
            {
                visible = false;
                return;
            }
            if (IsSettingKeybind)
            {
                var e = Event.current;
                if (e.isKey)
                {
                    switch (SetButton)
                    {
                        case 0: module.trigger1 = e.keyCode; break;
                        case 1: module.trigger2 = e.keyCode; break;
                    }
                    IsSettingKeybind = false;
                    SetButton = -1;
                }
            }
            GUILayout.Label("Clockwise Key");
            IsSettingKeybind = GUILayout.Button(IsSettingKeybind && SetButton == 0 ? "Press a key for use" : module.trigger1.ToString()) != IsSettingKeybind;
            if (IsSettingKeybind && SetButton == -1)
            {
                SetButton = 0;
            }

            GUILayout.Label("CounterClockwise Key");
            IsSettingKeybind = GUILayout.Button(IsSettingKeybind && SetButton == 1 ? "Press a key for use" : module.trigger2.ToString()) != IsSettingKeybind;
            if (IsSettingKeybind && SetButton == -1)
            {
                SetButton = 1;
            }

            if (!IsSettingKeybind && SetButton != -1)
            {
                SetButton = -1;
            }

            module.LockAngle = GUILayout.Toggle(module.LockAngle, "Restrict Angle");
            float Angle = (Mathf.Repeat(module.AngleCenter + 180, 360) - 180);
            GUILayout.Label("Center of Limit: " + Angle.ToString());
            module.AngleCenter = Mathf.RoundToInt((GUILayout.HorizontalSlider(Angle, -180, 179) + 360) % 360);
            GUILayout.Label("Range of Limit: " + module.AngleRange.ToString());
            module.AngleRange = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.AngleRange, 0, 179 - module.RotateSpeed) % 360);

            module.mode = (ModuleSwivel.Mode)GUILayout.SelectionGrid((int)module.mode, modeOptions, 2);

            if (module.CanModifySpeed)
            {
                GUILayout.Label("Rotation Speed : " + module.RotateSpeed.ToString());
                module.RotateSpeed = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.RotateSpeed * 2, 1, module.MaxSpeed * 2)) * .5f;
            }
            Angle = (Mathf.Repeat(module.CurrentAngle + 180, 360) - 180);

            GUILayout.Label("Angle : " + ((int)Angle).ToString());
            var newAngle = GUILayout.HorizontalSlider(Angle, -180, 179);
            if (newAngle != Angle)
            {
                module.CurrentAngle = Mathf.Clamp(newAngle, Angle - module.MaxSpeed, Angle + module.MaxSpeed) - 360;
            }

            module.LocalControl = GUILayout.Toggle(module.LocalControl, "Local to tech");

            if (module.mode != ModuleSwivel.Mode.AimAtPlayer && module.mode != ModuleSwivel.Mode.AimAtVelocity)
            {
                if (module.mode != ModuleSwivel.Mode.Directional)
                {
                    GUILayout.Label("Start Pause: " + module.StartDelay.ToString());
                    module.StartDelay = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.StartDelay, 0, 360));
                }
                if (module.mode == ModuleSwivel.Mode.Cycle || module.mode == ModuleSwivel.Mode.Directional || module.mode == ModuleSwivel.Mode.OnOff)
                {
                    GUILayout.Label("CW Limiter Pause: " + module.CWDelay.ToString());
                    module.CWDelay = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.CWDelay, 0, 360));
                    GUILayout.Label("CCW Limiter Pause: " + module.CCWDelay.ToString());
                    module.CCWDelay = Mathf.RoundToInt(GUILayout.HorizontalSlider(module.CCWDelay, 0, 360));
                }
            }

            GUILayout.Label("Swivel : " + module.block.cachedLocalPosition.ToString());

            if (module.CurrentCellPush > module.MaximumBlockPush)
            {
                GUILayout.Label(" The swivel is overburdened! (>" + module.MaximumBlockPush.ToString() + ")");
            }
            else if (module.CurrentCellPush == -1)
            {
                GUILayout.Label(" The swivel is structurally locked!");
            }
            else if (module.CurrentCellPush == -2)
            {
                GUILayout.Label(" This swivel cannot move any cabs!");
            }
            else
            {
                GUILayout.Label(" Burden : " + module.CurrentCellPush.ToString());
            }

            if (GUILayout.Button("Close"))
            {
                visible = false;
                IsSettingKeybind = false;
                module = null;
            }
            GUI.DragWindow();
            GUILayout.EndScrollView();
        }
    }

    internal class OptionMenuSteeringRegulator : MonoBehaviour
    {
        private readonly int ID = 7788;

        private bool visible = false;

        private ModuleSteeringRegulator module;

        private Rect win;

        private void Update()
        {
            if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetMouseButtonDown(1))
            {
                win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 100f, 200f, 200f);
                try
                {
                    module = Singleton.Manager<ManPointer>.inst.targetVisible.block.GetComponent<ModuleSteeringRegulator>();
                }
                catch
                {
                    //Console.WriteLine(e);
                    module = null;
                }
                visible = module;
            }
        }

        private void OnGUI()
        {
            if (!visible || !module)
            {
                return;
            }

            try
            {
                win = GUI.Window(ID, win, new GUI.WindowFunction(DoWindow), "Stabiliser PiD");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void DoWindow(int id)
        {
            if (module == null)
            {
                visible = false;
                return;
            }
            GUILayout.Label("Sensitivity RADIUS : " + module.MaxDist.ToString());
            module.MaxDist = Mathf.Round(GUILayout.HorizontalSlider(module.MaxDist * 4, 0f, 20f)) * .25f;
            GUILayout.Label("Hover PiD Effect");
            module.HoverMod = Mathf.Round(GUILayout.HorizontalSlider(module.HoverMod * 2f, 0f, 15f)) * .5f;
            GUILayout.Label("Steering Jet PiD Effect");
            module.JetMod = Mathf.Round(GUILayout.HorizontalSlider(module.JetMod, 0f, 15f));
            GUILayout.Label("Turbine PiD Effect");
            module.TurbineMod = Mathf.Round(GUILayout.HorizontalSlider(module.TurbineMod * 2f, 0f, 15f)) * .5f;
            if (GUILayout.Button("Close"))
            {
                visible = false;
                module = null;
            }
            GUI.DragWindow();
        }
    }
}