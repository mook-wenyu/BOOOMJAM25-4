using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CharacterData
{
    public string id;

    /// <summary>
    /// 全名
    /// </summary>
    public string fullName;

    /// <summary>
    /// 生命值
    /// </summary>
    private int _hp;
    public int Hp
    {
        get
        {
            return _hp;
        }
        set
        {
            _hp = value;
            OnHpChanged?.Invoke(this, value);
        }
    }

    /// <summary>
    /// 体力
    /// </summary>
    private int _stamina;
    public int Stamina
    {
        get
        {
            return _stamina;
        }
        set
        {
            _stamina = value;
            OnStaminaChanged?.Invoke(this, value);
        }
    }

    /// <summary>
    /// 精神
    /// </summary>
    private int _spirit;
    public int Spirit
    {
        get
        {
            return _spirit;
        }
        set
        {
            _spirit = value;
            OnSpiritChanged?.Invoke(this, value);
        }
    }

    /// <summary>
    /// 饱食度
    /// </summary>
    private int _satiety;
    public int Satiety
    {
        get
        {
            return _satiety;
        }
        set
        {
            _satiety = value;
            OnSatietyChanged?.Invoke(this, value);
        }
    }

    /// <summary>
    /// 活动的增益和减益效果
    /// </summary>
    [SerializeField]
    public List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    /// <summary>
    /// Buff添加事件
    /// </summary>
    public event Action<CharacterData, ActiveBuff> OnBuffAdded;

    /// <summary>
    /// Buff移除事件
    /// </summary>
    public event Action<CharacterData, ActiveBuff> OnBuffRemoved;

    /// <summary>
    /// Buff更新事件
    /// </summary>
    public event Action<CharacterData, ActiveBuff> OnBuffUpdated;

    /// <summary>
    /// 生命值变化事件
    /// </summary>
    public event Action<CharacterData, int> OnHpChanged;

    /// <summary>
    /// 体力变化事件
    /// </summary>
    public event Action<CharacterData, int> OnStaminaChanged;

    /// <summary>
    /// 精神变化事件
    /// </summary>
    public event Action<CharacterData, int> OnSpiritChanged;

    /// <summary>
    /// 饱食度变化事件
    /// </summary>
    public event Action<CharacterData, int> OnSatietyChanged;

    /// <summary>
    /// 添加激活的Buff
    /// </summary>
    /// <param name="buffDataId">要添加的Buff数据ID</param>
    /// <returns>添加的ActiveBuff实例</returns>
    public ActiveBuff AddBuff(string buffDataId)
    {
        // 获取Buff数据
        BuffData buffData = ConfigManager.Instance.GetConfig<BuffData>(buffDataId);
        if (buffData == null)
        {
            Debug.LogWarning($"尝试添加不存在的Buff: {buffDataId}");
            return null;
        }
        
        // 检查是否已存在相同ID的Buff
        ActiveBuff existingBuff = activeBuffs.FirstOrDefault(b => b.buffDataId == buffDataId);
        
        if (existingBuff != null)
        {
            // 如果已存在，刷新持续时间并增加层数
            existingBuff.Refresh();
            existingBuff.AddStacks();
            OnBuffUpdated?.Invoke(this, existingBuff);
            return existingBuff;
        }
        else
        {
            // 创建新的激活Buff
            ActiveBuff newBuff = new ActiveBuff(buffDataId, this.id);
            
            // 添加到列表
            activeBuffs.Add(newBuff);
            
            // 应用效果
            newBuff.Apply();
            
            // 触发事件
            OnBuffAdded?.Invoke(this, newBuff);
            
            return newBuff;
        }
    }
    
    /// <summary>
    /// 直接添加ActiveBuff实例
    /// </summary>
    /// <param name="activeBuff">要添加的ActiveBuff</param>
    public void AddActiveBuff(ActiveBuff activeBuff)
    {
        // 确保设置了正确的角色ID
        activeBuff.characterId = this.id;
        
        // 检查是否已存在相同ID的Buff
        ActiveBuff existingBuff = activeBuffs.FirstOrDefault(b => b.buffDataId == activeBuff.buffDataId);
        
        if (existingBuff != null)
        {
            // 如果已存在，刷新持续时间并增加层数
            existingBuff.Refresh();
            existingBuff.AddStacks();
            OnBuffUpdated?.Invoke(this, existingBuff);
        }
        else
        {
            // 添加到列表
            activeBuffs.Add(activeBuff);
            
            // 应用效果
            activeBuff.Apply();
            
            // 触发事件
            OnBuffAdded?.Invoke(this, activeBuff);
        }
    }

    /// <summary>
    /// 移除指定ID的Buff
    /// </summary>
    /// <param name="buffDataId">Buff数据ID</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveBuff(string buffDataId)
    {
        ActiveBuff buff = activeBuffs.FirstOrDefault(b => b.buffDataId == buffDataId);
        if (buff != null)
        {
            RemoveBuff(buff);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 移除指定的ActiveBuff
    /// </summary>
    /// <param name="activeBuff">要移除的ActiveBuff</param>
    public void RemoveBuff(ActiveBuff activeBuff)
    {
        activeBuff.Remove();
        activeBuffs.Remove(activeBuff);
        OnBuffRemoved?.Invoke(this, activeBuff);
    }

    /// <summary>
    /// 更新所有Buff状态
    /// </summary>
    /// <param name="deltaTime">时间间隔</param>
    public void UpdateBuffs(float deltaTime)
    {
        // 创建临时列表以存储需要移除的Buff
        List<ActiveBuff> expiredBuffs = new List<ActiveBuff>();
        
        // 更新所有Buff
        foreach (var buff in activeBuffs)
        {
            if (buff.Update(deltaTime))
            {
                expiredBuffs.Add(buff);
            }
        }
        
        // 移除过期的Buff
        foreach (var buff in expiredBuffs)
        {
            RemoveBuff(buff);
        }
    }

    /// <summary>
    /// 获取指定ID的激活Buff
    /// </summary>
    /// <param name="buffDataId">Buff数据ID</param>
    /// <returns>找到的ActiveBuff，未找到返回null</returns>
    public ActiveBuff GetActiveBuff(string buffDataId)
    {
        return activeBuffs.FirstOrDefault(b => b.buffDataId == buffDataId);
    }
    
    /// <summary>
    /// 获取指定实例ID的激活Buff
    /// </summary>
    /// <param name="instanceId">Buff实例ID</param>
    /// <returns>找到的ActiveBuff，未找到返回null</returns>
    public ActiveBuff GetActiveBuffByInstanceId(string instanceId)
    {
        return activeBuffs.FirstOrDefault(b => b.instanceId == instanceId);
    }
    
    /// <summary>
    /// 减少Buff的堆叠层数
    /// </summary>
    /// <param name="buffDataId">Buff数据ID</param>
    /// <param name="amount">减少的层数</param>
    /// <returns>是否成功减少</returns>
    public bool ReduceBuffStacks(string buffDataId, int amount = 1)
    {
        ActiveBuff buff = GetActiveBuff(buffDataId);
        if (buff != null)
        {
            if (buff.ReduceStacks(amount))
            {
                RemoveBuff(buff);
            }
            else
            {
                OnBuffUpdated?.Invoke(this, buff);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 清除所有Buff
    /// </summary>
    /// <param name="includePositive">是否包括正面Buff</param>
    /// <param name="includeNegative">是否包括负面Buff</param>
    public void ClearBuffs(bool includePositive = true, bool includeNegative = true)
    {
        List<ActiveBuff> buffsToRemove = new List<ActiveBuff>();
        
        foreach (var buff in activeBuffs)
        {
            bool isDebuff = buff.BuffData != null && buff.BuffData.isDebuff;
            if ((isDebuff && includeNegative) || (!isDebuff && includePositive))
            {
                buffsToRemove.Add(buff);
            }
        }
        
        foreach (var buff in buffsToRemove)
        {
            RemoveBuff(buff);
        }
    }
    
    /// <summary>
    /// 判断是否拥有指定ID的Buff
    /// </summary>
    /// <param name="buffDataId">Buff数据ID</param>
    /// <returns>是否拥有</returns>
    public bool HasBuff(string buffDataId)
    {
        return activeBuffs.Any(b => b.buffDataId == buffDataId);
    }
}
