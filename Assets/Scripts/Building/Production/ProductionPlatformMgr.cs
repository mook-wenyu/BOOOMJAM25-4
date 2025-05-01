
using System.Collections.Generic;

public static class ProductionPlatformMgr
{
    public static RecipesConfig GetRecipesConfig(string recipesId)
    {
        return ConfigManager.Instance.GetConfig<RecipesConfig>(recipesId);
    }

    public static IList<RecipesConfig> GetAllRecipesConfigs()
    {
        return ConfigManager.Instance.GetConfigList<RecipesConfig>();
    }

    /// <summary>
    /// 获取生产平台数据
    /// </summary>
    public static ProductionPlatformData GetProductionPlatformData(string instanceId)
    {
        return GameMgr.currentSaveData.productionPlatforms[instanceId];
    }

    /// <summary>
    /// 创建生产平台数据到列表中
    /// </summary>
    public static void CreateProductionPlatformData(string instanceId, string pName, List<string> recipeIds)
    {
        ProductionPlatformData productionPlatformData = new(instanceId, pName, recipeIds);
        GameMgr.currentSaveData.productionPlatforms[productionPlatformData.instanceId] = productionPlatformData;
    }

    public static bool HasProductionPlatformData(string instanceId)
    {
        return GameMgr.currentSaveData.productionPlatforms.ContainsKey(instanceId);
    }

}