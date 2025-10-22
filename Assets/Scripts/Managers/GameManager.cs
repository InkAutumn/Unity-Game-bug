using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Names")]
    public string dialogueSceneName = "DialogueScene";
    public string dumplingGameSceneName = "DumplingGameScene";

    private int returnNodeId = -1; // 从小游戏返回时要显示的对话节点
    private int preMinigameNodeId = -1; // 小游戏触发前的对话节点（用于保存点）
    
    // 小游戏UI设置
    private MinigameUISettings currentMinigameUI;
    
    // 特殊道具设置
    private SpecialItem currentSpecialItem;

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

    void Start()
    {
        // 订阅DialogueManager的小游戏触发事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == dialogueSceneName)
        {
            // 对话场景加载完成
            DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager != null)
            {
                dialogueManager.OnMinigameTriggered += OnMinigameTriggered;

                // 如果有返回节点，继续对话
                if (returnNodeId >= 0)
                {
                    dialogueManager.ShowNode(returnNodeId);
                    returnNodeId = -1;
                }
            }
        }
    }

    void OnMinigameTriggered()
    {
        LoadDumplingGame();
    }

    public void LoadDumplingGame()
    {
        // 保存小游戏触发前的对话节点ID
        if (DialogueManager.Instance != null)
        {
            preMinigameNodeId = DialogueManager.Instance.GetCurrentNodeId();
            Debug.Log($"保存小游戏前节点ID: {preMinigameNodeId}");
        }

        SceneManager.LoadScene(dumplingGameSceneName);
    }

    public void ReturnToDialogue(int nodeId = -1)
    {
        if (nodeId >= 0)
        {
            returnNodeId = nodeId;
        }
        SceneManager.LoadScene(dialogueSceneName);
    }

    public void LoadDialogue(DialogueData dialogue)
    {
        // 可以在这里保存对话数据的引用
        SceneManager.LoadScene(dialogueSceneName);
    }

    // 新增：获取小游戏前的节点ID
    public int GetPreMinigameNodeId()
    {
        return preMinigameNodeId;
    }

    // 新增：设置小游戏前的节点ID
    public void SetPreMinigameNodeId(int nodeId)
    {
        preMinigameNodeId = nodeId;
    }
    
    // UI设置传递
    public void SetMinigameUISettings(MinigameUISettings settings)
    {
        currentMinigameUI = settings;
    }
    
    public MinigameUISettings GetMinigameUISettings()
    {
        return currentMinigameUI ?? new MinigameUISettings();
    }
    
    // 特殊道具设置
    public void SetSpecialItem(SpecialItem item)
    {
        currentSpecialItem = item;
    }
    
    public SpecialItem GetSpecialItem()
    {
        return currentSpecialItem;
    }
    
    public void ClearSpecialItem()
    {
        currentSpecialItem = null;
    }
    
    // 便捷方法：设置使用特殊道具
    public void SetUseSpecialItem(bool use, SpecialItemType itemType, int appearOn)
    {
        if (use)
        {
            currentSpecialItem = new SpecialItem
            {
                itemType = itemType,
                appearOnDumplingNumber = appearOn
            };
        }
        else
        {
            currentSpecialItem = null;
        }
    }
    
    // 从小游戏返回对话（保持兼容性）
    public void ReturnToDialogueFromMinigame()
    {
        // 清除特殊道具设置
        ClearSpecialItem();
        
        // 返回对话场景
        ReturnToDialogue();
    }
}
