using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarehouseUIPanel : MonoSingleton<WarehouseUIPanel>
{
    public GameObject uiPanel;

    public TextMeshProUGUI titleName;

    public Button closeBtn;

    public ScrollRect itemSlotContainer;
    public GameObject itemSlotPrefab;

    private List<ItemSlot> _activeSlots;
    private ItemSlot _selectedSlot;

    private string _buildingInstanceId;
    private WarehouseBuildingData _warehouseBuildingData;
    private WarehouseData _warehouseData;

    void Awake()
    {
        closeBtn.onClick.AddListener(() => { AudioMgr.Instance.PlaySound("点击关闭"); Hide(); });
        // 清除所有物品槽
        itemSlotContainer.content.DestroyAllChildren();
        _activeSlots = new List<ItemSlot>();

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

    // 创建固定数量的物品槽
    private void CreateItemSlots()
    {
        int currentCount = _activeSlots.Count;
        int targetCount = _warehouseData.capacity;

        // 如果目标数量小于当前数量，需要移除多余的物品槽
        if (targetCount < currentCount)
        {
            for (int i = currentCount - 1; i >= targetCount; i--)
            {
                var slot = _activeSlots[i];
                // 取消订阅事件
                slot.OnSlotClicked -= OnItemSlotClicked;
                slot.OnSlotRightClicked -= OnItemSlotRightClicked;
                slot.OnSwapItems -= HandleItemSwap;
                slot.OnCrossContainerSwap -= HandleCrossContainerSwap;
                // 销毁游戏对象
                Destroy(slot.gameObject);
                // 从列表中移除
                _activeSlots.RemoveAt(i);
            }
        }

        // 创建新的物品槽
        for (int i = currentCount; i < targetCount; i++)
        {
            var newSlot = Instantiate(itemSlotPrefab, itemSlotContainer.content).GetComponent<ItemSlot>();
            newSlot.Clear();
            newSlot.InventoryId = _warehouseData.inventoryId;
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

    // 更新仓库显示
    private void UpdateUI()
    {
        // 确保有足够的插槽
        int currentSlotCount = _warehouseData.capacity;
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
            var item = _warehouseData.GetItemAtSlot(i);
            var slot = _activeSlots[i];

            // 确保每个槽位都有正确的InventoryId和SlotIndex
            slot.InventoryId = _warehouseData.inventoryId;
            slot.SlotIndex = i;

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

        if (toSlot.CurrentItem == null)
        {
            // 移动到空槽位
            InventoryHelper.MoveItemToEmptySlot(fromSlot, toSlot, _warehouseData);
        }
        else
        {
            // 交换物品
            InventoryHelper.SwapItems(fromSlot, toSlot, _warehouseData);
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

        // 检查仓库类型限制
        if (toInventory is WarehouseData toWarehouse)
        {
            if (!CheckWarehouseTypeRestriction(toWarehouse, fromSlot.CurrentItem))
            {
                GlobalUIMgr.Instance.ShowMessage("该物品不能存放在此类型的仓库中");
                return;
            }
        }

        // 如果是交换，还需要检查源仓库的类型限制
        if (toSlot.CurrentItem != null && fromInventory is WarehouseData fromWarehouse)
        {
            if (!CheckWarehouseTypeRestriction(fromWarehouse, toSlot.CurrentItem))
            {
                GlobalUIMgr.Instance.ShowMessage("该物品不能存放在源仓库中");
                return;
            }
        }

        // 如果是装备，则卸下装备
        if (fromSlot.CurrentItem != null && fromSlot.CurrentItem.GetItemType() == ItemType.Equipment)
        {
            if (fromSlot.CurrentItem.isEquipped)
            {
                // 卸下装备
                InventoryMgr.GetPlayerInventoryData().UnequipItem(CharacterMgr.Player(), fromSlot.CurrentItem.instanceId);
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
            UpdateUI();
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

            // 如果是装备，则卸下装备
            if (item.GetItemType() == ItemType.Equipment)
            {
                if (item.isEquipped)
                {
                    // 卸下装备
                    InventoryMgr.GetPlayerInventoryData().UnequipItem(CharacterMgr.Player(), item.instanceId);
                }
            }

            // 尝试将物品添加到背包中，如果背包已满，则提示用户背包已满
            int count = InventoryMgr.GetPlayerInventoryData().CalculateCanAddItem(item.itemId, item.GetCount()); // 计算能装下物品的数量
            if (count > 0)
            {
                InventoryMgr.GetPlayerInventoryData().AddItem(item.itemId, count); // 添加物品到背包中
                _warehouseData.RemoveItemCountByInstanceId(item.instanceId, count); // 仓库中移除物品实例
            }
            else
            {
                GlobalUIMgr.Instance.ShowMessage("背包已满");
            }
        }
    }

    public WarehouseData GetWarehouseData()
    {
        return _warehouseData;
    }

    public void Show(string buildingInstanceId)
    {
        this._buildingInstanceId = buildingInstanceId;
        this._warehouseBuildingData = BuildingMgr.GetBuildingData<WarehouseBuildingData>(buildingInstanceId);
        if (this._warehouseBuildingData != null)
        {
            titleName.text = this._warehouseBuildingData.GetBuildingConfig().name;
        }

        this._warehouseData = this._warehouseBuildingData.GetWarehouseData();
        if (this._warehouseData != null)
        {
            this._warehouseData.OnInventoryChanged += UpdateUI;
        }

        GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
        // 显示仓库UI
        uiPanel.SetActive(true);
        uiPanel.transform.SetAsLastSibling();
        // 初始显示
        UpdateUI();
    }

    public void ShowWarehouse(string instanceId, bool isExplore = false)
    {
        this._buildingInstanceId = string.Empty;
        this._warehouseData = InventoryMgr.GetWarehouseData(instanceId);
        if (this._warehouseData != null)
        {
            titleName.text = this._warehouseData.wName;
            this._warehouseData.OnInventoryChanged += UpdateUI;
        }

        // 显示仓库UI
        uiPanel.SetActive(true);
        uiPanel.transform.SetAsLastSibling();
        // 初始显示
        UpdateUI();
    }

    public void Hide()
    {
        if (this._warehouseData != null)
        {
            this._warehouseData.OnInventoryChanged -= UpdateUI;
        }
        uiPanel.transform.SetAsFirstSibling();
        uiPanel.SetActive(false);
    }

}
