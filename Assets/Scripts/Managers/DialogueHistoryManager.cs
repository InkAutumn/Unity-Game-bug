using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 历史对话记录管理器
/// 记录并显示已发生的对话内容
/// </summary>
public class DialogueHistoryManager : MonoBehaviour
{
    public static DialogueHistoryManager Instance { get; private set; }

    [System.Serializable]
    public class DialogueHistoryEntry
    {
        public string characterName;
        public string dialogueText;
        public string timestamp;  // 可选：记录时间戳

        public DialogueHistoryEntry(string name, string text)
        {
            characterName = name;
            dialogueText = text;
            timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        }
    }

    [Header("UI References")]
    public GameObject historyPanel;
    public Transform historyContentContainer;  // ScrollView的Content
    public GameObject historyEntryPrefab;      // 历史条目预制体

    [Header("Settings")]
    public int maxHistoryEntries = 100;        // 最大记录数量

    private List<DialogueHistoryEntry> historyEntries = new List<DialogueHistoryEntry>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 添加一条对话记录
    /// </summary>
    public void AddDialogueEntry(string characterName, string dialogueText)
    {
        // 创建新条目
        DialogueHistoryEntry entry = new DialogueHistoryEntry(characterName, dialogueText);
        historyEntries.Add(entry);

        // 限制最大数量
        if (historyEntries.Count > maxHistoryEntries)
        {
            historyEntries.RemoveAt(0);
        }

        Debug.Log($"添加历史记录: [{characterName}] {dialogueText}");
    }

    /// <summary>
    /// 刷新历史记录显示
    /// </summary>
    public void RefreshHistoryDisplay()
    {
        if (historyContentContainer == null)
        {
            Debug.LogWarning("历史记录容器未设置！");
            return;
        }

        // 清空现有显示
        ClearHistoryDisplay();

        // 创建历史条目UI
        foreach (DialogueHistoryEntry entry in historyEntries)
        {
            CreateHistoryEntryUI(entry);
        }

        // 滚动到底部（最新记录）
        Canvas.ForceUpdateCanvases();
        if (historyContentContainer.GetComponentInParent<ScrollRect>() != null)
        {
            ScrollRect scrollRect = historyContentContainer.GetComponentInParent<ScrollRect>();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    void CreateHistoryEntryUI(DialogueHistoryEntry entry)
    {
        GameObject entryObj;

        if (historyEntryPrefab != null)
        {
            // 使用预制体
            entryObj = Instantiate(historyEntryPrefab, historyContentContainer);
        }
        else
        {
            // 创建简单文本条目
            entryObj = new GameObject("HistoryEntry");
            entryObj.transform.SetParent(historyContentContainer);
            entryObj.AddComponent<LayoutElement>().preferredHeight = 80;
        }

        // 设置文本内容
        Text[] texts = entryObj.GetComponentsInChildren<Text>();
        if (texts.Length >= 2)
        {
            texts[0].text = entry.characterName;      // 角色名
            texts[1].text = entry.dialogueText;       // 对话内容
        }
        else if (texts.Length == 1)
        {
            texts[0].text = $"【{entry.characterName}】\n{entry.dialogueText}";
        }

        entryObj.transform.localScale = Vector3.one;
    }

    void ClearHistoryDisplay()
    {
        if (historyContentContainer == null) return;

        // 删除所有子对象
        foreach (Transform child in historyContentContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 清空所有历史记录
    /// </summary>
    public void ClearAllHistory()
    {
        historyEntries.Clear();
        ClearHistoryDisplay();
        Debug.Log("清空所有历史记录");
    }

    /// <summary>
    /// 获取所有历史记录
    /// </summary>
    public List<DialogueHistoryEntry> GetAllEntries()
    {
        return new List<DialogueHistoryEntry>(historyEntries);
    }

    /// <summary>
    /// 保存历史记录到存档
    /// </summary>
    public List<DialogueHistoryEntry> GetHistoryForSave()
    {
        return historyEntries;
    }

    /// <summary>
    /// 从存档加载历史记录
    /// </summary>
    public void LoadHistoryFromSave(List<DialogueHistoryEntry> savedHistory)
    {
        if (savedHistory != null)
        {
            historyEntries = new List<DialogueHistoryEntry>(savedHistory);
            Debug.Log($"加载历史记录: {historyEntries.Count} 条");
        }
    }
}
