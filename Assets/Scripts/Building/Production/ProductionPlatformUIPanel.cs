using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductionPlatformUIPanel : MonoSingleton<ProductionPlatformUIPanel>
{
    public GameObject uiPanel;

    public TextMeshProUGUI titleName;

    public ScrollRect itemContainer;
    public GameObject itemPrefab;

    public TextMeshProUGUI itemName, itemDesc, itemTime;
    public Transform requiredContainer;
    public GameObject requiredItemPrefab;
    public Button startProductionBtn;

    public ScrollRect productSlotContainer;
    public GameObject productSlotPrefab;

    public List<ProductSlot> _activeSlots;

    private string _buildingInstanceId;
    private ProductionBuildingData _productionBuildingData;
    private ProductionPlatformData _productionPlatformData;

    void Awake()
    {
        // 清除所有物品槽
        itemContainer.content.DestroyAllChildren();
        productSlotContainer.content.DestroyAllChildren();
        _activeSlots = new List<ProductSlot>();

        Hide();
    }

    void CreateItem()
    {
        itemContainer.content.DestroyAllChildren();
        foreach (var slot in _activeSlots)
        {
            slot.Clear();
        }

        itemName.text = "";
        itemDesc.text = "";
        itemTime.text = "";
        startProductionBtn.interactable = false;
        startProductionBtn.onClick.RemoveAllListeners();

        foreach (string recipeId in _productionPlatformData.recipes)
        {
            var item = Instantiate(itemPrefab, itemContainer.content).GetComponent<ProductionPlatformItem>();
            var recipeConfig = RecipeMgr.GetRecipesConfig(recipeId);
            item.Setup(recipeConfig);
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                var itemConfig = InventoryMgr.GetItemConfig(recipeConfig.productID[0]);
                itemName.text = itemConfig.name;
                itemDesc.text = itemConfig.desc;
                requiredContainer.DestroyAllChildren();
                for (int i = 0; i < recipeConfig.materialIDGroup.Length; i++)
                {
                    var requiredItem = Instantiate(requiredItemPrefab, requiredContainer).GetComponent<RequiredItemSlot>();
                    requiredItem.Setup(recipeConfig.materialIDGroup[i].ToString(), recipeConfig.materialAmountGroup[i]);
                }
                itemTime.text = recipeConfig.time.ToString() + "小时";

                startProductionBtn.interactable = true;
                startProductionBtn.onClick.RemoveAllListeners();
                startProductionBtn.onClick.AddListener(() =>
                {
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
                    bool hasEmptySlot = false;
                    ProductSlot emptySlot = null;
                    foreach (var slot in _activeSlots)
                    {
                        if (slot.CurrentItem == null)
                        {
                            hasEmptySlot = true;
                            emptySlot = slot;
                            break;
                        }
                    }
                    if (!hasEmptySlot)
                    {
                        Debug.Log("没有空槽位");
                        return;
                    }

                    //Hide();
                    var productionData = _productionPlatformData.StartProduction(recipeConfig);
                    emptySlot.Setup(productionData);
                });
            });
        }
    }

    public void CreateItemSlots()
    {
        int currentCount = _activeSlots.Count;
        int targetCount = 5;

        // 创建新的物品槽
        for (int i = currentCount; i < targetCount; i++)
        {
            var newSlot = Instantiate(productSlotPrefab, productSlotContainer.content).GetComponent<ProductSlot>();
            newSlot.Clear();
            newSlot.OnSlotClicked += OnItemSlotClicked;
            _activeSlots.Add(newSlot);
        }

        RefreshSlots();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < _activeSlots.Count; i++)
        {
            var item = i < _productionPlatformData.productionProgress.Count ? _productionPlatformData.productionProgress[i] : null;
            var slot = _activeSlots[i];

            if (item != null)
            {
                slot.Setup(item);
            }
            else
            {
                slot.Clear();
            }
        }
    }

    private void OnItemSlotClicked(ProductionData item)
    {
        if (item == null) return;
        if (item.IsComplete())
        {
            var recipeConfig = RecipeMgr.GetRecipesConfig(item.recipeId);
            InventoryMgr.GetPlayerInventoryData().AddItem(recipeConfig.productID[0], 1);
            _productionPlatformData.productionProgress.Remove(item);
            CreateItemSlots();
        }
    }

    public void Show(string buildingInstanceId)
    {
        this._buildingInstanceId = buildingInstanceId;
        this._productionBuildingData = BuildingMgr.GetBuildingData<ProductionBuildingData>(buildingInstanceId);

        if (this._productionBuildingData != null)
        {
            titleName.text = this._productionBuildingData.GetBuildingConfig().name;
        }

        this._productionPlatformData = this._productionBuildingData.GetProductionPlatformData();

        // 显示仓库UI
        uiPanel.SetActive(true);
        // 创建物品
        CreateItem();
        CreateItemSlots();
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }
}
