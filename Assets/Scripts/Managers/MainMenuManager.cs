using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button achievementButton;      // 留影册按钮
    public Button settingsButton;
    public Button quitButton;

    [Header("Panels")]
    public GameObject settingsPanel;

    [Header("Achievement System")]
    public AchievementUIManager achievementUIManager;  // 留影册UI管理器

    [Header("Scene Names")]
    public string tvIntroSceneName = "TVIntroScene";  // 电视机开场场景
    public string dialogueSceneName = "DialogueScene";
    public bool useTVIntro = true;  // 是否使用电视机开场

    void Start()
    {
        // 绑定按钮事件
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnNewGame);
        }

        if (loadGameButton != null)
        {
            loadGameButton.onClick.AddListener(OnLoadGame);
            // 检查是否存在存档，决定按钮是否可用
            UpdateLoadGameButton();
        }

        if (achievementButton != null)
        {
            achievementButton.onClick.AddListener(OnAchievement);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettings);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuit);
        }

        // 隐藏设置面板
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // 播放主菜单BGM
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM("mainMenuBGM");
        }
    }

    void UpdateLoadGameButton()
    {
        if (loadGameButton == null) return;

        // 检查是否存在存档
        bool hasSaveFile = SaveLoadManager.Instance != null && SaveLoadManager.Instance.HasSaveFile();

        // 设置按钮可交互状态
        loadGameButton.interactable = hasSaveFile;

        // 可选：改变按钮视觉效果
        Text buttonText = loadGameButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.color = hasSaveFile ? Color.white : Color.gray;
        }
    }

    void OnNewGame()
    {
        Debug.Log("开始新游戏");

        // 播放点击音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

        // 清除旧存档（可选：弹出确认对话框）
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.DeleteSave();
        }

        // 重置剧情标记
        if (DialogueManager.Instance != null)
        {
            // DialogueManager会在场景切换时保留，所以先清理
            DialogueManager.Instance.ClearAllFlags();
        }

        // ====== ChapterManager集成：设置新游戏标记 ======
        // 在场景切换后，DialogueScene的初始化脚本会检查这个标记
        // 然后调用DialogueManager.Instance.StartNewGame()
        // StartNewGame()会自动从ChapterManager获取第一章并开始游戏
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isStartingNewGame = true;
        }

        // 根据设置决定跳转到哪个场景
        if (useTVIntro && !string.IsNullOrEmpty(tvIntroSceneName))
        {
            // 使用电视机开场
            Debug.Log("加载电视机开场场景");
            SceneManager.LoadScene(tvIntroSceneName);
        }
        else
        {
            // 直接进入对话场景
            Debug.Log("直接加载对话场景");
            SceneManager.LoadScene(dialogueSceneName);
        }
    }

    void OnLoadGame()
    {
        Debug.Log("加载游戏");

        // 播放点击音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

        // 加载存档
        if (SaveLoadManager.Instance != null)
        {
            SaveData data = SaveLoadManager.Instance.LoadGame();

            if (data != null)
            {
                // 加载对话场景，然后恢复进度
                SceneManager.LoadScene(dialogueSceneName);
            }
            else
            {
                Debug.LogWarning("加载存档失败！");
            }
        }
    }

    void OnSettings()
    {
        Debug.Log("打开设置");

        // 播放点击音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

        // 显示设置面板
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    void OnQuit()
    {
        Debug.Log("退出游戏");

        // 播放点击音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnAchievement()
    {
        Debug.Log("打开留影册");

        // 播放点击音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

        // 打开留影册面板
        if (achievementUIManager != null)
        {
            achievementUIManager.OpenAchievementPanel();
        }
        else
        {
            Debug.LogWarning("AchievementUIManager未设置！");
        }
    }

    // 关闭设置面板（由SettingsPanel中的返回按钮调用）
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}
