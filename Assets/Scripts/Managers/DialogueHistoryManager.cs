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
        public string timestamp;

        public DialogueHistoryEntry(string name, string text)
        {
            characterName = name;
            dialogueText = text;
            timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        }
    }

    [Header("UI References")]
    public GameObject historyPanel;
    public Transform historyContentContainer; 
    public GameObject historyEntryPrefab; 

    [Header("Settings")]
    public int maxHistoryEntries = 100;

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
        DialogueHistoryEntry entry = new DialogueHistoryEntry(characterName, dialogueText);
        historyEntries.Add(entry);

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

        ClearHistoryDisplay();

        foreach (DialogueHistoryEntry entry in historyEntries)
        {
            CreateHistoryEntryUI(entry);
        }

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
            entryObj = Instantiate(historyEntryPrefab, historyContentContainer);
        }
        else
        {
            entryObj = new GameObject("HistoryEntry");
            entryObj.transform.SetParent(historyContentContainer);
            entryObj.AddComponent<LayoutElement>().preferredHeight = 80;
        }

        Text[] texts = entryObj.GetComponentsInChildren<Text>();
        if (texts.Length >= 2)
        {
            texts[0].text = entry.characterName;      
            texts[1].text = entry.dialogueText;
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
    
    /// <summary>
    /// 切换历史面板显示/隐藏
    /// </summary>
    public void ToggleHistoryPanel()
    {
        if (historyPanel == null)
        {
            Debug.LogWarning("[DialogueHistoryManager] 历史面板未设置");
            return;
        }
        
        bool isActive = historyPanel.activeSelf;
        
        if (isActive)
        {
            historyPanel.SetActive(false);
            Debug.Log("[DialogueHistoryManager] 关闭历史面板");
        }
        else
        {
            historyPanel.SetActive(true);
            RefreshHistoryDisplay();
            Debug.Log("[DialogueHistoryManager] 打开历史面板");
        }
    }
    
    /// <summary>
    /// 显示历史面板
    /// </summary>
    public void ShowHistoryPanel()
    {
        if (historyPanel != null)
        {
            historyPanel.SetActive(true);
            RefreshHistoryDisplay();
        }
    }
    
    /// <summary>
    /// 隐藏历史面板
    /// </summary>
    public void HideHistoryPanel()
    {
        if (historyPanel != null)
        {
            historyPanel.SetActive(false);
        }
    }
}
