
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
    [Header("角色旋轉速度")]
    public float turnspeed = 10;
    [Header("檢查地板球體半徑")]
    public float radius = 1f;
    [Header("檢查地板球體位移")]
    public Vector3 offset;
    [Header("跳愈次數限制")]
    public int jumpCountLimit=2;

    private int jumpCount;                                         // 記錄玩家跳躍次數


    /// <summary>
    /// 判斷是否落地
    /// </summary>
    private bool onGround;
    private Animator ani;
    private Rigidbody rig;
    private Transform cam;
    private float x;
    private float y;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1,0,0,0.5f);                       // 射線顏色 50% 紅
        Gizmos.DrawSphere(transform.position + offset, radius);     // 畫球體 ( 中心點 + 位移 ,半徑 )   
                                                                    // 有些角色模組的中心點在身體不在雙腳附近，可從介面調整位移將球體射線移到雙腳
    }

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
    /// 固定更新事件：50FPS
    /// </summary>
    private void FixedUpdate()
    {
        Jump();
    }


    /// <summary>
    /// 移動方法
    /// </summary>
    private void Move()
    {
        float v = Input.GetAxis("Vertical");        // 取得 前後軸 值 W S 上下
        float h = Input.GetAxis("Horizontal");      // 取得 前後軸 值 A D 左右

        

        // 攝影機旋轉角度後前進，玩家也跟著旋轉後的前進方向移動
        Transform camNew = cam;                                               // 新攝影機角度
        camNew.eulerAngles = new Vector3(0, cam.eulerAngles.y, 0);            // 去掉 x 與 z 角度

        // 角色的角度 = 角色 , 攝影機 , 角色的差值
        transform.rotation = Quaternion.Lerp(transform.rotation, camNew.rotation, 0.5f * turnspeed * Time.deltaTime);

        // 加速度 = ( 新攝影機角度 前方 * 前後值 + 右方 * 左右值 ) * 速度 * 1/60 + 上方 * 加速度上下值
        rig.velocity = ((cam.forward * v + cam.right * h) * speed * Time.deltaTime) + transform.up * rig.velocity.y;
        
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

    /// <summary>
    /// 跳躍
    /// </summary>
    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && onGround)    // 按空白鍵且在地上
        {
            jumpCount++;                                    // 跳躍
            rig.Sleep();                                    // 睡著 - 先將鋼體關閉
            rig.WakeUp();                                   // 醒來 - 開啟鋼體
            rig.AddForce(Vector3.up * jump);                // 向上 推力
            ani.SetTrigger("跳躍觸發");
        }

        // 碰撞物件陣列 = 物理 . 球體碰撞範圍 (中心點 , 半徑 , 圖層)
        Collider[] hit = Physics.OverlapSphere(transform.position + offset, radius, 1 << 8);
        if (hit.Length > 0 && hit[0])     
        {
            onGround = true;                                // 如果 碰到物件陣列數量 > 0 且存在 ， 就設定在地面上
            jumpCount = 0;
        }
        else onGround = false;                              // 球體碰到地面，就設定為 不在地上
        ani.SetBool("是否在地面上", onGround);
    }
}
