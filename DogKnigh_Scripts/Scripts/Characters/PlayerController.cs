using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;//使用NavMeshAgent类

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;

    private Animator anim;

    private CharacterStats characterStats;

    private GameObject attackTarget;

    private float lastAttackTime;

    private bool isDead;

    private float stopDistance;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();

        stopDistance = agent.stoppingDistance;
    }

    private void OnEnable()//人物启用时
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;//事件被触发时，调用MoveToTarget方法
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RegisterPlayer(characterStats);
    }
    private void Start()
    {
        SavaManager.Instance.LoadPlayerData();
    }

    private void OnDisable()
    {
        if (!MouseManager.IsInitialized) return;
        {
            MouseManager.Instance.OnMouseClicked -= MoveToTarget;//当人物去另外场景时，取消订阅该事件
            MouseManager.Instance.OnEnemyClicked -= EventAttack;
        }
    }

    private void Update()
    {
        isDead = characterStats.CurrentHealth == 0;

        if (isDead)
            GameManager.Instance.NotifyObservers();

        SwitchAnimation();

        lastAttackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()//切换动画
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);//用sqrMagnitude将vector3类型返回浮点数类型
        anim.SetBool("Death", isDead);
    }

    public void MoveToTarget(Vector3 target)//移动到目标处
    {
        StopAllCoroutines();//解决  在点击敌人后导航至敌人的路上无法取消  这一问题
        if (isDead) return;

        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;//不再停止，不加这行代码会导致玩家攻击之后无法去下一个目的地。
        agent.destination = target;
    }

    private void EventAttack(GameObject target)
    {
        if (isDead) return;

        if (target != null)
        {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());//执行协程
        }

    }

    
    IEnumerator MoveToAttackTarget()//协程
    {
        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;
        
        transform.LookAt(attackTarget.transform);//将人物转向攻击目标


        while (Vector3.Distance(attackTarget.transform.position,transform.position)> characterStats.attackData.attackRange)//判断攻击距离
        {
            agent.destination = attackTarget.transform.position;//导航，把攻击目标的位置设置为目的地
            yield return null;//使其下一帧再执行此循环
        }

        agent.isStopped = true;
        
        //Attack
        if(lastAttackTime < 0)
        {
            anim.SetBool("Critical",characterStats.isCritical);

            anim.SetTrigger("Attack");//设置攻击动画
            //重置冷却时间
            lastAttackTime = characterStats.attackData.coolDown;
        }    

    }

    //Animation Event

    void Hit()
    {
            if (attackTarget.CompareTag("Attackable"))//如果攻击的是可攻击物体（石头等可互动的）
            {
                if (attackTarget.GetComponent<Golem_Rock>() && attackTarget.GetComponent<Golem_Rock>().rockStates == Golem_Rock.RockStates.HitNothing)
                {
                    attackTarget.GetComponent<Golem_Rock>().rockStates = Golem_Rock.RockStates.HitEnemy;
                    attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                    attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
                }
            }
            else 
            {
                var targetStats = attackTarget.GetComponent<CharacterStats>();
                targetStats.TakeDamage(characterStats, targetStats);
            }

        

    }

}
 