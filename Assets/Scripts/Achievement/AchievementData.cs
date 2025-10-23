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
/// 成就数据库 - 定义所有成就
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
            allAchievements = new List<Achievement>
            {
                // 成就1：初次相遇
                new Achievement(
                    1,
                    "初次相遇",
                    "第一次与家人对话",
                    "firstMeeting",
                    "achievement_01"
                ),

                // 成就2：包饺子新手
                new Achievement(
                    2,
                    "包饺子新手",
                    "第一次完成包饺子",
                    "firstDumpling",
                    "achievement_02"
                ),

                // 成就3：猪肉大葱爱好者
                new Achievement(
                    3,
                    "猪肉大葱爱好者",
                    "选择了猪肉大葱馅",
                    "chosePorkFilling",
                    "achievement_03"
                ),

                // 成就4：韭菜鸡蛋拥护者
                new Achievement(
                    4,
                    "韭菜鸡蛋拥护者",
                    "选择了韭菜鸡蛋馅",
                    "choseLeekFilling",
                    "achievement_04"
                ),

                // 成就5：包饺子大师
                new Achievement(
                    5,
                    "包饺子大师",
                    "累计包出10个完美饺子",
                    "dumplingMaster",
                    "achievement_05"
                ),

                // 成就6：硬币的秘密
                new Achievement(
                    6,
                    "硬币的秘密",
                    "完成了特殊的硬币饺子",
                    "coinDumplingCompleted",
                    "achievement_06"
                ),

                // 成就7：奶奶的传承
                new Achievement(
                    7,
                    "奶奶的传承",
                    "听完了奶奶的完整故事",
                    "grandmaStoryComplete",
                    "achievement_07"
                ),

                // 成就8：温暖的回忆
                new Achievement(
                    8,
                    "温暖的回忆",
                    "解锁了所有家庭对话",
                    "allFamilyDialogues",
                    "achievement_08"
                ),

                // 成就9：完美主义者
                new Achievement(
                    9,
                    "完美主义者",
                    "一次性包出5个完美饺子",
                    "perfectStreak",
                    "achievement_09"
                ),

                // 成就10：大团圆
                new Achievement(
                    10,
                    "大团圆",
                    "完成了游戏的所有结局",
                    "allEndingsComplete",
                    "achievement_10"
                )
            };
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
