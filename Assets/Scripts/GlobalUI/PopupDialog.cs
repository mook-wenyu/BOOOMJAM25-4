using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupDialog : PopupBase
{
    [Header("对话框组件")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;
    public Button confirmButton;
    public Button cancelButton;
    private TextMeshProUGUI _confirmButtonText;
    private TextMeshProUGUI _cancelButtonText;

    private UnityAction _onConfirm;
    private UnityAction _onCancel;

    protected override void Awake()
    {
        base.Awake();
        _confirmButtonText = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
        _cancelButtonText = cancelButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetDialog(string title, string content, UnityAction onConfirm = null, UnityAction onCancel = null,
        string confirmText = "确认", string cancelText = "取消", bool singleButton = false)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        // 设置文本内容
        titleText.text = title;
        contentText.text = content;
        _confirmButtonText.text = confirmText;

        // 清理之前的事件监听
        ClearButtonListeners();

        // 设置确认按钮事件
        confirmButton.onClick.AddListener(OnConfirmClicked);

        // 处理取消按钮
        if (singleButton)
        {
            cancelButton.gameObject.SetActive(false);
        }
        else
        {
            cancelButton.gameObject.SetActive(true);
            _cancelButtonText.text = cancelText;
            cancelButton.onClick.AddListener(OnCancelClicked);
        }
    }

    private void OnConfirmClicked()
    {
        _onConfirm?.Invoke();
        Hide();
    }

    private void OnCancelClicked()
    {
        _onCancel?.Invoke();
        Hide();
    }

    private void ClearButtonListeners()
    {
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
    }

    protected override void OnBeforeHide()
    {
        ClearButtonListeners();

        _onConfirm = null;
        _onCancel = null;

        base.OnBeforeHide();
    }

    protected override void OnHideComplete()
    {
        base.OnHideComplete();
        GlobalUIMgr.Instance?.RecyclePopup(this);
    }

}