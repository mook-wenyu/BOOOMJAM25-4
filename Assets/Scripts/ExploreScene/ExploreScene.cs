using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExploreScene : MonoBehaviour
{
    void Awake()
    {
#if UNITY_EDITOR
        if (!GameMgr.initGame)
        {
            SceneManager.LoadScene("InitScene");
            return;
        }
#endif
        AudioMgr.Instance.PlayMusic("bgm室外");
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
