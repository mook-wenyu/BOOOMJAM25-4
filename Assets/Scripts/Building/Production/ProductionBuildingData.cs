using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class ProductionBuildingData : BuildingData
{
    public string productionPlatformInstanceId; // 生产实例ID

    public ProductionBuildingData() : base() { }

    public ProductionBuildingData(string buildingId)
        : base(buildingId)
    {
        var config = BuildingMgr.GetBuildingConfig(buildingId);
        if (config == null)
        {
            return;
        }
        var recipeIds = new List<string>();
        foreach (int recipeId in config.recipes)
        {
            recipeIds.Add(recipeId.ToString());
        }
        // 创建生产数据
        ProductionPlatformData productionPlatformData = new(instanceId, recipeIds);
        productionPlatformInstanceId = productionPlatformData.instanceId;
        GameMgr.currentSaveData.productionPlatforms[productionPlatformInstanceId] = productionPlatformData;
    }

    /// <summary>
    /// 获取生产平台数据
    /// </summary>
    public ProductionPlatformData GetProductionPlatformData()
    {
        return GameMgr.currentSaveData.productionPlatforms[productionPlatformInstanceId];
    }
}
