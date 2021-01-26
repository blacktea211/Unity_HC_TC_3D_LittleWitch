
using UnityEngine;

public class player : MonoBehaviour
{
    [Header("移動速度"), Range(0, 1000)]
    public float speed = 10;
    [Header("跳躍速度"), Range(0, 1000)]
    public float jump = 10;

    /// <summary>
    /// 判斷是否落地
    /// </summary>
    private bool onGround;
    private Animator ani;
    private Rigidbody rig;


    // 喚醒事件：在 Start 前執行一次
    private void Awake()
    {
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Move();
    }


    /// <summary>
    /// 移動方法
    /// </summary>
    private void Move()
    {

    }
}
