using System;

[Serializable]
public class WarehouseBuildingData : BuildingData
{
    public string inventoryId;

    public WarehouseBuildingData() : base() { }

    public WarehouseBuildingData(string buildingId, WarehouseType warehouseType, int capacity)
        : base(buildingId)
    {
        WarehouseData warehouseData = new(buildingId, warehouseType, capacity);
        inventoryId = warehouseData.inventoryId;
        GameMgr.currentSaveData.inventories[inventoryId] = warehouseData;
    }

    public WarehouseData GetWarehouseData()
    {
        return GameMgr.currentSaveData.inventories[inventoryId] as WarehouseData;
    }
}
