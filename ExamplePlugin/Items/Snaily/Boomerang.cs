using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace SecretsOfTheScug.Items
{
    class Boomerang
    {
        #region config
        public string ConfigName => "Boomerang";

        public static GameObject boomerangPrefab;
        [AutoConfig("Max Boomerang Fly-Out Time", 0.3f)]
        public static float maxFlyOutTime = 0.3f; //0.6f
        [AutoConfig("Boomerang Scale Factor", 0.3f)]
        public static float boomerangScale = 0.3f; //1.0f
        [AutoConfig("Boomerang Speed", 100f)]
        public static float boomerangSpeed = 100f; //1.0f

        [AutoConfig("Damage Base", 2f)]
        public static float damageBase = 2f;
        [AutoConfig("Damage Stack", 1f)]
        public static float damageStack = 1f;
        [AutoConfig("Proc Coefficient", 0.8f)]
        public static float procCoefficient = 0.8f;
        [AutoConfig("Force", 150f)]
        public static float force = 150f;
        #endregion
        public static BuffDef boomerangBuff;
        public static ItemDef boomerangItemDef;

        public static void BoomerangInit()
        {
            CreateItem();
            CreateProjectile();
            CreateBuff();
            Hooks();
        }

        public static void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += AddItemBehavior;
        }
        private static void AddItemBehavior(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                if (self.master)
                {
                    BoomerangBehavior boomerangBehavior = self.AddItemBehavior<BoomerangBehavior>(self.inventory.GetItemCount(boomerangItemDef.itemIndex));
                }
            }
        }

        public static void FireBoomerang(CharacterBody body, int count)
        {
            Log.Debug("fired boomb");
            if (!NetworkServer.active)
                return;
            if (body == null || body.GetBuffCount(boomerangBuff) <= 0)
                return;
            body.RemoveBuff(boomerangBuff);

            Vector3 forward = body.inputBank.aimDirection;
            ProjectileManager.instance.FireProjectile(new FireProjectileInfo
            {
                damage = body.damage * (damageBase + damageStack * count),
                crit = body.RollCrit(),
                damageColorIndex = DamageColorIndex.Item,
                position = body.corePosition/* + Vector3.up * 3*/,
                force = force,
                owner = body.gameObject,
                projectilePrefab = boomerangPrefab,
                rotation = RoR2.Util.QuaternionSafeLookRotation(forward),
                speedOverride = boomerangSpeed //20
            });
        }

        private static void CreateItem()
        {

            boomerangItemDef = ScriptableObject.CreateInstance<ItemDef>();
            boomerangItemDef.name = "BOOMERANG";
            boomerangItemDef.nameToken = "BOOMERANG_NAME";
            boomerangItemDef.pickupToken = "BOOMERANG_PICKUP";
            boomerangItemDef.descriptionToken = "BOOMERANG_DESC";
            boomerangItemDef.loreToken = "BOOMERANG_LORE";

#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
            boomerangItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
#pragma warning restore Publicizer001

            boomerangItemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Bear/texBearIcon.png").WaitForCompletion();
            boomerangItemDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bear/PickupBear.prefab").WaitForCompletion();

            boomerangItemDef.tags = new ItemTag[] { ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

            boomerangItemDef.canRemove = true;
            boomerangItemDef.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);

            LanguageAPI.Add(boomerangItemDef.nameToken, "Swinging ball");
            LanguageAPI.Add(boomerangItemDef.pickupToken, "Gain a tethered ball. Swinging the ball damages enemies based on its speed.");
            LanguageAPI.Add(boomerangItemDef.descriptionToken, $"");
            LanguageAPI.Add(boomerangItemDef.loreToken, "");
            ItemAPI.Add(new CustomItem(boomerangItemDef, displayRules));
        }
        private static void CreateBuff()
        {
           boomerangBuff = ScriptableObject.CreateInstance<BuffDef>();
            boomerangBuff.name = "Boomerang stock";
            boomerangBuff.buffColor = Color.red;
            boomerangBuff.canStack = true;
            boomerangBuff.isDebuff = false;
            boomerangBuff.eliteDef = null;
            boomerangBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ElementalRings/texBuffElementalRingsReadyIcon.tif").WaitForCompletion();
            ContentAddition.AddBuffDef(boomerangBuff);
        }
        private static void CreateProjectile()
        {
            boomerangPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/Sawmerang").InstantiateClone("SnailyBoomerang", true);
            GameObject ghost = LegacyResourcesAPI.Load<GameObject>("prefabs/projectileghosts/WindbladeProjectileGhost").InstantiateClone("SnailyBoomerangGhost", false);//if this doesnt work and you have to do it the other way:RoR2/Base/Vulture/WindbladeProjectileGhost.prefab
            boomerangPrefab.transform.localScale = Vector3.one * boomerangScale;

            BoomerangProjectile bp = boomerangPrefab.GetComponent<BoomerangProjectile>();
            bp.travelSpeed = boomerangSpeed;
            bp.transitionDuration = 0.8f;
            bp.distanceMultiplier = maxFlyOutTime;
            bp.canHitWorld = false;

            ProjectileController pc = bp.GetComponent<ProjectileController>();
            pc.ghostPrefab = ghost;

            ProjectileDamage pd = bp.GetComponent<ProjectileDamage>();
            pd.damageType.damageSource = DamageSource.Primary;

            ProjectileDotZone pdz = boomerangPrefab.GetComponent<ProjectileDotZone>();
            /*pdz.overlapProcCoefficient = 0.8f;
            pdz.damageCoefficient = 1f;
            pdz.resetFrequency = 1 / (maxFlyOutTime + bp.transitionDuration);
            pdz.fireFrequency = 20f;*/
            UnityEngine.Object.Destroy(pdz);

            ProjectileOverlapAttack poa = boomerangPrefab.GetComponent<ProjectileOverlapAttack>();
            poa.damageCoefficient = 1f;
            poa.overlapProcCoefficient = procCoefficient;

            ContentAddition.AddProjectile(boomerangPrefab);
        }
    }
    public class BoomerangBehavior : CharacterBody.ItemBehavior
    {
        public static float cooldownDuration = 4;
        float cooldownTimer = 0;
        private void FixedUpdate()
        {
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.deltaTime;
            }
            if (cooldownTimer <= 0)//make this not hardcoded
            {
                RechargeBuff();
            }
        }
        private void RechargeBuff()
        {
            if (body.GetBuffCount(Boomerang.boomerangBuff) <= 3 + stack)
            {
                body.AddBuff(Boomerang.boomerangBuff);
            }
            cooldownTimer = cooldownDuration;
        }
    }
}
