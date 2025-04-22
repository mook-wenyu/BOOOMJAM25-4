using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 物品容器类型
/// </summary>
[Serializable]
public enum InventoryType
{
    /// <summary>
    /// 背包
    /// </summary>
    Inventory,
    /// <summary>
    /// 仓库
    /// </summary>
    Warehouse,
    /// <summary>
    /// 装备
    /// </summary>
    Equipment
}

[Serializable]
public abstract class BaseInventoryData
{
    public string inventoryId = System.Guid.NewGuid().ToString("N"); // 容器ID
    public int capacity;
    public InventoryType inventoryType; // 容器类型
    public Dictionary<string, InventoryItem> items = new();

    // 字典，用于维护物品槽位顺序
    public Dictionary<int, string> itemOrder = new();

    /// <summary>
    /// 物品变化事件
    /// </summary>
    public event Action OnInventoryChanged;

    protected virtual void RaiseInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    protected BaseInventoryData() { }

    protected BaseInventoryData(InventoryType inventoryType, int capacity)
    {
        this.capacity = capacity;
        this.inventoryType = inventoryType;
    }
    
    /// <summary>
    /// 获取容器类型
    /// </summary>
    public virtual InventoryType GetInventoryType()
    {
        return inventoryType;
    }

    /// <summary>
    /// 添加物品
    /// </summary>
    public virtual bool AddItem(string itemId, int amount = 1)
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
                itemOrder[slotIndex] = newItem.instanceId;

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
            itemOrder[slotIndex] = newItem.instanceId;

            OnInventoryChanged?.Invoke();
            remainingAmount -= stackAmount;

            if (remainingAmount <= 0) break;
        }

        return remainingAmount < amount;
    }

    /// <summary>
    /// 增加指定实例ID物品的数量
    /// </summary>
    public virtual bool AddItemCountByInstanceId(string instanceId, int amount)
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

            // 找到第一个可用的插槽索引并设置
            int slotIndex = FindFirstAvailableSlot();
            itemOrder[slotIndex] = newItem.instanceId;

            OnInventoryChanged?.Invoke();
            remainingAmount -= stackAmount;

            if (remainingAmount <= 0) break;
        }

        return remainingAmount < amount;
    }

    /// <summary>
    /// 从容器移除指定物品
    /// </summary>
    public virtual bool RemoveItem(string itemId, int amount = 1)
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
            var slotIndex = itemOrder.FirstOrDefault(x => x.Value == key).Key;
            itemOrder.Remove(slotIndex);
            OnInventoryChanged?.Invoke();
        }

        return remainingToRemove == 0;
    }

    /// <summary>
    /// 减少指定实例ID物品的数量
    /// </summary>
    public virtual bool RemoveItemCountByInstanceId(string instanceId, int amount)
    {
        if (amount <= 0 || !items.TryGetValue(instanceId, out var item))
            return false;

        if (amount > item.GetCount())
            return false;

        item.RemoveCount(amount);

        if (item.GetCount() <= 0)
        {
            items.Remove(instanceId);
            
            // 找到并移除对应的插槽索引
            var slotIndex = itemOrder.FirstOrDefault(x => x.Value == instanceId).Key;
            itemOrder.Remove(slotIndex);
            
            OnInventoryChanged?.Invoke();
        }

        return true;
    }

    /// <summary>
    /// 检查是否有可用的插槽
    /// </summary>
    public virtual bool HasAvailableSlot()
    {
        return items.Count < capacity;
    }

    /// <summary>
    /// 获取可用的插槽数量
    /// </summary>
    public virtual int GetAvailableSlotCount()
    {
        return capacity - items.Count;
    }
    
    /// <summary>
    /// 获取指定物品数量
    /// </summary>
    public virtual int GetItemCount(string itemId)
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
    
    /// <summary>
    /// 检查是否有足够的物品数量
    /// </summary>
    public virtual bool HasItemCount(string itemId, int amount = 1)
    {
        return GetItemCount(itemId) >= amount;
    }

    /// <summary>
    /// 移动物品实例到此容器
    /// </summary>
    public virtual bool MoveItemInstance(InventoryItem item)
    {
        if (item == null || !HasAvailableSlot()) return false;

        // 检查物品配置是否存在
        if (InventoryMgr.GetItemConfig(item.itemId) == null)
        {
            Debug.LogError($"物品数据不存在: {item.itemId}");
            return false;
        }

        // 添加物品实例
        items[item.instanceId] = item;

        return true;
    }

    /// <summary>
    /// 移除物品实例
    /// </summary>
    public virtual InventoryItem RemoveItemInstance(string instanceId)
    {
        if (items.TryGetValue(instanceId, out var item))
        {
            items.Remove(instanceId);

            // 找到并移除对应的插槽索引
            int slotIndex = itemOrder.FirstOrDefault(x => x.Value == instanceId).Key;
            itemOrder.Remove(slotIndex);

            OnInventoryChanged?.Invoke();
            return item;
        }
        return null;
    }

    /// <summary>
    /// 查找第一个可用的插槽索引
    /// </summary>
    public virtual int FindFirstAvailableSlot()
    {
        int slotIndex = 0;
        while (itemOrder.ContainsKey(slotIndex))
        {
            slotIndex++;
        }
        return slotIndex;
    }

    /// <summary>
    /// 获取指定位置的物品
    /// </summary>
    public virtual InventoryItem GetItemAtSlot(int slotIndex)
    {
        if (!itemOrder.ContainsKey(slotIndex))
            return null;

        string instanceId = itemOrder[slotIndex];
        return items.TryGetValue(instanceId, out var item) ? item : null;
    }

    /// <summary>
    /// 设置指定槽位的物品
    /// </summary>
    public virtual void SetSlot(int slotIndex, InventoryItem item)
    {
        if (slotIndex < 0 || slotIndex >= capacity) return;

        // 更新槽位映射
        if (item == null)
        {
            itemOrder.Remove(slotIndex);
        }
        else
        {
            itemOrder[slotIndex] = item.instanceId;
        }

        OnInventoryChanged?.Invoke();
    }
    
}
