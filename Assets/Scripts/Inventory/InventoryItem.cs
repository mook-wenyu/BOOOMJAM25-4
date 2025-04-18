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

    // 装备特有属性
    private int _currentDurability;      // 当前耐久度
    public int CurrentDurability 
    { 
        get => _currentDurability; 
        private set
        {
            if (_currentDurability != value)
            {
                _currentDurability = Mathf.Max(0, value);
                OnDurabilityChanged?.Invoke(this);
                
                // 耐久度为0时触发破损事件
                if (_currentDurability <= 0 && GetItemData().durability > 0)
                {
                    OnBroken?.Invoke(this);
                }
            }
        }
    }
    
    // 使用次数（对于可重复使用的消耗品）
    private int _usesRemaining;
    public int UsesRemaining 
    { 
        get => _usesRemaining; 
        private set
        {
            if (_usesRemaining != value)
            {
                _usesRemaining = Mathf.Max(0, value);
                OnUsesRemainingChanged?.Invoke(this);
            }
        }
    }

    // 添加数量变化事件
    public event Action<InventoryItem> OnCountChanged;
    
    // 添加耐久度变化事件
    public event Action<InventoryItem> OnDurabilityChanged;
    
    // 添加物品破损事件
    public event Action<InventoryItem> OnBroken;
    
    // 添加使用次数变化事件
    public event Action<InventoryItem> OnUsesRemainingChanged;

    public InventoryItem(string itemId, int count = 1)
    {
        this.instanceId = System.Guid.NewGuid().ToString("N");
        this.itemId = itemId;

        _cachedItemData = InventoryMgr.GetItemData(itemId);
        if (_cachedItemData != null)
        {
            this.Count = Mathf.Min(count, _cachedItemData.maxStack);
            
            // 初始化装备耐久度
            if (_cachedItemData.itemType == ItemType.Equipment && _cachedItemData.durability > 0)
            {
                _currentDurability = _cachedItemData.durability;
            }
            
            // 初始化可重复使用的消耗品
            if (_cachedItemData.itemType == ItemType.Consumable && _cachedItemData.isReusable)
            {
                // 这里可以从配置读取初始使用次数，暂时设为固定值10
                _usesRemaining = 10;
            }
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
    
    /// <summary>
    /// 使用物品
    /// </summary>
    /// <returns>是否使用成功</returns>
    public bool Use()
    {
        var itemData = GetItemData();
        if (itemData == null) return false;
        
        switch (itemData.itemType)
        {
            case ItemType.Consumable:
                return UseConsumable();
                
            case ItemType.Equipment:
                return EquipItem();
                
            case ItemType.Material:
            case ItemType.Quest:
            case ItemType.Currency:
            case ItemType.Other:
                // 这些类型的物品不能直接使用
                return false;
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// 使用消耗品
    /// </summary>
    private bool UseConsumable()
    {
        var itemData = GetItemData();
        
        // 如果是可重复使用的消耗品
        if (itemData.isReusable)
        {
            if (UsesRemaining <= 0) return false;
            
            // 减少使用次数
            UsesRemaining--;
            
            // 如果使用次数用完，移除物品
            if (UsesRemaining <= 0)
            {
                RemoveCount(1);
            }
            
            return true;
        }
        else // 一次性消耗品
        {
            // 消耗一个
            RemoveCount(1);
            return true;
        }
    }
    
    /// <summary>
    /// 装备物品
    /// </summary>
    private bool EquipItem()
    {
        var itemData = GetItemData();
        if (itemData.itemType != ItemType.Equipment) return false;
        
        // 这里应该调用装备系统的相关方法
        // 暂时只是演示，返回成功
        return true;
    }
    
    /// <summary>
    /// 减少装备耐久度
    /// </summary>
    /// <param name="amount">减少量</param>
    /// <returns>是否成功减少</returns>
    public bool ReduceDurability(int amount = 1)
    {
        var itemData = GetItemData();
        if (itemData == null || itemData.itemType != ItemType.Equipment || itemData.durability <= 0)
            return false;
            
        CurrentDurability -= amount;
        return true;
    }
    
    /// <summary>
    /// 修复装备耐久度
    /// </summary>
    /// <param name="amount">修复量</param>
    /// <returns>是否成功修复</returns>
    public bool RepairDurability(int amount)
    {
        var itemData = GetItemData();
        if (itemData == null || itemData.itemType != ItemType.Equipment || itemData.durability <= 0)
            return false;
            
        if (CurrentDurability >= itemData.durability)
            return false;
            
        CurrentDurability = Mathf.Min(CurrentDurability + amount, itemData.durability);
        return true;
    }
    
    /// <summary>
    /// 获取物品类型
    /// </summary>
    public ItemType GetItemType()
    {
        var itemData = GetItemData();
        return itemData != null ? itemData.itemType : ItemType.Other;
    }
    
    /// <summary>
    /// 获取装备类型
    /// </summary>
    public EquipmentType GetEquipmentType()
    {
        var itemData = GetItemData();
        return itemData != null && itemData.itemType == ItemType.Equipment 
            ? itemData.equipmentType 
            : EquipmentType.None;
    }
    
    /// <summary>
    /// 判断物品是否已损坏（装备）
    /// </summary>
    public bool IsBroken()
    {
        var itemData = GetItemData();
        return itemData != null && itemData.itemType == ItemType.Equipment 
            && itemData.durability > 0 && CurrentDurability <= 0;
    }
}