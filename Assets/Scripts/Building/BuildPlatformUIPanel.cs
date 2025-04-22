using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildPlatformUIPanel : MonoSingleton<BuildPlatformUIPanel>
{
    public GameObject uiPanel;

    public TextMeshProUGUI titleName;
    
    public ScrollRect itemContainer;
    public GameObject itemPrefab;
    
    public TextMeshProUGUI itemName, itemDesc, required, itemTime;
    public Button startBuildBtn;
    
    private string _buildingInstanceId;
    private BuildBuildingData _buildingData;
    
    void Awake()
    {
        titleName.text = "设计台";
        Hide();
    }
    
    void CreateItem()
    {
        itemContainer.content.DestroyAllChildren();
        itemName.text = "";
        itemDesc.text = "";
        required.text = "";
        itemTime.text = "";
        startBuildBtn.interactable = false;
        startBuildBtn.onClick.RemoveAllListeners();
        
        foreach (var building in BuildingMgr.GetAllBuildingConfigs())
        {
            var item = Instantiate(itemPrefab, itemContainer.content).GetComponent<ProductionItem>();
            string buildingId = building.id;
            item.Setup(buildingId);
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                itemName.text = building.name;
                itemDesc.text = building.desc;
                required.text = "";
                for (int i = 0; i < building.materialIDGroup.Length; i++)
                {
                    required.text += InventoryMgr.GetItemConfig(building.materialIDGroup[i].ToString()).name + " x " + building.materialAmountGroup[i] + "\n";
                }
                itemTime.text = building.time.ToString() + "秒";
                
                startBuildBtn.interactable = true;
                startBuildBtn.onClick.RemoveAllListeners();
                startBuildBtn.onClick.AddListener(() =>
                {
                    _buildingData.GetBuildPlatformData().StartBuild(buildingId);
                });
            });
        }
    }

    public void Show(string buildingInstanceId)
    {
        this._buildingInstanceId = buildingInstanceId;
        this._buildingData = BuildingMgr.GetBuildingData<BuildBuildingData>(buildingInstanceId);
        if (this._buildingData != null)
        {
            
        }
        
        // 显示UI
        uiPanel.SetActive(true);
        // 创建物品
        CreateItem();
    }
    
    public void Hide()
    {
        uiPanel.SetActive(false);
    }
}
