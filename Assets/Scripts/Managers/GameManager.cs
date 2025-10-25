using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Names")]
    public string dialogueSceneName = "DialogueScene";
    public string dumplingGameSceneName = "DumplingGameScene";

    private int returnNodeId = -1; // 从小游戏返回时要显示的对话节点
    private int preMinigameNodeId = -1; // 小游戏触发前的对话节点
    
    private MinigameConfig currentMinigameConfig; // 当前小游戏配置
    
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
            DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager != null)
            {
                dialogueManager.OnMinigameTriggered += OnMinigameTriggered;

                if (isStartingNewGame)
                {
                    dialogueManager.StartNewGame();
                    isStartingNewGame = false; 
                    Debug.Log("[GameManager] 已启动新游戏");
                }
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
        if (DialogueManager.Instance != null)
        {
            preMinigameNodeId = DialogueManager.Instance.GetCurrentNodeId();
            
            DialogueNode currentNode = DialogueManager.Instance.GetCurrentNode();
            if (currentNode != null && currentNode.triggerMinigame)
            {
                currentMinigameConfig = new MinigameConfig
                {
                    targetCount = currentNode.minigameTargetCount,
                    difficulty = currentNode.difficulty,
                    enableLuckyDumpling = currentNode.enableLuckyDumpling,
                    luckyDumplingPosition = currentNode.luckyDumplingPosition,
                    backgroundImage = currentNode.minigameBackgroundImage,
                    dialogueBoxBackground = currentNode.minigameDialogueBoxBackground,
                    minigameDialogues = currentNode.minigameDialogues
                };
                
                Debug.Log($"[GameManager] 保存小游戏配置 - 难度: {currentMinigameConfig.difficulty}, 背景: {currentMinigameConfig.backgroundImage}");
            }
            
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
        SceneManager.LoadScene(dialogueSceneName);
    }

    public int GetPreMinigameNodeId()
    {
        return preMinigameNodeId;
    }

    public void SetPreMinigameNodeId(int nodeId)
    {
        preMinigameNodeId = nodeId;
    }
    
    public MinigameConfig GetMinigameConfig()
    {
        return currentMinigameConfig;
    }
}

// 小游戏配置数据类
[System.Serializable]
public class MinigameConfig
{
    public int targetCount;
    public MinigameDifficulty difficulty;
    public bool enableLuckyDumpling;
    public int luckyDumplingPosition;
    public string backgroundImage;
    public string dialogueBoxBackground;
    public System.Collections.Generic.List<MinigameDialogue> minigameDialogues;
}
