using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Nuterra.BlockInjector;
using Harmony;
using System.Reflection;

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
                    .SetMass(1.25f)
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
                view.transform.localPosition = new Vector3(0f,0.175f,-0.1f);
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
            #endregion
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

                AddMeshToPiston(mat, new Vector3(.95f, .725f, .95f), new Vector3(0f, -.125f, 0f), par, Properties.Resources.piston_base);
                AddMeshToPiston(mat, new Vector3(.75f, .8f, .75f), Vector3.zero, par, Properties.Resources.piston_shaft);
                AddMeshToPiston(mat, new Vector3(.8f, .9f, .8f), Vector3.zero, par, Properties.Resources.piston_head);

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
            #endregion
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

                AddMeshToPiston(mat, new Vector3(1.99f, .95f, 1.99f), new Vector3(.5f, 0f, .5f), par, Properties.Resources.GEOp_blockbottom);
                AddMeshToPiston(mat, new Vector3(1.6f, 1f, 1.6f), new Vector3(.5f, .5f, .5f), par, Properties.Resources.GEOp_shaftbottom);
                AddMeshToPiston(mat, new Vector3(1.3f, 1f, 1.3f), new Vector3(.5f, .5f, .5f), par, Properties.Resources.GEOp_shafttop);
                AddMeshToPiston(mat, new Vector3(1.99f, .95f, 1.99f), new Vector3(.5f, 1f, .5f), par, Properties.Resources.GEOp_blocktop);

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
                })  .AddComponent<ModulePiston>(SetGeoCorpPiston)
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
            #endregion
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

                AddMeshToPiston(mat, new Vector3(.95f, .95f, .95f), Vector3.zero, par, Properties.Resources.HEp_blockbottom);
                AddMeshToPiston(mat, new Vector3(.875f, .47f, .875f), Vector3.down * .25f, par, Properties.Resources.HEp_shaftbottom);
                AddMeshToPiston(mat, new Vector3(.75f, .95f, .75f), Vector3.zero, par, Properties.Resources.HEp_shaftmidb);
                AddMeshToPiston(mat, new Vector3(.75f, .95f, .75f), Vector3.zero, par, Properties.Resources.HEp_shaftmidt);
                AddMeshToPiston(mat, new Vector3(.875f, .47f, .875f), Vector3.up * .25f, par, Properties.Resources.HEp_shafttop);
                AddMeshToPiston(mat, new Vector3(.95f, .95f, .95f), Vector3.zero, par, Properties.Resources.HEp_blocktop);

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
            #endregion
            #region Steering Regulator
            {
                var SteeringRegulator = new BlockPrefabBuilder("BF_Block(111)")
                    .SetName("Stabilizer PiD S.Regulator Dongle")
                    .SetDescription("Right click to configure.\nThis is an extension to the Better Future Stabilizer Computer, adding a form of PiD to fight against idle movement on top through the BFSC's access to hovers, hover jets, and turbines\n\nAfter the release of the Better Future Stabilizer, the Steering Regulator had to be pulled from stock due to fatal conflicts within its presence. However, with high hopes for this new BFSC prototype, this has been repurposed for providing one of its core modules to this block. In case the idea was not readily accepted by the BFSC producers to integrate themselves, of course...")
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
            #endregion
            #endregion

            GameObject _holder = new GameObject();
            _holder.AddComponent<OptionMenuPiston>();
            _holder.AddComponent<LogGUI>();
            _holder.AddComponent<OptionMenuSteeringRegulator>();
            UnityEngine.Object.DontDestroyOnLoad(_holder);
        }

        internal static void AddMeshToPiston(Material mat, Vector3 colliderSize, Vector3 colliderOffset, Transform par, string Mesh)
        {
            GameObject sub = new GameObject("Piston Part");
            sub.layer = Globals.inst.layerTank;
            sub.AddComponent<MeshFilter>().sharedMesh = GameObjectJSON.MeshFromFile(Mesh, "piston_submesh");
            sub.AddComponent<MeshRenderer>().sharedMaterial = mat;

            var mhc = sub.AddComponent<BoxCollider>();
            mhc.size = colliderSize;
            mhc.center = colliderOffset;
            sub.transform.SetParent(par);
            sub.transform.localPosition = Vector3.zero;
            sub.transform.localRotation = Quaternion.identity;
        }

        internal static void SetGSOPiston(ModulePiston piston)
        {
            piston.MaximumBlockPush = 72;
            piston.curves = new AnimationCurve[]
            {
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, .375f, 0f, 0f)), //shaft

                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(.5f, .5f, .6f, .6f), new Keyframe(1f, 1f, 0f, 0f)) //block top
            };
            piston.StretchSpeed = 0.08f;
            piston.CanModifyStretch = false;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0)
            };
        }

        internal static void SetGeoCorpPiston(ModulePiston piston)
        {
            piston.MaximumBlockPush = 256;
            piston.curves = new AnimationCurve[]
            {
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, .5f, 0f, 0f)),
                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1.1f, 0f, 0f)),

                new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 2f, 0f, 0f))
            };
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
            piston.MaximumBlockPush = 80;
            piston.curves = new AnimationCurve[]
            {
                new AnimationCurve(new Keyframe(0f, 0f, 0f, .5f), new Keyframe(.33333f, .5f, .5f,  0f), new Keyframe(.66667f,  .5f,  0f,  0f), new Keyframe(1f,  .5f, 0f, 0f)), //shaft bottom
                new AnimationCurve(new Keyframe(0f, 0f, 0f, .5f), new Keyframe(.33333f, .5f, .5f, .5f), new Keyframe(.66667f,   1f, .5f,  0f), new Keyframe(1f,   1f, 0f, 0f)), //shaft mid bottom
                new AnimationCurve(new Keyframe(0f, 0f, 0f, .5f), new Keyframe(.33333f, .5f, .5f, .5f), new Keyframe(.66667f,   1f, .5f,  1f), new Keyframe(1f,   2f, 1f, 0f)), //shaft mid top
                new AnimationCurve(new Keyframe(0f, 0f, 0f, .5f), new Keyframe(.33333f, .5f, .5f,  1f), new Keyframe(.66667f, 1.5f,  1f,  1f), new Keyframe(1f, 2.5f, 1f, 0f)), //shaft top

                new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(.33333f, 1f, 1f, 1f), new Keyframe(.66667f, 2f, 1f, 1f), new Keyframe(1f, 3f, 1f, 0f)), //block top
            };
            piston.StretchSpeed = 0.025f;
            piston.CanModifyStretch = true;
            piston.StretchModifier = 3; piston.MaxStr = 3;
            piston.startblockpos = new IntVector3[]
            {
                new IntVector3(0,1,0),
                new IntVector3(0,0,-1)
            };
        }

        public static string LogAllComponents(Transform SearchIn, string Indenting = "")
        {
            string result = "";
            Component[] c = SearchIn.GetComponents<Component>();
            foreach (Component comp in c)
            {
                result += "\n" + Indenting + comp.name + " : " + comp.GetType().Name;
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
        private int ID = 45925;

        private bool visible = false;

        private TankBlock module;

        string Log = "";

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
            if (!visible || !module) return;
            try
            {
                win = GUI.Window(ID, win, new GUI.WindowFunction(DoWindow), "Dump");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        Vector2 scroll = Vector2.zero;

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
        static FieldInfo H_mASS, H_mTCU, H_mTCR, H_pB,
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
            }
            catch(Exception E)
            {

            }
            }
        [HarmonyPatch(typeof(BlockManager), "AddBlock")]
        private static class BlockManagerFix
        {
            private static void Prefix(ref BlockManager __instance, ref TankBlock block, IntVector3 localPos)
            {
                foreach (TankBlock _b in __instance.IterateBlocks())
                {
                    var module = _b.GetComponent<ModulePiston>();
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
                        if (Mathf.Abs(num) < 0.1f)
                        {
                            num = 0f;
                        }
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
                        float num = ___m_AutoStabiliseStrength * Vector3.Dot(pointVelocity, forward);
                        if (num < 0.1f)
                        {
                            num = 0f;
                        }
                        var cs = (float)B_mFSC.GetValue(__instance);
                        B_mFSC.SetValue(__instance, Mathf.Clamp(cs+num,cs, 1f));
                    }
                }
            }
        }
    }

    class OptionMenuPiston : MonoBehaviour
    {
        
        private int ID = 7787;

        private bool visible = false;

        private ModulePiston module;

        private Rect win;

        string[] toggleOptions = new string[] { "Normal", "DelayedInput", "PreferOpen", "PreferClosed" };
        string[] notToggleOptions = new string[] { "Normal", "InvertInput" };
        private void Update()
        {
            if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetMouseButtonDown(1))
            {
                win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 175f, 200f, 350f);
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
            if (!visible || !module) return;
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

        private void DoWindow(int id)
        {
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
                GUILayout.Label(" The piston is overburdened! (>"+module.MaximumBlockPush.ToString()+")");
            }
            else if (module.CurrentCellPush == -1)
            {
                GUILayout.Label(" The piston is structurally locked!");
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
        }
    }
    class OptionMenuSteeringRegulator : MonoBehaviour
    {

        private int ID = 7788;

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
            if (!visible || !module) return;
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
            module.TurbineMod = GUILayout.HorizontalSlider(module.TurbineMod * 2f, 0f, 15f) * .5f;
            if (GUILayout.Button("Close"))
            {
                visible = false;
                module = null;
            }
            GUI.DragWindow();
        }
    }
}