using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExploreMapUIPanel : MonoSingleton<ExploreMapUIPanel>
{
    public GameObject uiPanel;
    public Button closeBtn;

    public ToggleGroup toggleGroup;


    public TextMeshProUGUI infoName, infoDesc, infoDistance, infoTime;
    public Button goOut;

    void Awake()
    {
        closeBtn.onClick.AddListener(OnCloseBtnClicked);
        goOut.onClick.AddListener(OnGoOutBtnClicked);
        // 获取所有子Toggle并设置group
        Toggle[] toggles = toggleGroup.GetComponentsInChildren<Toggle>();
        foreach (Toggle toggle in toggles)
        {
            toggle.group = toggleGroup;
            // 添加监听事件
            toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(toggle, isOn));
        }
        Hide();
    }

    private void OnToggleValueChanged(Toggle changedToggle, bool isOn)
    {
        if (isOn)
        {
            SelectedMap(changedToggle.name);
        }
    }

    public void SelectedMap(string toggleName)
    {
        Debug.Log(toggleName + " 被选中");
        switch (toggleName)
        {
            case "JiaoWaiSenLin":
                infoName.text = "郊外森林";
                infoDesc.text = "郊外森林描述";
                infoDistance.text = "距离: 20KM";
                infoTime.text = "时间: 1.5H";

                ExploreNodeMgr.currentMapId = "1";

                break;
            case "ZhuZhaiQu":
                infoName.text = "住宅区";
                infoDesc.text = "住宅区描述";
                infoDistance.text = "距离: 20KM";
                infoTime.text = "时间: 1.5H";

                ExploreNodeMgr.currentMapId = "2";

                break;
            case "ShiZhongXin":
                infoName.text = "市中心";
                infoDesc.text = "市中心描述";
                infoDistance.text = "距离: 20KM";
                infoTime.text = "时间: 1.5H";

                ExploreNodeMgr.currentMapId = "3";

                break;
        }
    }

    // 获取当前选中的Toggle
    public Toggle GetSelectedToggle()
    {
        foreach (Toggle toggle in toggleGroup.ActiveToggles())
        {
            return toggle;
        }
        return null;
    }

    public void OnGoOutBtnClicked()
    {
        int consumeTime = 90;
        if (!GameMgr.currentSaveData.gameTime.IsTimeBefore(new GameTime(GameMgr.currentSaveData.gameTime.day + 1, 0, 0), consumeTime))
        {
            GlobalUIMgr.Instance.ShowMessage("太晚了，明天再探索吧！");
            return; // 时间不足，无法出门
        }

        CharacterMgr.Player().SetStatus(CharacterStatus.Explore);
        GlobalUIMgr.Instance.ShowLoadingMask(true);
        Hide();
        TopLevelUIPanel.Instance.goOut.gameObject.SetActive(false);
        TopLevelUIPanel.Instance.comeBack.gameObject.SetActive(true);

        SceneMgr.Instance.LoadScene("ExploreScene", LoadSceneMode.Additive);
    }

    public void OnCloseBtnClicked()
    {
        CharacterMgr.Player().SetStatus(CharacterStatus.Idle);
        GameMgr.ResumeTime();
        Hide();
    }

    public void Show()
    {
        GameMgr.PauseTime();
        CharacterMgr.Player().SetStatus(CharacterStatus.Busy);
        uiPanel.SetActive(true);
        uiPanel.transform.SetAsLastSibling();

        SelectedMap(GetSelectedToggle().name);
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }
}
