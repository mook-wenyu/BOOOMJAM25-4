using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldMgr : MonoSingleton<WorldMgr>
{
    public Light2D globalLight;

    public GameObject blackScreen;

    public GameObject endGameScreen;
    public Button endGameBtn;

    void Awake()
    {
        blackScreen.SetActive(false);

        endGameBtn.onClick.AddListener(OnEndGameBtnClicked);
        endGameScreen.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnEndGameBtnClicked()
    {
        SceneManager.LoadScene("StartScene");
    }
}
