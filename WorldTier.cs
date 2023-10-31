using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimDifficultyScale
{
   public static class WorldTier
   {
      public static HashSet<string> BossDefeatedKeys = new HashSet<string>(new[] { "defeated_eikthyr", "defeated_gdking", "defeated_bonemass", "defeated_dragon", "defeated_goblinking", "defeated_queen" });
      public static int CurrentTier = 0;
      public static void UpdateWorldTier()
      {
         CurrentTier = BossDefeatedKeys.Count((string defeatedBossKey) => ZoneSystem.instance.GetGlobalKey(defeatedBossKey));
      }
      // Update On Clients
      [HarmonyPatch(typeof(ZoneSystem), "RPC_GlobalKeys")]
      private static class ZoneSystem_RPC_GlobalKeys_Patch
      {
         private static void Postfix()
         {
            UpdateWorldTier();
         }
      }
      // Update On Server
      [HarmonyPatch(typeof(ZoneSystem), "GlobalKeyAdd")]
      private static class ZoneSystem_GlobalKeyAdd_Patch
      {
         private static void Postfix()
         {
            UpdateWorldTier();
         }
      }
   }
}