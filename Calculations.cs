using System;
using System.Linq;
using UnityEngine;
using ValheimDifficultyScale.Utility;

namespace ValheimDifficultyScale
{
   public static class Calculations
   {
      // Each index represents the health/damage that biome recieves when upgrading to the next biome tier  (meadow -> Black Forest enemie swill recieve 22 extra health and 14 extra damage)
      private static int[] FactionHealthIncreases = new[] { 22, 27, 33, 33, 19, 22, 21, 20, 20, 20, 20 };
      private static int[] FactionDamageIncreases = new[] { 14, 13, 11, 14, 13, 12, 12, 12, 12, 12, 12 };
      public static float GetDistanceFromCenter(Character character)
      {
         ZoneSystem.instance.GetLocationIcon(Game.instance.m_StartLocation, out var startLocation);
         float x = character.transform.position.x - startLocation.x;
         float z = character.transform.position.z - startLocation.z;
         return (float)Math.Sqrt(x * x + z * z);
      }
      public static int GetLocationTier(Character character)
      {
         float distanceFromCenter = GetDistanceFromCenter(character);
         return (int)Mathf.Floor(distanceFromCenter / (ValheimDifficultyScale.EDGE_OF_WORLD / ValheimDifficultyScale.NUM_LOCATION_TIERS));
      }
      public static int GetFactionDifference(Character character, out int characterWorldLevel)
      {
         if (!SpawnSystemOverride.NameToWorldLevel.TryGetValue(character.m_name, out characterWorldLevel))
            characterWorldLevel = GetWorldLevelFromFaction(character);
         return WorldTier.CurrentTier - characterWorldLevel;
      }
      public static int GetWorldLevelFromFaction(Character character)
      {
         Character.Faction faction = character.GetFaction();
         if (ValheimDifficultyScale.FactionToWorldLevel.TryGetValue(faction, out int worldLevel))
            return worldLevel;
         else if (faction == Character.Faction.Boss)
         {
            return Array.IndexOf(WorldTier.BossDefeatedKeys.ToArray(), character.m_defeatSetGlobalKey);
         }
         return 10; // just default to the hardest world level
      }
      public static float GetDamageMultiplier(Character character, float oldDamage, out int characterWorldLevelOut)
      {
         int factionDifference = GetFactionDifference(character, out characterWorldLevelOut);
         if (oldDamage <= 0)
            return 1f;

         float newDamage = oldDamage;
         int characterWorldLevel = characterWorldLevelOut;
         if (factionDifference > 0)
         {
            int baseHealthIncrease = Enumerable.Range(0, factionDifference).Sum(diffNdx => FactionDamageIncreases[characterWorldLevel + diffNdx]);
            newDamage += (baseHealthIncrease + (oldDamage * .05f));
         }
         int locationTier = GetLocationTier(character);
         if (locationTier > 0)
         {
            float distanceFactor = Mathf.Pow(locationTier, 1.75f) / ValheimDifficultyScale.NUM_LOCATION_TIERS_POW;
            float multiplier = 1.05f; // silly compiler doesn't know that <=0, 1, 2, 3, 4+ covers all the ints,must assign value
            if (factionDifference <= 0)
               multiplier = 1.075f;
            else if (factionDifference == 1)
               multiplier = 1.3f;
            else if (factionDifference == 2)
               multiplier = 1.65f;
            else if (factionDifference == 3)
               multiplier = 2f;
            else if (factionDifference >= 4)
               multiplier = 2.35f;

            newDamage *= Mathf.Lerp(1, multiplier, distanceFactor);
         }
         return newDamage / oldDamage;
      }
      public static void GetAllStats(Character character, out Heightmap.Biome biome, out int factionDifference, out int characterWorldLevelOut, out int locationTier)
      {
         factionDifference = GetFactionDifference(character, out characterWorldLevelOut);
         biome = SpawnSystemOverride.GetBiomeFromWorldLevel(characterWorldLevelOut);
         locationTier = GetLocationTier(character);
      }
      public static float GetModifiedHealth(Character character, float currentHealth, out int factionDifference, out int characterWorldLevelOut, out int locationTier)
      {
         float newHealth = currentHealth;
         factionDifference = GetFactionDifference(character, out characterWorldLevelOut);
         int characterWorldLevel = characterWorldLevelOut;
         if (factionDifference > 0)
         {
            int baseHealthIncrease = Enumerable.Range(0, factionDifference).Sum(diffNdx => FactionHealthIncreases[characterWorldLevel + diffNdx]);
            newHealth += (baseHealthIncrease + (currentHealth * .05f));
         }
         locationTier = GetLocationTier(character);
         if (locationTier > 0)
         {
            float distanceFactor = Mathf.Pow(locationTier, 1.75f) / ValheimDifficultyScale.NUM_LOCATION_TIERS_POW;
            float multiplier = 1.05f; // silly compiler doesn't know that <=0, 1, 2, 3, 4+ covers all the ints,must assign value
            if (factionDifference <= 0)
               multiplier = 1.05f;
            else if (factionDifference == 1)
               multiplier = 1.3f;
            else if (factionDifference == 2)
               multiplier = 1.65f;
            else if (factionDifference == 3)
               multiplier = 2f;
            else if (factionDifference >= 4)
               multiplier = 2.35f;

            newHealth *= Mathf.Lerp(1, multiplier, distanceFactor);
         }
         return newHealth;
      }
   }
}
