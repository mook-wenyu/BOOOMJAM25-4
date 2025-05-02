using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitScene : MonoBehaviour
{
    void Awake()
    {
        PrimeTweenConfig.warnZeroDuration = false;
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        PrimeTweenConfig.defaultEase = Ease.Linear;

        IOHelper.CreateDirectory(Utils.GetSavePath());

        GameMgr.initGame = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadSceneAsyncOperation("StartScene").Forget();
    }

    private async UniTask LoadSceneAsyncOperation(string sceneName)
    {
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        if (asyncLoad == null) return;
        asyncLoad.allowSceneActivation = false;

        // 初始化游戏数据
        await InitGameData();

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                await UniTask.Yield();
                asyncLoad.allowSceneActivation = true;
            }
            await UniTask.Yield();
        }
    }

    private async UniTask InitGameData()
    {
        // 初始化配置
        ConfigManager.Instance.Init();
        await UniTask.Yield();

        // 初始化
        GameMgr.Init();
        await UniTask.Yield();

        AudioMgr.Instance.Init();
        await UniTask.Yield();

        // 初始化全局UI管理器
        GlobalUIMgr.Instance.Init();
        await UniTask.Yield();

        ExploreNodeMgr.Init();
        await UniTask.Yield();

        // 初始化对话
        DialogueMgr.Initialize();
        await UniTask.Yield();

        // 初始化角色
        CharacterMgr.Init();
        await UniTask.Yield();

    }
}
