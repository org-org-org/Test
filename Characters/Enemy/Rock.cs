using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock: MonoBehaviour {

    // 声明枚举变量 设置石头不同状态
    public enum RockStates { HitPlayer, HitEnemy, HitNothing}
    // 保存不同的状态 public 让其他类可以访问
    public RockStates rockStates;

    // 声明刚体变量
    private Rigidbody rb;

    [Header("Basic Settings")]
    // 设置力的基础数据
    public float force;
    // 设置石头伤害
    public int damage;
    // 声明攻击目标
    public GameObject target;
    // 声明攻击方向
    private Vector3 direction;
    // 声明石头破坏后效果
    public GameObject breakEffect;

    private void Start() {
        // 生成时进行赋值
        rb = GetComponent<Rigidbody>();

        // 设置生成时的初始速度 防止开始速度为 0
        // 导致后面的方法判断速度小于 1 时 在开始时变为 HitNothing 状态
        rb.velocity = Vector3.one;

        // 初始状态设置攻击player
        rockStates = RockStates.HitPlayer;

        FlyToTarget();
    }

    // 刚体特殊的物理更新方法
    void FixedUpdate() {
        // 当石头速度小于 1 时切换为 HitNothing 状态
        if(rb.velocity.sqrMagnitude < 1f) {
            rockStates = RockStates.HitNothing;
        }
    }


    public void FlyToTarget() {

        // 如果进入远程动画 但脱离攻击范围 主动找到player并攻击
        // 写是写了 但似乎没有用
        if(target == null)
            target = FindObjectOfType<PlayerController>().gameObject;

        // 加上一点向上的方向 防止角度过于向下 使得飞行曲线有一定弧度
        direction = (target.transform.position - transform.position + Vector3.up).normalized;

        // 第二个参数 选择力的类型 冲击力
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    // 判断发生碰撞时对象 切换不同状态
    void OnCollisionEnter(Collision other) {
        switch(rockStates) {
            case RockStates.HitPlayer:
                if(other.gameObject.CompareTag("Player")){
                    // 获取玩家对象 弹开并眩晕
                    other.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    other.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;
                    other.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    other.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, other.gameObject.GetComponent<CharacterStats>());
                    
                    // 攻击完后不再造成伤害 改为Nothing
                    rockStates = RockStates.HitNothing;
                }
                break;

            case RockStates.HitEnemy:
                // 获取对象函数内置是否是对象 可以直接获取对象判断目标
                if(other.gameObject.GetComponent<Golem>()) {
                    // 保存敌人临时对象
                    var otherStats = other.gameObject.GetComponent<CharacterStats>();

                    // 伤害作用于敌人身上
                    otherStats.TakeDamage(damage, otherStats);

                    // 作用于敌人后生成破坏效果
                    Instantiate(breakEffect, transform.position, Quaternion.identity);

                    // 作用后销毁
                    Destroy(gameObject);
                }
                break;
        }
    }
}
