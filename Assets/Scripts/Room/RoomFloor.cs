using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 房间楼层
/// </summary>
[Serializable]
public class RoomFloor
{
    public string id;
    public float yPosition;          // 楼层Y坐标
    public float minX;               // 房间左边界
    public float maxX;               // 房间右边界
    public float gridCellSize = 1f;  // 本层网格大小
    
    [NonSerialized]
    public Color gizmoColor = Color.blue;

    [HideInInspector] public Dictionary<string, BuildingSlot> placedBuildings = new();
}

/// <summary>
/// 建筑槽
/// </summary>
[Serializable]
public class BuildingSlot
{
    public float startX;
    public float endX;
    public string floorId;
    public string buildingInstanceId;
    public string buildingId;

    [NonSerialized]
    public GameObject building;
}