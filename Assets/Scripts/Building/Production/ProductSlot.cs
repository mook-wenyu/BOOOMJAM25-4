using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProductSlot : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image itemIcon;

    private RectTransform _rectTransform;  // 添加缓存的RectTransform

    public event Action<ProductionData> OnSlotClicked;

    public ProductionData CurrentItem { get; private set; }
    public ItemConfig CurrentItemConfig { get; private set; }
    public bool IsPointerOver { get; private set; }

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(ProductionData item)
    {
        // 移除旧物品的事件订阅
        if (CurrentItem != null)
        {
            UnsubscribeEvents();
        }

        CurrentItem = item;

        if (CurrentItem != null)
        {
            CurrentItemConfig = InventoryMgr.GetItemConfig(CurrentItem.GetRecipe().productID[0]);
            if (!string.IsNullOrEmpty(CurrentItemConfig.path) && CurrentItemConfig.path != "0")
            {
                itemIcon.sprite = Resources.Load<Sprite>(CurrentItemConfig.path);
            }
            else
            {
                itemIcon.sprite = Resources.Load<Sprite>(Path.Combine("Icon", "icon_item_unknown"));
            }

            // 确保图标可见
            itemIcon.gameObject.SetActive(true);

            // 订阅事件
            SubscribeEvents();
        }
        else
        {
            // 清空槽位
            Clear();
        }
    }

    private void SubscribeEvents()
    {
        if (CurrentItem == null) return;

        CurrentItem.OnRecipeTimeChanged += HandleRecipeTimeChanged;
    }

    private void UnsubscribeEvents()
    {
        if (CurrentItem == null) return;

        CurrentItem.OnRecipeTimeChanged -= HandleRecipeTimeChanged;
    }

    private void HandleRecipeTimeChanged(ProductionData item)
    {
        if (item == CurrentItem)
        {
            // 更新剩余时间显示
            UpdateRemainingTime();
        }
    }

    private void UpdateRemainingTime()
    {
        // 在这里更新剩余时间的显示
    }

    /// <summary>
    /// 更新并显示物品提示
    /// </summary>
    public void UpdateTips(ItemConfig item)
    {
        if (!IsPointerOver) return; // 只在鼠标悬停时才显示提示

        var tipsUI = GlobalUIMgr.Instance.Show<SimpleTipsUI>(GlobalUILayer.TooltipLayer);

        tipsUI.SetContent(item.name);

        // 直接使用RectTransform的position，因为UI元素的position已经是屏幕空间的坐标
        tipsUI.UpdatePosition(_rectTransform.position, _rectTransform.sizeDelta);
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
        if (CurrentItem != null)
        {
            if (eventData.button == PointerEventData.InputButton.Left || eventData.button == PointerEventData.InputButton.Right)
            {
                if (CurrentItem.IsComplete())
                {
                    OnSlotClicked?.Invoke(CurrentItem);
                    GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
                }
                else
                {
                    GlobalUIMgr.Instance.ShowMessage("生产未完成！");
                }
            }
        }
    }

    public void Clear()
    {
        // 取消订阅事件
        UnsubscribeEvents();

        // 清空物品引用
        CurrentItem = null;
        CurrentItemConfig = null;

        // 重置UI显示
        itemIcon.sprite = null;
        itemIcon.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        // 确保取消所有事件订阅
        UnsubscribeEvents();

        GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
    }
}
