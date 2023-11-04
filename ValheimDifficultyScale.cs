using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ValheimDifficultyScale
{
   [BepInPlugin("ValheimDifficultyScale", "Difficulty In Completed Biomes", "1.0.0")]
   [BepInProcess("valheim.exe")]
   public class ValheimDifficultyScale : BaseUnityPlugin
   {
      public static ValheimDifficultyScale instance;
      internal static ModConfig ModConfiguration;
      private static readonly Harmony _harmony = new Harmony("ValheimDifficultyScale");
      public static float EDGE_OF_WORLD = 10420f;
      public static int NUM_LOCATION_TIERS = 15;
      public static float NUM_LOCATION_TIERS_POW = Mathf.Pow(NUM_LOCATION_TIERS, 1.75f);
      public static float DROP_CHANCE_DENOMINATOR = 105f;
      public static int NUM_TOTAL_FACTION_LEVELS = 6;
      public static AssetBundle AssetBundle = null;

      // This was my original attempt and it was not accurate (mountain golems were forest monsters).
      // Keeping it in here as a fail safe, but a scary one.
      public static Dictionary<Character.Faction, int> FactionToWorldLevel = new Dictionary<Character.Faction, int>()
      {
         { Character.Faction.ForestMonsters , 0 }, // Meadows + Black Forest...? hp 10-60. greling hp 20 dmg 5,  eikthyr dmg 15-20, troll hp 600 dmg 50/60/70

         { Character.Faction.Undead, 1 }, // Black Forest and swamp 40. skeleton dmg 25 brute hp 150 dmg 30. 

         { Character.Faction.Demon, 2 }, // Swamp 60-200 draugr hp 100 dmg 48. D.Elite hp 200, dmg 58. abom hp 800, dmg 60/80

         { Character.Faction.MountainMonsters , 3 }, // Mountain. Wolf hp 80, dmg 70. drake hp 100 dmg 90. Moder hp 7500

         { Character.Faction.PlainsMonsters , 4 }, // Plains fuling hp 175, 85/90

         { Character.Faction.MistlandsMonsters , 5 }, // Mistlands
         { Character.Faction.Dverger , 5 },

         // don't effect these based on faction with this mod
         { Character.Faction.AnimalsVeg, 10 },
         //{ Character.Faction.Boss , 10 },
         { Character.Faction.SeaMonsters , 10 },
         { Character.Faction.Players , 10 },
      };
      void Awake()
      {
         instance = this;
         ModConfiguration = new ModConfig(Config);
         AssetBundle = Resources.FindObjectsOfTypeAll<AssetBundle>().FirstOrDefault((AssetBundle a) => a.name == "difficulty_scale") ?? AssetBundle.LoadFromStream(Utils.GetStreamFileFileName("ValheimDifficultyScale.assets.difficulty_scale"));
         ItemData.RegisterItems(AssetBundle);
         _harmony.PatchAll();
      }
      internal class ModConfig
      {
         public static ConfigEntry<KeyboardShortcut> ToggleMinimapCirclesKey = null;


         public ModConfig(ConfigFile configFile)
         {
            ToggleMinimapCirclesKey = configFile.Bind("Keyboard Shortcuts", "Toggle Minimap Circles", new KeyboardShortcut(KeyCode.Semicolon), new ConfigDescription("Toggle Minimap circles with viewing minimap."));
         }
      }
   }
}