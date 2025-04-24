using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubmitUIPanel : MonoSingleton<SubmitUIPanel>
{
    public GameObject uiPanel;

    public TextMeshProUGUI titleName;

    public TextMeshProUGUI desc, required, time;
    public Button startBtn;

    void Awake()
    {
        titleName.text = "提交";
        Hide();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
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
