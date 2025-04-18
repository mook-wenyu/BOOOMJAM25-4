using System.Collections;
using System.Collections.Generic;
using MookDialogueScript;

public class SaveData
{
    /// <summary>
    /// 当前时间
    /// </summary>
    public GameTime GameTime { get; set; } = new(0, 7, 0);

    /// <summary>
    /// 背包数据
    /// </summary>
    public InventoryData Inventory { get; set; } = new();

    /// <summary>
    /// 角色数据
    /// </summary>
    public Dictionary<string, CharacterData> CharacterDatas { get; set; } = new();

    /// <summary>
    /// 对话存储
    /// </summary>
    public DialogueStorage DialogueStorage { get; set; } = new();
}
