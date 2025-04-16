using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public abstract class PopupBase : GlobalUIBase
{
    public Button rootMask;
    public GameObject panel;
    public Button closeButton;

    protected override void Awake()
    {
        base.Awake();
        rootMask.onClick.AddListener(Hide);
        closeButton?.onClick.AddListener(Hide);
    }

    public override void Show()
    {
        base.Show();
        // 弹出动画
        panel.transform.localScale = Vector3.zero;
        Tween.Scale(panel.transform, 1, 0.4f, Ease.OutQuart);
    }

    public override void Hide()
    {
        // 收起动画
        Tween.Scale(panel.transform, 0, 0.35f, Ease.InQuart).OnComplete(() =>
        {
            OnBeforeHide();
            base.Hide();
            OnHideComplete();
        });
    }

    /// <summary>
    /// 隐藏前执行
    /// </summary>
    protected virtual void OnBeforeHide()
    {
        // 子类可以重写此方法以在隐藏前执行清理操作
    }

    /// <summary>
    /// 隐藏后执行
    /// </summary>
    protected virtual void OnHideComplete()
    {
        // 子类可以重写此方法以在隐藏后执行回收操作
    }

    protected override void OnDestroy()
    {
        if (rootMask) rootMask.onClick.RemoveAllListeners();
        if (closeButton) closeButton.onClick.RemoveAllListeners();
        base.OnDestroy();
    }
}