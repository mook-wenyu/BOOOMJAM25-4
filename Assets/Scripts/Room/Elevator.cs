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
                    _collider.gameObject.transform.position = upCollider.transform.position;
                    EnabledCollider(upCollider).Forget();
                }
            }
            if (downCollider)
            {
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                {
                    downCollider.enabled = false;
                    _collider.gameObject.transform.position = downCollider.transform.position;
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
            Debug.Log($"进入{name}");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collider = null;
            _isEnabled = false;
            Debug.Log($"离开{name}");
        }
    }

}
