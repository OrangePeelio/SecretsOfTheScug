using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SecretsOfTheScug.Items
{
    class Peashooter
    {
        #region config
        public static ItemDef peashooterItemDef;
        public static GameObject peashooterPrefab;
        [AutoConfig("Peashooter Lifetime", 5f)]
        public static float lifetime = 5f; //0.6f
        [AutoConfig("Peashooter Scale Factor", 0.3f)]
        public static float peashooterScale = 0.3f; //1.0f
        [AutoConfig("Peashooter Speed", 100f)]
        public static float peashooterSpeed = 100f; //1.0f

        [AutoConfig("Damage Base", .4f)]
        public static float damageBase = .4f;
        [AutoConfig("Damage Stack", .4f)]
        public static float damageStack = .4f;
        [AutoConfig("Proc Coefficient", 0.1f)]
        public static float procCoefficient = 0.1f;
        [AutoConfig("Force", 0f)]
        public static float force = 0f;
        #endregion

        public static void PeashooterInit()
        {
            CreateItem();
            CreateProjectile();
        }


        public static void FirePeashooter(CharacterBody body, int count)
        {
            if (body == null)
                return;
            Vector3 forward = body.inputBank.aimDirection;
            ProjectileManager.instance.FireProjectile(new FireProjectileInfo
            {
                damage = body.damage * (damageBase + damageStack * count),
                crit = body.RollCrit(),
                damageColorIndex = DamageColorIndex.Item,
                position = body.corePosition,
                force = force,
                owner = body.gameObject,
                projectilePrefab = peashooterPrefab,
                rotation = RoR2.Util.QuaternionSafeLookRotation(forward),
                speedOverride = peashooterSpeed,
                damageTypeOverride = DamageTypeCombo.GenericPrimary
            });
        }

        public static void CreateItem()
        {

            peashooterItemDef = ScriptableObject.CreateInstance<ItemDef>();
            peashooterItemDef.name = "PEASHOOTER";
            peashooterItemDef.nameToken = "PEASHOOTER_NAME";
            peashooterItemDef.pickupToken = "PEASHOOTER_PICKUP";
            peashooterItemDef.descriptionToken = "PEASHOOTER_DESC";
            peashooterItemDef.loreToken = "PEASHOOTER_LORE";

#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
            peashooterItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
#pragma warning restore Publicizer001

            peashooterItemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Bear/texBearIcon.png").WaitForCompletion();
            peashooterItemDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bear/PickupBear.prefab").WaitForCompletion();

            peashooterItemDef.tags = new ItemTag[] { ItemTag.Damage };

            peashooterItemDef.canRemove = true;
            peashooterItemDef.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);

            LanguageAPI.Add(peashooterItemDef.nameToken, "Swinging ball");
            LanguageAPI.Add(peashooterItemDef.pickupToken, "Gain a tethered ball. Swinging the ball damages enemies based on its speed.");
            LanguageAPI.Add(peashooterItemDef.descriptionToken, $"");
            LanguageAPI.Add(peashooterItemDef.loreToken, "");
            ItemAPI.Add(new CustomItem(peashooterItemDef, displayRules));
        }
        private static void CreateProjectile()
        {
            peashooterPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarExploder/LunarExploderShardProjectile.prefab").WaitForCompletion().InstantiateClone("SnailyPeashooter", true);
            GameObject ghost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarExploder/LunarExploderShardGhost.prefab").WaitForCompletion().InstantiateClone("SnailyPeashooterGhost", false);
            peashooterPrefab.transform.localScale = Vector3.one * peashooterScale;

            ProjectileSimple ps = peashooterPrefab.GetComponent<ProjectileSimple>();
            ps.desiredForwardSpeed = peashooterSpeed;
            ps.lifetime = lifetime;

            ProjectileController pc = ps.GetComponent<ProjectileController>();
            pc.ghostPrefab = ghost;

            ContentAddition.AddProjectile(peashooterPrefab);
        }
    }
}
