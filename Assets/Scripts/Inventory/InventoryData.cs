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
    /// 背包物品变化事件
    /// </summary>
    public event Action OnInventoryChanged;
    /// <summary>
    /// 背包容量变化事件
    /// </summary>
    public event Action OnSlotCountChanged;

    // 装备变化事件
    public event Action<EquipmentType, InventoryItem> OnEquipmentChanged;

    public InventoryData() : base(InventoryType.Inventory, InventoryMgr.SLOT_COUNT_DEFAULT)
    {
    }

    public override void SwapItems(int slotIndex1, int slotIndex2)
    {
        base.SwapItems(slotIndex1, slotIndex2);
        OnInventoryChanged?.Invoke();
    }

    public override bool MoveItemInstance(InventoryItem item)
    {
        bool success = base.MoveItemInstance(item);
        if (success)
        {
            OnInventoryChanged?.Invoke();
        }
        return success;
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
    /// 添加物品到背包
    /// </summary>
    public override bool AddItem(string itemId, int amount = 1)
    {
        if (InventoryMgr.GetItemConfig(itemId) == null || amount <= 0)
        {
            Debug.LogError($"物品数据不存在: {itemId}");
            return false;
        }

        ItemConfig itemData = InventoryMgr.GetItemConfig(itemId);
        int remainingAmount = amount;

        // 如果是装备类型且不可堆叠，直接创建新物品
        if (itemData.type == (int)ItemType.Equipment)
        {
            while (remainingAmount > 0 && HasAvailableSlot())
            {
                var newItem = new InventoryItem(itemId, 1);
                items.Add(newItem.instanceId, newItem);

                // 找到第一个可用的插槽索引
                int slotIndex = FindFirstAvailableSlot();
                _itemOrder[slotIndex] = newItem.instanceId;

                OnInventoryChanged?.Invoke();
                remainingAmount--;

                if (remainingAmount <= 0) break;
            }

            return remainingAmount < amount;
        }

        // 尝试堆叠到现有物品上（对于非装备物品）
        foreach (var item in items.Values)
        {
            if (item.itemId == itemId && item.CanAddMore())
            {
                int canAdd = Math.Min(remainingAmount, itemData.stacking - item.GetCount());
                if (canAdd > 0)
                {
                    item.AddCount(canAdd);
                    remainingAmount -= canAdd;

                    if (remainingAmount <= 0)
                        return true;
                }
            }
        }

        // 创建新堆叠
        while (remainingAmount > 0 && HasAvailableSlot())
        {
            int stackAmount = Math.Min(remainingAmount, itemData.stacking);
            if (stackAmount <= 0) break;

            var newItem = new InventoryItem(itemId, stackAmount);
            items.Add(newItem.instanceId, newItem);

            // 找到第一个可用的插槽索引
            int slotIndex = FindFirstAvailableSlot();
            _itemOrder[slotIndex] = newItem.instanceId;

            OnInventoryChanged?.Invoke();
            remainingAmount -= stackAmount;

            if (remainingAmount <= 0) break;
        }

        return remainingAmount < amount;
    }

    /// <summary>
    /// 增加指定实例ID物品的数量
    /// </summary>
    /// <param name="instanceId">物品实例ID</param>
    /// <param name="amount">要增加的数量</param>
    /// <returns>是否增加成功</returns>
    public bool AddItemCountByInstanceId(string instanceId, int amount)
    {
        if (amount <= 0 || !items.TryGetValue(instanceId, out var item))
            return false;

        ItemConfig itemData = InventoryMgr.GetItemConfig(item.itemId);
        int remainingAmount = amount;

        // 先尝试在原有堆叠上增加
        if (item.CanAddMore())
        {
            int canAdd = Math.Min(remainingAmount, itemData.stacking - item.GetCount());
            item.AddCount(canAdd);
            remainingAmount -= canAdd;

            if (remainingAmount <= 0)
                return true;
        }

        // 如果还有剩余数量，创建新的堆叠
        while (remainingAmount > 0 && HasAvailableSlot())
        {
            int stackAmount = Math.Min(remainingAmount, itemData.stacking);
            if (stackAmount <= 0) break;

            var newItem = new InventoryItem(item.itemId, stackAmount);
            items.Add(newItem.instanceId, newItem);
            OnInventoryChanged?.Invoke();
            remainingAmount -= stackAmount;

            if (remainingAmount <= 0) break;
        }

        return remainingAmount < amount;
    }

    /// <summary>
    /// 从背包移除指定物品
    /// </summary>
    public bool RemoveInventoryItem(string itemId, int amount = 1)
    {
        if (amount <= 0) return false;

        int remainingToRemove = amount;
        var itemsToRemove = new List<string>();

        foreach (var pair in items)
        {
            if (pair.Value.itemId == itemId)
            {
                int canRemove = Math.Min(remainingToRemove, pair.Value.GetCount());
                pair.Value.RemoveCount(canRemove);
                remainingToRemove -= canRemove;

                if (pair.Value.GetCount() <= 0)
                {
                    itemsToRemove.Add(pair.Key);
                }

                if (remainingToRemove <= 0)
                    break;
            }
        }

        foreach (var key in itemsToRemove)
        {
            items.Remove(key);
            // 找到并移除对应的插槽索引
            var slotIndex = _itemOrder.FirstOrDefault(x => x.Value == key).Key;
            _itemOrder.Remove(slotIndex);
            OnInventoryChanged?.Invoke();
        }

        return remainingToRemove == 0;
    }

    /// <summary>
    /// 减少指定实例ID物品的数量
    /// </summary>
    /// <param name="instanceId">物品实例ID</param>
    /// <param name="amount">要减少的数量</param>
    /// <returns>是否减少成功</returns>
    public override bool RemoveItemCountByInstanceId(string instanceId, int amount)
    {
        if (amount <= 0 || !items.TryGetValue(instanceId, out var item))
            return false;

        if (amount > item.GetCount())
            return false;

        item.RemoveCount(amount);

        if (item.GetCount() <= 0)
        {
            items.Remove(instanceId);
            OnInventoryChanged?.Invoke();
        }

        return true;
    }

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

    /// <summary>
    /// 使用物品
    /// </summary>
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
            OnInventoryChanged?.Invoke();
        }

        return success;
    }

    /// <summary>
    /// 获取剩余背包容量
    /// </summary>
    public int GetRemainingSlots()
    {
        return capacity - items.Count;
    }

    /// <summary>
    /// 检查玩家背包是否拥有足够数量的指定物品
    /// </summary>
    public bool HasInventoryItem(string itemId, int amount = 1)
    {
        if (amount <= 0) return true;

        int totalCount = 0;
        foreach (var item in items.Values)
        {
            if (item.itemId == itemId)
            {
                totalCount += item.GetCount();
                if (totalCount >= amount)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取玩家背包指定物品数量
    /// </summary>
    public int GetInventoryItemCount(string itemId)
    {
        int totalCount = 0;
        foreach (var item in items.Values)
        {
            if (item.itemId == itemId)
            {
                totalCount += item.GetCount();
            }
        }
        return totalCount;
    }

}
