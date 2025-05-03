using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public BoxCollider2D upCollider;
    public BoxCollider2D downCollider;

    private bool _isEnabled = false;
    private Collider2D _collider;
    private BoxCollider2D _boxCollider;

    private SimpleTipsUI _tipsUI;
    private Sprite _iconW = null;
    private Sprite _iconS = null;

    void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _boxCollider.isTrigger = true;
    }

    void Update()
    {
        if (_isEnabled)
        {
            if (upCollider)
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                {
                    upCollider.enabled = false;
                    _collider.gameObject.transform.position = new Vector3(upCollider.transform.position.x, upCollider.transform.position.y - 1.5f, _collider.gameObject.transform.position.z);
                    CharacterEntityMgr.Instance.GetPlayer().GetCharacterData().pos.x = upCollider.transform.position.x;
                    CharacterEntityMgr.Instance.GetPlayer().GetCharacterData().pos.y = upCollider.transform.position.y;
                    EnabledCollider(upCollider).Forget();
                }
            }
            if (downCollider)
            {
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                {
                    downCollider.enabled = false;
                    _collider.gameObject.transform.position = new Vector3(downCollider.transform.position.x, downCollider.transform.position.y - 1.5f, _collider.gameObject.transform.position.z);
                    CharacterEntityMgr.Instance.GetPlayer().GetCharacterData().pos.x = downCollider.transform.position.x;
                    CharacterEntityMgr.Instance.GetPlayer().GetCharacterData().pos.y = downCollider.transform.position.y;
                    EnabledCollider(downCollider).Forget();
                }
            }
        }
    }

    void LateUpdate()
    {
        // 如果提示UI存在且启用，则每帧更新位置
        if (_isEnabled && _tipsUI != null)
        {
            UpdateTipsPosition();
        }
    }

    private async UniTask EnabledCollider(BoxCollider2D colliders)
    {
        await UniTask.Delay(200);
        await UniTask.Yield();
        colliders.enabled = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = collision;
            _isEnabled = true;
            UpdateTips();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = null;
            _isEnabled = false;
            GlobalUIMgr.Instance.Hide<SimpleTipsUI>();
        }
    }

    /// <summary>
    /// 更新并显示物品提示
    /// </summary>
    public void UpdateTips()
    {
        _tipsUI = GlobalUIMgr.Instance.Show<SimpleTipsUI>(GlobalUILayer.TooltipLayer);
        if (_iconW == null)
        {
            _iconW = Resources.Load<Sprite>(Path.Combine("Icon", "UI", "keyboard_w"));
            _iconS = Resources.Load<Sprite>(Path.Combine("Icon", "UI", "keyboard_s"));
        }
        if (upCollider && downCollider)
        {
            _tipsUI.SetIcon(_iconW, _iconS);
        }
        else if (upCollider)
        {
            _tipsUI.SetIcon(_iconW);
        }
        else if (downCollider)
        {
            _tipsUI.SetIcon(_iconS);
        }
        UpdateTipsPosition();
    }

    private void UpdateTipsPosition()
    {
        Vector2 worldPos = transform.position;
        // 调整世界坐标到建筑物顶部中心
        worldPos.x -= _boxCollider.bounds.extents.x / 4;
        worldPos.y += _boxCollider.bounds.size.y - _boxCollider.offset.y / 1.2f;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        // 将世界空间的尺寸转换为屏幕空间的尺寸
        Vector2 screenSize = new Vector2(
            _boxCollider.bounds.size.x / Screen.width * Camera.main.pixelWidth,
            _boxCollider.bounds.size.y / Screen.height * Camera.main.pixelHeight);

        _tipsUI.UpdatePosition(screenPos, screenSize);
    }

}
