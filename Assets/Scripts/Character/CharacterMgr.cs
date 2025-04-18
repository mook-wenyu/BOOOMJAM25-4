public static class CharacterMgr
{
    public static CharacterData GetCharacter(string characterId)
    {
        return GameMgr.currentSaveData.CharacterDatas[characterId];
    }
}
