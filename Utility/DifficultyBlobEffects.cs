using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ClutterSystem;

namespace ValheimDifficultyScale
{
   [HarmonyPatch(typeof(Player))]
   public static class PlayerOverrides
   {

      [HarmonyPatch("Load"), HarmonyPostfix, HarmonyPriority(100), UsedImplicitly]
      private static void Player_Load_Postfix(Player __instance)
      {
         foreach (ItemDrop.ItemData item in __instance.m_inventory.m_inventory)
         {
            if (item.m_customData.TryGetValue(InventoryOverrides.UPGRADE_KEY, out string curUpgradeAmountStr) && int.TryParse(curUpgradeAmountStr, out int curUpgradeAmount))
            {
               if (item.IsWeapon())
               {
                  float highestDamage = InventoryOverrides.GetHighestDamage(item.m_shared.m_damages);
                  InventoryOverrides.IncreaseHighestDamage(ref item.m_shared.m_damages, highestDamage, curUpgradeAmount);
               }
               else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Utility)
               {
                  item.m_shared.m_armor += curUpgradeAmount;
               }
            }
         }
      }
   }
   [HarmonyPatch(typeof(InventoryGui))]
   public static class InventoryOverrides
   {
      public static string UPGRADE_KEY = "DifficultyBlob_UpgradeAmount";
      [HarmonyPatch("OnRightClickItem"), HarmonyPrefix, HarmonyPriority(100), UsedImplicitly]
      private static void InventoryGui_OnRightClickItem_Prefix(InventoryGrid grid, ref ItemDrop.ItemData item)
      {
         if (item?.m_shared?.m_name == "Difficulty Blob" && Player.m_localPlayer != null)
         {
            Inventory inventory = grid.GetInventory();
            if (inventory.ContainsItem(item))
            {
               ItemDrop.ItemData currentWeapon = Player.m_localPlayer.GetCurrentWeapon();
               if (currentWeapon != null && currentWeapon.m_shared.m_skillType != Skills.SkillType.Unarmed)
               {
                  currentWeapon.m_customData = currentWeapon.m_customData ?? new Dictionary<string, string>();
                  int upgradeCost = 1;
                  if (currentWeapon.m_customData.TryGetValue(UPGRADE_KEY, out string curUpgradeAmountStr) && int.TryParse(curUpgradeAmountStr, out int curUpgradeAmount))
                     upgradeCost += curUpgradeAmount;

                  if (item.m_stack >= upgradeCost)
                  {
                     currentWeapon.m_customData[UPGRADE_KEY] = $"{upgradeCost}"; // The cost is equal to the current damaged raised (first upgrade costs 1, 14th upgrade costs 14).
                     inventory.RemoveItem(item, upgradeCost);
                     float highestDamage = GetHighestDamage(currentWeapon.m_shared.m_damages);
                     IncreaseHighestDamage(ref currentWeapon.m_shared.m_damages, highestDamage);
                     Player.m_localPlayer.m_zanim.SetTrigger("interact");
                     Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Upgraded {currentWeapon.m_shared.m_name} damage by 1!");
                  }
                  else
                  {
                     Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"{currentWeapon.m_shared.m_name} requires {upgradeCost} Blobs to upgrade");
                  }
               }
               else
               {
                  Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Equip a weapon to upgrade upgrade!");
               }
            }
         }
         else if (item?.m_shared?.m_name == "Mountain Difficulty Blob")
         {
            Inventory inventory = grid.GetInventory();
            if (inventory.ContainsItem(item))
            {
               ItemDrop.ItemData currentUtility = Player.m_localPlayer.m_utilityItem;
               if (currentUtility != null)
               {
                  currentUtility.m_customData = currentUtility.m_customData ?? new Dictionary<string, string>();
                  int upgradeCost = 1;
                  if (currentUtility.m_customData.TryGetValue(UPGRADE_KEY, out string curUpgradeAmountStr) && int.TryParse(curUpgradeAmountStr, out int curUpgradeAmount))
                     upgradeCost += curUpgradeAmount;

                  if (item.m_stack >= upgradeCost)
                  {
                     currentUtility.m_customData[UPGRADE_KEY] = $"{upgradeCost}"; // The cost is equal to the current damaged raised (first upgrade costs 1, 14th upgrade costs 14).
                     inventory.RemoveItem(item, upgradeCost);
                     Player.m_localPlayer.m_zanim.SetTrigger("interact");
                     Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Upgraded {currentUtility.m_shared.m_name} armor by 1!");
                  }
                  else
                  {
                     Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"{currentUtility.m_shared.m_name} requires {upgradeCost} Blobs to upgrade");
                  }
               }
               else
               {
                  Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Equip a utility item to upgrade upgrade!");
               }
            }
         }
      }

      public static void IncreaseHighestDamage(ref HitData.DamageTypes damages, float highestDamage, float incrementAmount = 1)
      {
         float beforeDamage = damages.GetTotalDamage();
         if (damages.m_fire == highestDamage)
            damages.m_fire += incrementAmount;
         else if (damages.m_frost == highestDamage)
            damages.m_frost += incrementAmount;
         else if (damages.m_lightning == highestDamage)
            damages.m_lightning += incrementAmount;
         else if (damages.m_poison == highestDamage)
            damages.m_poison += incrementAmount;
         else if (damages.m_spirit == highestDamage)
            damages.m_spirit += incrementAmount;
         else if (damages.m_blunt == highestDamage)
            damages.m_blunt += incrementAmount;
         else if (damages.m_slash == highestDamage)
            damages.m_slash += incrementAmount;
         else if (damages.m_pierce == highestDamage)
            damages.m_pierce += incrementAmount;
         else if (damages.m_chop == highestDamage)
            damages.m_chop += incrementAmount;
         else if (damages.m_pickaxe == highestDamage)
            damages.m_pickaxe += incrementAmount;
         else if (damages.m_damage == highestDamage)
            damages.m_damage += incrementAmount;
      }

      public static float GetHighestDamage(HitData.DamageTypes damages)
      {
         float highestDamage = 0;
         if (damages.m_damage > highestDamage)
            highestDamage = damages.m_damage;
         if (damages.m_blunt > highestDamage)
            highestDamage = damages.m_blunt;
         if (damages.m_slash > highestDamage)
            highestDamage = damages.m_slash;
         if (damages.m_pierce > highestDamage)
            highestDamage = damages.m_pierce;
         if (damages.m_chop > highestDamage)
            highestDamage = damages.m_chop;
         if (damages.m_pickaxe > highestDamage)
            highestDamage = damages.m_pickaxe;
         if (damages.m_fire > highestDamage)
            highestDamage = damages.m_fire;
         if (damages.m_frost > highestDamage)
            highestDamage = damages.m_frost;
         if (damages.m_lightning > highestDamage)
            highestDamage = damages.m_lightning;
         if (damages.m_poison > highestDamage)
            highestDamage = damages.m_poison;
         if (damages.m_spirit > highestDamage)
            highestDamage = damages.m_spirit;
         return highestDamage;
      }


      [HarmonyPatch(typeof(Player), nameof(Character.GetBodyArmor))]
      private class PlayerHitCharacter
      {
         [UsedImplicitly, HarmonyPriority(Priority.First)]
         private static void Postfix(Player __instance, ref float __result)
         {
            // Using only custom data because too many other mods add utility's armor
            if (__instance.m_utilityItem != null && __instance.m_utilityItem.m_customData.TryGetValue(UPGRADE_KEY, out string upgradeValStr) && int.TryParse(upgradeValStr, out int upgradeVal))
               __result += upgradeVal + (Game.m_worldLevel * (float)Game.instance.m_worldLevelGearBaseAC);
         }
      }
   }
}
