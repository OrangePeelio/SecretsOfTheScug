using System;
using BepInEx;
using HarmonyLib;
using R2API;
using static R2API.RecalculateStatsAPI;
using RoR2;
using RoR2.Items;
using RoR2.Orbs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;


namespace SecretsOfTheScug
{
	public class SwingingBall
	{
        #region config
        public static ItemDef swingingBallItemDef;
        [AutoConfig("Swinging ball base movespeed", .2f)]
        public static float baseMoveSpeed = .2f;
        [AutoConfig("swinging ball stack movespeed", .2f)]
        public static float stackMoveSpeed = .2f;

        [AutoConfig("swinging ball base damage", 5f)]
        public float baseDamage = 5f;
        [AutoConfig("swinging ball stack damage", 5f)]
        public float stackDamage = 5f;

        #endregion
        public static void SwingingBallInit()
        {
            Hooks();
            CreateItem();
        }

        private static void Hooks()
        {
            GetStatCoefficients += SwingingBallStats;
        }

        static void SwingingBallStats(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.inventory == null) return;
            int itemCount = sender.inventory.GetItemCount(swingingBallItemDef);
            if (itemCount < 1) return;
            itemCount--;
            args.baseMoveSpeedAdd += baseMoveSpeed + stackMoveSpeed * itemCount;
        }
        public static void CreateItem()
		{

            swingingBallItemDef = ScriptableObject.CreateInstance<ItemDef>();
            swingingBallItemDef.name = "SWINGINGBALL";
            swingingBallItemDef.nameToken = "SWINGINGBALL_NAME";
            swingingBallItemDef.pickupToken = "SWINGINGBALL_PICKUP";
            swingingBallItemDef.descriptionToken = "SWINGINGBALL_DESC";
            swingingBallItemDef.loreToken = "SWINGINGBALL_LORE";

#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
            swingingBallItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
#pragma warning restore Publicizer001

            swingingBallItemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Bear/texBearIcon.png").WaitForCompletion();
            swingingBallItemDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bear/PickupBear.prefab").WaitForCompletion();

            swingingBallItemDef.tags = new ItemTag[] { ItemTag.Damage, ItemTag.SprintRelated, ItemTag.Utility };

            swingingBallItemDef.canRemove = true;
            swingingBallItemDef.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);

            LanguageAPI.Add(swingingBallItemDef.nameToken, "Swinging ball");
            LanguageAPI.Add(swingingBallItemDef.pickupToken, "Gain a tethered ball. Swinging the ball damages enemies based on its speed.");
            LanguageAPI.Add(swingingBallItemDef.descriptionToken, $"");
            LanguageAPI.Add(swingingBallItemDef.loreToken, "");
            ItemAPI.Add(new CustomItem(swingingBallItemDef, displayRules));
        }
	}

}

