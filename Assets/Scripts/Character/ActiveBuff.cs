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
    /// 当前堆叠层数
    /// </summary>
    public int stacks = 1;
    
    /// <summary>
    /// 计时器（用于周期性效果）
    /// </summary>
    [NonSerialized]
    private float effectTimer;
    
    /// <summary>
    /// Buff数据缓存
    /// </summary>
    [NonSerialized]
    private BuffData _buffData;
    
    /// <summary>
    /// 获取Buff数据
    /// </summary>
    public BuffData BuffData 
    { 
        get 
        {
            if (_buffData == null)
            {
                _buffData = ConfigManager.Instance.GetConfig<BuffData>(buffDataId);
            }
            return _buffData;
        }
    }
    
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
        this.instanceId = Guid.NewGuid().ToString();
        
        // 获取Buff数据
        BuffData buffData = BuffData;
        if (buffData != null)
        {
            // 设置初始持续时间
            remainingTime = buffData.duration;
        }
    }
    
    /// <summary>
    /// 应用Buff效果
    /// </summary>
    public void Apply()
    {
        CharacterData character = CharacterMgr.GetCharacter(characterId);
        if (character == null || BuffData == null)
            return;
            
        // 根据Buff类型应用效果
        switch (BuffData.buffType)
        {
            case BuffType.StatModifier:
                ApplyStatModifier(character);
                break;
            case BuffType.StatusEffect:
                ApplyStatusEffect(character);
                break;
            case BuffType.Custom:
                // 自定义效果需要特殊处理
                break;
        }
    }
    
    /// <summary>
    /// 更新Buff状态
    /// </summary>
    /// <param name="deltaTime">时间间隔</param>
    /// <returns>Buff是否已结束</returns>
    public bool Update(float deltaTime)
    {
        CharacterData character = CharacterMgr.GetCharacter(characterId);
        if (character == null || BuffData == null)
            return true;
            
        // 更新计时器
        effectTimer += deltaTime;
        
        // 处理周期性效果
        HandlePeriodicEffect(character);
        
        // 更新持续时间
        if (BuffData.duration > 0)
        {
            remainingTime -= deltaTime;
            if (remainingTime <= 0)
            {
                return true; // Buff结束
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 处理周期性效果
    /// </summary>
    private void HandlePeriodicEffect(CharacterData character)
    {
        // 周期性效果的间隔（假设为1秒）
        float interval = 1.0f;
        
        if (effectTimer >= interval)
        {
            effectTimer -= interval;
            
            // 根据Buff类型执行周期性效果
            switch (BuffData.buffType)
            {
                case BuffType.HealthRegen:
                    // 获取治疗量参数
                    int healAmount = GetParameter<int>("healAmount", 0);
                    character.Hp += healAmount * stacks;
                    break;
                    
                case BuffType.DamageOverTime:
                    // 获取伤害量参数
                    int damageAmount = GetParameter<int>("damageAmount", 0);
                    character.Hp -= damageAmount * stacks;
                    break;
            }
        }
    }
    
    /// <summary>
    /// 应用属性修改效果
    /// </summary>
    private void ApplyStatModifier(CharacterData character)
    {
        // 这里需要根据游戏中的属性系统来实现
        // 例如：
        // int strModifier = GetParameter<int>("strModifier", 0);
        // character.Strength += strModifier * stacks;
    }
    
    /// <summary>
    /// 应用状态效果
    /// </summary>
    private void ApplyStatusEffect(CharacterData character)
    {
        // 这里需要根据游戏中的状态系统来实现
        // 例如：
        // string statusType = GetParameter<string>("statusType", "");
        // character.AddStatus(statusType);
    }
    
    /// <summary>
    /// 移除Buff效果
    /// </summary>
    public void Remove()
    {
        CharacterData character = CharacterMgr.GetCharacter(characterId);
        if (character == null || BuffData == null)
            return;
            
        // 根据Buff类型移除效果
        switch (BuffData.buffType)
        {
            case BuffType.StatModifier:
                RemoveStatModifier(character);
                break;
            case BuffType.StatusEffect:
                RemoveStatusEffect(character);
                break;
        }
    }
    
    /// <summary>
    /// 移除属性修改效果
    /// </summary>
    private void RemoveStatModifier(CharacterData character)
    {
        // 与应用效果相反的操作
    }
    
    /// <summary>
    /// 移除状态效果
    /// </summary>
    private void RemoveStatusEffect(CharacterData character)
    {
        // 与应用效果相反的操作
    }
    
    /// <summary>
    /// 增加堆叠层数
    /// </summary>
    /// <param name="amount">增加的层数</param>
    public void AddStacks(int amount = 1)
    {
        int oldStacks = stacks;
        stacks = Mathf.Min(stacks + amount, BuffData?.maxStacks ?? 1);
        
        // 如果层数实际增加了，重新应用效果
        if (stacks > oldStacks)
        {
            Apply();
        }
    }
    
    /// <summary>
    /// 减少堆叠层数
    /// </summary>
    /// <param name="amount">减少的层数</param>
    /// <returns>是否应该移除Buff</returns>
    public bool ReduceStacks(int amount = 1)
    {
        stacks -= amount;
        
        if (stacks <= 0)
        {
            return true; // 应该移除Buff
        }
        
        Apply(); // 重新应用效果
        return false;
    }
    
    /// <summary>
    /// 刷新持续时间
    /// </summary>
    public void Refresh()
    {
        if (BuffData != null && BuffData.duration > 0)
        {
            remainingTime = BuffData.duration;
        }
    }
    
    /// <summary>
    /// 获取参数值
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="paramName">参数名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>参数值</returns>
    private T GetParameter<T>(string paramName, T defaultValue)
    {
        if (BuffData == null)
            return defaultValue;
            
        T value = BuffData.GetParameter<T>(paramName);
        return EqualityComparer<T>.Default.Equals(value, default) ? defaultValue : value;
    }
} 