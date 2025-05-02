using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExploreNodeEntity : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    private ExploreNodeData _node;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _iconRenderer;

    // 定义不同状态的颜色
    private Color _defaultColor;   // 默认状态颜色
    private Color _enteredColor;  // 进入状态颜色
    private Color _normalColor;    // 正常状态颜色 
    private Color _highlightColor; // 高亮状态颜色
    private Color _pressedColor;   // 按下状态颜色
    private Color _disabledColor;  // 禁用状态颜色

    private bool _isInteractable = true; // 是否可交互
    private bool _isPressed = false;     // 追踪按下状态

    public bool IsPointerOver { get; private set; }

    public event Action<ExploreNodeData> OnClick;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _iconRenderer = transform.Find("Icon").GetComponent<SpriteRenderer>();

        // 初始化各种状态的颜色
        _defaultColor = Color.green;
        _enteredColor = _spriteRenderer.color;
        _normalColor = _defaultColor;
        _highlightColor = new Color(_spriteRenderer.color.r * 1.2f, _spriteRenderer.color.g * 1.2f, _spriteRenderer.color.b * 1.2f, _spriteRenderer.color.a);
        _pressedColor = new Color(_spriteRenderer.color.r * 0.8f, _spriteRenderer.color.g * 0.8f, _spriteRenderer.color.b * 0.8f, _spriteRenderer.color.a);
        _disabledColor = new Color(_spriteRenderer.color.r * 0.5f, _spriteRenderer.color.g * 0.5f, _spriteRenderer.color.b * 0.5f, _spriteRenderer.color.a * 0.5f);
    }

    public void Setup(ExploreNodeData node)
    {
        this._node = node;
        var config = node.GetConfig();
        _spriteRenderer.color = _normalColor;
        if (config.type == (int)ExploreNodeType.Empty)
        {
            _iconRenderer.gameObject.SetActive(false);
        }
        else
        {
            //if (!string.IsNullOrEmpty(config.path) && config.path != "0")
            //{
            //    _iconRenderer.sprite = Resources.Load<Sprite>(config.path);
            //}
            switch ((ExploreNodeType)config.type)
            {
                case ExploreNodeType.Empty:
                    break;
                case ExploreNodeType.Functional:
                    SetPic("Location");
                    break;
                case ExploreNodeType.Reward:
                    SetPic("Reward");
                    break;
                case ExploreNodeType.Submit:
                    SetPic("Obstacle");
                    break;
                case ExploreNodeType.Production:
                    SetPic("Reward");
                    break;
                case ExploreNodeType.Story:
                    SetPic("Dialogue");
                    break;
            }
        }
    }

    public ExploreNodeData GetNode()
    {
        return _node;
    }

    // 设置交互状态
    public void SetInteractable(bool interactable)
    {
        _isInteractable = interactable;
        _spriteRenderer.color = _isInteractable ? _normalColor : _disabledColor;
    }

    public void SetEntered()
    {
        _normalColor = _enteredColor;
        _spriteRenderer.color = _normalColor;
        if (!GameMgr.currentSaveData.enteredNodes.ContainsKey(ExploreNodeMgr.currentMapId))
        {
            GameMgr.currentSaveData.enteredNodes.Add(ExploreNodeMgr.currentMapId, new HashSet<string>());
        }
        if (!GameMgr.currentSaveData.enteredNodes[ExploreNodeMgr.currentMapId].Contains(_node.id))
        {
            GameMgr.currentSaveData.enteredNodes[ExploreNodeMgr.currentMapId].Add(_node.id);
        }
    }

    /// <summary>
    /// 更新并显示物品提示
    /// </summary>
    public void UpdateTips(ExploreNodeData itemData)
    {
        if (!IsPointerOver) return; // 只在鼠标悬停时才显示提示

        var tipsUI = GlobalUIMgr.Instance.Show<SimpleTipsUI>(GlobalUILayer.TooltipLayer);

        tipsUI.SetContent(itemData.GetConfig().name);

        Vector3 uiPos = Camera.main.WorldToScreenPoint(transform.position);
        uiPos.y += 100;
        tipsUI.UpdatePosition(uiPos, Vector2.zero);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        _spriteRenderer.color = _highlightColor;

        if (this._node.GetConfig().type == (int)ExploreNodeType.Empty)
        {
            return;
        }
        IsPointerOver = true;
        UpdateTips(_node);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        _spriteRenderer.color = _normalColor;
        _isPressed = false; // 鼠标移出时重置按下状态

        IsPointerOver = false;
        var tipsUI = GlobalUIMgr.Instance.Get<SimpleTipsUI>();

        if (!IsPointerOver && tipsUI != null && tipsUI.gameObject.activeSelf)
        {
            GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        _spriteRenderer.color = _pressedColor;
        _isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        _spriteRenderer.color = _highlightColor; // 鼠标还在物体上时保持高亮

        if (_isPressed) // 只有在之前按下的情况下才触发点击
        {
            OnClick?.Invoke(_node);
        }
        _isPressed = false;
    }

    public void SetPic(string picPath)
    {
        _iconRenderer.sprite = Resources.Load<Sprite>(Path.Combine("UI", "Explore", "Pic", picPath));
    }

    void OnDestroy()
    {
        GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
    }
}
