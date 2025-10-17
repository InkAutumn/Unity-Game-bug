using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewDumplingGameManager : MonoBehaviour
{
    public enum GameMode
    {
        TargetCount,    // 目标数量模式：完成n个饺子后结束
        TimedMode       // 计时模式：时间结束后统计
    }

    [Header("Game Mode")]
    public GameMode gameMode = GameMode.TargetCount;
    public int targetDumplingCount = 5;      // 目标数量
    public float timeLimit = 60f;            // 时间限制

    [Header("References")]
    public DumplingMakingController makingController;

    [Header("UI")]
    public Text timerText;
    public Text scoreText;
    public Text perfectCountText;
    public Text goodCountText;
    public Text failedCountText;
    public GameObject scoreboard;
    public Text scoreboardTitle;
    public Text scoreboardDetails;
    public Button returnButton;

    [Header("Scoring")]
    public int perfectScore = 100;
    public int goodScore = 50;
    public int failedScore = 10;

    private float currentTime;
    private bool gameActive = false;

    // 统计数据
    private int perfectDumplings = 0;
    private int goodDumplings = 0;
    private int failedDumplings = 0;
    private int totalScore = 0;

    private Dictionary<DumplingQuality, int> qualityCount = new Dictionary<DumplingQuality, int>();

    void Start()
    {
        InitializeGame();

        if (makingController != null)
        {
            makingController.OnDumplingCompleted += OnDumplingCompleted;
        }

        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnToDialogue);
        }

        if (scoreboard != null)
        {
            scoreboard.SetActive(false);
        }
    }

    void InitializeGame()
    {
        currentTime = timeLimit;
        gameActive = true;

        perfectDumplings = 0;
        goodDumplings = 0;
        failedDumplings = 0;
        totalScore = 0;

        qualityCount.Clear();
        qualityCount[DumplingQuality.Perfect] = 0;
        qualityCount[DumplingQuality.Good] = 0;
        qualityCount[DumplingQuality.TooLittleWater] = 0;
        qualityCount[DumplingQuality.TooMuchWater] = 0;

        UpdateUI();
    }

    void Update()
    {
        if (!gameActive) return;

        // 计时模式或有时间限制时更新计时器
        if (gameMode == GameMode.TimedMode || timeLimit > 0)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                EndGame();
            }

            UpdateTimerUI();
        }
    }

    void OnDumplingCompleted(DumplingQuality quality)
    {
        // 统计饺子
        switch (quality)
        {
            case DumplingQuality.Perfect:
                perfectDumplings++;
                totalScore += perfectScore;
                break;

            case DumplingQuality.Good:
                goodDumplings++;
                totalScore += goodScore;
                break;

            case DumplingQuality.TooLittleWater:
            case DumplingQuality.TooMuchWater:
                failedDumplings++;
                totalScore += failedScore;
                break;
        }

        qualityCount[quality]++;

        UpdateUI();

        // 检查是否达到目标
        if (gameMode == GameMode.TargetCount)
        {
            int totalDumplings = perfectDumplings + goodDumplings + failedDumplings;
            if (totalDumplings >= targetDumplingCount)
            {
                EndGame();
            }
        }

        // 播放音效
        if (AudioManager.Instance != null)
        {
            if (quality == DumplingQuality.Perfect)
            {
                AudioManager.Instance.PlaySFX("dumplingComplete");
            }
            else
            {
                AudioManager.Instance.PlaySFX("itemPlace");
            }
        }
    }

    void EndGame()
    {
        gameActive = false;

        // 显示计分板
        ShowScoreboard();

        // 根据表现设置不同的剧情标记
        SetStoryFlagsBasedOnPerformance();
    }

    void ShowScoreboard()
    {
        if (scoreboard == null) return;

        scoreboard.SetActive(true);

        int totalDumplings = perfectDumplings + goodDumplings + failedDumplings;

        // 设置标题
        if (scoreboardTitle != null)
        {
            if (gameMode == GameMode.TargetCount)
            {
                scoreboardTitle.text = "完成！";
            }
            else
            {
                scoreboardTitle.text = "时间到！";
            }
        }

        // 设置详细信息
        if (scoreboardDetails != null)
        {
            string details = string.Format(
                "完成饺子：{0}个\n\n" +
                "完美：{1}个 (+{2}分)\n" +
                "良好：{3}个 (+{4}分)\n" +
                "失败：{5}个 (+{6}分)\n\n" +
                "总分：{7}分",
                totalDumplings,
                perfectDumplings, perfectDumplings * perfectScore,
                goodDumplings, goodDumplings * goodScore,
                failedDumplings, failedDumplings * failedScore,
                totalScore
            );

            scoreboardDetails.text = details;
        }
    }

    void SetStoryFlagsBasedOnPerformance()
    {
        if (DialogueManager.Instance == null) return;

        int totalDumplings = perfectDumplings + goodDumplings + failedDumplings;

        // 基础完成标记
        DialogueManager.Instance.SetStoryFlag("dumplingGameCompleted", true);

        // 根据表现设置不同标记
        float perfectRate = totalDumplings > 0 ? (float)perfectDumplings / totalDumplings : 0f;

        if (perfectRate >= 0.8f)
        {
            // 80%以上完美
            DialogueManager.Instance.SetStoryFlag("dumplingExcellent", true);
            Debug.Log("剧情标记：优秀表现");
        }
        else if (perfectRate >= 0.5f)
        {
            // 50%以上完美
            DialogueManager.Instance.SetStoryFlag("dumplingGood", true);
            Debug.Log("剧情标记：良好表现");
        }
        else if (failedDumplings > perfectDumplings + goodDumplings)
        {
            // 失败多于成功
            DialogueManager.Instance.SetStoryFlag("dumplingPoor", true);
            Debug.Log("剧情标记：需要改进");
        }
        else
        {
            // 普通表现
            DialogueManager.Instance.SetStoryFlag("dumplingAverage", true);
            Debug.Log("剧情标记：普通表现");
        }

        // 目标数量模式特殊判定
        if (gameMode == GameMode.TargetCount)
        {
            if (totalDumplings >= targetDumplingCount)
            {
                DialogueManager.Instance.SetStoryFlag("targetReached", true);
            }
        }

        // 计时模式特殊判定
        if (gameMode == GameMode.TimedMode)
        {
            if (totalDumplings >= 10)
            {
                DialogueManager.Instance.SetStoryFlag("highProduction", true);
                Debug.Log("剧情标记：高产量");
            }

            // 保存分数供对话系统读取
            PlayerPrefs.SetInt("LastGameScore", totalScore);
            PlayerPrefs.SetInt("LastGamePerfect", perfectDumplings);
            PlayerPrefs.SetInt("LastGameTotal", totalDumplings);
        }

        // 特殊成就
        if (perfectDumplings >= 5 && failedDumplings == 0)
        {
            DialogueManager.Instance.SetStoryFlag("noMistakes", true);
            Debug.Log("剧情标记：零失误！");
        }
    }

    void UpdateUI()
    {
        int totalDumplings = perfectDumplings + goodDumplings + failedDumplings;

        if (scoreText != null)
        {
            if (gameMode == GameMode.TargetCount)
            {
                scoreText.text = string.Format("进度: {0}/{1} | 得分: {2}",
                    totalDumplings, targetDumplingCount, totalScore);
            }
            else
            {
                scoreText.text = string.Format("数量: {0} | 得分: {1}",
                    totalDumplings, totalScore);
            }
        }

        if (perfectCountText != null)
        {
            perfectCountText.text = "完美: " + perfectDumplings;
        }

        if (goodCountText != null)
        {
            goodCountText.text = "良好: " + goodDumplings;
        }

        if (failedCountText != null)
        {
            failedCountText.text = "失败: " + failedDumplings;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("时间: {0:00}:{1:00}", minutes, seconds);
        }
    }

    void ReturnToDialogue()
    {
        // 根据游戏结果决定返回到哪个对话节点
        int returnNodeId = DetermineReturnNode();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToDialogue(returnNodeId);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DialogueScene");
        }
    }

    int DetermineReturnNode()
    {
        // 根据表现决定返回哪个对话节点
        // 这个逻辑可以根据你的对话设计调整

        float perfectRate = (perfectDumplings + goodDumplings + failedDumplings) > 0 ?
            (float)perfectDumplings / (perfectDumplings + goodDumplings + failedDumplings) : 0f;

        if (perfectRate >= 0.8f)
        {
            return 100; // 优秀表现的对话节点
        }
        else if (perfectRate >= 0.5f)
        {
            return 101; // 良好表现的对话节点
        }
        else
        {
            return 102; // 需要改进的对话节点
        }
    }

    // 公共方法：设置游戏模式（从对话系统调用）
    public void SetGameMode(GameMode mode, int targetCount = 5, float timeLimitSeconds = 60f)
    {
        gameMode = mode;
        targetDumplingCount = targetCount;
        timeLimit = timeLimitSeconds;
        InitializeGame();
    }

    // 公共方法：重置游戏
    public void ResetGame()
    {
        InitializeGame();

        if (scoreboard != null)
        {
            scoreboard.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (makingController != null)
        {
            makingController.OnDumplingCompleted -= OnDumplingCompleted;
        }
    }
}
