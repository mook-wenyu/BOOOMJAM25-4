using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIPanel : MonoBehaviour
{
    public GameObject inventoryPanel;

    public ScrollRect itemSlotContainer;
    public GameObject itemSlotPrefab;

    private List<ItemSlot> _activeSlots;
    private ItemSlot _selectedSlot;

    void Awake()
    {
        // 清除所有物品槽
        itemSlotContainer.content.DestroyAllChildren();
        _activeSlots = new List<ItemSlot>();

        // 注册事件
        InventoryMgr.GetInventoryData().OnInventoryChanged += UpdateInventoryUI;
        InventoryMgr.GetInventoryData().OnSlotCountChanged += CreateItemSlots;

        // 初始显示
        CreateItemSlots();
        UpdateInventoryUI();

        Hide();
        Show();
    }

    // 创建固定数量的物品槽
    private void CreateItemSlots()
    {
        int targetCount = InventoryMgr.SLOT_COUNT_DEFAULT;
        int currentCount = _activeSlots.Count;

        if (currentCount > targetCount)
        {
            // 如果当前槽位数量过多，移除多余的槽位
            for (int i = currentCount - 1; i >= targetCount; i--)
            {
                var slot = _activeSlots[i];
                if (slot != null)
                {
                    // 如果槽位有物品，先取消注册点击事件
                    if (slot.CurrentItem != null)
                    {
                        slot.OnSlotClicked -= OnItemSlotClicked;
                        slot.OnSlotRightClicked -= OnItemSlotRightClicked;
                    }
                    slot.Clear();
                    Destroy(slot.gameObject);
                }
                _activeSlots.RemoveAt(i);
            }
        }
        else if (currentCount < targetCount)
        {
            // 如果当前槽位不足，添加新槽位
            for (int i = currentCount; i < targetCount; i++)
            {
                var slotObj = Instantiate(itemSlotPrefab, itemSlotContainer.content);
                var slot = slotObj.GetComponent<ItemSlot>();
                slot.Clear(); // 确保槽位初始状态为空
                _activeSlots.Add(slot);
            }
        }

#if UNITY_EDITOR
        Debug.Log($"创建物品槽: {currentCount} -> {targetCount}");
#endif
    }

    // 更新背包显示
    private void UpdateInventoryUI()
    {
        var items = InventoryMgr.GetInventoryData().Items;

        // 确保有足够的槽位
        int currentSlotCount = InventoryMgr.GetInventoryData().CurrentSlotCount;
        if (_activeSlots.Count != currentSlotCount)
        {
            CreateItemSlots();
        }

        // 先清空所有槽位，并取消所有事件注册
        foreach (var slot in _activeSlots)
        {
            if (slot.CurrentItem != null)
            {
                slot.OnSlotClicked -= OnItemSlotClicked;
                slot.OnSlotRightClicked -= OnItemSlotRightClicked;
                // 取消对物品数量变化的监听
                slot.CurrentItem.OnCountChanged -= OnItemCountChanged;
            }
            slot.Clear();
        }

        // 将物品放入对应槽位
        int index = 0;
        foreach (var item in items.Values)
        {
            if (index < _activeSlots.Count)
            {
                var slot = _activeSlots[index];
                slot.Setup(item);
                // 注册点击事件
                slot.OnSlotClicked += OnItemSlotClicked;
                slot.OnSlotRightClicked += OnItemSlotRightClicked;
                // 注册物品数量变化事件
                item.OnCountChanged += OnItemCountChanged;
                index++;
            }
        }
    }

    // 处理物品槽点击
    private void OnItemSlotClicked(InventoryItem item)
    {
        if (item == null) return;

        var slot = _activeSlots.Find(s => s.CurrentItem == item);
        if (slot != null)
        {
            // 取消之前的选中状态
            _selectedSlot?.SetSelected(false);

            // 更新选中状态
            _selectedSlot = slot;
            slot.SetSelected(true);

            // TODO: 显示物品详情或其他操作
#if UNITY_EDITOR
            Debug.Log($"左键点击物品: {item.instanceId} -> {item.itemId} -> {item.GetCount()} -> {item.GetItemData().name}");
#endif
        }
    }

    // 处理物品槽右键点击
    private void OnItemSlotRightClicked(InventoryItem item)
    {
        if (item == null) return;

        var slot = _activeSlots.Find(s => s.CurrentItem == item);
        if (slot != null)
        {
            // 取消之前的选中状态
            _selectedSlot?.SetSelected(false);

            // 更新选中状态
            _selectedSlot = slot;
            slot.SetSelected(true);

            // TODO: 在这里添加右键菜单或其他操作
            // 例如：使用物品、丢弃物品等
            /*GlobalUIMgr.Instance.ShowItemActionPopup(item, "使用", (count) =>
            {
                Debug.Log($"使用物品: {item.instanceId} -> {item.itemId} -> {item.GetItemData().name} x {count}");

                // 使用物品
                InventoryMgr.GetInventoryData().RemoveInventoryItem(item.itemId, count);
            });*/

            InventoryMgr.GetInventoryData().UseItem(item.instanceId);
        }
    }

    // 物品数量变化的处理方法
    private void OnItemCountChanged(InventoryItem item)
    {
        // 找到对应的槽位并更新显示
        var slot = _activeSlots.Find(s => s.CurrentItem == item);
        if (slot != null)
        {
            // 如果物品数量为0，清空槽位
            if (item.GetCount() <= 0)
            {
                slot.OnSlotClicked -= OnItemSlotClicked;
                slot.OnSlotRightClicked -= OnItemSlotRightClicked;
                item.OnCountChanged -= OnItemCountChanged;
                slot.Clear();
            }
            else
            {
                // 更新槽位显示
                slot.UpdateCount(item.GetCount());
            }
        }
    }

    // 显示背包UI
    public void Show()
    {
        inventoryPanel.SetActive(true);
        UpdateInventoryUI();
    }

    // 隐藏背包UI
    public void Hide()
    {
        inventoryPanel.SetActive(false);
        // 取消选中状态
        if (_selectedSlot != null)
        {
            _selectedSlot.SetSelected(false);
            _selectedSlot = null;
        }
    }

    private void OnDestroy()
    {
        // 取消注册事件
        InventoryMgr.GetInventoryData().OnInventoryChanged -= UpdateInventoryUI;
        InventoryMgr.GetInventoryData().OnSlotCountChanged -= CreateItemSlots;

        // 清理所有物品的事件监听
        foreach (var slot in _activeSlots)
        {
            if (slot?.CurrentItem != null)
            {
                slot.CurrentItem.OnCountChanged -= OnItemCountChanged;
            }
        }
    }
}
