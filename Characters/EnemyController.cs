using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 四种不同状态的枚举
public enum EnemyStates { GUARD, PATROL, CHASE, DEAD }

// 当生成对象没有会自动生成 NavMeshAgent 和 CharacterStats
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]

public class EnemyController: MonoBehaviour, IEndGameObserver {
    private EnemyStates enemyStates;
    private NavMeshAgent agent;
    private Animator anim;
    // 让子类可以访问 
    protected CharacterStats characterStats;
    // 声明敌人碰撞体
    private Collider coll;

    [Header("Basic Settings")]

    // 定义敌人的可视范围（追击范围
    public float sightRadius;
    // 判断是否是巡逻的对象
    public bool isGuard;
    // 定义巡逻后的停顿观察时间 模拟真实巡逻状态而不是一直走动
    public float lookAtTime;
    private float remainLookAtTime;
    // 攻击状态 冷却时间
    private float lastAttackTime;
    // protected 修饰 子类继承可以访问
    protected GameObject attackTarget;
    // 保存切换攻击状态后的速度
    private float attackSpeed;

    // 保存巡逻前的旋转角度
    private Quaternion guardRotation;


    // 定义巡逻状态
    [Header("Patrol State")]
    public float patrolRange;
    // 巡逻路上的随机点
    private Vector3 wayPoint;
    // 定义初始位置的坐标 防止随机巡逻时超出范围 
    // 或切换状态后返回原始位置
    private Vector3 guardPos;

    // bool 配合动画
    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;
    bool playerDead;

    // 变量在play后 执行Awake初始化 获得各个组件的状态
    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();

        attackSpeed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
    }

    //
    void Start() {
        if(isGuard) {
            enemyStates = EnemyStates.GUARD;
        }
        else {
            enemyStates = EnemyStates.PATROL;
            // 初始化时 赋值需要巡逻的点
            GetNewWayPoint();
        }

        // 后面修改
        GameManager.Instance.AddObserver(this);
    }

    // 切换场景时 启用和禁用观察者 添加和移除到观察者列表
    // void OnEnable() {
    //     GameManager.Instance.AddObserver(this);
    // }

    // 人物消失时会进行一次调用
    void OnDisable() {
        // 避免重复调用 判断生成才进行移除
        if(!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }

    // 通过Update调用SwitchStates不断更新当前对象状态
    void Update() {
        // 检测是否死亡 血量为空时
        if(characterStats.CurrentHealth == 0)
            isDead = true;

        // 通过玩家死亡的状态变量 判断是否更新播放的动画
        if(!playerDead) {
            SwitchStates();
            SwitchAnimation();
            // 攻击冷却时间
            lastAttackTime -= Time.deltaTime;
        }

    }

    void SwitchAnimation() {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Death", isDead);
    }

    // 切换不同的状态
    void SwitchStates() {

        // 检测到死亡 切换状态
        if(isDead) enemyStates = EnemyStates.DEAD;
        // 非死亡状态下 如果发现player 切换CHASE
        else if(FoundPlayer()) enemyStates = EnemyStates.CHASE;


        switch(enemyStates) {
            case EnemyStates.GUARD:
                // 回到巡逻状态 不再追击 则更新追击状态为 False
                isChase = false;

                // 回到巡逻的原位
                if(transform.position != guardPos) {
                    // 切换行走状态 设置目标终点
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;

                    // SqrMagnitude 计算3维目标点的距离 到达则停止行走状态 （类似下面的 Distance 但更节省性能
                    if(Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance) {
                        isWalk = false;

                        // 重置巡逻前的角度 
                        // 为不导致瞬间重置的僵硬转换 使用方法匀速变换
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.01f);
                    }
                }
                break;

            case EnemyStates.PATROL:
                isChase = false;
                // 乘法比除法开销更小 因此更建议使用乘法
                // 切换为更慢的巡逻移动速度
                agent.speed = attackSpeed * 0.5f;

                // 如果随机得到的巡逻点与当前位置小于攻击距离 == 走到了巡逻点
                if(Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance) {
                    isWalk = false;

                    // 如果还有观察时间则递减
                    if(remainLookAtTime > 0)
                        remainLookAtTime -= Time.deltaTime;
                    // 否则获得新更新的点
                    else
                        GetNewWayPoint();
                }
                // 没有达到随机得到的巡逻点
                else {
                    isWalk = true;
                    agent.destination = wayPoint;
                }
                break;

            case EnemyStates.CHASE:

                // 切换到Chase状态 因此关闭Walk状态
                isWalk = false;
                isChase = true;

                // 切换为更快的攻击移动速度
                agent.speed = attackSpeed;

                // 如果找不到player 返回上一个状态（GUARD/PATROL
                if(FoundPlayer() == false) {

                    isFollow = false;

                    // 如果仍然有观察时间 递减时间 并站立原处
                    if(remainLookAtTime > 0) {
                        remainLookAtTime -= Time.deltaTime;
                        agent.destination = transform.position;
                    }
                    // 判断是什么状态的敌人 返回到上一个状态(GUARD/PATROL)
                    else if(isGuard)
                        enemyStates = EnemyStates.GUARD;
                    else
                        enemyStates = EnemyStates.PATROL;

                }
                // 如果找到了player则追击 目标位置为移动终点
                else {
                    // 修改状态 恢复移动标记
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }

                // 在范围内攻击Player
                if(TargetInAttackRange() || TargetInSkillRange()) {
                    // 如果在攻击范围内(近战/远程) 不再追逐
                    isFollow = false;
                    agent.isStopped = true;

                    // 上次攻击时间CD < 0 可以攻击
                    if(lastAttackTime < 0) {
                        // 重置攻击CD
                        lastAttackTime = characterStats.attackData.coolDown;

                        // 暴击判断 随机数值 < 定义的暴击值(几何分布) 返回bool值
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;

                        // 执行攻击 调用函数
                        Attack();
                    }

                }
                break;

            case EnemyStates.DEAD:
                // 关闭敌人碰撞体 避免动画播放完之前还能与碰撞体互动
                coll.enabled = false;
                // 将敌人所有功能关闭 不能进行任何互动（攻击、移动
                // agent.enabled = false;
                // 缩小敌人范围 不会索敌player 解决agent关闭导致动画报错
                agent.radius = 0;
                // 播放死亡动画后 将敌人从场景中移除
                Destroy(gameObject, 2f);
                break;
        }
    }

    void Attack() {
        // 攻击前需要先转向攻击目标
        transform.LookAt(attackTarget.transform);
        // 在近距离范围内
        if(TargetInAttackRange()) {
            // 播放近战攻击动画
            anim.SetTrigger("Attack");
        }
        if(TargetInSkillRange()) {
            // 播放技能攻击动画
            anim.SetTrigger("Skill");
        }
    }

    // 是否找到player
    bool FoundPlayer() {
        // 球体 半径内 是否存在player碰撞体
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);

        // 循环找到的所有碰撞体中是否是player
        foreach(var target in colliders)
            if(target.CompareTag("Player")) {
                // 如果找到了将player赋值给攻击对象
                attackTarget = target.gameObject;

                return true;
            }

        // 循环所有碰撞体没找到 没有攻击目标null 返回false
        attackTarget = null;
        return false;
    }

    // 判断是否在近战攻击范围
    bool TargetInAttackRange() {
        // 存在攻击目标 判断攻击目标是否在攻击范围内的bool值
        if(attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        else return false;
    }

    // 判断是否在远程攻击范围
    bool TargetInSkillRange() {
        if(attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        else return false;
    }
    // 获得巡逻路上随机的点
    void GetNewWayPoint() {

        // 等待结束 重新巡逻 重新赋值剩余观察时间
        remainLookAtTime = lookAtTime;

        // Y轴不变
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        // Y轴保持当前的坐标，防止高低地形不同
        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);

        // 可能出现的问题（可能走到不能移动的点not walkable
        NavMeshHit hit;
        // 三目运算 判断随机出来的位置是否Walkable?更新随机点:否则保持原地
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        // Sphere 球体实心 要包括 Wire 射线
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    // Animation Event
    void Hit() {
        // 判断攻击目标是否为空 角色已经跑开 避免Unity报错
        // 并且判断目标是否在正前方扇区
        if(attackTarget != null && transform.IsFacingTarget(attackTarget.transform)) {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }

    }

    public void EndNotify() {
        // 玩家死亡后 向所有敌人广播 执行以下命令
        // 播放获胜动画 停止移动 停止Agent
        anim.SetBool("Win", true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;

    }
}
