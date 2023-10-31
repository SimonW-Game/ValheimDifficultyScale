using System;
using System.IO;
using UnityEngine;

namespace ValheimDifficultyScale
{
   internal static class Utils
   {
      public static Stream GetStreamFileFileName(string path)
      {
         System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
         return a.GetManifestResourceStream(path);
      }
   }

}