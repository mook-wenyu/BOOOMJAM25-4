using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubmitUIPanel : MonoSingleton<SubmitUIPanel>
{
    public GameObject uiPanel;

    public TextMeshProUGUI titleName;

    public TextMeshProUGUI desc, time;
    public Transform requiredContainer;
    public GameObject requiredItemPrefab;
    public Button startBtn;

    private ExploreNodeData _node;

    void Awake()
    {
        titleName.text = "提交";
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

    public void CreateItem(string[] requiredMaterialIdGroup, int[] requiredMaterialAmountGroup, double submitTime)
    {
        requiredContainer.DestroyAllChildren();
        for (int i = 0; i < requiredMaterialIdGroup.Length; i++)
        {
            var requiredItem = Instantiate(requiredItemPrefab, requiredContainer).GetComponent<RequiredItemSlot>();
            requiredItem.Setup(requiredMaterialIdGroup[i], requiredMaterialAmountGroup[i]);
        }
        time.text = submitTime.ToString() + "小时";
        startBtn.onClick.RemoveAllListeners();
        startBtn.onClick.AddListener(() =>
        {
            // 检查材料是否足够
            for (int i = 0; i < requiredMaterialIdGroup.Length; i++)
            {
                if (!InventoryMgr.GetPlayerInventoryData().HasItemCount(requiredMaterialIdGroup[i], requiredMaterialAmountGroup[i]))
                {
                    GlobalUIMgr.Instance.ShowMessage("材料不足");
                    return;
                }
            }

            // 检查时间是否足够
            if (!GameMgr.currentSaveData.gameTime.IsTimeBefore(new GameTime(GameMgr.currentSaveData.gameTime.day + 1, 0, 0), GameTime.HourToMinute(submitTime)))
            {
                GlobalUIMgr.Instance.ShowMessage("太晚了，明天再来吧！");
                return;
            }

            // 扣除材料
            for (int i = 0; i < requiredMaterialIdGroup.Length; i++)
            {
                InventoryMgr.GetPlayerInventoryData().RemoveItem(requiredMaterialIdGroup[i], requiredMaterialAmountGroup[i]);
            }

            // 扣除时间
            _ = GameMgr.currentSaveData.gameTime.AddHours(submitTime);

            _node.SetCompleted();
            Hide();
        });
    }

    public void Show(ExploreNodeData node)
    {
        _node = node;
        var config = node.GetConfig();
        uiPanel.SetActive(true);
        uiPanel.transform.SetAsLastSibling();
        titleName.text = config.name;
        desc.text = config.desc;
        CreateItem(config.requiredMaterialIdGroup, config.requiredMaterialAmountGroup, config.submitTime);
    }

    public void Hide()
    {
        uiPanel.transform.SetAsFirstSibling();
        uiPanel.SetActive(false);
    }

}
