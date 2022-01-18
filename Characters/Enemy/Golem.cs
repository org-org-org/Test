using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 重载 EnemyController 在此基础上修改
public class Golem : EnemyController
{
    [Header("Skill")]
    // 击飞力量的基础数值
    public float kickForce = 25;
    // 声明石头对象
    public GameObject rockPrefab;
    // 声明石头人投掷时手的坐标
    public Transform handPos;

    // 近战攻击动画事件
    public void KickOff() {
        if(attackTarget != null && transform.IsFacingTarget(attackTarget.transform)) {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            // 攻击方向向量化 direction.Normalize();
            Vector3 direction = (attackTarget.transform.position - transform.position).normalized;

            targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
            targetStats.GetComponent<Animator>().SetTrigger("Dizzy");
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    // 远程攻击动画事件
    public void ThrowRock() {
        // 攻击目标存在时执行
        if(attackTarget) {
            // 生成石头对象 设置生成位置 初始旋转为自身 维持原来的状态
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);

            // 设置攻击目标
            rock.GetComponent<Rock>().target = attackTarget;
        }
    }
}
