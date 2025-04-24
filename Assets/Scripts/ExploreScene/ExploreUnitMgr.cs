using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploreUnitMgr : MonoSingleton<ExploreUnitMgr>
{
    public GameObject playerGo;

    private UnitPathMover _player;

    void Awake()
    {
        _player = playerGo.GetComponent<UnitPathMover>();
    }

    public UnitPathMover Player()
    {
        if (_player == null)
        {
            _player = playerGo.GetComponent<UnitPathMover>();
        }
        return _player;
    }
}
