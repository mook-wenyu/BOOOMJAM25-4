using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPathMover : MonoBehaviour
{
    public string CurrentNodeId { get; set; }

    private SpriteRenderer _spriteRenderer;

    private CharacterData _characterData;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init()
    {
        _characterData = CharacterMgr.Player();
        var currentMap = ExploreNodeMgr.GetExploreMapData(ExploreNodeMgr.currentMapId);
        var startNode = ExploreNodeMgr.GetExploreNodeData(currentMap.startNodeId);
        CurrentNodeId = startNode.id;
        transform.position = new Vector3(startNode.pos.x, startNode.pos.y, transform.position.z);
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
