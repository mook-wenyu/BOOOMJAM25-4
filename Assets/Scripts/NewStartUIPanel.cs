using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewStartUIPanel : MonoBehaviour
{
    public GameObject uiPanel;
    public TextMeshProUGUI content;
    public TextMeshProUGUI tips;
    private string _content;


    void Awake()
    {
        _content = content.text;
        content.text = string.Empty;
        tips.gameObject.SetActive(false);
        Hide();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!GameMgr.currentSaveData.flags.Contains("new_start"))
        {
            GameMgr.currentSaveData.flags.Add("new_start");
            Show();
        }
        else
        {

            GameMgr.StartTime();
        }
    }

    public void Show()
    {
        uiPanel.SetActive(true);
        uiPanel.GetComponent<Button>().onClick.RemoveAllListeners();
        StartGame().Forget();
    }

    private async UniTask StartGame()
    {
        while (GlobalUIMgr.Instance.IsShowLoadingMask())
        {
            await UniTask.Yield();
        }
        await UniTask.Yield();
        content.gameObject.SetActive(true);

        // 直接使用TypeTextComponent实现打字机效果
        content.TypeText(_content, 0.08f, () =>
        {
            uiPanel.GetComponent<Button>().onClick.AddListener(OnStartBtnClick);
            TextFlickering().Forget();
        });

        // 等待打字效果完成
        while (content.IsSkippable())
        {
            await UniTask.Yield();
        }
    }

    private async UniTask TextFlickering()
    {
        tips.gameObject.SetActive(true);

        // 循环闪烁效果
        while (uiPanel.activeSelf)
        {
            // 淡出效果
            await Tween.Alpha(tips, 0.2f, 2f, Ease.InOutSine);
            // 淡入效果
            await Tween.Alpha(tips, 1f, 1.5f, Ease.InOutSine);
        }
    }

    public void OnStartBtnClick()
    {
        NewStartGame().Forget();
    }

    private async UniTask NewStartGame()
    {
        Hide();
        GameMgr.SetPause();
        GameMgr.StartTime();
        await UniTask.Delay(200);
        DialogueUIPanel.Instance.StartDialogue("Start_Game");
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }
}
