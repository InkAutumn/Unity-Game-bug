using System;
using System.Collections.Generic;
using UnityEngine;

// 小游戏难度
[Serializable]
public enum MinigameDifficulty
{
    Easy,    
    Normal,  
    Hard     
}

// 小游戏对话触发时机
[Serializable]
public enum MinigameDialogueTrigger
{
    OnGameStart,        // 游戏开始时（进入准备阶段）
    OnDumplingComplete, // 饺子完成后（可指定第几个）
    OnFail,            // 失败时
    OnAllComplete      // 全部完成时
}

[Serializable]
public class DialogueNode
{
    [Header("Basic Settings")]
    public int nodeId;
    public string characterName;
    public string dialogueText;
    public string handsSprite; // 手部图片名称（第一人称视角）
    public string backgroundImage; // 背景图名称
    public List<DialogueChoice> choices = new List<DialogueChoice>();
    public int nextNodeId = -1; // 如果没有选项，自动跳转到下一个节点
    
    [Header("Minigame Settings")]
    public bool triggerMinigame = false; // 是否触发包饺子小游戏
    public int minigameTargetCount = 5; // 目标饺子数量
    public MinigameDifficulty difficulty = MinigameDifficulty.Normal;
    
    [Header("Minigame Scene Settings")]
    public string minigameBackgroundImage = ""; // 小游戏场景背景图
    public string minigameDialogueBoxBackground = ""; // 小游戏对话框背景图
    
    [Header("Lucky Dumpling")]
    public bool enableLuckyDumpling = false; // 启用幸运饺子
    public int luckyDumplingPosition = 5; // 幸运饺子位置（第几个饺子）
    
    [Header("In-Game Dialogues")]
    public List<MinigameDialogue> minigameDialogues = new List<MinigameDialogue>(); // 游戏内对话列表
    
    [Header("Auto Save")]
    public bool autoSaveAtThisNode = false; // 是否在此节点自动存档
}

// 小游戏内对话配置
[Serializable]
public class MinigameDialogue
{
    public MinigameDialogueTrigger trigger = MinigameDialogueTrigger.OnGameStart; // 触发时机
    public int triggerAtCount = -1; // 当trigger为OnDumplingComplete时指定第几个（-1表示任意）
    public string characterName = ""; // 角色名（空字符串=旁白）
    public string dialogueText = ""; // 对话内容
    public List<DialogueChoice> choices = new List<DialogueChoice>(); // 可选的选项
    public bool pauseGame = true; // 是否暂停游戏
    public bool playOnce = true; // 是否只播放一次
    
    [Header("对话框样式（可选，留空则使用默认）")]
    public string customDialogueBoxBackground = ""; // 自定义对话框背景图名称
    public string customChoiceButtonBackground = ""; // 自定义选项按钮背景图名称
}

[Serializable]
public class DialogueChoice
{
    public string choiceText;
    public int targetNodeId;
    public string requirementFlag; // 可选：需要满足的条件标记
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Visual Novel/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public int startNodeId = 0;
    public List<DialogueNode> nodes = new List<DialogueNode>();

    public DialogueNode GetNode(int nodeId)
    {
        return nodes.Find(n => n.nodeId == nodeId);
    }
}
