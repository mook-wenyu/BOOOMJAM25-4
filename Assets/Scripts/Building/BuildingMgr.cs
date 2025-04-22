using System.Collections.Generic;
public static class BuildingMgr
{
    /// <summary>
    /// 获取原始建筑数据
    /// </summary>
    public static BuildingConfig GetBuildingConfig(string buildingId)
    {
        return ConfigManager.Instance.GetConfig<BuildingConfig>(buildingId);
    }
    
    public static IList<BuildingConfig> GetAllBuildingConfigs()
    {
        return ConfigManager.Instance.GetConfigList<BuildingConfig>();
    }

    /// <summary>
    /// 获取建造完成的建筑数据
    /// </summary>
    public static BuildingData GetBuildingData(string instanceId)
    {
        return GameMgr.currentSaveData.buildings[instanceId];
    }

    /// <summary>
    /// 获取建造完成的建筑数据
    /// </summary>
    public static T GetBuildingData<T>(string instanceId) where T : BuildingData
    {
        return GameMgr.currentSaveData.buildings[instanceId] as T;
    }

    /// <summary>
    /// 添加已完成的建筑数据到列表中
    /// </summary>
    public static void AddBuildingData(BuildingData buildingData)
    {
        GameMgr.currentSaveData.buildings.Add(buildingData.instanceId, buildingData);
    }

}