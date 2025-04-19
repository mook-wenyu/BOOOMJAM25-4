using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScene : MonoBehaviour
{
    public Button continueGameBtn;
    public Button playGameBtn;
    public Button exitGameBtn;

    private bool _isLoadSaveData;
    void Awake()
    {
#if UNITY_EDITOR
        if (!GameMgr.initGame)
        {
            SceneManager.LoadScene("InitScene");
            return;
        }
        Debug.Log("StartGame");
#endif

        continueGameBtn.onClick.AddListener(OnContinueGameBtnClick);
        playGameBtn.onClick.AddListener(OnPlayGameBtnClick);
        exitGameBtn.onClick.AddListener(OnExitGameBtnClick);

        if (File.Exists(Utils.GetAutoSavePath()))
        {
            continueGameBtn.interactable = true;
        }
        else
        {
            continueGameBtn.interactable = false;
        }
    }

    private void OnContinueGameBtnClick()
    {
        _isLoadSaveData = true;
        LoadSceneAsync().Forget();
    }

    private void OnPlayGameBtnClick()
    {
        _isLoadSaveData = false;
        LoadSceneAsync().Forget();
    }

    private void OnExitGameBtnClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    private async UniTask LoadSceneAsync()
    {
        // 在加载新场景之前，可以在这里处理相关操作
        GlobalUIMgr.Instance.ShowLoadingMask(true);

        if (_isLoadSaveData)
        {
#if UNITY_EDITOR
            Debug.Log("加载存档数据");
#endif
            await HandleLoadSaveData();
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("新游戏");
#endif
            await HandleNewData();
            await HandleSaveData();
        }

        var asyncLoad = SceneManager.LoadSceneAsync("MainScene");
        if (asyncLoad == null) return;
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            // UpdateLoadingProgress(progress);

            if (asyncLoad.progress >= 0.9f)
            {
                await UniTask.Delay(200);
                asyncLoad.allowSceneActivation = true;
            }

            await UniTask.Yield();
        }
        GlobalUIMgr.Instance.ShowLoadingMask(false);
    }

    /// <summary>
    /// 保存存档数据
    /// </summary>
    private async UniTask HandleSaveData()
    {
        await GameMgr.SaveGameData();
        await UniTask.Yield();
    }

    /// <summary>
    /// 加载存档数据
    /// </summary>
    private async UniTask HandleLoadSaveData()
    {
        await GameMgr.LoadGameData();
        await UniTask.Yield();
    }

    /// <summary>
    /// 创建新存档数据
    /// </summary>
    private async UniTask HandleNewData()
    {
        NewSaveData();
        await UniTask.Yield();
    }

    // 生成新存档数据
    private void NewSaveData()
    {
        GameMgr.currentSaveData = new SaveData();
        Debug.Log($"添加新物品：{InventoryMgr.AddInventoryItem("10001")}");
        Debug.Log($"添加新物品：{InventoryMgr.AddInventoryItem("10002")}");
        Debug.Log($"添加新物品：{InventoryMgr.AddInventoryItem("12001")}");
    }

}
