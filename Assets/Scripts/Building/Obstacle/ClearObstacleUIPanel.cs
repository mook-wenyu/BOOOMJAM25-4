using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClearObstacleUIPanel : MonoSingleton<ClearObstacleUIPanel>
{
    public GameObject uiPanel;

    public TextMeshProUGUI titleName;

    public TextMeshProUGUI clearTime;
    public Button startBuildBtn;

    private string _buildingInstanceId;
    private BuildingData _buildingData;

    void Awake()
    {
        titleName.text = "清除障碍";
        Hide();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Show(string buildingInstanceId)
    {
        this._buildingInstanceId = buildingInstanceId;
        
        uiPanel.SetActive(true);
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }


}
