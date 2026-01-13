using HG;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;
using JetBrains.Annotations;

namespace SecretsOfTheScug.Equips
{
    class Mulligan
    {
        public static EquipmentDef mulliganEquipDef;
        public static float cooldown = 120;

        public static void MulliganInit()
        {
            CreateEquipment();
            Hooks();
        }

        static void Hooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
        }
        static void CreateEquipment()
        {
            mulliganEquipDef = ScriptableObject.CreateInstance<EquipmentDef>();
            mulliganEquipDef.name = "MULLIGAN";
            mulliganEquipDef.nameToken = "MULLIGAN_NAME";
            mulliganEquipDef.pickupToken = "MULLIGAN_PICKUP";
            mulliganEquipDef.descriptionToken = "MULLIGAN_DESC";
            mulliganEquipDef.loreToken = "MULLIGAN_LORE";

            mulliganEquipDef.isLunar = true;
            mulliganEquipDef.cooldown = cooldown;
            mulliganEquipDef.enigmaCompatible = true;
            mulliganEquipDef.canBeRandomlyTriggered = false;
            ContentAddition.AddEquipmentDef(mulliganEquipDef);
        }

        protected static bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == mulliganEquipDef)
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }
        protected static bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.teamComponent || !slot.characterBody.master) return false;

            CharacterMaster ownerMaster = slot.characterBody.master;
            CharacterBody body = ownerMaster.GetBody();
            Inventory inventory = body.inventory;
            if (inventory == null)
            {
                return false;
            }
            ReRollItems(inventory);
            return true;
        }

        [Server]
        static void ReRollItems(Inventory inventory)
        {
            List<ItemIndex> uniqueItems;
            using (CollectionPool<ItemIndex, List<ItemIndex>>.RentCollection(out uniqueItems))
            {
                List<ItemIndex> list2;
                using (CollectionPool<ItemIndex, List<ItemIndex>>.RentCollection(out list2))
                {
                    List<ItemIndex> everyItem;
                    using (CollectionPool<ItemIndex, List<ItemIndex>>.RentCollection(out everyItem))
                    {
                        bool flag = false;
                        foreach (ItemTierDef itemTierDef in ItemTierCatalog.allItemTierDefs)
                        {
                            if (itemTierDef.canRestack)
                            {
                                int itemTotal = 0;
                                float temptTotal = 0f;
                                uniqueItems.Clear();
                                list2.Clear();
                                everyItem.Clear();

                                everyItem = GetEveryItemInTier(itemTierDef);                        //i do it :3
                                if (everyItem == null)                                              //i do it :3
                                {                                                                   //i do it :3
                                    Log.Error("item tier " + itemTierDef.tier + " null!");          //i do it :3
                                    break;                                                          //i do it :3
                                }                                                                   //i do it :3
                                inventory.effectiveItemStacks.GetNonZeroIndices(list2);
                                foreach (ItemIndex itemIndex in list2)
                                {                                    
                                    //gets here and poos
                                    inventory.effectiveItemStacks.GetStackValue(itemIndex);
                                    ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                                    if(itemDef.tier != ItemTier.NoTier)
                                    {
                                        Log.Warning("m2");
                                        Log.Debug($"tier = " + itemTierDef.tier);
                                        Log.Warning($"tier = " + itemDef.tier);
                                    }
                                    if (itemTierDef.tier == itemDef.tier && itemDef.DoesNotContainTag(ItemTag.ObjectiveRelated) && itemDef.DoesNotContainTag(ItemTag.PowerShape))
                                    {
                                        Log.Warning("m3");

                                        itemTotal += inventory.GetItemCountPermanent(itemIndex);
                                        temptTotal += (float)inventory.GetItemCountTemp(itemIndex);
                                        uniqueItems.Add(itemIndex);
                                        inventory.ResetItemPermanent(itemIndex);
                                        inventory.ResetItemTemp(itemIndex);
                                    }
                                }
                                if (uniqueItems.Count <= 0)
                                {
                                    Log.Warning("no unieque items");
                                }
                                else
                                {
                                    Log.Warning("m4");
                                    ItemIndex itemIndex2 = RoR2Application.rng.NextElementUniform<ItemIndex>(everyItem);//i do it :3
                                    inventory.GiveItemPermanent(itemIndex2, itemTotal);
                                    inventory.GiveItemTemp(itemIndex2, temptTotal);
                                    flag = true;
                                    Log.Warning("m5");
                                }
                                
                            }
                        }
                        if (flag)
                        {
                            //base.SetDirtyBit(8U);
                        }
                    }
                }
            }
        }
        static List<ItemIndex> GetEveryItemInTier(ItemTierDef itemTierDef)
        {
            List<PickupIndex> list = null;
            switch (itemTierDef.tier)
            {
                case ItemTier.Tier1:
                    list = Run.instance.availableTier1DropList;
                    break;
                case ItemTier.Tier2:
                    list = Run.instance.availableTier2DropList;
                    break;
                case ItemTier.Tier3:
                    list = Run.instance.availableTier3DropList;
                    break;
                case ItemTier.Lunar:
                    list = Run.instance.availableLunarItemDropList;
                    break;
                case ItemTier.Boss:
                    list = Run.instance.availableBossDropList;
                    break;
                case ItemTier.VoidTier1:
                    list = Run.instance.availableVoidTier1DropList;
                    break;
                case ItemTier.VoidTier2:
                    list = Run.instance.availableVoidTier2DropList;
                    break;
                case ItemTier.VoidTier3:
                    list = Run.instance.availableVoidTier3DropList;
                    break;
                case ItemTier.VoidBoss:
                    list = Run.instance.availableVoidBossDropList;
                    break;
                case ItemTier.FoodTier:
                    list = Run.instance.availableFoodTierDropList;
                    break;
            }
            //if (itemDef.ContainsTag(ItemTag.PowerShape))
            //{
            //    list = Run.instance.availablePowerShapeItemsDropList;
            //}
            //if (list != null && itemDef.DoesNotContainTag(ItemTag.WorldUnique))
            //{
            //    list.Add(PickupCatalog.FindPickupIndex(itemIndex));
            //}
            List<ItemIndex> returnValue = new List<ItemIndex>();
            foreach(PickupIndex pickupIndex in list)
            {
                returnValue.Add(pickupIndex.itemIndex);
            }
            Log.Warning($"added {returnValue.Count} items to the list");
            return returnValue;
        }
    }
}
