using Cysharp.Threading.Tasks;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public BoxCollider2D upCollider;
    public BoxCollider2D downCollider;

    private bool _isEnabled = false;
    private Collider2D _collider;

    void Update()
    {
        if (_isEnabled)
        {
            if (upCollider)
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                {
                    upCollider.enabled = false;
                    _collider.gameObject.transform.position = new Vector3(upCollider.transform.position.x, upCollider.transform.position.y - 1f, _collider.gameObject.transform.position.z);
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
                    _collider.gameObject.transform.position = new Vector3(downCollider.transform.position.x, downCollider.transform.position.y - 1f, _collider.gameObject.transform.position.z);
                    CharacterEntityMgr.Instance.GetPlayer().GetCharacterData().pos.x = downCollider.transform.position.x;
                    CharacterEntityMgr.Instance.GetPlayer().GetCharacterData().pos.y = downCollider.transform.position.y;
                    EnabledCollider(downCollider).Forget();
                }
            }
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
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = null;
            _isEnabled = false;
        }
    }

}
