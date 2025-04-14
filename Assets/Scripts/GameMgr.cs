using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class GameMgr
{
    public static bool initGame = false;

    /// <summary>
    /// 当前存档数据
    /// </summary>
    public static SaveData currentSaveData;

    /// <summary>
    /// 游戏时间暂停事件
    /// </summary>
    public static event Action OnGameTimePaused;
    /// <summary>
    /// 游戏时间恢复事件
    /// </summary>
    public static event Action OnGameTimeResumed;

    // 时间更新取消令牌
    private static CancellationTokenSource _timeUpdateCts;

    /// <summary>
    /// 时间是否暂停
    /// </summary>
    private static bool _isTimePaused;

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        if (!File.Exists(Utils.GetConfigPath()))
        {
            // TODO: 生成默认配置文件
        }

        currentSaveData = new SaveData();
    }

    /// <summary>
    /// 加载游戏数据
    /// </summary>
    public static async UniTask LoadGameData(string saveName = "auto_save")
    {
        string path = Utils.GetSavePath(saveName);
        currentSaveData = await IOHelper.LoadDataAsync<SaveData>(path);
        DialogueMgr.RunMgrs.SetStorage(currentSaveData.dialogueStorage);

#if UNITY_EDITOR
        Debug.Log($"加载游戏数据 - {saveName}");
#endif
    }

    /// <summary>
    /// 保存游戏数据
    /// </summary>
    public static async UniTask SaveGameData(string saveName = "auto_save")
    {
        currentSaveData.dialogueStorage = DialogueMgr.RunMgrs.GetCurrentStorage();
        string path = Utils.GetSavePath(saveName);
        await IOHelper.SaveDataAsync(path, currentSaveData);

#if UNITY_EDITOR
        Debug.Log($"保存游戏数据 - {saveName}");
#endif
    }
}
