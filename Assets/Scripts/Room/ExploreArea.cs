using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploreArea : MonoBehaviour
{
    private bool _isEnabled = false;
    private Collider2D _collider;
    private BoxCollider2D _boxCollider;
    private SimpleTipsUI _tipsUI;

    void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _boxCollider.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isEnabled)
        {
            if (Input.GetKeyUp(KeyCode.F))
            {
                if (!ExploreMapUIPanel.Instance.uiPanel.activeSelf)
                {
                    ExploreMapUIPanel.Instance.Show();
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = collision;
            _isEnabled = true;
            UpdateTips("按 F 键外出探索");
            Debug.Log($"进入{name}");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = null;
            _isEnabled = false;
            GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
            Debug.Log($"离开{name}");
        }
    }

    /// <summary>
    /// 更新并显示物品提示
    /// </summary>
    public void UpdateTips(string content)
    {
        _tipsUI = GlobalUIMgr.Instance.Show<SimpleTipsUI>(GlobalUILayer.TooltipLayer);
        _tipsUI.SetContent(content);
        UpdateTipsPosition();
    }

    void LateUpdate()
    {
        // 如果提示UI存在且启用，则每帧更新位置
        if (_isEnabled && _tipsUI != null)
        {
            UpdateTipsPosition();
        }
    }

    private void UpdateTipsPosition()
    {
        Vector2 worldPos = _boxCollider.offset;
        // 调整世界坐标到建筑物顶部中心
        worldPos.x -= _boxCollider.bounds.extents.x / 2;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        // 将世界空间的尺寸转换为屏幕空间的尺寸
        Vector2 screenSize = new Vector2(
            _boxCollider.bounds.size.x / Screen.width * Camera.main.pixelWidth,
            _boxCollider.bounds.size.y / Screen.height * Camera.main.pixelHeight);

        _tipsUI.UpdatePosition(screenPos, screenSize);
    }
}
