using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Runtime.CompilerServices;
using CreatureLevelControl;

namespace ValheimDifficultyScale
{
   [HarmonyPatch(typeof(Minimap))]
   public static class WorldLevelMinimapCircles
   {
      public static bool IsShowingCircles = true;
      public static bool _keyDown = false;

      private static Color[] _originalMapColors = null;
      private static Color[] _originalForestColors = null;
      private static Color[] _originalHeightColors = null;
      private static Color[] _originalfogColors = null;

      private static Color[] _modifiedMapColors = null;
      private static Color[] _modifiedForestColors = null;
      private static Color[] _modifiedHeightColors = null;
      private static Color[] _modifiedfogColors = null;

      private static HashSet<Vector2Int> _fogColors = new HashSet<Vector2Int>();

      private static Texture2D _mapTexture;
      private static Texture2D _forestMaskTexture;
      private static Texture2D _heightTexture;
      private static Texture2D _fogTexture;

      [HarmonyPatch("GenerateWorldMap"), HarmonyPostfix, HarmonyPriority(100), UsedImplicitly]
      private static void Minimap_GenerateWorldMap_Postfix(Minimap __instance,
         Texture2D ___m_mapTexture,
         Texture2D ___m_forestMaskTexture,
         Texture2D ___m_heightTexture,
         Texture2D ___m_fogTexture,
         Color ___noForest)
      {
         if (!IsShowingCircles)
            return;

         _originalMapColors = ___m_mapTexture.GetPixels();
         _originalForestColors = ___m_forestMaskTexture.GetPixels();
         _originalHeightColors = ___m_heightTexture.GetPixels();
         _originalfogColors = ___m_fogTexture.GetPixels();

         _mapTexture = ___m_mapTexture;
         _forestMaskTexture = ___m_forestMaskTexture;
         _heightTexture = ___m_heightTexture;
         _fogTexture = ___m_fogTexture;

         DrawDistanceLevelCircles(__instance, ___m_heightTexture, ___m_forestMaskTexture, ___m_mapTexture, ___noForest);
      }

      private static void DrawDistanceLevelCircles(Minimap minimap,
         Texture2D m_heightTexture,
         Texture2D m_forestMaskTexture,
         Texture2D m_mapTexture,
         Color noForest)
      {
         _modifiedMapColors = new Color[_originalMapColors.Length];
         Array.Copy(_originalMapColors, _modifiedMapColors, _originalMapColors.Length);
         _modifiedForestColors = m_forestMaskTexture.GetPixels();
         Array.Copy(_originalForestColors, _modifiedForestColors, _originalForestColors.Length);
         _modifiedHeightColors = m_heightTexture.GetPixels();
         Array.Copy(_originalHeightColors, _modifiedHeightColors, _originalHeightColors.Length);

         foreach (int tierCount in Enumerable.Range(1, ValheimDifficultyScale.NUM_LOCATION_TIERS - 1))
         {
            float item = (ValheimDifficultyScale.EDGE_OF_WORLD / ValheimDifficultyScale.NUM_LOCATION_TIERS) * tierCount;
            ZoneSystem.instance.GetLocationIcon(Game.instance.m_StartLocation, out var pos);
            Vector2 vector = (float)minimap.m_textureSize / 2f * Vector2.one + new Vector2(pos.x, pos.z) / minimap.m_pixelSize;
            float pixelDistance = (float)item / minimap.m_pixelSize;
            int circumference = Mathf.CeilToInt(pixelDistance * 2f * (float)Math.PI);
            int i;
            for (i = 0; i < circumference; i++)
            {
               float xSin = vector.x + pixelDistance * Mathf.Sin((float)Math.PI * 2f * (float)i / (float)circumference);
               float yCos = vector.y + pixelDistance * Mathf.Cos((float)Math.PI * 2f * (float)i / (float)circumference);
               int xNeg = ((!(xSin % 1f > 0.5f)) ? 1 : (-1));
               int yNeg = ((!(yCos % 1f > 0.5f)) ? 1 : (-1));
               apply(xSin, yCos, .4f - Mathf.Abs(0.5f - xSin % 1f) * Mathf.Abs(0.5f - yCos % 1f) * 2f);
               apply(xSin + (float)xNeg, yCos, 0.4f - (0.5f - Mathf.Abs(0.5f - xSin % 1f)) * Mathf.Abs(0.5f - yCos % 1f) * 2f);
               apply(xSin, yCos + (float)yNeg, 0.4f - (0.5f - Mathf.Abs(0.5f - yCos % 1f)) * Mathf.Abs(0.5f - xSin % 1f) * 2f);
               apply(xSin + (float)xNeg, yCos + (float)yNeg, 0.4f - (0.5f - Mathf.Abs(0.5f - xSin % 1f)) * (0.5f - Mathf.Abs(0.5f - yCos % 1f)) * 2f);
            }

            void apply(float x, float y, float intensity)
            {
               if (!(x < 0f) && Mathf.RoundToInt(x) < minimap.m_textureSize && !(y < 0f) && Mathf.RoundToInt(y) < minimap.m_textureSize)
               {
                  int textureIndex = Mathf.RoundToInt(y) * minimap.m_textureSize + Mathf.RoundToInt(x);
                  Color color = ((i % 20 < 10) ? Color.red : Color.blue);
                  _modifiedMapColors[textureIndex] = (_modifiedMapColors[textureIndex] == Color.white) ? color : Color.Lerp(_modifiedMapColors[textureIndex], color, intensity);
                  _fogColors.Add(new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y)));
                  if (intensity > 0.1f)
                  {
                     _modifiedForestColors[textureIndex] = noForest;
                     _modifiedHeightColors[textureIndex] = new Color(Mathf.Clamp(_modifiedHeightColors[textureIndex].r, ZoneSystem.instance.m_waterLevel + 4f, 89f), 0f, 0f);
                  }
               }
            }
         }

         m_mapTexture.SetPixels(_modifiedMapColors);
         m_mapTexture.Apply();
         m_forestMaskTexture.SetPixels(_modifiedForestColors);
         m_forestMaskTexture.Apply();
         m_heightTexture.SetPixels(_modifiedHeightColors);
         m_heightTexture.Apply();
      }
      [HarmonyPatch("SetMapData"), HarmonyPostfix, HarmonyPriority(100), UsedImplicitly]
      private static void PatchFogCircles_Postfix(Minimap __instance,
            Texture2D ___m_fogTexture,
            bool[] ___m_exploredOthers)
      {
         foreach (Vector2Int point in _fogColors)
         {
            float fogOverwrite = .65f;
            Color fogcolor = ___m_fogTexture.GetPixel(point.x, point.y);
            if (IsShowingCircles)
               fogcolor.g = Math.Min(fogcolor.g, fogOverwrite);
            else
               fogcolor.g = ___m_exploredOthers[point.y * __instance.m_textureSize + point.x] ? 0 : 1;
            ___m_fogTexture.SetPixel(point.x, point.y, fogcolor);
         }
         ___m_fogTexture.Apply();
      }

      private static void UpdateShowingCircles(Minimap minimap, bool[] m_exploredOthers)
      {
         _mapTexture.SetPixels(IsShowingCircles ? _modifiedMapColors : _originalMapColors);
         _mapTexture.Apply();
         _forestMaskTexture.SetPixels(IsShowingCircles ? _modifiedForestColors : _originalForestColors);
         _forestMaskTexture.Apply();
         _heightTexture.SetPixels(IsShowingCircles ? _modifiedHeightColors : _originalHeightColors);
         _heightTexture.Apply();
         PatchFogCircles_Postfix(minimap, _fogTexture, m_exploredOthers);
      }

      [HarmonyPatch("UpdateMap"), HarmonyPostfix, HarmonyPriority(100), UsedImplicitly]
      private static void UpdateMap_Postfix(Minimap __instance, bool[] ___m_exploredOthers)
      {
         if (_keyDown != ValheimDifficultyScale.ModConfig.ToggleMinimapCirclesKey.Value.IsPressed())
         {
            _keyDown = !_keyDown;
            if (_keyDown)
            {
               IsShowingCircles = !IsShowingCircles;
               UpdateShowingCircles(__instance, ___m_exploredOthers);
            }
         }
      }
   }
}