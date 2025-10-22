using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DumplingGameDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public Text characterNameText;
    public Text dialogueText;
    public GameObject choicesPanel;
    public GameObject choiceButtonPrefab;
    
    [Header("Settings")]
    public float textSpeed = 0.05f;
    
    private bool isDialogueActive = false;
    private string currentDialogue = "";
    private int currentCharIndex = 0;
    private List<DialogueChoice> currentChoices;
    
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
    
    // 显示对话（带可选的选项）
    public void ShowDialogue(string characterName, string dialogue, List<DialogueChoice> choices = null)
    {
        // 暂停游戏
        if (NewDumplingGameManager.Instance != null)
        {
            NewDumplingGameManager.Instance.PauseGame();
        }
        
        isDialogueActive = true;
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        // 显示角色名
        if (characterNameText != null)
        {
            characterNameText.text = characterName;
        }
        
        // 开始打字机效果
        currentDialogue = dialogue;
        currentCharIndex = 0;
        currentChoices = choices;
        
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
        
        InvokeRepeating("TypeNextCharacter", 0f, textSpeed);
        
        // 如果有选项，等打字完成后显示
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
        
        // 清空现有选项
        foreach (Transform child in choicesPanel.transform)
        {
            Destroy(child.gameObject);
        }
        
        choicesPanel.SetActive(true);
        
        // 创建选项按钮
        foreach (DialogueChoice choice in currentChoices)
        {
            if (choiceButtonPrefab == null)
            {
                Debug.LogError("选项按钮预制体未设置！");
                continue;
            }
            
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            Button button = buttonObj.GetComponent<Button>();
            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            
            if (buttonText != null)
            {
                buttonText.text = choice.choiceText;
            }
            
            // 绑定点击事件
            if (button != null)
            {
                DialogueChoice selectedChoice = choice; // 闭包
                button.onClick.AddListener(() => OnChoiceSelected(selectedChoice));
            }
        }
    }
    
    void OnChoiceSelected(DialogueChoice choice)
    {
        Debug.Log("玩家选择：" + choice.choiceText);
        
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
        
        // 继续游戏
        if (NewDumplingGameManager.Instance != null)
        {
            NewDumplingGameManager.Instance.ResumeGame();
        }
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
        
        // 清空选项
        if (choicesPanel != null)
        {
            foreach (Transform child in choicesPanel.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    // 点击对话框继续（无选项时）
    public void OnDialogueClick()
    {
        if (!isDialogueActive) return;
        
        // 如果还在打字，立即完成
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
            // 无选项，直接关闭对话
            CloseDialogue();
            
            // 恢复游戏
            if (NewDumplingGameManager.Instance != null)
            {
                NewDumplingGameManager.Instance.ResumeGame();
            }
        }
    }
    
    void Update()
    {
        // PC端：点击屏幕继续
        if (isDialogueActive && InputHandler.GetInputDown())
        {
            OnDialogueClick();
        }
    }
}
