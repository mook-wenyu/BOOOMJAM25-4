using System;

/// <summary>
/// 建筑类型
/// </summary>
[Serializable]
public enum BuildingType
{
    /// <summary>
    /// 仓库
    /// </summary>
    Warehouse,
    /// <summary>
    /// 生产
    /// </summary>
    Production,
    /// <summary>
    /// 照明
    /// </summary>
    Light
}

[Serializable]
public class BuildingData
{
    public string instanceId;   // 实例ID
    public string buildingId;   // 建筑ID

    public int remainingTime;   // 剩余时间

    private int totalTime;  // 总时间

    public ILightingData lightingData = null;

    [NonSerialized]
    private BuildingConfig _building;    // 建筑数据缓存

    public event Action<BuildingData> OnBuildingTimeChanged;

    public event Action<BuildingData> OnBuildingComplete;

    public BuildingData() { }

    public BuildingData(string buildingId)
    {
        this.instanceId = System.Guid.NewGuid().ToString("N");
        this.buildingId = buildingId;

        _building = BuildingMgr.GetBuildingConfig(buildingId);
        if (_building != null)
        {
            totalTime = GameTime.HourToMinute(_building.time);
            remainingTime = totalTime;
        }
    }

    public BuildingData(string buildingId, string instanceId)
    {
        this.buildingId = buildingId;
        this.instanceId = instanceId;

        _building = BuildingMgr.GetBuildingConfig(buildingId);
        if (_building != null)
        {
            totalTime = GameTime.HourToMinute(_building.time);
            remainingTime = totalTime;
        }
    }

    /// <summary>
    /// 设置照明数据
    /// </summary>
    public void SetLightingData(ILightingData lightingData)
    {
        this.lightingData = lightingData;
    }

    public BuildingConfig GetBuildingConfig()
    {
        _building ??= BuildingMgr.GetBuildingConfig(buildingId);
        return _building;
    }

    public BuildingType GetBuildingType()
    {
        _building ??= BuildingMgr.GetBuildingConfig(buildingId);
        return (BuildingType)_building.type;
    }

    /// <summary>
    /// 设置时间
    /// </summary>
    public void SetTime(int time)
    {
        _building ??= BuildingMgr.GetBuildingConfig(buildingId);
        if (_building == null || _building.time <= 0)
            return;

        remainingTime = Math.Min(time, totalTime);
        OnBuildingTimeChanged?.Invoke(this);

        if (IsComplete())
        {
            OnBuildingComplete?.Invoke(this);
        }
    }

    /// <summary>
    /// 减少时间
    /// </summary>
    public void ReduceTime(int deltaTime = 1)
    {
        SetTime(remainingTime - deltaTime);
    }

    /// <summary>
    /// 是否完成
    /// </summary>
    public bool IsComplete()
    {
        return totalTime > 0 && remainingTime <= 0;
    }
}
