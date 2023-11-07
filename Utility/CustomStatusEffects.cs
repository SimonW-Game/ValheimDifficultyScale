using UnityEngine;
using ValheimDifficultyScale.Effects;

namespace ValheimDifficultyScale
{
   public static class CustomStatusEffects
   {
      public static SE_Stats SlowdownEffect;
      public static SE_Stats AntiGravEffect;
      static CustomStatusEffects()
      {
         SlowdownEffect = ScriptableObject.CreateInstance<SE_Stats>();
         SlowdownEffect.m_speedModifier = -.6f;
         SlowdownEffect.m_ttl = 5f;

         AntiGravEffect = ScriptableObject.CreateInstance<SE_StayInAirNotMoving>();
      }
   }
}