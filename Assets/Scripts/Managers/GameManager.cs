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
    
    // ====== ChapterManager集成：新游戏标记 ======
    public bool isStartingNewGame = false; // 标记是否正在开始新游戏

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

                // ====== ChapterManager集成：检查新游戏标记 ======
                if (isStartingNewGame)
                {
                    // 开始新游戏，自动从第一章开始
                    dialogueManager.StartNewGame();
                    isStartingNewGame = false; // 重置标记
                    Debug.Log("[GameManager] 已启动新游戏");
                }
                // 如果有返回节点，继续对话
                else if (returnNodeId >= 0)
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
}
