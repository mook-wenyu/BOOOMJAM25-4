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
        InventoryMgr.GetPlayerInventoryData().OnInventoryChanged += UpdateInventoryUI;
        InventoryMgr.GetPlayerInventoryData().OnSlotCountChanged += CreateItemSlots;

        // 初始显示
        CreateItemSlots();
        UpdateInventoryUI();

        Hide();
        Show();
    }

    // 创建固定数量的物品槽
    private void CreateItemSlots()
    {
        int currentCount = _activeSlots.Count;
        int targetCount = InventoryMgr.GetPlayerInventoryData().capacity;

        // 创建新的物品槽
        for (int i = currentCount; i < targetCount; i++)
        {
            var newSlot = Instantiate(itemSlotPrefab, itemSlotContainer.content).GetComponent<ItemSlot>();
            newSlot.Clear();
            newSlot.InventoryId = InventoryMgr.GetPlayerInventoryData().inventoryId;
            newSlot.SlotIndex = i;
            newSlot.OnSlotClicked += OnItemSlotClicked;
            newSlot.OnSlotRightClicked += OnItemSlotRightClicked;
            // 订阅交换事件
            newSlot.OnSwapItems += HandleItemSwap;
            // 添加跨容器交换事件处理
            newSlot.OnCrossContainerSwap += HandleCrossContainerSwap;
            _activeSlots.Add(newSlot);
        }
    }

    // 更新背包显示
    private void UpdateInventoryUI()
    {
        // 确保有足够的插槽
        int currentSlotCount = InventoryMgr.GetPlayerInventoryData().capacity;
        if (_activeSlots.Count != currentSlotCount)
        {
            CreateItemSlots();
        }

        // 先清空所有插槽
        foreach (var slot in _activeSlots)
        {
            slot.Clear();
        }

        // 根据插槽顺序刷新所有插槽
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < _activeSlots.Count; i++)
        {
            var item = InventoryMgr.GetPlayerInventoryData().GetItemAtSlot(i);
            var slot = _activeSlots[i];

            if (item != null)
            {
                slot.Setup(item);
            }
            else
            {
                slot.Clear();
            }
        }
    }

    // 处理物品交换
    private void HandleItemSwap(ItemSlot fromSlot, ItemSlot toSlot)
    {
        if (fromSlot == null || toSlot == null) return;

        var inventory = InventoryMgr.GetPlayerInventoryData();
        
        if (toSlot.CurrentItem == null)
        {
            // 移动到空槽位
            InventoryHelper.MoveItemToEmptySlot(fromSlot, toSlot, inventory);
        }
        else
        {
            // 交换物品
            InventoryHelper.SwapItems(fromSlot, toSlot, inventory);
        }
    }

    // 处理跨容器物品交换
    private void HandleCrossContainerSwap(ItemSlot fromSlot, ItemSlot toSlot, string fromInventoryId, string toInventoryId)
    {
        if (fromSlot == null || toSlot == null) return;

        var fromInventory = InventoryMgr.GetInventoryData(fromInventoryId);
        var toInventory = InventoryMgr.GetInventoryData(toInventoryId);

        if (fromInventory == null || toInventory == null)
        {
            Debug.LogError("无效的容器ID");
            return;
        }

        // 如果目标是仓库，检查仓库类型限制
        if (toInventory.GetInventoryType() == InventoryType.Warehouse && toInventory is WarehouseData toWarehouse)
        {
            if (!CheckWarehouseTypeRestriction(toWarehouse, fromSlot.CurrentItem))
            {
                Debug.Log("该物品不能存放在此类型的仓库中");
                return;
            }
        }

        // 如果是交换，且源是仓库，还需要检查源仓库的类型限制
        if (toSlot.CurrentItem != null && 
            fromInventory.GetInventoryType() == InventoryType.Warehouse && 
            fromInventory is WarehouseData fromWarehouse)
        {
            if (!CheckWarehouseTypeRestriction(fromWarehouse, toSlot.CurrentItem))
            {
                Debug.Log("该物品不能存放在源仓库中");
                return;
            }
        }

        bool success;
        if (toSlot.CurrentItem == null)
        {
            success = InventoryHelper.MoveCrossContainerItem(
                fromInventory, toInventory,
                fromSlot, toSlot,
                fromSlot.SlotIndex,
                toSlot.SlotIndex
            );
        }
        else
        {
            success = InventoryHelper.SwapCrossContainerItems(
                fromInventory, toInventory,
                fromSlot, toSlot,
                fromSlot.SlotIndex,
                toSlot.SlotIndex
            );
        }

        if (success)
        {
            UpdateInventoryUI();
        }
    }

    // 检查仓库类型限制
    private bool CheckWarehouseTypeRestriction(WarehouseData warehouse, InventoryItem item)
    {
        if (item == null) return true;

        var itemConfig = InventoryMgr.GetItemConfig(item.itemId);
        if (itemConfig == null) return false;

        // 根据仓库类型检查物品是否可以存放
        switch (warehouse.warehouseType)
        {
            case WarehouseType.Box:
                return true; // 通用仓库可以存放所有物品
            case WarehouseType.IceBox:
                return itemConfig.type != (int)ItemType.Equipment;
            default:
                return false;
        }
    }

    // 处理物品插槽点击
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

    // 处理物品插槽右键点击
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
                InventoryMgr.GetPlayerInventoryData().RemoveItem(item.itemId, count);
            });*/

            InventoryMgr.GetPlayerInventoryData().UseItem(CharacterMgr.Player(), item.instanceId);
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
        InventoryMgr.GetPlayerInventoryData().OnInventoryChanged -= UpdateInventoryUI;
        InventoryMgr.GetPlayerInventoryData().OnSlotCountChanged -= CreateItemSlots;

        // 清理所有物品的事件监听
        foreach (var slot in _activeSlots)
        {
            if (slot != null)
            {
                slot.OnSwapItems -= HandleItemSwap;
            }
        }
    }

}
