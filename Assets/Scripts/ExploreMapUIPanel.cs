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


    public TextMeshProUGUI infoName, infoDesc, infoDistance, infoTime;
    public Button goOut;

    void Awake()
    {
        closeBtn.onClick.AddListener(OnCloseBtnClicked);
        goOut.onClick.AddListener(OnGoOutBtnClicked);
        Hide();
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

        ExploreNodeMgr.currentMapId = "0";

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
        infoName.text = "探索地图";
        infoDesc.text = "探索地图描述";
        infoDistance.text = "距离: 20KM";
        infoTime.text = "时间: 1.5H";
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }
}
