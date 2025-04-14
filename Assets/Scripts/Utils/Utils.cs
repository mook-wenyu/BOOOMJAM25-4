using System.IO;
using UnityEngine;

public static class Utils
{
    #region 常量
    /// <summary>
    /// 自动存档名称
    /// </summary>
    public const string AUTO_SAVE_NAME = "auto_save";

    /// <summary>
    /// 存档文件夹名称
    /// </summary>
    public const string SAVE_PATH_NAME = "Save";

    /// <summary>
    /// JSON文件扩展名
    /// </summary>
    public const string JSON_EXTENSION = ".json";
    #endregion

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    public static string GetConfigPath()
    {
        return Path.Combine(Application.persistentDataPath, "config.json");
    }

    /// <summary>
    /// 存档文件夹路径
    /// </summary>
    public static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_PATH_NAME);
    }

    /// <summary>
    /// 获取存档文件路径
    /// </summary>
    public static string GetSavePath(string saveName)
    {
        return Path.Combine(GetSavePath(), $"{saveName}{JSON_EXTENSION}");
    }

    /// <summary>
    /// 自动存档文件路径
    /// </summary>
    public static string GetAutoSavePath()
    {
        return Path.Combine(GetSavePath(), $"{AUTO_SAVE_NAME}{JSON_EXTENSION}");
    }

    /// <summary>
    /// 将 Vector2 转换为 Pos
    /// </summary>
    /// <param name="vector"> Vector2 </param>
    /// <returns> Pos </returns>
    public static Pos ToPos(this Vector2 vector)
    {
        return new Pos(vector);
    }

    /// <summary>
    /// 将 Vector2Int 转换为 Pos
    /// </summary>
    /// <param name="vector"> Vector2Int </param>
    /// <returns> Pos </returns>
    public static Pos ToPos(this Vector2Int vector)
    {
        return new Pos(vector.x, vector.y);
    }

    /// <summary>
    /// 销毁Transform下的所有子物体
    /// </summary>
    /// <param name="transform">目标Transform</param>
    public static void DestroyAllChildren(this Transform transform)
    {
        if (transform == null)
        {
            Debug.LogError("Transform is null!");
            return;
        }

        // 缓存子物体数量，避免多次访问transform.childCount
        int childCount = transform.childCount;

        // 如果没有子物体，直接返回
        if (childCount == 0) return;

        // 从后向前遍历，这样在销毁时不会影响索引
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child != null)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        // 清除所有子物体的引用
        transform.DetachChildren();
    }
}
