using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// [System.Serializable]
// public class EventVector3 : UnityEvent<Vector3> {}

public class MouseManager : Singleton<MouseManager> {

    // 继承泛型单例后就不需要了
    // public static MouseManager Instance;
    
    // 鼠标不同材质的对象
    public Texture2D point, doorway, attack, target, arrow;
    RaycastHit hitInfo;
    public event Action<Vector3> OnMouseClicked;
    // 点击敌人
    public event Action<GameObject> OnEnemyClicked;

    // 单例被注释 被泛型单例模式替代
    //void Awake(){
    //    if(Instance != null)
    //        Destroy(gameObject);
    //    Instance = this;
    //}

    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    // 更新鼠标的材质以及鼠标的控制
    void Update(){
        SetCursorTexture();
        MouseControl();
    }

    void SetCursorTexture(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hitInfo)){
            // 切换鼠标贴图
            switch(hitInfo.collider.gameObject.tag){
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Portal":
                    Cursor.SetCursor(doorway, new Vector2(16, 16), CursorMode.Auto);
                    break;
            }
        }
    }

    void MouseControl(){
        if(Input.GetMouseButtonDown(0) && hitInfo.collider != null){
            
            // 点击地面判断碰撞信息 Clicked存在传入碰撞指向的坐标
            if(hitInfo.collider.gameObject.CompareTag("Ground"))
                OnMouseClicked?.Invoke(hitInfo.point);

            if(hitInfo.collider.gameObject.CompareTag("Enemy"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);

            if(hitInfo.collider.gameObject.CompareTag("Attackable"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);

            if (hitInfo.collider.gameObject.CompareTag("Portal"))
                OnMouseClicked?.Invoke(hitInfo.point);
        }
    }
}
