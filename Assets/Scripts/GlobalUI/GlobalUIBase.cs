using System;
using UnityEngine;
using UnityEngine.UI;

public enum GlobalUILayer
{
    DefaultLayer = 0,    // 默认层
    SystemLayer = 1,    // 系统层
    PopupLayer = 2,     // 弹窗层（确认框、选择框等）
    MessageLayer = 3,   // 消息提示（消息、警告、错误）
    TooltipLayer = 4,  // 提示信息
    TopLayer = 5         // 最顶层（加载、过场等）
}

/// <summary>
/// 所有UI元素的基类
/// </summary>
public abstract class GlobalUIBase : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (!_canvasGroup) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    protected virtual void OnDestroy()
    {
        // 基类的 OnDestroy，子类可以根据需要重写
    }
}

/// <summary>
/// Image 扩展方法
/// </summary>
public static class ImageExtensions
{
    public static void SetAlpha(this Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}