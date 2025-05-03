using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterStatusUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{

    private RectTransform _rectTransform;  // 添加缓存的RectTransform

    public bool IsPointerOver { get; private set; }

    private string _tipText;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(string tipText)
    {
        _tipText = tipText;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 即使没有物品也要设置指针状态，因为这关系到提示框的显示逻辑
        IsPointerOver = true;

        // 如果有物品，则显示提示
        if (!string.IsNullOrEmpty(_tipText))
        {
            UpdateTips(_tipText);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOver = false;
        var tipsUI = GlobalUIMgr.Instance.Get<SimpleTipsUI>();

        if (!IsPointerOver && tipsUI != null && tipsUI.gameObject.activeSelf)
        {
            GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
        }
    }

    /// <summary>
    /// 更新并显示物品提示
    /// </summary>
    public void UpdateTips(string content)
    {
        if (!IsPointerOver) return; // 只在鼠标悬停时才显示提示

        var itemTipsUI = GlobalUIMgr.Instance.Show<SimpleTipsUI>(GlobalUILayer.TooltipLayer);
        itemTipsUI.SetContent(content);

        // 直接使用RectTransform的position，因为UI元素的position已经是屏幕空间的坐标
        itemTipsUI.UpdatePosition(_rectTransform.position, _rectTransform.sizeDelta);
    }
}
