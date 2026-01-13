using RoR2;
using SecretsOfTheScug.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecretsOfTheScug.Util
{
    class ProgressiveWeaponUtil
    {
        public static void ProgressiveWeaponHooks()
        {
            On.RoR2.CharacterBody.OnSkillActivated += SnailyOnSkill;

        }
        private static void SnailyOnSkill(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            orig(self, skill);

            if (skill != self.skillLocator.primary)
                return;
            int count = 0;
            if (self.HasBuff(RainbowWave.rainbowWaveBuff))
            {
                count = self.inventory.GetItemCount(RainbowWave.rainbowWaveItemDef.itemIndex);
                RainbowWave.FireRainbowWave(self, count - 1);
            }
            else if (self.HasBuff(Boomerang.boomerangBuff))
            {
                count = self.inventory.GetItemCount(Boomerang.boomerangItemDef.itemIndex);
                Boomerang.FireBoomerang(self, count - 1);
            }
            else if ((count = self.inventory.GetItemCount(Peashooter.peashooterItemDef.itemIndex)) > 0)
            {
                Peashooter.FirePeashooter(self, count - 1);
            }
        }
    }
}
