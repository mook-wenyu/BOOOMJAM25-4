using UnityEngine;
using System;

public class InventoryItem
{
    public string instanceId;  // 物品实例ID
    public string itemId;      // 物品ID
    
    private int _count;          // 当前数量
    public int Count
    {
        get => _count;
        private set
        {
            if (_count != value)
            {
                _count = value;
                OnCountChanged?.Invoke(this);
            }
        }
    }

    private ItemData _cachedItemData;     // 缓存的物品数据

    // 添加数量变化事件
    public event Action<InventoryItem> OnCountChanged;

    public InventoryItem(string itemId, int count = 1)
    {
        this.instanceId = System.Guid.NewGuid().ToString("N");
        this.itemId = itemId;

        _cachedItemData = InventoryMgr.GetItemData(itemId);
        if (_cachedItemData != null)
        {
            this.Count = Mathf.Min(count, _cachedItemData.maxStack);
        }
        else
        {
            this.Count = count;
            Debug.LogWarning($"创建物品实例时未找到物品数据: {itemId}");
        }
    }

    /// <summary>
    /// 是否可以添加更多
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool CanAddMore(int amount = 1)
    {
        if (_cachedItemData == null) return false;
        return Count + amount <= _cachedItemData.maxStack;
    }

    /// <summary>
    /// 添加数量
    /// </summary>
    /// <param name="amount"></param>
    public void AddCount(int amount = 1)
    {
        if (_cachedItemData != null)
        {
            Count = Mathf.Min(Count + amount, _cachedItemData.maxStack);
        }
        else
        {
            Count += amount;
        }
    }

    /// <summary>
    /// 移除数量
    /// </summary>
    /// <param name="amount"></param>
    public void RemoveCount(int amount = 1)
    {
        Count = Mathf.Max(Count - amount, 0);
    }

    /// <summary>
    /// 获取缓存的物品数据
    /// </summary>
    /// <returns></returns>
    public ItemData GetItemData()
    {
        if (_cachedItemData == null)
        {
            _cachedItemData = InventoryMgr.GetItemData(itemId);
        }
        return _cachedItemData;
    }
}