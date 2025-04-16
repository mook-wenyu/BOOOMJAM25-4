using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;

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

}