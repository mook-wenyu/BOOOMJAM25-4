using System;

[Serializable]
public class BuildBuildingData : BuildingData
{
    public string buildPlatformInstanceId; // 建筑实例ID
    
    public BuildBuildingData() : base() { }
    
    public BuildBuildingData(string buildingId)
        : base(buildingId)
    {
        // 创建建筑数据
        BuildPlatformData buildPlatformData = new();
        buildPlatformInstanceId = buildPlatformData.instanceId;
        GameMgr.currentSaveData.buildPlatforms[buildPlatformInstanceId] = buildPlatformData;
    }

    public BuildBuildingData(string buildingId, string instanceId)
        : base(buildingId, instanceId)
    {
        // 创建建筑数据
        BuildPlatformData buildPlatformData = new();
        buildPlatformInstanceId = buildPlatformData.instanceId;
        GameMgr.currentSaveData.buildPlatforms[buildPlatformInstanceId] = buildPlatformData;
    }

    public BuildPlatformData GetBuildPlatformData()
    {
        return GameMgr.currentSaveData.buildPlatforms[buildPlatformInstanceId];
    }
}
