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
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnNewGame);
        }

        if (loadGameButton != null)
        {
            loadGameButton.onClick.AddListener(OnLoadGame);
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

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM("mainMenuBGM");
        }
    }

    void UpdateLoadGameButton()
    {
        if (loadGameButton == null) return;

        bool hasSaveFile = SaveLoadManager.Instance != null && SaveLoadManager.Instance.HasSaveFile();

        loadGameButton.interactable = hasSaveFile;

        Text buttonText = loadGameButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.color = hasSaveFile ? Color.white : Color.gray;
        }
    }

    void OnNewGame()
    {
        Debug.Log("开始新游戏");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.DeleteSave();
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ClearAllFlags();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.isStartingNewGame = true;
        }

        if (useTVIntro && !string.IsNullOrEmpty(tvIntroSceneName))
        {
            Debug.Log("加载电视机开场场景");
            SceneManager.LoadScene(tvIntroSceneName);
        }
        else
        {
            Debug.Log("直接加载对话场景");
            SceneManager.LoadScene(dialogueSceneName);
        }
    }

    void OnLoadGame()
    {
        Debug.Log("加载游戏");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

        if (SaveLoadManager.Instance != null)
        {
            SaveData data = SaveLoadManager.Instance.LoadGame();

            if (data != null)
            {
                SceneManager.LoadScene(dialogueSceneName);
            }
            else
            {
                Debug.LogWarning("加载存档失败");
            }
        }
    }

    void OnSettings()
    {
        Debug.Log("打开设置");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    void OnQuit()
    {
        Debug.Log("退出游戏");

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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

        if (achievementUIManager != null)
        {
            achievementUIManager.OpenAchievementPanel();
        }
        else
        {
            Debug.LogWarning("AchievementUIManager未设置！");
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}
