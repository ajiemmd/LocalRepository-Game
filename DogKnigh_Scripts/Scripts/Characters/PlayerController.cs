using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;//ʹ��NavMeshAgent��

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

    private void OnEnable()//��������ʱ
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;//�¼�������ʱ������MoveToTarget����
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
            MouseManager.Instance.OnMouseClicked -= MoveToTarget;//������ȥ���ⳡ��ʱ��ȡ�����ĸ��¼�
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

    private void SwitchAnimation()//�л�����
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);//��sqrMagnitude��vector3���ͷ��ظ���������
        anim.SetBool("Death", isDead);
    }

    public void MoveToTarget(Vector3 target)//�ƶ���Ŀ�괦
    {
        StopAllCoroutines();//���  �ڵ�����˺󵼺������˵�·���޷�ȡ��  ��һ����
        if (isDead) return;

        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;//����ֹͣ���������д���ᵼ����ҹ���֮���޷�ȥ��һ��Ŀ�ĵء�
        agent.destination = target;
    }

    private void EventAttack(GameObject target)
    {
        if (isDead) return;

        if (target != null)
        {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());//ִ��Э��
        }

    }

    
    IEnumerator MoveToAttackTarget()//Э��
    {
        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;
        
        transform.LookAt(attackTarget.transform);//������ת�򹥻�Ŀ��


        while (Vector3.Distance(attackTarget.transform.position,transform.position)> characterStats.attackData.attackRange)//�жϹ�������
        {
            agent.destination = attackTarget.transform.position;//�������ѹ���Ŀ���λ������ΪĿ�ĵ�
            yield return null;//ʹ����һ֡��ִ�д�ѭ��
        }

        agent.isStopped = true;
        
        //Attack
        if(lastAttackTime < 0)
        {
            anim.SetBool("Critical",characterStats.isCritical);

            anim.SetTrigger("Attack");//���ù�������
            //������ȴʱ��
            lastAttackTime = characterStats.attackData.coolDown;
        }    

    }

    //Animation Event

    void Hit()
    {
            if (attackTarget.CompareTag("Attackable"))//����������ǿɹ������壨ʯͷ�ȿɻ����ģ�
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
 