using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class TopLevelUIPanel : MonoBehaviour
{
    public GameObject timeInfo;
    public TextMeshProUGUI dataText;
    public TextMeshProUGUI timeText;
    
    public Button saveBtn;

    private void Awake()
    {
        GameMgr.OnGameTimePaused += HandleGameTimePaused;
        GameMgr.OnGameTimeResumed += HandleGameTimeResumed;
        GameMgr.currentSaveData.gameTime.OnTimeChanged += HandleTimeChanged;
        
        saveBtn.onClick.AddListener(() => _ = GameMgr.SaveGameData());
    }

    void Start()
    {
        _ = HandleTimeChanged(GameMgr.currentSaveData.gameTime);

        GameMgr.StartTime();
    }

    private void HandleGameTimePaused()
    {
        Debug.Log("时间暂停");
    }

    private void HandleGameTimeResumed()
    {
        Debug.Log("时间恢复");
    }

    private async UniTask HandleTimeChanged(GameTime gameTime)
    {
        dataText.text = $"第 {gameTime.day} 天";
        timeText.text = $"{gameTime.hour} : {gameTime.minute}";
        await UniTask.CompletedTask;
    }

    private void OnDestroy()
    {
        GameMgr.OnGameTimePaused -= HandleGameTimePaused;
        GameMgr.OnGameTimeResumed -= HandleGameTimeResumed;
        GameMgr.currentSaveData.gameTime.OnTimeChanged -= HandleTimeChanged;
    }

}
