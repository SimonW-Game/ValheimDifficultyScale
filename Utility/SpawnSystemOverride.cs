using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace ValheimDifficultyScale.Utility
{
   [HarmonyPatch(typeof(SpawnSystem))]
   public static class SpawnSystemOverride
   {
      public static Dictionary<string, int> NameToWorldLevel = new Dictionary<string, int>();
      private static HashSet<string> ZoneCtrlNames = new HashSet<string>();
      [HarmonyPatch("Awake"), HarmonyPostfix, UsedImplicitly]
      private static void SpawnSystem_Awake_Postfix(SpawnSystem __instance)
      {
         if (!ZoneCtrlNames.Contains(__instance.name))
         {
            ZoneCtrlNames.Add(__instance.name);
            foreach (SpawnSystemList spawnList in __instance.m_spawnLists)
            {
               foreach (SpawnSystem.SpawnData spawnData in spawnList.m_spawners)
               {
                  string critterName = spawnData.m_prefab.GetComponent<Humanoid>()?.m_name;
                  if (critterName != null)
                  {
                     if (!NameToWorldLevel.TryGetValue(critterName, out var level))
                        NameToWorldLevel.Add(critterName, GetWorldLevelFromBiome(spawnData.m_biome));
                     else
                        NameToWorldLevel[critterName] = Math.Min(GetWorldLevelFromBiome(spawnData.m_biome), NameToWorldLevel[critterName]);
                  }
               }
            }
         }
      }
      public static int GetWorldLevelFromBiome(Heightmap.Biome biome)
      {
         if (biome.HasFlag(Heightmap.Biome.Meadows))
            return 0;
         if (biome.HasFlag(Heightmap.Biome.BlackForest))
            return 1;
         if (biome.HasFlag(Heightmap.Biome.Swamp))
            return 2;
         if (biome.HasFlag(Heightmap.Biome.Mountain))
            return 3;
         if (biome.HasFlag(Heightmap.Biome.Plains))
            return 4;
         if (biome.HasFlag(Heightmap.Biome.Mistlands))
            return 5;
         if (biome.HasFlag(Heightmap.Biome.AshLands))
            return 6;
         if (biome.HasFlag(Heightmap.Biome.DeepNorth))
            return 7;
         if (biome.HasFlag(Heightmap.Biome.Ocean))
            return 10;
         return 10;
      }
      public static Heightmap.Biome GetBiomeFromWorldLevel(int worldLevel)
      {
         if (worldLevel == 0)
            return Heightmap.Biome.Meadows;
         if (worldLevel == 1)
            return Heightmap.Biome.BlackForest;
         if (worldLevel == 2)
            return Heightmap.Biome.Swamp;
         if (worldLevel == 3)
            return Heightmap.Biome.Mountain;
         if (worldLevel == 4)
            return Heightmap.Biome.Plains;
         if (worldLevel == 5)
            return Heightmap.Biome.Mistlands;
         if (worldLevel == 6)
            return Heightmap.Biome.AshLands;
         if (worldLevel == 7)
            return Heightmap.Biome.DeepNorth;
         return Heightmap.Biome.None;
      }
   }
}
