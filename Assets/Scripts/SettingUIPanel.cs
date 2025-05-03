using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUIPanel : MonoSingleton<SettingUIPanel>
{
    public GameObject uiPanel;
    public Button closeBtn;

    // Start is called before the first frame update
    void Awake()
    {
        closeBtn.onClick.AddListener(() =>
        {
            Hide();
        });
        Hide();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show()
    {
        uiPanel.SetActive(true);
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }
}
