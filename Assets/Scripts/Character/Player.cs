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
        // 状态检查
        if (_characterData.status != CharacterStatus.Move && _characterData.status != CharacterStatus.Idle)
        {
            return;
        }

        // 获取输入（-1左, 0无输入, 1右）
        float moveInput = Input.GetAxisRaw("Horizontal");
        bool isMoving = Mathf.Abs(moveInput) > 0.1f; // 检测是否有输入

        // 更新动画状态
        _animator.SetBool("IsMove", isMoving);

        // 移动和翻转控制
        if (isMoving)
        {
            _characterData.SetStatus(CharacterStatus.Move);

            // 设置方向（true=左，false=右）
            _characterData.direction = moveInput < 0;

            // 根据方向翻转角色
            transform.localScale = new Vector3(
                moveInput < 0 ? 0.8f : -0.8f, // 左:0.8, 右:-0.8
                0.8f,
                1
            );

            // 物理移动
            _rigidbody2D.velocity = new Vector2(
                moveInput * _characterData.GetMoveSpeed(),
                _rigidbody2D.velocity.y
            );
        }
        else // 无输入时
        {
            _characterData.SetStatus(CharacterStatus.Idle);
            _rigidbody2D.velocity = new Vector2(0, _rigidbody2D.velocity.y); // 立刻停止
        }
    }

    void LateUpdate()
    {
        // 同步位置数据（如果需要）
        _characterData.pos.x = transform.position.x;
        _characterData.pos.y = transform.position.y;
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
