using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image bg;

    public ActiveBuff CurrentBuff { get; private set; }

    private RectTransform _rectTransform;  // 添加缓存的RectTransform

    private float _remainingTime;

    public bool IsPointerOver { get; private set; }

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(ActiveBuff buff)
    {
        // 移除旧物品的事件订阅
        if (CurrentBuff != null)
        {
            UnsubscribeEvents();
        }

        CurrentBuff = buff;

        var config = buff.GetConfig();
        if (config == null || string.IsNullOrEmpty(config.path))
        {
            bg.sprite = Resources.Load<Sprite>(Path.Combine("Icon", "icon_item_unknown"));
        }

        if (config.path == "0")
        {
            bg.sprite = Resources.Load<Sprite>(Path.Combine("UI", "buff"));
        }
        else if (config.path == "1")
        {
            bg.sprite = Resources.Load<Sprite>(Path.Combine("UI", "debuff"));
        }

        _remainingTime = buff.remainingTime;

        // 订阅事件
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (CurrentBuff == null) return;

        CurrentBuff.OnBuffTimeChanged += HandleBuffTimeChanged;
    }

    private void UnsubscribeEvents()
    {
        if (CurrentBuff == null) return;

        CurrentBuff.OnBuffTimeChanged -= HandleBuffTimeChanged;
    }

    private void HandleBuffTimeChanged(ActiveBuff buff)
    {
        if (buff == CurrentBuff)
        {
            _remainingTime = buff.remainingTime;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 即使没有物品也要设置指针状态，因为这关系到提示框的显示逻辑
        IsPointerOver = true;

        // 如果有物品，则显示提示
        if (CurrentBuff != null)
        {
            UpdateTips(CurrentBuff);
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
    public void UpdateTips(ActiveBuff item)
    {
        if (!IsPointerOver) return; // 只在鼠标悬停时才显示提示

        var config = item.GetConfig();

        var tipsUI = GlobalUIMgr.Instance.Show<SimpleTipsUI>(GlobalUILayer.TooltipLayer);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{config.name}");
        sb.AppendLine();
        sb.AppendLine($"{config.desc}");
        if (config.defaultTime > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"剩余时间: {_remainingTime.ToString("F0")} 小时");
        }

        tipsUI.SetContent(sb.ToString());

        var sizeDelta = _rectTransform.sizeDelta;
        sizeDelta.x /= 2;
        // 直接使用RectTransform的position，因为UI元素的position已经是屏幕空间的坐标
        tipsUI.UpdatePosition(_rectTransform.position, sizeDelta);
    }

    void OnDisable()
    {
        // 确保取消所有事件订阅
        UnsubscribeEvents();

        GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
    }

    void OnDestroy()
    {
        // 确保取消所有事件订阅
        UnsubscribeEvents();

        GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
    }
}
