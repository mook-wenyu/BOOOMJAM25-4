using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
        AudioMgr.Instance.PlaySound("点击");
        Debug.Log(toggleName + " 被选中");
        switch (toggleName)
        {
            case "JiaoWaiSenLin":
                var config = ExploreNodeMgr.GetMapConfig("1");
                infoName.text = config.name;
                infoDesc.text = config.desc;
                infoDistance.text = $"体力: {config.energyCost}";
                infoTime.text = $"时间: {config.timeCost}小时";

                ExploreNodeMgr.currentMapId = "1";

                break;

            case "ZhuZhaiQu":
                config = ExploreNodeMgr.GetMapConfig("2");
                infoName.text = config.name;
                infoDesc.text = config.desc;
                infoDistance.text = $"体力: {config.energyCost}";
                infoTime.text = $"时间: {config.timeCost}小时";

                ExploreNodeMgr.currentMapId = "2";

                break;

            case "ShiZhongXin":
                config = ExploreNodeMgr.GetMapConfig("3");
                infoName.text = config.name;
                infoDesc.text = config.desc;
                infoDistance.text = $"体力: {config.energyCost}";
                infoTime.text = $"时间: {config.timeCost}小时";

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
        AudioMgr.Instance.PlaySound("点击");
        var config = ExploreNodeMgr.GetMapConfig(ExploreNodeMgr.currentMapId);
        if (config == null)
        {
            GlobalUIMgr.Instance.ShowMessage("请选择一个地点！");
            return;
        }
        // 检查时间是否足够
        int consumeTime = GameTime.HourToMinute(config.timeCost);
        if (!GameMgr.currentSaveData.gameTime.IsTimeBefore(new GameTime(GameMgr.currentSaveData.gameTime.day + 1, 0, 0), consumeTime))
        {
            GlobalUIMgr.Instance.ShowMessage("太晚了，明天再探索吧！");
            return; // 时间不足，无法出门
        }
        // 检查体力是否足够
        if (CharacterMgr.Player().energy < config.energyCost)
        {
            GlobalUIMgr.Instance.ShowMessage("体力不足，无法出门！");
            return; // 体力不足，无法出门
        }

        GameMgr.currentSaveData.gameTime.AddMinutes(consumeTime).Forget();
        CharacterMgr.Player().DecreaseEnergy((float)config.energyCost);
        CharacterMgr.Player().SetStatus(CharacterStatus.Explore);
        GlobalUIMgr.Instance.ShowLoadingMask(true);
        Hide();
        TopLevelUIPanel.Instance.goOut.gameObject.SetActive(false);
        TopLevelUIPanel.Instance.comeBack.gameObject.SetActive(true);

        SceneMgr.Instance.LoadScene("ExploreScene", LoadSceneMode.Additive);
    }

    public void OnCloseBtnClicked()
    {
        AudioMgr.Instance.PlaySound("点击关闭");
        CharacterMgr.Player().SetStatus(CharacterStatus.Idle);
        GameMgr.ResumeTime();
        Hide();
    }

    public void Show()
    {
        GameMgr.PauseTime();
        CharacterMgr.Player().SetStatus(CharacterStatus.Busy);
        GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
        uiPanel.SetActive(true);
        uiPanel.transform.SetAsLastSibling();

        SelectedMap(GetSelectedToggle().name);
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }
}
