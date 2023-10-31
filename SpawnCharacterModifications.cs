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
               foreach (Item item in ItemData.BiomeToItemMap.Values)
               {
                  CharacterDrop.Drop dropy = new CharacterDrop.Drop() { m_amountMin = 1, m_amountMax = 1, m_chance = 1, m_prefab = item.Prefab, m_levelMultiplier = true, m_onePerPlayer = false, m_dontScale = true };
                  if (__instance.m_boss)
                  {
                     dropy.m_chance *= 1.75f;
                     dropy.m_amountMin = 1;//(int)Math.Ceiling(characterWorldLevelOut / 2f);
                     dropy.m_amountMax = 2;//ValheimDifficultyScale.NUM_TOTAL_FACTION_LEVELS + 1 - factionDifference;
                  }
                  __instance.GetComponent<CharacterDrop>()?.m_drops.Add(dropy);
               }
               Calculations.GetAllStats(__instance, out Heightmap.Biome biome, out int factionDifference, out int characterWorldLevelOut, out int locationTier);
               if (factionDifference > 0)
               {
                  float dropChance = (float)Math.Pow(WorldTier.CurrentTier + characterWorldLevelOut + locationTier, 1.4f) / ValheimDifficultyScale.DROP_CHANCE_DENOMINATOR;
                  CharacterDrop.Drop drop = new CharacterDrop.Drop() { m_amountMin = 1, m_amountMax = 1, m_chance = dropChance, m_prefab = ItemData.DifficultyBlob.Prefab, m_levelMultiplier = true, m_onePerPlayer = false, m_dontScale = true };
                  if (__instance.m_boss)
                  {
                     drop.m_chance *= 1.5f;
                     drop.m_amountMin = (int)Math.Ceiling(characterWorldLevelOut / 2f);
                     drop.m_amountMax = ValheimDifficultyScale.NUM_TOTAL_FACTION_LEVELS + 1 - factionDifference;
                  }
                  __instance.GetComponent<CharacterDrop>()?.m_drops.Add(drop);

               }

               if (__instance.m_nview?.GetZDO()?.GetInt(CUSTOM_CHARACTER_HEALTH_MODIFIED, 0) != WorldTier.CurrentTier)
               {
                  float health = humanoid.m_health * humanoid.GetLevel(); // How base code does setup max health.
                  UpdateHumanoidHealth(humanoid, ref health);
               }
               else
               {
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
                  //Debug.Log($"Character existing health {humanoid.m_name}, hp: {humanoid.m_health} => {health}");
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
            health *= starLevel;
         //humanoid.m_health = health; // Don't change this value as it will be used to to generate max health later.  Don't want to stack it.
         humanoid.m_nview?.GetZDO()?.Set(ZDOVars.s_maxHealth, health);
         // 4+ AT POW 1.25: [9 - (24.68 - 37.08) - (42-55) - 61.5466].
         // 5+ POW 1.4 [9.52 - (36.27-57.2) - (66.29 - 90.59) 100.9]
         if (factionDifference > 0)
         {
            float dropChance = (float)Math.Pow(WorldTier.CurrentTier + characterWorldLevelOut + locationTier, 1.4f) / ValheimDifficultyScale.DROP_CHANCE_DENOMINATOR;
            CharacterDrop.Drop drop = new CharacterDrop.Drop() { m_amountMin = 1, m_amountMax = 1, m_chance = dropChance, m_prefab = ValheimDifficultyScale.AssetBundle.LoadAsset<GameObject>("DifficultyBlob"), m_levelMultiplier = true, m_onePerPlayer = false, m_dontScale = true };
            if (humanoid.m_boss)
            {
               drop.m_chance *= 1.5f;
               drop.m_amountMin = (int)Math.Ceiling(characterWorldLevelOut / 2f);
               drop.m_amountMax = ValheimDifficultyScale.NUM_TOTAL_FACTION_LEVELS + 1 - factionDifference;
            }
            humanoid.GetComponent<CharacterDrop>()?.m_drops.Add(drop);
            humanoid.m_nview?.GetZDO()?.Set(CUSTOM_CHARACTER_HEALTH_MODIFIED, WorldTier.CurrentTier);
            //Debug.Log($"Character setup health {humanoid.m_name}, hp: {prior} => {health}");
         }
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
                     float totalActualDamage = __result.m_shared.m_damages.GetTotalBlockableDamage() + __result.m_shared.m_damages.m_damage;
                     if (totalActualDamage > 0)
                     {
                        __result.m_customData[CUSTOM_ITEM_DATA_MODIFIED] = $"{oldDamage}";
                        __result.m_shared.m_damages.Modify(multiplier);
                     }
                  }
               }
            }
         }
      }
   }
}
