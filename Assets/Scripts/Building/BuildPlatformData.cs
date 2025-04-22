using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildPlatformData
{
    public string instanceId = System.Guid.NewGuid().ToString("N");   // 实例ID

    public List<string> buildings = new();    // 建筑列表
    
    public List<BuildingData> buildingProgress = new(); // 正在生产中的建筑
    
    public BuildPlatformData() { }
    
    /// <summary>
    /// 开始建造
    /// </summary>
    public void StartBuild(string buildingId)
    {
        var buildingConfig = BuildingMgr.GetBuildingConfig(buildingId);
        if (buildingConfig == null)
        {
            Debug.Log("建筑不存在");
            return;
        }
        var playerInventory = InventoryMgr.GetPlayerInventoryData();
        bool hasEnoughMaterials = true;
        // 检查材料是否足够
        for (int i = 0; i < buildingConfig.materialIDGroup.Length; i++)
        {
            if (!playerInventory.HasItemCount(buildingConfig.materialIDGroup[i].ToString(), buildingConfig.materialAmountGroup[i]))
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
        for (int i = 0; i < buildingConfig.materialIDGroup.Length; i++)
        {
            playerInventory.RemoveItem(buildingConfig.materialIDGroup[i].ToString(), buildingConfig.materialAmountGroup[i]);
        }
        
        var buildingData = new BuildingData(buildingId);
        buildingProgress.Add(buildingData);
    }
    
}