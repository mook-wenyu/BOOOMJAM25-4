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
    /// 正在建造中的建筑
    /// </summary>
    public List<BuildingData> buildingProgress = new();

    /// <summary>
    /// 正在生产中的配方
    /// </summary>
    public List<RecipeData> recipeProgress = new();

    /// <summary>
    /// 已建造的建筑数据
    /// </summary>
    public Dictionary<string, BuildingData> buildings = new();

    /// <summary>
    /// 对话存储
    /// </summary>
    public DialogueStorage dialogueStorage = new();
}
