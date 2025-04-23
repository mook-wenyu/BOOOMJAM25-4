using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    /// <summary>
    /// 开始时间更新
    /// </summary>
    public static void StartTime()
    {
        if (_timeUpdateCts != null)
        {
            StopTime();
        }

        currentSaveData.gameTime.OnHourChanged += HandleHourChanged;  // 订阅整点事件
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
    private static void StopTime()
    {
        if (_timeUpdateCts == null) return;

        currentSaveData.gameTime.OnHourChanged -= HandleHourChanged;
        _timeUpdateCts.Cancel();
        _timeUpdateCts.Dispose();
        _timeUpdateCts = null;
    }

    private const int BATCH_SIZE = 50;

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
                    await currentSaveData.gameTime.AddMinutes();
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

    private static async UniTask HandleHourChanged(GameTime gameTime)
    {
        await UpdateHourlyTasksAsync(BATCH_SIZE);
    }

    private static async UniTask UpdateHourlyTasksAsync(int batchSize)
    {
        // 更新角色
        var characters = currentSaveData.characters.Values.ToList();
        for (int i = 0; i < characters.Count; i += batchSize)
        {
            int count = Math.Min(batchSize, characters.Count - i);
            UpdateCharacterBatch(characters, i, count);

            if (i + batchSize < characters.Count)
            {
                await UniTask.Yield();
            }
        }

        // 更新配方 - 遍历所有生产平台
        var platforms = currentSaveData.productionPlatforms.Values.ToList();
        for (int i = 0; i < platforms.Count; i += batchSize)
        {
            int count = Math.Min(batchSize, platforms.Count - i);
            UpdateProductionPlatformBatch(platforms, i, count);

            if (i + batchSize < platforms.Count)
            {
                await UniTask.Yield();
            }
        }

        // 更新建筑 - 遍历所有建造平台
        var buildPlatforms = currentSaveData.buildPlatforms.Values.ToList();
        for (int i = 0; i < buildPlatforms.Count; i += batchSize)
        {
            int count = Math.Min(batchSize, buildPlatforms.Count - i);
            UpdateBuildPlatformBatch(buildPlatforms, i, count);

            if (i + batchSize < buildPlatforms.Count)
            {
                await UniTask.Yield();
            }
        }
    }

    /// <summary>
    /// 批量更新角色属性
    /// </summary>
    private static void UpdateCharacterBatch(List<CharacterData> characters, int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count && i < characters.Count; i++)
        {
            var character = characters[i];
            character.UpdateBuffs();
            character.SetHunger(character.hunger - 1);
            // 其他属性更新...
        }
    }

    /// <summary>
    /// 批量更新建造平台
    /// </summary>
    private static void UpdateBuildPlatformBatch(List<BuildPlatformData> platforms, int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count && i < platforms.Count; i++)
        {
            var platform = platforms[i];
            var completedBuildings = new List<BuildingData>();

            // 更新平台中的所有建筑
            foreach (var building in platform.buildingProgress)
            {
                building.ReduceTime();

                if (building.IsComplete())
                {
                    completedBuildings.Add(building);
                    // 将建筑添加到建筑列表中
                    currentSaveData.buildings.Add(building.instanceId, building);
                    // 停止建造动画
                    CharacterEntityMgr.Instance.GetPlayer().GetCharacterData().status = CharacterStatus.Idle;
                    CharacterEntityMgr.Instance.GetPlayer().GetAnimator().SetBool("IsBuild", false);
#if UNITY_EDITOR
                    Debug.Log($"建筑完成: {building.buildingId} - {building.instanceId} - {building.GetBuilding().name}");
#endif
                }
            }

            // 移除已完成的建筑
            foreach (var building in completedBuildings)
            {
                platform.buildingProgress.Remove(building);
            }
        }
    }

    /// <summary>
    /// 批量更新生产平台
    /// </summary>
    private static void UpdateProductionPlatformBatch(List<ProductionPlatformData> platforms, int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count && i < platforms.Count; i++)
        {
            var platform = platforms[i];

            // 更新平台中的所有配方
            foreach (var recipe in platform.productionProgress)
            {
                if (!recipe.IsComplete())
                {
                    recipe.ReduceTime();
                }
            }
        }
    }

}
