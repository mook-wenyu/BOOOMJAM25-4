using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RequiredItemSlot : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI countText;

    private RectTransform _rectTransform;  // 添加缓存的RectTransform

    public string itemId;
    public int count;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(string itemId, int count)
    {
        this.itemId = itemId;
        this.count = count;

        var itemData = InventoryMgr.GetItemConfig(itemId);
        if (!string.IsNullOrEmpty(itemData.path) && itemData.path != "0")
        {
            itemIcon.sprite = Resources.Load<Sprite>(itemData.path);
        }
        else
        {
            itemIcon.sprite = Resources.Load<Sprite>(Path.Combine("Icon", "icon_item_unknown"));
        }

        UpdateCount();
    }

    public void UpdateCount()
    {
        if (countText != null)
        {
            var itemCount = InventoryMgr.GetPlayerInventoryData().GetItemCount(itemId);
            countText.color = itemCount >= this.count ? Color.green : Color.red;
            countText.text = $"{itemCount}/{this.count}";
        }
    }

    public void Clear()
    {
        itemId = null;
        count = 0;

        countText.text = string.Empty;
        itemIcon.sprite = null;
        itemIcon.gameObject.SetActive(false);
    }
}
