using System;
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
        AudioMgr.Instance.PlaySound("点击");
        _isLoadSaveData = true;
        LoadSceneAsync().Forget();
    }

    private void OnPlayGameBtnClick()
    {
        AudioMgr.Instance.PlaySound("点击");
        _isLoadSaveData = false;
        LoadSceneAsync().Forget();
    }

    private void OnExitGameBtnClick()
    {
        AudioMgr.Instance.PlaySound("点击");
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
        GameMgr.currentSaveData.gameTime = new GameTime(1, 8, 0);

        foreach (var map in ExploreNodeMgr.tempExploreMaps)
        {
            GameMgr.currentSaveData.exploreMaps.Add(map.Key, map.Value);
        }

        CharacterData characterData = new CharacterData();
        characterData.id = "player";
        characterData.fullName = "玩家";
        characterData.healthMax = (int)Utils.GetGeneralParametersConfig("mostHP").par;
        characterData.hungerMax = (int)Utils.GetGeneralParametersConfig("mostHunger").par;
        characterData.energyMax = (int)Utils.GetGeneralParametersConfig("mostEnergy").par;
        characterData.spiritMax = (int)Utils.GetGeneralParametersConfig("mostSpirit").par;
        characterData.health = (int)Utils.GetGeneralParametersConfig("startHP").par;
        characterData.hunger = (int)Utils.GetGeneralParametersConfig("startHunger").par;
        characterData.energy = (int)Utils.GetGeneralParametersConfig("startEnergy").par;
        characterData.spirit = (int)Utils.GetGeneralParametersConfig("startSpirit").par;

        GameMgr.currentSaveData.characters.Add(characterData.id, characterData);
        GameMgr.currentSaveData.playerId = characterData.id;

        BuildingMgr.AddBuildingData(new BuildBuildingData(string.Empty, "design_platform") { remainingTime = 0 });
        BuildingMgr.AddBuildingData(new BuildBuildingData(string.Empty, "bed") { remainingTime = 0 });

        BuildingMgr.AddBuildingData(new BuildBuildingData("20005", "lighting_1_1") { remainingTime = 0, lightingData = new LightingData() });
        BuildingMgr.AddBuildingData(new BuildBuildingData("20005", "lighting_1_2") { remainingTime = 0, lightingData = new LightingData() });
        BuildingMgr.AddBuildingData(new BuildBuildingData("20005", "lighting_2_1") { remainingTime = 0, lightingData = new LightingData() });
        BuildingMgr.AddBuildingData(new BuildBuildingData("20005", "lighting_2_2") { remainingTime = 0, lightingData = new LightingData() });
        BuildingMgr.AddBuildingData(new BuildBuildingData("20005", "lighting_3_1") { remainingTime = 0, lightingData = new LightingData() });
        BuildingMgr.AddBuildingData(new BuildBuildingData("20005", "lighting_3_2") { remainingTime = 0, lightingData = new LightingData() });

        BuildingMgr.AddBuildingData(new WarehouseBuildingData("20003", "warehouse_1_3", WarehouseType.Box, 9) { remainingTime = 0 });
        var warehouseData_1_3 = BuildingMgr.GetBuildingData<WarehouseBuildingData>("warehouse_1_3").GetWarehouseData();
        warehouseData_1_3.AddItem("10031", 5);
        BuildingMgr.AddBuildingData(new WarehouseBuildingData("20003", "warehouse_2_1", WarehouseType.Box, 9) { remainingTime = 0 });
        var warehouseData_2_1 = BuildingMgr.GetBuildingData<WarehouseBuildingData>("warehouse_2_1").GetWarehouseData();
        warehouseData_2_1.AddItem("10001", 10);
        warehouseData_2_1.AddItem("10002", 10);
        warehouseData_2_1.AddItem("10003", 10);
        BuildingMgr.AddBuildingData(new WarehouseBuildingData("20003", "warehouse_3_3", WarehouseType.Box, 9) { remainingTime = 0 });
        var warehouseData_3_3 = BuildingMgr.GetBuildingData<WarehouseBuildingData>("warehouse_3_3").GetWarehouseData();
        warehouseData_3_3.AddItem("10028", 1);

    }

}
