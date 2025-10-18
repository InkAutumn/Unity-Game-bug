using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNode
{
    public int nodeId;
    public string characterName;
    public string dialogueText;
    public string handsSprite; // 手部图片名称（第一人称视角）
    public string backgroundImage; // 背景图名称
    public List<DialogueChoice> choices = new List<DialogueChoice>();
    public int nextNodeId = -1; // 如果没有选项，自动跳转到下一个节点
    public bool triggerMinigame = false; // 是否触发包饺子小游戏
    public int minigameTargetCount = 0; // 小游戏目标数量（0表示计时模式）
    public float minigameTimeLimit = 60f; // 小游戏时间限制
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
