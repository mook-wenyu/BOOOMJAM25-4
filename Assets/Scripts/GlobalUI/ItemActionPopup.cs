using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemActionPopup : PopupBase
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;

    [Header("数量选择")]
    public GameObject countSelector;
    public Slider countSlider;
    public TMP_InputField countInput;

    [Header("确认按钮")]
    public Button confirmButton;
    private TextMeshProUGUI _confirmButtonText;

    private InventoryItem _currentItem;
    private UnityAction<int> _onConfirm;
    private int _maxCount = 1;
    private bool _isUpdatingUI;

    protected override void Awake()
    {
        base.Awake();
        _confirmButtonText = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetupPopup(InventoryItem item, string actionName, UnityAction<int> onConfirm)
    {
        _currentItem = item;
        _onConfirm = onConfirm;

        SetupItemInfo(item, actionName);

        // 只有当物品数量大于1时才显示数量选择器
        bool showCountSelector = item.GetCount() > 1;
        countSelector.SetActive(showCountSelector);

        if (showCountSelector)
        {
            SetupCountSelector(item.GetCount());
        }

        RegisterEvents();
    }

    private void SetupItemInfo(InventoryItem item, string actionName)
    {
        var itemData = item.GetItemData();
        titleText.text = itemData.name;
        contentText.text = "是否要" + actionName + "？";
        _confirmButtonText.text = actionName;
    }

    private void SetupCountSelector(int maxCount)
    {
        _maxCount = maxCount;
        countSlider.minValue = 1;
        countSlider.maxValue = _maxCount;
        countSlider.value = 1;
        UpdateCountDisplay(1);
    }

    private void RegisterEvents()
    {
        countSlider.onValueChanged.AddListener(OnSliderValueChanged);
        countInput.onValueChanged.AddListener(OnInputValueChanged);
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    private void OnSliderValueChanged(float value)
    {
        if (_isUpdatingUI) return;
        UpdateCountDisplay(Mathf.RoundToInt(value));
    }

    private void OnInputValueChanged(string value)
    {
        if (_isUpdatingUI) return;
        if (int.TryParse(value, out int count))
        {
            count = Mathf.Clamp(count, 1, _maxCount);
            _isUpdatingUI = true;
            countSlider.value = count;
            UpdateCountDisplay(count);
            _isUpdatingUI = false;
        }
    }

    private void UpdateCountDisplay(int count)
    {
        if (!_isUpdatingUI)
        {
            _isUpdatingUI = true;
            countInput.text = count.ToString();
            _isUpdatingUI = false;
        }
    }

    private void OnConfirmClicked()
    {
        // 如果物品数量为1，直接使用1，否则使用滑动条的值
        int count = _currentItem.GetCount() > 1 ? Mathf.RoundToInt(countSlider.value) : 1;
        _onConfirm?.Invoke(count);
        Hide();
    }

    protected override void OnBeforeHide()
    {
        // 清理事件
        countSlider.onValueChanged.RemoveAllListeners();
        countInput.onValueChanged.RemoveAllListeners();
        confirmButton.onClick.RemoveAllListeners();

        // 清理引用
        _currentItem = null;
        _onConfirm = null;
        _maxCount = 1;
        _isUpdatingUI = false;

        base.OnBeforeHide();
    }
}