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
        Gizmos.DrawSphere(transform.position + attackOffset, attackRadius);
    }

    private void Update()
    {
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
        }

        nma.isStopped = true;
        ani.SetBool("走路開關", false);

    }   
        
    private IEnumerator DelayAttaack()
        {
            yield return new WaitForSeconds(attackDelay);
            // 碰撞陣列 = 物理.覆蓋球體(位置 + 位移，半徑，只碰撞到圖層9(玩家))
            Collider[] hits = Physics.OverlapSphere(transform.position + attackOffset, attackRadius, 1 << 9);

            // 如果打到超過1個東西 => 打到.抓取玩家腳本的傷害(攻擊值)
            if (hits.Length > 1)
            {

                hits[0].GetComponent<Player>().Damage(attack);
            }


        }
    }

