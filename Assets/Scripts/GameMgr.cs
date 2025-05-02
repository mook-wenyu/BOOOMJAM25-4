using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MookDialogueScript;
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
        currentSaveData.gameTime.OnTimeChanged += HandleTimeChanged;    // 订阅时间变化事件
        currentSaveData.gameTime.OnHourChanged += HandleHourChanged;    // 订阅整点事件
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

        currentSaveData.gameTime.OnTimeChanged -= HandleTimeChanged;
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
                    await currentSaveData.gameTime.AddMinutes(1);
                }
                await UniTask.Delay(334, cancellationToken: _timeUpdateCts.Token);
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
            StopTime();
        }
    }

    public static async UniTask PlayerSleep()
    {
        PauseTime();
        GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
        CharacterMgr.Player().SetStatus(CharacterStatus.Sleep);
        WorldMgr.Instance.blackScreen.SetActive(true);
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
        _ = currentSaveData.gameTime.ConsumeTimeToHour(7);
    }

    [ScriptFunc("end_game")]
    public static void End_Game()
    {
        PauseTime();
        CharacterMgr.Player().SetStatus(CharacterStatus.Busy);
        WorldMgr.Instance.endGameScreen.SetActive(true);
    }


    // 时间变化事件处理
    private static async UniTask HandleTimeChanged(GameTime gameTime)
    {
        // 更新配方 - 遍历所有生产平台
        var platforms = currentSaveData.productionPlatforms.Values.ToList();
        for (int i = 0; i < platforms.Count; i += BATCH_SIZE)
        {
            int count = Math.Min(BATCH_SIZE, platforms.Count - i);
            UpdateProductionPlatformBatch(platforms, i, count);

            if (i + BATCH_SIZE < platforms.Count)
            {
                await UniTask.Yield();
            }
        }

        // 更新建筑 - 遍历所有建造平台
        var buildPlatforms = currentSaveData.buildPlatforms.Values.ToList();
        for (int i = 0; i < buildPlatforms.Count; i += BATCH_SIZE)
        {
            int count = Math.Min(BATCH_SIZE, buildPlatforms.Count - i);
            UpdateBuildPlatformBatch(buildPlatforms, i, count);

            if (i + BATCH_SIZE < buildPlatforms.Count)
            {
                await UniTask.Yield();
            }
        }
    }

    // 整点事件处理
    private static async UniTask HandleHourChanged(GameTime gameTime)
    {
        // 当前是上午7点，开启全局光源
        if (currentSaveData.gameTime.IsSpecificFullHour(7))
        {
            // 起床
            if (CharacterMgr.Player().status == CharacterStatus.Sleep)
            {
                // 睡觉起床
                CharacterMgr.Player().IncreaseEnergy((float)Utils.GetGeneralParametersConfig("energyGrowInSleep").par);
            }
            WorldMgr.Instance.globalLight.intensity = 1f;
            WorldMgr.Instance.blackScreen.SetActive(false);
            ResumeTime();
            CharacterMgr.Player().SetStatus(CharacterStatus.Idle);
        }
        // 当前是下午午18点，关闭全局光源
        if (currentSaveData.gameTime.IsSpecificFullHour(18))
        {
            WorldMgr.Instance.globalLight.intensity = 0.1f;
        }

        // 当前是下午1点，暂停时间，黑屏，强制进入睡眠状态
        if (currentSaveData.gameTime.IsSpecificFullHour(1))
        {
            if (CharacterMgr.Player().status != CharacterStatus.Sleep)
            {
                // 睡觉
                await PlayerSleep();
            }
        }

        // 更新角色
        var characters = currentSaveData.characters.Values.ToList();
        for (int i = 0; i < characters.Count; i += BATCH_SIZE)
        {
            int count = Math.Min(BATCH_SIZE, characters.Count - i);
            UpdateCharacterBatch(characters, i, count);

            if (i + BATCH_SIZE < characters.Count)
            {
                await UniTask.Yield();
            }
        }
    }

    /// <summary>
    /// 批量更新角色（每小时）
    /// </summary>
    private static void UpdateCharacterBatch(List<CharacterData> characters, int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count && i < characters.Count; i++)
        {
            var character = characters[i];
            // 更新Buff
            character.UpdateBuffs();

            // 更新属性
            if (character.hunger > 0)
            {
                // 饱食度大于0，减少饱食度
                character.DecreaseHunger((float)Utils.GetGeneralParametersConfig("hungerLostValue").par);
            }
            else
            {
                // 饱食度为0，减少精神和生命值
                character.DecreaseSpirit((float)Utils.GetGeneralParametersConfig("zeroHungerLostSpiritValue").par);
                character.DecreaseHealth((float)Utils.GetGeneralParametersConfig("zeroHungerLostHPValue").par);
            }

            if (character.hunger >= (float)Utils.GetGeneralParametersConfig("hPGrowHunger").par && character.health > 0)
            {
                // 饱食度大于80，增加生命值
                character.IncreaseHealth((float)Utils.GetGeneralParametersConfig("hPGrowValue").par);
            }

            if (currentSaveData.gameTime.hour >= 7 && currentSaveData.gameTime.hour <= 18)
            {
                // 白天，增加体力
                character.IncreaseEnergy(2);
            }
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
                    CharacterMgr.Player().SetStatus(CharacterStatus.Idle);
                    CharacterEntityMgr.Instance.GetPlayer().GetAnimator().SetBool("IsBuild", false);
#if UNITY_EDITOR
                    Debug.Log($"建筑完成: {building.buildingId} - {building.instanceId} - {building.GetBuildingConfig().name}");
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
