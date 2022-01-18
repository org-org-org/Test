using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 重载 EnemyController 在此基础上修改
public class Grunt : EnemyController
{
    [Header("Skill")]
    // 击飞力量的基础数值
    public float kickForce = 10;

    public void KickOff() {
        if(attackTarget) {
            // 让敌人看向角色
            transform.LookAt(attackTarget.transform);

            // 获得击飞方向 
            Vector3 direction = attackTarget.transform.position -transform.position;
            // 获得量化的 0 or 1
            direction.Normalize();

            // 打断玩家可能进行的任何移动 再进行击飞
            attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
            // 获得速度变量 给予 反向速度 = 方向 * 击飞力
            attackTarget.GetComponent<NavMeshAgent>().velocity = direction * kickForce;

            attackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
        }
    }

}
