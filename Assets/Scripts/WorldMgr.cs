using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WorldMgr : MonoSingleton<WorldMgr>
{
    public Light2D globalLight;

    public GameObject blackScreen;

    void Awake()
    {
        blackScreen.SetActive(false);
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
