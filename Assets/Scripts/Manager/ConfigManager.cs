using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Scripting;

public class ConfigManager : SingleMono<ConfigManager>
{
    private Dictionary<string, Dictionary<int, BaseConfig>> ConfigDatas;

    public delegate bool ConfigFilter<T>(T config) where T : BaseConfig;
    public override void Init()
    {
        ConfigDatas = new Dictionary<string, Dictionary<int, BaseConfig>>();

        var textAssets = Resources.LoadAll<TextAsset>("Configs");
        for (int i = 0; i < textAssets?.Length; i++)
        {
            var textAsset = textAssets[i];
            var data = JsonConvert.DeserializeObject<Dictionary<int, BaseConfig>>(textAsset.text, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            ConfigDatas.Add(textAsset.name, data);
        }
    }

    [Preserve]
    public T GetConfig<T>(int id) where T : BaseConfig
    {
        var configName = typeof(T).Name;
        if (ConfigDatas.ContainsKey(configName)) 
        {
            var data = ConfigDatas[configName];
            if (data.ContainsKey(id)) 
            {
                return data[id] as T;
            }
        }

        return null;
    }
    
    [Preserve]
    public IList<T> GetConfigList<T>() where T : BaseConfig
    {
        return GetConfigListWithFilter<T>();
    }

    [Preserve]
    public IList<T> GetConfigListWithFilter<T>(ConfigFilter<T> configFilter = null) where T : BaseConfig
    {
        var configName = typeof(T).Name;
        var configList = new List<T>();
        if (ConfigDatas.ContainsKey(configName))
        {
            var data = ConfigDatas[configName];
            foreach (var kv in data)
            {
                var value = kv.Value as T;
                if (configFilter == null || configFilter(value))
                    configList.Add(value);
            }

            return configList;
        }

        return null;
    }
}
