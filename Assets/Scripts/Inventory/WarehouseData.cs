using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 仓库类型
/// </summary>
[Serializable]
public enum WarehouseType
{
    /// <summary>
    /// 木箱
    /// </summary>
    Box,
    /// <summary>
    /// 冰箱
    /// </summary>
    IceBox
}

[Serializable]
public class WarehouseData : BaseInventoryData
{
    public string buildingInstanceId;
    public WarehouseType warehouseType;

    /// <summary>
    /// 仓库物品变化事件
    /// </summary>
    public event Action OnWarehouseChanged;

    public WarehouseData() : base()
    {
    }

    public WarehouseData(string buildingId, WarehouseType warehouseType, int capacity)
        : base(InventoryType.Warehouse, capacity)
    {
        this.buildingInstanceId = buildingId;
        this.warehouseType = warehouseType;
    }

    public override void SwapItems(int slotIndex1, int slotIndex2)
    {
        base.SwapItems(slotIndex1, slotIndex2);
        OnWarehouseChanged?.Invoke();
    }

    public override bool MoveItemInstance(InventoryItem item)
    {
        bool success = base.MoveItemInstance(item);
        if (success)
        {
            OnWarehouseChanged?.Invoke();
        }
        return success;
    }

    /// <summary>
    /// 添加物品到仓库
    /// </summary>
    public override bool AddItem(string itemId, int amount = 1)
    {
        ItemConfig itemData = InventoryMgr.GetItemConfig(itemId);

        if (itemData == null || amount <= 0)
        {
            Debug.LogError($"物品数据不存在: {itemId}");
            return false;
        }

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

                OnWarehouseChanged?.Invoke();
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

            OnWarehouseChanged?.Invoke();
            remainingAmount -= stackAmount;

            if (remainingAmount <= 0) break;
        }

        return remainingAmount < amount;
    }

    /// <summary>
    /// 减少指定实例ID物品的数量
    /// </summary>
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
            OnWarehouseChanged?.Invoke();
        }

        return true;
    }

    /// <summary>
    /// 获取剩余容量
    /// </summary>
    public int GetRemainingSlots()
    {
        return capacity - items.Count;
    }

    /// <summary>
    /// 检查玩家是否拥有足够数量的指定物品
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
    /// 获取指定物品数量
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
