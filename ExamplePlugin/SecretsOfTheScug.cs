using BepInEx;
using HarmonyLib;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using SecretsOfTheScug.Util;
using SecretsOfTheScug.Items;
using SecretsOfTheScug.Equips;

namespace SecretsOfTheScug
{

    [BepInDependency(ItemAPI.PluginGUID)]

    [BepInDependency(LanguageAPI.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class SecretsOfTheScug : BaseUnityPlugin
    {

        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "AuthorName";
        public const string PluginName = "SecretsOfTheScug";
        public const string PluginVersion = "1.0.0";

        public void Awake()
        {
            Log.Init(Logger);
            InitItems();

            ProgressiveWeaponUtil.ProgressiveWeaponHooks();
        }

        private void InitItems()
        {
            Peashooter.PeashooterInit();
            Boomerang.BoomerangInit();
            RainbowWave.RainbowWaveInit();
            //SwingingBall.SwingingBallInit();
            PearWiggler.PearWigglerInit();
            VoidEnergyDrink.VoidEnergyDrinkInit();
            VoidScug.VoidScugInit();
            DanseMacabre.DanseMacabreInit();
            //equips
            Mainspring.MainspringInit();
            Mulligan.MulliganInit();
        }
        public void CreateTransformation(On.RoR2.Items.ContagiousItemManager.orig_Init orig, ItemDef item1, ItemDef item2)
        {
            ItemDef.Pair transformation = new ItemDef.Pair()
            {
                itemDef1 = item1,
                itemDef2 = item2
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
            orig();
        }
    }
}
