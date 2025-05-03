using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductionPlatformUIPanel : MonoSingleton<ProductionPlatformUIPanel>
{
    public GameObject uiPanel;

    public TextMeshProUGUI titleName;

    public Button closeBtn;

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

    private bool _isExplore = false;

    void Awake()
    {
        closeBtn.onClick.AddListener(() => { AudioMgr.Instance.PlaySound("点击关闭"); Hide(); });
        // 清除所有物品槽
        itemContainer.content.DestroyAllChildren();
        productSlotContainer.content.DestroyAllChildren();
        _activeSlots = new List<ProductSlot>();

        Hide();
    }

    void Update()
    {
        if (!uiPanel.activeSelf) return;
        if (uiPanel.transform.GetSiblingIndex() != uiPanel.transform.parent.childCount - 1)
        {
            return;
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Hide();
        }
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
            var recipeConfig = ProductionPlatformMgr.GetRecipesConfig(recipeId);
            item.Setup(recipeConfig);
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                var itemConfig = InventoryMgr.GetItemConfig(recipeConfig.productID[0]);
                itemName.text = itemConfig.name;
                itemDesc.text = itemConfig.desc;
                requiredContainer.DestroyAllChildren();
                for (int i = 0; i < recipeConfig.materialIDGroup.Length; i++)
                {
                    if (recipeConfig.materialIDGroup[i] == "0") continue;
                    var requiredItem = Instantiate(requiredItemPrefab, requiredContainer).GetComponent<RequiredItemSlot>();
                    requiredItem.Setup(recipeConfig.materialIDGroup[i].ToString(), recipeConfig.materialAmountGroup[i]);
                }
                itemTime.text = $"生产时间：{recipeConfig.time}小时";

                startProductionBtn.interactable = true;
                startProductionBtn.onClick.RemoveAllListeners();
                startProductionBtn.onClick.AddListener(() =>
                {
                    AudioMgr.Instance.PlaySound("点击");
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
                        GlobalUIMgr.Instance.ShowMessage("材料不足！");
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
                        GlobalUIMgr.Instance.ShowMessage("产品已满，无法生产更多产品！");
                        return;
                    }

                    // 探索
                    if (_isExplore)
                    {
                        // 检查时间是否足够
                        if (!GameMgr.currentSaveData.gameTime.IsTimeBefore(new GameTime(GameMgr.currentSaveData.gameTime.day + 1, 0, 0), GameTime.HourToMinute(recipeConfig.time)))
                        {
                            GlobalUIMgr.Instance.ShowMessage("太晚了，明天再做吧！");
                            return;
                        }
                    }

                    // 消耗材料
                    for (int i = 0; i < recipeConfig.materialIDGroup.Length; i++)
                    {
                        playerInventory.RemoveItem(recipeConfig.materialIDGroup[i], recipeConfig.materialAmountGroup[i]);
                    }

                    var productionData = new ProductionData(recipeConfig.id, _productionPlatformData.instanceId);
                    _productionPlatformData.productionProgress.Add(productionData);
                    emptySlot.Setup(productionData);

                    // 探索
                    if (_isExplore)
                    {
                        // 扣除时间
                        _ = GameMgr.currentSaveData.gameTime.AddHours(recipeConfig.time);
                    }
                });
            });
        }
    }

    public void CreateItemSlots()
    {
        int currentCount = _activeSlots.Count;
        int targetCount = 3;

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
            var recipeConfig = ProductionPlatformMgr.GetRecipesConfig(item.recipeId);
            // 尝试将物品添加到背包中，如果背包已满，则提示用户背包已满
            int count = InventoryMgr.GetPlayerInventoryData().CalculateCanAddItem(recipeConfig.productID[0], 1);
            if (count <= 0)
            {
                GlobalUIMgr.Instance.ShowMessage("背包已满");
                return;
            }
            InventoryMgr.GetPlayerInventoryData().AddItem(recipeConfig.productID[0], count);
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

        GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
        // 显示仓库UI
        uiPanel.SetActive(true);
        uiPanel.transform.SetAsLastSibling();
        // 创建物品
        CreateItem();
        CreateItemSlots();
    }

    public void ShowProductionPlatform(string productionPlatformInstanceId, bool isExplore = false)
    {
        _isExplore = isExplore;
        this._buildingInstanceId = string.Empty;

        this._productionPlatformData = ProductionPlatformMgr.GetProductionPlatformData(productionPlatformInstanceId);
        if (this._productionPlatformData != null)
        {
            titleName.text = _productionPlatformData.pName;
        }

        // 显示仓库UI
        uiPanel.SetActive(true);
        uiPanel.transform.SetAsLastSibling();
        // 创建物品
        CreateItem();
        CreateItemSlots();
    }

    public void Hide()
    {
        _isExplore = false;
        uiPanel.transform.SetAsFirstSibling();
        uiPanel.SetActive(false);
    }
}
