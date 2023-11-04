using HarmonyLib;
using ItemManager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ValheimDifficultyScale
{
   public static class SpawnCharacterModifications
   {
      private static string CUSTOM_CHARACTER_HEALTH_MODIFIED = "V-Diff-Health-Modified";
      private static string CUSTOM_ITEM_DATA_MODIFIED = "V-Diff-Scale-old-damage";

      [HarmonyPatch(typeof(Humanoid), "Awake")]
      private static class Humanoid_Awake
      {
         private static void Postfix(Humanoid __instance)
         {
            if (!(__instance is Player)
               && __instance is Humanoid humanoid)
            {
               Calculations.GetAllStats(__instance, out Heightmap.Biome biome, out int factionDifference, out int characterWorldLevelOut, out int locationTier);
               CharacterDrop characterDrop = __instance.GetComponent<CharacterDrop>();
               if (characterDrop != null)
               {
                  if (factionDifference > 0)
                  {
                     if (ItemData.BiomeToItemMap.TryGetValue(biome, out Item item))
                     {
                        float dropChanceForBiome = (float)Math.Pow(WorldTier.CurrentTier + characterWorldLevelOut + locationTier, 1.45f) / (ValheimDifficultyScale.DROP_CHANCE_DENOMINATOR * 2.5f);
                        CharacterDrop.Drop dropy = new CharacterDrop.Drop() { m_amountMin = 1, m_amountMax = 1, m_chance = dropChanceForBiome, m_prefab = item.Prefab, m_levelMultiplier = true, m_onePerPlayer = false, m_dontScale = true };
                        if (__instance.m_boss)
                        {
                           dropy.m_chance *= 2.5f;
                           dropy.m_amountMin = (int)Math.Ceiling(characterWorldLevelOut / 2f);
                           dropy.m_amountMax = ValheimDifficultyScale.NUM_TOTAL_FACTION_LEVELS + 1 - factionDifference;
                        }
                        characterDrop.m_drops.RemoveAll(d => d.m_prefab == item.Prefab);
                        characterDrop.m_drops.Add(dropy);
                     }
                     float dropChance = (float)Math.Pow(WorldTier.CurrentTier + characterWorldLevelOut + locationTier, 1.4f) / ValheimDifficultyScale.DROP_CHANCE_DENOMINATOR;
                     CharacterDrop.Drop drop = new CharacterDrop.Drop() { m_amountMin = 1, m_amountMax = 1, m_chance = dropChance, m_prefab = ItemData.DifficultyBlob.Prefab, m_levelMultiplier = true, m_onePerPlayer = false, m_dontScale = true };
                     if (__instance.m_boss)
                     {
                        drop.m_chance *= 1.5f;
                        drop.m_amountMin = (int)Math.Ceiling(characterWorldLevelOut / 2f);
                        drop.m_amountMax = ValheimDifficultyScale.NUM_TOTAL_FACTION_LEVELS + 1 - factionDifference;
                     }
                     int removedDifficultyBlobCount = characterDrop.m_drops.RemoveAll(d => d.m_prefab == ItemData.DifficultyBlob.Prefab);
                     characterDrop.m_drops.Add(drop);
                     Debug.Log($"DropCount == {characterDrop?.m_drops.Count} - {removedDifficultyBlobCount}");
                  }
               }


               int? savedWorldTier = __instance.m_nview?.GetZDO()?.GetInt(CUSTOM_CHARACTER_HEALTH_MODIFIED, 0);
               if (savedWorldTier != WorldTier.CurrentTier)
               {
                  float health = humanoid.m_health * humanoid.GetLevel(); // How base code does setup max health.
                  UpdateHumanoidHealth(humanoid, ref health);
               }
               else
               {
                  Debug.Log($"Character existing health {humanoid.m_name}, hp: {humanoid.m_health} * {humanoid.GetLevel()} => {__instance.m_nview?.GetZDO()?.GetFloat(ZDOVars.s_maxHealth)}");
               }
            }
         }
      }
      [HarmonyPatch(typeof(Character), "SetMaxHealth")]
      private static class Character_SetMaxHealth
      {
         private static void Prefix(Character __instance, ref float health)
         {
            // Only change the value if we haven't set max health yet.
            if (!(__instance is Player)
               && __instance is Humanoid humanoid)
            {
               if (__instance.m_nview?.GetZDO()?.GetInt(CUSTOM_CHARACTER_HEALTH_MODIFIED, 0) != WorldTier.CurrentTier)
               {
                  UpdateHumanoidHealth(humanoid, ref health);
               }
               else
               {
                  Debug.Log($"Character existing health {humanoid.m_name}, hp: {humanoid.m_health} * {humanoid.GetLevel()} => {health}");
               }
            }
         }
      }

      private static void UpdateHumanoidHealth(Humanoid humanoid, ref float health)
      {
         float prior = health;
         float starLevel = (float)humanoid.GetLevel();
         if (starLevel > 1)
            health = health / starLevel; // revert to pre buff level;
         health = Calculations.GetModifiedHealth(humanoid, health, out int factionDifference, out int characterWorldLevelOut, out int locationTier);
         if (starLevel > 1)
            health *= ((starLevel - 1) / 2f) + 1;
         //humanoid.m_health = health; // Don't change this value as it will be used to to generate max health later.  Don't want to stack it.
         humanoid.m_nview?.GetZDO()?.Set(ZDOVars.s_maxHealth, health);
         humanoid.m_nview?.GetZDO()?.Set(CUSTOM_CHARACTER_HEALTH_MODIFIED, WorldTier.CurrentTier);
         Debug.Log($"Character setup health {humanoid.m_name}, hp: {prior} * {humanoid.GetLevel()} => {health}, factionDifference: {factionDifference}, characterWorldLevelOut: {characterWorldLevelOut}, locationTier: {locationTier}");
      }

      [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.PickupPrefab))]
      private static class Humanoid_PickupPrefab
      {
         private static void Postfix(Humanoid __instance, ref ItemDrop.ItemData __result)
         {
            if (!(__instance is Player))
            {
               __result.m_customData = __result.m_customData ?? new Dictionary<string, string>();
               if (!__result.m_customData.TryGetValue(CUSTOM_ITEM_DATA_MODIFIED, out string preDamage))
               {
                  float oldDamage = __result.m_shared.m_damages.GetTotalBlockableDamage() + __result.m_shared.m_damages.m_damage;
                  float multiplier = Calculations.GetDamageMultiplier(__instance, oldDamage, out int characterWorldLevel);
                  if (multiplier > 0)
                  {
                     __result.m_shared.m_damages.Modify(multiplier);
                     float totalActualDamage = __result.m_shared.m_damages.GetTotalBlockableDamage() + __result.m_shared.m_damages.m_damage;
                     __result.m_customData[CUSTOM_ITEM_DATA_MODIFIED] = $"{oldDamage}";
                     Debug.Log($"Character setup DMG {__instance.m_name}, DMG: {oldDamage} => {totalActualDamage}");
                  }
               }
               else
               {
                  Debug.Log($"Character existing DMG {__instance.m_name}, DMG: {__result.m_shared.m_damages.GetTotalBlockableDamage() + __result.m_shared.m_damages.m_damage}");
               }
            }
         }
      }
   }
}
