using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RequiredItemSlot : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI countText;

    private RectTransform _rectTransform;  // 添加缓存的RectTransform

    public string CurrentItemId { get; private set; }
    public int RequiredCount { get; private set; }
    public ItemConfig CurrentItemConfig { get; private set; }

    public bool IsPointerOver { get; private set; }

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(string itemId, int count)
    {
        this.CurrentItemId = itemId;
        this.RequiredCount = count;

        CurrentItemConfig = InventoryMgr.GetItemConfig(itemId);
        if (!string.IsNullOrEmpty(CurrentItemConfig.path) && CurrentItemConfig.path != "0")
        {
            itemIcon.sprite = Resources.Load<Sprite>(CurrentItemConfig.path);
        }
        else
        {
            itemIcon.sprite = Resources.Load<Sprite>(Path.Combine("Icon", "icon_item_unknown"));
        }

        UpdateCount();
    }

    public void UpdateCount()
    {
        if (countText != null)
        {
            var itemCount = InventoryMgr.GetPlayerInventoryData().GetItemCount(CurrentItemId);
            countText.color = itemCount >= this.RequiredCount ? Color.green : Color.red;
            countText.text = $"{itemCount}/{this.RequiredCount}";
        }
    }

    /// <summary>
    /// 更新并显示物品提示
    /// </summary>
    public void UpdateTips(ItemConfig item)
    {
        if (!IsPointerOver) return; // 只在鼠标悬停时才显示提示

        var tipsUI = GlobalUIMgr.Instance.Show<SimpleTipsUI>(GlobalUILayer.TooltipLayer);

        tipsUI.SetContent($"{item.name}\r\n\r\n{item.desc}");

        var sizeDelta = _rectTransform.sizeDelta;
        sizeDelta.x /= 2;
        // 直接使用RectTransform的position，因为UI元素的position已经是屏幕空间的坐标
        tipsUI.UpdatePosition(_rectTransform.position, sizeDelta);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 即使没有物品也要设置指针状态，因为这关系到提示框的显示逻辑
        IsPointerOver = true;

        // 如果有物品，则显示提示
        if (CurrentItemConfig != null)
        {
            UpdateTips(CurrentItemConfig);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOver = false;
        var tipsUI = GlobalUIMgr.Instance.Get<SimpleTipsUI>();

        if (!IsPointerOver && tipsUI != null && tipsUI.gameObject.activeSelf)
        {
            GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
        }
    }

    public void Clear()
    {
        CurrentItemId = null;
        RequiredCount = 0;

        countText.text = string.Empty;
        itemIcon.sprite = null;
        itemIcon.gameObject.SetActive(false);
    }
}
