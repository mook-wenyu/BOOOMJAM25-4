using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingEntity : MonoBehaviour
{
    [SerializeField] private string _instanceId;   // 实例ID
    [SerializeField] private string _buildingId;   // 建筑ID
    [SerializeField] private bool _isObstacle = false;   // 是否障碍物
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;

    private BuildingData _buildingData;   // 建筑数据

    private bool _isEnabled = false;
    private bool _isObstacleEnabled = false;
    private Collider2D _collider;
    private Collision2D _collision;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();
        GetComponent<Collider2D>().isTrigger = !_isObstacle;
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

    public bool GetIsObstacle()
    {
        return _isObstacle;
    }

    public void SetIsObstacle(bool value)
    {
        _isObstacle = value;
        GetComponent<Collider2D>().isTrigger = !_isObstacle;
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

        if (_isObstacleEnabled)
        {
            if (Input.GetKeyUp(KeyCode.F) && CharacterMgr.Player().status == CharacterStatus.Idle)
            {
                // 清障
                Debug.Log("清障");
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collision = collision;
            _isObstacleEnabled = true;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collision = collision;
            _isObstacleEnabled = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collision = null;
            _isObstacleEnabled = false;
        }
    }
}
