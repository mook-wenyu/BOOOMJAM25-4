using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildPlatformUIPanel : MonoSingleton<BuildPlatformUIPanel>
{
    public GameObject uiPanel;

    public TextMeshProUGUI titleName;

    public Button closeBtn;

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
        closeBtn.onClick.AddListener(() => { AudioMgr.Instance.PlaySound("点击关闭"); Hide(); });
        itemContainer.content.DestroyAllChildren();
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
        itemName.text = "";
        itemDesc.text = "";
        itemTime.text = "";
        startBuildBtn.interactable = false;
        startBuildBtn.onClick.RemoveAllListeners();

        foreach (var building in BuildingMgr.GetAllBuildingConfigs())
        {
            if (building.materialIDGroup == null && building.time <= 0) continue;

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
                itemTime.text = $"建造时间：{building.time}小时";

                startBuildBtn.interactable = true;
                startBuildBtn.onClick.RemoveAllListeners();
                startBuildBtn.onClick.AddListener(() =>
                {
                    AudioMgr.Instance.PlaySound("点击");
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

        GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
        // 显示UI
        uiPanel.SetActive(true);
        uiPanel.transform.SetAsLastSibling();
        // 创建物品
        CreateItem();
    }

    public void Hide()
    {
        uiPanel.transform.SetAsFirstSibling();
        uiPanel.SetActive(false);
    }
}
