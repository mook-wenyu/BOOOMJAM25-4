using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProductionPlatformItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image itemIcon;

    public event Action<string> OnItemClicked;

    public string ItemId { get; private set; }

    public void Setup(string itemId)
    {
        Clear();

        this.ItemId = itemId;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 处理点击事件
        if (string.IsNullOrEmpty(ItemId)) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnItemClicked?.Invoke(ItemId);
        }
    }

    public void Clear()
    {
        ItemId = string.Empty;

        // 重置UI显示
        itemIcon.sprite = null;
        itemIcon.gameObject.SetActive(false);
    }
}
