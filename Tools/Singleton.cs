using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// where 约束 T 是 Singleton<T> 类型
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;

    // 单例模式 仅可读 get
    public static T Instance {
        get { return instance; }
    }

    // 只允许继承类访问 重写 继承
    protected virtual void Awake() {
        if (instance != null) {
            Destroy(gameObject);
        }
        else {
            // 泛型 this 适应不同类型
            instance = (T)this;
        }
    }

    // 判断当前泛型单例模式是否已被生成
    public static bool IsInitialized {
        // 单例不空 == true
        get { return instance != null; }
    }

    // 
    protected virtual void OnDestory() {
        
        // 事例被销毁 令为空
        if(instance == this) {
            instance = null;
        }
    }

}
