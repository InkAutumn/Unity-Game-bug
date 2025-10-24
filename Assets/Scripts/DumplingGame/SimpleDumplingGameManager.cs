using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// 简化版包饺子游戏管理器 - 新版指针判定游戏
/// 管理游戏流程、计数、幸运饺子等
/// Unity 5兼容版本
/// </summary>
public class SimpleDumplingGameManager : MonoBehaviour
{
    [Header("Controllers")]
    public GameObject progressBarContainer;     // 进度条容器对象
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
    public Text countText;                // 计数文本（可选）
    
    [Header("Game Settings")]
    public int targetCount = 5;            // 目标饺子数量
    public MinigameDifficulty difficulty = MinigameDifficulty.Normal;
    public bool enableLuckyDumpling = false;
    public int luckyDumplingPosition = 5;
    
    [Header("Dialogue Settings")]
    public List<MinigameDialogue> minigameDialogues = new List<MinigameDialogue>(); // 游戏内对话配置
    
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
        // 获取组件引用
        InitializeComponents();
        
        // 从GameManager获取配置
        LoadConfigFromGameManager();
        
        // 初始化
        InitializeGame();
        
        // 触发游戏开始对话
        TriggerDialogue(MinigameDialogueTrigger.OnGameStart, 0);
    }
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void InitializeComponents()
    {
        // 获取ProgressBarController
        if (progressBarContainer != null)
        {
            progressBar = progressBarContainer.GetComponent<ProgressBarController>();
            if (progressBar == null)
            {
                Debug.LogError("[SimpleDumplingGame] ProgressBarContainer上未找到ProgressBarController组件！");
            }
        }
        
        // 获取LuckyCoinController
        if (luckyCoinObject != null)
        {
            luckyCoin = luckyCoinObject.GetComponent<LuckyCoinController>();
            if (luckyCoin == null)
            {
                Debug.LogError("[SimpleDumplingGame] LuckyCoin对象上未找到LuckyCoinController组件！");
            }
        }
        
        // 获取DumplingGameDialogueManager
        if (dialogueManagerObject != null)
        {
            dialogueManager = dialogueManagerObject.GetComponent<DumplingGameDialogueManager>();
            if (dialogueManager == null)
            {
                Debug.LogError("[SimpleDumplingGame] DialogueManager对象上未找到DumplingGameDialogueManager组件！");
            }
        }
    }
    
    void Update()
    {
        // 检测点击（准备阶段）
        if (currentPhase == GamePhase.Preparation)
        {
            if (InputHandler.GetInputDown())
            {
                StartGameplay();
            }
        }
    }
    
    /// <summary>
    /// 从GameManager加载配置
    /// </summary>
    private void LoadConfigFromGameManager()
    {
        // 简化版游戏使用Inspector中的默认配置
        // 如果需要从DialogueManager加载配置，可以在这里添加
        Debug.Log("[SimpleDumplingGame] 使用Inspector默认配置");
    }
    
    /// <summary>
    /// 初始化游戏
    /// </summary>
    private void InitializeGame()
    {
        // 重置状态
        completedCount = 0;
        dumplingsList.Clear();
        currentPhase = GamePhase.Preparation;
        waitingForCoin = false;
        
        // 显示准备阶段
        if (preparationPhase != null)
        {
            preparationPhase.SetActive(true);
        }
        if (gameplayPhase != null)
        {
            gameplayPhase.SetActive(false);
        }
        
        // 隐藏硬币
        if (luckyCoin != null)
        {
            luckyCoin.HideCoin();
        }
        
        // 清空饺子容器
        if (dumplingContainer != null)
        {
            foreach (Transform child in dumplingContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }
        
        // 更新计数显示
        UpdateCountText();
        
        Debug.Log(string.Format("[SimpleDumplingGame] 初始化完成 - 目标: {0}, 难度: {1}", targetCount, difficulty));
    }
    
    /// <summary>
    /// 开始游戏环节
    /// </summary>
    private void StartGameplay()
    {
        // 播放点击音效
        PlaySound(clickSound);
        
        // 检查是否是幸运饺子
        int nextIndex = completedCount + 1;
        if (enableLuckyDumpling && nextIndex == luckyDumplingPosition)
        {
            // 显示硬币，等待点击
            ShowLuckyCoin();
        }
        else
        {
            // 正常流程，进入游戏
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
            // 绑定点击回调
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
        Debug.Log("[SimpleDumplingGame] 硬币已点击，进入游戏");
        EnterGameplayPhase();
    }
    
    /// <summary>
    /// 进入游戏阶段
    /// </summary>
    private void EnterGameplayPhase()
    {
        currentPhase = GamePhase.Gameplay;
        
        // 切换界面
        if (preparationPhase != null)
        {
            preparationPhase.SetActive(false);
        }
        if (gameplayPhase != null)
        {
            gameplayPhase.SetActive(true);
        }
        
        // 设置难度并开始进度条游戏
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
            // 成功
            OnSuccess();
        }
        else
        {
            // 失败
            OnFail();
        }
    }
    
    /// <summary>
    /// 成功
    /// </summary>
    private void OnSuccess()
    {
        // 播放成功音效
        PlaySound(successSound);
        
        // 增加计数
        completedCount++;
        
        // 显示饺子
        AddDumplingToContainer();
        
        // 更新计数
        UpdateCountText();
        
        // 触发饺子完成对话
        bool hasDialogue = TriggerDialogue(MinigameDialogueTrigger.OnDumplingComplete, completedCount);
        
        // 检查是否完成所有
        if (completedCount >= targetCount)
        {
            CompleteGame();
        }
        else if (!hasDialogue)
        {
            // 如果没有对话，直接继续下一个
            ReturnToPreparation();
        }
        // 如果有对话，等对话完成后会自动继续
    }
    
    /// <summary>
    /// 失败
    /// </summary>
    private void OnFail()
    {
        // 播放失败音效
        PlaySound(failSound);
        
        // 触发失败对话
        bool hasDialogue = TriggerDialogue(MinigameDialogueTrigger.OnFail, completedCount);
        
        // 失败不计数，重新包这个饺子
        if (!hasDialogue)
        {
            ReturnToPreparation();
        }
        // 如果有对话，等对话完成后会自动继续
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
            
            Debug.Log(string.Format("[SimpleDumplingGame] 饺子已添加 - 总数: {0}", dumplingsList.Count));
        }
    }
    
    /// <summary>
    /// 返回准备阶段
    /// </summary>
    private void ReturnToPreparation()
    {
        currentPhase = GamePhase.Preparation;
        
        // 切换界面
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
        
        // 播放完成音效
        PlaySound(completeSound);
        
        Debug.Log("[SimpleDumplingGame] 游戏完成！");
        
        // 触发完成对话
        bool hasDialogue = TriggerDialogue(MinigameDialogueTrigger.OnAllComplete, completedCount);
        
        if (!hasDialogue)
        {
            // 如果没有对话，延迟后返回对话场景
            Invoke("ReturnToDialogue", 2f);
        }
        // 如果有对话，等对话完成后会自动返回
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
    /// 公共方法：设置游戏配置
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
    /// 触发对话（如果有配置）
    /// </summary>
    /// <returns>是否触发了对话</returns>
    private bool TriggerDialogue(MinigameDialogueTrigger trigger, int currentCount)
    {
        if (dialogueManager == null || minigameDialogues == null || minigameDialogues.Count == 0)
        {
            return false;
        }
        
        // 查找匹配的对话
        foreach (MinigameDialogue dialogue in minigameDialogues)
        {
            // 检查触发条件
            if (dialogue.trigger != trigger)
                continue;
            
            // 如果是OnDumplingComplete类型，检查计数匹配
            if (trigger == MinigameDialogueTrigger.OnDumplingComplete)
            {
                if (dialogue.triggerAtCount != -1 && dialogue.triggerAtCount != currentCount)
                    continue;
            }
            
            // 检查是否只播放一次（简化版：使用trigger和count作为键）
            string dialogueKey = string.Format("{0}_{1}", trigger.ToString(), currentCount);
            if (dialogue.playOnce)
            {
                if (HasPlayedDialogue(dialogueKey))
                    continue;
                
                // 标记为已播放
                MarkDialogueAsPlayed(dialogueKey);
            }
            
            // 触发对话
            Debug.Log(string.Format("[SimpleDumplingGame] 触发对话 - 时机:{0}, 计数:{1}", trigger, currentCount));
            
            if (dialogue.pauseGame)
            {
                currentPhase = GamePhase.WaitingDialogue;
            }
            
            // 显示对话（使用3参数版本）
            dialogueManager.ShowDialogue(
                dialogue.characterName,
                dialogue.dialogueText,
                dialogue.choices
            );
            
            // 等待对话完成后调用回调
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
        // 等待对话管理器的对话面板关闭
        while (dialogueManager != null && dialogueManager.dialoguePanel != null && dialogueManager.dialoguePanel.activeSelf)
        {
            yield return null;
        }
        
        // 对话完成，调用回调
        OnDialogueComplete();
    }
    
    // 播放追踪（简化实现）
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
        
        // 根据当前状态决定下一步
        if (currentPhase == GamePhase.Complete)
        {
            // 如果是完成阶段的对话，返回主场景
            Invoke("ReturnToDialogue", 1f);
        }
        else
        {
            // 其他情况返回准备阶段
            ReturnToPreparation();
        }
    }
}
