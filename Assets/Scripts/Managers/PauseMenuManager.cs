using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public enum PauseMenuType
    {
        Dialogue,   // 对话场景暂停菜单（包含历史记录）
        Minigame    // 小游戏暂停菜单（不包含历史记录）
    }

    public static PauseMenuManager Instance { get; private set; }

    [Header("Settings")]
    public PauseMenuType menuType = PauseMenuType.Dialogue;

    [Header("UI References")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button historyButton;        // 仅对话场景有效
    public Button settingsButton;
    public Button returnToMainButton;
    public Button menuToggleButton;     // 右上角菜单按钮（安卓）

    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject historyPanel;     // 历史对话记录面板

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;
    private float previousTimeScale = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 绑定按钮事件
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(Resume);
        }

        if (historyButton != null)
        {
            historyButton.onClick.AddListener(ShowHistory);
            // 根据菜单类型显示/隐藏历史按钮
            historyButton.gameObject.SetActive(menuType == PauseMenuType.Dialogue);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(ShowSettings);
        }

        if (returnToMainButton != null)
        {
            returnToMainButton.onClick.AddListener(ReturnToMain);
        }

        if (menuToggleButton != null)
        {
            menuToggleButton.onClick.AddListener(TogglePause);
        }

        // 初始隐藏所有面板
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (historyPanel != null)
        {
            historyPanel.SetActive(false);
        }
    }

    void Update()
    {
        // PC端：ESC键切换暂停
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("menuOpen");
        }

        Debug.Log("游戏暂停");
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = previousTimeScale;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // 关闭其他面板
        CloseAllSubPanels();

        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

        Debug.Log("继续游戏");
    }

    void ShowHistory()
    {
        if (menuType != PauseMenuType.Dialogue)
        {
            Debug.LogWarning("小游戏场景不支持历史记录！");
            return;
        }

        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

        // 隐藏暂停菜单，显示历史面板
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (historyPanel != null)
        {
            historyPanel.SetActive(true);
        }

        // 刷新历史记录内容
        if (DialogueHistoryManager.Instance != null)
        {
            DialogueHistoryManager.Instance.RefreshHistoryDisplay();
        }

        Debug.Log("显示历史对话记录");
    }

    void ShowSettings()
    {
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

        // 隐藏暂停菜单，显示设置面板
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

        Debug.Log("打开设置");
    }

    void ReturnToMain()
    {
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

        // 如果是小游戏场景，保存到小游戏前的对话节点
        if (menuType == PauseMenuType.Minigame)
        {
            SaveBeforeMinigame();
        }
        else
        {
            // 对话场景：保存当前进度
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.SaveGame();
            }
        }

        // 恢复时间缩放
        Time.timeScale = 1f;
        isPaused = false;

        // 返回主菜单
        SceneManager.LoadScene(mainMenuSceneName);

        Debug.Log("返回主菜单");
    }

    void SaveBeforeMinigame()
    {
        // 保存到小游戏触发前的对话节点
        if (SaveLoadManager.Instance != null && GameManager.Instance != null)
        {
            // 从GameManager获取小游戏前的节点ID
            int preMinigameNodeId = GameManager.Instance.GetPreMinigameNodeId();

            if (preMinigameNodeId >= 0)
            {
                SaveLoadManager.Instance.SaveGameAtNode(preMinigameNodeId);
                Debug.Log($"保存到小游戏前节点: {preMinigameNodeId}");
            }
            else
            {
                Debug.LogWarning("无法获取小游戏前节点ID，使用当前状态保存");
                SaveLoadManager.Instance.SaveGame();
            }
        }
    }

    // 关闭所有子面板，返回暂停菜单
    public void CloseAllSubPanels()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (historyPanel != null)
        {
            historyPanel.SetActive(false);
        }

        // 显示暂停菜单
        if (pauseMenuPanel != null && isPaused)
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    void OnDestroy()
    {
        // 恢复时间缩放
        if (isPaused)
        {
            Time.timeScale = 1f;
        }
    }

    // 公共方法：检查是否暂停
    public bool IsPaused()
    {
        return isPaused;
    }
}
