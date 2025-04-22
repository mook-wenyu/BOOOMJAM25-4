
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
            remainingTime = _building.time;
        }
    }

    public BuildingConfig GetBuilding()
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

        remainingTime = Math.Min(time, _building.time);
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
        _building ??= BuildingMgr.GetBuildingConfig(buildingId);

        return _building != null && _building.time > 0 && remainingTime <= 0;
    }
}
