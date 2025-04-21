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
    public string inventoryId = System.Guid.NewGuid().ToString("N");  // 容器ID
    public int capacity;
    public InventoryType inventoryType;  // 容器类型
    public Dictionary<string, InventoryItem> items = new();

    // 字典，用于维护物品槽位顺序
    protected Dictionary<int, string> _itemOrder = new();

    public BaseInventoryData() { }

    public BaseInventoryData(InventoryType inventoryType, int capacity)
    {
        this.capacity = capacity;
        this.inventoryType = inventoryType;
    }

    public abstract bool AddItem(string itemId, int count);
    public abstract bool RemoveItemCountByInstanceId(string instanceId, int count);

    public virtual bool HasAvailableSlot()
    {
        return items.Count < capacity;
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
            var slotIndex = _itemOrder.FirstOrDefault(x => x.Value == instanceId).Key;
            _itemOrder.Remove(slotIndex);

            return item;
        }
        return null;
    }

    /// <summary>
    /// 查找第一个可用的插槽索引
    /// </summary>
    protected int FindFirstAvailableSlot()
    {
        int slotIndex = 0;
        while (_itemOrder.ContainsKey(slotIndex))
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
        if (!_itemOrder.ContainsKey(slotIndex))
            return null;

        string instanceId = _itemOrder[slotIndex];
        return items.TryGetValue(instanceId, out var item) ? item : null;
    }

    /// <summary>
    /// 交换物品位置
    /// </summary>
    public virtual void SwapItems(int slotIndex1, int slotIndex2)
    {
        if (!_itemOrder.ContainsKey(slotIndex1) || !_itemOrder.ContainsKey(slotIndex2))
            return;

        // 交换字典中的实例ID
        string temp = _itemOrder[slotIndex1];
        _itemOrder[slotIndex1] = _itemOrder[slotIndex2];
        _itemOrder[slotIndex2] = temp;
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
            _itemOrder.Remove(slotIndex);
        }
        else
        {
            _itemOrder[slotIndex] = item.instanceId;
        }
    }

}
