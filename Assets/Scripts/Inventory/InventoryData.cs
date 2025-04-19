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
public class InventoryData
{
    private int _currentSlotCount = InventoryMgr.SLOT_COUNT_DEFAULT;
    public int CurrentSlotCount
    {
        get => _currentSlotCount;
        set
        {
            if (_currentSlotCount != value)
            {
                _currentSlotCount = value;
                OnSlotCountChanged?.Invoke();
            }
        }
    }

    public Dictionary<string, InventoryItem> Items { get; private set; } = new();

    /// <summary>
    /// 背包物品变化事件
    /// </summary>
    public event Action OnInventoryChanged;
    /// <summary>
    /// 背包容量变化事件
    /// </summary>
    public event Action OnSlotCountChanged;

    // 当前装备的物品
    private Dictionary<EquipmentType, InventoryItem> _equippedItems = new();
    public IReadOnlyDictionary<EquipmentType, InventoryItem> EquippedItems => _equippedItems;

    // 新增装备变化事件
    public event Action<EquipmentType, InventoryItem> OnEquipmentChanged;


    /// <summary>
    /// 设置背包容量
    /// </summary>
    /// <param name="count">新容量</param>
    /// <returns>是否设置成功</returns>
    public bool SetSlotCount(int count)
    {
        if (count < 0 || count > InventoryMgr.SLOT_COUNT_MAX)
            return false;

        CurrentSlotCount = count;
        return true;
    }

    /// <summary>
    /// 增加背包容量
    /// </summary>
    /// <param name="amount">增加数量</param>
    /// <returns>是否增加成功</returns>
    public bool AddSlotCount(int amount)
    {
        if (amount <= 0) return false;

        int newCount = CurrentSlotCount + amount;
        if (newCount > InventoryMgr.SLOT_COUNT_MAX)
            return false;

        CurrentSlotCount = newCount;
        return true;
    }

    /// <summary>
    /// 添加物品到背包
    /// </summary>
    public bool AddInventoryItem(string itemId, int amount = 1)
    {
        if (InventoryMgr.GetItemData(itemId) == null || amount <= 0)
        {
            Debug.LogError($"物品数据不存在: {itemId}");
            return false;
        }

        ItemConfig itemData = InventoryMgr.GetItemData(itemId);
        int remainingAmount = amount;

        // 如果是装备类型且不可堆叠，直接创建新物品
        if (itemData.type == (int)ItemType.Equipment)
        {
            while (remainingAmount > 0 && HasAvailableSlot())
            {
                var newItem = new InventoryItem(itemId, 1);
                Items.Add(newItem.instanceId, newItem);
                OnInventoryChanged?.Invoke();
                remainingAmount--;

                if (remainingAmount <= 0) break;
            }

            return remainingAmount < amount;
        }

        // 尝试堆叠到现有物品上（对于非装备物品）
        foreach (var item in Items.Values)
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
            Items.Add(newItem.instanceId, newItem);
            OnInventoryChanged?.Invoke(); // 只在添加新物品时触发
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
        if (amount <= 0 || !Items.TryGetValue(instanceId, out var item))
            return false;

        ItemConfig itemData = InventoryMgr.GetItemData(item.itemId);
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
            Items.Add(newItem.instanceId, newItem);
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

        foreach (var pair in Items)
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
            Items.Remove(key);
            OnInventoryChanged?.Invoke(); // 只在移除物品时触发
        }

        return remainingToRemove == 0;
    }

    /// <summary>
    /// 减少指定实例ID物品的数量
    /// </summary>
    /// <param name="instanceId">物品实例ID</param>
    /// <param name="amount">要减少的数量</param>
    /// <returns>是否减少成功</returns>
    public bool RemoveItemCountByInstanceId(string instanceId, int amount)
    {
        if (amount <= 0 || !Items.TryGetValue(instanceId, out var item))
            return false;

        if (amount > item.GetCount())
            return false;

        item.RemoveCount(amount);

        if (item.GetCount() <= 0)
        {
            Items.Remove(instanceId);
            OnInventoryChanged?.Invoke();
        }

        return true;
    }

    public bool Use(string instanceId, int count)
    {
        if (!Items.TryGetValue(instanceId, out var item))
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

        item.RemoveCount(count);

        // TODO: 应用物品效果
        Debug.Log($"使用物品: {item.itemId} -> {itemData.name} x {count}");

        return true;
    }

    /// <summary>
    /// 装备物品
    /// </summary>
    /// <param name="instanceId">物品实例ID</param>
    /// <returns>是否装备成功</returns>
    public bool EquipItem(string instanceId)
    {
        if (!Items.TryGetValue(instanceId, out var item))
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
        if (_equippedItems.TryGetValue((EquipmentType)itemData.equipmentParts, out var equippedItem))
        {
            UnequipItem(equippedItem.instanceId);
        }

        // 装备新物品
        _equippedItems[(EquipmentType)itemData.equipmentParts] = item;
        item.isEquipped = true;

        // TODO: 应用装备效果
        Debug.Log($"装备物品: {item.itemId} -> {itemData.name}");

        OnEquipmentChanged?.Invoke((EquipmentType)itemData.equipmentParts, item);

        return true;
    }

    /// <summary>
    /// 卸下装备
    /// </summary>
    /// <param name="instanceId">物品实例ID</param>
    /// <returns>是否卸下成功</returns>
    public bool UnequipItem(string instanceId)
    {
        if (!Items.TryGetValue(instanceId, out var item))
            return false;

        var itemData = item.GetItemData();
        if (itemData == null || itemData.type != (int)ItemType.Equipment)
            return false;

        // 确保该物品确实被装备了
        if (!_equippedItems.TryGetValue((EquipmentType)itemData.equipmentParts, out var equippedItem)
            || equippedItem.instanceId != instanceId)
        {
            return false;
        }

        // 卸下装备
        _equippedItems.Remove((EquipmentType)itemData.equipmentParts);
        item.isEquipped = false;

        // TODO: 移除装备效果
        Debug.Log($"卸下物品: {item.itemId} -> {itemData.name}");

        OnEquipmentChanged?.Invoke((EquipmentType)itemData.equipmentParts, null);

        return true;
    }

    /// <summary>
    /// 获取已装备的物品
    /// </summary>
    /// <param name="equipType">装备类型</param>
    /// <returns>已装备物品</returns>
    public InventoryItem GetEquippedItem(EquipmentType equipType)
    {
        _equippedItems.TryGetValue(equipType, out var item);
        return item;
    }

    /// <summary>
    /// 使用物品
    /// </summary>
    /// <param name="instanceId">物品实例ID</param>
    /// <param name="count">使用数量</param>
    /// <returns>是否使用成功</returns>
    public bool UseItem(string instanceId, int count = 1)
    {
        if (!Items.TryGetValue(instanceId, out var item))
            return false;

        var itemData = item.GetItemData();
        if (itemData == null)
            return false;

        bool success = false;

        switch ((ItemType)itemData.type)
        {
            case ItemType.Consumable:
                success = Use(instanceId, count);
                break;

            case ItemType.Equipment:
                if (item.isEquipped)
                {
                    success = UnequipItem(instanceId);
                }
                else
                {
                    success = EquipItem(instanceId);
                }
                break;

            case ItemType.Material:
                // 这些类型的物品不能直接使用
                return false;
        }

        // 如果物品数量变为0，则从背包移除
        if (item.GetCount() <= 0)
        {
            Items.Remove(instanceId);
            OnInventoryChanged?.Invoke();
        }

        return success;
    }

    /// <summary>
    /// 检查是否有可用背包容量
    /// </summary>
    public bool HasAvailableSlot()
    {
        return Items.Count < CurrentSlotCount;
    }

    /// <summary>
    /// 获取剩余背包容量
    /// </summary>
    public int GetRemainingSlots()
    {
        return CurrentSlotCount - Items.Count;
    }

    /// <summary>
    /// 检查玩家背包是否拥有足够数量的指定物品
    /// </summary>
    public bool HasInventoryItem(string itemId, int amount = 1)
    {
        if (amount <= 0) return true;

        int totalCount = 0;
        foreach (var item in Items.Values)
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
        foreach (var item in Items.Values)
        {
            if (item.itemId == itemId)
            {
                totalCount += item.GetCount();
            }
        }
        return totalCount;
    }
}