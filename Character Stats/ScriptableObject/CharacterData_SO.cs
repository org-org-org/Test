using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Character Stats/Data")]
public class CharacterData_SO: ScriptableObject {

    [Header("Stats Info")]
    public int maxHealth;
    public int currentHealth;
    public int baseDefence;
    public int currentDefence;

    [Header("Kill")]
    public int killPoint;

    [Header("Level")]
    public int maxLevel;
    public int currentLevel;
    public int baseExp;
    public int currentExp;
    // 等级提升固定的倍率
    public float levelBuff;
    // 翻倍的倍率
    public float levelMultpiler {
        // 加上等级增量的倍率
        get { return 1 + (currentLevel - 1) * levelBuff; }
    }

    public void UpdateExp(int point) {
        currentExp += point;

        if (currentExp >= baseExp)
            LevelUp();
    }

    private void LevelUp() {
        // 所有需要提升的数据属性
        // 保证升级不会超过最大等级
        currentLevel = Mathf.Clamp(currentLevel + 1, 0, maxLevel);
        baseExp += (int) (baseExp * levelMultpiler);
        
        currentHealth = maxHealth = (int) (maxHealth * levelMultpiler);

        Debug.Log("LEVEL UP!");
    }
}
