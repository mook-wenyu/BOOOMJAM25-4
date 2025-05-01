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

    [SerializeField] private Image background;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI countText;

    [SerializeField] private GameObject durabilityBar; // 耐久度条
    [SerializeField] private Image durabilityFill; // 耐久度填充

    private GameObject _dragItemClone;    // 拖动时的物品副本
    private RectTransform _dragRectTransform;
    private CanvasGroup _dragCanvasGroup;

    private RectTransform _rectTransform;  // 添加缓存的RectTransform

    public event Action<InventoryItem> OnSlotClicked;
    public event Action<InventoryItem> OnSlotRightClicked;
    // 交换事件委托
    public event Action<ItemSlot, ItemSlot> OnSwapItems;
    // 用于跨容器交换的事件
    public event Action<ItemSlot, ItemSlot, string, string> OnCrossContainerSwap;

    // 容器ID属性
    public string InventoryId { get; set; }
    // 槽位索引属性
    public int SlotIndex { get; set; }
    public InventoryItem CurrentItem { get; private set; }
    public bool IsPointerOver { get; private set; }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(InventoryItem newItem)
    {
        // 移除旧物品的事件订阅
        if (CurrentItem != null)
        {
            UnsubscribeItemEvents();
        }

        CurrentItem = newItem;

        if (CurrentItem != null)
        {
            var itemData = InventoryMgr.GetItemConfig(CurrentItem.itemId);
            if (!string.IsNullOrEmpty(itemData.path) && itemData.path != "0")
            {
                itemIcon.sprite = Resources.Load<Sprite>(itemData.path);
            }
            else
            {
                itemIcon.sprite = Resources.Load<Sprite>(Path.Combine("Icon", "icon_item_unknown"));
            }

            // 更新数量显示
            UpdateCount();

            // 确保图标可见
            itemIcon.gameObject.SetActive(true);

            // 设置耐久度条（如果是装备）
            UpdateDurabilityBar();

            // 订阅物品事件
            SubscribeItemEvents();
        }
        else
        {
            // 清空槽位
            Clear();
        }
    }

    private void SubscribeItemEvents()
    {
        if (CurrentItem == null) return;

        // 订阅物品数量变化事件
        CurrentItem.OnCountChanged += HandleItemCountChanged;

        // 如果是装备类型，订阅耐久度变化事件
        if (CurrentItem.GetItemType() == ItemType.Equipment)
        {
            CurrentItem.OnDurabilityChanged += HandleDurabilityChanged;
            CurrentItem.OnBroken += HandleItemBroken;
        }
    }

    private void UnsubscribeItemEvents()
    {
        if (CurrentItem == null) return;

        // 取消订阅物品数量变化事件
        CurrentItem.OnCountChanged -= HandleItemCountChanged;

        // 取消装备类型事件订阅
        if (CurrentItem.GetItemType() == ItemType.Equipment)
        {
            CurrentItem.OnDurabilityChanged -= HandleDurabilityChanged;
            CurrentItem.OnBroken -= HandleItemBroken;
        }
    }

    // 更新耐久度条
    private void UpdateDurabilityBar()
    {
        if (CurrentItem == null || durabilityBar == null || durabilityFill == null) return;

        var itemData = CurrentItem.GetItemData();
        if (itemData == null || itemData.type != (int)ItemType.Equipment || itemData.durability <= 0)
        {
            durabilityBar.SetActive(false);
            return;
        }

        durabilityBar.SetActive(true);

        float durabilityPercent = (float)(CurrentItem.GetDurability() / itemData.durability);
        durabilityFill.fillAmount = durabilityPercent;

        // 根据耐久度百分比设置颜色
        if (durabilityPercent > 0.6f)
        {
            durabilityFill.color = InventoryMgr.DurabilityGoodColor;
        }
        else if (durabilityPercent > 0.3f)
        {
            durabilityFill.color = InventoryMgr.DurabilityMediumColor;
        }
        else
        {
            durabilityFill.color = InventoryMgr.DurabilityLowColor;
        }
    }

    // 处理耐久度变化
    private void HandleDurabilityChanged(InventoryItem item)
    {
        if (item == CurrentItem)
        {
            UpdateDurabilityBar();
        }
    }

    // 处理物品破损
    private void HandleItemBroken(InventoryItem item)
    {
        if (item == CurrentItem)
        {
            // 物品破损效果
            itemIcon.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        }
    }

    // 处理物品数量变化
    private void HandleItemCountChanged(InventoryItem item)
    {
        if (item != CurrentItem) return;

        // 如果物品数量为0，清空槽位
        if (item.GetCount() <= 0)
        {
            Clear();
        }
        else
        {
            // 更新数量显示
            UpdateCount(item.GetCount());
        }
    }

    // 更新数量显示
    public void UpdateCount()
    {
        if (CurrentItem == null)
        {
            countText.text = string.Empty;
            return;
        }

        countText.text = CurrentItem.GetCount() > 1 ? CurrentItem.GetCount().ToString() : string.Empty;
    }

    public void UpdateCount(int newCount)
    {
        if (countText != null)
        {
            countText.text = newCount > 1 ? newCount.ToString() : string.Empty;
        }
    }

    // 设置选中状态
    public void SetSelected(bool selected)
    {
        if (background != null)
        {
            // 保存原始边框颜色
            Color originalColor = background.color;
            // 设置选中时的颜色（保留色调，增加亮度）
            background.color = selected ? new Color(
                Mathf.Min(originalColor.r * 1.2f, 1f),
                Mathf.Min(originalColor.g * 1.2f, 1f),
                Mathf.Min(originalColor.b * 1.2f, 1f)
            ) : originalColor;
        }
    }

    /// <summary>
    /// 更新并显示物品提示
    /// </summary>
    public void UpdateTips(InventoryItem item)
    {
        if (!IsPointerOver) return; // 只在鼠标悬停时才显示提示

        var itemTipsUI = GlobalUIMgr.Instance.Show<ItemTipsUI>(GlobalUILayer.TooltipLayer);
        itemTipsUI.SetItemInfo(item);

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
        bool isInValidArea = false;
        bool isInEquipment = false;

        foreach (var result in results)
        {
            // 检查是否拖到了另一个ItemSlot上
            var targetSlot = result.gameObject.GetComponent<ItemSlot>();
            if (targetSlot != null && targetSlot != this)
            {
                // 检查是否是跨容器操作
                if (InventoryId != targetSlot.InventoryId)
                {
                    // 触发跨容器交换事件
                    OnCrossContainerSwap?.Invoke(this, targetSlot, InventoryId, targetSlot.InventoryId);
                }
                else
                {
                    // 同容器内交换
                    OnSwapItems?.Invoke(this, targetSlot);
                }
                isInValidArea = true;
                handled = true;
                break;
            }

            // 检查是否在背包或仓库面板内
            if (result.gameObject.name is "InventoryBG" or "WarehouseBG")
            {
                isInValidArea = true;
                break;
            }

            // 检查是否在装备面板内
            if (result.gameObject.name == "EquipmentBG")
            {
                isInEquipment = true;
                break;
            }
        }

        // 如果不在背包或仓库面板内，则丢弃物品
        if (!isInValidArea && !isInEquipment)
        {
            // 显示确认对话框
            /*GlobalUIMgr.Instance.ShowItemActionPopup(CurrentItem, "丢弃",
                (count) =>
                {
                    // 确认丢弃
                    string itemName = CurrentItem.GetItemData().name;
                    bool result = InventoryMgr.GetInventoryData(InventoryId).RemoveItemCountByInstanceId(CurrentItem.instanceId, count);
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
            );*/
            int count = 1;
            string itemName = CurrentItem.GetItemData().name;
            bool result = InventoryMgr.GetInventoryData(InventoryId).RemoveItemCountByInstanceId(CurrentItem.instanceId, count);
            if (result)
            {
                handled = true;
                GlobalUIMgr.Instance.ShowMessage($"丢弃 {itemName} x {count}");
            }
            else
            {
                GlobalUIMgr.Instance.ShowMessage("丢弃失败");
            }
        }

        // 如果没有处理过，添加一个无效放置的动画效果
        if (!handled)
        {
            var rectTransform = GetComponent<RectTransform>();
            // Tween.UIAnchoredPosition(rectTransform, rectTransform.anchoredPosition, 0.2f, Ease.OutBounce);
        }

        if (isInEquipment && CurrentItem.GetItemType() == ItemType.Equipment
            && !CurrentItem.IsBroken() && !CurrentItem.isEquipped)
        {
            if (InventoryId == InventoryMgr.GetPlayerInventoryData().inventoryId)
            {
                // 装备物品
                InventoryMgr.GetPlayerInventoryData().EquipItem(CharacterMgr.Player(), CurrentItem.instanceId);
            }
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

        // 如果有物品，则显示提示
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
        // 处理点击事件
        if (CurrentItem != null)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnSlotClicked?.Invoke(CurrentItem);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnSlotRightClicked?.Invoke(CurrentItem);
                if (CurrentItem != null)
                {
                    // 重新显示tips
                    UpdateTips(CurrentItem);
                }
                else
                {
                    GlobalUIMgr.Instance.Hide<ItemTipsUI>();
                }
            }
        }
    }

    public void Clear()
    {
        // 取消订阅事件
        UnsubscribeItemEvents();

        // 清理拖拽副本
        if (_dragItemClone != null)
        {
            Destroy(_dragItemClone);
            _dragItemClone.transform.SetParent(null);
        }
        _dragRectTransform = null;
        _dragCanvasGroup = null;

        GlobalUIMgr.Instance.Hide<ItemTipsUI>();

        // 清空物品引用
        CurrentItem = null;

        // 重置UI显示
        itemIcon.sprite = null;
        itemIcon.gameObject.SetActive(false);
        countText.text = string.Empty;

        // 隐藏耐久度条
        if (durabilityBar != null)
        {
            durabilityBar.SetActive(false);
        }
    }

    void OnDisable()
    {
        // 确保取消所有事件订阅
        UnsubscribeItemEvents();

        // 清理拖拽副本
        if (_dragItemClone != null)
        {
            Destroy(_dragItemClone);
            _dragItemClone.transform.SetParent(null);
        }
        _dragRectTransform = null;
        _dragCanvasGroup = null;

        GlobalUIMgr.Instance.Hide<ItemTipsUI>();
    }

    void OnDestroy()
    {
        // 确保取消所有事件订阅
        UnsubscribeItemEvents();

        // 清理拖拽副本
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
