using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 物品类型
/// </summary>
public enum ItemType
{
    /// <summary>
    /// 材料
    /// </summary>
    Material,
    /// <summary>
    /// 消耗品
    /// </summary>
    Consumable,
    /// <summary>
    /// 装备
    /// </summary>
    Equipment,
}

/// <summary>
/// 装备类型
/// </summary>
public enum EquipmentType
{
    /// <summary>
    /// 无
    /// </summary>
    None,
    /// <summary>
    /// 手持
    /// </summary>
    Handheld,
    /// <summary>
    /// 衣服
    /// </summary>
    Clothes,
    /// <summary>
    /// 饰品
    /// </summary>
    Accessory,
}

[Serializable]
public class InventoryData : BaseInventoryData
{
    /// <summary>
    /// 当前装备的物品
    /// </summary>
    public Dictionary<EquipmentType, InventoryItem> equippedItems = new();

    /// <summary>
    /// 背包容量变化事件
    /// </summary>
    public event Action OnSlotCountChanged;

    // 装备变化事件
    public event Action<EquipmentType, InventoryItem> OnEquipmentChanged;

    public InventoryData() : base(InventoryType.Inventory, InventoryMgr.SLOT_COUNT_DEFAULT)
    {
    }

    /// <summary>
    /// 设置背包容量
    /// </summary>
    /// <param name="count">新容量</param>
    /// <returns>是否设置成功</returns>
    public bool SetSlotCount(int count)
    {
        if (count < 0 || count > InventoryMgr.SLOT_COUNT_MAX)
            return false;

        capacity = count;
        OnSlotCountChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 增加背包容量
    /// </summary>
    /// <param name="amount">增加数量</param>
    /// <returns>是否增加成功</returns>
    public bool AddSlotCount(int amount)
    {
        return SetSlotCount(capacity + amount);
    }

    /// <summary>
    /// 使用物品
    /// </summary>
    /// <param name="character"></param>
    /// <param name="instanceId">物品实例ID</param>
    /// <param name="count">使用数量</param>
    /// <returns>是否使用成功</returns>
    public bool UseItem(CharacterData character, string instanceId, int count = 1)
    {
        if (!items.TryGetValue(instanceId, out var item))
            return false;

        var itemData = item.GetItemData();
        if (itemData == null)
            return false;

        bool success = false;

        switch ((ItemType)itemData.type)
        {
            case ItemType.Consumable:
                success = Use(character, instanceId, count);
                break;

            case ItemType.Equipment:
                if (item.isEquipped)
                {
                    success = UnequipItem(character, instanceId);
                }
                else
                {
                    success = EquipItem(character, instanceId);
                }
                break;

            case ItemType.Material:
                // 这些类型的物品不能直接使用
                return false;
        }

        // 如果物品数量变为0，则从背包移除
        if (item.GetCount() <= 0)
        {
            items.Remove(instanceId);
            RaiseInventoryChanged();
        }

        return success;
    }
    
    /// <summary>
    /// 使用可消耗物品
    /// </summary>
    public bool Use(CharacterData character, string instanceId, int count)
    {
        if (!items.TryGetValue(instanceId, out var item))
            return false;

        var itemData = item.GetItemData();
        if (itemData == null || itemData.type != (int)ItemType.Consumable)
            return false;

        // 检查物品是否已损坏
        if (item.IsBroken())
        {
            Debug.Log("无法使用已损坏的物品");
            return false;
        }

        ApplyConsumable(character, itemData, count);

        item.RemoveCount(count);

        return true;
    }
    
    /// <summary>
    /// 应用可消耗物品效果
    /// </summary>
    private void ApplyConsumable(CharacterData character, ItemConfig item, int count)
    {
        // TODO: 应用物品效果
        Debug.Log($"使用物品: {item.id} -> {item.name} x {count}");

        if (item.healthAdjust != 0)
        {
            character.SetHealth(character.health + item.healthAdjust * count);
        }
        if (item.hungerAdjust != 0)
        {
            character.SetHunger(character.hunger + item.hungerAdjust * count);
        }
        if (item.energyAdjust != 0)
        {
            character.SetEnergy(character.energy + item.energyAdjust * count);
        }
        if (item.spiritAdjust != 0)
        {
            character.SetSpirit(character.spirit + item.spiritAdjust * count);
        }

        if (item.getBuff != null && item.getBuff.Length > 0)
        {
            foreach (var buffId in item.getBuff)
            {
                if (buffId > 0 && BuffMgr.GetBuffData(buffId.ToString()) != null)
                {
                    character.AddBuff(buffId.ToString());
                }
            }
        }

        if (item.removeBuff != null && item.removeBuff.Length > 0)
        {
            foreach (var buffId in item.removeBuff)
            {
                if (buffId > 0 && BuffMgr.GetBuffData(buffId.ToString()) != null)
                {
                    character.RemoveBuff(buffId.ToString());
                }
            }
        }
    }

    /// <summary>
    /// 装备物品
    /// </summary>
    /// <param name="character"></param>
    /// <param name="instanceId">物品实例ID</param>
    /// <returns>是否装备成功</returns>
    public bool EquipItem(CharacterData character, string instanceId)
    {
        if (!items.TryGetValue(instanceId, out var item))
            return false;

        var itemData = item.GetItemData();
        if (itemData == null || itemData.type != (int)ItemType.Equipment)
            return false;

        // 检查物品是否已损坏
        if (item.IsBroken())
        {
            Debug.Log("无法装备已损坏的物品");
            return false;
        }

        // 如果该装备槽已有装备，则先卸下
        if (equippedItems.TryGetValue((EquipmentType)itemData.equipmentParts, out var equippedItem))
        {
            UnequipItem(character, equippedItem.instanceId);
        }

        // 装备新物品
        equippedItems[(EquipmentType)itemData.equipmentParts] = item;
        item.isEquipped = true;

        ApplyEquipment(character, itemData);

        OnEquipmentChanged?.Invoke((EquipmentType)itemData.equipmentParts, item);

        return true;
    }

    /// <summary>
    /// 卸下装备
    /// </summary>
    /// <param name="character"></param>
    /// <param name="instanceId">物品实例ID</param>
    /// <returns>是否卸下成功</returns>
    public bool UnequipItem(CharacterData character, string instanceId)
    {
        if (!items.TryGetValue(instanceId, out var item))
            return false;

        var itemData = item.GetItemData();
        if (itemData == null || itemData.type != (int)ItemType.Equipment)
            return false;

        // 确保该物品确实被装备了
        if (!equippedItems.TryGetValue((EquipmentType)itemData.equipmentParts, out var equippedItem)
            || equippedItem.instanceId != instanceId)
        {
            return false;
        }

        // 卸下装备
        equippedItems.Remove((EquipmentType)itemData.equipmentParts);
        item.isEquipped = false;

        RemoveEquipment(character, itemData);

        OnEquipmentChanged?.Invoke((EquipmentType)itemData.equipmentParts, null);

        return true;
    }

    /// <summary>
    /// 应用装备效果
    /// </summary>
    private void ApplyEquipment(CharacterData character, ItemConfig item)
    {
        // 应用装备效果
        Debug.Log($"装备物品: {item.id} -> {item.name}");

        if (item.getBuff != null && item.getBuff.Length > 0)
        {
            foreach (var buffId in item.getBuff)
            {
                if (buffId > 0 && BuffMgr.GetBuffData(buffId.ToString()) != null)
                {
                    character.AddBuff(buffId.ToString());
                }
            }
        }
    }

    /// <summary>
    /// 移除装备效果
    /// </summary>
    private void RemoveEquipment(CharacterData character, ItemConfig item)
    {
        // 移除装备效果
        Debug.Log($"卸下物品: {item.id} -> {item.name}");

        if (item.getBuff != null && item.getBuff.Length > 0)
        {
            foreach (var buffId in item.getBuff)
            {
                if (buffId > 0 && BuffMgr.GetBuffData(buffId.ToString()) != null)
                {
                    character.RemoveBuff(buffId.ToString());
                }
            }
        }
    }

    /// <summary>
    /// 获取已装备的物品
    /// </summary>
    /// <param name="equipType">装备类型</param>
    /// <returns>已装备物品</returns>
    public InventoryItem GetEquippedItem(EquipmentType equipType)
    {
        equippedItems.TryGetValue(equipType, out var item);
        return item;
    }


}
