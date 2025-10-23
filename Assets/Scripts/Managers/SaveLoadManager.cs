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
    public int totalPerfectDumplings = 0;                      // 完美饺子总数
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

        // 保存对话管理器状态
        if (DialogueManager.Instance != null)
        {
            data.currentNodeId = DialogueManager.Instance.GetCurrentNodeId();
            data.storyFlags = DialogueManager.Instance.GetAllStoryFlags();
        }

        // 保存历史记录
        if (DialogueHistoryManager.Instance != null)
        {
            data.dialogueHistory = DialogueHistoryManager.Instance.GetHistoryForSave();
        }

        // 保存小游戏前节点ID
        if (GameManager.Instance != null)
        {
            data.preMinigameNodeId = GameManager.Instance.GetPreMinigameNodeId();
        }

        // 保存成就数据
        if (AchievementManager.Instance != null)
        {
            data.unlockedAchievements = AchievementManager.Instance.GetUnlockedAchievementIds();
            data.totalPerfectDumplings = AchievementManager.Instance.GetTotalPerfectDumplings();
        }

        data.saveTime = DateTime.Now;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log("游戏已保存到: " + saveFilePath);
    }

    /// <summary>
    /// 保存到指定节点（用于小游戏退出保存）
    /// </summary>
    public void SaveGameAtNode(int nodeId)
    {
        SaveData data = new SaveData();

        // 保存到指定节点
        data.currentNodeId = nodeId;

        // 保存剧情标记
        if (DialogueManager.Instance != null)
        {
            data.storyFlags = DialogueManager.Instance.GetAllStoryFlags();
        }

        // 保存历史记录
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
                AchievementManager.Instance.SetTotalPerfectDumplings(data.totalPerfectDumplings);
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
    /// 获取存档信息（用于显示存档时间等）
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
}
