using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 电梯方向
/// </summary>
public enum ElevatorDirection
{
    /// <summary>
    /// 上
    /// </summary>
    Up,
    /// <summary>
    /// 下
    /// </summary>
    Down
}

public class Elevator : MonoBehaviour
{
    public BoxCollider2D endCollider;
    public ElevatorDirection direction;

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (direction == ElevatorDirection.Up)
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                {
                    collision.gameObject.transform.position = endCollider.transform.position;
                }
            }
            if (direction == ElevatorDirection.Down)
            {
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                {
                    collision.gameObject.transform.position = endCollider.transform.position;
                }
            }
        }
    }

}
