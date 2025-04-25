using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploreArea : MonoBehaviour
{
    private bool _isEnabled = false;
    private Collider2D _collider;

    // Update is called once per frame
    void Update()
    {
        if (_isEnabled)
        {
            if (Input.GetKeyUp(KeyCode.F))
            {
                if (!ExploreMapUIPanel.Instance.uiPanel.activeSelf)
                {
                    ExploreMapUIPanel.Instance.Show();
                }
            }
        }
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
