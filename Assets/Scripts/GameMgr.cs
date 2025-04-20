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
            const int BATCH_SIZE = 50;

            while (!_timeUpdateCts.Token.IsCancellationRequested)
            {
                if (!_isTimePaused)
                {
                    currentSaveData.gameTime.AddMinutes(1);

                    // 检查是否到达整点
                    bool isFullHour = currentSaveData.gameTime.IsFullHour();
                    if (isFullHour)
                    {
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

                        // 更新配方
                        var recipes = currentSaveData.recipeProgress;
                        for (int i = 0; i < recipes.Count; i += BATCH_SIZE)
                        {
                            int count = Math.Min(BATCH_SIZE, recipes.Count - i);
                            UpdateRecipeBatch(recipes, i, count);

                            if (i + BATCH_SIZE < recipes.Count)
                            {
                                await UniTask.Yield();
                            }
                        }

                        // 更新建筑
                        var buildings = currentSaveData.buildingProgress;
                        for (int i = 0; i < buildings.Count; i += BATCH_SIZE)
                        {
                            int count = Math.Min(BATCH_SIZE, buildings.Count - i);
                            UpdateBuildingBatch(buildings, i, count);

                            if (i + BATCH_SIZE < buildings.Count)
                            {
                                await UniTask.Yield();
                            }
                        }

                    }
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

    /// <summary>
    /// 批量更新角色属性
    /// </summary>
    private static void UpdateCharacterBatch(List<CharacterData> characters, int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count && i < characters.Count; i++)
        {
            var character = characters[i];
            character.UpdateBuffs();
            // 其他属性更新...
        }
    }

    private static void UpdateRecipeBatch(List<RecipeData> recipes, int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count && i < recipes.Count; i++)
        {
            var recipe = recipes[i];
            recipe.ReduceTime();

            if (recipe.IsComplete())
            {
                // 从生产中列表移除
                currentSaveData.recipeProgress.Remove(recipe);
                // 添加产品到背包
                CharacterMgr.Player().Inventory.AddInventoryItem(recipe.GetRecipe().productID.ToString(), recipe.GetRecipe().productAmount);
            }
        }
    }

    /// <summary>
    /// 批量更新建筑
    /// </summary>
    private static void UpdateBuildingBatch(List<BuildingData> buildings, int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count && i < buildings.Count; i++)
        {
            var building = buildings[i];
            building.ReduceTime();

            if (building.IsComplete())
            {
                // 从建造中列表移除
                currentSaveData.buildingProgress.Remove(building);
                // 添加到已完成建筑列表
                currentSaveData.buildings.Add(building.instanceId, building);
            }
#if UNITY_EDITOR
            Debug.Log($"整点更新: {currentSaveData.gameTime.GetTimeString()} - 更新建筑数量: {buildings.Count}");
#endif
        }
    }
}
