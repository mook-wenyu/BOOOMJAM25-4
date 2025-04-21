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
    public int health;
    public int healthMax;

    /// <summary>
    /// 饱食度
    /// </summary>
    public int hunger;
    public int hungerMax;

    /// <summary>
    /// 活力
    /// </summary>
    public int energy;
    public int energyMax;

    /// <summary>
    /// 精神
    /// </summary>
    public int spirit;
    public int spiritMax;

    /// <summary>
    /// 背包数据
    /// </summary>
    public string inventoryId;

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
    /// 生命值变化事件
    /// </summary>
    public event Action<CharacterData, int> OnHpChanged;
    public event Action<CharacterData, int> OnHpMaxChanged;

    /// <summary>
    /// 体力变化事件
    /// </summary>
    public event Action<CharacterData, int> OnStaminaChanged;
    public event Action<CharacterData, int> OnStaminaMaxChanged;

    /// <summary>
    /// 精神变化事件
    /// </summary>
    public event Action<CharacterData, int> OnSpiritChanged;
    public event Action<CharacterData, int> OnSpiritMaxChanged;

    /// <summary>
    /// 饱食度变化事件
    /// </summary>
    public event Action<CharacterData, int> OnSatietyChanged;
    public event Action<CharacterData, int> OnSatietyMaxChanged;

    public CharacterData()
    {
        var inventory = new InventoryData();
        inventoryId = inventory.inventoryId;
        GameMgr.currentSaveData.inventories[inventoryId] = inventory;
    }

    /// <summary>
    /// 设置生命值
    /// </summary>
    public void SetHealth(int newHealth)
    {
        health = newHealth;
        OnHpChanged?.Invoke(this, health);
    }

    /// <summary>
    /// 设置最大生命值
    /// </summary>
    public void SetHealthMax(int newHealthMax)
    {
        healthMax = newHealthMax;
        OnHpMaxChanged?.Invoke(this, healthMax);
    }

    /// <summary>
    /// 设置饱食度
    /// </summary>
    public void SetHunger(int newHunger)
    {
        hunger = newHunger;
        OnSatietyChanged?.Invoke(this, hunger);
    }
    public void SetHungerMax(int newHungerMax)
    {
        hungerMax = newHungerMax;
        OnSatietyMaxChanged?.Invoke(this, hungerMax);
    }

    /// <summary>
    /// 设置活力
    /// </summary>
    public void SetEnergy(int newEnergy)
    {
        energy = newEnergy;
        OnStaminaChanged?.Invoke(this, energy);
    }
    public void SetEnergyMax(int newEnergyMax)
    {
        energyMax = newEnergyMax;
        OnStaminaMaxChanged?.Invoke(this, energyMax);
    }

    /// <summary>
    /// 设置精神
    /// </summary>
    public void SetSpirit(int newSpirit)
    {
        spirit = newSpirit;
        OnSpiritChanged?.Invoke(this, spirit);
    }
    public void SetSpiritMax(int newSpiritMax)
    {
        spiritMax = newSpiritMax;
        OnSpiritMaxChanged?.Invoke(this, spiritMax);
    }


    /// <summary>
    /// 添加激活的Buff
    /// </summary>
    /// <param name="buffDataId">要添加的Buff数据ID</param>
    /// <returns>添加的ActiveBuff实例</returns>
    public ActiveBuff AddBuff(string buffDataId)
    {
        // 获取Buff数据
        BuffConfig buffData = BuffMgr.GetBuffData(buffDataId);
        if (buffData == null)
        {
            Debug.LogWarning($"尝试添加不存在的Buff: {buffDataId}");
            return null;
        }

        // 检查是否已存在相同ID的Buff
        ActiveBuff existingBuff = activeBuffs.FirstOrDefault(b => b.buffDataId == buffDataId);

        if (existingBuff != null)
        {
            // 如果已存在，刷新
            existingBuff.Refresh();
            return existingBuff;
        }
        else
        {
            // 创建新的激活Buff
            ActiveBuff newBuff = new ActiveBuff(buffDataId, this.id);

            // 添加到列表
            activeBuffs.Add(newBuff);

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
            // 如果已存在，刷新
            existingBuff.Refresh();
        }
        else
        {
            // 添加到列表
            activeBuffs.Add(activeBuff);

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
        activeBuffs.Remove(activeBuff);
        OnBuffRemoved?.Invoke(this, activeBuff);
    }

    /// <summary>
    /// 更新所有Buff状态
    /// </summary>
    /// <param name="deltaTime">时间间隔</param>
    public void UpdateBuffs(float deltaTime = 1f)
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
    /// 清除所有Buff
    /// </summary>
    public void ClearBuffs()
    {
        foreach (var buff in activeBuffs)
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
