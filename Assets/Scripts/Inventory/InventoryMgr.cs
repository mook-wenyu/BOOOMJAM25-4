using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

public static class InventoryMgr
{
    public const int SLOT_COUNT_DEFAULT = 10;    // 当前背包容量
    public const int SLOT_COUNT_MAX = 10;  // 背包最大容量

    // 耐久度条颜色
    public static Color DurabilityGoodColor { get; private set; } = Color.green;
    public static Color DurabilityMediumColor { get; private set; } = Color.yellow;
    public static Color DurabilityLowColor { get; private set; } = Color.red;

    /// <summary>
    /// 获取物品数据
    /// </summary>
    public static ItemConfig GetItemConfig(string itemId)
    {
        return ConfigManager.Instance.GetConfig<ItemConfig>(itemId);
    }

    public static T GetInventoryData<T>(string inventoryId) where T : BaseInventoryData
    {
        return GameMgr.currentSaveData.inventories[inventoryId] as T;
    }

    public static BaseInventoryData GetInventoryData(string inventoryId)
    {
        return GameMgr.currentSaveData.inventories[inventoryId];
    }

    public static InventoryData GetPlayerInventoryData()
    {
        return GetInventoryData<InventoryData>(CharacterMgr.Player().inventoryId);
    }

    /// <summary>
    /// 获取指定类型的所有物品数据
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <returns>物品数据列表</returns>
    public static List<ItemConfig> GetItemsByType(ItemType itemType)
    {
        return ConfigManager.Instance.GetConfigList<ItemConfig>()
            .Where(item => item.type == (int)itemType)
            .ToList();
    }

    /// <summary>
    /// 获取指定装备类型的所有物品数据
    /// </summary>
    /// <param name="equipType">装备类型</param>
    /// <returns>物品数据列表</returns>
    public static List<ItemConfig> GetItemsByEquipmentType(EquipmentType equipType)
    {
        return ConfigManager.Instance.GetConfigList<ItemConfig>()
            .Where(item => item.type == (int)ItemType.Equipment && item.equipmentParts == (int)equipType)
            .ToList();
    }

    /// <summary>
    /// 获取玩家背包中指定类型的物品
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <returns>物品列表</returns>
    public static List<InventoryItem> GetPlayerItemsByType(ItemType itemType)
    {
        var inventory = GetPlayerInventoryData();
        if (inventory == null) return new List<InventoryItem>();

        return inventory.items.Values
            .Where(item =>
            {
                var itemData = item.GetItemData();
                return itemData != null && itemData.type == (int)itemType;
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
        var inventory = GetPlayerInventoryData();
        if (inventory == null) return new List<InventoryItem>();

        return inventory.items.Values
            .Where(item =>
            {
                var itemData = item.GetItemData();
                return itemData != null &&
                       itemData.type == (int)ItemType.Equipment &&
                       itemData.equipmentParts == (int)equipType;
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
        var itemData = GetItemConfig(itemId);
        if (itemData == null) return false;

        // 装备类型物品不可堆叠
        if (itemData.type == (int)ItemType.Equipment)
            return false;

        // 其他类型可以堆叠
        return itemData.stacking > 1;
    }

}