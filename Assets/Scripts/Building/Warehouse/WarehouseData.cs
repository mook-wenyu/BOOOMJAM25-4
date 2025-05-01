using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 仓库类型
/// </summary>
[Serializable]
public enum WarehouseType
{
    /// <summary>
    /// 木箱
    /// </summary>
    Box,
    /// <summary>
    /// 冰箱
    /// </summary>
    IceBox
}

[Serializable]
public class WarehouseData : BaseInventoryData
{
    public string buildingInstanceId;
    public string wName;
    public WarehouseType warehouseType;

    public WarehouseData() : base()
    {
    }

    public WarehouseData(string buildingInstanceId, WarehouseType warehouseType, int capacity)
        : base(InventoryType.Warehouse, capacity)
    {
        this.buildingInstanceId = buildingInstanceId;
        this.warehouseType = warehouseType;
    }

    public WarehouseData(string inventoryId, string wName, WarehouseType warehouseType, int capacity)
        : base(inventoryId, InventoryType.Warehouse, capacity)
    {
        this.buildingInstanceId = string.Empty;
        this.wName = wName;
        this.warehouseType = warehouseType;
    }

}
