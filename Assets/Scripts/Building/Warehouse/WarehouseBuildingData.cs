using System;

[Serializable]
public class WarehouseBuildingData : BuildingData
{
    public string inventoryId; // 仓库ID

    public WarehouseBuildingData() : base() { }

    public WarehouseBuildingData(string buildingId, WarehouseType warehouseType, int capacity)
        : base(buildingId)
    {
        // 创建仓库数据
        WarehouseData warehouseData = new(instanceId, warehouseType, capacity);
        inventoryId = warehouseData.inventoryId;
        GameMgr.currentSaveData.inventories[inventoryId] = warehouseData;
    }

    public WarehouseBuildingData(string buildingId, string instanceId, WarehouseType warehouseType, int capacity)
        : base(buildingId, instanceId)
    {
        // 创建仓库数据
        WarehouseData warehouseData = new(instanceId, warehouseType, capacity);
        inventoryId = warehouseData.inventoryId;
        GameMgr.currentSaveData.inventories[inventoryId] = warehouseData;
    }
    
    /// <summary>
    /// 获取仓库数据
    /// </summary>
    public WarehouseData GetWarehouseData()
    {
        return GameMgr.currentSaveData.inventories[inventoryId] as WarehouseData;
    }
}
