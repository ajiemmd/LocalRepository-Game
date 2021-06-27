using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD,PATROL,CHASE,DEAD}

[RequireComponent(typeof(NavMeshAgent))]//确保组件一定存在。如果挂载物体没有该组件，则自动添加该组件
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{
    private EnemyStates enemystates;

    private NavMeshAgent agent;

    private Animator anim;

    protected CharacterStats characterstats;

    private Collider coll;

    [Header("Baisc Settings")]
    public float sightRadius;//视野范围
    
    public bool isGuard;

    private float speed;

    protected GameObject attackTarget;

    private Quaternion guardRotation;

    [Tooltip("怪物巡逻一个点发呆时间")]
    public float lookAtTime;//怪物巡逻一个点发呆时间
    private float remainLookAtTime;//发呆时间倒计时

    private float lastAttackTime;

    
    [Header("Patrol State")]
    [Tooltip("巡逻范围")]
    public float patrolRange;//巡逻范围

    private Vector3 wayPoint;//巡逻位置的坐标

    private Vector3 guardPos;//获取初始坐标


    //bool配合动画
    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;

    bool playerDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterstats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();



        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;

    }

    private void Start()
    {
        if (isGuard)
        {
            enemystates = EnemyStates.GUARD;
        }
        else
        {
            enemystates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
        //TODO:场景切换后修改掉
        GameManager.Instance.AddObserver(this);

    }
    //场景切换时启用
    //void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}

    void OnDisable()
    {
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }



    private void Update()
    {
        if (characterstats.CurrentHealth == 0)
            isDead = true;

        if(!playerDead)
        {
            SwtichStates();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
        
    }

    void SwitchAnimation()//将Animator动画中的Bool值与代码中的bool值关联起来
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterstats.isCritical);
        anim.SetBool("Death", isDead);
    }

    void SwtichStates()
    {
        if (isDead)
            enemystates = EnemyStates.DEAD;

        //如果发现Player  切换到Chase
        else if (FoundPlayer())
        {
            enemystates = EnemyStates.CHASE;
        }

        switch(enemystates)
        {
            case EnemyStates.GUARD:
                isChase = false;

                if(transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;

                    if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)//Vector3.SqrMagnitude方法类似Distance方法，不过性能开销更小
                    {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation,0.03f);
                    }
                }
                break;
            case EnemyStates.PATROL:
                isChase = false;
                agent.speed = speed * 0.5f;

                if(Vector3.Distance(wayPoint,transform.position) <= agent.stoppingDistance)//如果巡逻的点距离小于停止距离（此时怪物到了随机巡逻点）
                {
                    isWalk = false;//关闭Walk动画
                    if(remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;//此时怪物发呆时间开始倒计时
                    }
                    else
                        GetNewWayPoint();
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }

                break;
            case EnemyStates.CHASE:
                isWalk = false;
                isChase = true;


                agent.speed = speed;
                if (!FoundPlayer())//如果玩家脱离视野
                {
                    isFollow = false;//取消追击玩家
                    if (remainLookAtTime > 0)//如果发呆倒计时大于0
                    {
                        agent.destination = transform.position;//一旦玩家脱离怪物视野，怪物就立刻停下（以当前自身位置为目的地）
                        remainLookAtTime -= Time.deltaTime;//此时怪物发呆时间开始倒计时
                    }

                    else if (isGuard)
                    {
                        enemystates = EnemyStates.GUARD;
                    }
                    else
                        enemystates = EnemyStates.PATROL;

                }
                else//追击玩家
                {
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;//怪物追击玩家
                }
                //在攻击范围内则攻击
                if(TargetInAttackRange() || TargetInSkillRange())//在攻击或技能范围内
                {
                    isFollow = false;
                    agent.isStopped = true;

                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterstats.attackData.coolDown;

                        //暴击判断
                        characterstats.isCritical = Random.value < characterstats.attackData.criticalChance;//Random.value随机返回一个0到1之间的小数
                        //执行攻击
                        Attack();
                    }
                    
                }

                break;
            case EnemyStates.DEAD:
                coll.enabled = false;
                //agent.enabled = false;//agent关闭会导致动画StopAgent中agent丢失
                agent.radius = 0;
                Destroy(gameObject,2f);
                break;
        }
    }

    void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if (TargetInAttackRange())
        {
            //近战攻击动画
            anim.SetTrigger("Attack");
        }
        if (TargetInSkillRange())
        {
            //技能攻击动画
            anim.SetTrigger("Skill");
        }
    }
    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);//Physics.OverlapSphere方法，用于敌人周围一个圆形区域的碰撞体检测
        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;//如果敌人找到Player，敌人攻击目标就是Player
                return true;
            }
        }
        attackTarget = null;//如果敌人没找到，则改为null
        
        return false;
    }

    bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterstats.attackData.attackRange;
        else
            return false;
    }

    bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterstats.attackData.skillRange;
        else
            return false;
    }



    void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;//复原=复原怪物发呆倒计时

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;

    }

    private void OnDrawGizmosSelected()//画出选中物体范围
    {
        Gizmos.color = Color.blue;//蓝色
        Gizmos.DrawWireSphere(transform.position, sightRadius);//DrawWireSphere方法：画出空心球体，
    }
    
    //Animation Event 

    void Hit()
    {
        if(attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterstats, targetStats);

        }
    }

    public void EndNotify()
    {
        //获胜动画
        //停止所有移动
        //停止Agent
        anim.SetBool("Win", true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}
