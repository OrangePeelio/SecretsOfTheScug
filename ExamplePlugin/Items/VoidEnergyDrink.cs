using HarmonyLib;
using On.RoR2.Items;
using R2API;
using static R2API.RecalculateStatsAPI;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace SecretsOfTheScug
{
    class VoidEnergyDrink
    {
        public static ItemDef voidEnergyDrinkItemDef;

        public static void VoidEnergyDrinkInit()
        {
            CreateItem();
            Hooks();
        }

        private static void Hooks()
        {
            On.RoR2.Items.ContagiousItemManager.Init += CreateTransformation;
            GetStatCoefficients += voidEnergyDrinkStats;
        }

        private static void voidEnergyDrinkStats(CharacterBody sender, StatHookEventArgs args)
        {
            int itemCount = sender.inventory?.GetItemCount(voidEnergyDrinkItemDef) ?? 0;
            args.sprintSpeedAdd += .4f * itemCount;
        }


        public static void CreateItem()
        {

            voidEnergyDrinkItemDef = ScriptableObject.CreateInstance<ItemDef>();
            voidEnergyDrinkItemDef.name = "BRINEYBEAD";
            voidEnergyDrinkItemDef.nameToken = "BRINEYBEAD_NAME";
            voidEnergyDrinkItemDef.pickupToken = "BRINEYBEAD_PICKUP";
            voidEnergyDrinkItemDef.descriptionToken = "BRINEYBEAD_DESC";
            voidEnergyDrinkItemDef.loreToken = "BRINEYBEAD_LORE";

#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
            voidEnergyDrinkItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/DLC1/Common/VoidTier1Def.asset").WaitForCompletion();
#pragma warning restore Publicizer001

            voidEnergyDrinkItemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Bear/texBearIcon.png").WaitForCompletion();
            voidEnergyDrinkItemDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bear/PickupBear.prefab").WaitForCompletion();

            //tags

            voidEnergyDrinkItemDef.canRemove = true;
            voidEnergyDrinkItemDef.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);

            LanguageAPI.Add(voidEnergyDrinkItemDef.nameToken, "Briney Bead");
            LanguageAPI.Add(voidEnergyDrinkItemDef.pickupToken, "DIVE speed up. Corrupts etc");
            LanguageAPI.Add(voidEnergyDrinkItemDef.descriptionToken, $"");
            LanguageAPI.Add(voidEnergyDrinkItemDef.loreToken, "");
            ItemAPI.Add(new CustomItem(voidEnergyDrinkItemDef, displayRules));
        }

        public static void CreateTransformation(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new ItemDef.Pair()
            {
                itemDef1 = RoR2Content.Items.SprintBonus,
                itemDef2 = voidEnergyDrinkItemDef
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
            orig();
        }
    }
}
