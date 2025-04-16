using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour,
IBeginDragHandler,
IDragHandler,
IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    public Image background;
    public Image itemIcon;
    public TextMeshProUGUI countText;

    public InventoryItem CurrentItem { get; private set; }
    public bool IsPointerOver { get; private set; }

    private GameObject _dragItemClone;    // 拖动时的物品副本
    private RectTransform _dragRectTransform;
    private CanvasGroup _dragCanvasGroup;

    private RectTransform _rectTransform;  // 添加缓存的RectTransform

    public event Action<InventoryItem> OnSlotClicked;
    public event Action<InventoryItem> OnSlotRightClicked;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(InventoryItem newItem)
    {
        CurrentItem = newItem;

        if (CurrentItem != null)
        {
            var itemData = InventoryMgr.GetItemData(CurrentItem.itemId);
            if (itemData.iconPath != null)
            {
                itemIcon.sprite = Resources.Load<Sprite>(Path.Combine("Icon", itemData.iconPath));
            }
            else
            {
                itemIcon.sprite = Resources.Load<Sprite>(Path.Combine("Icon", "icon_item_unknown"));
            }

            // 更新数量显示
            UpdateCount();

            // 确保图标可见
            itemIcon.gameObject.SetActive(true);
        }
    }

    public void UpdateCount()
    {
        countText.text = CurrentItem.Count > 1 ? CurrentItem.Count.ToString() : string.Empty;
    }

    public void UpdateCount(int newCount)
    {
        if (countText != null)
        {
            countText.text = newCount > 1 ? newCount.ToString() : string.Empty;
        }
    }

    public void SetSelected(bool selected)
    {
        background.color = selected ? new Color(0.8f, 0.8f, 0.8f, 1) : Color.white;
    }

    /// <summary>
    /// 更新并显示物品提示
    /// </summary>
    public void UpdateTips(InventoryItem item)
    {
        if (!IsPointerOver) return; // 只在鼠标悬停时才显示提示

        var itemTipsUI = GlobalUIMgr.Instance.Show<ItemTipsUI>(GlobalUILayer.TooltipLayer);
        itemTipsUI.SetItemInfo(item, this);

        // 直接使用RectTransform的position，因为UI元素的position已经是屏幕空间的坐标
        itemTipsUI.UpdatePosition(_rectTransform.position, _rectTransform.sizeDelta);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 如果没有物品，直接返回
        if (CurrentItem == null) return;

        // 开始拖动时隐藏所有tips
        GlobalUIMgr.Instance.Hide<ItemTipsUI>();

        // 创建拖动时的物品副本
        _dragItemClone = gameObject.InstantiateToLayer(GlobalUILayer.TooltipLayer);
        _dragRectTransform = _dragItemClone.GetComponent<RectTransform>();
        _dragRectTransform.sizeDelta = GetComponent<RectTransform>().sizeDelta;
        _dragCanvasGroup = _dragItemClone.AddComponent<CanvasGroup>();

        // 设置副本属性
        _dragCanvasGroup.blocksRaycasts = false;  // 禁用射线检测，使其不影响下方物品
        _dragCanvasGroup.alpha = 0.9f;            // 设置半透明

        // 将副本移动到最顶层
        _dragItemClone.transform.SetAsLastSibling();

        // 禁用副本上的所有组件，只保留显示功能
        var components = _dragItemClone.GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            if (component.GetType() != typeof(Image) && component.GetType() != typeof(CanvasGroup))
            {
                Destroy(component);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 如果没有物品或没有拖拽副本，直接返回
        if (CurrentItem == null || _dragRectTransform == null) return;

        _dragRectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 如果没有物品，直接返回
        if (CurrentItem == null) return;

        // 检查是否拖放到了有效的目标上
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool handled = false;
        bool isInInventory = false;

        foreach (var result in results)
        {
            // 检查是否在背包面板内
            if (result.gameObject.name == "InventoryPanel")
            {
                isInInventory = true;
                break;
            }
        }

        // 如果不在背包面板内,则丢弃物品
        if (!isInInventory)
        {
            // 显示确认对话框
            GlobalUIMgr.Instance.ShowItemActionPopup(CurrentItem, "丢弃",
                (count) =>
                {
                    // 确认丢弃
                    string itemName = CurrentItem.GetItemData().itemName;
                    bool result = InventoryMgr.GetInventoryData().RemoveItemCountByInstanceId(CurrentItem.instanceId, count);
                    if (result)
                    {
                        handled = true;
                        GlobalUIMgr.Instance.ShowMessage($"丢弃 {itemName} x {count}");
                    }
                    else
                    {
                        GlobalUIMgr.Instance.ShowMessage($"丢弃失败");
                    }
                }
            );
        }

        // 如果没有处理过，添加一个无效放置的动画效果
        if (!handled)
        {
            var rectTransform = GetComponent<RectTransform>();
            // Tween.UIAnchoredPosition(rectTransform, rectTransform.anchoredPosition, 0.2f, Ease.OutBounce);
        }

        // 销毁拖动副本
        if (_dragItemClone != null)
        {
            Destroy(_dragItemClone);
        }

        // 重新显示tips
        UpdateTips(CurrentItem);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 即使没有物品也要设置指针状态，因为这关系到提示框的显示逻辑
        IsPointerOver = true;

        // 只有在有物品时才显示提示
        if (CurrentItem != null)
        {
            UpdateTips(CurrentItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOver = false;
        var itemTipsUI = GlobalUIMgr.Instance.Get<ItemTipsUI>();

        if (!IsPointerOver && itemTipsUI != null && itemTipsUI.gameObject.activeSelf)
        {
            GlobalUIMgr.Instance.Hide<ItemTipsUI>();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 确保物品存在
        if (CurrentItem != null)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // 左键点击
                OnSlotClicked?.Invoke(CurrentItem);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // 右键点击
                OnSlotRightClicked?.Invoke(CurrentItem);
            }
        }

        // 阻止事件继续传播
        eventData.Use();
    }

    public void Clear()
    {
        CurrentItem = null;
        itemIcon.sprite = null;
        itemIcon.gameObject.SetActive(false);
        countText.text = "";
        SetSelected(false);
    }

    void OnDestroy()
    {
        if (_dragItemClone != null)
        {
            Destroy(_dragItemClone);
            _dragItemClone.transform.SetParent(null);
        }
        _dragRectTransform = null;
        _dragCanvasGroup = null;

        GlobalUIMgr.Instance.Hide<ItemTipsUI>();
    }


}