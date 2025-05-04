using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BuildingEntity : MonoBehaviour
{
    [SerializeField] private string _instanceId;   // 实例ID
    [SerializeField] private string _buildingId;   // 建筑ID
    [SerializeField] private int _lightingRange = 0;   // 照亮区域大小
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;
    private Light2D _light2D;

    private BuildingData _buildingData;   // 建筑数据

    public bool IsComplete { get; private set; } = false;

    private bool _isEnabled = false;
    private Collider2D _collider;
    private SimpleTipsUI _tipsUI;
    private Sprite _iconF = null;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _boxCollider.isTrigger = true;
        _light2D = GetComponentInChildren<Light2D>();
        _light2D.enabled = false;
    }

    public void Setup(string buildingId, string instanceId)
    {
        _instanceId = instanceId;
        _buildingId = buildingId;

        IsComplete = BuildingMgr.HasBuildingData(instanceId);

        var config = BuildingMgr.GetBuildingConfig(buildingId);
        if (config != null)
        {
            if (!string.IsNullOrEmpty(config.path) && config.path != "0")
            {
                _spriteRenderer.sprite = Resources.Load<Sprite>(config.path);
                _boxCollider.size = _spriteRenderer.sprite.bounds.size;
                _boxCollider.offset = _spriteRenderer.sprite.bounds.center;
            }

            _lightingRange = config.light;
            _light2D.transform.position = transform.position;
            _light2D.transform.position += Vector3.up * 3.2f;
            if (IsComplete && config.light > 0)
            {
                _light2D.enabled = true;
                _light2D.pointLightInnerRadius = config.light * 2;
                _light2D.pointLightOuterRadius = config.light * 4;
            }
            else
            {
                _light2D.enabled = false;
            }
        }

        if (IsComplete)
        {
            Complete();
        }
        else
        {
            UnComplete();
        }
    }

    public string GetId()
    {
        return _instanceId;
    }

    public string GetBuildingId()
    {
        return _buildingId;
    }

    public void SetLightingRange(int value)
    {
        _light2D.enabled = value > 0;
        _light2D.pointLightInnerRadius = value * 2;
        _light2D.pointLightOuterRadius = value * 4;
    }

    public void UnComplete()
    {
        IsComplete = false;
        _spriteRenderer.color = Color.gray;
        Debug.Log($"UnComplete: {_instanceId} - {_spriteRenderer.color}");
    }

    public void Complete()
    {
        IsComplete = true;
        _spriteRenderer.color = Color.white;

        if (_lightingRange > 0)
        {
            SetLightingRange(_lightingRange);
        }
    }

    void Update()
    {
        if (_isEnabled)
        {
            if (Input.GetKeyUp(KeyCode.F) && CharacterMgr.Player().status == CharacterStatus.Idle)
            {
                // 打开设计台界面
                if (_instanceId == "design_platform")
                {
                    if (BuildPlatformUIPanel.Instance.uiPanel.activeSelf)
                    {
                        BuildPlatformUIPanel.Instance.Hide();
                        return;
                    }
                    BuildPlatformUIPanel.Instance.Show(_instanceId);

                    ClearTips();
                    return;
                }

                if (_instanceId == "bed")
                {
                    GameMgr.PlayerSleep().Forget();
                    ClearTips();
                    return;
                }

                _buildingData = BuildingMgr.GetBuildingData(_instanceId);
                if (_buildingData == null)
                {
                    GlobalUIMgr.Instance.ShowMessage("正在建造中...");   // 显示提示信息
                    return;
                }
                switch (_buildingData.GetBuildingType())
                {
                    case BuildingType.Warehouse:
                        if (WarehouseUIPanel.Instance.uiPanel.activeSelf)
                        {
                            WarehouseUIPanel.Instance.Hide();
                            return;
                        }
                        WarehouseUIPanel.Instance.Show(_instanceId);
                        break;
                    case BuildingType.Production:
                        if (ProductionPlatformUIPanel.Instance.uiPanel.activeSelf)
                        {
                            ProductionPlatformUIPanel.Instance.Hide();
                            return;
                        }
                        ProductionPlatformUIPanel.Instance.Show(_instanceId);
                        break;
                }
                ClearTips();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = collision;
            _isEnabled = true;

            if (_tipsUI != null)
            {
                return;
            }

            if (_instanceId == "design_platform")
            {
                UpdateTips();
                return;
            }
            if (_instanceId == "bed")
            {
                UpdateTips();
                return;
            }

            _buildingData = BuildingMgr.GetBuildingData(_instanceId);
            if (_buildingData != null && _buildingData.GetBuildingType() != BuildingType.Light)
            {
                UpdateTips();
            }
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = collision;
            _isEnabled = true;

            if (WorldMgr.Instance.uiRoot.GetChild(WorldMgr.Instance.uiRoot.childCount - 1).gameObject.activeSelf || CharacterMgr.Player().status == CharacterStatus.Sleep || CharacterMgr.Player().status == CharacterStatus.Busy)
            {
                ClearTips();
                return;
            }

            if (_tipsUI != null && _tipsUI.gameObject.activeSelf)
            {
                return;
            }

            if (_instanceId == "design_platform")
            {
                UpdateTips();
                return;
            }

            if (_instanceId == "bed")
            {
                UpdateTips();
                return;
            }

            if (_buildingData != null && _buildingData.GetBuildingType() != BuildingType.Light)
            {
                UpdateTips();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = null;
            _isEnabled = false;
            ClearTips();
        }
    }

    public void ClearTips()
    {
        if (_tipsUI != null)
        {
            GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
            _tipsUI = null;
        }
    }

    /// <summary>
    /// 更新并显示物品提示
    /// </summary>
    public void UpdateTips()
    {
        _tipsUI = GlobalUIMgr.Instance.Show<SimpleTipsUI>(GlobalUILayer.TooltipLayer);
        if (_iconF == null)
        {
            _iconF = Resources.Load<Sprite>(Path.Combine("Icon", "UI", "keyboard_f"));
        }
        _tipsUI.SetIcon(_iconF);
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
        Vector2 worldPos = transform.position;
        // 调整世界坐标到建筑物顶部中心
        worldPos.x -= _boxCollider.bounds.extents.x / 4;
        worldPos.y += _boxCollider.bounds.size.y + 0.5f;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        // 将世界空间的尺寸转换为屏幕空间的尺寸
        Vector2 screenSize = new Vector2(
            _boxCollider.bounds.size.x / Screen.width * Camera.main.pixelWidth,
            _boxCollider.bounds.size.y / Screen.height * Camera.main.pixelHeight);

        _tipsUI.UpdatePosition(screenPos, screenSize);
    }

}
