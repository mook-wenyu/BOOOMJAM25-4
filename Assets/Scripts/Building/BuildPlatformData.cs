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
    public void StartBuild(string buildingId, string buildingInstanceId)
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
        
        // 设置角色状态为建造中
        CharacterEntityMgr.Instance.GetPlayer().GetCharacterData().status = CharacterStatus.Build;
        CharacterEntityMgr.Instance.GetPlayer().GetAnimator().SetBool("IsBuild", true);
        switch ((BuildingType) buildingConfig.type)
        {
            case BuildingType.Warehouse:
                if(buildingConfig.id == "20001")
                {
                    buildingProgress.Add(new WarehouseBuildingData(buildingId, buildingInstanceId, WarehouseType.Box, 9));
                }
                if(buildingConfig.id == "20002")
                {
                    buildingProgress.Add(new WarehouseBuildingData(buildingId, buildingInstanceId, WarehouseType.IceBox, 9));
                }
                break;
            case BuildingType.Production:
                buildingProgress.Add(new ProductionBuildingData(buildingId, buildingInstanceId));                
                break;
        }
        Debug.Log("开始建造");
    }
    
}