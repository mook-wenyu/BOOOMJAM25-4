using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryData
{
    private int _currentSlotCount;
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

    public event Action OnInventoryChanged;
    public event Action OnSlotCountChanged;


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
    /// 添加物品到玩家背包
    /// </summary>
    public bool AddInventoryItem(string itemId, int amount = 1)
    {
        if (InventoryMgr.GetItemData(itemId) == null || amount <= 0)
        {
            Debug.LogError($"物品数据不存在: {itemId}");
            return false;
        }

        ItemData itemData = InventoryMgr.GetItemData(itemId);
        int remainingAmount = amount;

        // 尝试堆叠到现有物品上
        foreach (var item in Items.Values)
        {
            if (item.itemId == itemId && item.CanAddMore())
            {
                int canAdd = Math.Min(remainingAmount, itemData.maxStack - item.Count);
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
            int stackAmount = Math.Min(remainingAmount, itemData.maxStack);
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

        ItemData itemData = InventoryMgr.GetItemData(item.itemId);
        int remainingAmount = amount;

        // 先尝试在原有堆叠上增加
        if (item.CanAddMore())
        {
            int canAdd = Math.Min(remainingAmount, itemData.maxStack - item.Count);
            item.AddCount(canAdd);
            remainingAmount -= canAdd;

            if (remainingAmount <= 0)
                return true;
        }

        // 如果还有剩余数量，创建新的堆叠
        while (remainingAmount > 0 && HasAvailableSlot())
        {
            int stackAmount = Math.Min(remainingAmount, itemData.maxStack);
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
    /// 从玩家背包移除指定物品
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
                int canRemove = Math.Min(remainingToRemove, pair.Value.Count);
                pair.Value.RemoveCount(canRemove);
                remainingToRemove -= canRemove;

                if (pair.Value.Count <= 0)
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

        if (amount > item.Count)
            return false;

        item.RemoveCount(amount);

        if (item.Count <= 0)
        {
            Items.Remove(instanceId);
            OnInventoryChanged?.Invoke();
        }

        return true;
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
        int totalCount = 0;
        foreach (var item in Items.Values)
        {
            if (item.itemId == itemId)
            {
                totalCount += item.Count;
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
                totalCount += item.Count;
            }
        }
        return totalCount;
    }

}