using System;
using System.Collections.Generic;

[Serializable]
public class ExploreMapData
{
    public string id;

    public string startNodeId;

    public Dictionary<string, ExploreNodeData> nodes = new();

    public ExploreMapData()
    {
    }

    public ExploreMapData(string id)
    {
        this.id = id;
    }

    public void Add(ExploreNodeData node)
    {
        nodes[node.id] = node;
    }

    public void SetStartNodeId(string startNodeId)
    {
        this.startNodeId = startNodeId;
    }

}
