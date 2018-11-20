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

            {
                var ControlBlock = new BlockPrefabBuilder("GSOBlock(111)")
                    .SetName("Piston Block")
                    .SetDescription("A configurable piston that can push and pull blocks on a tech. Right click to configure")
                    .SetBlockID(1293838, "f53931ef3e14ba8e")
                    .SetFaction(FactionSubTypes.GSO)
                    .SetCategory(BlockCategories.Base)
                    .SetGrade(3)
                    .SetPrice(5142)
                    .SetHP(3000)
                    .SetMass(3.5f)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.piston_icon_png)));

                var mat = GameObjectJSON.GetObjectFromGameResources<Material>("GSO_Main");

                GameObject mbase = new GameObject("base"), mshaft = new GameObject("shaft"), mhead = new GameObject("head");
                mbase.layer = Globals.inst.layerTank;
                mshaft.layer = Globals.inst.layerTank;
                mhead.layer = Globals.inst.layerTank;

                mbase.AddComponent<MeshFilter>().sharedMesh = GameObjectJSON.MeshFromFile(Properties.Resources.piston_base, "piston_base");
                mbase.AddComponent<MeshRenderer>().sharedMaterial = mat;
                var mbc = mbase.AddComponent<BoxCollider>();
                mbc.size = new Vector3(.9f, .725f, .9f);
                mbc.center = new Vector3(0f, -.125f, 0f);
                mbase.transform.SetParent(ControlBlock.Prefab.transform);

                mshaft.AddComponent<MeshFilter>().sharedMesh = GameObjectJSON.MeshFromFile(Properties.Resources.piston_shaft, "piston_shaft");
                mshaft.AddComponent<MeshRenderer>().sharedMaterial = mat;
                mshaft.transform.SetParent(ControlBlock.Prefab.transform);

                mhead.AddComponent<MeshFilter>().sharedMesh = GameObjectJSON.MeshFromFile(Properties.Resources.piston_head, "piston_head");
                mhead.AddComponent<MeshRenderer>().sharedMaterial = mat;
                var mhc = mhead.AddComponent<BoxCollider>();
                mhc.size = new Vector3(.8f, .9f, .8f);
                mhead.transform.SetParent(ControlBlock.Prefab.transform);

                ControlBlock.SetSizeManual(new IntVector3[] { IntVector3.zero }, new Vector3[]{
                    Vector3.up*0.5f,
                    Vector3.down * 0.5f,
                    Vector3.left * 0.5f,
                    Vector3.right * 0.5f,
                    Vector3.forward * 0.5f,
                    Vector3.back * 0.5f })
                    .AddComponent<ModulePiston>()
                    .RegisterLater();
                GameObject _holder = new GameObject();
                _holder.AddComponent<OptionMenu>();
                UnityEngine.Object.DontDestroyOnLoad(_holder);

                CustomRecipe.RegisterRecipe(
                    new CustomRecipe.RecipeInput[]
                    {
                    new CustomRecipe.RecipeInput((int)ChunkTypes.GluonBeam, 1),
                    new CustomRecipe.RecipeInput((int)ChunkTypes.SensoryTransmitter, 1),
                    new CustomRecipe.RecipeInput((int)ChunkTypes.PlubonicAlloy, 2),
                    },
                    new CustomRecipe.RecipeOutput[]
                    {
                    new CustomRecipe.RecipeOutput(1293838)
                    });
            }
            {
                var SteeringRegulator = new BlockPrefabBuilder("VENSteeringHover (111)")
                    .SetName("Steering Regulator")
                    .SetDescription("Stabilize hovertechs to keep from drifting in to the sunset")
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
        }
    }

    internal class Patches
    {
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
        static MethodInfo airMethod = typeof(ModuleBooster).GetMethod("DriveControlAirborne", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
            groundMethod = typeof(ModuleBooster).GetMethod("DriveControlGrounded", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPatch(typeof(ModuleBooster), "DriveControlInput")]
        private static class OverrideBoosterInput
        {
            private static bool Prefix(ModuleBooster __instance, float drive, float turn)
            {
                var controlBlock = __instance.block.tank.GetComponentInChildren<ModuleSteeringRegulator>();
                if (controlBlock == null || !__instance.block.tank.grounded)
                {
                    return true;
                }
                else
                {
                    var pv = controlBlock.PositionalFixingVector;
                    bool UseGrounded = controlBlock.UseGroundMode(pv);
                    if (UseGrounded)
                    {
                        controlBlock.SetColor(Color.magenta);
                        groundMethod.Invoke(__instance, new object[] { drive + pv.y, turn + pv.z});
                    }
                    else
                    {
                        controlBlock.SetColor(Color.cyan);
                        airMethod.Invoke(__instance, new object[] { drive + pv.y, turn - pv.x });
                    }
                    return false;
                }
            }
        }
    }

    class OptionMenu : MonoBehaviour
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
                win = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 200f, 200f, 300f);
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
                win = GUI.Window(ID, win, new GUI.WindowFunction(DoWindow), "Block Configuration");
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

            module.IsToggle = GUILayout.Toggle(module.IsToggle, "Is toggle");

            module.InverseTrigger = (byte)GUILayout.SelectionGrid(module.InverseTrigger, (module.IsToggle ? toggleOptions : notToggleOptions), 2);

            module.LocalControl = GUILayout.Toggle(module.LocalControl, "Local to tech");

            GUILayout.Label("Piston : " + module.block.cachedLocalPosition.ToString());
            GUILayout.Label(" Burden : " + module.CurrentCellPush.ToString());
            if (module.CurrentCellPush > ModulePiston.MaxBlockPush)
            {
                GUILayout.Label("- The piston is overburdened! (>"+ModulePiston.MaxBlockPush.ToString()+")");
            }
            else if (module.CurrentCellPush == -1)
            {
                GUILayout.Label("- The piston is structurally locked!");
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
}