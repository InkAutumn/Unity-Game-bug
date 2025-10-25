using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 成就数据结构
/// </summary>
[Serializable]
public class Achievement
{
    public int achievementId;           // 成就ID (1-10)
    public string achievementName;      // 成就名称
    public string description;          // 成就描述
    public string unlockCondition;      // 解锁条件（剧情标记）
    public string imageFileName;        // 成就图片文件名
    public bool isUnlocked;            // 是否已解锁

    public Achievement(int id, string name, string desc, string condition, string imageName)
    {
        achievementId = id;
        achievementName = name;
        description = desc;
        unlockCondition = condition;
        imageFileName = imageName;
        isUnlocked = false;
    }
}

/// <summary>
/// 成就数据库
/// </summary>
public static class AchievementDatabase
{
    private static List<Achievement> allAchievements;

    /// <summary>
    /// 初始化所有成就数据
    /// </summary>
    public static List<Achievement> GetAllAchievements()
    {
        if (allAchievements == null)
        {
            allAchievements = new List<Achievement>();
        }

        return allAchievements;
    }

    /// <summary>
    /// 根据ID获取成就
    /// </summary>
    public static Achievement GetAchievementById(int id)
    {
        List<Achievement> achievements = GetAllAchievements();
        return achievements.Find(a => a.achievementId == id);
    }

    /// <summary>
    /// 根据解锁条件获取成就
    /// </summary>
    public static Achievement GetAchievementByCondition(string condition)
    {
        List<Achievement> achievements = GetAllAchievements();
        return achievements.Find(a => a.unlockCondition == condition);
    }
}
