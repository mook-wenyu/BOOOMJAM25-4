using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ProductionPlatformData
{
    public string instanceId;   // 实例ID
    // 所属建筑实例id
    public string buildingInstanceId;
    
    public List<string> recipes = new();    // 配方列表
    
    public List<ProductionData> productionProgress = new(); // 正在生产中的配方
    
    public ProductionPlatformData() { }
    
    public ProductionPlatformData(string buildingInstanceId, List<string> recipes)
    {
        this.instanceId = System.Guid.NewGuid().ToString("N");
        this.buildingInstanceId = buildingInstanceId;
        this.recipes = recipes;
    }
    
    /// <summary>
    /// 开始生产
    /// </summary>
    public void StartProduction(string recipeId)
    {
        var recipeConfig = RecipeMgr.GetRecipesConfig(recipeId);
        if (recipeConfig == null)
        {
            Debug.Log("配方不存在");
            return;
        }
        var playerInventory = InventoryMgr.GetPlayerInventoryData();
        bool hasEnoughMaterials = true;
        // 检查材料是否足够
        for (int i = 0; i < recipeConfig.materialIDGroup.Length; i++)
        {
            if (!playerInventory.HasItemCount(recipeConfig.materialIDGroup[i].ToString(), recipeConfig.materialAmountGroup[i]))
            {
                hasEnoughMaterials = false;
                break;
            }
        }
        if (!hasEnoughMaterials)
        {
            Debug.Log("材料不足");
            return;
        }
        // 消耗材料
        for (int i = 0; i < recipeConfig.materialIDGroup.Length; i++)
        {
            playerInventory.RemoveItem(recipeConfig.materialIDGroup[i].ToString(), recipeConfig.materialAmountGroup[i]);
        }
        
        var recipeData = new ProductionData(recipeId, instanceId);
        productionProgress.Add(recipeData);
    }
}
