using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI: MonoBehaviour {
    public GameObject healthUIPrefab;
    // 血条变量
    public Transform barPoint;
    // 控件长久可见设置
    public bool alwaysVisible;
    // 控件非长久可见下出现持续时间
    public float visibleTime;
    // 剩余显示时间
    private float timeLeft;
    // 血量滑动条
    Image healthSlider;
    // 血条位置
    Transform UIbar;
    // 摄像机位置
    Transform cameraPos;
    // 对象
    CharacterStats currentStats;

    void Awake() {
        currentStats = GetComponent<CharacterStats>();

        currentStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    // 人物启动则调用
    void OnEnable() {
        cameraPos = Camera.main.transform;

        // 遍历所有 Canvas 变量 生成UI 
        foreach(Canvas canvas in FindObjectsOfType<Canvas>()) {
            // 如果对象渲染是世界坐标模式
            if(canvas.renderMode == RenderMode.WorldSpace) {
                UIbar = Instantiate(healthUIPrefab, canvas.transform).transform;
                // 获取第一个子物体
                healthSlider = UIbar.GetChild(0).GetComponent<Image>();
                // 设置是否可见
                UIbar.gameObject.SetActive(alwaysVisible);
            }
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth) {

        if (UIbar) {
            if( currentHealth <= 0)
                Destroy(UIbar.gameObject);

            // 受到伤害时 UI必须被启用
            UIbar.gameObject.SetActive(true);
            // 更新剩余时间
            timeLeft = visibleTime;

            // 血量百分比
            float sliderPercent = (float)currentHealth / maxHealth;
            // 修改血条UI显示比例
            healthSlider.fillAmount = sliderPercent;
        }
      
    }

    // Update是每一帧执行 LateUpdate 上一帧渲染后再执行
    void LateUpdate() {
        // 防止销毁后还执行 需要判断是否存在
        if(UIbar) {
            UIbar.position = barPoint.position;
            // 设置血条对准摄像机 反向
            UIbar.forward = -cameraPos.forward;

            // 没有时间 且 不永久可见
            if (timeLeft <= 0 && !alwaysVisible)
                UIbar.gameObject.SetActive(false);
            // 减去时间 倒计时
            else timeLeft -= Time.deltaTime;

        }
    }
}
