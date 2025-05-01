using System.Collections.Generic;
using MookDialogueScript;

public static class CharacterMgr
{
    /// <summary>
    /// NPC 名字和文件名映射
    /// </summary>
    public static Dictionary<string, string> npcs = new Dictionary<string, string>();
    public static void Init()
    {
        npcs["瑞迪亚"] = "npc2";
        npcs["德鲁斯特"] = "npc3_right";
        npcs["加缪"] = "npc4";
        npcs["莉莉安"] = "npc5_left";
    }

    /// <summary>
    /// 获取角色数据
    /// </summary>
    public static CharacterData GetCharacter(string characterId)
    {
        return GameMgr.currentSaveData.characters[characterId];
    }

    /// <summary>
    /// 获取玩家角色数据
    /// </summary>
    public static CharacterData Player()
    {
        return GetCharacter(GameMgr.currentSaveData.playerId);
    }



    /// <summary>
    /// 增加玩家角色的生命值
    /// </summary>
    [ScriptFunc("increase_player_hp")]
    public static void IncreasePlayerHp(double amount)
    {
        Player().IncreaseHealth((float)amount);
    }

    [ScriptFunc("increase_player_hunger")]
    public static void IncreasePlayerHunger(double amount)
    {
        Player().IncreaseHunger((float)amount);
    }

    [ScriptFunc("increase_player_energy")]
    public static void IncreasePlayerEnergy(double amount)
    {
        Player().IncreaseEnergy((float)amount);
    }

    [ScriptFunc("increase_player_spirit")]
    public static void IncreasePlayerSpirit(double amount)
    {
        Player().IncreaseSpirit((float)amount);
    }

    /// <summary>
    /// 减少玩家角色的生命值
    /// </summary>
    [ScriptFunc("reduce_player_hp")]
    public static void ReducePlayerHp(double amount)
    {
        Player().DecreaseHealth((float)amount);
    }

    /// <summary>
    /// 减少玩家角色的饱食度
    /// </summary>
    [ScriptFunc("reduce_player_hunger")]
    public static void ReducePlayerHunger(double amount)
    {
        Player().DecreaseHunger((float)amount);
    }

    /// <summary>
    /// 减少玩家角色的体力
    /// </summary>
    [ScriptFunc("reduce_player_energy")]
    public static void ReducePlayerEnergy(double amount)
    {
        Player().DecreaseEnergy((float)amount);
    }

    /// <summary>
    /// 减少玩家角色的精神
    /// </summary>
    [ScriptFunc("reduce_player_spirit")]
    public static void ReducePlayerSpirit(double amount)
    {
        Player().DecreaseSpirit((float)amount);
    }

    /// <summary>
    /// 添加Buff到玩家身上
    /// </summary>
    [ScriptFunc("add_buff")]
    public static void AddBuff(string buffDataId)
    {
        Player().AddBuff(buffDataId);
    }

    /// <summary>
    /// 移除玩家身上的Buff
    /// </summary>
    [ScriptFunc("remove_buff")]
    public static void RemoveBuff(string buffDataId)
    {
        Player().RemoveBuff(buffDataId);
    }

}
