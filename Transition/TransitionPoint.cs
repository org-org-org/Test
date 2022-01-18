using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionPoint: MonoBehaviour {

    // 同场景 or 异场景 传送
    public enum TransitionType {
        SameScene, DifferentScene
    }

    [Header("Transition Info")]
    public string sceneName;
    public TransitionType transitionType;
    // 传送终点的标签
    public TransitionDestination.DestinationTag destinationTag;
    // 可以被传送的标签
    private bool canTrans;

    void Update() {
        // 检测按键
        if(Input.GetKeyDown(KeyCode.E) && canTrans) {
            // 传送当前这个点
            SceneController.Instance.TransitionToDestination(this);
        }    
    }

    void OnTriggerStay(Collider other) {

        // 只有Player才能被传送 设置标签
        if (other.CompareTag("Player"))
            canTrans = true;
    }

    void OnTriggerExit(Collider other) {

        // 离开设置false
        if (other.CompareTag("Player"))
            canTrans = false;
    }
}
