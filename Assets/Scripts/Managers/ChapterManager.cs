using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 章节管理器
/// 负责管理多个对话章节，根据节点ID自动加载对应章节
/// </summary>
public class ChapterManager : MonoBehaviour
{
    #region 单例模式
    
    private static ChapterManager instance;
    public static ChapterManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ChapterManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("ChapterManager");
                    instance = obj.AddComponent<ChapterManager>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ChapterManager initialized");
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    #endregion

    #region 章节配置

    /// <summary>
    /// 章节配置数据结构
    /// </summary>
    [System.Serializable]
    public class ChapterConfig
    {
        [Header("章节信息")]
        [Tooltip("章节名称（中文）")]
        public string chapterName = "";
        
        [Tooltip("章节描述")]
        public string description = "";
        
        [Header("节点范围")]
        [Tooltip("起始节点ID")]
        public int startNodeId = 0;
        
        [Tooltip("结束节点ID")]
        public int endNodeId = 0;
        
        [Header("对话数据")]
        [Tooltip("该章节的对话数据资产")]
        public DialogueData dialogueData = null;

        // 检查节点是否在此章节范围内
        public bool ContainsNode(int nodeId)
        {
            return nodeId >= startNodeId && nodeId <= endNodeId;
        }
    }

    [Header("章节列表")]
    [Tooltip("按顺序配置所有章节")]
    public List<ChapterConfig> chapters = new List<ChapterConfig>();

    [Header("调试选项")]
    [Tooltip("显示详细日志")]
    public bool showDebugLog = true;

    #endregion

    #region 章节查询

    /// <summary>
    /// 根据节点ID获取对应的章节对话数据
    /// </summary>
    public DialogueData GetChapterByNodeId(int nodeId)
    {
        foreach (ChapterConfig chapter in chapters)
        {
            if (chapter.ContainsNode(nodeId))
            {
                if (showDebugLog)
                {
                    Debug.Log("[ChapterManager] 节点" + nodeId + " 属于章节: " + chapter.chapterName);
                }
                return chapter.dialogueData;
            }
        }

        Debug.LogWarning("[ChapterManager] 未找到包含节点" + nodeId + "的章节！");
        return null;
    }

    /// <summary>
    /// 根据节点ID获取章节名称
    /// </summary>
    public string GetChapterName(int nodeId)
    {
        foreach (ChapterConfig chapter in chapters)
        {
            if (chapter.ContainsNode(nodeId))
            {
                return chapter.chapterName;
            }
        }
        return "未知章节";
    }

    /// <summary>
    /// 获取章节配置
    /// </summary>
    public ChapterConfig GetChapterConfig(int nodeId)
    {
        foreach (ChapterConfig chapter in chapters)
        {
            if (chapter.ContainsNode(nodeId))
            {
                return chapter;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据章节名称获取对话数据
    /// </summary>
    public DialogueData GetChapterByName(string chapterName)
    {
        foreach (ChapterConfig chapter in chapters)
        {
            if (chapter.chapterName == chapterName)
            {
                return chapter.dialogueData;
            }
        }
        
        Debug.LogWarning("[ChapterManager] 未找到章节: " + chapterName);
        return null;
    }

    /// <summary>
    /// 获取第一章（用于开始新游戏）
    /// </summary>
    public DialogueData GetFirstChapter()
    {
        if (chapters.Count > 0 && chapters[0].dialogueData != null)
        {
            if (showDebugLog)
            {
                Debug.Log("[ChapterManager] 加载第一章: " + chapters[0].chapterName);
            }
            return chapters[0].dialogueData;
        }
        
        Debug.LogError("[ChapterManager] 没有配置任何章节！");
        return null;
    }

    #endregion

    #region 章节验证

    /// <summary>
    /// 验证章节配置是否正确
    /// </summary>
    [ContextMenu("验证章节配置")]
    public void ValidateChapters()
    {
        Debug.Log("=== 开始验证章节配置 ===");
        
        if (chapters.Count == 0)
        {
            Debug.LogError("❌ 错误：没有配置任何章节！");
            return;
        }

        int totalNodes = 0;
        bool hasError = false;

        for (int i = 0; i < chapters.Count; i++)
        {
            ChapterConfig chapter = chapters[i];
            
            // 检查章节名称
            if (string.IsNullOrEmpty(chapter.chapterName))
            {
                Debug.LogError("❌ 错误：第" + i + "个章节没有设置名称！");
                hasError = true;
            }

            // 检查对话数据
            if (chapter.dialogueData == null)
            {
                Debug.LogError("❌ 错误：章节\"" + chapter.chapterName + "\"没有配置对话数据！");
                hasError = true;
            }

            // 检查节点范围
            if (chapter.startNodeId > chapter.endNodeId)
            {
                Debug.LogError("❌ 错误：章节\"" + chapter.chapterName + "\"的节点范围配置错误！起始节点(" + chapter.startNodeId + ") > 结束节点(" + chapter.endNodeId + ")");
                hasError = true;
            }

            // 检查与其他章节是否重叠
            for (int j = i + 1; j < chapters.Count; j++)
            {
                ChapterConfig otherChapter = chapters[j];
                if (IsRangeOverlap(chapter.startNodeId, chapter.endNodeId, 
                                  otherChapter.startNodeId, otherChapter.endNodeId))
                {
                    Debug.LogError("❌ 错误：章节\"" + chapter.chapterName + "\"与\"" + otherChapter.chapterName + "\"的节点范围重叠！");
                    hasError = true;
                }
            }

            // 统计节点数量
            int nodeCount = chapter.endNodeId - chapter.startNodeId + 1;
            totalNodes += nodeCount;
            
            Debug.Log("✓ 章节" + (i+1) + "：" + chapter.chapterName + " (节点" + chapter.startNodeId + "-" + chapter.endNodeId + ", 共" + nodeCount + "个节点)");
        }

        Debug.Log("=== 验证完成 ===");
        Debug.Log("总章节数：" + chapters.Count);
        Debug.Log("总节点数：" + totalNodes);
        
        if (!hasError)
        {
            Debug.Log("✅ 所有章节配置正确！");
        }
        else
        {
            Debug.LogWarning("⚠️ 章节配置存在错误，请修正后再使用！");
        }
    }

    /// <summary>
    /// 检查两个范围是否重叠
    /// </summary>
    private bool IsRangeOverlap(int start1, int end1, int start2, int end2)
    {
        return start1 <= end2 && end1 >= start2;
    }

    #endregion

    #region 调试功能

    /// <summary>
    /// 打印所有章节信息
    /// </summary>
    [ContextMenu("打印章节列表")]
    public void PrintChapterList()
    {
        Debug.Log("=== 章节列表 ===");
        for (int i = 0; i < chapters.Count; i++)
        {
            ChapterConfig chapter = chapters[i];
            Debug.Log((i+1) + ". " + chapter.chapterName + " (节点" + chapter.startNodeId + "-" + chapter.endNodeId + ")");
            Debug.Log("   描述: " + chapter.description);
            Debug.Log("   数据: " + (chapter.dialogueData != null ? chapter.dialogueData.name : "未配置"));
        }
    }

    /// <summary>
    /// 测试节点查询
    /// </summary>
    [ContextMenu("测试节点查询")]
    public void TestNodeQuery()
    {
        int[] testNodes = { 100, 200, 300, 400, 500 };
        
        Debug.Log("=== 测试节点查询 ===");
        foreach (int nodeId in testNodes)
        {
            string chapterName = GetChapterName(nodeId);
            DialogueData data = GetChapterByNodeId(nodeId);
            Debug.Log("节点" + nodeId + " → 章节: " + chapterName + ", 数据: " + (data != null ? data.name : "未找到"));
        }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 获取章节总数
    /// </summary>
    public int GetChapterCount()
    {
        return chapters.Count;
    }

    /// <summary>
    /// 检查章节是否已配置
    /// </summary>
    public bool IsChapterConfigured()
    {
        return chapters.Count > 0 && chapters[0].dialogueData != null;
    }

    /// <summary>
    /// 获取包含节点数量最多的章节
    /// </summary>
    public ChapterConfig GetLargestChapter()
    {
        ChapterConfig largest = null;
        int maxNodes = 0;

        foreach (ChapterConfig chapter in chapters)
        {
            int nodeCount = chapter.endNodeId - chapter.startNodeId + 1;
            if (nodeCount > maxNodes)
            {
                maxNodes = nodeCount;
                largest = chapter;
            }
        }

        return largest;
    }

    #endregion
}
