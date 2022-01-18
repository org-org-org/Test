using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI: MonoBehaviour {
    Text levelText;
    Image healthSlider;
    Image expSlider;

    void Awake() {
        // 获得自身变量的赋值 当前对象下标下的子物体的子物体
        healthSlider = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        expSlider = transform.GetChild(1).GetChild(0).GetComponent<Image>();
        levelText = transform.GetChild(2).GetComponent<Text>();
    }

    void Update() {
        // 一级可以格式化显示 01
        levelText.text = "LEVEL  " + GameManager.Instance.playerStats.characterData.currentLevel.ToString("00");
        UpdateHealth();
        UpdateExp();
    }

    void UpdateHealth() {
        float sliderPercent = (float) GameManager.Instance.playerStats.CurrentHealth / GameManager.Instance.playerStats.MaxHealth;
        healthSlider.fillAmount = sliderPercent;
    }

    void UpdateExp() {
        float sliderPercent = (float) GameManager.Instance.playerStats.characterData.currentExp / GameManager.Instance.playerStats.characterData.baseExp;
        expSlider.fillAmount = sliderPercent;
    }
}
