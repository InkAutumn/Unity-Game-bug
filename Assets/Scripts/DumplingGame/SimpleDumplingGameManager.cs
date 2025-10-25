using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
///包饺子游戏管理器
/// </summary>
public class SimpleDumplingGameManager : MonoBehaviour
{
    [Header("Controllers")]
    public GameObject progressBarHolder;        // 进度条主容器对象
    public GameObject luckyCoinObject;          // 幸运硬币对象
    public GameObject dialogueManagerObject;    // 对话管理器对象
    
    // 实际的控制器组件（自动获取）
    private ProgressBarController progressBar;
    private LuckyCoinController luckyCoin;
    private DumplingGameDialogueManager dialogueManager;
    
    [Header("UI Elements")]
    public GameObject preparationPhase;   // 阶段1准备界面
    public GameObject gameplayPhase;      // 阶段2游戏界面
    public GameObject dumplingContainer;  // 左侧饺子显示区域
    public GameObject dumplingPrefab;     // 饺子预制体
    public Text countText;                // 计数文本
    
    [Header("Background Elements")]
    public UnityEngine.UI.Image sceneBackground;  // 场景背景图
    public UnityEngine.UI.Image dialogueBoxBackground;  // 对话框背景图
    
    [Header("Game Settings")]
    public int targetCount = 5;            // 目标饺子数量
    public MinigameDifficulty difficulty = MinigameDifficulty.Normal;
    public bool enableLuckyDumpling = false;
    public int luckyDumplingPosition = 5;
    
    [Header("Default Backgrounds (按难度)")]
    public Sprite easyBackground;          // 童年背景
    public Sprite normalBackground;        // 青春期背景
    public Sprite hardBackground;          // 工作期背景
    public Sprite easyDialogueBox;         // 童年对话框背景
    public Sprite normalDialogueBox;       // 青春期对话框背景
    public Sprite hardDialogueBox;         // 工作期对话框背景
    public Sprite easyChoiceButton;        // 童年选项按钮背景
    public Sprite normalChoiceButton;      // 青春期选项按钮背景
    public Sprite hardChoiceButton;        // 工作期选项按钮背景
    
    [Header("Dialogue Settings")]
    public List<MinigameDialogue> minigameDialogues = new List<MinigameDialogue>(); 
    
    [Header("Audio")]
    public string clickSound = "click";
    public string successSound = "success";
    public string failSound = "fail";
    public string completeSound = "complete";
    
    // 游戏状态
    private GamePhase currentPhase = GamePhase.Preparation;
    private int completedCount = 0;
    private List<GameObject> dumplingsList = new List<GameObject>();
    private bool waitingForCoin = false;
    
    private enum GamePhase
    {
        Preparation,   // 准备阶段
        Gameplay,      // 游戏中
        WaitingCoin,   // 等待点击硬币
        WaitingDialogue, // 等待对话完成
        Complete       // 完成
    }
    
    void Start()
    {
        InitializeComponents();
        
        LoadConfigFromGameManager();
        
        InitializeGame();
        
        TriggerDialogue(MinigameDialogueTrigger.OnGameStart, 0);
    }
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void InitializeComponents()
    {
        if (progressBarHolder != null)
        {
            progressBar = progressBarHolder.GetComponent<ProgressBarController>();
            if (progressBar == null)
            {
                Debug.LogError("[SimpleDumplingGame] ProgressBarHolder上未找到ProgressBarController组件");
            }
            else
            {
                Debug.Log("[SimpleDumplingGame] 成功获取ProgressBarController组件");
            }
        }
        else
        {
            Debug.LogError("[SimpleDumplingGame] progressBarHolder引用为空");
        }
        
        if (luckyCoinObject != null)
        {
            luckyCoin = luckyCoinObject.GetComponent<LuckyCoinController>();
            if (luckyCoin == null)
            {
                Debug.LogError("[SimpleDumplingGame] LuckyCoin对象上未找到LuckyCoinController组件");
            }
        }
        
        if (dialogueManagerObject != null)
        {
            dialogueManager = dialogueManagerObject.GetComponent<DumplingGameDialogueManager>();
            if (dialogueManager == null)
            {
                Debug.LogError("[SimpleDumplingGame] DialogueManager对象上未找到DumplingGameDialogueManager组件");
            }
        }
    }
    
    void Update()
    {
        if (currentPhase == GamePhase.Preparation && !IsWaitingForDialogueChoice())
        {
            if (InputHandler.GetInputDown())
            {
                StartGameplay();
            }
        }
    }
    
    /// <summary>
    /// 检查是否正在等待对话选择
    /// </summary>
    private bool IsWaitingForDialogueChoice()
    {
        if (dialogueManager == null) return false;
        
        if (dialogueManager.dialoguePanel != null && dialogueManager.dialoguePanel.activeSelf)
        {
            if (dialogueManager.choicesPanel != null && dialogueManager.choicesPanel.activeSelf)
            {
                return true; 
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 从GameManager加载配置
    /// </summary>
    private void LoadConfigFromGameManager()
    {
        if (GameManager.Instance != null)
        {
            MinigameConfig config = GameManager.Instance.GetMinigameConfig();
            if (config != null)
            {
                targetCount = config.targetCount;
                difficulty = config.difficulty;
                enableLuckyDumpling = config.enableLuckyDumpling;
                luckyDumplingPosition = config.luckyDumplingPosition;
                
                if (config.minigameDialogues != null && config.minigameDialogues.Count > 0)
                {
                    minigameDialogues = config.minigameDialogues;
                }
                
                ApplyBackground(config.backgroundImage, config.dialogueBoxBackground);
                
                ApplyChoiceButtonBackground();
                
                Debug.Log($"[SimpleDumplingGame] 从GameManager加载配置 - 难度: {difficulty}, 目标: {targetCount}");
                return;
            }
        }
        
        Debug.Log("[SimpleDumplingGame] 使用Inspector默认配置");
        
        ApplyBackground("", "");
        
        ApplyChoiceButtonBackground();
    }
    
    /// <summary>
    /// 初始化游戏
    /// </summary>
    private void InitializeGame()
    {
        completedCount = 0;
        dumplingsList.Clear();
        currentPhase = GamePhase.Preparation;
        waitingForCoin = false;
        
        if (preparationPhase != null)
        {
            preparationPhase.SetActive(true);
        }
        if (gameplayPhase != null)
        {
            gameplayPhase.SetActive(false);
        }
        
        if (luckyCoin != null)
        {
            luckyCoin.HideCoin();
        }
        
        if (dumplingContainer != null)
        {
            foreach (Transform child in dumplingContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }
        
        UpdateCountText();
        
        Debug.Log(string.Format("[SimpleDumplingGame] 初始化完成 - 目标: {0}, 难度: {1}", targetCount, difficulty));
    }
    
    /// <summary>
    /// 开始游戏环节
    /// </summary>
    private void StartGameplay()
    {
        PlaySound(clickSound);
        
        int nextIndex = completedCount + 1;
        if (enableLuckyDumpling && nextIndex == luckyDumplingPosition)
        {
            ShowLuckyCoin();
        }
        else
        {
            EnterGameplayPhase();
        }
    }
    
    /// <summary>
    /// 显示幸运硬币
    /// </summary>
    private void ShowLuckyCoin()
    {
        currentPhase = GamePhase.WaitingCoin;
        
        if (luckyCoin != null)
        {
            luckyCoin.OnCoinClicked = OnCoinClicked;
            luckyCoin.ShowCoin();
        }
        
        Debug.Log("[SimpleDumplingGame] 等待点击硬币");
    }
    
    /// <summary>
    /// 硬币被点击
    /// </summary>
    private void OnCoinClicked()
    {
        Debug.Log("[SimpleDumplingGame] 硬币已点击");
        EnterGameplayPhase();
    }
    
    /// <summary>
    /// 进入游戏阶段
    /// </summary>
    private void EnterGameplayPhase()
    {
        currentPhase = GamePhase.Gameplay;
        
        if (preparationPhase != null)
        {
            preparationPhase.SetActive(false);
        }
        if (gameplayPhase != null)
        {
            gameplayPhase.SetActive(true);
        }
        
        if (progressBar != null)
        {
            progressBar.SetDifficulty(difficulty);
            progressBar.OnGameComplete = OnGameplayComplete;
            progressBar.StartGame();
        }
        
        Debug.Log("[SimpleDumplingGame] 进入游戏阶段");
    }
    
    /// <summary>
    /// 游戏环节完成回调
    /// </summary>
    private void OnGameplayComplete(bool success)
    {
        Debug.Log(string.Format("[SimpleDumplingGame] 游戏完成 - 成功: {0}", success));
        
        if (success)
        {
            OnSuccess();
        }
        else
        {
            OnFail();
        }
    }
    
    /// <summary>
    /// 成功
    /// </summary>
    private void OnSuccess()
    {
        PlaySound(successSound);
        
        completedCount++;
        
        AddDumplingToContainer();
        
        UpdateCountText();
        
        bool hasDialogue = TriggerDialogue(MinigameDialogueTrigger.OnDumplingComplete, completedCount);
        
        if (completedCount >= targetCount)
        {
            CompleteGame();
        }
        else if (!hasDialogue)
        {
            ReturnToPreparation();
        }
    }
    
    /// <summary>
    /// 失败
    /// </summary>
    private void OnFail()
    {
        PlaySound(failSound);
        
        bool hasDialogue = TriggerDialogue(MinigameDialogueTrigger.OnFail, completedCount);
        
        if (!hasDialogue)
        {
            ReturnToPreparation();
        }
    }
    
    /// <summary>
    /// 添加饺子到容器
    /// </summary>
    private void AddDumplingToContainer()
    {
        if (dumplingContainer != null && dumplingPrefab != null)
        {
            GameObject dumpling = Instantiate(dumplingPrefab, dumplingContainer.transform);
            dumplingsList.Add(dumpling);
            
            Debug.Log(string.Format("[SimpleDumplingGame] 饺子已添加 ， 总数: {0}", dumplingsList.Count));
        }
    }
    
    /// <summary>
    /// 返回准备阶段
    /// </summary>
    private void ReturnToPreparation()
    {
        currentPhase = GamePhase.Preparation;
        
        if (preparationPhase != null)
        {
            preparationPhase.SetActive(true);
        }
        if (gameplayPhase != null)
        {
            gameplayPhase.SetActive(false);
        }
        
        Debug.Log("[SimpleDumplingGame] 返回准备阶段");
    }
    
    /// <summary>
    /// 完成游戏
    /// </summary>
    private void CompleteGame()
    {
        currentPhase = GamePhase.Complete;
        
        PlaySound(completeSound);
        
        Debug.Log("[SimpleDumplingGame] 游戏完成！");
        
        bool hasDialogue = TriggerDialogue(MinigameDialogueTrigger.OnAllComplete, completedCount);
        
        if (!hasDialogue)
        {
            Invoke("ReturnToDialogue", 2f);
        }
    }
    
    /// <summary>
    /// 返回对话场景
    /// </summary>
    private void ReturnToDialogue()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToDialogue();
        }
        else
        {
            SceneManager.LoadScene("DialogueScene");
        }
    }
    
    /// <summary>
    /// 更新计数文本
    /// </summary>
    private void UpdateCountText()
    {
        if (countText != null)
        {
            countText.text = string.Format("{0}/{1}", completedCount, targetCount);
        }
    }
    
    /// <summary>
    /// 播放音效
    /// </summary>
    private void PlaySound(string soundName)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(soundName);
        }
    }
    
    /// <summary>
    /// 设置游戏配置
    /// </summary>
    public void SetGameConfig(int target, MinigameDifficulty diff, bool enableLucky, int luckyPos)
    {
        targetCount = target;
        difficulty = diff;
        enableLuckyDumpling = enableLucky;
        luckyDumplingPosition = luckyPos;
        
        Debug.Log(string.Format("[SimpleDumplingGame] 配置已更新 - 目标: {0}, 难度: {1}, 幸运饺子: {2} (位置: {3})",
            targetCount, difficulty, enableLuckyDumpling, luckyDumplingPosition));
    }
    
    /// <summary>
    /// 触发对话
    /// </summary>
    private bool TriggerDialogue(MinigameDialogueTrigger trigger, int currentCount)
    {
        if (dialogueManager == null || minigameDialogues == null || minigameDialogues.Count == 0)
        {
            return false;
        }
        
        foreach (MinigameDialogue dialogue in minigameDialogues)
        {
            if (dialogue.trigger != trigger)
                continue;
            
            if (trigger == MinigameDialogueTrigger.OnDumplingComplete)
            {
                if (dialogue.triggerAtCount != -1 && dialogue.triggerAtCount != currentCount)
                    continue;
            }
            
            string dialogueKey = string.Format("{0}_{1}", trigger.ToString(), currentCount);
            if (dialogue.playOnce)
            {
                if (HasPlayedDialogue(dialogueKey))
                    continue;
                
                MarkDialogueAsPlayed(dialogueKey);
            }
            
            Debug.Log(string.Format("[SimpleDumplingGame] 触发对话 时机:{0}, 计数:{1}", trigger, currentCount));
            
            if (dialogue.pauseGame)
            {
                currentPhase = GamePhase.WaitingDialogue;
            }
            
            dialogueManager.ShowDialogue(
                dialogue.characterName,
                dialogue.dialogueText,
                dialogue.choices,
                null,
                dialogue.customDialogueBoxBackground,
                dialogue.customChoiceButtonBackground
            );
            
            StartCoroutine(WaitForDialogueComplete());
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 等待对话完成
    /// </summary>
    private System.Collections.IEnumerator WaitForDialogueComplete()
    {
        while (dialogueManager != null && dialogueManager.dialoguePanel != null && dialogueManager.dialoguePanel.activeSelf)
        {
            yield return null;
        }
        
        OnDialogueComplete();
    }
    
    // 播放追踪
    private System.Collections.Generic.HashSet<string> playedDialogues = new System.Collections.Generic.HashSet<string>();
    
    private bool HasPlayedDialogue(string key)
    {
        return playedDialogues.Contains(key);
    }
    
    private void MarkDialogueAsPlayed(string key)
    {
        if (!playedDialogues.Contains(key))
        {
            playedDialogues.Add(key);
        }
    }
    
    /// <summary>
    /// 对话完成回调
    /// </summary>
    private void OnDialogueComplete()
    {
        Debug.Log("[SimpleDumplingGame] 对话完成，继续游戏");
        
        if (currentPhase == GamePhase.Complete)
        {
            Invoke("ReturnToDialogue", 1f);
        }
        else
        {
            ReturnToPreparation();
        }
    }
    
    /// <summary>
    /// 应用背景图
    /// </summary>
    private void ApplyBackground(string backgroundImageName, string dialogueBoxBackgroundName)
    {
        if (sceneBackground != null)
        {
            Sprite bgSprite = null;
            
            if (!string.IsNullOrEmpty(backgroundImageName))
            {
                bgSprite = Resources.Load<Sprite>("Backgrounds/" + backgroundImageName);
                if (bgSprite == null)
                {
                    Debug.LogWarning($"[SimpleDumplingGame] 未找到背景图: {backgroundImageName}，使用默认背景");
                }
            }
            
            if (bgSprite == null)
            {
                bgSprite = GetDefaultBackground(difficulty);
            }
            
            if (bgSprite != null)
            {
                sceneBackground.sprite = bgSprite;
                Debug.Log($"[SimpleDumplingGame] 场景背景已应用");
            }
        }
        
        if (dialogueBoxBackground != null)
        {
            Sprite dialogueBgSprite = null;
            
            if (!string.IsNullOrEmpty(dialogueBoxBackgroundName))
            {
                dialogueBgSprite = Resources.Load<Sprite>("Backgrounds/" + dialogueBoxBackgroundName);
                if (dialogueBgSprite == null)
                {
                    Debug.LogWarning($"[SimpleDumplingGame] 未找到对话框背景图: {dialogueBoxBackgroundName}，使用默认背景");
                }
            }
            
            if (dialogueBgSprite == null)
            {
                dialogueBgSprite = GetDefaultDialogueBoxBackground(difficulty);
            }
            
            if (dialogueBgSprite != null)
            {
                dialogueBoxBackground.sprite = dialogueBgSprite;
                Debug.Log($"[SimpleDumplingGame] 对话框背景已应用");
            }
        }
    }
    
    /// <summary>
    /// 根据难度获取默认场景背景
    /// </summary>
    private Sprite GetDefaultBackground(MinigameDifficulty diff)
    {
        switch (diff)
        {
            case MinigameDifficulty.Easy:
                return easyBackground;
            case MinigameDifficulty.Normal:
                return normalBackground;
            case MinigameDifficulty.Hard:
                return hardBackground;
            default:
                return normalBackground;
        }
    }
    
    /// <summary>
    /// 根据难度获取默认对话框背景
    /// </summary>
    private Sprite GetDefaultDialogueBoxBackground(MinigameDifficulty diff)
    {
        switch (diff)
        {
            case MinigameDifficulty.Easy:
                return easyDialogueBox;
            case MinigameDifficulty.Normal:
                return normalDialogueBox;
            case MinigameDifficulty.Hard:
                return hardDialogueBox;
            default:
                return normalDialogueBox;
        }
    }
    
    /// <summary>
    /// 根据难度获取选项按钮背景
    /// </summary>
    private Sprite GetChoiceButtonBackground(MinigameDifficulty diff)
    {
        switch (diff)
        {
            case MinigameDifficulty.Easy:
                return easyChoiceButton;
            case MinigameDifficulty.Normal:
                return normalChoiceButton;
            case MinigameDifficulty.Hard:
                return hardChoiceButton;
            default:
                return normalChoiceButton;
        }
    }
    
    /// <summary>
    /// 应用选项按钮背景到对话管理器
    /// </summary>
    private void ApplyChoiceButtonBackground()
    {
        if (dialogueManager != null)
        {
            Sprite choiceButtonBg = GetChoiceButtonBackground(difficulty);
            if (choiceButtonBg != null)
            {
                dialogueManager.SetChoiceButtonBackground(choiceButtonBg);
                Debug.Log($"[SimpleDumplingGame] 选项按钮背景已设置");
            }
        }
    }
}
