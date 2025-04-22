using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private CharacterData _characterData;
    private Animator _animator;
    private Rigidbody2D _rigidbody2D;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _characterData = CharacterMgr.Player();
        _animator.SetBool("IsMove", false);
    }

    void FixedUpdate()
    {
        bool isMoving = false;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.localScale = new Vector3(0.8f, 0.8f, 1);
            _animator.SetBool("IsMove", true);
            _rigidbody2D.MovePosition(transform.position + new Vector3(-(Time.deltaTime * 10f), 0, 0));
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.localScale = new Vector3(-0.8f, 0.8f, 1);
            _animator.SetBool("IsMove", true);
            _rigidbody2D.MovePosition(transform.position + new Vector3((Time.deltaTime * 10f), 0, 0));
            isMoving = true;
        }

        if (!isMoving)
        {
            _animator.SetBool("IsMove", false);
        }
    }
}
