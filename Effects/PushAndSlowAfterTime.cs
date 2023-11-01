using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimDifficultyScale.Effects
{
   public class PushAndSlowAfterTime : MonoBehaviour, IProjectile
   {
      private Character m_owner;
      public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
      {
         m_owner = owner;
         if (owner == Player.m_localPlayer)
            StartCoroutine(Stagger());
      }
      private IEnumerator Stagger()
      {
         yield return new WaitForSeconds(1f);
         int rayMask = LayerMask.GetMask("character", "character_net", "character_ghost");
         Collider[] collisions = Physics.OverlapSphere(base.transform.position, 15f, rayMask);
         foreach (Collider collision in collisions)
         {
            GameObject gameObject = Projectile.FindHitObject(collision);
            Character character = gameObject.GetComponent<Character>();
            if (character != null && !(character is Player) && !character.m_boss)
            {
               character.ApplyPushback(this.transform.position, 5);
               StatusEffect slowDownEffect = character.GetSEMan()?.AddStatusEffect(CustomStatusEffects.SlowdownEffect);
               if (slowDownEffect != null)
                  slowDownEffect.m_ttl = 3;
            }
         }
         ZNetScene.instance.Destroy(base.gameObject);
      }

      public string GetTooltipString(int itemQuality)
      {
         return "Stagger Enemies in area.";
      }
   }
}
