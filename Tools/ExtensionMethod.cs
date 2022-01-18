using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 拓展方法不会继承其他类 静态使得可以快速调用和执行
public static class ExtensionMethod {
    // 静态需要为常量 const 不可更改
    private const float dotThreshold = 0.5f;

    // 声明 判断目标是否在正前方
    // 函数参入的参数 ：前一个类为需要拓展的类 后一个是需要调用的类
    public static bool IsFacingTarget(this Transform transform, Transform target) {
        // 获得目标与自己的相对位置
        var vectorToTarget = target.position - transform.position;
        vectorToTarget.Normalize();

        // 传入当前敌人的朝向 
        float dot = Vector3.Dot(transform.forward, vectorToTarget);

        // 返回攻击目标是否在 45°攻击范围内
        return dot >= dotThreshold;
    }
}
