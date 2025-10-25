using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 包饺子游戏内对话管理器
/// </summary>
public class DumplingGameDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public Text characterNameText;
    public Text dialogueText;
    public GameObject choicesPanel;
    public GameObject choiceButtonPrefab;
    public Image dialogueBoxImage; // 对话框背景图
    
    [Header("Dialogue Box Styles")]
    public Sprite narratorDialogueBox; // 旁白对话框
    public Sprite characterDialogueBox; // 角色对话框
    
    [Header("Settings")]
    public float textSpeed = 0.05f;
    
    private bool isDialogueActive = false;
    private string currentDialogue = "";
    private int currentCharIndex = 0;
    private List<DialogueChoice> currentChoices;
    private System.Action onDialogueComplete; // 对话完成回调
    private Sprite choiceButtonBackground; // 选项按钮背景图
    private Sprite currentDialogueBoxBg; // 当前对话框背景
    
    // 追踪已播放的对话（避免重复）
    private HashSet<string> playedDialogues = new HashSet<string>();
    
    void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 显示对话
    /// </summary>
    public void ShowDialogue(string characterName, string dialogue, List<DialogueChoice> choices = null, System.Action onComplete = null)
    {
        ShowDialogue(characterName, dialogue, choices, onComplete, null, null);
    }
    
    /// <summary>
    /// 显示对话
    /// </summary>
    public void ShowDialogue(string characterName, string dialogue, List<DialogueChoice> choices, System.Action onComplete, string customDialogueBoxBg, string customChoiceButtonBg)
    {
        isDialogueActive = true;
        onDialogueComplete = onComplete;
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        bool isNarrator = string.IsNullOrEmpty(characterName);
        
        // 显示/隐藏角色名
        if (characterNameText != null)
        {
            if (isNarrator)
            {
                characterNameText.gameObject.SetActive(false);
            }
            else
            {
                characterNameText.gameObject.SetActive(true);
                characterNameText.text = characterName;
            }
        }
        
        // 切换对话框背景
        if (dialogueBoxImage != null)
        {
            Sprite dialogueBg = null;
            
            if (!string.IsNullOrEmpty(customDialogueBoxBg))
            {
                dialogueBg = Resources.Load<Sprite>("Backgrounds/" + customDialogueBoxBg);
                if (dialogueBg == null)
                {
                    Debug.LogWarning($"[DumplingGameDialogue] 未找到自定义对话框背景: {customDialogueBoxBg}");
                }
            }
            
            if (dialogueBg == null)
            {
                if (isNarrator && narratorDialogueBox != null)
                {
                    dialogueBg = narratorDialogueBox;
                }
                else if (!isNarrator && characterDialogueBox != null)
                {
                    dialogueBg = characterDialogueBox;
                }
            }
            
            if (dialogueBg != null)
            {
                dialogueBoxImage.sprite = dialogueBg;
                currentDialogueBoxBg = dialogueBg;
            }
        }
        
        if (!string.IsNullOrEmpty(customChoiceButtonBg))
        {
            Sprite customBg = Resources.Load<Sprite>("Backgrounds/" + customChoiceButtonBg);
            if (customBg != null)
            {
                choiceButtonBackground = customBg;
            }
        }
        
        currentDialogue = dialogue;
        currentCharIndex = 0;
        currentChoices = choices;
        
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
        
        InvokeRepeating("TypeNextCharacter", 0f, textSpeed);
        
        if (choices != null && choices.Count > 0)
        {
            float delayTime = dialogue.Length * textSpeed + 0.2f;
            Invoke("ShowChoices", delayTime);
        }
    }
    
    void TypeNextCharacter()
    {
        if (currentCharIndex < currentDialogue.Length)
        {
            if (dialogueText != null)
            {
                dialogueText.text += currentDialogue[currentCharIndex];
            }
            currentCharIndex++;
        }
        else
        {
            CancelInvoke("TypeNextCharacter");
        }
    }
    
    void ShowChoices()
    {
        if (choicesPanel == null || currentChoices == null || currentChoices.Count == 0)
            return;
        
        foreach (Transform child in choicesPanel.transform)
        {
            Destroy(child.gameObject);
        }
        
        choicesPanel.SetActive(true);
        
        foreach (DialogueChoice choice in currentChoices)
        {
            if (choiceButtonPrefab == null)
            {
                Debug.LogError("[DumplingGameDialogue] 选项按钮预制体未设置！");
                continue;
            }
            
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            Button button = buttonObj.GetComponent<Button>();
            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            
            if (buttonText != null)
            {
                buttonText.text = choice.choiceText;
            }
            
            if (choiceButtonBackground != null)
            {
                Image buttonImage = buttonObj.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = choiceButtonBackground;
                }
            }
            
            if (button != null)
            {
                DialogueChoice selectedChoice = choice;
                button.onClick.AddListener(() => OnChoiceSelected(selectedChoice));
            }
        }
    }
    
    void OnChoiceSelected(DialogueChoice choice)
    {
        Debug.Log("[DumplingGameDialogue] 玩家选择：" + choice.choiceText);
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }
        
        // 设置标记（如果有）
        if (DialogueManager.Instance != null && !string.IsNullOrEmpty(choice.requirementFlag))
        {
            DialogueManager.Instance.SetStoryFlag(choice.requirementFlag, true);
        }
        
        // 关闭对话
        CloseDialogue();
    }
    
    public void CloseDialogue()
    {
        isDialogueActive = false;
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }
        
        if (choicesPanel != null)
        {
            foreach (Transform child in choicesPanel.transform)
            {
                Destroy(child.gameObject);
            }
        }
        
        if (onDialogueComplete != null)
        {
            onDialogueComplete.Invoke();
            onDialogueComplete = null;
        }
    }
    
    /// <summary>
    /// 点击对话框继续（无选项时）
    /// </summary>
    public void OnDialogueClick()
    {
        if (!isDialogueActive) return;
        
        if (currentCharIndex < currentDialogue.Length)
        {
            CancelInvoke("TypeNextCharacter");
            if (dialogueText != null)
            {
                dialogueText.text = currentDialogue;
            }
            currentCharIndex = currentDialogue.Length;
        }
        else if (currentChoices == null || currentChoices.Count == 0)
        {
            CloseDialogue();
        }
    }
    
    void Update()
    {
        if (isDialogueActive && InputHandler.GetInputDown())
        {
            OnDialogueClick();
        }
    }
    
    /// <summary>
    /// 检查对话是否已播放
    /// </summary>
    public bool HasPlayed(string dialogueKey)
    {
        return playedDialogues.Contains(dialogueKey);
    }
    
    /// <summary>
    /// 标记对话已播放
    /// </summary>
    public void MarkAsPlayed(string dialogueKey)
    {
        if (!playedDialogues.Contains(dialogueKey))
        {
            playedDialogues.Add(dialogueKey);
        }
    }
    
    /// <summary>
    /// 重置播放记录
    /// </summary>
    public void ResetPlayedDialogues()
    {
        playedDialogues.Clear();
    }
    
    /// <summary>
    /// 获取对话唯一键
    /// </summary>
    public static string GetDialogueKey(MinigameDialogueTrigger trigger, int count)
    {
        return string.Format("{0}_{1}", trigger.ToString(), count);
    }
    
    /// <summary>
    /// 设置选项按钮背景
    /// </summary>
    public void SetChoiceButtonBackground(Sprite background)
    {
        choiceButtonBackground = background;
        Debug.Log("[DumplingGameDialogue] 选项按钮背景已设置");
    }
}
