using System.Collections.Generic;

public static class ExploreNodeMgr
{
    public static Dictionary<string, ExploreMapData> tempExploreMaps = new Dictionary<string, ExploreMapData>();

    /// <summary>
    /// 当前地图ID
    /// </summary>
    public static string currentMapId = "0";

    public static void Init()
    {
        tempExploreMaps.Clear();

        var configs = ConfigManager.Instance.GetConfigList<ExploreNodeConfig>();
        foreach (var config in configs)
        {
            if (!tempExploreMaps.ContainsKey(config.ownMap))
            {
                tempExploreMaps.Add(config.ownMap, new ExploreMapData(config.ownMap));
            }
            var node = new ExploreNodeData(config.id, new Pos(config.mapLocation[0], config.mapLocation[1]), config.neighborNodes);
            if (config.isStartPoint)
            {
                tempExploreMaps[config.ownMap].SetStartNodeId(config.id);
                node.SetInitCompleted();
            }
            tempExploreMaps[config.ownMap].Add(node);
        }
    }

    /// <summary>
    /// 获取探索节点配置
    /// </summary>
    public static ExploreNodeConfig GetExploreNodeConfig(string nodeId)
    {
        return ConfigManager.Instance.GetConfig<ExploreNodeConfig>(nodeId);
    }

    /// <summary>
    /// 获取探索地图数据
    /// </summary>
    public static ExploreMapData GetExploreMapData(string mapId)
    {
        return GameMgr.currentSaveData.exploreMaps.GetValueOrDefault(mapId, null);
    }

    /// <summary>
    /// 获取当前地图探索节点数据
    /// </summary>
    public static ExploreNodeData GetExploreNodeData(string nodeId)
    {
        return GetExploreNodeData(currentMapId, nodeId);
    }

    /// <summary>
    /// 获取探索节点数据
    /// </summary>
    public static ExploreNodeData GetExploreNodeData(string mapId, string nodeId)
    {
        return GetExploreMapData(mapId).nodes.GetValueOrDefault(nodeId, null);
    }

}