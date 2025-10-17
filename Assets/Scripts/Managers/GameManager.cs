using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Names")]
    public string dialogueSceneName = "DialogueScene";
    public string dumplingGameSceneName = "DumplingGameScene";

    private int returnNodeId = -1; // 从小游戏返回时要显示的对话节点

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
        // 保存当前对话状态
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            // 可以在这里保存要返回的节点ID
            // returnNodeId = 某个节点ID;
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
}
