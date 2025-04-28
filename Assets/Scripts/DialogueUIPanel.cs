using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MookDialogueScript;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueUIPanel : MonoSingleton<DialogueUIPanel>
{
    public GameObject dialogueUIPanel;
    public GameObject dialogueContentBG;
    public TypeTextComponent dialogueContent;
    public GameObject dialogueSpeakerBG;
    public TextMeshProUGUI dialogueSpeaker;
    public ScrollRect choiceContainer;
    public GameObject choiceBtn;

    private Button _clickHandler;

    private const float TYPING_SPEED = 0.04f;
    private bool _isTypingFinished = true;
    private CancellationTokenSource _typingCts;

    private ObjectPool<GameObject> _choicePool;
    private List<GameObject> _activeChoices = new List<GameObject>();

    void Awake()
    {
#if UNITY_EDITOR
        if (!GameMgr.initGame)
        {
            SceneManager.LoadScene("StartScene");
            return;
        }
#endif

        // 初始化点击处理
        _clickHandler = dialogueUIPanel.GetComponent<Button>();
        if (_clickHandler == null)
        {
            _clickHandler = dialogueUIPanel.AddComponent<Button>();
        }
        _clickHandler.onClick.AddListener(OnClickHandler);

        // 初始化选项按钮
        choiceContainer.content.DestroyAllChildren();

        // 初始化选项按钮对象池
        _choicePool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(choiceBtn, choiceContainer.content),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: Destroy,
            defaultCapacity: 5,
            maxSize: 10
        );

        DialogueMgr.RunMgrs.OnDialogueDisplayed += HandleDialogueDisplayed;
        DialogueMgr.RunMgrs.OnChoicesDisplayed += HandleChoicesDisplayed;
        DialogueMgr.RunMgrs.OnOptionSelected += HandleOptionSelected;
        DialogueMgr.RunMgrs.OnDialogueCompleted += HandleDialogueCompleted;

        dialogueUIPanel.SetActive(false);
    }

    void Start()
    {
        // 恢复对话
        if (DialogueMgr.RunMgrs.Storage.isInDialogue)
        {
            DialogueMgr.RunMgrs.StartDialogue(DialogueMgr.RunMgrs.Storage.initialNodeName,
                force: true,
                onDialogueStarted: async (nodeName) => await ShowDialogueUIPanel());
        }
    }

    public void StartDialogue(string nodeName = "start")
    {
        DialogueMgr.RunMgrs.StartDialogue(nodeName,
            onDialogueStarted: async (nodeName) => await HandleDialogueStarted(nodeName));
    }

    private async UniTask ShowDialogueUIPanel()
    {
        dialogueUIPanel.SetActive(true);
        dialogueContent.ClearText();
        dialogueSpeaker.text = string.Empty;
        await UniTask.CompletedTask;
    }

    // 对话开始
    private async UniTask HandleDialogueStarted(string nodeName)
    {
        Debug.Log("对话开始");

        if (CharacterMgr.Player().status != CharacterStatus.Explore)
        {
            GameMgr.PauseTime();
            // 存档时机
            await GameMgr.SaveGameData();
        }

        await ShowDialogueUIPanel();
    }

    // 显示对话内容
    private void HandleDialogueDisplayed(DialogueNode dialogue)
    {
        _isTypingFinished = false;

        // 标签
        if (dialogue.Labels != null && dialogue.Labels.Count > 0)
        {
            foreach (var label in dialogue.Labels)
            {
                Debug.Log($"标签：{label}");
            }
        }

        // 角色
        if (!string.IsNullOrEmpty(dialogue.Speaker))
        {
            dialogueSpeaker.text = dialogue.Speaker;
        }
        else
        {
            dialogueSpeaker.text = string.Empty;
        }

        // 对话内容
        HandleDialogueDisplayedAsync(dialogue).Forget();
    }

    // 显示对话内容
    private async UniTask HandleDialogueDisplayedAsync(DialogueNode dialogue)
    {
        string text = await DialogueMgr.RunMgrs.BuildDialogueText(dialogue);

        dialogueContent.SetTypeText(text, onComplete: () =>
        {
            _isTypingFinished = true;
        });
    }

    // 显示选项
    private void HandleChoicesDisplayed(List<ChoiceNode> choices)
    {
        if (_typingCts != null)
        {
            _typingCts.Cancel();
            _typingCts.Dispose();
            _typingCts = null;
        }
        _typingCts = new CancellationTokenSource();
        _ = HandleChoicesDisplayedAsync(choices, _typingCts.Token);
    }

    // 显示选项
    private async UniTask HandleChoicesDisplayedAsync(List<ChoiceNode> choices, CancellationToken token)
    {
        while (!_isTypingFinished)
        {
            await UniTask.Yield(token);
        }

        // 先清除所有现有选项
        ClearActiveChoices();

        // 按顺序创建选项
        for (int i = 0; i < choices.Count; i++)
        {
            var choice = choices[i];
            string text = await DialogueMgr.RunMgrs.BuildChoiceText(choice);
            Debug.Log($"选项：{text}");

            var go = _choicePool.Get();
            _activeChoices.Add(go);
            go.GetComponentInChildren<TextMeshProUGUI>().text = text;

            // 存储选项索引
            int choiceIndex = i;
            go.GetComponent<Button>().onClick.RemoveAllListeners();
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                ClearActiveChoices();
                _ = DialogueMgr.RunMgrs.SelectChoice(choiceIndex);
            });

            // 确保按顺序排列在UI中
            go.transform.SetSiblingIndex(i);
            go.SetActive(true);
        }
    }

    // 选项被选中
    private void HandleOptionSelected(ChoiceNode choice, int index)
    {
        _ = HandleOptionSelectedAsync(choice, index);
    }

    private async UniTask HandleOptionSelectedAsync(ChoiceNode choice, int index)
    {
        string text = await DialogueMgr.RunMgrs.BuildChoiceText(choice);
        Debug.Log($"选择：{index + 1}. {text}");
    }

    private void HandleDialogueCompleted()
    {
        Debug.Log("对话结束");

        dialogueUIPanel.SetActive(false);

        if (CharacterMgr.Player().status != CharacterStatus.Explore)
        {
            // 存档时机
            _ = GameMgr.SaveGameData();
            GameMgr.ResumeTime();
        }

    }

    private void OnClickHandler()
    {
        if (dialogueContent.IsSkippable())
        {
            dialogueContent.SkipTypeText();
        }
        else
        {
            DialogueMgr.RunMgrs.Continue();
        }
    }

    private void ClearActiveChoices()
    {
        if (_activeChoices.Count <= 0) return;
        foreach (var choice in _activeChoices)
        {
            _choicePool.Release(choice);
        }
        _activeChoices.Clear();
    }

    protected override void OnDestroy()
    {
        _choicePool.Clear();
        _choicePool.Dispose();

        DialogueMgr.RunMgrs.OnDialogueDisplayed -= HandleDialogueDisplayed;
        DialogueMgr.RunMgrs.OnChoicesDisplayed -= HandleChoicesDisplayed;
        DialogueMgr.RunMgrs.OnOptionSelected -= HandleOptionSelected;
        DialogueMgr.RunMgrs.OnDialogueCompleted -= HandleDialogueCompleted;

        base.OnDestroy();
    }


}
