using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SceneController: Singleton<SceneController> {

    public GameObject playerPrefab;
    GameObject player;
    NavMeshAgent playerAgent;

    protected override void Awake() {
        base.Awake();
        // 保证场景切换后不会销毁当前脚本 可以找到脚本下的终点
        DontDestroyOnLoad(this);
    }

    public void TransitionToDestination(TransitionPoint transitionPoint) {
        // 判断是当前还是不同场景
        switch (transitionPoint.transitionType) {
            case TransitionPoint.TransitionType.SameScene:
                // 开始携程 获得场景得到其名字
                StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.destinationTag));
                break;

            case TransitionPoint.TransitionType.DifferentScene:
                StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.destinationTag));
                break;
        }
    }

    // 场景名称 终点场景类型标签
    IEnumerator Transition(string sceneName, TransitionDestination.DestinationTag destinationTag) {

        // 传送前保存人物数据
        SaveManager.Instance.SavaPlayerData();

        // 判断是同场景还是异场景传送
        if(SceneManager.GetActiveScene().name != sceneName) {
            // 等场景加载完成 异步加载
            yield return SceneManager.LoadSceneAsync(sceneName);
            // 场景完成后生成任务
            yield return Instantiate(playerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);

            // 异场景传送读取人物数据
            SaveManager.Instance.LoadPlayerData();
            
            // 加载完后从携程中跳出
            yield break;
        }
        else {
            // 获得player对象
            player = GameManager.Instance.playerStats.gameObject;
            playerAgent = player.GetComponent<NavMeshAgent>();

            // 停止启用导航 防止传送后走回原位
            playerAgent.enabled = false;

            // 设置传送目标点的位置和旋转方向
            player.transform.SetPositionAndRotation(GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);

            // 传送后再恢复导航
            playerAgent.enabled = true;

            // 没有return的暂时设置
            yield return null;
        }
        
    }

    private TransitionDestination GetDestination(TransitionDestination.DestinationTag destinationTag) {
        // 获得所有终点的数组
        var entrances = FindObjectsOfType<TransitionDestination>();

        // 找到标签相同 返回终点场景对象
        foreach (var e in entrances)
            if (e.destinationTag == destinationTag)
                return e;

        return null;
    }
}
