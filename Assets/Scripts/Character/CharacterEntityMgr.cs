using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEntityMgr : MonoSingleton<CharacterEntityMgr>
{
    public Transform characterContainer;
    public GameObject playerGo;
    private Player player;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Player GetPlayer()
    {
        if (player == null)
        {
            player = playerGo.GetComponent<Player>();
        }
        return player;
    }
}
