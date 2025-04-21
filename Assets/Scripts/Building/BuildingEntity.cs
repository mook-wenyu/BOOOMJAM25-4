using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingEntity : MonoBehaviour
{
    [SerializeField] private string _instanceId;   // 实例ID

    private BuildingData _buildingData;   // 建筑数据

    private bool _isEnabled = false;
    private Collider2D _collider;

    void Update()
    {
        if (_isEnabled)
        {
            if (Input.GetKeyUp(KeyCode.F))
            {
                // 打开建筑界面
                Debug.Log("打开建筑界面");
                if (WarehouseUIPanel.Instance.warehousePanel.activeSelf)
                {
                    WarehouseUIPanel.Instance.Hide();
                    return;
                }
                WarehouseUIPanel.Instance.Show(GameMgr.currentSaveData.buildings.Values.ToList()[0].instanceId);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = collision;
            _isEnabled = true;
            Debug.Log("触发进入");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = null;
            _isEnabled = false;
            Debug.Log("触发离开");
        }
    }
}
