
using UnityEngine;

public class player : MonoBehaviour
{
    [Header("移動速度"), Range(0, 1000)]
    public float speed = 10;
    [Header("跳躍高度"), Range(0, 1000)]
    public float jump = 10;
    [Header("旋轉速度"), Range(0, 300)]
    public float turn = 10;
    [Header("攝影機角度限制")]
    public Vector2 camLimit = new Vector2(-20, 0);

    /// <summary>
    /// 判斷是否落地
    /// </summary>
    private bool onGround;
    private Animator ani;
    private Rigidbody rig;
    private Transform cam;
    private float x;
    private float y;


    // 喚醒事件：在 Start 前執行一次
    private void Awake()
    {
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        cam = GameObject.Find("攝影機根物件").transform;
    }

    private void Update()
    {
        Move();
        Turncamera();
    }


    /// <summary>
    /// 移動方法
    /// </summary>
    private void Move()
    {
        float v = Input.GetAxis("Vertical");        // 取得 前後軸 值 W S 上下
        float h = Input.GetAxis("Horizontal");      // 取得 前後軸 值 A D 左右

        #region 以下沒執行動作!!!

        // 攝影機旋轉角度後前進，玩家也跟著旋轉後的前進方向移動
        Transform camNew = cam;                                               // 新攝影機角度
        camNew.eulerAngles = new Vector3(0, cam.eulerAngles.y, 0);            // 去掉 x 與 z 角度

        #endregion

        // 加速度 = ( 前方 * 前後值 + 右方 * 左右值 ) * 速度 * 1/60 + 上方 * 加速度上下值
        rig.velocity = ((transform.forward * v + transform.right * h) * speed * Time.deltaTime) + transform.up * rig.velocity.y;
        
        // 動畫.設定布林值 ( "參數名稱" , 鋼體.加速度.值 > 0 )
        ani.SetBool("跑步開關", rig.velocity.magnitude > 0);
    }

    /// <summary>
    /// 旋轉攝影機
    /// </summary>
    private void Turncamera()
    {
        x += Input.GetAxis("Mouse X") * turn * Time.deltaTime;      // 取得滑鼠 x 值 * 選轉角度 * 1/60
        y += Input.GetAxis("Mouse Y") * turn * Time.deltaTime;      // 取得滑鼠 x 值 * 選轉角度 * 1/60
        y = Mathf.Clamp(y, camLimit.x, camLimit.y);                 // 限制y
        cam.localEulerAngles = new Vector3(y, x, 0);                // 攝影機.角度 ( y值 , x值 , 0)
        
    }
}
