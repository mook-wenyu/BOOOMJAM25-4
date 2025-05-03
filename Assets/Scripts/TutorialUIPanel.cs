using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUIPanel : MonoSingleton<TutorialUIPanel>
{
    public GameObject uiPanel;
    public Image image;

    private string _previousTutorialId;

    void Awake()
    {
        image = uiPanel.GetComponent<Image>();
        uiPanel.GetComponent<Button>().onClick.AddListener(OnClickHandler);
        Hide();
    }

    public void OnClickHandler()
    {
        switch (_previousTutorialId)
        {
            case "character_status":
                image.sprite = Resources.Load<Sprite>("Tutorial/time");
                _previousTutorialId = "time";
                Debug.Log("time");
                break;
            case "time":
                image.sprite = Resources.Load<Sprite>("Tutorial/sleep");
                _previousTutorialId = "sleep";
                Debug.Log("sleep");
                break;
            case "sleep":
                image.sprite = Resources.Load<Sprite>("Tutorial/inventory");
                _previousTutorialId = "inventory";
                Debug.Log("inventory");
                break;
            case "inventory":
                image.sprite = Resources.Load<Sprite>("Tutorial/move");
                _previousTutorialId = "move";
                Debug.Log("move");
                break;
            case "move":
                GameMgr.currentSaveData.flags.Add("tutorial_main");
                Hide();
                break;

            case "explore_map":
                image.sprite = Resources.Load<Sprite>("Tutorial/explore_info");
                _previousTutorialId = "explore_info";
                Debug.Log("explore_info");
                break;
            case "explore_info":
                GameMgr.currentSaveData.flags.Add("tutorial_explore_map");
                Hide();
                break;

            case "explore_1":
                image.sprite = Resources.Load<Sprite>("Tutorial/explore_2");
                _previousTutorialId = "explore_2";
                Debug.Log("explore_2");
                break;
            case "explore_2":
                image.sprite = Resources.Load<Sprite>("Tutorial/explore_3");
                _previousTutorialId = "explore_3";
                Debug.Log("explore_3");
                break;
            case "explore_3":
                GameMgr.currentSaveData.flags.Add("tutorial_explore");
                Hide();
                break;
        }
    }

    public void ShowMain()
    {
        this._previousTutorialId = "character_status";
        image.sprite = Resources.Load<Sprite>("Tutorial/character_status");
        Debug.Log("character_status");
        uiPanel.SetActive(true);
    }

    public void ShowReadyExplore()
    {
        this._previousTutorialId = "explore_map";
        image.sprite = Resources.Load<Sprite>("Tutorial/explore_map");
        Debug.Log("explore_map");
        uiPanel.SetActive(true);
    }

    public void EnterExplore()
    {
        this._previousTutorialId = "explore_1";
        image.sprite = Resources.Load<Sprite>("Tutorial/explore_1");
        Debug.Log("explore_1");
        uiPanel.SetActive(true);
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }
}
