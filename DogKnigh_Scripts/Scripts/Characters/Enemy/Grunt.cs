using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Grunt : EnemyController
{
    [Header("Skill")]
    public float kickFroce = 15;//推的力度

    public void KickOff()
    {
        if (attackTarget != null)
        {
            transform.LookAt(attackTarget.transform);
        }

        Vector3 direction = attackTarget.transform.position - transform.position;
        direction.Normalize();

        attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
        attackTarget.GetComponent<NavMeshAgent>().velocity = direction * kickFroce;
        attackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
    }
}
