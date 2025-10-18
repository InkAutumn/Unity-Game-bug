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

    public void StartDialogue(DialogueData dialogue)
    {
        currentDialogue = dialogue;
        ShowNode(dialogue.startNodeId);
    }

    public void ShowNode(int nodeId)
    {
        currentNode = currentDialogue.GetNode(nodeId);
        currentNodeId = nodeId;

        if (currentNode == null)
        {
            Debug.LogError("Node not found: " + nodeId);
            return;
        }

        // 更新角色名称
        if (characterNameText != null)
        {
            characterNameText.text = currentNode.characterName;
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

        // 显示对话文本
        ClearChoices();
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(currentNode.dialogueText));
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

        // 文本显示完毕后，显示选项或自动继续
        if (currentNode.triggerMinigame)
        {
            // 触发小游戏
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
            // 等待玩家点击继续
            // 可以在Update中处理点击事件
        }
    }

    void ShowChoices()
    {
        ClearChoices();

        foreach (DialogueChoice choice in currentNode.choices)
        {
            // 检查是否满足条件
            if (!string.IsNullOrEmpty(choice.requirementFlag))
            {
                if (!storyFlags.ContainsKey(choice.requirementFlag) || !storyFlags[choice.requirementFlag])
                {
                    continue; // 跳过不满足条件的选项
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
            // 快进文本显示
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
    }

    public bool GetStoryFlag(string flagName)
    {
        return storyFlags.ContainsKey(flagName) && storyFlags[flagName];
    }

    // 新增：获取所有剧情标记
    public Dictionary<string, bool> GetAllStoryFlags()
    {
        return new Dictionary<string, bool>(storyFlags);
    }

    // 新增：恢复剧情标记
    public void RestoreStoryFlags(Dictionary<string, bool> flags)
    {
        if (flags != null)
        {
            storyFlags = new Dictionary<string, bool>(flags);
        }
    }

    // 新增：清除所有标记（新游戏时）
    public void ClearAllFlags()
    {
        storyFlags.Clear();
    }

    // 新增：获取当前节点ID
    public int GetCurrentNodeId()
    {
        return currentNodeId;
    }

    void Update()
    {
        // 点击屏幕或触摸继续对话（安卓适配）
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
