using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ExploreNodeEntityMgr : MonoSingleton<ExploreNodeEntityMgr>
{
    public UnitPathMover playerUnit;

    public Transform nodeRoot;
    public GameObject nodePrefab;

    void Awake()
    {
        nodeRoot.DestroyAllChildren();
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
        playerUnit.Init(ExploreNodeMgr.currentMapId);
        playerUnit.OnNodeChanged += HandleNodeChanged;
    }

    /// <summary>
    /// 生成地图
    /// </summary>
    public void GenerateMap()
    {
        var nodes = ExploreNodeMgr.GetExploreMapData(ExploreNodeMgr.currentMapId).nodes.Values;
        foreach (var node in nodes)
        {
            var config = node.GetConfig();
            if (!config.isOnMap)
                continue;

            node.OnNodeReplaced += HandleNodeReplaced;
            node.OnNodeCompleted += HandleNodeCompleted;
            var nodeObj = Instantiate(nodePrefab, nodeRoot);
            nodeObj.name = node.id;
            nodeObj.transform.position = new Vector3(node.pos.x, node.pos.y, nodeRoot.position.z);
            ExploreNodeEntity nodeEntity = nodeObj.GetComponent<ExploreNodeEntity>();
            nodeEntity.Setup(node);
            nodeEntity.OnClick += HandleNodeClicked;

            nodeEntity.gameObject.SetActive(false);
        }

        // 重新生成路径
        RoadPathMgr.Instance.GenerateAllRoadPaths(nodes.ToList());

        // 初始化地图显示
        foreach (var node in nodes)
        {
            if (node.isCompleted)
            {
                var nodeConfig = node.GetConfig();
                // 显示节点
                nodeRoot.Find(node.id).gameObject.SetActive(true);
                // 默认解锁
                if (nodeConfig.unlocksMidNodes != null && nodeConfig.unlocksMidNodes.Length > 0)
                {
                    foreach (var nodeId in nodeConfig.unlocksMidNodes)
                    {
                        UnlockNode(nodeId);
                        ActivePath(node.id, nodeId);
                    }
                }

                // 完成后解锁
                if (nodeConfig.unlocksPostNodes != null && nodeConfig.unlocksPostNodes.Length > 0)
                {
                    foreach (var nodeId in nodeConfig.unlocksPostNodes)
                    {
                        UnlockNode(nodeId);
                        ActivePath(node.id, nodeId);
                    }
                }
            }
        }
    }

    // 解锁节点
    private void UnlockNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId) || nodeId == "0") return;
        nodeRoot.Find(nodeId).gameObject.SetActive(true);
    }

    // 激活路径
    private void ActivePath(string nodeId, string targetNodeId)
    {
        var lineId = RoadPathMgr.Instance.GetPathPairId(nodeId, targetNodeId);
        if (RoadPathMgr.Instance.GetPathRenderers().TryGetValue(lineId, out var lineRenderer))
            lineRenderer.gameObject.SetActive(true);
    }

    // 处理节点改变事件
    private void HandleNodeChanged(string nodeId)
    {
        var targetNode = ExploreNodeMgr.GetExploreNodeData(nodeId);
        var targetConfig = targetNode.GetConfig();

        // 默认解锁
        if (targetConfig.unlocksMidNodes != null && targetConfig.unlocksMidNodes.Length > 0)
        {
            foreach (var unlockNodeId in targetConfig.unlocksMidNodes)
            {
                UnlockNode(unlockNodeId);
                ActivePath(targetNode.id, unlockNodeId);
            }
        }

        HandleNodeInteraction(targetNode, targetConfig);

        Debug.Log($"节点改变: {nodeId}");
    }

    // 处理节点替换事件
    private void HandleNodeReplaced(ExploreNodeData node)
    {
        HideAllUIPanels();

        var nodeObj = nodeRoot.Find(node.id).gameObject;
        var nodeEntity = nodeObj.GetComponent<ExploreNodeEntity>();
        nodeEntity.Setup(node);

        var nodeConfig = node.GetConfig();
        // 默认解锁
        if (nodeConfig.unlocksMidNodes != null && nodeConfig.unlocksMidNodes.Length > 0)
        {
            foreach (var unlockNodeId in nodeConfig.unlocksMidNodes)
            {
                UnlockNode(unlockNodeId);
                ActivePath(node.id, unlockNodeId);
            }
        }

        Debug.Log($"节点替换: {node.id} -> {node.changedId}");
    }

    // 处理节点完成事件
    private void HandleNodeCompleted(ExploreNodeData node)
    {
        var nodeConfig = node.GetConfig();

        HideAllUIPanels();

        if (node.isCompleted)
        {
            // 完成后解锁
            if (nodeConfig.unlocksPostNodes != null && nodeConfig.unlocksPostNodes.Length > 0)
            {
                foreach (var nodeId in nodeConfig.unlocksPostNodes)
                {
                    UnlockNode(nodeId);
                    ActivePath(node.id, nodeId);
                }
            }

        }

        if (!string.IsNullOrEmpty(nodeConfig.nodeAfterComplete) && nodeConfig.nodeAfterComplete != "0")
        {
            node.SetChangedId(nodeConfig.nodeAfterComplete);
        }
    }

    // 处理节点点击事件
    private void HandleNodeClicked(ExploreNodeData targetNode)
    {
        if (playerUnit.IsMoving) return;

        var currentNode = ExploreNodeMgr.GetExploreNodeData(playerUnit.CurrentNodeId);
        var targetConfig = targetNode.GetConfig();

        if (currentNode.id == targetNode.id)
        {
            HandleNodeInteraction(targetNode, targetConfig);
            return;
        }

        if (!CanMoveToNode(currentNode, targetNode, targetConfig))
        {
            return;
        }

        // 检查需要消耗的时间
        int consumeTime = 1;
        if (!GameMgr.currentSaveData.gameTime.IsTimeBefore(new GameTime(GameMgr.currentSaveData.gameTime.day + 1, 0, 0), consumeTime))
        {
            GlobalUIMgr.Instance.ShowMessage("太晚了，先回家吧！");
            return; // 时间不足，
        }
        int energyCost = 1;
        // 检查需要消耗的体力
        if (CharacterMgr.Player().energy < energyCost)
        {
            GlobalUIMgr.Instance.ShowMessage("体力不足，无法前进！");
            return; // 体力不足，
        }

        HideAllUIPanels();

        MovePlayerToNode(targetNode);

        _ = GameMgr.currentSaveData.gameTime.AddMinutes(consumeTime);
        CharacterMgr.Player().SetEnergy(CharacterMgr.Player().energy - energyCost);

        Debug.Log($"点击节点: {targetNode.id}");
    }

    // 检查是否可以移动到目标节点
    private bool CanMoveToNode(ExploreNodeData currentNode, ExploreNodeData targetNode, ExploreNodeConfig targetConfig)
    {
        if (!currentNode.isCompleted && !targetNode.isCompleted)
        {
            Debug.Log($"当前节点未完成，无法到达新节点: {targetNode.id}，当前节点: {currentNode.id}");
            return false;
        }

        if (!targetNode.neighborNodes.Contains(currentNode.id) && targetConfig.type != (int)ExploreNodeType.Functional)
        {
            Debug.Log($"无法到达节点: {targetNode.id}，当前节点: {currentNode.id}");
            return false;
        }

        return true;
    }

    // 移动玩家到目标节点
    private void MovePlayerToNode(ExploreNodeData targetNode)
    {
        playerUnit.MoveToNode(targetNode.id);
    }

    // 隐藏所有UI面板
    private void HideAllUIPanels()
    {
        WarehouseUIPanel.Instance.Hide();
        ProductionPlatformUIPanel.Instance.Hide();
        SubmitUIPanel.Instance.Hide();
    }

    // 处理节点交互
    private void HandleNodeInteraction(ExploreNodeData node, ExploreNodeConfig config)
    {
        Debug.Log($"节点交互: {node.id}");
        switch ((ExploreNodeType)config.type)
        {
            case ExploreNodeType.Empty:
            case ExploreNodeType.Functional:
                node.SetCompleted();
                break;

            case ExploreNodeType.Reward:
                HandleRewardNode(node);
                break;

            case ExploreNodeType.Submit:
                HandleSubmitNode(node);
                break;

            case ExploreNodeType.Production:
                HandleProductionNode(node, config);
                break;

            case ExploreNodeType.Story:
                HandleStoryNode(node, config);
                break;
        }
    }

    // 奖励节点
    private void HandleRewardNode(ExploreNodeData node)
    {
        node.SetCompleted();
        if (!BuildingMgr.HasBuildingData(node.id))
        {
            BuildingMgr.AddBuildingData(new WarehouseBuildingData("20001", node.id, WarehouseType.Box, 9));
        }
        if (!WarehouseUIPanel.Instance.uiPanel.activeSelf)
            WarehouseUIPanel.Instance.Show(node.id);
        else
            WarehouseUIPanel.Instance.Hide();
    }

    // 提交节点
    private void HandleSubmitNode(ExploreNodeData node)
    {
        if (!node.isCompleted)
        {
            if (!SubmitUIPanel.Instance.uiPanel.activeSelf)
                SubmitUIPanel.Instance.Show(node);
            else
                SubmitUIPanel.Instance.Hide();
        }
    }

    // 生产节点
    private void HandleProductionNode(ExploreNodeData node, ExploreNodeConfig config)
    {
        node.SetCompleted();
        if (!BuildingMgr.HasBuildingData(node.id))
        {
            BuildingMgr.AddBuildingData(new ProductionBuildingData("20004", node.id, config.recipeIdGroup.ToList()));
        }
        if (!ProductionPlatformUIPanel.Instance.uiPanel.activeSelf)
            ProductionPlatformUIPanel.Instance.Show(node.id);
        else
            ProductionPlatformUIPanel.Instance.Hide();
    }

    // 剧情节点
    private void HandleStoryNode(ExploreNodeData node, ExploreNodeConfig config)
    {
        if (!node.isCompleted)
        {
            DialogueUIPanel.Instance.StartDialogue(config.storyId);
        }
    }

    public void Clear()
    {
        foreach (var node in ExploreNodeMgr.GetExploreMapData(ExploreNodeMgr.currentMapId).nodes.Values)
        {
            node.OnNodeReplaced -= HandleNodeReplaced;
            node.OnNodeCompleted -= HandleNodeCompleted;
        }
        nodeRoot.DestroyAllChildren();
    }

}
