using UnityEngine;

public static class InventoryHelper
{
    /// <summary>
    /// 在不同容器间移动物品
    /// </summary>
    public static bool MoveCrossContainerItem(BaseInventoryData fromInventory, BaseInventoryData toInventory,
        ItemSlot fromSlot, ItemSlot toSlot, int fromSlotIndex, int toSlotIndex)
    {
        // 验证容器和物品是否存在
        if (fromInventory == null || toInventory == null) return false;

        // 获取物品实例但不从源容器移除
        var item = fromSlot.CurrentItem;
        if (item == null) return false;

        // 检查目标容器是否有空间
        if (!toInventory.HasAvailableSlot())
        {
            Debug.Log("目标容器已满");
            return false;
        }

        // 从源容器移除物品
        item = fromInventory.RemoveItemInstance(item.instanceId);
        if (item == null) return false;

        // 移动到目标容器
        bool success = toInventory.MoveItemInstance(item);
        if (!success)
        {
            // 移动失败，恢复原始状态
            fromInventory.MoveItemInstance(item);
            fromInventory.SetSlot(fromSlotIndex, item);  // 恢复原始槽位
            Debug.Log("物品移动失败");
            return false;
        }

        // 设置槽位
        fromInventory.SetSlot(fromSlotIndex, null);  // 清空原始槽位
        toInventory.SetSlot(toSlotIndex, item);      // 设置新槽位

        // 更新UI显示
        toSlot.Setup(item);
        if (toSlot.CurrentItem != null)
        {
            toSlot.UpdateTips(toSlot.CurrentItem);
        }
        fromSlot.Clear();

        return true;
    }

    /// <summary>
    /// 在不同容器间交换物品
    /// </summary>
    public static bool SwapCrossContainerItems(BaseInventoryData fromInventory, BaseInventoryData toInventory,
        ItemSlot fromSlot, ItemSlot toSlot, int fromSlotIndex, int toSlotIndex)
    {
        // 验证容器和物品是否存在
        if (fromInventory == null || toInventory == null) return false;

        // 先获取物品实例但不移除
        var fromItem = fromSlot.CurrentItem;
        var toItem = toSlot.CurrentItem;

        if (fromItem == null || toItem == null) return false;

        // 从源容器移除物品
        fromItem = fromInventory.RemoveItemInstance(fromItem.instanceId);
        toItem = toInventory.RemoveItemInstance(toItem.instanceId);

        if (fromItem == null || toItem == null)
        {
            // 恢复原始状态
            if (fromItem != null)
            {
                fromInventory.MoveItemInstance(fromItem);
                fromInventory.SetSlot(fromSlotIndex, fromItem);
            }
            if (toItem != null)
            {
                toInventory.MoveItemInstance(toItem);
                toInventory.SetSlot(toSlotIndex, toItem);
            }
            return false;
        }

        // 交换物品实例
        bool success1 = toInventory.MoveItemInstance(fromItem);
        bool success2 = fromInventory.MoveItemInstance(toItem);

        if (!success1 || !success2)
        {
            // 交换失败，恢复原始状态
            if (success1)
            {
                toInventory.RemoveItemInstance(fromItem.instanceId);
            }
            fromInventory.MoveItemInstance(fromItem);
            fromInventory.SetSlot(fromSlotIndex, fromItem);
            toInventory.MoveItemInstance(toItem);
            toInventory.SetSlot(toSlotIndex, toItem);
            Debug.Log("物品交换失败");
            return false;
        }

        // 设置槽位
        fromInventory.SetSlot(fromSlotIndex, toItem);
        toInventory.SetSlot(toSlotIndex, fromItem);

        // 更新UI显示
        fromSlot.Setup(toItem);
        toSlot.Setup(fromItem);
        if (toSlot.CurrentItem != null)
        {
            toSlot.UpdateTips(toSlot.CurrentItem);
        }

        return true;
    }

    /// <summary>
    /// 在同一容器内交换物品
    /// </summary>
    public static void SwapItems(ItemSlot fromSlot, ItemSlot toSlot, BaseInventoryData inventory)
    {
        if (fromSlot == null || toSlot == null || inventory == null) return;

        // 保存当前物品的引用
        var fromTempItem = fromSlot.CurrentItem;
        var toTempItem = toSlot.CurrentItem;

        // 设置源槽位的物品为目标槽位的物品
        fromSlot.Setup(toTempItem);

        // 设置目标槽位的物品为源槽位的物品
        toSlot.Setup(fromTempItem);
        // 重新注册事件
        if (toSlot.CurrentItem != null)
        {
            toSlot.UpdateTips(toSlot.CurrentItem);
        }

        // 通知容器管理器更新物品位置
        inventory.SetSlot(fromSlot.SlotIndex, toTempItem);
        inventory.SetSlot(toSlot.SlotIndex, fromTempItem);
    }

    /// <summary>
    /// 在同一容器内移动物品到空槽位
    /// </summary>
    public static void MoveItemToEmptySlot(ItemSlot fromSlot, ItemSlot toSlot, BaseInventoryData inventory)
    {
        if (fromSlot == null || toSlot == null || inventory == null) return;

        var tempItem = fromSlot.CurrentItem;

        // 设置目标槽位的物品
        toSlot.Setup(tempItem);
        // 重新注册事件
        if (toSlot.CurrentItem != null)
        {
            toSlot.UpdateTips(toSlot.CurrentItem);
        }

        // 清空源槽位
        fromSlot.Clear();

        // 通知容器管理器更新物品位置
        inventory.SetSlot(fromSlot.SlotIndex, null);
        inventory.SetSlot(toSlot.SlotIndex, tempItem);
    }
}
