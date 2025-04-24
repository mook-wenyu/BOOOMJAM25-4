using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExploreNodeEntityMgr : MonoSingleton<ExploreNodeEntityMgr>
{
    public UnitPathMover playerUnit;

    public Transform nodeRoot;
    public GameObject nodePrefab;

    private static Dictionary<string, ExploreNodeData> _nodeDict = new();

    void Awake()
    {
        nodeRoot.DestroyAllChildren();
        _nodeDict.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
        playerUnit.Init();
    }

    /// <summary>
    /// 生成地图
    /// </summary>
    public void GenerateMap()
    {
        var nodes = ExploreNodeMgr.GetExploreMapData(ExploreNodeMgr.currentMapId).nodes.Values;
        foreach (var node in nodes)
        {
            var nodeObj = Instantiate(nodePrefab, nodeRoot);
            nodeObj.name = node.id;
            var config = node.GetConfig();
            nodeObj.transform.position = new Vector3(config.mapLocation[0], config.mapLocation[1], nodeRoot.position.z);
            ExploreNodeEntity nodeEntity = nodeObj.GetComponent<ExploreNodeEntity>();
            nodeEntity.Setup(node);
            nodeEntity.OnClick += HandleNodeClicked;
            /*if (node.type == PlaceType.Area)
            {
                Area area = AreaMgr.GetArea(node.id);
                if (area == null)
                {
                    area = new AreaBuilder(node.id)
                        .SetName($"Area {node.id}")
                        .SetDesc($"Area {node.id} Description")
                        .SetBgPath("/citybg_1")
                        .SetType(AreaType.City)
                        .SetBuildingType(BuildingType.None)
                        .SetIconPath("/city1")
                        .Build();
                }
                

                if (!string.IsNullOrEmpty(area.iconPath))
                {
                    // placeEntity.SetPic(area.iconPath);
                }

            }
            */
            _nodeDict[node.id] = node;
        }

        // 重新生成路径
        RoadPathMgr.Instance.GenerateAllRoadPaths(_nodeDict.Values.ToList());
    }

    private void HandleNodeClicked(ExploreNodeData targetNode)
    {
        var currentNode = _nodeDict.GetValueOrDefault(playerUnit.CurrentNodeId, null);
        var currentConfig = currentNode.GetConfig();
        var targetConfig = targetNode.GetConfig();
        if (currentNode.id == targetNode.id)
        {
            Debug.Log($"点击当前节点: {currentNode.id}");
            return;
        }
        if (!currentNode.isCompleted && !targetNode.isCompleted)
        {
            Debug.Log($"当前节点未完成，无法到达节点: {targetNode.id}，当前节点: {currentNode.id}");
            return;
        }

        if (!targetNode.neighborNodes.Contains(currentNode.id) && targetConfig.type != (int)ExploreNodeType.Functional)
        {
            Debug.Log($"无法到达节点: {targetNode.id}，当前节点: {currentNode.id}");
            return;
        }


        playerUnit.CurrentNodeId = targetNode.id;
        playerUnit.transform.position = new Vector3(targetNode.pos.x, targetNode.pos.y, playerUnit.transform.position.z);

        switch (targetConfig.type)
        {
            case (int)ExploreNodeType.Empty:
                targetNode.SetCompleted(true);

                break;
            case (int)ExploreNodeType.Functional:
                // 触发功能
                targetNode.SetCompleted(true);
                break;
            case (int)ExploreNodeType.Reward:
                // 触发奖励
                targetNode.SetCompleted(true);
                if (!BuildingMgr.HasBuildingData(targetNode.id))
                {
                    Debug.Log("创建仓库");
                    BuildingMgr.AddBuildingData(new WarehouseBuildingData("20001", targetNode.id, WarehouseType.Box, 9));
                }

                WarehouseUIPanel.Instance.Show(targetNode.id);
                break;
            case (int)ExploreNodeType.Submit:
                // 触发提交
                SubmitUIPanel.Instance.Show();
                break;
            case (int)ExploreNodeType.Production:
                // 触发生产
                targetNode.SetCompleted(true);
                if (!BuildingMgr.HasBuildingData(targetNode.id))
                {
                    Debug.Log("创建工具台");
                    BuildingMgr.AddBuildingData(new ProductionBuildingData("20004", targetNode.id, targetConfig.recipeIdGroup.ToList()));
                }

                ProductionPlatformUIPanel.Instance.Show(targetNode.id);
                break;
            case (int)ExploreNodeType.Story:
                // 触发剧情
                DialogueUIPanel.Instance.StartDialogue(targetConfig.storyId);
                break;
        }
        Debug.Log($"点击节点: {targetNode.id}");
    }

}
