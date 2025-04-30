using MookDialogueScript;

public static class CharacterMgr
{
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
        Player().IncreaseHealth((int)amount);
    }

    [ScriptFunc("increase_player_hunger")]
    public static void IncreasePlayerHunger(double amount)
    {
        Player().IncreaseHunger((int)amount);
    }

    [ScriptFunc("increase_player_energy")]
    public static void IncreasePlayerEnergy(double amount)
    {
        Player().IncreaseEnergy((int)amount);
    }

    [ScriptFunc("increase_player_spirit")]
    public static void IncreasePlayerSpirit(double amount)
    {
        Player().IncreaseSpirit((int)amount);
    }

    /// <summary>
    /// 减少玩家角色的生命值
    /// </summary>
    [ScriptFunc("reduce_player_hp")]
    public static void ReducePlayerHp(double amount)
    {
        Player().DecreaseHealth((int)amount);
    }

    /// <summary>
    /// 减少玩家角色的饱食度
    /// </summary>
    [ScriptFunc("reduce_player_hunger")]
    public static void ReducePlayerHunger(double amount)
    {
        Player().DecreaseHunger((int)amount);
    }

    /// <summary>
    /// 减少玩家角色的体力
    /// </summary>
    [ScriptFunc("reduce_player_energy")]
    public static void ReducePlayerEnergy(double amount)
    {
        Player().DecreaseEnergy((int)amount);
    }

    /// <summary>
    /// 减少玩家角色的精神
    /// </summary>
    [ScriptFunc("reduce_player_spirit")]
    public static void ReducePlayerSpirit(double amount)
    {
        Player().DecreaseSpirit((int)amount);
    }
}
