using System;
using System.Collections.Generic;

[Serializable]
public class ProductionPlatformData
{
    public string instanceId;   // 实例ID
    // 所属建筑实例id
    public string buildingInstanceId;

    public string pName; // 生产平台名称

    public List<string> recipes = new();    // 配方列表

    public List<ProductionData> productionProgress = new(); // 正在生产中的配方列表

    public ProductionPlatformData() { }

    public ProductionPlatformData(string buildingInstanceId, List<string> recipes)
    {
        this.instanceId = System.Guid.NewGuid().ToString("N");
        this.buildingInstanceId = buildingInstanceId;
        this.pName = string.Empty;
        this.recipes = recipes;
    }

    public ProductionPlatformData(string instanceId, string pName, List<string> recipes)
    {
        this.instanceId = instanceId;
        this.buildingInstanceId = string.Empty;
        this.pName = pName;
        this.recipes = recipes;
    }

}
