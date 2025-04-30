using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingMask : GlobalUIBase
{
    public Image backgroundImage;
    public Transform loadingIcon;
    public TextMeshProUGUI loadingText;

    private bool _isLoading = true;
    private string _loadingText = "Loading";
    private float _timer = 0f;
    private int _dotCount = 0;

    private void Update()
    {
        if (loadingIcon && gameObject.activeSelf && _isLoading)
        {
            loadingIcon.Rotate(0, 0, -360 * Time.deltaTime);
        }

        if (loadingText && gameObject.activeSelf && _isLoading)
        {
            _timer += Time.deltaTime;
            if (_timer >= 0.5f)
            {
                _timer = 0f;
                _dotCount = (_dotCount + 1) % 4;
                string dots = new string('.', _dotCount);
                loadingText.text = _loadingText + dots;
            }
        }
    }

    /// <summary>
    /// 设置加载状态
    /// </summary>
    /// <param name="isLoading">是否是加载场景</param>
    public void SetLoading(bool isLoading)
    {
        _isLoading = isLoading;
        if (loadingIcon)
        {
            loadingIcon.gameObject.SetActive(isLoading);
        }
        if (loadingText)
        {
            loadingText.gameObject.SetActive(isLoading);
        }
    }

    public override void Show()
    {
        base.Show();
        if (backgroundImage && backgroundImage.color.a != 1)
        {
            backgroundImage.SetAlpha(1);
        }
    }

    public override void Hide()
    {
        if (backgroundImage && !_isLoading)
        {
            Tween.Alpha(backgroundImage, 0, 0.5f, Ease.OutSine).OnComplete(() => base.Hide());
        }
        else
        {
            base.Hide();
        }
    }
}