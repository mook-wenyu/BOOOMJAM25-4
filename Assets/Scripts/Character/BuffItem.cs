using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffItem : MonoBehaviour
{
    public Image bg;

    public ActiveBuff CurrentBuff { get; private set; }

    public void Setup(ActiveBuff buff)
    {
        CurrentBuff = buff;
        
    }
}
