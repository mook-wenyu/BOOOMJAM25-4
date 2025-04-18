using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestJson : MonoBehaviour
{
    public string currentiID;
    

    private void Awake()
    {
        currentiID = "1001";
    }
    public void ShowWord()
    {
        Debug.Log(ConfigManager.Instance.GetConfig<WordConfig>(currentiID).desc);
        currentiID = ConfigManager.Instance.GetConfig<WordConfig>(currentiID).nextid;
    }
}
