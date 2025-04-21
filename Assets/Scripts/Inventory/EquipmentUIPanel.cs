using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentUIPanel : MonoBehaviour
{
    public EquipmentSlot handheld;
    public EquipmentSlot clothes;
    public EquipmentSlot accessory;

    void Awake()
    {
        InventoryMgr.GetPlayerInventoryData().OnEquipmentChanged += UpdateEquipmentUI;
        handheld.OnEquipmentSlotRightClicked += UpdateHandheldSlotRightClicked;
        clothes.OnEquipmentSlotRightClicked += UpdateClothesRightClicked;
        accessory.OnEquipmentSlotRightClicked += UpdateAccessoryRightClicked;
    }

    private void UpdateEquipmentUI(EquipmentType type, InventoryItem item)
    {
        switch (type)
        {
            case EquipmentType.Handheld:
                handheld.Setup(item);
                break;
            case EquipmentType.Clothes:
                clothes.Setup(item);
                break;
            case EquipmentType.Accessory:
                accessory.Setup(item);
                break;
        }
    }

    private void UpdateHandheldSlotRightClicked(InventoryItem item)
    {
        if (item.GetItemType() == ItemType.Equipment)
        {
            if (item.isEquipped)
            {
                // 卸下装备
                InventoryMgr.GetPlayerInventoryData().UnequipItem(CharacterMgr.Player(), item.instanceId);
            }
        }
    }

    private void UpdateClothesRightClicked(InventoryItem item)
    {
        if (item.GetItemType() == ItemType.Equipment)
        {
            if (item.isEquipped)
            {
                // 卸下装备
                InventoryMgr.GetPlayerInventoryData().UnequipItem(CharacterMgr.Player(), item.instanceId);
            }
        }
    }

    private void UpdateAccessoryRightClicked(InventoryItem item)
    {
        if (item.GetItemType() == ItemType.Equipment)
        {
            if (item.isEquipped)
            {
                // 卸下装备
                InventoryMgr.GetPlayerInventoryData().UnequipItem(CharacterMgr.Player(), item.instanceId);
            }
        }
    }

    private void OnDestroy()
    {
        InventoryMgr.GetPlayerInventoryData().OnEquipmentChanged -= UpdateEquipmentUI;
        handheld.OnEquipmentSlotRightClicked -= UpdateHandheldSlotRightClicked;
        clothes.OnEquipmentSlotRightClicked -= UpdateClothesRightClicked;
        accessory.OnEquipmentSlotRightClicked -= UpdateAccessoryRightClicked;
    }
}
