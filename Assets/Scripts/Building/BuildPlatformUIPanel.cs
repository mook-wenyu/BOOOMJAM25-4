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

    public TextMeshProUGUI itemName, itemDesc, itemTime;
    public Transform requiredContainer;
    public GameObject requiredItemPrefab;

    public Button startBuildBtn;

    private string _buildingInstanceId;
    private BuildBuildingData _buildingData;

    void Awake()
    {
        titleName.text = "设计台";
        itemContainer.content.DestroyAllChildren();
        Hide();
    }

    void CreateItem()
    {
        itemContainer.content.DestroyAllChildren();
        itemName.text = "";
        itemDesc.text = "";
        itemTime.text = "";
        startBuildBtn.interactable = false;
        startBuildBtn.onClick.RemoveAllListeners();

        foreach (var building in BuildingMgr.GetAllBuildingConfigs())
        {
            var item = Instantiate(itemPrefab, itemContainer.content).GetComponent<BuildPlatformItem>();
            item.Setup(building);
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                itemName.text = building.name;
                itemDesc.text = building.desc;
                requiredContainer.DestroyAllChildren();
                for (int i = 0; i < building.materialIDGroup.Length; i++)
                {
                    var requiredItem = Instantiate(requiredItemPrefab, requiredContainer).GetComponent<RequiredItemSlot>();
                    requiredItem.Setup(building.materialIDGroup[i].ToString(), building.materialAmountGroup[i]);
                }
                itemTime.text = building.time.ToString() + "小时";

                startBuildBtn.interactable = true;
                startBuildBtn.onClick.RemoveAllListeners();
                startBuildBtn.onClick.AddListener(() =>
                {
                    var playerInventory = InventoryMgr.GetPlayerInventoryData();
                    bool hasEnoughMaterials = true;
                    // 检查材料是否足够
                    for (int i = 0; i < building.materialIDGroup.Length; i++)
                    {
                        if (!playerInventory.HasItemCount(building.materialIDGroup[i].ToString(), building.materialAmountGroup[i]))
                        {
                            hasEnoughMaterials = false;
                            break;
                        }
                    }
                    if (!hasEnoughMaterials)
                    {
                        GlobalUIMgr.Instance.ShowMessage("材料不足");
                        return;
                    }
                    Hide();
                    RoomBuildingSystem.Instance.StartPlacingBuilding(_buildingInstanceId, building.id);
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
