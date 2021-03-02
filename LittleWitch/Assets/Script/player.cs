
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class Player : MonoBehaviour
{
    #region 基本數值
    [Header("跑步移動速度"), Range(0, 1000)]
    public float speed = 10;
    [Header("走路移動速度"), Range(0, 1000)]
    public float speedWalk = 10;
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
    public int jumpCountLimit = 2;
   

    private int jumpCount;                                         // 記錄玩家跳躍次數


    [Header("血量"), Range(0, 5000)]
    public float hp = 100;
    private float hpMax;
    [Header("魔力"), Range(0, 5000)]
    public float mp = 100;
    private float mpMax;
    [Header("體力"), Range(0, 5000)]
    public float sp = 100;
    private float spMax;

    [Header("吧條")]
    public Image barHp;
    public Image barMp;
    public Image barSp;

    [Header("移動時每秒扣除體力"), Range(0, 5000)]
    public float spMove=1;
    [Header("跳躍時每秒扣除體力"), Range(0, 5000)]
    public float spJump=5;
    [Header("停止時每秒恢復體力"), Range(0, 5000)]
    public float spRecover=10;
    


    /// <summary>
    /// 判斷是否落地
    /// </summary>
    private bool onGround;
    private Animator ani;
    private Rigidbody rig;
    private Transform cam;
    private float x;
    private float y;
    #endregion

    #region 攻擊參數
    [Header("生成攻擊特效位置")]
    public Transform attackPoint;
    [Header("攻擊特效")]
    public GameObject attackPS;
    [Header("攻擊特效速度")]
    public float attackSpeed = 500;
    [Header("攻擊力")]
    public float attack = 50;
    [Header("攻擊魔力消耗")]
    public float attackCost = 20;
    [Header("生成攻擊特效延遲")]
    public float attackPSDeplay = 0.15f;
    [Header("生成多久後可進行下次攻擊")]
    public float attackDeplay = 2f;

    /// <summary>
    /// 是否攻擊中
    /// </summary>
    private bool attacking;
    #endregion

    #region 事件
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1,0,0,0.5f);                       // 射線顏色 50% 紅
        Gizmos.DrawSphere(transform.position + offset, radius);     // 畫球體 ( 中心點 + 位移 ,半徑 )   
                                                                    // 有些角色模組的中心點在身體不在雙腳附近，可從介面調整位移將球體射線移到雙腳
    }

    /// <summary>
    /// 結束畫面
    /// </summary>
    private CanvasGroup final;

    // 喚醒事件：在 Start 前執行一次
    private void Awake()
    {
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        cam = GameObject.Find("攝影機根物件").transform;
        final= GameObject.Find("結束畫面").GetComponent<CanvasGroup>();
        hpMax = hp;
        mpMax = mp;
        spMax = sp;
    }

    private void Update()
    {
        if (attacking) return;

        Move();                      //
        Turncamera();
        Jump();                     //
        spSystem();
        Attack();

        //扣血測試
        if (Input.GetKeyDown(KeyCode.Alpha1)) Cure(-10) ;

    }

    /// <summary>
    /// 固定更新事件：50FPS
    /// </summary>
    private void FixedUpdate()
    {
                               
    }

    #endregion

    #region 方法

    private void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && mp > attackCost && !attacking)                 // 按下左鍵 且 mp > 魔力消耗 且 不在攻擊中
        {
            StartCoroutine(AttackTimeControl());
        }
    }

    /// <summary>
    /// 攻擊時間控制，協成
    /// </summary>
    /// <returns></returns>
    private IEnumerator AttackTimeControl()
    {
        rig.velocity = Vector3.zero;                                                            // 鋼體向量位置歸0，玩家才不會邊滑步邊攻擊
        attacking = true;                                                                       // 正在攻擊中
        mp -= attackCost;                                                                        // 扣mp
        barMp.fillAmount = mp / mpMax;                                                         // 更新mp介面

        ani.SetTrigger("攻擊觸發");
        yield return new WaitForSeconds(attackPSDeplay);                                        // 延遲攻擊特效

        GameObject temp = Instantiate(attackPS, attackPoint.position, attackPoint.rotation);    // 生成攻擊特效在位置上
        temp.GetComponent<Rigidbody>().AddForce(transform.forward * attackSpeed);               // 取得攻擊特效並添加推力
        temp.GetComponent<Magic>().attack = attack;

        yield return new WaitForSeconds(attackDeplay);                                          // 延遲再次攻擊
        attacking = false;                                                                      // 沒在攻擊
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


        if (sp > 0)
        {
            // 加速度 = ( 新攝影機角度 前方 * 前後值 + 右方 * 左右值 ) * 速度 * 1/60 + 上方 * 加速度上下值
            rig.velocity = ((cam.forward * v + cam.right * h) * speed * Time.deltaTime) + transform.up * rig.velocity.y;

            // 動畫.設定布林值 ( "參數名稱" , 鋼體.加速度.值 > 0 )
            ani.SetBool("跑步開關", rig.velocity.magnitude > 0);
        }

        else
        {
            rig.velocity = ((cam.forward * v + cam.right * h) * speedWalk * Time.deltaTime) + transform.up * rig.velocity.y;
            ani.SetBool("走路開關", rig.velocity.magnitude > 0);
            ani.SetBool("跑步開關", false);
        }

        
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

        // 碰撞物件陣列 = 物理 . 球體碰撞範圍 (中心點 , 半徑 , 圖層)
        Collider[] hit = Physics.OverlapSphere(transform.position + offset, radius, 1 << 8);
        if (hit.Length > 0 && hit[0])
        {
            onGround = true;                                // 如果 碰到物件陣列數量 > 0 且存在 ， 就設定在地面上
            jumpCount = 0;
        }
        else onGround = false;                              // 球體碰到地面，就設定為 不在地上
        ani.SetBool("是否在地面上", onGround);

        if (sp < spJump) return;                            //如果體力 < 跳躍需要體力 ， 就跳出

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < jumpCountLimit - 1)    // 按空白鍵且在地上
        {
            jumpCount++;                                    // 跳躍
            rig.Sleep();                                    // 睡著 - 先將鋼體關閉
            rig.WakeUp();                                   // 醒來 - 開啟鋼體
            rig.AddForce(Vector3.up * jump);                // 向上 推力
            ani.SetTrigger("跳躍觸發");

            //跳躍扣除體力
            sp -= spJump;
            barSp.fillAmount = sp / spMax;
        }

        
    }

    /// <summary>
    /// 體力系統
    /// </summary>
    private void spSystem()
    {
        if (ani.GetBool("跑步開關"))
        {
            sp -= spMove * Time.deltaTime;
            barSp.fillAmount = sp / spMax;
        }
        else if (!ani.GetBool("走路開關"))
        {
            sp += spRecover * Time.deltaTime;
            barSp.fillAmount = sp / spMax;
        }

        sp = Mathf.Clamp(sp, 0, spMax);


    }

    /// <summary>
    /// 治癒
    /// </summary>
    /// <param name="curevalue">要治癒的值</param>
    public void Cure(float curevalue)
    {
        hp += curevalue;                        // 補血
        hp = Mathf.Clamp(hp, 0, hpMax);         // 夾住 血量 (血量 , 0 , hpMax)
        barHp.fillAmount = hp / hpMax;          // 更新血條
    }

    /// <summary>
    /// 受傷
    /// </summary>
    /// <param name="getDamege"></param>
    public void Damage(float getDamege)
    {
        ani.SetTrigger("受傷觸發");
        hp = -getDamege;
        barHp.fillAmount = hp / hpMax;
        if (hp <= 0) Dead();
    }


    /// <summary>
    /// 死亡
    /// </summary>
    private void Dead()
    {
        hp = 0;
        ani.SetBool("死亡開關",true);
        enabled = false;
        StartCoroutine(ShowFinal());
    }

    /// <summary>
    /// 結束畫面
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowFinal()
    {
        final.interactable = true;          // 將 interactable 打勾，可以互動
        final.blocksRaycasts = true;        // 將 blocksRaycasts 打勾，開啟遮擋，滑鼠才能點到
        float a = final.alpha;              // 取得 透明度

        // while (布林值) {程式區域}
        while (a<1)
        {
            a += 0.1f;
            final.alpha = a;
            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion
}
