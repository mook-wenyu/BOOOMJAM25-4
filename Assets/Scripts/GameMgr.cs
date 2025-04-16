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
        DialogueMgr.RunMgrs.SetStorage(currentSaveData.DialogueStorage);

#if UNITY_EDITOR
        Debug.Log($"加载游戏数据 - {saveName}");
#endif
    }

    /// <summary>
    /// 保存游戏数据
    /// </summary>
    public static async UniTask SaveGameData(string saveName = "auto_save")
    {
        currentSaveData.DialogueStorage = DialogueMgr.RunMgrs.GetCurrentStorage();
        string path = Utils.GetSavePath(saveName);
        await IOHelper.SaveDataAsync(path, currentSaveData);

#if UNITY_EDITOR
        Debug.Log($"保存游戏数据 - {saveName}");
#endif
    }

    /// <summary>
    /// 开始时间更新
    /// </summary>
    public static void StartTime()
    {
        if (_timeUpdateCts != null)
        {
            StopTime();
        }

        UpdateTimeChangedAsync().Forget();
    }


    /// <summary>
    /// 暂停时间
    /// </summary>
    public static void PauseTime()
    {
        _isTimePaused = true;
        OnGameTimePaused?.Invoke();
    }

    /// <summary>
    /// 恢复时间
    /// </summary>
    public static void ResumeTime()
    {
        _isTimePaused = false;
        OnGameTimeResumed?.Invoke();
    }

    /// <summary>
    /// 取消时间更新
    /// </summary>
    public static void StopTime()
    {
        if (_timeUpdateCts == null) return;

        _timeUpdateCts.Cancel();
        _timeUpdateCts.Dispose();
        _timeUpdateCts = null;
    }

    /// <summary>
    /// 更新时间
    /// </summary>
    private static async UniTask UpdateTimeChangedAsync()
    {
        try
        {
            _timeUpdateCts = new CancellationTokenSource();

            while (!_timeUpdateCts.Token.IsCancellationRequested)
            {
                if (!_isTimePaused)
                {
                    currentSaveData.GameTime.AddMinutes(1);
                }
                await UniTask.Delay(0, cancellationToken: _timeUpdateCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // 正常取消,不做处理
        }
        catch (Exception e)
        {
            Debug.LogError($"时间更新出错: {e}");
        }
        finally
        {
            _timeUpdateCts?.Dispose();
            _timeUpdateCts = null;
        }
    }
}
