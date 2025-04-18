using System;
using UnityEngine;

// 物品类型枚举
public enum ItemType
{
    Material,   // 材料型
    Consumable, // 消耗型
    Equipment,  // 装备型
    Quest,      // 任务物品
    Currency,   // 货币
    Other       // 其他
}

// 装备类型枚举
public enum EquipmentType
{
    None,       // 无
    Weapon,     // 武器
    Armor,      // 护甲
    Helmet,     // 头盔
    Accessory,  // 饰品
    Tool        // 工具
}

// 消耗品效果类型
public enum ConsumableEffectType
{
    None,           // 无效果
    RestoreHealth,  // 恢复生命值
    RestoreMana,    // 恢复魔法值
    RestoreStamina, // 恢复耐力
    TemporaryBuff,  // 临时增益
    RemoveDebuff,   // 移除负面效果
    AddExp,         // 增加经验
    AddSkillPoint,  // 增加技能点
}

public class ItemData : BaseConfig
{
    public string itemName;        // 物品名称
    public string description;     // 物品描述
    public string iconPath;        // 图标路径
    public int maxStack = 99;      // 最大堆叠数量
    public ItemType itemType = ItemType.Material; // 物品类型
    
    // 装备相关属性
    public EquipmentType equipmentType = EquipmentType.None; // 装备类型
    public int durability = 0;     // 耐久度（0表示不会损坏）
    
    // 消耗品相关属性
    public int useTime = 0;        // 使用时间（毫秒，0表示立即使用）
    public bool isReusable = false; // 是否可重复使用
    
    // 材料相关属性
    public string[] craftingRecipes; // 可用于合成的配方ID
    
    // 经济相关
    public int baseValue = 0;      // 基础价值
    
    // 稀有度
    public int rarity = 0;         // 稀有度（0-普通，1-优秀，2-精良，3-史诗，4-传说）

    // 计算实际出售价格
    public virtual int GetSellPrice()
    {
        // 基础逻辑 - 可以在子类中重写
        return (int)(baseValue * 0.7f);
    }
    
    // 使用物品的效果处理
    public virtual void UseEffect(CharacterData character)
    {
        // 基类中无效果，由子类实现
        Debug.Log($"使用物品: {itemName}");
    }
}

/*
// 消耗品数据类
public class ConsumableItemData : ItemData
{
    // 消耗品效果类型
    public ConsumableEffectType effectType = ConsumableEffectType.None;
    
    // 效果数值（恢复多少血量/魔法值等）
    public int effectValue = 0;
    
    // 效果持续时间（对于buff类型，单位：秒）
    public float effectDuration = 0;
    
    // 效果区域（对于范围效果，单位：米）
    public float effectRadius = 0;
    
    // 冷却时间（单位：秒）
    public float cooldown = 0;
    
    public ConsumableItemData()
    {
        itemType = ItemType.Consumable;
    }
    
    public override void UseEffect(CharacterData character)
    {
        base.UseEffect(character);
        
        if (character == null)
        {
            Debug.LogWarning("使用消耗品时角色数据为空");
            return;
        }
        
        switch (effectType)
        {
            case ConsumableEffectType.RestoreHealth:
                // 恢复生命值
                if (character.health != null)
                {
                    character.health.current = Mathf.Min(
                        character.health.current + effectValue,
                        character.health.max
                    );
                    Debug.Log($"恢复生命值: {effectValue}，当前生命值: {character.health.current}/{character.health.max}");
                }
                break;
                
            case ConsumableEffectType.RestoreMana:
                // 恢复魔法值
                if (character.mana != null)
                {
                    character.mana.current = Mathf.Min(
                        character.mana.current + effectValue,
                        character.mana.max
                    );
                    Debug.Log($"恢复魔法值: {effectValue}，当前魔法值: {character.mana.current}/{character.mana.max}");
                }
                break;
                
            case ConsumableEffectType.RestoreStamina:
                // 恢复耐力
                if (character.stamina != null)
                {
                    character.stamina.current = Mathf.Min(
                        character.stamina.current + effectValue,
                        character.stamina.max
                    );
                    Debug.Log($"恢复耐力: {effectValue}，当前耐力: {character.stamina.current}/{character.stamina.max}");
                }
                break;
                
            case ConsumableEffectType.TemporaryBuff:
                // 添加临时buff
                character.AddBuff(id, effectValue, effectDuration);
                Debug.Log($"添加增益效果，持续时间: {effectDuration}秒，增益值: {effectValue}");
                break;
                
            case ConsumableEffectType.RemoveDebuff:
                // 移除负面效果
                character.RemoveDebuffs();
                Debug.Log("移除所有负面效果");
                break;
                
            case ConsumableEffectType.AddExp:
                // 增加经验
                character.AddExp(effectValue);
                Debug.Log($"增加经验: {effectValue}");
                break;
                
            case ConsumableEffectType.AddSkillPoint:
                // 增加技能点
                character.AddSkillPoint(effectValue);
                Debug.Log($"增加技能点: {effectValue}");
                break;
        }
    }
    
    // 重写出售价格计算方法
    public override int GetSellPrice()
    {
        // 消耗品的出售价格受效果影响
        float multiplier = 0.7f;
        
        // 根据稀有度调整出售价格
        switch (rarity)
        {
            case 1: // 优秀
                multiplier = 0.75f;
                break;
            case 2: // 精良
                multiplier = 0.8f;
                break;
            case 3: // 史诗
                multiplier = 0.85f;
                break;
            case 4: // 传说
                multiplier = 0.9f;
                break;
        }
        
        return (int)(baseValue * multiplier);
    }
}

// 装备物品数据类
public class EquipmentItemData : ItemData
{
    // 装备提供的属性加成
    public int addHealth = 0;      // 增加生命值
    public int addMana = 0;        // 增加魔法值
    public int addStamina = 0;     // 增加耐力
    public int addStrength = 0;    // 增加力量
    public int addIntelligence = 0; // 增加智力
    public int addDexterity = 0;   // 增加敏捷
    public int addDefense = 0;     // 增加防御
    public int addMagicDefense = 0; // 增加魔法防御
    
    // 装备的特殊效果ID列表
    public string[] equipEffects;
    
    // 装备等级需求
    public int requiredLevel = 1;
    
    // 装备是否绑定（不可交易）
    public bool isBound = false;
    
    public EquipmentItemData()
    {
        itemType = ItemType.Equipment;
    }
    
    public override void UseEffect(CharacterData character)
    {
        base.UseEffect(character);
        
        // 装备物品时的特殊效果
        if (character == null) return;
        
        // 先检查角色等级要求
        if (character.level < requiredLevel)
        {
            Debug.LogWarning($"角色等级不足，需要等级: {requiredLevel}，当前等级: {character.level}");
            return;
        }
        
        // 装备物品
        character.EquipItem(this);
        
        // 应用装备属性加成
        if (addHealth > 0 && character.health != null)
        {
            character.health.max += addHealth;
        }
        
        if (addMana > 0 && character.mana != null)
        {
            character.mana.max += addMana;
        }
        
        if (addStamina > 0 && character.stamina != null)
        {
            character.stamina.max += addStamina;
        }
        
        // 添加其他属性加成
        character.strength += addStrength;
        character.intelligence += addIntelligence;
        character.dexterity += addDexterity;
        character.defense += addDefense;
        character.magicDefense += addMagicDefense;
        
        // 应用装备特殊效果
        if (equipEffects != null)
        {
            foreach (var effectId in equipEffects)
            {
                character.AddEquipEffect(effectId);
            }
        }
        
        Debug.Log($"装备物品: {itemName}，提供属性加成: 生命值+{addHealth}, 力量+{addStrength}, 防御+{addDefense}");
    }
}
*/