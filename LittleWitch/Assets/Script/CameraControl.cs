using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("追蹤速度"), Range(0, 1000)]
    public float speed = 10;
  
    /// <summary>
    /// 目標
    /// </summary>
    private Transform target;

    private void Awake()
    {
        target = GameObject.Find("玩家").transform;
    }

    // Update 約60秒更新一次
    // Lateupdate 比 Update 晚一楨 執行
    // 若用追蹤，建議用 Lateupdate
    private void LateUpdate()
    {
        Track();
    }

    private void Track()
    {
        Vector3 posTarget = target.position;        //目標座標
        Vector3 posCarema = transform.position;     //攝影機座標

        // 攝影機的新座標 =  三維向量.往前移動 ( 攝影機座標 , 目標座標, 速度 * 1/60 )
        posCarema = Vector3.MoveTowards(posCarema, posTarget, speed * Time.deltaTime);
        //攝影機的座標 = 攝影機的新座標
        transform.position = posCarema;
    }
}
