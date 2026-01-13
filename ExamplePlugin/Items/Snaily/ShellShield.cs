using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SecretsOfTheScug.Items.Snaily
{
    class ShellShield
    {
        #region config
        [AutoConfig("Percent Barrier Base", 0.4f)]
        public static float percentBase = 0.4f;
        [AutoConfig("Percent Barrier Stack", 0)]
        public static float percentStack = 0;
        [AutoConfig("Flat Barrier Base", 0)]
        public static int flatBase = 0;
        [AutoConfig("Flat Barrier Stack", 40)]
        public static int flatStack = 40;
        [AutoConfig("Barrier Decay Freeze Base", 1f)]
        public static float decayFreezeBase = 1f;
        [AutoConfig("Barrier Decay Freeze Stack", 1f)]
        public static float decayFreezeStack = 1f;

        public string ConfigName => "Item: Shell Shield";
        #endregion
        public static ItemDef shellShieldItemDef;

        public static void CreateItem()
        {

            shellShieldItemDef = ScriptableObject.CreateInstance<ItemDef>();
            shellShieldItemDef.name = "SHELLSHIELD";
            shellShieldItemDef.nameToken = "SHELLSHIELD_NAME";
            shellShieldItemDef.pickupToken = "SHELLSHIELD_PICKUP";
            shellShieldItemDef.descriptionToken = "SHELLSHIELD_DESC";
            shellShieldItemDef.loreToken = "SHELLSHIELD_LORE";

#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
            shellShieldItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
#pragma warning restore Publicizer001

            shellShieldItemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Bear/texBearIcon.png").WaitForCompletion();
            shellShieldItemDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bear/PickupBear.prefab").WaitForCompletion();

            shellShieldItemDef.tags = new ItemTag[] { ItemTag.Utility };

            shellShieldItemDef.canRemove = true;
            shellShieldItemDef.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);

            LanguageAPI.Add(shellShieldItemDef.nameToken, "Shell Shield");
            LanguageAPI.Add(shellShieldItemDef.pickupToken, "Gain a tethered ball. Swinging the ball damages enemies based on its speed.");
            LanguageAPI.Add(shellShieldItemDef.descriptionToken, $"");
            LanguageAPI.Add(shellShieldItemDef.loreToken, "");
            ItemAPI.Add(new CustomItem(shellShieldItemDef, displayRules));
        }

    }
}
