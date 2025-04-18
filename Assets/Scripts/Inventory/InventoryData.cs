using System;
using System.Collections.Generic;
using System.Linq;
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

    // 新增物品类型筛选事件
    public event Action<ItemType> OnFilterByType;
    
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
        
        // 如果是装备类型且不可堆叠，直接创建新物品
        if (itemData.itemType == ItemType.Equipment)
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
    /// 获取指定类型的物品
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <returns>物品列表</returns>
    public List<InventoryItem> GetItemsByType(ItemType itemType)
    {
        OnFilterByType?.Invoke(itemType);
        
        return Items.Values
            .Where(item => {
                var itemData = item.GetItemData();
                return itemData != null && itemData.itemType == itemType;
            })
            .ToList();
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
        if (itemData == null || itemData.itemType != ItemType.Equipment)
            return false;
            
        // 检查物品是否已损坏
        if (item.IsBroken())
        {
            Debug.Log("无法装备已损坏的物品");
            return false;
        }
        
        // 如果该装备槽已有装备，则先卸下
        if (_equippedItems.TryGetValue(itemData.equipmentType, out var equippedItem))
        {
            UnequipItem(equippedItem.instanceId);
        }
        
        // 装备新物品
        _equippedItems[itemData.equipmentType] = item;
        OnEquipmentChanged?.Invoke(itemData.equipmentType, item);
        
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
        if (itemData == null || itemData.itemType != ItemType.Equipment)
            return false;
            
        // 确保该物品确实被装备了
        if (!_equippedItems.TryGetValue(itemData.equipmentType, out var equippedItem) 
            || equippedItem.instanceId != instanceId)
        {
            return false;
        }
        
        // 卸下装备
        _equippedItems.Remove(itemData.equipmentType);
        OnEquipmentChanged?.Invoke(itemData.equipmentType, null);
        
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
    /// <returns>是否使用成功</returns>
    public bool UseItem(string instanceId)
    {
        if (!Items.TryGetValue(instanceId, out var item))
            return false;
            
        var itemData = item.GetItemData();
        if (itemData == null)
            return false;
            
        bool success = false;
        
        switch (itemData.itemType)
        {
            case ItemType.Consumable:
                success = item.Use();
                break;
                
            case ItemType.Equipment:
                success = EquipItem(instanceId);
                break;
                
            case ItemType.Material:
            case ItemType.Quest:
            case ItemType.Currency:
            case ItemType.Other:
                // 这些类型的物品不能直接使用
                Debug.Log($"该类型物品无法直接使用: {itemData.itemType}");
                return false;
        }
        
        // 如果物品数量变为0，则从背包移除
        if (item.Count <= 0)
        {
            Items.Remove(instanceId);
            OnInventoryChanged?.Invoke();
        }
        
        return success;
    }
    
    /// <summary>
    /// 获取背包中特定稀有度的物品
    /// </summary>
    /// <param name="rarity">稀有度</param>
    /// <returns>物品列表</returns>
    public List<InventoryItem> GetItemsByRarity(int rarity)
    {
        return Items.Values
            .Where(item => {
                var itemData = item.GetItemData();
                return itemData != null && itemData.rarity == rarity;
            })
            .ToList();
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