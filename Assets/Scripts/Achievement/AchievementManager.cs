using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 成就管理器 - 负责成就的解锁、保存和查询
/// </summary>
public class AchievementManager : MonoBehaviour
{
    // 单例
    public static AchievementManager Instance { get; private set; }

    // 所有成就列表
    public List<Achievement> achievements;

    // 已解锁的成就ID集合（用于快速查询）
    private HashSet<int> unlockedAchievementIds;

    // 成就解锁事件
    public event Action<Achievement> OnAchievementUnlocked;

    // 完美饺子计数器（用于"包饺子大师"成就）
    private int totalPerfectDumplings = 0;

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化成就系统
    /// </summary>
    void Initialize()
    {
        // 从数据库加载所有成就
        achievements = new List<Achievement>();
        List<Achievement> template = AchievementDatabase.GetAllAchievements();
        
        // 创建副本（避免修改原始数据）
        foreach (Achievement original in template)
        {
            achievements.Add(new Achievement(
                original.achievementId,
                original.achievementName,
                original.description,
                original.unlockCondition,
                original.imageFileName
            ));
        }

        unlockedAchievementIds = new HashSet<int>();

        Debug.Log($"成就系统初始化完成，共{achievements.Count}个成就");
    }

    /// <summary>
    /// 尝试解锁成就（通过条件标记）
    /// </summary>
    public void TryUnlockAchievement(string condition)
    {
        if (string.IsNullOrEmpty(condition))
            return;

        // 查找匹配条件的成就
        Achievement achievement = achievements.Find(a => a.unlockCondition == condition);

        if (achievement != null && !achievement.isUnlocked)
        {
            UnlockAchievement(achievement);
        }
    }

    /// <summary>
    /// 解锁指定成就
    /// </summary>
    void UnlockAchievement(Achievement achievement)
    {
        achievement.isUnlocked = true;
        unlockedAchievementIds.Add(achievement.achievementId);

        Debug.Log($"🏆 成就解锁：{achievement.achievementName}");

        // 触发事件
        if (OnAchievementUnlocked != null)
        {
            OnAchievementUnlocked.Invoke(achievement);
        }

        // 自动保存
        SaveAchievements();
    }

    /// <summary>
    /// 根据ID解锁成就
    /// </summary>
    public void UnlockAchievementById(int achievementId)
    {
        Achievement achievement = achievements.Find(a => a.achievementId == achievementId);
        if (achievement != null && !achievement.isUnlocked)
        {
            UnlockAchievement(achievement);
        }
    }

    /// <summary>
    /// 检查成就是否已解锁
    /// </summary>
    public bool IsAchievementUnlocked(int achievementId)
    {
        return unlockedAchievementIds.Contains(achievementId);
    }

    /// <summary>
    /// 获取所有成就
    /// </summary>
    public List<Achievement> GetAllAchievements()
    {
        return new List<Achievement>(achievements);
    }

    /// <summary>
    /// 获取已解锁的成就数量
    /// </summary>
    public int GetUnlockedCount()
    {
        return unlockedAchievementIds.Count;
    }

    /// <summary>
    /// 获取已解锁的成就ID列表
    /// </summary>
    public List<int> GetUnlockedAchievementIds()
    {
        return new List<int>(unlockedAchievementIds);
    }

    /// <summary>
    /// 加载已解锁的成就（从存档）
    /// </summary>
    public void LoadUnlockedAchievements(List<int> ids)
    {
        if (ids == null || ids.Count == 0)
            return;

        unlockedAchievementIds.Clear();

        foreach (int id in ids)
        {
            unlockedAchievementIds.Add(id);

            // 更新成就状态
            Achievement achievement = achievements.Find(a => a.achievementId == id);
            if (achievement != null)
            {
                achievement.isUnlocked = true;
            }
        }

        Debug.Log($"成就加载完成，已解锁{unlockedAchievementIds.Count}个成就");
    }

    /// <summary>
    /// 保存成就到存档
    /// </summary>
    void SaveAchievements()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame();
        }
    }

    /// <summary>
    /// 增加完美饺子计数
    /// </summary>
    public void AddPerfectDumpling()
    {
        totalPerfectDumplings++;

        Debug.Log($"完美饺子总数：{totalPerfectDumplings}");

        // 检查"包饺子大师"成就（10个完美饺子）
        if (totalPerfectDumplings >= 10)
        {
            TryUnlockAchievement("dumplingMaster");
        }
    }

    /// <summary>
    /// 获取完美饺子总数
    /// </summary>
    public int GetTotalPerfectDumplings()
    {
        return totalPerfectDumplings;
    }

    /// <summary>
    /// 设置完美饺子总数（用于加载存档）
    /// </summary>
    public void SetTotalPerfectDumplings(int count)
    {
        totalPerfectDumplings = count;
    }

    /// <summary>
    /// 重置所有成就（调试用）
    /// </summary>
    public void ResetAllAchievements()
    {
        foreach (Achievement achievement in achievements)
        {
            achievement.isUnlocked = false;
        }
        unlockedAchievementIds.Clear();
        totalPerfectDumplings = 0;

        Debug.Log("所有成就已重置");
        SaveAchievements();
    }

#if UNITY_EDITOR
    // 编辑器调试方法
    [ContextMenu("解锁所有成就")]
    void DebugUnlockAll()
    {
        foreach (Achievement achievement in achievements)
        {
            if (!achievement.isUnlocked)
            {
                UnlockAchievement(achievement);
            }
        }
    }

    [ContextMenu("重置所有成就")]
    void DebugResetAll()
    {
        ResetAllAchievements();
    }

    [ContextMenu("显示成就状态")]
    void DebugShowStatus()
    {
        Debug.Log($"=== 成就状态 ===");
        Debug.Log($"已解锁：{unlockedAchievementIds.Count}/10");
        Debug.Log($"完美饺子：{totalPerfectDumplings}");
        foreach (Achievement a in achievements)
        {
            Debug.Log($"{a.achievementId}. {a.achievementName}: {(a.isUnlocked ? "✓" : "✗")}");
        }
    }
#endif
}
