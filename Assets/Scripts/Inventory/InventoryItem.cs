using UnityEngine;
using System;

[Serializable]
public class InventoryItem
{
    public string instanceId;      // 物品实例ID
    public string itemId;          // 物品ID

    public int count;              // 当前数量
    public bool isEquipped = false;    // 是否已装备
    public int currentDurability = 0;    // 当前耐久度

    [NonSerialized]
    private ItemConfig _cachedItemData;     // 缓存的物品数据

    // 添加数量变化事件
    public event Action<InventoryItem> OnCountChanged;

    // 添加耐久度变化事件
    public event Action<InventoryItem> OnDurabilityChanged;

    // 添加物品破损事件
    public event Action<InventoryItem> OnBroken;

    public InventoryItem(string itemId, int count = 1)
    {
        this.instanceId = System.Guid.NewGuid().ToString("N");
        this.itemId = itemId;

        _cachedItemData = InventoryMgr.GetItemData(itemId);

        this.count = Mathf.Min(count, _cachedItemData.stacking);
        // 初始化耐久度
        if (_cachedItemData.durability > 0)
        {
            this.currentDurability = _cachedItemData.durability;
        }
    }

    /// <summary>
    /// 是否可以添加更多
    /// </summary>
    public bool CanAddMore(int amount = 1)
    {
        _cachedItemData ??= InventoryMgr.GetItemData(itemId);

        return count + amount <= _cachedItemData.stacking;
    }

    /// <summary>
    /// 获取当前数量
    /// </summary>
    public int GetCount()
    {
        return count;
    }

    public void SetCount(int newCount)
    {
        _cachedItemData ??= InventoryMgr.GetItemData(itemId);

        if (_cachedItemData != null)
        {
            count = Mathf.Min(newCount, _cachedItemData.stacking);
        }
        else
        {
            count = newCount;
        }
        OnCountChanged?.Invoke(this);
    }

    /// <summary>
    /// 添加数量
    /// </summary>
    public void AddCount(int amount = 1)
    {
        SetCount(count + amount);
    }

    /// <summary>
    /// 移除数量
    /// </summary>
    public void RemoveCount(int amount = 1)
    {
        SetCount(count - amount);
    }

    /// <summary>
    /// 获取缓存的物品数据
    /// </summary>
    public ItemConfig GetItemData()
    {
        _cachedItemData ??= InventoryMgr.GetItemData(itemId);
        return _cachedItemData;
    }

    /// <summary>
    /// 获取当前耐久度
    /// </summary>
    public int GetDurability()
    {
        return currentDurability;
    }

    /// <summary>
    /// 判断物品是否已损坏
    /// </summary>
    public bool IsBroken()
    {
        _cachedItemData ??= InventoryMgr.GetItemData(itemId);
        return _cachedItemData != null
            && _cachedItemData.durability > 0 && currentDurability <= 0;
    }

    public void SetDurability(int newDurability)
    {
        _cachedItemData ??= InventoryMgr.GetItemData(itemId);

        if (_cachedItemData == null || _cachedItemData.durability <= 0)
            return;

        currentDurability = Mathf.Min(newDurability, _cachedItemData.durability);
        OnDurabilityChanged?.Invoke(this);
        if (IsBroken())
        {
            OnBroken?.Invoke(this);
        }
    }

    /// <summary>
    /// 减少耐久度
    /// </summary>
    public void ReduceDurability(int amount = 1)
    {
        SetDurability(currentDurability - amount);
    }

    /// <summary>
    /// 修复耐久度
    /// </summary>
    public void RepairDurability(int amount)
    {
        SetDurability(currentDurability + amount);
    }

    /// <summary>
    /// 获取物品是否已装备
    /// </summary>
    public bool GetIsEquipped()
    {
        return isEquipped;
    }

    /// <summary>
    /// 设置物品是否已装备
    /// </summary>
    public void SetIsEquipped(bool value)
    {
        isEquipped = value;
    }

    /// <summary>
    /// 获取物品类型
    /// </summary>
    public ItemType GetItemType()
    {
        _cachedItemData ??= InventoryMgr.GetItemData(itemId);
        return _cachedItemData != null ? (ItemType)_cachedItemData.type : ItemType.Material;
    }

    /// <summary>
    /// 获取装备类型
    /// </summary>
    public EquipmentType GetEquipmentType()
    {
        _cachedItemData ??= InventoryMgr.GetItemData(itemId);
        return _cachedItemData != null && _cachedItemData.type == (int)ItemType.Equipment
            ? (EquipmentType)_cachedItemData.equipmentParts
            : EquipmentType.None;
    }

}