using UnityEngine;

public abstract class TipsUIBase : GlobalUIBase
{
    protected RectTransform _rectTransform;

    protected override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 更新位置
    /// </summary>
    /// <param name="position">物体位置</param>
    /// <param name="size">物体尺寸</param>
    public virtual void UpdatePosition(Vector2 position, Vector2 size)
    {
        _rectTransform.anchoredPosition = AdjustPosition(position, size);
    }

    /// <summary>
    /// 调整位置
    /// </summary>
    /// <param name="objectPosition">物体位置</param>
    /// <param name="size">物体尺寸</param>
    /// <returns>调整后的位置</returns>
    protected Vector2 AdjustPosition(Vector2 objectPosition, Vector2 size)
    {
        // 获取Canvas和提示框的信息
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            objectPosition,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        // 获取提示框的尺寸
        Vector2 tipsSize = _rectTransform.sizeDelta;

        // 设置锚点为左上角
        _rectTransform.pivot = new Vector2(0, 1);

        // 计算对象尺寸
        Vector2 objectSize = new(size.x + 8, size.y / 2);

        // 计算最终位置，默认显示在对象右侧偏下
        Vector2 position = localPoint + new Vector2(objectSize.x, objectSize.y);

        // 确保提示框不会超出屏幕右侧
        if (objectPosition.x + tipsSize.x + objectSize.x > Screen.width)
        {
            position.x = localPoint.x - tipsSize.x - objectSize.x;
        }

        // 确保提示框不会超出屏幕底部
        if (objectPosition.y - tipsSize.y + objectSize.y < Screen.height / 2)
        {
            position.y = localPoint.y + tipsSize.y - objectSize.y;
        }

        return position;
    }
}
