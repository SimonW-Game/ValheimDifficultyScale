using HarmonyLib;
using ItemManager;
using PieceManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using ValheimDifficultyScale.Effects;

namespace ValheimDifficultyScale
{
   public static class ItemData
   {
      public static GameObject InactivePlaceHolderObject;
      public static BuildPiece BlobSmusherTable;
      public static Item DifficultyBlob;
      public static Dictionary<Heightmap.Biome, Item> BiomeToItemMap = new Dictionary<Heightmap.Biome, Item>();
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

         Item bombOoze = new Item(assetBundle, "BombOoze");
         bombOoze.Crafting.Add("piece_BlobMusher", 1);
         bombOoze.CraftAmount = 3;
         bombOoze.RequiredItems.Add(blobName, 1);
         bombOoze.RequiredItems.Add("Ooze", 5);
         bombOoze.RequiredItems.Add("BoneFragments", 5);

         bombOoze.Prefab.GetComponentInChildren<MeshRenderer>().material.color = color;
         var main = bombOoze.Prefab.GetComponent<ParticleSystem>().main;
         main.startColor = color;
         bombOoze.Prefab.name = "StaggerBlobBomb";
         ItemDrop bombDrop = bombOoze.Prefab.GetComponent<ItemDrop>();
         bombDrop.m_itemData.m_shared.m_name = "Stagger Blob Bomb";
         bombDrop.m_itemData.m_shared.m_description = "Throw this to stagger all nearby enemies.";
         GameObject spawnObject = bombDrop.m_itemData.m_shared.m_attack.m_attackProjectile.GetComponent<Projectile>().m_spawnOnHit;
         UnityEngine.Object.Destroy(spawnObject.GetComponent<Aoe>());
         spawnObject.AddComponent<StaggerAfterTime>();

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

         Item bombOoze = new Item(assetBundle, "BombOoze");
         bombOoze.Crafting.Add("piece_BlobMusher", 1);
         bombOoze.CraftAmount = 3;
         bombOoze.RequiredItems.Add(blobName, 1);
         bombOoze.RequiredItems.Add("Resin", 8);
         bombOoze.RequiredItems.Add("FreezeGland", 4);
         bombOoze.RequiredItems.Add("Thistle", 4);

         bombOoze.Prefab.GetComponentInChildren<MeshRenderer>().material.color = color;
         var main = bombOoze.Prefab.GetComponent<ParticleSystem>().main;
         main.startColor = color;
         bombOoze.Prefab.name = "PusherBlobBomb";
         ItemDrop bombDrop = bombOoze.Prefab.GetComponent<ItemDrop>();
         bombDrop.m_itemData.m_shared.m_name = "Momentum Freezer Blob Bomb";
         bombDrop.m_itemData.m_shared.m_description = "Throw this to push and slow all nearby enemies.";
         GameObject spawnObject = bombDrop.m_itemData.m_shared.m_attack.m_attackProjectile.GetComponent<Projectile>().m_spawnOnHit;
         UnityEngine.Object.Destroy(spawnObject.GetComponent<Aoe>());
         spawnObject.AddComponent<PushAndSlowAfterTime>();

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
         GameObject blobPrefab = UnityEngine.Object.Instantiate(assetBundle.LoadAsset<GameObject>("DifficultyBlob"), InactivePlaceHolderObject.transform);
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
         ValheimDifficultyScale.instance.StartCoroutine(Delay());

         IEnumerator Delay()
         {
            yield return new WaitForSeconds(10f);
            blobItem.Snapshot();
         }
         return blobItem;
      }
   }
}