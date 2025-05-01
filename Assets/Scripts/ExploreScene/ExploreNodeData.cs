
using System;

/// <summary>
/// 探索节点类型
/// </summary>
[Serializable]
public enum ExploreNodeType
{
    /// <summary>
    /// 空节点
    /// </summary>
    Empty,
    /// <summary>
    /// 功能型
    /// </summary>
    Functional,
    /// <summary>
    /// 奖励型
    /// </summary>
    Reward,
    /// <summary>
    /// 提交型
    /// </summary>
    Submit,
    /// <summary>
    /// 生产型
    /// </summary>
    Production,
    /// <summary>
    /// 剧情型
    /// </summary>
    Story
}

[Serializable]
public class ExploreNodeData
{
    public string id;
    public string changedId = string.Empty;
    public Pos pos;
    public string[] neighborNodes;

    public bool isCompleted;

    public event Action<ExploreNodeData> OnNodeReplaced;
    public event Action<ExploreNodeData> OnNodeCompleted;

    public ExploreNodeData()
    {
    }

    public ExploreNodeData(string id, Pos pos, string[] neighborNodes)
    {
        this.id = id;
        this.pos = pos;
        this.neighborNodes = neighborNodes;
    }

    public void SetChangedId(string changedId)
    {
        this.changedId = changedId;
        this.isCompleted = false;
        OnNodeReplaced?.Invoke(this); // 触发节点替换事件，用于更新地图显示
    }

    public void SetInitCompleted()
    {
        this.isCompleted = true;
    }

    public void SetCompleted()
    {
        if (isCompleted) return;
        this.isCompleted = true;
        OnNodeCompleted?.Invoke(this);
    }

    public void SetUnCompleted()
    {
        this.isCompleted = false;
    }

    /// <summary>
    /// 获取原始配置
    /// </summary>
    /// <returns></returns>
    public ExploreNodeConfig GetOriginalConfig()
    {
        return ExploreNodeMgr.GetExploreNodeConfig(id);
    }

    /// <summary>
    /// 获取节点配置
    /// </summary>
    public ExploreNodeConfig GetConfig()
    {
        if (!string.IsNullOrEmpty(changedId))
        {
            return ExploreNodeMgr.GetExploreNodeConfig(changedId);
        }
        return ExploreNodeMgr.GetExploreNodeConfig(id);
    }


}
