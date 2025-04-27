using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPathMover : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private CharacterData _characterData;

    public List<string> PreviousNodeIds { get; private set; } = new List<string>();
    public string CurrentNodeId { get; private set; }
    public bool IsMoving { get; private set; } = false;

    private ExploreNodeData _targetNode;
    private Vector2 _targetPos;

    public event Action<string> OnNodeChanged;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(string mapId)
    {
        _characterData = CharacterMgr.Player();
        _characterData.currentMapId = mapId;
        var currentMap = ExploreNodeMgr.GetExploreMapData(_characterData.currentMapId); // 获取角色当前地图数据
        if (!_characterData.currentMapNodeIds.ContainsKey(_characterData.currentMapId))
        {
            _characterData.currentMapNodeIds[_characterData.currentMapId] = currentMap.startNodeId;
        }
        var startNode = ExploreNodeMgr.GetExploreNodeData(_characterData.currentMapNodeIds[_characterData.currentMapId]); // 获取角色当前节点数据
        CurrentNodeId = startNode.id;
        transform.position = new Vector3(startNode.pos.x, startNode.pos.y, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (IsMoving)
        {
            // 移动到目标位置
            transform.position = Vector2.MoveTowards(transform.position,
            _targetPos,
            _characterData.moveSpeed * Time.deltaTime);

            // 到达目标位置后停止移动
            if (Vector2.Distance(transform.position, _targetPos) < 0.1f)
            {
                transform.position = _targetPos;
                IsMoving = false;
                PreviousNodeIds.Add(CurrentNodeId);
                if (PreviousNodeIds.Count > 10)
                {
                    PreviousNodeIds.RemoveAt(0);
                }
                _ = GameMgr.currentSaveData.gameTime.AddMinutes(1);
                CurrentNodeId = _targetNode.id;
                _characterData.currentMapNodeIds[_characterData.currentMapId] = CurrentNodeId; // 更新角色当前节点ID
                OnNodeChanged?.Invoke(CurrentNodeId);
            }
        }
    }

    public void MoveToNode(string nodeId)
    {
        _targetNode = ExploreNodeMgr.GetExploreNodeData(nodeId);
        _targetPos = new Vector2(_targetNode.pos.x, _targetNode.pos.y);
        IsMoving = true;
    }
}
