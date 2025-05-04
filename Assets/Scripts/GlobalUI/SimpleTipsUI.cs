using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleTipsUI : TipsUIBase
{
    public GameObject iconContainer;
    public Image icon;
    public Image icon2;

    public TextMeshProUGUI contentText;
    public float showDelay = 0.1f; // 显示延迟时间

    private float _hoverTime;
    private bool _isHovering;

    public void SetIcon(Sprite sprite, Sprite sprite2 = null, bool hideText = true)
    {
        contentText.gameObject.SetActive(!hideText);
        icon.sprite = sprite;
        icon.gameObject.SetActive(true);
        icon2.sprite = sprite2;
        icon2.gameObject.SetActive(sprite2 != null);

        GetComponent<Image>().SetAlpha(0);

        // 更新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }

    /// <summary>
    /// 设置提示内容
    /// </summary>
    /// <param name="content">提示内容</param>
    public void SetContent(string content, bool hideIcon = true)
    {
        icon.gameObject.SetActive(!hideIcon);
        icon2.gameObject.SetActive(false);
        contentText.text = content;
        contentText.gameObject.SetActive(true);

        GetComponent<Image>().SetAlpha(1);

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