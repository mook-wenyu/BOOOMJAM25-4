using System.Collections;
using System.Collections.Generic;
using MookDialogueScript;
using UnityEngine;

public static class DialogueMgr
{
    public static Runner RunMgrs { get; private set; }

    public static void Init()
    {
        RunMgrs = new Runner("DialogueScripts");
    }

}
