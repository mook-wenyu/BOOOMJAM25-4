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
    IPointerClickHandler,
    IDropHandler
{
    public Image background;
    public Image itemIcon;
    public TextMeshProUGUI countText;

    // 新增物品类型相关UI
    [SerializeField] private Image typeIndicator; // 物品类型指示器
    [SerializeField] private GameObject durabilityBar; // 耐久度条
    [SerializeField] private Image durabilityFill; // 耐久度填充

    // 不同物品类型的颜色
    [SerializeField] private Color materialColor = new Color(0.5f, 0.8f, 0.5f);
    [SerializeField] private Color consumableColor = new Color(0.8f, 0.5f, 0.5f);
    [SerializeField] private Color equipmentColor = new Color(0.5f, 0.5f, 0.8f);
    [SerializeField] private Color questColor = new Color(0.8f, 0.8f, 0.5f);
    [SerializeField] private Color currencyColor = new Color(0.8f, 0.7f, 0.2f);
    [SerializeField] private Color otherColor = new Color(0.7f, 0.7f, 0.7f);

    // 稀有度边框颜色
    [SerializeField]
    private Color[] rarityColors = new Color[]
    {
        new Color(0.7f, 0.7f, 0.7f), // 普通
        new Color(0.3f, 0.8f, 0.3f), // 优秀
        new Color(0.3f, 0.3f, 0.8f), // 精良
        new Color(0.8f, 0.3f, 0.8f), // 史诗
        new Color(1.0f, 0.6f, 0.0f)  // 传说
    };

    // 耐久度条颜色
    [SerializeField] private Color durabilityGoodColor = Color.green;
    [SerializeField] private Color durabilityMediumColor = Color.yellow;
    [SerializeField] private Color durabilityLowColor = Color.red;

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
        // 移除旧物品的事件订阅
        if (CurrentItem != null)
        {
            UnsubscribeItemEvents();
        }

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

            // 设置物品类型指示器
            UpdateTypeIndicator(itemData);

            // 设置稀有度边框
            UpdateRarityBorder(itemData);

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

        // 取消装备类型事件订阅
        if (CurrentItem.GetItemType() == ItemType.Equipment)
        {
            CurrentItem.OnDurabilityChanged -= HandleDurabilityChanged;
            CurrentItem.OnBroken -= HandleItemBroken;
        }
    }

    // 更新物品类型指示器
    private void UpdateTypeIndicator(ItemData itemData)
    {
        if (typeIndicator == null) return;

        typeIndicator.gameObject.SetActive(true);

        switch (itemData.itemType)
        {
            case ItemType.Material:
                typeIndicator.color = materialColor;
                break;
            case ItemType.Consumable:
                typeIndicator.color = consumableColor;
                break;
            case ItemType.Equipment:
                typeIndicator.color = equipmentColor;
                break;
            case ItemType.Quest:
                typeIndicator.color = questColor;
                break;
            case ItemType.Currency:
                typeIndicator.color = currencyColor;
                break;
            case ItemType.Other:
                typeIndicator.color = otherColor;
                break;
        }
    }

    // 更新稀有度边框
    private void UpdateRarityBorder(ItemData itemData)
    {
        if (background == null) return;

        int rarityIndex = Mathf.Clamp(itemData.rarity, 0, rarityColors.Length - 1);
        background.color = rarityColors[rarityIndex];
    }

    // 更新耐久度条
    private void UpdateDurabilityBar()
    {
        if (CurrentItem == null || durabilityBar == null || durabilityFill == null) return;

        var itemData = CurrentItem.GetItemData();
        if (itemData == null || itemData.itemType != ItemType.Equipment || itemData.durability <= 0)
        {
            durabilityBar.SetActive(false);
            return;
        }

        durabilityBar.SetActive(true);

        float durabilityPercent = (float)CurrentItem.CurrentDurability / itemData.durability;
        durabilityFill.fillAmount = durabilityPercent;

        // 根据耐久度百分比设置颜色
        if (durabilityPercent > 0.6f)
        {
            durabilityFill.color = durabilityGoodColor;
        }
        else if (durabilityPercent > 0.3f)
        {
            durabilityFill.color = durabilityMediumColor;
        }
        else
        {
            durabilityFill.color = durabilityLowColor;
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

    public void UpdateCount()
    {
        if (CurrentItem == null)
        {
            countText.text = string.Empty;
            return;
        }

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

        // 如果有物品，则显示提示
        if (CurrentItem != null)
        {
            UpdateTips(CurrentItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOver = false;

        // 隐藏提示
        GlobalUIMgr.Instance.Hide<ItemTipsUI>();
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

                // 右键使用物品
                if (CurrentItem.GetItemType() == ItemType.Consumable ||
                    CurrentItem.GetItemType() == ItemType.Equipment)
                {
                    InventoryMgr.UseItem(CurrentItem.instanceId);
                }
            }
        }
    }

    public void Clear()
    {
        // 取消事件订阅
        UnsubscribeItemEvents();

        CurrentItem = null;
        itemIcon.sprite = null;
        itemIcon.gameObject.SetActive(false);
        countText.text = string.Empty;

        // 重置类型指示器
        if (typeIndicator != null)
        {
            typeIndicator.gameObject.SetActive(false);
        }

        // 重置耐久度条
        if (durabilityBar != null)
        {
            durabilityBar.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // 确保取消所有事件订阅
        UnsubscribeItemEvents();

        // 清理拖拽副本
        if (_dragItemClone != null)
        {
            Destroy(_dragItemClone);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        ItemSlot droppedSlot = dropped.GetComponent<ItemSlot>();

        if (droppedSlot != null)
        {
            // 实现物品交换逻辑
            InventoryItem tempItem = CurrentItem;
            CurrentItem = droppedSlot.CurrentItem;
            droppedSlot.CurrentItem = tempItem;

            // 更新槽位显示
            Setup(CurrentItem);
            droppedSlot.Setup(droppedSlot.CurrentItem);
        }
    }
}