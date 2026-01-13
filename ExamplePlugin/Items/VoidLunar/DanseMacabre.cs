using HarmonyLib;
using R2API;
using RoR2;
using static RoR2.Util;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static R2API.DirectorAPI;
using static RoR2.CombatDirector;

namespace SecretsOfTheScug.Items
{
    class DanseMacabre
    {
        public static DirectorCardCategorySelection lunarCards;
        public static DirectorCardCategorySelection voidCards;
        public static ItemDef danseMacabreItemDef;

        public static void DanseMacabreInit()
        {
            Addressables.LoadAssetAsync<DirectorCardCategorySelection>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_moon.dccsMoonMonsters_asset).Completed +=
                (ctx) => lunarCards = ctx.Result;
            Addressables.LoadAssetAsync<DirectorCardCategorySelection>(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_VoidCamp.dccsVoidCampMonsters_asset).Completed +=
                (ctx) => voidCards = ctx.Result;

            CreateItem();
            Hooks();
        }
        private static void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += AddItemBehavior;
            On.RoR2.Items.ContagiousItemManager.Init += CreateTransformation;
        }

        private static void AddItemBehavior(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                if (self.master)
                {
                    self.AddItemBehavior<DanseMacabreBehavior>(self.inventory.GetItemCount(danseMacabreItemDef));
                }
            }
        }

        public static void CreateItem()
        {

            danseMacabreItemDef = ScriptableObject.CreateInstance<ItemDef>();
            danseMacabreItemDef.name = "DANSEMACABRE";
            danseMacabreItemDef.nameToken = "DANSEMACABRE_NAME";
            danseMacabreItemDef.pickupToken = "DANSEMACABRE_PICKUP";
            danseMacabreItemDef.descriptionToken = "DANSEMACABRE_DESC";
            danseMacabreItemDef.loreToken = "DANSEMACABRE_LORE";

#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
            danseMacabreItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/DLC1/Common/VoidBossDef.asset").WaitForCompletion();
#pragma warning restore Publicizer001

            danseMacabreItemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Bear/texBearIcon.png").WaitForCompletion();
            danseMacabreItemDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bear/PickupBear.prefab").WaitForCompletion();

            danseMacabreItemDef.tags = new ItemTag[] { ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.CannotSteal, ItemTag.WorldUnique };

            danseMacabreItemDef.canRemove = true;
            danseMacabreItemDef.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);

            LanguageAPI.Add(danseMacabreItemDef.nameToken, "Danse Macabre");
            LanguageAPI.Add(danseMacabreItemDef.pickupToken, "Stay vigilant, and most importantly *have fun!*");
            LanguageAPI.Add(danseMacabreItemDef.descriptionToken, $"");
            LanguageAPI.Add(danseMacabreItemDef.loreToken, "");
            ItemAPI.Add(new CustomItem(danseMacabreItemDef, displayRules));
        }

        public static void CreateTransformation(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new ItemDef.Pair()
            {
                itemDef1 = RoR2Content.Items.MonstersOnShrineUse,
                itemDef2 = danseMacabreItemDef
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
            orig();
        }
    }
    public class DanseMacabreBehavior : CharacterBody.ItemBehavior
    {
        public static float lunarInvadeRate = 8;
        float lunarCooldownTimer = 8;
        public static float voidInvadeRate = 16;
        float voidCooldownTimer = 0;

        WeightedSelection<DirectorCard> lunarSelection;
        WeightedSelection<DirectorCard> voidSelection;

        void OnEnable()
        {
            if (DanseMacabre.lunarCards == null)
            {
                Log.Error("lnuarscards null");
            }
            if (DanseMacabre.voidCards == null)
            {
                Log.Error("voidacrds null");
            }
            lunarSelection = DanseMacabre.lunarCards.GenerateDirectorCardWeightedSelection();
            voidSelection = DanseMacabre.voidCards.GenerateDirectorCardWeightedSelection();
            if (lunarSelection == null)
            {
                Log.Error("lnuars null");
            }
            if (voidSelection == null)
            {
                Log.Error("voids null");
            }
        }
        private static WeightedSelection<DirectorCard> CreateReasonableDirectorCardSpawnList(float availableCredit, WeightedSelection<DirectorCard> monsterSelection)
        {
            if(monsterSelection.Count == 0)
            {
                Log.Error("weighted selection empty!!");
            }
            int minimumToSpawn = 2;
            int maximumNumberToSpawnBeforeSkipping = 6;

            //WeightedSelection<DirectorCard> monsterSelection = ClassicStageInfo.instance.monsterSelection;
            WeightedSelection<DirectorCard> weightedSelection = new WeightedSelection<DirectorCard>(8);
            for (int i = 0; i < monsterSelection.Count; i++)
            {
                DirectorCard value = monsterSelection.choices[i].value;
                if (value == null)
                {
                    Log.Error("no card!!!");
                }
                float combatDirectorHighestEliteCostMultiplier = CombatDirector.CalcHighestEliteCostMultiplier(value.GetSpawnCard().eliteRules);

                Log.Message(combatDirectorHighestEliteCostMultiplier);
                Log.Message(availableCredit);
                Log.Message(maximumNumberToSpawnBeforeSkipping);
                Log.Message(value.spawnCard.name);
                Log.Message(value.spawnCard.directorCreditCost);
                Log.Message(combatDirectorHighestEliteCostMultiplier);




                if (RoR2.Util.DirectorCardIsReasonableChoice(availableCredit, maximumNumberToSpawnBeforeSkipping, minimumToSpawn, value, combatDirectorHighestEliteCostMultiplier))
                {
                    Log.Debug("adding option" + monsterSelection.choices[i]);//THIS DOES NOT
                    weightedSelection.AddChoice(value, monsterSelection.choices[i].weight);
                }
            }
            return weightedSelection;
        }
        private DirectorCard ChooseDirectorCard(float availableCredit, WeightedSelection<DirectorCard> weightedSelection)
        {
            weightedSelection = CreateReasonableDirectorCardSpawnList(availableCredit, weightedSelection);
            if (weightedSelection.Count == 0)
            {
                Log.Error("weaighted selection emtpy!!");
                return null;
            }
            return weightedSelection.Evaluate(Run.instance.spawnRng.nextNormalizedFloat);
        }

        private void FixedUpdate()
        {
            if (body.outOfCombat)
            {
                lunarCooldownTimer -= Time.deltaTime;
            }
            if (lunarCooldownTimer <= 0)//make this not hardcoded
            {
                LunarInvade();
            }
            if (body.isSprinting)
            {
                voidCooldownTimer -= Time.fixedDeltaTime;
                if (voidCooldownTimer <= 0f && base.body.moveSpeed > 0f)
                {
                    voidCooldownTimer += voidInvadeRate / base.body.moveSpeed;
                    VoidInvade();
                }
            }
        }
        private void LunarInvade()
        {
            Log.Debug("lunar invade");
            lunarCooldownTimer = lunarInvadeRate;
            DoInvasion(2);
        }
        private void VoidInvade()
        {
            Log.Debug("void invade");
            voidCooldownTimer = voidInvadeRate;
            DoInvasion(3);
        }
        public void DoInvasion(int invasionType)
        {
            Log.Debug("doing invasion type " + invasionType);
            //1=monster team
            //2=lunar team
            //3=void team


            Interactor interactor;
            if ((interactor = body.GetComponent<Interactor>()) == null)
            {
                Log.Error("interactor null?");
                return;
            }
            GameObject gameObject2 = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/Encounters/MonstersOnShrineUseEncounter");
            if (gameObject2 == null)
            {
                return;
            }
            Vector3 vector = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            Transform transform = body.transform;
            if (transform)
            {
                vector = transform.position;
                rotation = transform.rotation;
            }

            GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(gameObject2, vector, Quaternion.identity);
            NetworkServer.Spawn(gameObject3);
            CombatDirector combatDirector = gameObject3.GetComponent<CombatDirector>();
            if (combatDirector && RoR2.Stage.instance)
            {
                float monsterCredit = 200f * RoR2.Stage.instance.entryDifficultyCoefficient * stack;

                
                DirectorCard directorCard = ChooseDirectorCard(combatDirector,invasionType, monsterCredit);
                if (directorCard == null)
                {
                    Log.Error("stinky null");
                    NetworkServer.Destroy(gameObject3);
                    return;
                }
                switch (invasionType)
                {
                    case 2:
                        combatDirector.teamIndex = TeamIndex.Lunar;
                        break;
                    case 3:
                        combatDirector.teamIndex = TeamIndex.Void;
                        break;
                    default:
                        combatDirector.teamIndex = TeamIndex.Monster;
                        break;
                }
                combatDirector.CombatShrineActivation(interactor, monsterCredit, directorCard);
                
                EffectData effectData = new EffectData
                {
                    origin = vector,
                    rotation = rotation
                };
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MonstersOnShrineUse"), effectData, true);
                return;
            }
            
        }
        DirectorCard ChooseDirectorCard(CombatDirector combatDirector, int invasionType, float credits)
        {
            DirectorCard directorCard;
            switch (invasionType)
            {
                case 2:
                    directorCard = ChooseDirectorCard(credits, lunarSelection);
                    Log.Debug("danse2");
                    break;
                case 3:
                    directorCard= ChooseDirectorCard(credits, voidSelection);
                    Log.Debug("danse3");

                    break;
                default:
                    directorCard = combatDirector.SelectMonsterCardForCombatShrine(credits);
                    Log.Debug("danse1");
                    break;
            }
            if (directorCard == null)
            {
                Log.Error("direcgtor card null...");
            }
            return directorCard;
        }
    }
}
