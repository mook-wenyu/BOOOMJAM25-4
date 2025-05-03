using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class TopLevelUIPanel : MonoSingleton<TopLevelUIPanel>
{
    public GameObject timeInfo;
    public TextMeshProUGUI dataText;
    public TextMeshProUGUI timeText;

    public Button settingBtn;

    public Button saveBtn;
    public TMP_InputField sayIdInput;
    public Button sayBtn;
    public TMP_InputField itemIdInput;
    public Button itemAddBtn, backNode, fullAll;

    public Transform buffContainer;
    public GameObject buffPrefab;

    public Slider health, hunger, energy, spirit;
    public TextMeshProUGUI healthText, hungerText, energyText, spiritText;

    public Button goOut, comeBack;

    private CharacterData player;

    private void Awake()
    {
        buffContainer.DestroyAllChildren();

        GameMgr.OnGameTimePaused += HandleGameTimePaused;
        GameMgr.OnGameTimeResumed += HandleGameTimeResumed;
        GameMgr.currentSaveData.gameTime.OnTimeChanged += HandleTimeChanged;

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
        player.OnBuffAdded += HandleBuffAdded;
        player.OnBuffRemoved += HandleBuffRemoved;

        goOut.onClick.AddListener(OnGoOutBtnClicked);
        comeBack.onClick.AddListener(OnComeBackBtnClicked);
        comeBack.gameObject.SetActive(false);

        settingBtn.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlaySound("点击");
            SettingUIPanel.Instance.Show();
        });

        saveBtn.onClick.AddListener(() => _ = GameMgr.SaveGameData());

        itemAddBtn.onClick.AddListener(() =>
        {
            InventoryMgr.GetPlayerInventoryData().AddItem(itemIdInput.text.Trim(), 1);
        });

        sayBtn.onClick.AddListener(() =>
        {
            DialogueUIPanel.Instance.StartDialogue(sayIdInput.text.Trim());
        });

        backNode.onClick.AddListener(() =>
        {
            ExploreNodeMgr.BackNode();
        });
        fullAll.onClick.AddListener(() =>
        {
            player.health = player.healthMax;
            player.hunger = player.hungerMax;
            player.energy = player.energyMax;
            player.spirit = player.spiritMax;
        });
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

        healthText.text = $"{player.health} / {player.healthMax}";
        hungerText.text = $"{player.hunger} / {player.hungerMax}";
        energyText.text = $"{player.energy} / {player.energyMax}";
        spiritText.text = $"{player.spirit} / {player.spiritMax}";

        foreach (var buff in player.activeBuffs)
        {
            HandleBuffAdded(player, buff);
        }
    }

    private void HandleHpChanged(CharacterData character, float hp)
    {
        health.value = hp;
        healthText.text = $"{hp} / {player.healthMax}";
        if (hp <= 0)
        {
            GameMgr.PauseTime();
            GlobalUIMgr.Instance.ShowMessage("您已死亡！");
        }
    }

    private void HandleHpMaxChanged(CharacterData character, float hpMax)
    {
        health.maxValue = hpMax;
        healthText.text = $"{player.health} / {hpMax}";
    }

    private void HandleHungerChanged(CharacterData character, float hunger)
    {
        this.hunger.value = hunger;
        hungerText.text = $"{hunger} / {player.hungerMax}";
    }

    private void HandleHungerMaxChanged(CharacterData character, float hungerMax)
    {
        this.hunger.maxValue = hungerMax;
        hungerText.text = $"{player.hunger} / {hungerMax}";
    }

    private void HandleEnergyChanged(CharacterData character, float energy)
    {
        this.energy.value = energy;
        energyText.text = $"{energy} / {player.energyMax}";
    }

    private void HandleEnergyMaxChanged(CharacterData character, float energyMax)
    {
        this.energy.maxValue = energyMax;
        energyText.text = $"{player.energy} / {energyMax}";
    }

    private void HandleSpiritChanged(CharacterData character, float spirit)
    {
        this.spirit.value = spirit;
        spiritText.text = $"{spirit} / {player.spiritMax}";
    }

    private void HandleSpiritMaxChanged(CharacterData character, float spiritMax)
    {
        this.spirit.maxValue = spiritMax;
        spiritText.text = $"{player.spirit} / {spiritMax}";
    }

    private void HandleBuffAdded(CharacterData character, ActiveBuff buff)
    {
        var buffObj = Instantiate(buffPrefab, buffContainer);
        buffObj.name = buff.instanceId;
        buffObj.GetComponent<BuffItem>().Setup(buff);
    }

    private void HandleBuffRemoved(CharacterData character, ActiveBuff buff)
    {
        var buffObj = buffContainer.Find(buff.instanceId).gameObject;
        if (buffObj != null)
        {
            Destroy(buffObj);
        }
    }

    public void OnGoOutBtnClicked()
    {
        AudioMgr.Instance.PlaySound("点击");
        ExploreMapUIPanel.Instance.Show();
    }

    public void OnComeBackBtnClicked()
    {
        AudioMgr.Instance.PlaySound("点击");
        GlobalUIMgr.Instance.ShowLoadingMask(true);
        comeBack.gameObject.SetActive(false);
        goOut.gameObject.SetActive(true);
        SceneMgr.Instance.UnloadScene("ExploreScene");
        ExploreNodeMgr.currentMapId = string.Empty;
        CharacterMgr.Player().currentMapId = string.Empty;
        CharacterMgr.Player().currentMapNodeIds.Clear();
        CharacterMgr.Player().SetStatus(CharacterStatus.Idle);
        GameMgr.ResumeTime();
    }

    protected override void OnDestroy()
    {
        GameMgr.OnGameTimePaused -= HandleGameTimePaused;
        GameMgr.OnGameTimeResumed -= HandleGameTimeResumed;
        GameMgr.currentSaveData.gameTime.OnTimeChanged -= HandleTimeChanged;

        base.OnDestroy();
    }

}
