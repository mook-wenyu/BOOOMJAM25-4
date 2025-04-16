using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleTipsUI : TipsUIBase
{
    public TextMeshProUGUI contentText;
    public float showDelay = 0.1f; // 显示延迟时间

    private float _hoverTime;
    private bool _isHovering;

    /// <summary>
    /// 设置提示内容
    /// </summary>
    /// <param name="content">提示内容</param>
    public void SetContent(string content)
    {
        contentText.text = content;

        // 更新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }

    private void Update()
    {
        if (_isHovering && !gameObject.activeSelf && Time.time - _hoverTime >= showDelay)
        {
            Show();
        }
    }

    /// <summary>
    /// 显示提示
    /// </summary>
    public override void Show()
    {
        base.Show();
        UpdatePosition(Input.mousePosition, Vector2.zero);
    }

    /// <summary>
    /// 隐藏提示
    /// </summary>
    public override void Hide()
    {
        base.Hide();
    }
}