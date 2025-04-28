using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPathMgr : MonoSingleton<RoadPathMgr>
{
    // 路径容器
    public Transform pathContainer;
    // 路径字典
    public Dictionary<string, List<Vector2>> pathDict = new Dictionary<string, List<Vector2>>();
    // 路径渲染器
    private Dictionary<string, LineRenderer> pathRenderers = new Dictionary<string, LineRenderer>();

    /// <summary>
    /// 生成所有道路路径点
    /// </summary>
    public void GenerateAllRoadPaths(List<ExploreNodeData> nodes)
    {
        ClearPathRenderers();
        pathDict.Clear();
        HashSet<string> processedPairs = new HashSet<string>();

        foreach (var node in nodes)
        {
            if (node.neighborNodes == null || node.neighborNodes.Length == 0) continue;
            foreach (var connectedId in node.neighborNodes)
            {
                string pathPairId = GetPathPairId(node.id, connectedId);
                if (processedPairs.Contains(pathPairId))
                    continue;

                processedPairs.Add(pathPairId);

                // 创建新的路径并添加到roadPaths和pathDictionary中
                var path = CreateRoadPath(node.id, connectedId);
                pathDict[pathPairId] = path;

                // 创建路径渲染器
                CreatePathRenderer(pathPairId, path);
            }
        }
    }

    /// <summary>
    /// 创建道路路径
    /// </summary>
    private List<Vector2> CreateRoadPath(string startNodeId, string endNodeId)
    {
        var path = new List<Vector2>();

        ExploreNodeConfig startNode = ExploreNodeMgr.GetExploreNodeConfig(startNodeId);
        ExploreNodeConfig endNode = ExploreNodeMgr.GetExploreNodeConfig(endNodeId);

        if (startNode == null || endNode == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"无法找到节点: {startNodeId} 或 {endNodeId}");
#endif
            return path;
        }

        path.Add(new Vector2(startNode.mapLocation[0], startNode.mapLocation[1]));
        path.Add(new Vector2(endNode.mapLocation[0], endNode.mapLocation[1]));

        return path;
    }



    /// <summary>
    /// 创建路径渲染器
    /// </summary>
    private void CreatePathRenderer(string pathId, List<Vector2> path)
    {
        var lineRenderer = new GameObject($"Path_{pathId}").AddComponent<LineRenderer>();
        lineRenderer.transform.SetParent(pathContainer);

        // 设置线条属性
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // 线条渲染
        lineRenderer.numCornerVertices = 32;
        lineRenderer.numCapVertices = 32;
        lineRenderer.alignment = LineAlignment.TransformZ;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.allowOcclusionWhenDynamic = false;

        // 设置材质
        var material = new Material(Shader.Find("Sprites/Default"))
        {
            color = Color.white
        };
        material.renderQueue = 3000;

        lineRenderer.material = material;
        lineRenderer.useWorldSpace = true;
        lineRenderer.generateLightingData = false;
        lineRenderer.textureMode = LineTextureMode.Stretch;

        // 获取路径点
        List<Vector2> pathPoints = path;
        lineRenderer.positionCount = pathPoints.Count;
        float z = pathContainer.position.z;
        for (int i = 0; i < pathPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(pathPoints[i].x, pathPoints[i].y, z));
        }

        lineRenderer.gameObject.SetActive(false);

        pathRenderers[pathId] = lineRenderer;
    }

    /// <summary>
    /// 获取路径渲染器字典
    /// </summary>
    public Dictionary<string, LineRenderer> GetPathRenderers()
    {
        return pathRenderers;
    }

    /// <summary>
    /// 生成路径对的唯一标识符
    /// </summary>
    public string GetPathPairId(string id1, string id2)
    {
        // 确保相同的两个节点无论顺序如何都会生成相同的ID
        var orderedIds = new[] { id1, id2 };
        Array.Sort(orderedIds);
        return $"{orderedIds[0]}_{orderedIds[1]}";
    }


    /// <summary>
    /// 清除路径渲染器
    /// </summary>
    private void ClearPathRenderers()
    {
        pathContainer.DestroyAllChildren();
        foreach (var renderer in pathRenderers.Values)
        {
            if (renderer != null)
            {
                Destroy(renderer.gameObject);
            }
        }
        pathRenderers.Clear();
    }



}
