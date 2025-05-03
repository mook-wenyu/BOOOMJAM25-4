using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingUIPanel : MonoSingleton<SettingUIPanel>
{
    public GameObject uiPanel;
    public Button backGameBtn;
    public Button saveGameBtn;
    public Button backStartBtn;
    public Button saveAndExitBtn;
    public Button exitGameBtn;

    // Start is called before the first frame update
    void Awake()
    {
        backGameBtn.onClick.AddListener(OnBackGameBtnClick);
        saveGameBtn.onClick.AddListener(OnSaveGameBtnClick);
        backStartBtn.onClick.AddListener(OnBackStartBtnClick);
        saveAndExitBtn.onClick.AddListener(() => OnSaveAndExitBtnClick().Forget());
        exitGameBtn.onClick.AddListener(OnExitGameBtnClick);

        uiPanel.GetComponent<Button>().onClick.AddListener(OnBackGameBtnClick);

        Hide();
    }

    public void OnBackGameBtnClick()
    {
        AudioMgr.Instance.PlaySound("点击");
        Hide();
    }

    public void OnSaveGameBtnClick()
    {
        AudioMgr.Instance.PlaySound("点击");
        _ = GameMgr.SaveGameData();
    }

    public void OnBackStartBtnClick()
    {
        AudioMgr.Instance.PlaySound("点击");
        SceneManager.LoadScene("StartScene");
    }

    public async UniTask OnSaveAndExitBtnClick()
    {
        AudioMgr.Instance.PlaySound("点击");
        await GameMgr.SaveGameData();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void OnExitGameBtnClick()
    {
        AudioMgr.Instance.PlaySound("点击");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }


    public void Show()
    {
        GameMgr.PauseTime();
        uiPanel.SetActive(true);
    }

    public void Hide()
    {
        if (CharacterMgr.Player().status != CharacterStatus.Explore)
        {
            GameMgr.ResumeTime();
        }
        uiPanel.SetActive(false);
    }
}
