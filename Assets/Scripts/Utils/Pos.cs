using System;
using UnityEngine;

/// <summary>
/// 位置
/// </summary>
[Serializable]
public class Pos
{
    /// <summary>
    /// X坐标
    /// </summary>
    public float x;

    /// <summary>
    /// Y坐标
    /// </summary>
    public float y;

    /// <summary>
    /// 构造函数
    /// </summary>
    public Pos() { }

    /// <summary>
    /// 构造函数
    /// </summary>
    public Pos(Pos pos)
    {
        x = pos.x;
        y = pos.y;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    public Pos(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    public Pos(Vector2 vector)
    {
        x = vector.x;
        y = vector.y;
    }

    /// <summary>
    /// 转换为Vector2
    /// </summary>
    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }

    /// <summary>
    /// 转换为Vector2Int
    /// </summary>
    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int)x, (int)y);
    }

    /// <summary>
    /// 隐式转换为Vector2
    /// </summary>
    public static implicit operator Vector2(Pos pos)
    {
        return new Vector2(pos.x, pos.y);
    }

    /// <summary>
    /// 隐式转换为Pos
    /// </summary>
    public static implicit operator Pos(Vector2 vector)
    {
        return new Pos(vector);
    }

    /// <summary>
    /// 隐式转换为Vector2Int
    /// </summary>
    public static implicit operator Vector2Int(Pos pos)
    {
        return new Vector2Int((int)pos.x, (int)pos.y);
    }

    /// <summary>
    /// 隐式转换为Pos
    /// </summary>
    public static implicit operator Pos(Vector2Int vector)
    {
        return new Pos(vector.x, vector.y);
    }

}
