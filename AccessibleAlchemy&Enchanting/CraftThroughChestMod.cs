using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using static AreaManager;

namespace CraftThroughChest
{    
    [BepInPlugin(ID, NAME, VERSION)]
    public class CraftThroughChestMod : BaseUnityPlugin
    {
        const string ID = "com.apernicus.craftthroughchest";
        const string NAME = "Craft Through Chest";
        const string VERSION = "0.1.0";

        public static CraftThroughChestMod Instance;
        
        public static readonly Dictionary<AreaEnum, string> StashAreaToStashUID = new Dictionary<AreaEnum, string>
        {
            {
                AreaEnum.Berg,
                "ImqRiGAT80aE2WtUHfdcMw"
            },
            {
                AreaEnum.CierzoVillage,
                "ImqRiGAT80aE2WtUHfdcMw"
            },
            {
                AreaEnum.Levant,
                "ZbPXNsPvlUeQVJRks3zBzg"
            },
            {
                AreaEnum.Monsoon,
                "ImqRiGAT80aE2WtUHfdcMw"
            },
            {
                AreaEnum.Harmattan,
                "ImqRiGAT80aE2WtUHfdcMw"
            },
            {
                AreaEnum.NewSirocco,
                "IqUugGqBBkaOcQdRmhnMng"
            }
        };

        public static ManualLogSource MyLogger = BepInEx.Logging.Logger.CreateLogSource(NAME);

        internal void Awake()
        {
            try
            {
                Instance = this;
                var harmony = new Harmony(ID);
                harmony.PatchAll();
            }
            catch (Exception ex)
            {
                MyLogger.LogError("Awake:" + ex.Message);
            }
        }

        public static TreasureChest CurrentStash()
        {
            if (!CheckStash())
            {
                return null;
            }
            
            AreaEnum areaN = (AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
            return (TreasureChest)ItemManager.Instance.GetItem(StashAreaToStashUID[areaN]);
        }

        public static bool CheckStash()
        {
            AreaEnum areaN = (AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
            if (!StashAreaToStashUID.Keys.Contains(areaN))
            {
                return false;
            }

            if (CharacterManager.Instance.PlayerCharacters.Count == 0)
            {
                return false;
            }
            if (CharacterManager.Instance.PlayerCharacters.Values.Count == 0)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CharacterInventory), "InventoryIngredients",
    new Type[] { typeof(Tag), typeof(DictionaryExt<int, CompatibleIngredient>) },
    new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref })]
    public class CharacterInventory_InventoryIngredients
    {
        [HarmonyPostfix]
        public static void InventoryIngredients(CharacterInventory __instance, Tag _craftingStationTag, ref DictionaryExt<int, CompatibleIngredient> _sortedIngredient)
        {
            try
            {
                if (CraftThroughChestMod.CurrentStash() == null || !CraftThroughChestMod.CurrentStash().IsInteractable)
                {
                    return;
                }

                MethodInfo mi = AccessTools.GetDeclaredMethods(typeof(CharacterInventory)).FirstOrDefault(m => m.Name == "InventoryIngredients"
                    && m.GetParameters().Any(p => p.Name == "_items"));

                mi.Invoke(__instance, new object[] {
                        _craftingStationTag, _sortedIngredient, CraftThroughChestMod.CurrentStash().GetContainedItems()
                });
            }
            catch (Exception ex)
            {
                CraftThroughChestMod.MyLogger.LogError("InventoryIngredients: " + ex.Message);
            }
        }
    }
}