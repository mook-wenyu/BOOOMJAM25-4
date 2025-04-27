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
        // Todo: 计算时间，存档

        CharacterMgr.Player().status = CharacterStatus.Explore;
        GlobalUIMgr.Instance.ShowLoadingMask(true);
        Hide();
        TopLevelUIPanel.Instance.goOut.gameObject.SetActive(false);
        TopLevelUIPanel.Instance.comeBack.gameObject.SetActive(true);

        ExploreNodeMgr.currentMapId = "0";

        SceneMgr.Instance.LoadScene("ExploreScene", LoadSceneMode.Additive);
    }

    public void OnCloseBtnClicked()
    {
        CharacterMgr.Player().status = CharacterStatus.Idle;
        GameMgr.ResumeTime();
        Hide();
    }

    public void Show()
    {
        GameMgr.PauseTime();
        CharacterMgr.Player().status = CharacterStatus.Busy;
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
