using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimDifficultyScale.Effects
{
   public class PullAndStall : MonoBehaviour, IProjectile
   {
      private Character m_owner;
      private List<Character> _characters = new List<Character>();
      private float timeToLive = 2.5f;
      private bool _setup = false;
      public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
      {
         m_owner = owner;
         if (owner == Player.m_localPlayer)
         {
            int rayMask = LayerMask.GetMask("character", "character_net", "character_ghost");
            Collider[] collisions = Physics.OverlapSphere(base.transform.position, 15f, rayMask);
            foreach (Collider collision in collisions)
            {
               GameObject gameObject = Projectile.FindHitObject(collision);
               Character character = gameObject.GetComponent<Character>();
               if (character != null && !(character is Player) && !character.m_boss)
               {
                  _characters.Add(character);
                  character.ApplyPushback(this.transform.position - character.transform.position, 10);
                  SE_Stats slowDownEffect = character.GetSEMan()?.AddStatusEffect(CustomStatusEffects.SlowdownEffect) as SE_Stats;
                  if (slowDownEffect != null)
                  {
                     slowDownEffect.m_speedModifier = -1f;
                     slowDownEffect.m_ttl = 3f;
                  }
               }
            }
         }
         _setup = true;
      }
      private void FixedUpdate()
      {
         if (!_setup)
            return;
         timeToLive -= Time.fixedDeltaTime;
         if (timeToLive <= 0 || _characters.Count == 0)
         {
            enabled = false;
            ZNetScene.instance.Destroy(base.gameObject);
            return;
         }

         List<Character> deletedCharacters = new List<Character>();
         foreach (Character character in _characters)
         {
            character.ApplyPushback(this.transform.position - character.transform.position, 10);
            if ((this.transform.position - character.transform.position).magnitude < 1)
               deletedCharacters.Add(character);
         }

         foreach (Character character in deletedCharacters)
         {
            character.GetSEMan()?.RemoveStatusEffect(CustomStatusEffects.SlowdownEffect);
            _characters.Remove(character);
         }
      }

      public string GetTooltipString(int itemQuality)
      {
         return "Push and Slow Enemies in area.";
      }
   }
}
