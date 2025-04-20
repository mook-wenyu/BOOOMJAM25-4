
public static class BuffMgr
{
    /// <summary>
    /// 获取Buff数据
    /// </summary>
    public static BuffConfig GetBuffData(string buffDataId)
    {
        return ConfigManager.Instance.GetConfig<BuffConfig>(buffDataId);
    }

    public static ActiveBuff AddBuff(this CharacterData characterData, string buffDataId)
    {
        return new ActiveBuff(buffDataId, characterData.id);
    }

    public static void RemoveBuff(this CharacterData characterData, string buffDataId)
    {
        var buff = characterData.GetActiveBuff(buffDataId);
        if (buff != null)
        {
            characterData.RemoveBuff(buff);
        }
    }

}
