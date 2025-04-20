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
}
