using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildPlatformItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image itemIcon;

    private RectTransform _rectTransform;  // 添加缓存的RectTransform

    public event Action<BuildingConfig> OnItemClicked;

    public BuildingConfig CurrentItemConfig { get; private set; }

    public bool IsPointerOver { get; private set; }

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(BuildingConfig buildingConfig)
    {
        this.CurrentItemConfig = buildingConfig;

        if (this.CurrentItemConfig != null)
        {
            if (!string.IsNullOrEmpty(CurrentItemConfig.path) && CurrentItemConfig.path != "0")
            {
                itemIcon.sprite = Resources.Load<Sprite>(CurrentItemConfig.path);
            }
            else
            {
                itemIcon.sprite = Resources.Load<Sprite>(Path.Combine("Icon", "icon_item_unknown"));
            }

            itemIcon.gameObject.SetActive(true);
        }
        else
        {
            Clear();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 处理点击事件
        if (CurrentItemConfig == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnItemClicked?.Invoke(CurrentItemConfig);
        }
    }

    public void Clear()
    {
        CurrentItemConfig = null;

        // 重置UI显示
        itemIcon.sprite = null;
        itemIcon.gameObject.SetActive(false);
    }
}
