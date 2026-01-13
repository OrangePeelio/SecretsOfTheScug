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
    class RainbowWave
    {
        #region config
        public string ConfigName => "Rainbow Wave";

        public static GameObject rainbowWavePrefab;
        [AutoConfig("Max Rainbow Wave Fly-Out Time", 10f)]
        public static float maxFlyOutTime = 10f; //0.6f
        [AutoConfig("Rainbow Wave Scale Factor", 10f)]
        public static float rainbowWaveScale = 10f; //1.0f
        [AutoConfig("Rainbow Wave Speed", 100f)]
        public static float rainbowWaveSpeed = 100f; //1.0f

        [AutoConfig("Damage Base", 8f)]
        public static float damageBase = 8f;
        [AutoConfig("Damage Stack", 8f)]
        public static float damageStack = 8f;
        [AutoConfig("Proc Coefficient", 1f)]
        public static float procCoefficient = 1f;
        [AutoConfig("Force", 300f)]
        public static float force = 300f;
        #endregion
        public static BuffDef rainbowWaveBuff;
        public static ItemDef rainbowWaveItemDef;

        public static void RainbowWaveInit()
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
                    RainbowWaveBehavior rainbowWaveBehavior = self.AddItemBehavior<RainbowWaveBehavior>(self.inventory.GetItemCount(rainbowWaveItemDef.itemIndex));
                }
            }
        }

        public static void FireRainbowWave(CharacterBody body, int count)
        {
            if (!NetworkServer.active)
                return;
            if (body == null || body.GetBuffCount(rainbowWaveBuff) <= 0)
                return;
            body.RemoveBuff(rainbowWaveBuff);

            Vector3 forward = body.inputBank.aimDirection;
            ProjectileManager.instance.FireProjectile(new FireProjectileInfo
            {
                damage = body.damage * (damageBase + damageStack * count),
                crit = body.RollCrit(),
                damageColorIndex = DamageColorIndex.Item,
                position = body.corePosition/* + Vector3.up * 3*/,
                force = force,
                owner = body.gameObject,
                projectilePrefab = rainbowWavePrefab,
                rotation = RoR2.Util.QuaternionSafeLookRotation(forward),
                speedOverride = rainbowWaveSpeed //20
            });
        }

        private static void CreateItem()
        {

            rainbowWaveItemDef = ScriptableObject.CreateInstance<ItemDef>();
            rainbowWaveItemDef.name = "RAINBOWWAVE";
            rainbowWaveItemDef.nameToken = "RAINBOWWAVE_NAME";
            rainbowWaveItemDef.pickupToken = "RAINBOWWAVE_PICKUP";
            rainbowWaveItemDef.descriptionToken = "RAINBOWWAVE_DESC";
            rainbowWaveItemDef.loreToken = "RAINBOWWAVE_LORE";

#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
            rainbowWaveItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
#pragma warning restore Publicizer001

            rainbowWaveItemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Bear/texBearIcon.png").WaitForCompletion();
            rainbowWaveItemDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bear/PickupBear.prefab").WaitForCompletion();

            rainbowWaveItemDef.tags = new ItemTag[] { ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

            rainbowWaveItemDef.canRemove = true;
            rainbowWaveItemDef.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);

            LanguageAPI.Add(rainbowWaveItemDef.nameToken, "Swinging ball");
            LanguageAPI.Add(rainbowWaveItemDef.pickupToken, "Gain a tethered ball. Swinging the ball damages enemies based on its speed.");
            LanguageAPI.Add(rainbowWaveItemDef.descriptionToken, $"");
            LanguageAPI.Add(rainbowWaveItemDef.loreToken, "");
            ItemAPI.Add(new CustomItem(rainbowWaveItemDef, displayRules));
        }
        private static void CreateBuff()
        {
            rainbowWaveBuff = ScriptableObject.CreateInstance<BuffDef>();
            rainbowWaveBuff.name = "Rainbow wave stock";
            rainbowWaveBuff.buffColor = Color.blue;
            rainbowWaveBuff.canStack = true;
            rainbowWaveBuff.isDebuff = false;
            rainbowWaveBuff.eliteDef = null;
            rainbowWaveBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ElementalRings/texBuffElementalRingsReadyIcon.tif").WaitForCompletion();
            ContentAddition.AddBuffDef(rainbowWaveBuff);
        }
        private static void CreateProjectile()
        {
            rainbowWavePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/FMJRamping.prefab").WaitForCompletion().InstantiateClone("SnailyRainbowWave", true);
            GameObject ghost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/FMJRampingGhost.prefab").WaitForCompletion().InstantiateClone("SnailyRainbowWaveGhost", false);//if this doesnt work and you have to do it the other way:RoR2/Base/Vulture/WindbladeProjectileGhost.prefab


            ProjectileController pc = rainbowWavePrefab.GetComponent<ProjectileController>();
            pc.ghostPrefab = ghost;
            rainbowWavePrefab.transform.localScale = Vector3.one * rainbowWaveScale;//testig :3
            ghost.transform.localScale = Vector3.one * rainbowWaveScale;

            ProjectileSimple ps = rainbowWavePrefab.GetComponent<ProjectileSimple>();
            ps.desiredForwardSpeed = rainbowWaveSpeed;
            ps.lifetime = maxFlyOutTime;

            //ProjectileDamage pd = bp.GetComponent<ProjectileDamage>();
            //pd.damageType |= DamageType.BonusToLowHealth;

            ProjectileDotZone pdz = rainbowWavePrefab.GetComponent<ProjectileDotZone>();
            /*pdz.overlapProcCoefficient = 0.8f;
            pdz.damageCoefficient = 1f;
            pdz.resetFrequency = 1 / (maxFlyOutTime + bp.transitionDuration);
            pdz.fireFrequency = 20f;*/
            UnityEngine.Object.Destroy(pdz);

            ProjectileOverlapAttack poa = rainbowWavePrefab.GetComponent<ProjectileOverlapAttack>();
            poa.damageCoefficient = 1f;
            poa.overlapProcCoefficient = procCoefficient;

            ContentAddition.AddProjectile(rainbowWavePrefab);
        }
    }
    public class RainbowWaveBehavior : CharacterBody.ItemBehavior
    {
        public static float cooldownDuration = 14;
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
            if (body.GetBuffCount(RainbowWave.rainbowWaveBuff) <= 1 + stack)
            {
                body.AddBuff(RainbowWave.rainbowWaveBuff);
            }
            cooldownTimer = cooldownDuration;
        }
    }
}
