using HarmonyLib;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.RecalculateStatsAPI;

namespace SecretsOfTheScug.Items
{
    class VoidScug
    {
        public static ItemDef voidScugItemDef;
        public static BuffDef voidScugBuffDef;

        public static Action<CharacterBody, StatHookEventArgs> GetStatCoefficients { get; private set; }

        public static void VoidScugInit()
        {
            CreateItem();
            CreateBuff();
            Hooks();
        }

        private static void Hooks()
        {
            On.RoR2.Items.ContagiousItemManager.Init += CreateTransformation;
            GetStatCoefficients += voidEnergyDrinkStats;
            On.RoR2.HealthComponent.TakeDamageProcess += VoidScugOnHit;
        }

        private static void VoidScugOnHit(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            Inventory inv;
            if ((inv = self.body.inventory) == null)
            {
                orig(self, damageInfo);
                return;
            }
            if (inv.GetItemCount(voidScugItemDef) == 0 )
            {
                orig(self, damageInfo);
                return;
            }
            if (!self.body.outOfDanger)
            {
                orig(self, damageInfo);
                return;
            }
            self.body.AddTimedBuffAuthority(voidScugBuffDef.buffIndex, 5 * inv.GetItemCount(voidScugItemDef));
            orig(self, damageInfo);
        }

        private static void voidEnergyDrinkStats(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(voidScugBuffDef))
            {
                args.moveSpeedMultAdd += 0.1f;
                args.attackSpeedMultAdd += 0.1f;
                args.damageMultAdd += 0.1f;
                args.armorAdd *= 1.1f;
                args.regenMultAdd += 0.1f;
            }
        }


        public static void CreateItem()
        {

            voidScugItemDef = ScriptableObject.CreateInstance<ItemDef>();
            voidScugItemDef.name = "ASCUG";
            voidScugItemDef.nameToken = "ASCUG_NAME";
            voidScugItemDef.pickupToken = "ASCUG_PICKUP";
            voidScugItemDef.descriptionToken = "ASCUG_DESC";
            voidScugItemDef.loreToken = "ASCUG_LORE";

#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
            voidScugItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/DLC1/Common/VoidTier1Def.asset").WaitForCompletion();
#pragma warning restore Publicizer001

            voidScugItemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Bear/texBearIcon.png").WaitForCompletion();
            voidScugItemDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bear/PickupBear.prefab").WaitForCompletion();

            voidScugItemDef.canRemove = true;
            voidScugItemDef.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);

            LanguageAPI.Add(voidScugItemDef.nameToken, "ime a scug");
            LanguageAPI.Add(voidScugItemDef.pickupToken, "wawawoo");
            LanguageAPI.Add(voidScugItemDef.descriptionToken, $"");
            LanguageAPI.Add(voidScugItemDef.loreToken, "");
            ItemAPI.Add(new CustomItem(voidScugItemDef, displayRules));
        }

        public static void CreateTransformation(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new ItemDef.Pair()
            {
                itemDef1 = RoR2Content.Items.HealWhileSafe,
                itemDef2 = voidScugItemDef
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
            orig();
        }
        private static void CreateBuff()
        {
            voidScugBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            voidScugBuffDef.name = "ScugStatsUp";
            voidScugBuffDef.buffColor = Color.magenta;
            voidScugBuffDef.canStack = true;
            voidScugBuffDef.isDebuff = false;
            voidScugBuffDef.eliteDef = null;
            voidScugBuffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ElementalRings/texBuffElementalRingsReadyIcon.tif").WaitForCompletion();
            ContentAddition.AddBuffDef(voidScugBuffDef);
        }
    }
}
