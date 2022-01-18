using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController: MonoBehaviour {
    private NavMeshAgent agent;
    private Animator anim;
    // 创建角色的数据面板
    private CharacterStats characterStats;
    // 攻击的目标
    private GameObject attackTarget;
    // 攻击的冷却时间
    private float lastAttackTime;
    // 死亡状态
    private bool isDead;
    // 攻击停止的距离
    private float stopDistance;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        stopDistance = agent.stoppingDistance;
    }

    void Start() {
        // 直接使用单例模式 注册 类型为角色状态的数据
        GameManager.Instance.RigisterPlayer(characterStats);
    }

    private void OnEnable() {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
    }

    // 当人物切换到另一个场景后 取消上面 start 方法的订阅
    private void OnDisable() {
        if (!MouseManager.IsInitialized) return;
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EventAttack;
    }

    void Update() {
        // 判断血量赋值死亡状态
        isDead = characterStats.CurrentHealth == 0;

        // 判断角色死亡状态 进行广播
        if(isDead)
            GameManager.Instance.NotifyObservers();

        SwitchAnimation();

        // 更新状态的时候衰减时间
        lastAttackTime -= Time.deltaTime;
    }

    // 切换动画
    private void SwitchAnimation() {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }

    public void MoveToTarget(Vector3 target) {
        // 终止所有协程以大打断攻击 并恢复移动状态
        StopAllCoroutines();

        // 死亡不用更新状态 直接返回
        if(isDead) return;

        // 移动时 将攻击设置的由武器决定的停止距离 改回来
        agent.stoppingDistance = stopDistance;

        agent.isStopped = false;
        agent.destination = target;
    }

    // 对变量按F2集体修改变量名
    public void EventAttack(GameObject target) {

        if(isDead) return;

        // 确定攻击目标存在
        if(target != null) {
            attackTarget = target;

            // 计算是否暴击 ( 随机的暴击 比上 角色设定的暴击
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;

            // 攻击对象（协程
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget() {

        // true 停下来 false 仍然移动 确定一开始仍在移动
        agent.isStopped = false;

        // 攻击时 角色移动到目标停止的距离 变成武器的距离
        agent.stoppingDistance = characterStats.attackData.attackRange;

        // 目标与player太远 先转向后移动
        transform.LookAt(attackTarget.transform);

        // 判断两点之间的距离 目标位置, player位置 > player攻击距离 时说明太远需要移动到目标
        while(Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange) {
            // 终点为攻击目标的位置
            agent.destination = attackTarget.transform.position;

            // yield 令下一帧从当前位置开始执行
            yield return null;
        }

        // 循环结束 走到了目标 停下来
        agent.isStopped = true;

        // Attack 攻击冷却结束
        if(lastAttackTime < 0) {

            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");

            // 重置冷却时间
            lastAttackTime = characterStats.attackData.coolDown;

        }

    }

    void Hit() {
        // 如果对象为可攻击物体
        if(attackTarget.CompareTag("Attackable")) {

            // 判断是不是石头 且石头落地了
            if(attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().rockStates == Rock.RockStates.HitNothing) {
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;

                // 设置石头速度为 1 反击击中石头人后消失
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                
                // 方向为 player 正面 
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
            }
        }
        // 对象为敌人
        else {
            // 获得身上的状态
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            // 造成伤害为 攻击 - 防御
            targetStats.TakeDamage(characterStats, targetStats);
        }

    }

}
