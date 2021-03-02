using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    #region 基本欄位
    [Header("追蹤玩家範圍"), Range(0, 100)]
    public float rangeTrack = 5;
    [Header("移動速度")]
    public float speed = 3;
    [Header("攻擊玩家範圍"), Range(0, 100)]
    public float rangeAttack = 3;
    [Header("攻擊冷卻時間"), Range(0, 100)]
    public float attackCD = 2.5f;
    [Header("攻擊球體半徑"), Range(0, 100)]
    public float attackRadius = 1f;
    [Header("攻擊球體位移")]
    public Vector3 attackOffset;
    [Header("攻擊延遲對玩家造成的傷害")]
    public float attackDelay;
    [Header("攻擊力"),Range(0,100)]
    public float attack=30;
    [Header("血量)"), Range(0, 10000)]
    public float hp = 3000;

    private Animator ani;
    private Transform player;
    private NavMeshAgent nma;
    private float timer;
    #endregion

    private void Awake()
    {
        ani = GetComponent<Animator>();
        nma = GetComponent<NavMeshAgent>();
        nma.stoppingDistance = rangeAttack;
        player = GameObject.Find("玩家").transform;

    }

    private void OnDrawGizmos()
    {
        // 怪物 偵測到玩家 並 追蹤 的範圍(紅色)
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, rangeTrack);
        // 怪物停止走路，攻擊 的範圍(藍色)
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, rangeAttack);
        // 攻擊球體範圍 (位置 + 位移，半徑)
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawSphere(transform.position + transform.forward * attackOffset.z + transform.right * attackOffset.x, attackRadius);
    }

    private void Update()
    {   // 如果 正在攻擊中就跳出
        // 如果 取得目前動畫控制器的狀態(0=>沒設置圖層=>Base Layer).攻擊 => 跳出 不 Track()
        if (ani.GetCurrentAnimatorStateInfo(0).IsName("攻擊")) return;
        Track();
    }

    /// <summary>
    /// 追蹤
    /// </summary>
    private void Track()
    {
        // 距離 = 三維向量向量.距離(位置a，位置b)
        float dis = Vector3.Distance(player.position, transform.position);
        if (dis < rangeAttack)
        {
            Attack();
        }
        else if (dis < rangeTrack)
        {
            // 代理器.設定目的地(玩家.座標)
            nma.SetDestination(player.position);
            // 大寫 後面 + ( )，小寫 + =
            nma.isStopped = false;
            ani.SetBool("走路開關", true);
        }
        else
        {
            nma.isStopped = true;
            ani.SetBool("走路開關", false);
        }
    }

    private void Attack()
    {
        if (timer >= attackCD)                   // 如果 計時器 > 攻擊冷卻時間  => 計時器歸0，觸發攻擊，再歸0，攻擊 => loop
        {
            timer = 0;
            ani.SetTrigger("攻擊觸發");
            StartCoroutine(DelayAttaack());      // 延遲對玩家造成的傷害
        }

        else
        {
            timer = timer + Time.deltaTime;      // 累加時間
            Vector3 pos = player.position;       // 座標 = 取得玩家座標
            pos.y = transform.position.y;        // 玩家 y軸 改為 怪物y軸 
            transform.LookAt(pos);               // 面向 (修改後的座標)     ，LookAt 面向API
        }

        nma.isStopped = true;
        ani.SetBool("走路開關", false);

    }   
        
    private IEnumerator DelayAttaack()
        {
            yield return new WaitForSeconds(attackDelay);
            // 碰撞陣列 = 物理.覆蓋球體(位置 + 位移，半徑，只碰撞到圖層9(玩家))
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * attackOffset.z + transform.right * attackOffset.x, attackRadius, 1 << 9);

            // 如果打到超過1個東西 => 打到.抓取玩家腳本的傷害(攻擊值)
            if (hits.Length > 1)
            {

                hits[0].GetComponent<Player>().Damage(attack);
            }


        }

    /// <summary>
    /// 受傷
    /// </summary>
    /// <param name="getDamege"></param>
    public void Damage(float getDamege)
    {
        ani.SetTrigger("受傷觸發");
        hp = -getDamege;
        if (hp <= 0) Dead();
    }


    /// <summary>
    /// 死亡
    /// </summary>
    private void Dead()
    {
        hp = 0;
        ani.SetBool("死亡開關", true);
        enabled = false;
    }
}

