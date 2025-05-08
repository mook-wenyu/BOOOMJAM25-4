using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Text;

public class ItemTipsUI : TipsUIBase
{
    [Header("UI 组件")]
    public Image itemIconImage;                 // 物品图标
    public TextMeshProUGUI itemNameText;        // 物品名称
    public TextMeshProUGUI itemDescText;        // 物品描述
    public TextMeshProUGUI itemCountText;       // 物品数量

    public TextMeshProUGUI itemQualityText;     // 物品品质
    public TextMeshProUGUI itemTypeText;        // 物品类型
    public TextMeshProUGUI itemCostText;        // 物品价格

    protected override void Awake()
    {
        base.Awake();
    }

    public void SetItemInfo(InventoryItem itemInfo)
    {
        var itemData = InventoryMgr.GetItemConfig(itemInfo.itemId);

        // 设置基础信息
        itemNameText.text = itemData.name;
        StringBuilder sb = new();

        if (itemData.type == (int)ItemType.Equipment)
        {
            sb.AppendLine($"装备类型: {InventoryMgr.EquipmentPartToString((EquipmentType)itemData.equipmentParts)}");
            sb.AppendLine("可穿戴");
            sb.AppendLine();
        }

        sb.AppendLine(itemData.desc);
        itemDescText.text = sb.ToString();

        //if (itemData.durability > 0)
        //{
        //    sb.AppendLine($"耐久度: {itemInfo.GetDurability()} / {itemData.durability}");
        //    sb.AppendLine();
        //}

        itemCountText.text = $"数量: {itemInfo.GetCount().ToString()}";

        // itemIconImage.sprite = itemInfo.icon;

        // 更新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemDescText.rectTransform);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }

    public override void Hide()
    {
        base.Hide();
    }
}