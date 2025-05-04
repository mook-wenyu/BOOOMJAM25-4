using System.Collections.Generic;
using UnityEngine;

public class RoomBuildingSystem : MonoSingleton<RoomBuildingSystem>
{
    public List<RoomFloor> floors = new();
    public Transform buildingContainer;
    public GameObject buildingPrefab;

    // 正在生产中的建筑的UI容器
    public Transform buildingTimeContainer;
    public GameObject buildingTimePrefab;

    private GameObject currentBuilding;
    private RoomFloor targetFloor;
    private float buildingWidth;

    private string currentBuildingInstanceId;

    private string buildingInstanceId;
    private string buildingId;

    private bool startReady = false;    // 是否可以开始放置

    private Color validColor = new Color(0.5f, 1f, 0.5f, 1f);
    private Color invalidColor = new Color(1f, 0.5f, 0.5f, 1f);
    private Color buildingColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    // 存储所有正在生产中的建筑的UI引用
    public Dictionary<string, GameObject> buildingTimeUIs = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (GameMgr.currentSaveData.floors.Count == 0)
        {
            foreach (var floor in floors)
            {
                GameMgr.currentSaveData.floors[floor.id] = floor;
            }
        }

        // 加入预设建筑
        foreach (Transform building in buildingContainer)
        {
            // 获取建筑的位置
            float xPos = building.transform.position.x;
            float yPos = building.transform.position.y;

            // 根据位置找到对应的房间楼层
            var floor = FindRoomFloorByPosition(xPos, yPos);
            if (floor == null) continue;

            var be = building.GetComponent<BuildingEntity>();
            var boxCollider = building.GetComponent<BoxCollider2D>();
            float buildingWidth = boxCollider.size.x;

            // 计算左边缘位置
            float leftEdge = xPos - buildingWidth / 2;

            building.name = be.GetId();

            // 创建建筑槽位
            var slot = new BuildingSlot
            {
                startX = leftEdge,
                endX = leftEdge + buildingWidth,
                floorId = floor.id,
                building = building.gameObject,
                buildingInstanceId = be.GetId(),
                buildingId = be.GetBuildingId()
            };
            floor.placedBuildings[slot.buildingInstanceId] = slot;
        }

        buildingContainer.DestroyAllChildren();

        foreach (var floor in GameMgr.currentSaveData.floors.Values)
        {
            foreach (var slot in floor.placedBuildings.Values)
            {
                var building = Instantiate(buildingPrefab, buildingContainer);
                building.name = slot.buildingInstanceId;
                // 设置位置时，将左边缘转换为中心点位置
                float centerX = slot.startX + (slot.endX - slot.startX) / 2;
                building.transform.position = new Vector2(centerX, floor.yPosition);
                building.GetComponent<BuildingEntity>().Setup(slot.buildingId, slot.buildingInstanceId);
            }
        }

        // 找到并存储现有的正在生产中的建筑的UI引用
        foreach (Transform child in buildingTimeContainer)
        {
            buildingTimeUIs[child.name] = child.gameObject;
        }
    }

    void Update()
    {
        if (currentBuilding != null)
        {
            UpdateBuildingPosition();

            if (Input.GetMouseButtonUp(0) && startReady)
            {
                TryPlaceBuilding();
            }

            if (Input.GetMouseButtonUp(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacing();
            }
        }

        // 更新所有正在生产中的建筑的UI位置
        UpdateBuildingTimePositions();
    }

    private void LateUpdate()
    {
        if (currentBuilding != null)
        {
            startReady = true;
        }
    }

    /// <summary>
    /// 开始放置建筑物
    /// </summary>
    public void StartPlacingBuilding(string currentBuildingInstanceId, string buildingId)
    {
        if (currentBuilding != null)
        {
            Destroy(currentBuilding);
        }

        this.currentBuildingInstanceId = currentBuildingInstanceId;
        this.buildingInstanceId = System.Guid.NewGuid().ToString("N");
        this.buildingId = buildingId;

        currentBuilding = Instantiate(buildingPrefab, buildingContainer);
        currentBuilding.name = buildingInstanceId;
        var entity = currentBuilding.GetComponent<BuildingEntity>();
        entity.Setup(buildingId, buildingInstanceId);

        buildingWidth = currentBuilding.GetComponent<BoxCollider2D>().size.x;

        // 初始禁用碰撞体
        currentBuilding.GetComponent<Collider2D>().enabled = false;

        WorldMgr.Instance.StopCameraFollowing();
    }

    /// <summary>
    /// 更新预览位置
    /// </summary>
    void UpdateBuildingPosition()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 1. 根据位置找到对应房间的楼层
        targetFloor = null;
        float minDistance = float.MaxValue;

        foreach (var floor in GameMgr.currentSaveData.floors.Values)
        {
            // 检查水平位置是否在房间范围内
            if (mousePos.x >= floor.minX && mousePos.x <= floor.maxX)
            {
                // 在房间范围内，计算与楼层的垂直距离
                float dist = Mathf.Abs(mousePos.y - floor.yPosition);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    targetFloor = floor;
                }
            }
        }

        // 如果没有找到合适的楼层，使用最近的楼层
        if (targetFloor == null)
        {
            foreach (var floor in GameMgr.currentSaveData.floors.Values)
            {
                float dist = Mathf.Abs(mousePos.y - floor.yPosition);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    targetFloor = floor;
                }
            }
        }

        float targetY = targetFloor.yPosition;

        // 2. 吸附到X轴网格
        float snappedX = SnapToGrid(mousePos.x, targetFloor);

        // 3. 约束在房间范围内
        snappedX = Mathf.Clamp(
            snappedX,
            targetFloor.minX + buildingWidth / 2,
            targetFloor.maxX - buildingWidth / 2
        );

        // 更新预览位置
        currentBuilding.transform.position = new Vector2(snappedX, targetY);

        // 4. 更新预览颜色
        bool isValid = IsPositionValid(targetFloor, snappedX, buildingWidth);
        currentBuilding.GetComponent<SpriteRenderer>().color =
            isValid ? validColor : invalidColor;
    }

    /// <summary>
    /// 尝试放置建筑物
    /// </summary>
    void TryPlaceBuilding()
    {
        float snappedX = currentBuilding.transform.position.x;

        if (IsPositionValid(targetFloor, snappedX, buildingWidth))
        {
            AudioMgr.Instance.PlaySound("建造");
            startReady = false;

            // 正式放置建筑物，计算左边缘位置
            float leftEdge = snappedX - buildingWidth / 2;
            var newSlot = new BuildingSlot
            {
                startX = leftEdge,
                endX = leftEdge + buildingWidth,
                building = currentBuilding,
                buildingInstanceId = buildingInstanceId,
                buildingId = buildingId,
                floorId = targetFloor.id
            };

            targetFloor.placedBuildings[newSlot.buildingInstanceId] = newSlot;
            GameMgr.currentSaveData.floors[targetFloor.id] = targetFloor;

            // 启用碰撞体
            currentBuilding.GetComponent<Collider2D>().enabled = true;
            currentBuilding.GetComponent<SpriteRenderer>().color = buildingColor;

            // 创建并初始化UI
            var buildingTime = Instantiate(buildingTimePrefab, buildingTimeContainer);
            buildingTime.name = buildingInstanceId;

            // 存储UI引用
            buildingTimeUIs[buildingInstanceId] = buildingTime;

            // 初始化位置
            Vector2 worldPos = new Vector2(snappedX + buildingWidth / 2, targetFloor.yPosition + 0.5f);
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            buildingTime.GetComponent<RectTransform>().position = screenPos;

            currentBuilding = null;
            targetFloor = null;

            BuildingMgr.GetBuildingData<BuildBuildingData>(currentBuildingInstanceId)
                .GetBuildPlatformData().StartBuild(buildingId, buildingInstanceId);

            // 恢复相机跟随
            WorldMgr.Instance.RestoreCameraFollowing();
        }
        else
        {
            GlobalUIMgr.Instance.ShowMessage("无法在此位置放置建筑物");
        }
    }

    /// <summary>
    /// 取消放置
    /// </summary>
    void CancelPlacing()
    {
        Destroy(currentBuilding);
        currentBuilding = null;
        targetFloor = null;
        startReady = false;

        // 恢复相机跟随
        WorldMgr.Instance.RestoreCameraFollowing();
    }

    /// <summary>
    /// 找到最近的楼层
    /// </summary>
    RoomFloor FindNearestFloor(float yPos)
    {
        RoomFloor nearest = null;
        float minDistance = float.MaxValue;

        foreach (var floor in floors)
        {
            float dist = Mathf.Abs(yPos - floor.yPosition);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = floor;
            }
        }
        return nearest;
    }

    /// <summary>
    /// 检查位置是否合法
    /// </summary>
    bool IsPositionValid(RoomFloor floor, float xPos, float width)
    {
        // 计算建筑的左右边缘
        float leftEdge = xPos - width / 2;
        float rightEdge = xPos + width / 2;

        // 检查房间边界
        if (leftEdge < floor.minX || rightEdge > floor.maxX)
        {
            return false;
        }

        // 检查与其他建筑物的重叠
        foreach (var slot in floor.placedBuildings.Values)
        {
            if (leftEdge < slot.endX && rightEdge > slot.startX)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 吸附到网格
    /// </summary>
    float SnapToGrid(float xPos, RoomFloor floor)
    {
        float gridSize = floor.gridCellSize;
        float relativeX = xPos - floor.minX;
        return floor.minX + Mathf.Round(relativeX / gridSize) * gridSize;
    }

    void OnDrawGizmos()
    {
        if (floors == null) return;

        foreach (var floor in floors)
        {
            Gizmos.color = floor.gizmoColor;

            // 绘制楼层线
            Gizmos.DrawLine(
                new Vector3(floor.minX, floor.yPosition, 0),
                new Vector3(floor.maxX, floor.yPosition, 0)
            );

            // 绘制房间边界
            Gizmos.DrawLine(
                new Vector3(floor.minX, floor.yPosition - 0.5f, 0),
                new Vector3(floor.minX, floor.yPosition + 0.5f, 0)
            );

            Gizmos.DrawLine(
                new Vector3(floor.maxX, floor.yPosition - 0.5f, 0),
                new Vector3(floor.maxX, floor.yPosition + 0.5f, 0)
            );

            // 绘制网格
            if (floor.gridCellSize > 0)
            {
                Gizmos.color = new Color(floor.gizmoColor.r, floor.gizmoColor.g, floor.gizmoColor.b, 1f);
                for (float x = floor.minX; x <= floor.maxX; x += floor.gridCellSize)
                {
                    Gizmos.DrawLine(
                        new Vector3(x, floor.yPosition - 0.1f, 0),
                        new Vector3(x, floor.yPosition + 0.1f, 0)
                    );
                }
            }
        }
    }

    /// <summary>
    /// 根据世界位置更新所有正在生产中的建筑的UI位置
    /// </summary>
    void UpdateBuildingTimePositions()
    {
        foreach (var kvp in buildingTimeUIs)
        {
            string instanceId = kvp.Key;
            GameObject timeUI = kvp.Value;

            // 找到匹配的建筑槽位
            BuildingSlot slot = null;
            RoomFloor floor = null;

            foreach (var f in GameMgr.currentSaveData.floors.Values)
            {
                if (f.placedBuildings.TryGetValue(instanceId, out slot))
                {
                    floor = f;
                    break;
                }
            }

            if (slot != null && floor != null)
            {
                // 计算世界位置（使用建筑中心点）
                float buildingCenter = slot.startX + (slot.endX - slot.startX) / 2;
                Vector2 worldPos = new Vector2(buildingCenter, floor.yPosition + 0.5f);

                // 转换为屏幕位置
                Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                timeUI.GetComponent<RectTransform>().position = screenPos;
            }
        }
    }

    /// <summary>
    /// 根据X坐标找到对应的房间楼层
    /// </summary>
    RoomFloor FindRoomFloorByPosition(float xPos, float yPos)
    {
        RoomFloor targetFloor = null;
        float minDistance = float.MaxValue;

        foreach (var floor in floors)
        {
            // 检查水平位置是否在房间范围内
            if (xPos >= floor.minX && xPos <= floor.maxX)
            {
                // 在房间范围内，计算与楼层的垂直距离
                float dist = Mathf.Abs(yPos - floor.yPosition);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    targetFloor = floor;
                }
            }
        }

        // 如果没有找到合适的楼层，使用最近的楼层
        if (targetFloor == null)
        {
            foreach (var floor in floors)
            {
                float dist = Mathf.Abs(yPos - floor.yPosition);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    targetFloor = floor;
                }
            }
        }

        return targetFloor;
    }
}
