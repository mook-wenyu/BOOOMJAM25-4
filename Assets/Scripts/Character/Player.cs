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

        if (_characterData.pos == null)
        {
            _characterData.pos = new Pos(transform.position.x, transform.position.y);
        }
        else
        {
            transform.position = _characterData.pos.ToVector2();
        }
        if (_characterData.direction)
        {
            transform.localScale = new Vector3(0.8f, 0.8f, 1);
        }
        else
        {
            transform.localScale = new Vector3(-0.8f, 0.8f, 1);
        }

        _characterData.status = CharacterStatus.Idle;
        _animator.SetBool("IsMove", false);
    }

    void FixedUpdate()
    {
        bool isMoving = false;
        if (_characterData.status != CharacterStatus.Move && _characterData.status != CharacterStatus.Idle)
        {
            return;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            _characterData.direction = true;
            transform.localScale = new Vector3(0.8f, 0.8f, 1);
            _characterData.status = CharacterStatus.Move;
            _animator.SetBool("IsMove", true);
            _rigidbody2D.MovePosition(transform.position + new Vector3(-(Time.deltaTime * _characterData.moveSpeed), 0, 0));
            _characterData.pos.x -= Time.deltaTime * _characterData.moveSpeed;
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            _characterData.direction = false;
            transform.localScale = new Vector3(-0.8f, 0.8f, 1);
            _characterData.status = CharacterStatus.Move;
            _animator.SetBool("IsMove", true);
            _rigidbody2D.MovePosition(transform.position + new Vector3((Time.deltaTime * _characterData.moveSpeed), 0, 0));
            _characterData.pos.x += Time.deltaTime * _characterData.moveSpeed;
            isMoving = true;
        }

        if (!isMoving)
        {
            _characterData.pos.x = transform.position.x;
            _characterData.pos.y = transform.position.y;
            _characterData.status = CharacterStatus.Idle;
            _animator.SetBool("IsMove", false);
        }
    }

    public Animator GetAnimator()
    {
        return _animator;
    }

    public Rigidbody2D GetRigidbody2D()
    {
        return _rigidbody2D;
    }

    public CharacterData GetCharacterData()
    {
        return _characterData;
    }

}
