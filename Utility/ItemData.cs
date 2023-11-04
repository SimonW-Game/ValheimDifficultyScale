using HarmonyLib;
using ItemManager;
using PieceManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ValheimDifficultyScale.Effects;

namespace ValheimDifficultyScale
{
   public static class ItemData
   {
      public static GameObject InactivePlaceHolderObject;
      public static BuildPiece BlobSmusherTable;
      public static Item DifficultyBlob;
      public static Dictionary<Heightmap.Biome, Item> BiomeToItemMap = new Dictionary<Heightmap.Biome, Item>();
      //[HarmonyPatch(typeof(Piece), nameof(Piece.Awake))]
      //internal static class PieceUtil
      //{
      //   private static void Prefix(Piece __instance)
      //   {
      //      if (__instance.name == "piece_stonecutter")
      //         Debug.Log("ALERT ALERT ALERT ALERT ALERT ALERT ALERT ALERT ALERT ALERT ALERT ALERT ALERT ALERT ALERT ALERT ");
      //      Debug.Log($"@@@####!!!!!~~~~~ Registering {__instance.name}");
      //   }
      //}

      [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
      internal static class ODBUtil
      {
         private static void Prefix(ObjectDB other)
         {
            GameObject stone_cutter = other.m_items.FirstOrDefault(go => go.GetComponent<ItemDrop>()?.m_itemData?.m_shared.m_buildPieces?.m_pieces?.FirstOrDefault(p => p.name == "piece_stonecutter"));

            Debug.Log($"!!!!!~~~~~ Found Stonecutter? {stone_cutter != null}, is it a piece? {stone_cutter?.GetComponent<Piece>() != null}");
            GameObject hammerObj = other.m_items.FirstOrDefault(go => go.name == "Hammer");
            GameObject stoneCutter = hammerObj.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_buildPieces?.m_pieces?.FirstOrDefault(p => p.name == "piece_stonecutter");

            Debug.Log($"Stonecutter is something? : {(stoneCutter?.name ?? "no it isn't")}");
            //if (stoneCutter != null)
            //{
            //   AddTable(ValheimDifficultyScale.AssetBundle);// stoneCutter);
            //}
            GameObject bombOozeObj = other.m_items.FirstOrDefault(go => go.name == "BombOoze");
            MakeMeadowBlobBomb(bombOozeObj);
            MakeForestBlobBomb(bombOozeObj);
            MakeSwampBlobBomb(bombOozeObj);
         }

         internal static void MakeMeadowBlobBomb(GameObject bombOozeObj)
         {
            Color color = new Color(0f, 1f, 0f);
            Dictionary<string, int> recipe = new Dictionary<string, int>()
            {
               { "MeadowBlob", 1 },
               { "Ooze", 6 },
               { "BoneFragments", 6 },
            };
            string name = "StaggerBlobBomb";
            string friendlyName = "Stagger Blob Bomb";
            string description = "Throw this to stagger all nearby enemies.";
            RegisterBlobBomb<StaggerAfterTime>(bombOozeObj, color, friendlyName, name, description, recipe);
         }
         internal static void MakeForestBlobBomb(GameObject bombOozeObj)
         {
            Color color = new Color(.1f, .1f, .1f);
            Dictionary<string, int> recipe = new Dictionary<string, int>()
            {
               { "Resin", 1 },
               //{ "ForestBlob", 1 },
               //{ "Guck", 4 },
               //{ "WitheredBone", 1 },
            };
            string name = "PullBlobBomb";
            string friendlyName = "Pull Blob Bomb";
            string description = "Throw this to pull all nearby enemies.";
            RegisterBlobBomb<PullAndStall>(bombOozeObj, color, friendlyName, name, description, recipe);
         }
         internal static void MakeSwampBlobBomb(GameObject bombOozeObj)
         {
            Color color = new Color(.36f, 0f, .36f);
            Dictionary<string, int> recipe = new Dictionary<string, int>()
            {
               { "SwampBlob", 1 },
               { "FreezeGland", 4 },
               { "Resin", 8 },
               { "Honey", 6 },
            };
            string name = "PusherBlobBomb";
            string friendlyName = "Push N Slow Blob Bomb";
            string description = "Throw this to push and slow all nearby enemies.";
            RegisterBlobBomb<PushAndSlow>(bombOozeObj, color, friendlyName, name, description, recipe);
         }
         internal static void RegisterBlobBomb<T>(GameObject bombOozeObj,
            Color color,
            string friendlyName,
            string name,
            string description,
            Dictionary<string, int> recipe) where T : MonoBehaviour
         {
            GameObject bombPrefab = Object.Instantiate(bombOozeObj, InactivePlaceHolderObject.transform, false);
            bombPrefab.name = name;
            Item bombOoze = new Item(bombPrefab);
            bombOoze.Crafting.Add("piece_BlobMusher", 1);
            bombOoze.CraftAmount = 4;
            foreach (var kv in recipe)
               bombOoze.RequiredItems.Add(kv.Key, kv.Value);
            bombOoze.Prefab.GetComponentInChildren<MeshRenderer>().material.color = color;
            var main = bombOoze.Prefab.GetComponent<ParticleSystem>().main;
            main.startColor = color;
            ItemDrop bombDrop = bombOoze.Prefab.GetComponent<ItemDrop>();
            bombDrop.name = bombPrefab.name;

            bombDrop.m_itemData = bombPrefab.GetComponent<ItemDrop>().m_itemData.Clone();
            bombDrop.m_itemData.m_shared.m_name = friendlyName;
            bombDrop.m_itemData.m_shared.m_description = description;
            bombDrop.m_itemData.m_shared.m_attack.m_attackProjectile = Object.Instantiate(bombDrop.m_itemData.m_shared.m_attack.m_attackProjectile, InactivePlaceHolderObject.transform, false);
            Projectile projectile = bombDrop.m_itemData.m_shared.m_attack.m_attackProjectile.GetComponent<Projectile>();
            GameObject spawnObject = projectile.m_spawnOnHit = Object.Instantiate(projectile.m_spawnOnHit, InactivePlaceHolderObject.transform, false);
            UnityEngine.Object.Destroy(spawnObject.GetComponent<Aoe>());
            spawnObject.AddComponent<T>();

            GameObject[] destroyParticles = new[] { spawnObject.transform.Find("particles/wetsplsh")?.gameObject, spawnObject.transform.Find("particles/splash_overtime")?.gameObject, spawnObject.transform.Find("particles/ooz (1)")?.gameObject };
            foreach (GameObject go in destroyParticles.Where(p => p != null))
               UnityEngine.Object.Destroy(go);
            ValheimDifficultyScale.instance.StartCoroutine(DelaySnapshot(bombOoze));
         }
      }
      public static void RegisterItems(AssetBundle assetBundle)
      {
         InactivePlaceHolderObject = new GameObject("DifficultyScale_Placeholder");
         InactivePlaceHolderObject.SetActive(false);
         GameObject.DontDestroyOnLoad(InactivePlaceHolderObject);

         DifficultyBlob = new Item(assetBundle, "DifficultyBlob");
         string difficultyBlobName = DifficultyBlob.Prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;

         BlobSmusherTable = AddTable(assetBundle);

         BiomeToItemMap[Heightmap.Biome.Meadows] = RegisterMeadowBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.BlackForest] = RegisterBlackForestBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.Swamp] = RegisterSwampBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.Mountain] = RegisterMountainBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.Plains] = RegisterPlainsBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.Mistlands] = RegisterMistlandsBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.AshLands] = RegisterAshlandsBlob(assetBundle, difficultyBlobName);
         BiomeToItemMap[Heightmap.Biome.DeepNorth] = RegisterDeepNorthBlob(assetBundle, difficultyBlobName);
      }
      private static BuildPiece AddTable(AssetBundle assetBundle)
      {
         //GameObject blobSmusher = Object.Instantiate(stoneCutter, InactivePlaceHolderObject.transform, false);
         //blobSmusher.name = "piece_BlobMusher";
         BuildPiece blobMusherTable = new BuildPiece(assetBundle, "piece_BlobMusher");
         blobMusherTable.RequiredItems.Add("MeadowBlob", 1, true);
         blobMusherTable.RequiredItems.Add("ForestBlob", 1, true);
         blobMusherTable.RequiredItems.Add("RoundLog", 10, true);
         blobMusherTable.RequiredItems.Add("DifficultyBlob", 1, true);
         blobMusherTable.Category.Set(BuildPieceCategory.Crafting);
         blobMusherTable.Snapshot();
         return blobMusherTable;
      }

      private static Item RegisterMeadowBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         Color color = new Color(0f, 1f, 0f);
         string blobName = "MeadowBlob";
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         Item blob = RegisterCustomBlob(assetBundle, blobName, "Meadow Difficulty Blob", color, 1, 1, description);
         return blob;
      }
      private static Item RegisterBlackForestBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         Item blob = RegisterCustomBlob(assetBundle, "ForestBlob", "Black Forest Difficulty Blob", new Color(.1f, .1f, .1f), 1, 1, description);
         return blob;
      }
      private static Item RegisterSwampBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         Color color = new Color(.36f, 0f, .36f);
         string blobName = "SwampBlob";
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         Item blob = RegisterCustomBlob(assetBundle, blobName, "Swamp Difficulty Blob", color, 1, 2, description);
         return blob;
      }
      private static Item RegisterMountainBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         // Used to increase armor on utility
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher, or use this item to upgrade your equipped utility item's armor.";
         Item blob = RegisterCustomBlob(assetBundle, "MountainBlob", "Mountain Difficulty Blob", new Color(.96f, .96f, .96f), 1, 2, description);
         return blob;
      }
      private static Item RegisterPlainsBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         Item blob = RegisterCustomBlob(assetBundle, "PlainsBlob", "Plains Difficulty Blob", new Color(1f, .55f, .25f), 1, 2, description);
         return blob;
      }
      private static Item RegisterMistlandsBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         Item blob = RegisterCustomBlob(assetBundle, "MistlandsBlob", "Mistlands Difficulty Blob", new Color(0, .1f, .8f), 2, 5, description);
         return blob;
      }
      private static Item RegisterAshlandsBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         Item blob = RegisterCustomBlob(assetBundle, "AshlandsBlob", "Ashlands Difficulty Blob", new Color(1f, .2f, .1f), 1, 3, description);
         return blob;
      }
      private static Item RegisterDeepNorthBlob(AssetBundle assetBundle, string difficultyBlobName)
      {
         string description = $"Can be converted to regular {difficultyBlobName} in a Blob Smusher.";
         Item blob = RegisterCustomBlob(assetBundle, "NorthBlob", "Deep North Difficulty Blob", new Color(.6f, .6f, .6f), 1, 3, description);
         return blob;
      }

      private static Item RegisterCustomBlob(AssetBundle assetBundle, string blobName, string friendlyName, Color color, int blobRequirement, int diffBlobConversion, string description)
      {
         GameObject blobPrefab = Object.Instantiate(assetBundle.LoadAsset<GameObject>("DifficultyBlob"), InactivePlaceHolderObject.transform, false);
         blobPrefab.name = blobName;
         ItemDrop blobDrop = blobPrefab.GetComponent<ItemDrop>();
         blobDrop.m_itemData.m_shared.m_name = friendlyName;
         blobDrop.m_itemData.m_shared.m_description = description;
         blobPrefab.GetComponentInChildren<MeshRenderer>().material.color = color;
         blobPrefab.GetComponentInChildren<MeshRenderer>().material.SetColor("_Color", color);
         var main = blobPrefab.GetComponent<ParticleSystem>().main;
         main.startColor = color;

         DifficultyBlob[blobName].Crafting.Add("piece_BlobMusher", 1);
         DifficultyBlob[blobName].CraftAmount = diffBlobConversion;
         DifficultyBlob[blobName].RequiredItems.Add(blobName, blobRequirement);

         Item blobItem = new Item(blobPrefab);
         ValheimDifficultyScale.instance.StartCoroutine(DelaySnapshot(blobItem));

         return blobItem;
      }
      private static IEnumerator DelaySnapshot(Item item)
      {
         yield return new WaitForSeconds(10f);
         item.Snapshot();
      }
   }
}