using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD,PATROL,CHASE,DEAD}

[RequireComponent(typeof(NavMeshAgent))]//ȷ�����һ�����ڡ������������û�и���������Զ���Ӹ����
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{
    private EnemyStates enemystates;

    private NavMeshAgent agent;

    private Animator anim;

    protected CharacterStats characterstats;

    private Collider coll;

    [Header("Baisc Settings")]
    public float sightRadius;//��Ұ��Χ
    
    public bool isGuard;

    private float speed;

    protected GameObject attackTarget;

    private Quaternion guardRotation;

    [Tooltip("����Ѳ��һ���㷢��ʱ��")]
    public float lookAtTime;//����Ѳ��һ���㷢��ʱ��
    private float remainLookAtTime;//����ʱ�䵹��ʱ

    private float lastAttackTime;

    
    [Header("Patrol State")]
    [Tooltip("Ѳ�߷�Χ")]
    public float patrolRange;//Ѳ�߷�Χ

    private Vector3 wayPoint;//Ѳ��λ�õ�����

    private Vector3 guardPos;//��ȡ��ʼ����


    //bool��϶���
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
        //TODO:�����л����޸ĵ�
        GameManager.Instance.AddObserver(this);

    }
    //�����л�ʱ����
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

    void SwitchAnimation()//��Animator�����е�Boolֵ������е�boolֵ��������
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

        //�������Player  �л���Chase
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

                    if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)//Vector3.SqrMagnitude��������Distance�������������ܿ�����С
                    {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation,0.03f);
                    }
                }
                break;
            case EnemyStates.PATROL:
                isChase = false;
                agent.speed = speed * 0.5f;

                if(Vector3.Distance(wayPoint,transform.position) <= agent.stoppingDistance)//���Ѳ�ߵĵ����С��ֹͣ���루��ʱ���ﵽ�����Ѳ�ߵ㣩
                {
                    isWalk = false;//�ر�Walk����
                    if(remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;//��ʱ���﷢��ʱ�俪ʼ����ʱ
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
                if (!FoundPlayer())//������������Ұ
                {
                    isFollow = false;//ȡ��׷�����
                    if (remainLookAtTime > 0)//�����������ʱ����0
                    {
                        agent.destination = transform.position;//һ��������������Ұ�����������ͣ�£��Ե�ǰ����λ��ΪĿ�ĵأ�
                        remainLookAtTime -= Time.deltaTime;//��ʱ���﷢��ʱ�俪ʼ����ʱ
                    }

                    else if (isGuard)
                    {
                        enemystates = EnemyStates.GUARD;
                    }
                    else
                        enemystates = EnemyStates.PATROL;

                }
                else//׷�����
                {
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;//����׷�����
                }
                //�ڹ�����Χ���򹥻�
                if(TargetInAttackRange() || TargetInSkillRange())//�ڹ������ܷ�Χ��
                {
                    isFollow = false;
                    agent.isStopped = true;

                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterstats.attackData.coolDown;

                        //�����ж�
                        characterstats.isCritical = Random.value < characterstats.attackData.criticalChance;//Random.value�������һ��0��1֮���С��
                        //ִ�й���
                        Attack();
                    }
                    
                }

                break;
            case EnemyStates.DEAD:
                coll.enabled = false;
                //agent.enabled = false;//agent�رջᵼ�¶���StopAgent��agent��ʧ
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
            //��ս��������
            anim.SetTrigger("Attack");
        }
        if (TargetInSkillRange())
        {
            //���ܹ�������
            anim.SetTrigger("Skill");
        }
    }
    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);//Physics.OverlapSphere���������ڵ�����Χһ��Բ���������ײ����
        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;//��������ҵ�Player�����˹���Ŀ�����Player
                return true;
            }
        }
        attackTarget = null;//�������û�ҵ������Ϊnull
        
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
        remainLookAtTime = lookAtTime;//��ԭ=��ԭ���﷢������ʱ

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;

    }

    private void OnDrawGizmosSelected()//����ѡ�����巶Χ
    {
        Gizmos.color = Color.blue;//��ɫ
        Gizmos.DrawWireSphere(transform.position, sightRadius);//DrawWireSphere�����������������壬
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
        //��ʤ����
        //ֹͣ�����ƶ�
        //ֹͣAgent
        anim.SetBool("Win", true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}
