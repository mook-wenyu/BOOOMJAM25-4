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

    public Slider health, hunger, energy, spirit;
    // public TextMeshProUGUI healthText, hungerText, energyText, spiritText;

    private CharacterData player;

    private void Awake()
    {
        GameMgr.OnGameTimePaused += HandleGameTimePaused;
        GameMgr.OnGameTimeResumed += HandleGameTimeResumed;
        GameMgr.currentSaveData.gameTime.OnTimeChanged += HandleTimeChanged;

        saveBtn.onClick.AddListener(() => _ = GameMgr.SaveGameData());

        player = CharacterMgr.Player();
        UpdateCharacterData();
        player.OnHpChanged += HandleHpChanged;
        player.OnHpMaxChanged += HandleHpMaxChanged;
        player.OnHungerChanged += HandleHungerChanged;
        player.OnHungerMaxChanged += HandleHungerMaxChanged;
        player.OnEnergyChanged += HandleEnergyChanged;
        player.OnEnergyMaxChanged += HandleEnergyMaxChanged;
        player.OnSpiritChanged += HandleSpiritChanged;
        player.OnSpiritMaxChanged += HandleSpiritMaxChanged;

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

    private void UpdateCharacterData()
    {
        health.maxValue = player.healthMax;
        hunger.maxValue = player.hungerMax;
        energy.maxValue = player.energyMax;
        spirit.maxValue = player.spiritMax;

        health.value = player.health;
        hunger.value = player.hunger;
        energy.value = player.energy;
        spirit.value = player.spirit;
    }

    private void HandleHpChanged(CharacterData character, int hp)
    {
        health.value = hp;
        //healthText.text = $"{hp} / {player.healthMax}";
    }

    private void HandleHpMaxChanged(CharacterData character, int hpMax)
    {
        health.maxValue = hpMax;
        //healthText.text = $"{player.health} / {hpMax}";
    }

    private void HandleHungerChanged(CharacterData character, int hunger)
    {
        this.hunger.value = hunger;
        //hungerText.text = $"{hunger} / {player.hungerMax}";
    }

    private void HandleHungerMaxChanged(CharacterData character, int hungerMax)
    {
        this.hunger.maxValue = hungerMax;
        //hungerText.text = $"{player.hunger} / {hungerMax}";
    }

    private void HandleEnergyChanged(CharacterData character, int energy)
    {
        this.energy.value = energy;
        //energyText.text = $"{energy} / {player.energyMax}";
    }

    private void HandleEnergyMaxChanged(CharacterData character, int energyMax)
    {
        this.energy.maxValue = energyMax;
        //energyText.text = $"{player.energy} / {energyMax}";
    }

    private void HandleSpiritChanged(CharacterData character, int spirit)
    {
        this.spirit.value = spirit;
        //spiritText.text = $"{spirit} / {player.spiritMax}";
    }

    private void HandleSpiritMaxChanged(CharacterData character, int spiritMax)
    {
        this.spirit.maxValue = spiritMax;
        //spiritText.text = $"{player.spirit} / {spiritMax}";
    }



    private void OnDestroy()
    {
        GameMgr.OnGameTimePaused -= HandleGameTimePaused;
        GameMgr.OnGameTimeResumed -= HandleGameTimeResumed;
        GameMgr.currentSaveData.gameTime.OnTimeChanged -= HandleTimeChanged;
    }

}
