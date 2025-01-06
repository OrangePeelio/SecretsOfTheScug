﻿using BepInEx.Configuration;
using SecretsOfTheScug.Modules;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static SecretsOfTheScug.Modules.Language.Styling;

namespace SecretsOfTheScug.Items
{
    class Multitool : ItemBase<Multitool>
    {
        #region config
        public override string ConfigName => "Multitool";

        [AutoConfig("Blood Shrines", true)]
        public static bool shrineBlood = true;
        [AutoConfig("Mountain Shrines", true)]
        public static bool shrineMountain = true;
        [AutoConfig("Chance Shrines", true)]
        public static bool shrineChance = true;
        [AutoConfig("Combat Shrines", false)]
        public static bool shrineCombat = false;
        [AutoConfig("Terminals", "Includes printers and shops.", true)]
        public static bool terminals = true;
        [AutoConfig("Chests", "Includes chests and scavenger bags.", true)]
        public static bool chests = true;
        [AutoConfig("Roulette Chests", true)]
        public static bool roulette = true;
        #endregion

        public override string ItemName => "Multi-tool";

        public override string ItemLangTokenName => "MULTITOOL";

        public override string ItemPickupDesc => "The next interactable triggers an additional time. Regenerates every stage.";

        public override string ItemFullDescription => "Hack the next interactable to trigger ";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.OnStageBeginEffect };

        public override GameObject ItemModel => LoadDropPrefab();

        public override Sprite ItemIcon => LoadItemIcon();

        public override AssetBundle assetBundle => ScugPlugin.mainAssetBundle;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {
            CreateItem();
            On.RoR2.PurchaseInteraction.OnInteractionBegin += MultitoolInteract;

            if (terminals)
                On.RoR2.ShopTerminalBehavior.DropPickup += MultitoolTerminal;
            if (chests)
                On.RoR2.ChestBehavior.ItemDrop += MultitoolChest;
            if (roulette)
                On.RoR2.RouletteChestController.EjectPickupServer += MultitoolRoulette;
            if (shrineMountain)
                On.RoR2.ShrineBossBehavior.AddShrineStack += MultitoolMountain;
            if (shrineBlood)
                On.RoR2.ShrineBloodBehavior.AddShrineStack += MultitoolBlood;
            if (shrineChance)
                IL.RoR2.ShrineChanceBehavior.AddShrineStack += MultitoolChance;
            if (shrineCombat)
                IL.RoR2.ShrineCombatBehavior.AddShrineStack += MultitoolCombat;
        }

        private void MultitoolCombat(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<ShrineCombatBehavior>("get_monsterCredit")
                );
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, ShrineCombatBehavior, float>>((monsterCredit, self) =>
            {
                bool multitoolSuccess = false;
                MultitoolComponent mtc = null;
                if (self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        multitoolSuccess = true;

                        return monsterCredit * 2;
                    }
                }
                return monsterCredit;
            });
        }

        private void MultitoolTerminal(On.RoR2.ShopTerminalBehavior.orig_DropPickup orig, ShopTerminalBehavior self)
        {
            orig(self);

            if (NetworkServer.active)
            {
                MultitoolComponent mtc = null;
                if (self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        PickupDropletController.CreatePickupDroplet(self.pickupIndex, 
                            (self.dropTransform ? self.dropTransform : self.transform).position, 
                            self.transform.TransformVector(self.dropVelocity));
                    }
                }
            }
        }

        private void MultitoolRoulette(On.RoR2.RouletteChestController.orig_EjectPickupServer orig, RouletteChestController self, PickupIndex pickupIndex)
        {
            orig(self, pickupIndex);
            if (pickupIndex == PickupIndex.none)
            {
                return;
            }
            if (NetworkServer.active)
            {
                MultitoolComponent mtc = null;
                if (self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        PickupDropletController.CreatePickupDroplet(pickupIndex, self.ejectionTransform.position, self.ejectionTransform.rotation * (self.localEjectionVelocity + new Vector3(-2, 0, 0)));
                    }
                }
            }
        }

        private void MultitoolChance(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<PickupDropletController>(nameof(PickupDropletController.CreatePickupDroplet))
                );
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<PickupIndex, Vector3, Vector3, ShrineChanceBehavior>>((pickupIndex, dropletOrigin, dropletDirection, self) =>
            {
                bool multitoolSuccess = false;
                MultitoolComponent mtc = null;
                if (self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        multitoolSuccess = true;

                        float angle = 45;
                        Vector3 vector = dropletDirection;
                        Quaternion rotation = Quaternion.AngleAxis(-angle / 2, Vector3.up);
                        vector = rotation * vector;
                        rotation = Quaternion.AngleAxis(angle, Vector3.up);
                        for (int i = 0; i < 2; i++)
                        {
                            PickupDropletController.CreatePickupDroplet(pickupIndex, dropletOrigin, vector);
                            vector = rotation * vector;
                        }
                    }
                }
                if (!multitoolSuccess)
                {
                    PickupDropletController.CreatePickupDroplet(pickupIndex, dropletOrigin, dropletDirection);
                }
            });
        }

        private void MultitoolBlood(On.RoR2.ShrineBloodBehavior.orig_AddShrineStack orig, ShrineBloodBehavior self, Interactor interactor)
        {
            if (NetworkServer.active)
            {
                MultitoolComponent mtc = null;
                if (self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        self.purchaseCount--;
                        self.AddShrineStack(interactor);
                    }
                }
            }

            orig(self, interactor);
        }

        private void MultitoolMountain(On.RoR2.ShrineBossBehavior.orig_AddShrineStack orig, ShrineBossBehavior self, Interactor interactor)
        {
            if (NetworkServer.active)
            {
                MultitoolComponent mtc = null;
                if (self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        self.purchaseCount--;
                        self.AddShrineStack(interactor);
                    }
                }
            }

            orig(self, interactor);
        }

        private void MultitoolChest(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            if (NetworkServer.active)
            {
                MultitoolComponent mtc = null;
                if(self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        self.dropCount += 1;
                    }
                }
            }
            orig(self);
        }

        private void MultitoolInteract(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, RoR2.PurchaseInteraction self, RoR2.Interactor activator)
        {
            if (!self.CanBeAffordedByInteractor(activator))
            {
                return;
            }

            CharacterBody activatorBody = null;
            if(activator.gameObject.TryGetComponent(out activatorBody))
            {
                Inventory inv = activatorBody.inventory;
                int multitoolCount = GetCount(inv);
                if(multitoolCount > 0)
                {
                    MultitoolComponent mtc = self.gameObject.GetComponent<MultitoolComponent>();
                    if (mtc == null)
                        mtc = self.gameObject.AddComponent<MultitoolComponent>();

                    mtc.SetInteractor(activatorBody, inv);
                }
            }

            orig(self, activator);
        }

        public override void Init(ConfigFile config)
        {
            base.Init();
        }
    }

    public class MultitoolComponent : MonoBehaviour
    {
        ItemIndex multitoolIndex => Multitool.instance.ItemsDef.itemIndex;
        ItemIndex multitoolBrokenIndex => MultitoolConsumed.instance.ItemsDef.itemIndex;
        CharacterBody lastInteractor;
        Inventory interactorInventory;

        public void SetInteractor(CharacterBody interactor, Inventory inv = null)
        {
            lastInteractor = interactor;

            if (inv == null)
                interactorInventory = interactor.GetComponent<Inventory>();
            else
                interactorInventory = inv;
        }

        bool MultitoolViable()
        {
            return interactorInventory.GetItemCount(multitoolIndex) > 0;
        }
        void BreakMultitool()
        {
            interactorInventory.RemoveItem(multitoolIndex, 1);
            interactorInventory.GiveItem(multitoolBrokenIndex, 1);

            CharacterMasterNotificationQueue.SendTransformNotification(
                lastInteractor.master, multitoolIndex,
                multitoolBrokenIndex,
                CharacterMasterNotificationQueue.TransformationType.Default);
        }
        public bool VerifyMultitoolAndBreak()
        {
            if (MultitoolViable())
            {
                BreakMultitool();
                DestroyImmediate(this);
                return true;
            }
            return false;
        }
    }
}
