using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int currentNodeId;
    public Dictionary<string, bool> storyFlags = new Dictionary<string, bool>();
    public string currentDialogueName;
    public int preMinigameNodeId = -1;  // 小游戏前的对话节点ID
    public List<DialogueHistoryManager.DialogueHistoryEntry> dialogueHistory;
    public DateTime saveTime;
    
    // 成就系统数据
    public List<int> unlockedAchievements = new List<int>();  // 已解锁的成就ID列表
}

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    private string saveFilePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Application.persistentDataPath + "/savegame.json";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        if (DialogueManager.Instance != null)
        {
            data.currentNodeId = DialogueManager.Instance.GetCurrentNodeId();
            data.storyFlags = DialogueManager.Instance.GetAllStoryFlags();
        }

        if (DialogueHistoryManager.Instance != null)
        {
            data.dialogueHistory = DialogueHistoryManager.Instance.GetHistoryForSave();
        }

        if (GameManager.Instance != null)
        {
            data.preMinigameNodeId = GameManager.Instance.GetPreMinigameNodeId();
        }

        if (AchievementManager.Instance != null)
        {
            data.unlockedAchievements = AchievementManager.Instance.GetUnlockedAchievementIds();
        }

        data.saveTime = DateTime.Now;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log("游戏已保存到: " + saveFilePath);
    }

    /// <summary>
    /// 保存到指定节点
    /// </summary>
    public void SaveGameAtNode(int nodeId)
    {
        SaveData data = new SaveData();

        data.currentNodeId = nodeId;

        if (DialogueManager.Instance != null)
        {
            data.storyFlags = DialogueManager.Instance.GetAllStoryFlags();
        }

        if (DialogueHistoryManager.Instance != null)
        {
            data.dialogueHistory = DialogueHistoryManager.Instance.GetHistoryForSave();
        }

        data.saveTime = DateTime.Now;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log($"游戏已保存到节点 {nodeId}: " + saveFilePath);
    }

    public SaveData LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // 恢复对话状态
            if (DialogueManager.Instance != null && data != null)
            {
                DialogueManager.Instance.RestoreStoryFlags(data.storyFlags);
            }

            // 恢复历史记录
            if (DialogueHistoryManager.Instance != null && data != null)
            {
                DialogueHistoryManager.Instance.LoadHistoryFromSave(data.dialogueHistory);
            }

            // 恢复成就数据
            if (AchievementManager.Instance != null && data != null)
            {
                AchievementManager.Instance.LoadUnlockedAchievements(data.unlockedAchievements);
            }

            Debug.Log("游戏已加载自: " + saveFilePath);
            return data;
        }
        else
        {
            Debug.LogWarning("未找到存档文件: " + saveFilePath);
            return null;
        }
    }

    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }

    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("存档已删除");
        }
    }

    /// <summary>
    /// 获取存档信息
    /// </summary>
    public SaveData GetSaveInfo()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            return data;
        }
        return null;
    }
    
    /// <summary>
    /// 自动存档
    /// </summary>
    public void AutoSave()
    {
        SaveGame();
        Debug.Log("[SaveLoadManager] 自动存档完成");
    }
    
    /// <summary>
    /// 加载最近的存档
    /// </summary>
    public void LoadLatestSave()
    {
        if (!HasSaveFile())
        {
            Debug.LogWarning("[SaveLoadManager] 没有可用的存档");
            return;
        }
        
        SaveData data = LoadGame();
        
        if (data != null && DialogueManager.Instance != null)
        {
            // 跳转到存档的节点
            DialogueManager.Instance.StartDialogueFromNode(data.currentNodeId);
            Debug.Log($"[SaveLoadManager] 已加载存档，跳转到节点 {data.currentNodeId}");
        }
    }
}
