using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager: Singleton<SaveManager> {
    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Update() {
        // 保存
        if (Input.GetKeyDown(KeyCode.S)) {
            SavaPlayerData();
        }

        // 读取
        if (Input.GetKeyDown(KeyCode.L)) {
            LoadPlayerData();
        }
    }

    public void SavaPlayerData() {
        Save(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.name);
    }

    public void LoadPlayerData() {
        Load(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.name);
    }

    public void Save(object data, string key) {
        var jsonData = JsonUtility.ToJson(data, true);

        // 连接 key jsonData 保存在系统磁盘上
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.Save();
    }

    public void Load(object data, string key) {

        // 判断 key 是否有数值
        if (PlayerPrefs.HasKey(key))
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data);
        
    }
}
