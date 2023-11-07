using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.IO;
using UnityEngine;

namespace ValheimDifficultyScale
{
   internal static class Utils
   {
      public static Sprite LoadSprite(string assetPath)
      {
         // Load texture and create sprite
         Texture2D texture = LoadTexture(assetPath);
         if (!texture)
            return null;

         return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
      }
      public static Texture2D LoadTexture(string texturePath)
      {

         byte[] fileData = new byte[0];
         using (Stream resFilestream = GetStreamFileFileName(texturePath))
         {
            if (resFilestream == null) return null;
            fileData = new byte[resFilestream.Length];
            resFilestream.Read(fileData, 0, fileData.Length);
         }

         Texture2D tex = new Texture2D(2, 2);
         tex.LoadImage(fileData, false);
         return tex;
      }
      public static Stream GetStreamFileFileName(string path)
      {
         System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
         return a.GetManifestResourceStream(path);
      }
   }
}