using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Buff数据定义
/// </summary>
[Serializable]
public class BuffData : BaseConfig
{
    /// <summary>
    /// Buff名称
    /// </summary>
    public string name;

    /// <summary>
    /// Buff描述
    /// </summary>
    public string description;

    /// <summary>
    /// 是否为负面效果
    /// </summary>
    public bool isDebuff;

    /// <summary>
    /// Buff图标路径
    /// </summary>
    public string iconPath;

    /// <summary>
    /// Buff默认持续时间(秒)，-1表示永久
    /// </summary>
    public float duration = -1;

    /// <summary>
    /// 最大堆叠层数
    /// </summary>
    public int maxStacks = 1;
    
    /// <summary>
    /// Buff的类型
    /// </summary>
    public BuffType buffType;
    
    /// <summary>
    /// 额外参数（JSON格式）
    /// </summary>
    public string parameters;
    
    /// <summary>
    /// 获取参数值
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="paramName">参数名称</param>
    /// <returns>参数值</returns>
    public T GetParameter<T>(string paramName)
    {
        if (string.IsNullOrEmpty(parameters))
            return default;
            
        try
        {
            // 将JSON字符串解析为Dictionary
            var paramDict = JsonConvert.DeserializeObject<ParameterWrapper>(parameters);
            
            // 尝试获取参数值
            if (paramDict.TryGetValue(paramName, out object value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
                
                // 尝试转换
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    Debug.LogWarning($"无法将参数 {paramName} 转换为类型 {typeof(T).Name}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"解析Buff参数时出错: {e.Message}");
        }
        
        return default;
    }
    
    [Serializable]
    private class ParameterWrapper : Dictionary<string, object> { }
}

/// <summary>
/// Buff类型枚举
/// </summary>
public enum BuffType
{
    /// <summary>
    /// 持续恢复生命值
    /// </summary>
    HealthRegen,
    
    /// <summary>
    /// 持续伤害
    /// </summary>
    DamageOverTime,
    
    /// <summary>
    /// 属性修改
    /// </summary>
    StatModifier,
    
    /// <summary>
    /// 状态效果（如眩晕、沉默等）
    /// </summary>
    StatusEffect,
    
    /// <summary>
    /// 自定义效果
    /// </summary>
    Custom
} 