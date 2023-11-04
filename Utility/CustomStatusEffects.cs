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
   public static class CustomStatusEffects
   {
      public static SE_Stats SlowdownEffect;
      static CustomStatusEffects()
      {
         SlowdownEffect = ScriptableObject.CreateInstance<SE_Stats>();
         SlowdownEffect.m_speedModifier = -.6f;
         SlowdownEffect.m_ttl = 5f;
      }
   }
}