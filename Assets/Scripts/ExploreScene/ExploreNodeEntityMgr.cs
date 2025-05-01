using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cinemachine;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExploreNodeEntityMgr : MonoSingleton<ExploreNodeEntityMgr>
{
    public CinemachineVirtualCamera virtualCamera; // 添加虚拟相机引用
    public CinemachineConfiner2D confiner2D;       // 添加 Confiner2D 引用

    public UnitPathMover playerUnit;

    public Transform nodeRoot;
    public GameObject nodePrefab;

    private string currentMapId;

    private Vector3 _lastMousePosition;
    private bool _isDragging = false;

    private const float DRAG_SPEED = 2f;                              // 设置为 2 以实现 1:1 的移动比例

    void Awake()
    {
        nodeRoot.DestroyAllChildren();

        if (!virtualCamera)
        {
            virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
        }

        if (!confiner2D)
        {
            confiner2D = virtualCamera.GetComponent<CinemachineConfiner2D>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        currentMapId = ExploreNodeMgr.currentMapId;
        GenerateMap();
        playerUnit.Init(ExploreNodeMgr.currentMapId);
        playerUnit.OnNodeChanged += HandleNodeChanged;

        CenterCameraOnUnit(true);
    }

    // Update is called once per frame
    void Update()
    {
        HandleMapDragging();
    }

    /// <summary>
    /// 将相机中心移动到单位位置
    /// </summary>
    /// <param name="instant">是否立即移动</param>
    public void CenterCameraOnUnit(bool instant = false)
    {
        if (virtualCamera != null && playerUnit != null)
        {
            Vector3 unitPosition = playerUnit.transform.position;
            if (instant)
                virtualCamera.transform.position = new Vector3(unitPosition.x, unitPosition.y, virtualCamera.transform.position.z);
            else
                Tween.Position(virtualCamera.transform, new Vector3(unitPosition.x, unitPosition.y, virtualCamera.transform.position.z), 0.2f);
        }
    }


    /// <summary>
    /// 处理地图拖动
    /// </summary>
    private void HandleMapDragging()
    {
        // 检查是否点击了UI元素
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return; // 如果点击了UI，直接返回不处理拖动
        }

        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            _lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }

        if (_isDragging)
        {
            Vector3 delta = Input.mousePosition - _lastMousePosition;
            Vector3 translate = new Vector3(-delta.x, -delta.y, 0) * Camera.main.orthographicSize / Screen.height * DRAG_SPEED;

            // 计算预期位置
            Vector3 targetPosition = virtualCamera.transform.position + translate;

            // 检查是否超出边界
            if (confiner2D && confiner2D.enabled)
            {
                Bounds bounds = confiner2D.m_BoundingShape2D.bounds;
                // 获取相机的正交尺寸
                float orthoSize = virtualCamera.m_Lens.OrthographicSize;
                // 计算相机视口的宽度
                float aspectRatio = Screen.width / (float)Screen.height;
                float horizontalSize = orthoSize * aspectRatio;

                // 限制目标位置在边界内
                targetPosition.x = Mathf.Clamp(targetPosition.x,
                    bounds.min.x + horizontalSize,
                    bounds.max.x - horizontalSize);
                targetPosition.y = Mathf.Clamp(targetPosition.y,
                    bounds.min.y + orthoSize,
                    bounds.max.y - orthoSize);

            }
            virtualCamera.transform.position = targetPosition;

            _lastMousePosition = Input.mousePosition;
        }
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

        MovePlayerToNode(targetNode);

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

        if (!targetNode.neighborNodes.Contains(currentNode.id) && !currentNode.neighborNodes.Contains(targetNode.id) && targetConfig.type != (int)ExploreNodeType.Functional)
        {
            Debug.Log($"无法到达节点: {targetNode.id}，当前节点: {currentNode.id}");
            return false;
        }

        return true;
    }

    // 移动玩家到目标节点
    private bool MovePlayerToNode(ExploreNodeData targetNode)
    {
        HideAllUIPanels();

        // 检查需要消耗的时间 0.5小时
        int consumeTime = GameTime.HourToMinute(Utils.GetGeneralParametersConfig("nodeTimeCost").par);
        if (!GameMgr.currentSaveData.gameTime.IsTimeBefore(new GameTime(GameMgr.currentSaveData.gameTime.day + 1, 0, 0), consumeTime))
        {
            GlobalUIMgr.Instance.ShowMessage("太晚了，先回家吧！");
            return false;
        }
        float energyCost = (float)Utils.GetGeneralParametersConfig("nodeEnergyCost").par;
        // 检查需要消耗的体力
        if (CharacterMgr.Player().energy < energyCost)
        {
            GlobalUIMgr.Instance.ShowMessage("体力不足，无法前进！");
            return false;
        }

        playerUnit.MoveToNode(targetNode.id);

        _ = GameMgr.currentSaveData.gameTime.AddMinutes(consumeTime);
        CharacterMgr.Player().DecreaseEnergy(energyCost);

        return true;
    }

    // 移动玩家到上一个节点
    public void MovePlayerPreviousNode()
    {
        if (playerUnit.PreviousNodeIds.Count > 0)
        {
            if (!MovePlayerToNode(ExploreNodeMgr.GetExploreNodeData(playerUnit.PreviousNodeIds.Last())))
            {
                Debug.Log("无法返回上一个节点");
            }
        }
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
        if (!InventoryMgr.HasInventoryData(node.id))
        {
            InventoryMgr.CreateWarehouseData(node.id, node.GetConfig().name, WarehouseType.Box, 9);
            // 添加奖励物品
            var warehouseData = InventoryMgr.GetWarehouseData(node.id);
            var config = node.GetConfig();
            for (int i = 0; i < config.rewardIdGroup.Length; i++)
            {
                warehouseData.AddItem(config.rewardIdGroup[i], config.rewardAmountGroup[i]);
            }
        }
        if (!WarehouseUIPanel.Instance.uiPanel.activeSelf)
            WarehouseUIPanel.Instance.ShowWarehouse(node.id, true);
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
        if (!ProductionPlatformMgr.HasProductionPlatformData(node.id))
        {
            ProductionPlatformMgr.CreateProductionPlatformData(node.id, config.name, config.recipeIdGroup.ToList());
        }
        if (!ProductionPlatformUIPanel.Instance.uiPanel.activeSelf)
            ProductionPlatformUIPanel.Instance.ShowProductionPlatform(node.id, true);
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
        HideAllUIPanels();
        playerUnit.OnNodeChanged -= HandleNodeChanged;
        foreach (var node in ExploreNodeMgr.GetExploreMapData(currentMapId).nodes.Values)
        {
            node.OnNodeReplaced -= HandleNodeReplaced;
            node.OnNodeCompleted -= HandleNodeCompleted;
        }
        nodeRoot.DestroyAllChildren();
    }

    protected override void OnDestroy()
    {
        Clear();
    }

}
