using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 激活的Buff，表示运行时的状态
/// </summary>
[Serializable]
public class ActiveBuff
{
    /// <summary>
    /// 实例ID（用于区分同一类型的多个Buff实例）
    /// </summary>
    public string instanceId;

    /// <summary>
    /// Buff数据ID
    /// </summary>
    public string buffDataId;

    /// <summary>
    /// 所属角色ID
    /// </summary>
    public string characterId;

    /// <summary>
    /// 剩余时间
    /// </summary>
    public float remainingTime;

    /// <summary>
    /// Buff数据缓存
    /// </summary>
    [NonSerialized]
    private BuffConfig _buffData;

    // Buff时间变化事件
    public event Action<ActiveBuff> OnBuffTimeChanged;

    // Buff过期事件
    public event Action<ActiveBuff> OnBuffExpired;

    /// <summary>
    /// 默认构造函数（用于序列化）
    /// </summary>
    public ActiveBuff() { }

    /// <summary>
    /// 创建激活的Buff
    /// </summary>
    /// <param name="buffDataId">Buff数据ID</param>
    /// <param name="characterId">角色ID</param>
    public ActiveBuff(string buffDataId, string characterId)
    {
        this.buffDataId = buffDataId;
        this.characterId = characterId;
        this.instanceId = Guid.NewGuid().ToString("N");

        // 获取Buff数据
        _buffData = BuffMgr.GetBuffData(buffDataId);
        if (_buffData != null)
        {
            // 设置初始持续时间
            remainingTime = _buffData.defaultTime;
        }
    }

    /// <summary>
    /// 设置Buff的剩余时间
    /// </summary>
    public void SetTime(float time)
    {
        _buffData ??= BuffMgr.GetBuffData(buffDataId);

        if (_buffData == null || _buffData.defaultTime <= 0)
            return;

        remainingTime = Mathf.Min(time, _buffData.defaultTime); // 限制时间不能超过默认时间
        OnBuffTimeChanged?.Invoke(this);

        if (IsExpired())
        {
            OnBuffExpired?.Invoke(this);
        }
    }

    /// <summary>
    /// 减少Buff的剩余时间
    /// </summary>
    public void ReduceTime(float deltaTime)
    {
        SetTime(remainingTime - deltaTime);
    }

    /// <summary>
    /// 刷新Buff
    /// </summary>
    public void Refresh()
    {
        _buffData ??= BuffMgr.GetBuffData(buffDataId);
        SetTime(_buffData.defaultTime);
    }

    public bool Update(float deltaTime)
    {
        ReduceTime(deltaTime);
        return IsExpired();
    }

    /// <summary>
    /// 判断Buff是否已过期（时间耗尽）
    /// </summary>
    public bool IsExpired()
    {
        _buffData ??= BuffMgr.GetBuffData(buffDataId);

        return _buffData != null && _buffData.defaultTime > 0 && remainingTime <= 0;
    }


}