using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;

namespace SecretsOfTheScug.Items
{
    class PearWiggler
    {
        #region config
        [AutoConfig("Healing Base", 8)]
        public static int healingBase = 8;
        [AutoConfig("Healing Stack", 4)]
        public static int healingStack = 4;
        [AutoConfig("Barrier Base", 8)]
        public static int barrierBase = 8;
        [AutoConfig("Barrier Stack", 4)]
        public static int barrierStack = 4;
        #endregion
        public static ItemDef pearWigglerItemDef;
        public static BuffDef pearBuff;
        static GameObject pear;
        static float pearLifetime = 10;
        static float gravRadius = 1f;

        public static void PearWigglerInit()
        {
            CreateItem();
            CreatePear();
            CreateBuff();
            Hooks();
        }

        private static void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamageProcess += PearWigglerTakeDamage;
            On.RoR2.GlobalEventManager.OnHitEnemy += PearWigglerOnHit;
        }

        private static void CreateBuff()
        {
            pearBuff = ScriptableObject.CreateInstance<BuffDef>();
            pearBuff.name = "Pear";
            pearBuff.buffColor = Color.red;
            pearBuff.canStack = true;
            pearBuff.isDebuff = false;
            pearBuff.eliteDef = null;
            pearBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ElementalRings/texBuffElementalRingsReadyIcon.tif").WaitForCompletion();
            ContentAddition.AddBuffDef(pearBuff);
        }
        private static void CreateItem()
        {

            pearWigglerItemDef = ScriptableObject.CreateInstance<ItemDef>();
            pearWigglerItemDef.name = "PEARWIGGLER";
            pearWigglerItemDef.nameToken = "PEARWIGGLER_NAME";
            pearWigglerItemDef.pickupToken = "PEARWIGGLER_PICKUP";
            pearWigglerItemDef.descriptionToken = "PEARWIGGLER_DESC";
            pearWigglerItemDef.loreToken = "PEARWIGGLER_LORE";

#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
            pearWigglerItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
#pragma warning restore Publicizer001

            pearWigglerItemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Bear/texBearIcon.png").WaitForCompletion();
            pearWigglerItemDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bear/PickupBear.prefab").WaitForCompletion();

            pearWigglerItemDef.tags = new ItemTag[] { ItemTag.Healing };

            pearWigglerItemDef.canRemove = true;
            pearWigglerItemDef.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);

            LanguageAPI.Add(pearWigglerItemDef.nameToken, "Pear Wiggler");
            LanguageAPI.Add(pearWigglerItemDef.pickupToken, "Chance to store a pear on hit. Taking damage wiggles pears for barrier and healing.");
            LanguageAPI.Add(pearWigglerItemDef.descriptionToken, $"");
            LanguageAPI.Add(pearWigglerItemDef.loreToken, "");
            ItemAPI.Add(new CustomItem(pearWigglerItemDef, displayRules));
        }

       
        //the pears
        private static void CreatePear()
        {
            pear = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Tooth/HealPack.prefab").WaitForCompletion().InstantiateClone("Pear", true);

            GravitatePickup gravPickup = pear.GetComponentInChildren<GravitatePickup>();
            pear.transform.localScale = Vector3.one * 2;
            if (gravPickup != null)
            {
                gravPickup.gravitateAtFullHealth = true;
                Collider gravitateTrigger = gravPickup.gameObject.GetComponent<Collider>();
                if (gravitateTrigger.isTrigger)
                {
                    gravitateTrigger.transform.localScale = Vector3.one * gravRadius;
                }
            }
            else
            {
                Debug.Log("No gravitatepickup");
            }

            DestroyOnTimer destroyTimer = pear.GetComponentInChildren<DestroyOnTimer>();
            if (destroyTimer)
            {
                destroyTimer.duration = pearLifetime;
                BeginRapidlyActivatingAndDeactivating braad = pear.GetComponent<BeginRapidlyActivatingAndDeactivating>();
                if (braad)
                {
                    braad.delayBeforeBeginningBlinking = pearLifetime - 2f;
                }
            }
        }

        private static void PearWigglerOnHit(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (!NetworkServer.active)
                return;
            CharacterBody body;
            if ((body = damageInfo.attacker.GetComponent<CharacterBody>()) == null)
            {
                return;
            }
            if (damageInfo.damage / body.damage < 4)
            {
                return;
            }
            int pearCount = body.inventory?.GetItemCount(pearWigglerItemDef) ?? 0;
            if (pearCount <= 0)
            {
                return;
            }
            if (body.GetBuffCount(pearBuff) < pearCount * 3)//make this not hardcoded idgaf
                body.AddBuff(pearBuff);
        }

        private static void PearWigglerTakeDamage(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            orig(self, damageInfo);
            if (!NetworkServer.active)
                return;

            CharacterBody body = self.body;
            if (body == null)
                return;
            int itemCount = body.inventory?.GetItemCount(pearWigglerItemDef) ?? 0;
            if (itemCount <= 0)
            {
                return;
            }
            if (self.body.GetBuffCount(pearBuff) <= 0)
            {
                return;
            }
            int percentLost = (int)Math.Round((damageInfo.damage / self.fullCombinedHealth) * 20);
            WigglePears(itemCount--, percentLost, body, self);
        }

        private static void WigglePears(int a, int b, CharacterBody body, HealthComponent healthComponent)
        {
            if (!NetworkServer.active)
                return;
            for (int i = 0; i < b && body.GetBuffCount(pearBuff) > 0; i++)//a=itemcount -1; b=%hp lost
            {
                WigglePear(a, healthComponent);
                body.RemoveBuff(pearBuff);
            }
        }


        private static void WigglePear(int i, HealthComponent a)
        {
            GameObject pearInstance = UnityEngine.Object.Instantiate<GameObject>(pear, a.body.corePosition + UnityEngine.Random.insideUnitSphere * a.body.radius * 30/*hopefully this will make it so the pears arent immediately munched*/, UnityEngine.Random.rotation);
            TeamFilter pearFilter = pearInstance.GetComponent<TeamFilter>();
            if (pearFilter)
            {
                pearFilter.teamIndex = a.body.teamComponent.teamIndex;
            }
            HealthPickup pearPickup = pearInstance.GetComponentInChildren<HealthPickup>();
            if (pearPickup)
            {
                pearPickup.fractionalHealing = 0;
                pearPickup.flatHealing = healingBase + healingStack * i;
            }
            NetworkServer.Spawn(pearInstance);
            //for (int b = 0; b > )
            a.AddBarrier(barrierBase + (barrierStack * i));
        }
    }
}
