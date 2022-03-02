using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using SideLoader;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RuneModManager
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class RuneModManager : BaseUnityPlugin
    {
        const string ID = "com.david.runemodmanager";
        const string NAME = "Rune Mod Manager";
        const string VERSION = "0.1.0";

        public static RuneModManager Instance;
        public static ManualLogSource MyLogger = BepInEx.Logging.Logger.CreateLogSource(NAME);
        public static void Log(object message)
        {
            MyLogger.LogMessage(message);
        }

        Character character;

        public static List<int> Runes = new List<int>()
        {
            Dez,
            Shim,
            Egoth,
            Fal
        };

        public const int Dez = 8100210;
        public const int Shim = 8100220;
        public const int Egoth = 8100230;
        public const int Fal = 8100240;

        // List of all mod rune ID's
        public List<int> RuneIDs = new List<int>();

        // List of mod runes separated by which rune they are
        public List<int> DezRunes = new List<int>();
        public List<int> ShimRunes = new List<int>();
        public List<int> EgothRunes = new List<int>();
        public List<int> FalRunes = new List<int>();

        internal void Awake()
        {            
            try
            {
                Instance = this;

                character = null;
                SL.OnGameplayResumedAfterLoading += SL_OnGameplayResumed;

                var harmony = new Harmony(ID);
                harmony.PatchAll();
            }
            catch (Exception ex)
            {
                Log("Awake:" + ex.Message);
            }
        }

        // Needed so new rune wont overwrite the old rune
        int counter = -485170;
        public int CreateNewID()
        {
            counter -= 1;
            return counter;
        }

        bool hasAllRunes = false;
        void SL_OnGameplayResumed()
        {
            // If player installed new rune mod but already owns base rune, this will
            // make the player learn the rune

            Log("Gameplay resumed");
            
            if (character == null)
                character = CharacterManager.Instance.GetFirstLocalCharacter();
            
            if (hasAllRunes)
            {
                Log("all runes");
                return;
            }

            if (character.Inventory.SkillKnowledge.IsItemLearned(Dez))
            {
                Log("Learning Dez " + DezRunes.Count);
                LearnRunes(DezRunes);
            }
            if (character.Inventory.SkillKnowledge.IsItemLearned(Shim))
            {
                LearnRunes(ShimRunes);
            }
            if (character.Inventory.SkillKnowledge.IsItemLearned(Egoth))
            {
                LearnRunes(EgothRunes);
            }
            if (character.Inventory.SkillKnowledge.IsItemLearned(Fal))
            {
                LearnRunes(FalRunes);
            }

            hasAllRunes = true;
        }

        // Makes player learn skill
        void LearnRunes(List<int> runes)
        {
            foreach (var rune in runes)
            {
                Log("Learn " + rune);
                if (!character.Inventory.SkillKnowledge.IsItemLearned(rune))
                {
                    AttackSkill skill = ResourcesPrefabManager.Instance.GetItemPrefab(rune) as AttackSkill;
                    character.Inventory.TryUnlockSkill(skill);
                }
            }
        }
    }

    // Used to change rune ID before SideLoader loads it in the game
    [HarmonyPatch(typeof(SL_Item), "Internal_Create")]
    public class SL_Item_Internal_Create
    {
        [HarmonyPrefix]
        public static bool GetRunes(SL_Item __instance)
        {
            var slItem = __instance;          

            if (slItem.CastType == Character.SpellCastType.WriteRune)
            {
                if (replacingExistingRune() || replacingExistingRune2())
                {
                    // ID of the rune they are trying to replace
                    int currentID;
                    int newID;

                    if (replacingExistingRune2())
                    {
                        currentID = slItem.Target_ItemID;
                    }
                    else
                    {
                        currentID = slItem.New_ItemID;
                    }

                    if (slItem.SLPackName == "RuneModManager")
                    {
                        newID = currentID;
                    }
                    else
                    {
                        newID = RuneModManager.Instance.CreateNewID();

                        if (currentID == 8100210)
                        {
                            RuneModManager.Instance.DezRunes.Add(newID);
                        }
                        else if (currentID == 8100220)
                        {
                            RuneModManager.Instance.ShimRunes.Add(newID);
                        }
                        else if (currentID == 8100230)
                        {  
                            RuneModManager.Instance.EgothRunes.Add(newID);
                        }
                        else if (currentID == 8100240)
                        {
                            RuneModManager.Instance.FalRunes.Add(newID);
                        }

                        RuneModManager.Instance.RuneIDs.Add(newID);

                        slItem.Name = slItem.Name + " (" + slItem.SLPackName + ")";
                    }

                    slItem.New_ItemID = newID;
                }
                else
                {
                    RuneModManager.Log("Woah no " + slItem.Name + " not a rune");
                }
            }

            return true;

            //Return true if rune's new ID equals an original rune's ID
            bool replacingExistingRune()
            {
                return RuneModManager.Runes.Contains(slItem.New_ItemID);
            }

            //Return true if someone copied an original rune and set new ID to -1
            bool replacingExistingRune2()
            {
                return RuneModManager.Runes.Contains(slItem.Target_ItemID)
                       && slItem.New_ItemID == -1;
            }
        }
    }

    // Called when learning skill from skill trainer
    [HarmonyPatch(typeof(SkillSlot), "UnlockSkill")]
    public class SkillSlot_UnlockSkill
    {
        [HarmonyPostfix]
        public static void LearnRuneSkills(SkillSlot __instance, Character _character)
        {
            // Checks if learned skill is a rune
            if (!RuneModManager.Runes.Contains(__instance.Skill.ItemID))
                return;

            List<int> runes = new List<int>();

            // Checks which rune is being learned            
            if (__instance.Skill.ItemID == 8100210)
            {
                runes = RuneModManager.Instance.DezRunes;
            }
            else if (__instance.Skill.ItemID == 8100220)
            {
                runes = RuneModManager.Instance.ShimRunes;
            }
            else if (__instance.Skill.ItemID == 8100230)
            {
                runes = RuneModManager.Instance.EgothRunes;
            }
            else if (__instance.Skill.ItemID == 8100240)
            {
                runes = RuneModManager.Instance.FalRunes;
            }

            // Learns all mod runes respective to the rune currently being learned
            foreach (var item in runes)
            {
                AttackSkill skill = ResourcesPrefabManager.Instance.GetItemPrefab(item) as AttackSkill;
                _character.Inventory.TryUnlockSkill(skill);
            }
        }
    }

    [HarmonyPatch(typeof(AttackSkill), "OwnerHasAllRequiredItems")]
    public class AttackSkill_OwnerHasAllRequiredItems
    {
        // Outward code specifically looks for the original rune IDs so 
        // internalized lexicon doesn't work for mod runes
        [HarmonyPrefix]
        public static bool InternalizedLexiconFix(AttackSkill __instance, ref bool __result)
        {
            // If current skill is not mod rune => return
            if (!RuneModManager.Instance.RuneIDs.Contains(__instance.ItemID))
                return true;
            
            // Checks if player has internalized lexicon
            if (__instance.OwnerCharacter.Inventory.SkillKnowledge.IsItemLearned(8205170))
            {
                // Makes rune usable with internalized lexicon and skips original function
                __result = true;
                return false;;
            }

            return true;
        }
    }
}
