using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour{

    // 事件两边 满血及当前血量
    public event Action<int, int> UpdateHealthBarOnAttack;

    // 复制一份另外的模板数据
    public CharacterData_SO templateData;
    public CharacterData_SO characterData;
    public AttackData_SO attackData;

    // 在Inspector中不会看见
    [HideInInspector]
    // public可以在其他代码中访问 暴击标记 
    public bool isCritical;

    void Awake() {
        // 模板不为空 生成一份数据 给当前数据
        if(templateData != null) 
            characterData = Instantiate(templateData);
    }

    #region Read form Data_SO
    public int MaxHealth{
        get{
            // 如果面板不空 返回对应数值
            if(characterData != null)
                return characterData.maxHealth;
            // 面板为空返回0
            else return 0;
        }

        set{
            characterData.maxHealth = value;
        }
    }

    public int CurrentHealth{
        get{
            // 如果面板不空 返回对应数值
            if(characterData != null)
                return characterData.currentHealth;
            // 面板为空返回0
            else return 0;
        }

        set{
            characterData.currentHealth = value;
        }
    }

    public int BaseDefence{
        get{
            // 如果面板不空 返回对应数值
            if(characterData != null)
                return characterData.baseDefence;
            // 面板为空返回0
            else return 0;
        }

        set{
            characterData.baseDefence = value;
        }
    }

    public int CurrentDefence{
        get{
            // 如果面板不空 返回对应数值
            if(characterData != null)
                return characterData.currentDefence;
            // 面板为空返回0
            else return 0;
        }

        set{
            characterData.currentDefence = value;
        }
    }
    #endregion

    #region Character Combat
    public void TakeDamage(CharacterStats attacker, CharacterStats defender){
        // 获取两个对象属性值 计算伤害
        // 防止攻击力小于防御力产生负值 反而加血 取0为攻击值
        int damage = Mathf.Max(0, attacker.CurrentDamage() - defender.CurrentDefence);

        // 计算攻击后的血量 同理防止减去攻击后血量出现负值情况 取0位血量
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);

        // 攻击者暴击 设置受击者Trigger状态 播放受击动画
        if (attacker.isCritical) 
            defender.GetComponent<Animator>().SetTrigger("Hit");

        // 血量UI更新 为空 ? cur : max
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);

        // 当对象死亡时 将经验给予攻击者
        if (CurrentHealth <= 0)
            attacker.characterData.UpdateExp(characterData.killPoint);              
    }

    // 重写方法 改变参数
    public void TakeDamage(int damage, CharacterStats defender) {
        // 声明临时伤害变量 记录攻击的结果
        int currentDamage = Mathf.Max(0, damage - defender.CurrentDefence);
        // 计算血量
        CurrentHealth = Mathf.Max(0, CurrentHealth - currentDamage);
        // 血量条更新
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);

        // 如果用石头击败石头人 需要给予经验
        if (CurrentHealth <= 0)
            GameManager.Instance.playerStats.characterData.UpdateExp(characterData.killPoint);
    }

    private int CurrentDamage(){
        // 获取最大最小攻击值范围的随机值
        float coreDamage = UnityEngine.Random.Range(attackData.minDamge, attackData.maxDamge);

        // 判断暴击
        if(isCritical)
             coreDamage *= attackData.criticalMultiplier;
        
        return (int)coreDamage;
    }
    #endregion

}
