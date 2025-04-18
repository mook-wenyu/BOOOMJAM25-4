using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

public static class InventoryMgr
{
    private static Dictionary<string, ItemData> _itemDatabase = new();

    public const int SLOT_COUNT = 10;    // 当前背包容量
    public const int SLOT_COUNT_MAX = 20;  // 背包最大容量

    public static void Initialize()
    {
        Debug.Log("开始加载物品数据");
        _itemDatabase.Clear();

        try
        {
            var assetDatas = Resources.LoadAll<TextAsset>("ItemData");
            foreach (var assetData in assetDatas)
            {
                var itemData = JsonConvert.DeserializeObject<ItemData>(assetData.text);
                if (itemData != null)
                {
                    RegisterItem(itemData);
                }
            }
            Debug.Log($"成功加载 {assetDatas.Length} 个物品数据");
        }
        catch (Exception e)
        {
            Debug.LogError($"加载物品数据失败: {e.Message}");
        }
    }

    // 添加物品数据管理接口
    public static void RegisterItem(ItemData itemData)
    {
        _itemDatabase[itemData.id] = itemData;
    }

    public static ItemData GetItemData(string itemId)
    {
        return _itemDatabase.TryGetValue(itemId, out var itemData) ? itemData : null;
    }

    /// <summary>
    /// 获取背包数据
    /// </summary>
    /// <returns></returns>
    public static InventoryData GetInventoryData()
    {
        return GameMgr.currentSaveData.Inventory;
    }
    
    /// <summary>
    /// 获取指定类型的所有物品数据
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <returns>物品数据列表</returns>
    public static List<ItemData> GetItemsByType(ItemType itemType)
    {
        return _itemDatabase.Values
            .Where(item => item.itemType == itemType)
            .ToList();
    }
    
    /// <summary>
    /// 获取指定装备类型的所有物品数据
    /// </summary>
    /// <param name="equipType">装备类型</param>
    /// <returns>物品数据列表</returns>
    public static List<ItemData> GetItemsByEquipmentType(EquipmentType equipType)
    {
        return _itemDatabase.Values
            .Where(item => item.itemType == ItemType.Equipment && item.equipmentType == equipType)
            .ToList();
    }
    
    /// <summary>
    /// 使用物品
    /// </summary>
    /// <param name="instanceId">物品实例ID</param>
    /// <returns>是否使用成功</returns>
    public static bool UseItem(string instanceId)
    {
        var inventory = GetInventoryData();
        if (inventory == null || !inventory.Items.TryGetValue(instanceId, out var item))
        {
            Debug.LogWarning($"找不到物品: {instanceId}");
            return false;
        }
        
        return item.Use();
    }
    
    /// <summary>
    /// 获取玩家背包中指定类型的物品
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <returns>物品列表</returns>
    public static List<InventoryItem> GetPlayerItemsByType(ItemType itemType)
    {
        var inventory = GetInventoryData();
        if (inventory == null) return new List<InventoryItem>();
        
        return inventory.Items.Values
            .Where(item => {
                var itemData = item.GetItemData();
                return itemData != null && itemData.itemType == itemType;
            })
            .ToList();
    }
    
    /// <summary>
    /// 获取玩家背包中指定装备类型的物品
    /// </summary>
    /// <param name="equipType">装备类型</param>
    /// <returns>物品列表</returns>
    public static List<InventoryItem> GetPlayerItemsByEquipmentType(EquipmentType equipType)
    {
        var inventory = GetInventoryData();
        if (inventory == null) return new List<InventoryItem>();
        
        return inventory.Items.Values
            .Where(item => {
                var itemData = item.GetItemData();
                return itemData != null && 
                       itemData.itemType == ItemType.Equipment && 
                       itemData.equipmentType == equipType;
            })
            .ToList();
    }
    
    /// <summary>
    /// 检查物品是否可堆叠
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <returns>是否可堆叠</returns>
    public static bool IsItemStackable(string itemId)
    {
        var itemData = GetItemData(itemId);
        if (itemData == null) return false;
        
        // 装备类型物品不可堆叠
        if (itemData.itemType == ItemType.Equipment)
            return false;
            
        // 其他类型可以堆叠
        return itemData.maxStack > 1;
    }
    
    /// <summary>
    /// 获取特定稀有度的物品
    /// </summary>
    /// <param name="rarity">稀有度</param>
    /// <returns>物品数据列表</returns>
    public static List<ItemData> GetItemsByRarity(int rarity)
    {
        return _itemDatabase.Values
            .Where(item => item.rarity == rarity)
            .ToList();
    }
}