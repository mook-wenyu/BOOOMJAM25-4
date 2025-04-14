using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MookDialogueScript;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public GameObject dialogueBoxUI;
    public GameObject dialogueBG;
    public TextMeshProUGUI dialogueContent;
    public TextMeshProUGUI dialogueSpeaker;
    public Transform dialogueChoiceContainer;
    public Button dialogueChoiceBtn;

    private Button _clickHandler;

    private CancellationTokenSource _typingCts;
    private const float TYPING_SPEED = 0.04f;

    private List<ChoiceNode> _choices = new List<ChoiceNode>();

    public void StartDialogue()
    {
        DialogueMgr.RunMgrs.StartDialogue("start");
    }

    void Awake()
    {
#if UNITY_EDITOR
        if (!GameMgr.initGame)
        {
            SceneManager.LoadScene("StartScene");
            return;
        }
#endif
        Debug.Log("MainGame Start");

        // 初始化点击处理
        _clickHandler = dialogueBoxUI.GetComponent<Button>();
        if (_clickHandler == null)
        {
            _clickHandler = dialogueBoxUI.AddComponent<Button>();
        }
        _clickHandler.onClick.AddListener(OnClickHandler);

        DialogueMgr.RunMgrs.OnDialogueStarted += HandleDialogueStarted;
        DialogueMgr.RunMgrs.OnDialogueDisplayed += HandleDialogueDisplayed;
        DialogueMgr.RunMgrs.OnChoicesDisplayed += HandleChoicesDisplayed;
        DialogueMgr.RunMgrs.OnOptionSelected += HandleOptionSelected;
        DialogueMgr.RunMgrs.OnDialogueCompleted += HandleDialogueCompleted;

        dialogueBoxUI.SetActive(false);
    }

    // 停止打字效果
    private void StopTypingEffect()
    {
        if (_typingCts == null) return;
        _typingCts.Cancel();
        _typingCts.Dispose();
        _typingCts = null;
    }

    private async UniTask TypeTextAsync(string text, Action onComplete, CancellationToken cancellationToken)
    {
        try
        {
            foreach (char c in text)
            {
                dialogueContent.text += c;
                await UniTask.Delay(TimeSpan.FromSeconds(TYPING_SPEED), cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // 打字效果被取消时，直接显示完整文本
            dialogueContent.text = text;
#if UNITY_EDITOR
            Debug.Log("打字效果被取消，显示完整文本");
#endif
        }
        finally
        {
            _typingCts = null;
            onComplete?.Invoke();
        }
    }

    private void HandleDialogueStarted()
    {
        Debug.Log("对话开始");
        dialogueBoxUI.SetActive(true);
        dialogueContent.text = string.Empty;

        // 存档时机
        _ = GameMgr.SaveGameData();
    }

    private void HandleDialogueDisplayed(DialogueNode dialogue)
    {
        for (int i = 0; i < dialogueChoiceContainer.childCount; i++)
        {
            Destroy(dialogueChoiceContainer.GetChild(i).gameObject);
        }
        _choices.Clear();

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

        // 情绪
        // if (!string.IsNullOrEmpty(dialogue.Emotion))
        // {
        //     dialogueEmotion.text = dialogue.Emotion;
        // }

        // 对话内容
        _ = HandleDialogueDisplayedAsync(dialogue);
    }

    private async UniTask HandleDialogueDisplayedAsync(DialogueNode dialogue)
    {
        string text = await DialogueMgr.RunMgrs.BuildDialogueText(dialogue);

        _typingCts = new CancellationTokenSource();
        TypeTextAsync(text, () =>
        {
            ShowChoices(_choices);
        }, _typingCts.Token).Forget();
    }

    private void ShowChoices(List<ChoiceNode> choices)
    {
        if (choices.Count <= 0) return;

        foreach (var choice in choices)
        {
            DialogueMgr.RunMgrs.BuildText(choice.Text,
                (text) =>
                {
                    var go = Instantiate(dialogueChoiceBtn, dialogueChoiceContainer);
                    go.GetComponentInChildren<Text>().text = text;
                    go.onClick.AddListener(() =>
                    {
                        _ = DialogueMgr.RunMgrs.SelectChoice(choices.IndexOf(choice));
                    });
                });
        }
    }

    private void HandleChoicesDisplayed(List<ChoiceNode> choices)
    {
        for (int i = 0; i < dialogueChoiceContainer.childCount; i++)
        {
            Destroy(dialogueChoiceContainer.GetChild(i).gameObject);
        }

        _choices = choices;
    }

    private void HandleOptionSelected(ChoiceNode choice, int index)
    {
        DialogueMgr.RunMgrs.BuildText(choice.Text, s => Debug.Log("选择：" + (index + 1) + ". " + s));
    }

    private void HandleDialogueCompleted()
    {
        Debug.Log("对话结束");
        dialogueBoxUI.SetActive(false);

        // 存档时机
        _ = GameMgr.SaveGameData();
    }

    void OnClickHandler()
    {
        if (_typingCts != null)
        {
            StopTypingEffect();
        }
        else
        {
            DialogueMgr.RunMgrs.Continue();
        }
    }

    void OnDestroy()
    {
        DialogueMgr.RunMgrs.OnDialogueStarted -= HandleDialogueStarted;
        DialogueMgr.RunMgrs.OnDialogueDisplayed -= HandleDialogueDisplayed;
        DialogueMgr.RunMgrs.OnChoicesDisplayed -= HandleChoicesDisplayed;
        DialogueMgr.RunMgrs.OnOptionSelected -= HandleOptionSelected;
        DialogueMgr.RunMgrs.OnDialogueCompleted -= HandleDialogueCompleted;
    }


}
