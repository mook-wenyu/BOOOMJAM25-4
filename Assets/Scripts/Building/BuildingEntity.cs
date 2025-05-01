using System;
using System.Collections;
using System.Collections.Generic;
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

    private bool _isEnabled = false;
    private Collider2D _collider;

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

        var config = BuildingMgr.GetBuildingConfig(buildingId);
        if (config != null)
        {
            if (!string.IsNullOrEmpty(config.path) && config.path != "0")
            {
                _spriteRenderer.sprite = Resources.Load<Sprite>(config.path);
                _boxCollider.size = _spriteRenderer.sprite.bounds.size;
                _boxCollider.offset = _spriteRenderer.sprite.bounds.center;
            }
            _light2D.enabled = config.light > 0;
            _light2D.transform.position = new Vector3(_light2D.transform.position.x, _spriteRenderer.sprite.bounds.size.y / 2, _light2D.transform.position.z);
            _light2D.pointLightInnerRadius = config.light * 2;
            _light2D.pointLightOuterRadius = config.light * 4;
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
        _lightingRange = value;
        _light2D.enabled = _lightingRange > 0;
        _light2D.pointLightInnerRadius = _lightingRange * 2;
        _light2D.pointLightOuterRadius = _lightingRange * 4;
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
                    return;
                }

                if (_instanceId == "bed")
                {
                    GameMgr.PlayerSleep().Forget();
                    return;
                }

                _buildingData ??= BuildingMgr.GetBuildingData(_instanceId);
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
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = collision;
            _isEnabled = true;
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = collision;
            _isEnabled = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = null;
            _isEnabled = false;
        }
    }

}
