using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductionPlatformUIPanel : MonoSingleton<ProductionPlatformUIPanel>
{
    public GameObject productionPlatformPanel;
    
    public TextMeshProUGUI productionPlatformName;
    
    public ScrollRect itemContainer;
    public GameObject itemPrefab;
    
    public TextMeshProUGUI itemName, itemDesc, required, itemTime;
    public Button startProductionBtn;
    
    private string _buildingInstanceId;
    private ProductionBuildingData _productionBuildingData;
    
    void Awake()
    {
        // 清除所有物品槽
        itemContainer.content.DestroyAllChildren();

        Hide();
    }

    void CreateItem()
    {
        itemContainer.content.DestroyAllChildren();
        itemName.text = "";
        itemDesc.text = "";
        required.text = "";
        itemTime.text = "";
        startProductionBtn.interactable = false;
        startProductionBtn.onClick.RemoveAllListeners();
        
        foreach (string recipe in _productionBuildingData.GetProductionPlatformData().recipes)
        {
            var item = Instantiate(itemPrefab, itemContainer.content).GetComponent<ProductionItem>();
            string recipeId = recipe;
            item.Setup(recipeId);
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                var recipeConfig = RecipeMgr.GetRecipesConfig(recipeId);
                var itemConfig = InventoryMgr.GetItemConfig(recipeConfig.productID.ToString());
                itemName.text = itemConfig.name;
                itemDesc.text = itemConfig.desc;
                required.text = "";
                for (int i = 0; i < recipeConfig.materialIDGroup.Length; i++)
                {
                    required.text += InventoryMgr.GetItemConfig(recipeConfig.materialIDGroup[i].ToString()).name + " x " + recipeConfig.materialAmountGroup[i] + "\n";
                }
                itemTime.text = recipeConfig.time.ToString() + "秒";
                
                startProductionBtn.interactable = true;
                startProductionBtn.onClick.RemoveAllListeners();
                startProductionBtn.onClick.AddListener(() =>
                {
                    _productionBuildingData.GetProductionPlatformData().StartProduction(recipeId);
                });
            });
        }
    }
    
    public void Show(string buildingInstanceId)
    {
        this._buildingInstanceId = buildingInstanceId;
        this._productionBuildingData = BuildingMgr.GetBuildingData<ProductionBuildingData>(buildingInstanceId);
        if (this._productionBuildingData != null)
        {
            productionPlatformName.text = this._productionBuildingData.GetBuilding().name;
        }
        
        // 显示仓库UI
        productionPlatformPanel.SetActive(true);
        // 创建物品
        CreateItem();
    }
    
    public void Hide()
    {
        productionPlatformPanel.SetActive(false);
    }
}
