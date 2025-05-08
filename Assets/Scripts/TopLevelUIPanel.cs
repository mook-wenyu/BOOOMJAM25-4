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

    public GameObject mainOptions;

    public Transform buffContainer;
    public GameObject buffPrefab;

    public GameObject healthGO, hungerGO, energyGO, spiritGO;
    public Slider healthSlider, hungerSlider, energySlider, spiritSlider;
    public TextMeshProUGUI healthText, hungerText, energyText, spiritText;

    public Button goOut, comeBack;

    private CharacterData player;

    private void Awake()
    {
        buffContainer.DestroyAllChildren();

        GameMgr.OnGameTimePaused += HandleGameTimePaused;
        GameMgr.OnGameTimeResumed += HandleGameTimeResumed;
        GameMgr.currentSaveData.gameTime.OnTimeChanged += HandleTimeChanged;

        goOut.onClick.AddListener(OnGoOutBtnClicked);
        comeBack.onClick.AddListener(OnComeBackBtnClicked);
        goOut.gameObject.SetActive(false);
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

        healthGO.GetComponent<CharacterStatusUI>().Setup($"{ConfigManager.Instance.GetConfig<LanguageConfig>("hp").title}\n{ConfigManager.Instance.GetConfig<LanguageConfig>("hp").desc}");
        hungerGO.GetComponent<CharacterStatusUI>().Setup($"{ConfigManager.Instance.GetConfig<LanguageConfig>("hunger").title}\n{ConfigManager.Instance.GetConfig<LanguageConfig>("hunger").desc}");
        energyGO.GetComponent<CharacterStatusUI>().Setup($"{ConfigManager.Instance.GetConfig<LanguageConfig>("energy").title}\n{ConfigManager.Instance.GetConfig<LanguageConfig>("energy").desc}");
        spiritGO.GetComponent<CharacterStatusUI>().Setup($"{ConfigManager.Instance.GetConfig<LanguageConfig>("spirit").title}\n{ConfigManager.Instance.GetConfig<LanguageConfig>("spirit").desc}");
    }

    void Start()
    {
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

        HandleTimeChanged(GameMgr.currentSaveData.gameTime).Forget();
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
        HandleHpMaxChanged(player);
        HandleHungerMaxChanged(player);
        HandleEnergyMaxChanged(player);
        HandleSpiritMaxChanged(player);

        HandleHpChanged(player);
        HandleHungerChanged(player);
        HandleEnergyChanged(player);
        HandleSpiritChanged(player);

        foreach (var buff in player.activeBuffs)
        {
            HandleBuffAdded(player, buff);
        }
    }

    private void HandleHpChanged(CharacterData character)
    {
        healthSlider.value = character.health;
        healthText.text = $"{character.health} / {character.healthMax}";
        if (character.health <= 0)
        {
            GameMgr.PauseTime();
            GlobalUIMgr.Instance.ShowMessage("您已死亡！");
            GameMgr.End_Game();
        }
    }

    private void HandleHpMaxChanged(CharacterData character)
    {
        healthSlider.maxValue = character.healthMax;
        healthText.text = $"{character.health} / {character.healthMax}";
    }

    private void HandleHungerChanged(CharacterData character)
    {
        hungerSlider.value = character.hunger;
        hungerText.text = $"{character.hunger} / {character.hungerMax}";

        CharacterEntityMgr.Instance.GetPlayer().SetIconState(character.hunger <= 10); // 饥饿图标显示条件
    }

    private void HandleHungerMaxChanged(CharacterData character)
    {
        hungerSlider.maxValue = character.hungerMax;
        hungerText.text = $"{character.hunger} / {character.hungerMax}";
    }

    public void HandleEnergyChanged(CharacterData character)
    {
        energySlider.value = character.energy;
        energyText.text = $"{character.energy} / {character.energyMax}";
    }

    private void HandleEnergyMaxChanged(CharacterData character)
    {
        energySlider.maxValue = character.energyMax;
        energyText.text = $"{character.energy} / {character.energyMax}";
    }

    private void HandleSpiritChanged(CharacterData character)
    {
        spiritSlider.value = character.spirit;
        spiritText.text = $"{character.spirit} / {character.spiritMax}";
    }

    private void HandleSpiritMaxChanged(CharacterData character)
    {
        spiritSlider.maxValue = character.spiritMax;
        spiritText.text = $"{character.spirit} / {character.spiritMax}";
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
        // goOut.gameObject.SetActive(true);
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

        player.OnHpChanged -= HandleHpChanged;
        player.OnHpMaxChanged -= HandleHpMaxChanged;
        player.OnHungerChanged -= HandleHungerChanged;
        player.OnHungerMaxChanged -= HandleHungerMaxChanged;
        player.OnEnergyChanged -= HandleEnergyChanged;
        player.OnEnergyMaxChanged -= HandleEnergyMaxChanged;
        player.OnSpiritChanged -= HandleSpiritChanged;
        player.OnSpiritMaxChanged -= HandleSpiritMaxChanged;
        player.OnBuffAdded -= HandleBuffAdded;
        player.OnBuffRemoved -= HandleBuffRemoved;

        GameMgr.StopTime();

        base.OnDestroy();
    }

}
