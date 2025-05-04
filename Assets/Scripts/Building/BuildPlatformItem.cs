using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildPlatformItem : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image itemIcon;

    private RectTransform _rectTransform;  // 添加缓存的RectTransform

    public event Action<BuildingConfig> OnItemClicked;

    public BuildingConfig CurrentItemConfig { get; private set; }

    public bool IsPointerOver { get; private set; }

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(BuildingConfig buildingConfig)
    {
        this.CurrentItemConfig = buildingConfig;

        if (this.CurrentItemConfig != null)
        {
            if (!string.IsNullOrEmpty(CurrentItemConfig.iconPath) && CurrentItemConfig.iconPath != "0")
            {
                itemIcon.sprite = Resources.Load<Sprite>(CurrentItemConfig.iconPath);
            }
            else
            {
                itemIcon.sprite = Resources.Load<Sprite>(Path.Combine("Icon", "icon_item_unknown"));
            }

            itemIcon.gameObject.SetActive(true);
        }
        else
        {
            Clear();
        }
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

    public void OnPointerClick(PointerEventData eventData)
    {
        // 处理点击事件
        if (CurrentItemConfig == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnItemClicked?.Invoke(CurrentItemConfig);
        }
    }

    /// <summary>
    /// 更新并显示物品提示
    /// </summary>
    public void UpdateTips(BuildingConfig item)
    {
        if (!IsPointerOver) return; // 只在鼠标悬停时才显示提示

        var tipsUI = GlobalUIMgr.Instance.Show<SimpleTipsUI>(GlobalUILayer.TooltipLayer);

        tipsUI.SetContent($"{item.name}\r\n\r\n{item.desc}");

        var sizeDelta = _rectTransform.sizeDelta;
        sizeDelta.x /= 2;
        // 直接使用RectTransform的position，因为UI元素的position已经是屏幕空间的坐标
        tipsUI.UpdatePosition(_rectTransform.position, sizeDelta);
    }

    public void Clear()
    {
        CurrentItemConfig = null;

        // 重置UI显示
        itemIcon.sprite = null;
        itemIcon.gameObject.SetActive(false);
    }
}
