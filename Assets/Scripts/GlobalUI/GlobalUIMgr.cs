using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalUIMgr : MonoSingleton<GlobalUIMgr>
{
    private Canvas _uiCanvas;
    private RectTransform[] _layers; // 按GlobalUILayer枚举顺序排列

    // 消息相关配置
    private const float MESSAGE_DURATION = 3f; // 消息显示时间
    private const float MESSAGE_SPACING = 8f;  // 消息之间的间距

    private Dictionary<Type, GlobalUIBase> _uiCache = new();

    private LoadingMask _loadingMask;
    private Queue<PopupDialog> _popupPool = new();
    private ItemActionPopup _itemActionPopup;
    private Queue<Message> _messagePool = new();
    private List<Message> _activeMessages = new();

    public void Init()
    {
        InitializeCanvas();
        InitLoadingMask();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void OnDestroy()
    {
        ClearAll();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        base.OnDestroy();
    }

    #region Canvas初始化
    private void InitializeCanvas()
    {
        if (!_uiCanvas)
        {
            var canvasObj = new GameObject("UICanvas");
            _uiCanvas = canvasObj.AddComponent<Canvas>();
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasObj);
        }

        _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _uiCanvas.sortingOrder = 100;

        var scaler = _uiCanvas.GetComponent<CanvasScaler>();
        if (!scaler) scaler = _uiCanvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        var raycaster = _uiCanvas.GetComponent<GraphicRaycaster>();
        if (!raycaster) raycaster = _uiCanvas.gameObject.AddComponent<GraphicRaycaster>();

        if (_layers == null || _layers.Length == 0)
        {
            InitializeLayers();
        }

        if (!FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>())
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            DontDestroyOnLoad(eventSystem);
        }
    }

    private void InitializeLayers()
    {
        int layerCount = Enum.GetValues(typeof(GlobalUILayer)).Length;
        _layers = new RectTransform[layerCount];

        for (int i = 0; i < layerCount; i++)
        {
            var layerObj = new GameObject($"Layer_{(GlobalUILayer)i}");
            var rectTransform = layerObj.AddComponent<RectTransform>();
            rectTransform.SetParent(_uiCanvas.transform, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.SetSiblingIndex(i);
            _layers[i] = rectTransform;
        }
    }
    #endregion

    #region 加载遮罩
    private void InitLoadingMask()
    {
        if (!_loadingMask)
        {
            var prefab = Resources.Load<GameObject>(Path.Combine("Prefabs", "GlobalUI", "LoadingMask"));
            if (prefab)
            {
                _loadingMask = Instantiate(prefab, GetLayer(GlobalUILayer.TopLayer)).GetComponent<LoadingMask>();
                _loadingMask.gameObject.SetActive(false);
            }
        }
    }

    public void ShowLoadingMask(bool show, bool isLoading = true)
    {
        if (_loadingMask)
        {
            _loadingMask.SetLoading(isLoading);
            if (show) _loadingMask.Show();
            else _loadingMask.Hide();
        }
    }

    public bool IsShowLoadingMask()
    {
        if (_loadingMask)
        {
            return _loadingMask.gameObject.activeSelf;
        }
        return false;
    }
    #endregion

    #region 消息系统
    private Message CreateMessage()
    {
        Message msg;
        if (_messagePool.Count > 0)
        {
            msg = _messagePool.Dequeue();
        }
        else
        {
            var prefab = Resources.Load<GameObject>(Path.Combine("Prefabs", "GlobalUI", "Message"));
            if (!prefab)
            {
                Debug.LogError("找不到Message预制体");
                return null;
            }

            msg = Instantiate(prefab, GetLayer(GlobalUILayer.MessageLayer)).GetComponent<Message>();
            var rectTransform = msg.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(-24, -24);
        }

        msg.gameObject.SetActive(false);
        return msg;
    }

    public void ShowMessage(string content, MessageType type = MessageType.Info)
    {
        var msg = _messagePool.Count > 0 ? _messagePool.Dequeue() : CreateMessage();
        if (msg == null) return;

        // 设置初始位置(从底部开始)
        var msgRect = msg.GetComponent<RectTransform>();
        msgRect.anchoredPosition = new Vector2(-24, -(Screen.height / 2));

        msg.SetMessage(content, type);
        msg.Show();

        // 添加到活动消息列表
        _activeMessages.Add(msg);

        // 更新所有消息位置
        UpdateMessagePositions(true);

        StartCoroutine(AutoHideMessage(msg));
    }

    private IEnumerator AutoHideMessage(Message msg)
    {
        yield return new WaitForSeconds(MESSAGE_DURATION);
        _activeMessages.Remove(msg);
        msg.Hide();
        _messagePool.Enqueue(msg);
        UpdateMessagePositions();
    }

    private void UpdateMessagePositions(bool animate = false)
    {
        float yPos = 128;
        foreach (var msg in _activeMessages)
        {
            var rect = msg.GetComponent<RectTransform>();
            Vector2 targetPos = new(-24, -yPos);

            if (animate)
            {
                // 使用Tween动画移动到目标位置
                Tween.UIAnchoredPosition(rect, targetPos, 0.3f, Ease.OutCubic);
            }
            else
            {
                rect.anchoredPosition = targetPos;
            }

            yPos += rect.rect.height + MESSAGE_SPACING;
        }
    }
    #endregion

    #region 弹窗系统
    private PopupDialog CreatePopup()
    {
        PopupDialog popup;
        if (_popupPool.Count > 0)
        {
            popup = _popupPool.Dequeue();
        }
        else
        {
            var prefab = Resources.Load<GameObject>(Path.Combine("Prefabs", "GlobalUI", "PopupDialog"));
            if (!prefab)
            {
                Debug.LogError("找不到PopupDialog预制体");
                return null;
            }

            popup = Instantiate(prefab, GetLayer(GlobalUILayer.PopupLayer)).GetComponent<PopupDialog>();
        }

        popup.gameObject.SetActive(false);
        return popup;
    }

    public PopupDialog ShowPopup(string title, string content, UnityAction onConfirm = null,
        UnityAction onCancel = null, string confirmText = "确认", string cancelText = "取消",
        bool singleButton = false)
    {
        var popup = _popupPool.Count > 0 ? _popupPool.Dequeue() : CreatePopup();
        if (popup == null) return null;

        popup.transform.SetAsLastSibling();
        popup.SetDialog(title, content, onConfirm, onCancel, confirmText, cancelText, singleButton);
        popup.Show();
        return popup;
    }

    public PopupDialog ShowSingleButtonPopup(string title, string content, UnityAction onConfirm = null, string confirmText = "确认")
    {
        return ShowPopup(title, content, onConfirm, null, confirmText, null, true);
    }

    public void RecyclePopup(PopupDialog popup)
    {
        if (popup)
        {
            _popupPool.Enqueue(popup);
        }
    }
    #endregion

    #region 物品操作弹窗
    private ItemActionPopup CreateItemActionPopup()
    {
        if (_itemActionPopup != null) return _itemActionPopup;

        var prefab = Resources.Load<GameObject>(Path.Combine("Prefabs", "GlobalUI", "ItemActionPopup"));
        if (!prefab)
        {
            Debug.LogError("找不到ItemActionPopup预制体");
            return null;
        }

        _itemActionPopup = Instantiate(prefab, GetLayer(GlobalUILayer.PopupLayer)).GetComponent<ItemActionPopup>();
        _itemActionPopup.gameObject.SetActive(false);
        return _itemActionPopup;
    }

    public ItemActionPopup ShowItemActionPopup(InventoryItem item, string actionName, UnityAction<int> onConfirm)
    {
        var popup = _itemActionPopup ?? CreateItemActionPopup();
        if (popup == null) return null;

        popup.transform.SetAsLastSibling();
        popup.SetupPopup(item, actionName, onConfirm);
        popup.Show();
        return popup;
    }
    #endregion

    #region UI管理
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _uiCanvas.transform.SetAsLastSibling();
    }

    /// <summary>
    /// 获取UI层级
    /// </summary>
    /// <param name="layer">UI层级</param>
    /// <returns>UI层级</returns>
    public Transform GetLayer(GlobalUILayer layer)
    {
        int index = (int)layer;
        return _layers[index];
    }

    /// <summary>
    /// 获取UI
    /// </summary>
    /// <typeparam name="T">UI类型</typeparam>
    /// <returns>UI</returns>
    public T Get<T>() where T : GlobalUIBase
    {
        if (_uiCache.TryGetValue(typeof(T), out var ui))
        {
            return ui as T;
        }
        return null;
    }

    /// <summary>
    /// 显示UI
    /// </summary>
    /// <typeparam name="T">UI类型</typeparam>
    /// <param name="layer">UI层级</param>
    /// <returns>UI</returns>
    public T Show<T>(GlobalUILayer layer) where T : GlobalUIBase
    {
        if (!_uiCache.TryGetValue(typeof(T), out var ui))
        {
            ui = CreateUI<T>(layer);
            if (ui == null) return null;
            _uiCache[typeof(T)] = ui;
        }

        ui.Show();
        return ui as T;
    }

    /// <summary>
    /// 创建UI
    /// </summary>
    /// <typeparam name="T">UI类型</typeparam>
    /// <param name="layer">UI层级</param>
    /// <returns>UI</returns>
    private T CreateUI<T>(GlobalUILayer layer) where T : GlobalUIBase
    {
        string prefabPath = typeof(T).Name;
        var prefab = Resources.Load<GameObject>(Path.Combine("Prefabs", "GlobalUI", prefabPath));
        if (prefab == null)
        {
            Debug.LogError($"找不到UI预制体：{prefabPath}");
            return null;
        }

        return Instantiate(prefab, GetLayer(layer)).GetComponent<T>();
    }

    /// <summary>
    /// 隐藏UI
    /// </summary>
    /// <typeparam name="T">UI类型</typeparam>
    public void Hide<T>() where T : GlobalUIBase
    {
        if (_uiCache.TryGetValue(typeof(T), out var ui))
        {
            ui.Hide();
        }
    }
    #endregion

    #region 清理
    private void DestroyAndClearCache<T>(T component, Queue<T> queue = null) where T : Component
    {
        if (component != null)
        {
            Destroy(component.gameObject);
            queue?.TryDequeue(out _);
        }
    }

    public void ClearLayer(GlobalUILayer layer)
    {
        var layerTransform = GetLayer(layer);
        if (layerTransform == null) return;

        // 清理该层级下的所有子物体
        layerTransform.DestroyAllChildren();

        // 清理UI缓存中属于该层级的项
        var removeKeys = _uiCache.Where(kvp =>
            kvp.Value != null &&
            kvp.Value.transform.parent == layerTransform
        ).Select(kvp => kvp.Key).ToList();

        foreach (var key in removeKeys)
        {
            if (_uiCache.TryGetValue(key, out var ui))
            {
                Destroy(ui.gameObject);
                _uiCache.Remove(key);
            }
        }
    }

    public void ClearAll()
    {
        // 清理所有层级
        foreach (GlobalUILayer layer in Enum.GetValues(typeof(GlobalUILayer)))
        {
            ClearLayer(layer);
        }

        // 清理消息系统
        foreach (var msg in _activeMessages)
        {
            if (msg != null)
            {
                msg.Hide();
                _messagePool.Enqueue(msg);
            }
        }
        _activeMessages.Clear();

        // 清空消息池
        while (_messagePool.Count > 0)
        {
            var msg = _messagePool.Dequeue();
            if (msg != null) Destroy(msg.gameObject);
        }

        // 清空弹窗池
        while (_popupPool.Count > 0)
        {
            var popup = _popupPool.Dequeue();
            if (popup != null) Destroy(popup.gameObject);
        }

        // 清理物品操作弹窗
        DestroyAndClearCache(_itemActionPopup);

        // 清理加载遮罩
        DestroyAndClearCache(_loadingMask);

        // 清理UI缓存
        foreach (var ui in _uiCache.Values.Where(ui => ui != null))
        {
            Destroy(ui.gameObject);
        }
        _uiCache.Clear();

        // 清理Canvas
        DestroyAndClearCache(_uiCanvas);
        _layers = null;
    }
    #endregion

}

/// <summary>
/// 为GlobalUIMgr提供扩展方法，方便非UIBase的UI元素接入
/// </summary>
public static class GlobalUIMgrEx
{
    /// <summary>
    /// 将任意GameObject挂载到指定的UI层级
    /// </summary>
    /// <param name="uiObject">要挂载的UI对象</param>
    /// <param name="layer">目标UI层级</param>
    /// <param name="keepWorldPosition">是否保持世界坐标位置</param>
    /// <returns>挂载的UI对象</returns>
    public static GameObject AttachToLayer(this GameObject uiObject, GlobalUILayer layer, bool keepWorldPosition = false)
    {
        if (GlobalUIMgr.Instance != null)
        {
            var targetLayer = GlobalUIMgr.Instance.GetLayer(layer);
            uiObject.transform.SetParent(targetLayer, keepWorldPosition);
            return uiObject;
        }
        Debug.LogWarning("GlobalUIMgr instance not found!");
        return uiObject;
    }

    /// <summary>
    /// 实例化预制体并挂载到指定的UI层级
    /// </summary>
    /// <param name="prefab">UI预制体</param>
    /// <param name="layer">目标UI层级</param>
    /// <returns>实例化并挂载的UI对象</returns>
    public static GameObject InstantiateToLayer(this GameObject prefab, GlobalUILayer layer)
    {
        if (GlobalUIMgr.Instance != null)
        {
            var targetLayer = GlobalUIMgr.Instance.GetLayer(layer);
            return UnityEngine.Object.Instantiate(prefab, targetLayer);
        }
        Debug.LogWarning("GlobalUIMgr instance not found!");
        return UnityEngine.Object.Instantiate(prefab);
    }

}