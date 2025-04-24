using System.Collections;
using System.Collections.Generic;
using MookDialogueScript;

public class SaveData
{
    /// <summary>
    /// 当前时间
    /// </summary>
    public GameTime gameTime = new(1, 7, 0);

    /// <summary>
    /// 玩家ID
    /// </summary>
    public string playerId = "player";

    /// <summary>
    /// 角色数据
    /// </summary>
    public Dictionary<string, CharacterData> characters = new();

    /// <summary>
    /// 库存数据
    /// </summary>
    public Dictionary<string, BaseInventoryData> inventories = new();

    /// <summary>
    /// 建造平台
    /// </summary>
    public Dictionary<string, BuildPlatformData> buildPlatforms = new();

    /// <summary>
    /// 生产平台
    /// </summary>
    public Dictionary<string, ProductionPlatformData> productionPlatforms = new();

    /// <summary>
    /// 已建造的建筑数据
    /// </summary>
    public Dictionary<string, BuildingData> buildings = new();

    /// <summary>
    /// 障碍物清除进度
    /// </summary>
    public List<BuildingData> obstacleProgress = new();

    /// <summary>
    /// 房间楼层数据 用于网格系统
    /// </summary>
    public Dictionary<string, RoomFloor> floors = new();

    /// <summary>
    /// 对话存储
    /// </summary>
    public DialogueStorage dialogueStorage = new();

    /// <summary>
    /// 探索地图数据
    /// </summary>
    public Dictionary<string, ExploreMapData> exploreMaps = new();
}
