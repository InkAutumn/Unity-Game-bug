using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public Text characterNameText;
    public Text dialogueText;
    public Image handsImage; // 手部图片（第一人称视角）
    public Image backgroundImage;
    public GameObject choicesPanel;
    public GameObject choiceButtonPrefab;
    public Image dialogueBoxImage; // 对话框背景图

    [Header("Dialogue Box Styles")]
    public Sprite narratorDialogueBox; // 旁白对话框（图一：无角色名）
    public Sprite characterDialogueBox; // 角色对话框（图二：有角色名）
    
    [Header("UI Buttons")]
    public Button backButton; // 返回主界面按钮（左上角）
    public Button loadButton; // 加载存档按钮（右上角）
    public Button historyButton; // 历史对话按钮（右上角）

    [Header("Settings")]
    public float textSpeed = 0.05f;

    private DialogueData currentDialogue;
    private DialogueNode currentNode;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Dictionary<string, bool> storyFlags = new Dictionary<string, bool>();
    private int currentNodeId = -1;

    public event Action OnMinigameTriggered;

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
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadButtonClicked);
        }
        
        if (historyButton != null)
        {
            historyButton.onClick.AddListener(OnHistoryButtonClicked);
        }
    }

    public void StartDialogue(DialogueData dialogue)
    {
        currentDialogue = dialogue;
        ShowNode(dialogue.startNodeId);
    }

    public void StartDialogueFromNode(int nodeId)
    {
        // 使用ChapterManager自动查找对应章节
        if (ChapterManager.Instance != null)
        {
            DialogueData chapter = ChapterManager.Instance.GetChapterByNodeId(nodeId);
            
            if (chapter != null)
            {
                currentDialogue = chapter;
                ShowNode(nodeId);
            }
            else
            {
                Debug.LogError("无法找到包含节点" + nodeId + "的章节！");
            }
        }
        else
        {
            Debug.LogError("ChapterManager未初始化！");
        }
    }
    
    public void StartNewGame()
    {
        // 清除所有剧情标记
        ClearAllFlags();
        
        // 获取第一章
        if (ChapterManager.Instance != null)
        {
            DialogueData firstChapter = ChapterManager.Instance.GetFirstChapter();
            
            if (firstChapter != null)
            {
                StartDialogue(firstChapter);
                Debug.Log("[DialogueManager] 开始新游戏");
            }
            else
            {
                Debug.LogError("无法开始新游戏：未找到第一章！");
            }
        }
        else
        {
            Debug.LogError("ChapterManager未初始化！");
        }
    }

    public void ShowNode(int nodeId)
    {
        // 尝试在当前章节查找节点
        DialogueNode node = null;
        if (currentDialogue != null)
        {
            node = currentDialogue.GetNode(nodeId);
        }
        
        // 如果当前章节找不到，尝试切换章节
        if (node == null && ChapterManager.Instance != null)
        {
            Debug.Log("[DialogueManager] 节点" + nodeId + "不在当前章节，尝试切换章节...");
            
            DialogueData newChapter = ChapterManager.Instance.GetChapterByNodeId(nodeId);
            if (newChapter != null)
            {
                currentDialogue = newChapter;
                node = currentDialogue.GetNode(nodeId);
                
                string chapterName = ChapterManager.Instance.GetChapterName(nodeId);
                Debug.Log("[DialogueManager] 已切换到章节：" + chapterName);
            }
        }
        
        currentNode = node;
        currentNodeId = nodeId;

        if (currentNode == null)
        {
            Debug.LogError("Node not found: " + nodeId);
            return;
        }

        // 更新角色名称和对话框样式
        bool isNarrator = string.IsNullOrEmpty(currentNode.characterName);
        
        if (characterNameText != null)
        {
            if (isNarrator)
            {
                characterNameText.gameObject.SetActive(false);
            }
            else
            {
                characterNameText.gameObject.SetActive(true);
                characterNameText.text = currentNode.characterName;
            }
        }
        
        // 切换对话框背景
        if (dialogueBoxImage != null)
        {
            if (isNarrator && narratorDialogueBox != null)
            {
                dialogueBoxImage.sprite = narratorDialogueBox;
            }
            else if (!isNarrator && characterDialogueBox != null)
            {
                dialogueBoxImage.sprite = characterDialogueBox;
            }
        }

        // 更新手部图片
        if (handsImage != null && !string.IsNullOrEmpty(currentNode.handsSprite))
        {
            Sprite sprite = Resources.Load<Sprite>("Hands/" + currentNode.handsSprite);
            if (sprite != null)
            {
                handsImage.sprite = sprite;
                handsImage.enabled = true;
            }
        }
        else if (handsImage != null)
        {
            handsImage.enabled = false;
        }

        // 更新背景
        if (backgroundImage != null && !string.IsNullOrEmpty(currentNode.backgroundImage))
        {
            Sprite bg = Resources.Load<Sprite>("Backgrounds/" + currentNode.backgroundImage);
            if (bg != null)
            {
                backgroundImage.sprite = bg;
            }
        }

        ClearChoices();
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(currentNode.dialogueText));
        
        CheckAutoSave();
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;

        // 添加到历史记录
        if (DialogueHistoryManager.Instance != null)
        {
            DialogueHistoryManager.Instance.AddDialogueEntry(currentNode.characterName, currentNode.dialogueText);
        }

        if (currentNode.triggerMinigame)
        {
            if (OnMinigameTriggered != null)
            {
                OnMinigameTriggered.Invoke();
            }
        }
        else if (currentNode.choices.Count > 0)
        {
            ShowChoices();
        }
        else if (currentNode.nextNodeId >= 0)
        {
        }
    }

    void ShowChoices()
    {
        ClearChoices();

        foreach (DialogueChoice choice in currentNode.choices)
        {
            if (!string.IsNullOrEmpty(choice.requirementFlag))
            {
                if (!storyFlags.ContainsKey(choice.requirementFlag) || !storyFlags[choice.requirementFlag])
                {
                    continue; 
                }
            }

            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            Button button = buttonObj.GetComponent<Button>();
            Text buttonText = buttonObj.GetComponentInChildren<Text>();

            buttonText.text = choice.choiceText;

            int targetId = choice.targetNodeId;
            button.onClick.AddListener(() => OnChoiceSelected(targetId));
        }

        choicesPanel.SetActive(true);
    }

    void ClearChoices()
    {
        foreach (Transform child in choicesPanel.transform)
        {
            Destroy(child.gameObject);
        }
        choicesPanel.SetActive(false);
    }

    void OnChoiceSelected(int targetNodeId)
    {
        ClearChoices();
        ShowNode(targetNodeId);
    }

    public void ContinueDialogue()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            dialogueText.text = currentNode.dialogueText;
            isTyping = false;

            if (currentNode.choices.Count > 0)
            {
                ShowChoices();
            }
        }
        else if (currentNode != null && currentNode.nextNodeId >= 0 && currentNode.choices.Count == 0)
        {
            ShowNode(currentNode.nextNodeId);
        }
    }

    public void SetStoryFlag(string flagName, bool value)
    {
        storyFlags[flagName] = value;
        
        if (value && AchievementManager.Instance != null)
        {
            AchievementManager.Instance.TryUnlockAchievement(flagName);
        }
    }

    public bool GetStoryFlag(string flagName)
    {
        return storyFlags.ContainsKey(flagName) && storyFlags[flagName];
    }

    public Dictionary<string, bool> GetAllStoryFlags()
    {
        return new Dictionary<string, bool>(storyFlags);
    }

    public void RestoreStoryFlags(Dictionary<string, bool> flags)
    {
        if (flags != null)
        {
            storyFlags = new Dictionary<string, bool>(flags);
        }
    }

    public void ClearAllFlags()
    {
        storyFlags.Clear();
    }

    public int GetCurrentNodeId()
    {
        return currentNodeId;
    }
    
    public DialogueNode GetCurrentNode()
    {
        return currentNode;
    }
    
    
    /// <summary>
    /// 返回主界面按钮点击
    /// </summary>
    private void OnBackButtonClicked()
    {
        Debug.Log("[DialogueManager] 返回主界面");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToDialogue(-1);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
    
    /// <summary>
    /// 加载存档按钮点击
    /// </summary>
    private void OnLoadButtonClicked()
    {
        Debug.Log("[DialogueManager] 加载最近存档");
        
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.LoadLatestSave();
        }
        else
        {
            Debug.LogWarning("[DialogueManager] SaveLoadManager未找到");
        }
    }
    
    /// <summary>
    /// 历史对话按钮点击
    /// </summary>
    private void OnHistoryButtonClicked()
    {
        Debug.Log("[DialogueManager] 打开历史对话");
        
        if (DialogueHistoryManager.Instance != null)
        {
            DialogueHistoryManager.Instance.ToggleHistoryPanel();
        }
        else
        {
            Debug.LogWarning("[DialogueManager] DialogueHistoryManager未找到");
        }
    }
    
    
    /// <summary>
    /// 检查并执行自动存档
    /// </summary>
    private void CheckAutoSave()
    {
        if (currentNode != null && currentNode.autoSaveAtThisNode)
        {
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.AutoSave();
                Debug.Log($"[DialogueManager] 在节点 {currentNodeId} 自动存档");
            }
        }
    }

    void Update()
    {
        bool inputDetected = Input.GetMouseButtonDown(0);

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputDetected = true;
        }
#endif

        if (inputDetected && !isTyping && currentNode != null && currentNode.choices.Count == 0)
        {
            ContinueDialogue();
        }
    }
}
