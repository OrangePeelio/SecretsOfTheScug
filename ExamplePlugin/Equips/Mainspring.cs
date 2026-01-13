using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static R2API.CustomEquipment;
using static R2API.RecalculateStatsAPI;

namespace SecretsOfTheScug.Equips
{
    class Mainspring
    {
        #region config
        [AutoConfig("Max attack speed multipier", 2f)]
        public static float maxAttackSpeedMultiplier = 2f;
        [AutoConfig("Minimum attack speed multiplier", 0f)]
        public static float minAttackSpeedMultiplier = 0f;
        [AutoConfig("Max movement speed multipier", 2f)]
        public static float maxMoveSpeedMultiplier = 2f;
        [AutoConfig("Minimum movement speed multiplier", 0f)]
        public static float minMoveSpeedMultiplier = 0f;
        [AutoConfig("Cooldown", 5f)]
        public static float cooldown = 5f;
        #endregion

        public static EquipmentDef mainspringEquipmentDef;
        public static BuffDef mainspringBuffDef;
        public static BuffDef mainspringWindupBuffDef;
        public static void MainspringInit()
        {
            Hooks();
            CreateBuff();
            CreateEquipment();
        }

        static void Hooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
            GetStatCoefficients += MainspringStats;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += WindDown;

        }

        private static void WindDown(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            Log.Warning(buffDef.name);
            if (buffDef != BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex("Windup")))
            {
                return;
            }
            Log.Debug("buff killed");
            if (self.inventory.currentEquipmentIndex == mainspringEquipmentDef.equipmentIndex)
            {
                Log.Debug("skadoop");
            }
            Log.Debug("winding down!");
            self.AddBuff(mainspringBuffDef);

            
        }

        private static void MainspringStats(CharacterBody sender, StatHookEventArgs args)
        {
            int buffs;
            bool woundDown = sender.HasBuff(mainspringBuffDef);
            if ((buffs = sender.GetBuffCount(mainspringWindupBuffDef)) == 0 && !woundDown)
            {
                return;
            }
            if (woundDown && buffs == 0)
            {
                args.moveSpeedRootCount += 1;
                if (buffs == 0)
                {
                    args.attackSpeedReductionMultAdd += 10;
                    return;
                }                
            }
            if (buffs >= 10)
            {

                args.moveSpeedMultAdd += (buffs - 10) * 0.1f;
                args.attackSpeedMultAdd += (buffs -10) * 0.1f;
            }
            else
            {
                args.moveSpeedReductionMultAdd += 1 - (0.1f * buffs);
                args.attackSpeedReductionMultAdd += 1 - (0.1f * buffs);
            }
        }

        protected static bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == mainspringEquipmentDef)
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }

        protected static bool ActivateEquipment(EquipmentSlot slot)
        {
            //if (slot.subcooldownTimer < ForcedCooldownBetweenEquipmentUses) { slot.subcooldownTimer = ForcedCooldownBetweenEquipmentUses; }
            if (!slot.characterBody || !slot.characterBody.teamComponent || !slot.characterBody.master) return false;

            CharacterMaster ownerMaster = slot.characterBody.master;
            CharacterBody body = ownerMaster.GetBody();
            if (body.HasBuff(mainspringBuffDef))
            {
                body.RemoveBuff(mainspringBuffDef);
            }
            int buffCount = body.GetBuffCount(mainspringWindupBuffDef);
            for (int i = 0; i < 5 && buffCount < 20; i++)
            {
                body.AddTimedBuffAuthority(mainspringWindupBuffDef.buffIndex, 5 * buffCount);
                buffCount ++;
            }
            body.AddTimedBuffAuthority(mainspringBuffDef.buffIndex, 1);

            return true;
        }

        static void CreateBuff()
        {
            mainspringBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            mainspringBuffDef.name = "Wound down";
            mainspringBuffDef.buffColor = Color.black;
            mainspringBuffDef.canStack = false;
            mainspringBuffDef.isDebuff = false;
            mainspringBuffDef.eliteDef = null;
            mainspringBuffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ElementalRings/texBuffElementalRingsReadyIcon.tif").WaitForCompletion();
            ContentAddition.AddBuffDef(mainspringBuffDef);

            mainspringWindupBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            mainspringWindupBuffDef.name = "Windup";
            mainspringWindupBuffDef.buffColor = Color.white;
            mainspringWindupBuffDef.canStack = true;
            mainspringWindupBuffDef.isDebuff = false;
            mainspringWindupBuffDef.eliteDef = null;
            mainspringWindupBuffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ElementalRings/texBuffElementalRingsReadyIcon.tif").WaitForCompletion();
            ContentAddition.AddBuffDef(mainspringWindupBuffDef);
        }
        static void CreateEquipment()
        {
            mainspringEquipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();
            mainspringEquipmentDef.name = "MAINSPRING";
            mainspringEquipmentDef.nameToken = "MAINSPRING_NAME";
            mainspringEquipmentDef.pickupToken = "MAINSPRING_PICKUP";
            mainspringEquipmentDef.descriptionToken = "MAINSPRING_DESC";
            mainspringEquipmentDef.loreToken = "MAINSPRING_LORE";

            mainspringEquipmentDef.isLunar = true;
            mainspringEquipmentDef.cooldown = cooldown;
            mainspringEquipmentDef.enigmaCompatible = false;
            mainspringEquipmentDef.canBeRandomlyTriggered = false;
            ContentAddition.AddEquipmentDef(mainspringEquipmentDef);
        }
    }
}
