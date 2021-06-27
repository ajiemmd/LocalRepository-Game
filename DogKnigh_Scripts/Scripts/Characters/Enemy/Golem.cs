using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyController
{
    [Header("Skill")]
    public float kickFroce = 25;//推的力度

    public GameObject rockPrefab;
    public Transform handPos;

    //Animation Event
    public void KickOff()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            Vector3 direction = (attackTarget.transform.position - transform.position).normalized;
            //direction.Normalize();

            targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().velocity = direction * kickFroce;
            
            targetStats.GetComponent<Animator>().SetTrigger("Dizzy");//眩晕
            targetStats.TakeDamage(characterstats, targetStats);

        }
    }

    //Animation Event
    public void ThrowRock()
    {
        if(attackTarget != null)
        {
            var rock = Instantiate(rockPrefab,handPos.position,Quaternion.identity);
            rock.GetComponent<Golem_Rock>().target = attackTarget;
        }
    }
}
