using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using ValheimDifficultyScale.Effects;

namespace ValheimDifficultyScale
{
   public static class ItemData
   {
      public static GameObject InactivePlaceHolderObject;
      public static CustomPiece BlobSmusherTable;
      public static CustomItem DifficultyBlob;
      public static Dictionary<Heightmap.Biome, CustomItem> BiomeToItemMap = new Dictionary<Heightmap.Biome, CustomItem>();

      [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
      internal static class ODBUtil
      {
         private static void Prefix(ObjectDB other)
         {
            GameObject stone_cutter = other.m_items.FirstOrDefault(go => go.GetComponent<ItemDrop>()?.m_itemData?.m_shared.m_buildPieces?.m_pieces?.FirstOrDefault(p => p.name == "piece_stonecutter"));

            GameObject hammerObj = other.m_items.FirstOrDefault(go => go.name == "Hammer");
            GameObject stoneCutter = hammerObj.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_buildPieces?.m_pieces?.FirstOrDefault(p => p.name == "piece_stonecutter");
            if (stoneCutter != null)
               BlobSmusherTable = AddBlobSmusherTable(stoneCutter);

            GameObject bombOozeObj = other.m_items.FirstOrDefault(go => go.name == "BombOoze");
            MakeMeadowBlobBomb(bombOozeObj);
            MakeForestBlobBomb(bombOozeObj);
            MakeSwampBlobBomb(bombOozeObj);
            MakePlainsBlobBomb(bombOozeObj);
         }
         private static CustomPiece AddBlobSmusherTable(GameObject stoneCutter)
         {
            Color color = Color.magenta;
            string name = "Blob Smusher";
            GameObject blobSmusher = Object.Instantiate(stoneCutter, InactivePlaceHolderObject.transform, false);
            Transform newTableGraphics = blobSmusher.transform.Find("New").transform;
            newTableGraphics.Find("high").GetComponent<MeshRenderer>().material.color = color;
            newTableGraphics.Find("high").GetComponent<MeshRenderer>().material.SetColor("_Color", color);
            Transform wornTableGraphics = blobSmusher.transform.Find("Worn").transform;
            wornTableGraphics.Find("high").GetComponent<MeshRenderer>().material.color = color;
            wornTableGraphics.Find("high").GetComponent<MeshRenderer>().material.SetColor("_Color", color);
            Transform brokenTableGraphics = blobSmusher.transform.Find("Broken").transform;
            brokenTableGraphics.Find("high").GetComponent<MeshRenderer>().material.color = color;
            brokenTableGraphics.Find("high").GetComponent<MeshRenderer>().material.SetColor("_Color", color);
            CraftingStation craftingComponent = blobSmusher.GetComponent<CraftingStation>();
            blobSmusher.name = "piece_BlobSmusher";
            craftingComponent.name = blobSmusher.name;
            craftingComponent.m_name = name;
            PieceConfig smusherConfig = new PieceConfig()
            {
               Name = name,
               Description = "Time to smush some blobs!",
               Icon = Utils.LoadSprite("ValheimDifficultyScale.assets.Blob_Smusher_Icon.png"),
               PieceTable = PieceTables.Hammer,
               CraftingStation = CraftingStations.Workbench,
               Category = PieceCategories.Crafting,
               Requirements = new[] {
               new RequirementConfig( "MeadowBlob", 1),
               new RequirementConfig( "BlackForestBlob", 1),
               new RequirementConfig( "RoundLog", 10),
               new RequirementConfig( "DifficultyBlob", 1)
               }
            };
            CustomPiece blobMusherTable = new CustomPiece(blobSmusher, true, smusherConfig);

            PieceManager.Instance.AddPiece(blobMusherTable);
            return blobMusherTable;
         }

         internal static void MakeMeadowBlobBomb(GameObject bombOozeObj)
         {
            string name = "StaggerBlobBomb";
            string friendlyName = "Stagger Blob Bomb";
            string description = "Throw this to stagger all nearby enemies.";
            Color color = new Color(0f, 1f, 0f);
            RecipeConfig recipe = new RecipeConfig();
            recipe.Amount = 4;
            recipe.CraftingStation = "piece_BlobSmusher";
            recipe.Item = name;
            recipe.Name = friendlyName;
            recipe.Requirements = new[] {
               new RequirementConfig( "MeadowBlob", 1),
               new RequirementConfig( "Ooze", 6),
               new RequirementConfig( "BoneFragments", 6),
               new RequirementConfig( "Resin", 1)
            };
            RegisterBlobBomb<StaggerAfterTime>(bombOozeObj, color, friendlyName, name, description, recipe, Utils.LoadSprite($"ValheimDifficultyScale.assets.MeadowOozeBomb.png"));
         }
         internal static void MakeForestBlobBomb(GameObject bombOozeObj)
         {
            string name = "PullBlobBomb";
            string friendlyName = "Pull Blob Bomb";
            string description = "Throw this to pull all nearby enemies.";
            Color color = new Color(.1f, .1f, .1f);
            RecipeConfig recipe = new RecipeConfig();
            recipe.Amount = 4;
            recipe.CraftingStation = "piece_BlobSmusher";
            recipe.Item = name;
            recipe.Name = friendlyName;
            recipe.Requirements = new[] {
               new RequirementConfig( "BlackForestBlob", 1),
               new RequirementConfig( "Guck", 4),
               new RequirementConfig( "WitheredBone", 2),
               new RequirementConfig( "Resin", 1)
            };
            RegisterBlobBomb<PullAndStall>(bombOozeObj, color, friendlyName, name, description, recipe, Utils.LoadSprite($"ValheimDifficultyScale.assets.BlackForestOozeBomb.png"));
         }
         internal static void MakeSwampBlobBomb(GameObject bombOozeObj)
         {
            string name = "PusherBlobBomb";
            string friendlyName = "Push N Slow Blob Bomb";
            Color color = new Color(.36f, 0f, .36f);
            RecipeConfig recipe = new RecipeConfig();
            recipe.Amount = 4;
            recipe.CraftingStation = "piece_BlobSmusher";
            recipe.Item = name;
            recipe.Name = friendlyName;
            recipe.Requirements = new[] {
               new RequirementConfig( "SwampBlob", 1),
               new RequirementConfig( "FreezeGland", 4),
               new RequirementConfig( "Resin", 8),
               new RequirementConfig( "Honey", 6)
            };
            string description = "Throw this to push and slow all nearby enemies.";

            RegisterBlobBomb<PushAndSlow>(bombOozeObj, color, friendlyName, name, description, recipe, Utils.LoadSprite($"ValheimDifficultyScale.assets.SwampOozeBomb.png"));
         }
         internal static void MakePlainsBlobBomb(GameObject bombOozeObj)
         {
            string name = "LifterBlobBomb";
            string friendlyName = "Throw Up Bomb";
            Color color = new Color(1f, .55f, .25f);
            RecipeConfig recipe = new RecipeConfig();
            recipe.Amount = 4;
            recipe.CraftingStation = "piece_BlobSmusher";
            recipe.Item = name;
            recipe.Name = friendlyName;
            recipe.Requirements = new[] {
               new RequirementConfig( "PlainsBlob", 1),
               new RequirementConfig( "Tin", 3),
               new RequirementConfig( "MushroomYellow", 5),
               new RequirementConfig( "Pukeberries", 5)
            };
            string description = "Throw this to send nearby enemies in the air, out of cambat for a while.";

            RegisterBlobBomb<ThrowUpInAir>(bombOozeObj, color, friendlyName, name, description, recipe, Utils.LoadSprite($"ValheimDifficultyScale.assets.PlainsOozeBomb.png"));
         }
         internal static void RegisterBlobBomb<T>(GameObject bombOozeObj,
            Color color,
            string friendlyName,
            string name,
            string description,
            RecipeConfig recipe,
            Sprite icon) where T : MonoBehaviour
         {
            GameObject bombPrefab = Object.Instantiate(bombOozeObj, InactivePlaceHolderObject.transform, false);
            bombPrefab.name = name;
            CustomItem bombOoze = new CustomItem(bombPrefab, true);
            bombOoze.Recipe = new CustomRecipe(recipe);
            bombOoze.ItemPrefab.GetComponentInChildren<MeshRenderer>().material.color = color;
            var main = bombOoze.ItemPrefab.GetComponent<ParticleSystem>().main;
            main.startColor = color;
            ItemDrop bombDrop = bombOoze.ItemPrefab.GetComponent<ItemDrop>();
            bombDrop.name = bombPrefab.name;

            bombDrop.m_itemData = bombPrefab.GetComponent<ItemDrop>().m_itemData.Clone();
            bombDrop.m_itemData.m_shared.m_name = friendlyName;
            bombDrop.m_itemData.m_shared.m_icons = new[] { icon };
            bombDrop.m_itemData.m_shared.m_description = description;
            bombDrop.m_itemData.m_shared.m_attack.m_attackProjectile = Object.Instantiate(bombDrop.m_itemData.m_shared.m_attack.m_attackProjectile, InactivePlaceHolderObject.transform, false);
            Projectile projectile = bombDrop.m_itemData.m_shared.m_attack.m_attackProjectile.GetComponent<Projectile>();
            GameObject spawnObject = projectile.m_spawnOnHit = Object.Instantiate(projectile.m_spawnOnHit, InactivePlaceHolderObject.transform, false);
            UnityEngine.Object.Destroy(spawnObject.GetComponent<Aoe>());
            spawnObject.AddComponent<T>();

            GameObject[] destroyParticles = new[] { spawnObject.transform.Find("particles/wetsplsh")?.gameObject, spawnObject.transform.Find("particles/splash_overtime")?.gameObject, spawnObject.transform.Find("particles/ooz (1)")?.gameObject };
            foreach (GameObject go in destroyParticles.Where(p => p != null))
               UnityEngine.Object.Destroy(go);
            ItemManager.Instance.AddItem(bombOoze);
         }
      }
      public static void RegisterItems(AssetBundle assetBundle)
      {
         InactivePlaceHolderObject = new GameObject("DifficultyScale_Placeholder");
         InactivePlaceHolderObject.SetActive(false);
         GameObject.DontDestroyOnLoad(InactivePlaceHolderObject);
         GameObject diffBlob = assetBundle.LoadAsset<GameObject>("DifficultyBlob");
         DifficultyBlob = new CustomItem(diffBlob, true);
         ItemManager.Instance.AddItem(DifficultyBlob);
         string difficultyBlobName = DifficultyBlob.ItemPrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;

         BiomeToItemMap[Heightmap.Biome.Meadows] = RegisterMeadowBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.BlackForest] = RegisterBlackForestBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.Swamp] = RegisterSwampBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.Mountain] = RegisterMountainBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.Plains] = RegisterPlainsBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.Mistlands] = RegisterMistlandsBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.AshLands] = RegisterAshlandsBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.DeepNorth] = RegisterDeepNorthBlob(assetBundle, difficultyBlobName);
      }

      private static CustomItem RegisterMeadowBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         Color color = new Color(0f, 1f, 0f);
         string blobName = "MeadowBlob";
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         CustomItem blob = RegisterCustomBlob(assetBundle, blobName, "Meadow Difficulty Blob", color, 1, 1, description);
         return blob;
      }
      private static CustomItem RegisterBlackForestBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         CustomItem blob = RegisterCustomBlob(assetBundle, "BlackForestBlob", "Black Forest Difficulty Blob", new Color(.1f, .1f, .1f), 1, 1, description);
         return blob;
      }
      private static CustomItem RegisterSwampBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         Color color = new Color(.36f, 0f, .36f);
         string blobName = "SwampBlob";
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         CustomItem blob = RegisterCustomBlob(assetBundle, blobName, "Swamp Difficulty Blob", color, 1, 2, description);
         return blob;
      }
      private static CustomItem RegisterMountainBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         // Used to increase armor on utility
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher, or use this item to upgrade your equipped utility item's armor.";
         CustomItem blob = RegisterCustomBlob(assetBundle, "MountainBlob", "Mountain Difficulty Blob", new Color(.96f, .96f, .96f), 1, 2, description);
         return blob;
      }
      private static CustomItem RegisterPlainsBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         CustomItem blob = RegisterCustomBlob(assetBundle, "PlainsBlob", "Plains Difficulty Blob", new Color(1f, .55f, .25f), 1, 2, description);
         return blob;
      }
      private static CustomItem RegisterMistlandsBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         CustomItem blob = RegisterCustomBlob(assetBundle, "MistlandsBlob", "Mistlands Difficulty Blob", new Color(0, .1f, .8f), 2, 5, description);
         return blob;
      }
      private static CustomItem RegisterAshlandsBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         CustomItem blob = RegisterCustomBlob(assetBundle, "AshlandsBlob", "Ashlands Difficulty Blob", new Color(1f, .2f, .1f), 1, 3, description);
         return blob;
      }
      private static CustomItem RegisterDeepNorthBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         CustomItem blob = RegisterCustomBlob(assetBundle, "DeepNorthBlob", "Deep North Difficulty Blob", new Color(.6f, .6f, .6f), 1, 3, description);
         return blob;
      }

      private static CustomItem RegisterCustomBlob(AssetBundle assetBundle, string blobName, string friendlyName, Color color, int blobRequirement, int diffBlobConversion, string description)
      {
         GameObject blobPrefab = Object.Instantiate(assetBundle.LoadAsset<GameObject>("DifficultyBlob"), InactivePlaceHolderObject.transform, false);
         blobPrefab.name = blobName;
         ItemDrop blobDrop = blobPrefab.GetComponent<ItemDrop>();
         blobDrop.m_itemData.m_shared.m_icons = new[] { Utils.LoadSprite($"ValheimDifficultyScale.assets.{blobName}Icon.png") };
         blobDrop.m_itemData.m_shared.m_name = friendlyName;
         blobDrop.m_itemData.m_shared.m_description = description;
         blobPrefab.GetComponentInChildren<MeshRenderer>().material.color = color;
         blobPrefab.GetComponentInChildren<MeshRenderer>().material.SetColor("_Color", color);
         var main = blobPrefab.GetComponent<ParticleSystem>().main;
         main.startColor = color;

         CustomItem blobItem = new CustomItem(blobPrefab, true);
         blobItem.Recipe = new CustomRecipe(new RecipeConfig()
         {
            Amount = diffBlobConversion,
            CraftingStation = "piece_BlobSmusher",
            Item = "DifficultyBlob",
            Name = friendlyName,
            Requirements = new[] {
               new RequirementConfig(blobName, blobRequirement)
              }
         });

         ItemManager.Instance.AddItem(blobItem);
         return blobItem;
      }

   }
}