using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DumplingGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int targetDumplingCount = 5; // 目标饺子数量
    public float timeLimit = 60f; // 时间限制（秒）

    [Header("UI References")]
    public Text scoreText;
    public Text timerText;
    public GameObject gameOverPanel;
    public Text resultText;
    public Button returnButton;

    [Header("Game Objects")]
    public Transform workArea;
    public GameObject dumplingPrefab;

    private int currentScore = 0;
    private int dumplingsCompleted = 0;
    private float currentTime;
    private bool gameActive = false;

    // 当前包饺子的状态
    private bool hasDoughSkin = false;
    private bool hasFilling = false;

    void Start()
    {
        currentTime = timeLimit;
        gameActive = true;
        UpdateUI();

        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnToDialogue);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (gameActive)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                EndGame(false);
            }

            UpdateUI();
        }
    }

    public void OnItemPlaced(DraggableItem item, DropZone zone)
    {
        if (!gameActive) return;

        if (zone.zoneType == DropZone.ZoneType.WorkArea)
        {
            // 在工作区域放置物品
            if (item.itemType == DraggableItem.ItemType.DoughSkin)
            {
                hasDoughSkin = true;
                Debug.Log("放置饺子皮");
            }
            else if (item.itemType == DraggableItem.ItemType.Filling)
            {
                hasFilling = true;
                Debug.Log("放置馅料");
            }

            // 检查是否完成一个饺子
            CheckDumplingComplete();
        }
    }

    void CheckDumplingComplete()
    {
        if (hasDoughSkin && hasFilling)
        {
            // 完成一个饺子
            CompleteDumpling();

            // 重置状态
            hasDoughSkin = false;
            hasFilling = false;

            // 清理工作区
            ClearWorkArea();
        }
    }

    void CompleteDumpling()
    {
        dumplingsCompleted++;
        currentScore += 100;

        Debug.Log("完成饺子！总共: " + dumplingsCompleted);

        // 可以在这里生成完成的饺子视觉效果
        if (dumplingPrefab != null && workArea != null)
        {
            GameObject dumpling = Instantiate(dumplingPrefab, workArea);
            // 添加动画或效果
        }

        // 检查是否达到目标
        if (dumplingsCompleted >= targetDumplingCount)
        {
            EndGame(true);
        }
    }

    void ClearWorkArea()
    {
        // 清理工作区域的所有物品
        foreach (Transform child in workArea)
        {
            if (child.GetComponent<DraggableItem>() != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void EndGame(bool success)
    {
        gameActive = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (resultText != null)
        {
            if (success)
            {
                resultText.text = "太棒了！你完成了 " + dumplingsCompleted + " 个饺子！\n得分: " + currentScore;

                // 设置剧情标记
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.SetStoryFlag("dumplingGameComplete", true);
                }
            }
            else
            {
                resultText.text = "时间到！你完成了 " + dumplingsCompleted + " 个饺子。\n得分: " + currentScore;

                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.SetStoryFlag("dumplingGameFailed", true);
                }
            }
        }
    }

    void ReturnToDialogue()
    {
        // 返回到对话系统
        // 这里需要加载对话场景或重新激活对话UI
        UnityEngine.SceneManagement.SceneManager.LoadScene("DialogueScene");
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "饺子: " + dumplingsCompleted + "/" + targetDumplingCount + " | 得分: " + currentScore;
        }

        if (timerText != null)
        {
            timerText.text = "时间: " + Mathf.CeilToInt(currentTime) + "s";
        }
    }

    // 公共方法：重置游戏
    public void ResetGame()
    {
        currentScore = 0;
        dumplingsCompleted = 0;
        currentTime = timeLimit;
        gameActive = true;
        hasDoughSkin = false;
        hasFilling = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        ClearWorkArea();
        UpdateUI();
    }
}
