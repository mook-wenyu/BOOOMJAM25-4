public static class BuildingMgr
{
    /// <summary>
    /// 获取原始建筑数据
    /// </summary>
    public static BuildingConfig GetBuildingConfig(string buildingId)
    {
        return ConfigManager.Instance.GetConfig<BuildingConfig>(buildingId);
    }

    /// <summary>
    /// 获取正在建造的建筑数据
    /// </summary>
    public static BuildingData GetBuildingProgress(string instanceId)
    {
        return GameMgr.currentSaveData.buildingProgress.Find(building => building.instanceId == instanceId);
    }

    /// <summary>
    /// 获取建造完成的建筑数据
    /// </summary>
    public static BuildingData GetBuildingData(string instanceId)
    {
        return GameMgr.currentSaveData.buildings[instanceId];
    }

    /// <summary>
    /// 开始建造
    /// </summary>
    /// <param name="buildingId"></param>
    /// <returns></returns>
    public static BuildingData StartBuilding(string buildingId)
    {
        var buildingData = new BuildingData(buildingId);
        if (buildingData == null)
        {
            return null;
        }
        InventoryData playerInventory = CharacterMgr.Player().Inventory;
        bool hasEnoughMaterials = true;
        // 检查材料是否足够
        for (int i = 0; i < buildingData.GetBuilding().materialIDGroup.Length; i++)
        {
            if (!playerInventory.HasInventoryItem(buildingData.GetBuilding().materialIDGroup[i].ToString(), buildingData.GetBuilding().materialAmountGroup[i]))
            {
                hasEnoughMaterials = false;
                break;
            }
        }
        if (!hasEnoughMaterials)
        {
            return null;
        }
        // 消耗材料
        for (int i = 0; i < buildingData.GetBuilding().materialIDGroup.Length; i++)
        {
            playerInventory.RemoveInventoryItem(buildingData.GetBuilding().materialIDGroup[i].ToString(), buildingData.GetBuilding().materialAmountGroup[i]);
        }
        // 添加到正在建造的建筑列表
        GameMgr.currentSaveData.buildingProgress.Add(buildingData);
        return buildingData;
    }

}