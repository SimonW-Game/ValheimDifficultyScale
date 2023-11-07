using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimDifficultyScale.Effects
{
   internal class SE_StayInAirNotMoving : SE_Stats
   {
      public SE_StayInAirNotMoving()
      {
         m_speedModifier = -1f;
         m_ttl = 10f;
         m_maxMaxFallSpeed = -10f;
      }

      public override void ModifyWalkVelocity(ref Vector3 vel)
      {
         vel.y = 0f;
      }
      public override void ModifyJump(Vector3 baseJump, ref Vector3 jump)
      {
         jump += new Vector3(baseJump.x, 20, baseJump.z);
      }
   }
}
