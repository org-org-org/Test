using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : Singleton<GameManager>
{
    public CharacterStats playerStats;

    private CinemachineFreeLook followCamera;

    // 列表 汇集观察者
    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();

    // 注册player
    public void RigisterPlayer(CharacterStats player) {
        playerStats = player;
        // 拿到相机
        followCamera = FindObjectOfType<CinemachineFreeLook>();

        if(followCamera != null) {
            // 得到 LookAtPoint 的位置
            followCamera.Follow = playerStats.transform.GetChild(2);
            followCamera.LookAt = playerStats.transform.GetChild(2);
        }
    }

    protected override void Awake() {
        base.Awake();
        // 保证场景切换后不会销毁当前脚本 可以找到脚本下目标
        DontDestroyOnLoad(this);
    }

    // 将生成的敌人添加到观察者列表
    public void AddObserver(IEndGameObserver observer) {
        // 生成时才会添加 故不会存在重复对象 不需要判断是否在列表中存在
        endGameObservers.Add(observer);
    }

    // 移除观察者
    public void RemoveObserver(IEndGameObserver observer) {
        endGameObservers.Remove(observer);
    }

    // 向所有观察者广播
    public void NotifyObservers() {

        // 循环每一个观察者 播放游戏结束广播
        foreach (var observer in endGameObservers) 
            observer.EndNotify();
        
    }
}
