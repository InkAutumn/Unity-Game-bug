using System;
using UnityEngine;
using UnityEngine.UI;

public class DumplingMakingController : MonoBehaviour
{
    [Header("Step References")]
    public GameObject doughPrefab;           // 饺子皮预制体
    public GameObject waterBowl;             // 水碗
    public GameObject fillingBowl;           // 馅料碗
    public GameObject spoon;                 // 勺子
    public GameObject cuttingBoard;          // 案板

    [Header("Containers")]
    public Transform workArea;               // 工作区域
    public Transform finishedDumplingsArea;  // 完成的饺子区域

    [Header("UI")]
    public Text stepHintText;                // 步骤提示文本
    public Image fingerWaterIndicator;       // 手指蘸水指示器
    public Image currentDoughImage;          // 当前饺子皮图片
    public Slider waterCoverageSlider;       // 水分覆盖显示条（调试用）

    [Header("Water Application Settings")]
    public float minWaterCoverage = 0.25f;   // 最小水分覆盖率（1/4）
    public float maxWaterCoverage = 0.5f;    // 最大水分覆盋率（1/2）
    public float waterApplyRate = 0.1f;      // 涂抹速度

    [Header("Hand Sprites")]
    public Image handsImage;                 // 手部图片显示
    public Sprite normalHandsSprite;         // 普通手部
    public Sprite waterHandsSprite;          // 蘸水后的手部
    public Sprite spoonHandsSprite;          // 拿勺子的手部

    [Header("Special Items")]
    public GameObject coinPrefab;            // 硬币预制体
    public Transform coinSpawnPosition;      // 硬币生成位置（场景中某个位置）
    
    private DumplingMakingStep currentStep = DumplingMakingStep.PlaceDough;
    private DumplingState currentDumpling = new DumplingState();
    private bool isDraggingOnDough = false;
    private bool hasSpoon = false;
    private bool spoonHasFilling = false;
    
    // 特殊硬币相关
    private GameObject coinObject;           // 场景中的硬币对象
    private bool hasCoin = false;            // 是否拿着硬币
    private bool coinPlacedOnDough = false;  // 硬币是否已放在饺子皮上
    private bool isSpecialCoinGame = false;  // 是否为特殊硬币游戏
    private int completedDumplingCount = 0;
    
    // 游戏内对话管理器
    private DumplingGameDialogueManager gameDialogueManager;

    public event Action<DumplingQuality> OnDumplingCompleted;

    void Start()
    {
        UpdateStepHint();
        UpdateHandsSprite();

        if (fingerWaterIndicator != null)
        {
            fingerWaterIndicator.enabled = false;
        }
        
        // 查找游戏内对话管理器
        gameDialogueManager = FindObjectOfType<DumplingGameDialogueManager>();
        
        // 检查是否有特殊道具设置
        CheckForSpecialItem();
    }
    
    void CheckForSpecialItem()
    {
        if (GameManager.Instance != null)
        {
            SpecialItem item = GameManager.Instance.GetSpecialItem();
            
            if (item != null && item.itemType == SpecialItemType.Coin)
            {
                // 这是特殊硬币游戏
                isSpecialCoinGame = true;
                
                // 立即生成硬币
                SpawnCoin();
                
                Debug.Log("特殊硬币游戏已启动");
            }
        }
    }
    
    void SpawnCoin()
    {
        if (coinPrefab == null)
        {
            Debug.LogError("硬币预制体未设置！");
            return;
        }
        
        Vector3 spawnPos = coinSpawnPosition != null ? 
            coinSpawnPosition.position : 
            new Vector3(-200, 200, 0); // 默认位置
        
        coinObject = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        
        // 添加可点击的Collider（如果没有）
        if (coinObject.GetComponent<Collider2D>() == null)
        {
            coinObject.AddComponent<CircleCollider2D>();
        }
        
        Debug.Log("硬币已生成在场景中");
    }

    void Update()
    {
        // 特殊硬币游戏的输入处理（优先）
        if (isSpecialCoinGame && currentDumpling.hasDough && !coinPlacedOnDough)
        {
            HandleCoinInteraction();
            UpdateUI();
            return; // 等待玩家放置硬币后再继续
        }
        
        // 处理当前步骤的输入
        switch (currentStep)
        {
            case DumplingMakingStep.PlaceDough:
                HandlePlaceDoughStep();
                break;

            case DumplingMakingStep.DipWater:
                HandleDipWaterStep();
                break;

            case DumplingMakingStep.ApplyWater:
                HandleApplyWaterStep();
                break;

            case DumplingMakingStep.AddFilling:
                HandleAddFillingStep();
                break;

            case DumplingMakingStep.WrapDumpling:
                HandleWrapDumplingStep();
                break;
        }

        // 更新UI
        UpdateUI();
    }

    // === 步骤1：放置饺子皮 ===
    void HandlePlaceDoughStep()
    {
        if (InputHandler.GetInputDown() && !InputHandler.IsPointerOverUI())
        {
            Ray ray = Camera.main.ScreenPointToRay(InputHandler.GetInputPosition());
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject == doughPrefab)
            {
                // 点击饺子皮堆，创建一个饺子皮到工作区
                PlaceDoughInWorkArea();
            }
        }
    }

    void PlaceDoughInWorkArea()
    {
        if (currentDoughImage != null)
        {
            currentDoughImage.enabled = true;
            currentDumpling.hasDough = true;
            
            // 特殊硬币游戏：如果还没放硬币，需要先放硬币
            if (isSpecialCoinGame && !coinPlacedOnDough)
            {
                UpdateStepHintForCoin("现在点击硬币，将它放在饺子皮上");
                // 不进入下一步，等待玩家放硬币
                return;
            }
            
            AdvanceToNextStep();
        }
    }
    
    void UpdateStepHintForCoin(string hint)
    {
        if (stepHintText != null)
        {
            stepHintText.text = hint;
        }
    }
    
    // 特殊硬币游戏的输入处理
    void HandleCoinInteraction()
    {
        if (!isSpecialCoinGame) return;
        
        if (InputHandler.GetInputDown() && !InputHandler.IsPointerOverUI())
        {
            Ray ray = Camera.main.ScreenPointToRay(InputHandler.GetInputPosition());
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            
            if (hit.collider != null)
            {
                // 点击硬币拾取
                if (!hasCoin && hit.collider.gameObject == coinObject)
                {
                    PickUpCoin();
                }
                // 点击饺子皮放置硬币
                else if (hasCoin && hit.collider.gameObject == currentDoughImage.gameObject)
                {
                    PlaceCoinOnDough();
                }
            }
        }
    }
    
    void PickUpCoin()
    {
        hasCoin = true;
        
        // 隐藏场景中的硬币（实际是移到屏幕外或销毁）
        if (coinObject != null)
        {
            coinObject.SetActive(false);
        }
        
        UpdateStepHintForCoin("点击饺子皮，将硬币放在上面");
        Debug.Log("拾取硬币");
    }
    
    void PlaceCoinOnDough()
    {
        if (!hasCoin || !currentDumpling.hasDough) return;
        
        coinPlacedOnDough = true;
        hasCoin = false;
        
        // 可以在饺子皮上显示硬币图标
        Debug.Log("硬币已放在饺子皮上");
        
        // 继续正常的包饺子流程
        AdvanceToNextStep();
    }

    // === 步骤2：蘸水 ===
    void HandleDipWaterStep()
    {
        if (InputHandler.GetInputDown() && !InputHandler.IsPointerOverUI())
        {
            Ray ray = Camera.main.ScreenPointToRay(InputHandler.GetInputPosition());
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject == waterBowl)
            {
                DipFingerInWater();
            }
        }
    }

    void DipFingerInWater()
    {
        currentDumpling.hasWaterOnFinger = true;

        if (fingerWaterIndicator != null)
        {
            fingerWaterIndicator.enabled = true;
        }

        UpdateHandsSprite();
        AdvanceToNextStep();
    }

    // === 步骤3：涂抹水分 ===
    void HandleApplyWaterStep()
    {
        // 检测是否在饺子皮上拖动
        if (InputHandler.GetInput() && !InputHandler.IsPointerOverUI())
        {
            Ray ray = Camera.main.ScreenPointToRay(InputHandler.GetInputPosition());
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject == currentDoughImage.gameObject)
            {
                if (!isDraggingOnDough)
                {
                    isDraggingOnDough = true;
                }

                // 增加水分覆盖率
                ApplyWaterToDough();
            }
        }

        if (InputHandler.GetInputUp())
        {
            if (isDraggingOnDough)
            {
                isDraggingOnDough = false;
                EvaluateWaterCoverage();
            }
        }
    }

    void ApplyWaterToDough()
    {
        currentDumpling.waterCoverage += waterApplyRate * Time.deltaTime;
        currentDumpling.waterCoverage = Mathf.Clamp01(currentDumpling.waterCoverage);

        // 视觉反馈：改变饺子皮颜色或透明度
        if (currentDoughImage != null)
        {
            Color color = currentDoughImage.color;
            color.a = 0.7f + currentDumpling.waterCoverage * 0.3f;
            currentDoughImage.color = color;
        }
    }

    void EvaluateWaterCoverage()
    {
        // 评估水分覆盖率
        if (currentDumpling.waterCoverage < minWaterCoverage)
        {
            currentDumpling.quality = DumplingQuality.TooLittleWater;
            Debug.Log("水分太少！饺子会漏馅");
        }
        else if (currentDumpling.waterCoverage > maxWaterCoverage)
        {
            currentDumpling.quality = DumplingQuality.TooMuchWater;
            Debug.Log("水分太多！饺子会破皮");
        }
        else
        {
            currentDumpling.quality = DumplingQuality.Perfect;
            Debug.Log("水分刚好！");
        }

        // 清除手指上的水
        currentDumpling.hasWaterOnFinger = false;
        if (fingerWaterIndicator != null)
        {
            fingerWaterIndicator.enabled = false;
        }

        UpdateHandsSprite();
        AdvanceToNextStep();
    }

    // === 步骤4：添加馅料 ===
    void HandleAddFillingStep()
    {
        if (InputHandler.GetInputDown() && !InputHandler.IsPointerOverUI())
        {
            Ray ray = Camera.main.ScreenPointToRay(InputHandler.GetInputPosition());
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                if (!hasSpoon && hit.collider.gameObject == spoon)
                {
                    // 拿起勺子
                    PickUpSpoon();
                }
                else if (hasSpoon && !spoonHasFilling && hit.collider.gameObject == fillingBowl)
                {
                    // 舀馅料
                    ScoopFilling();
                }
                else if (hasSpoon && spoonHasFilling && hit.collider.gameObject == currentDoughImage.gameObject)
                {
                    // 放馅料到饺子皮
                    PlaceFillingOnDough();
                }
            }
        }
    }

    void PickUpSpoon()
    {
        hasSpoon = true;
        UpdateHandsSprite();
        Debug.Log("拿起勺子");
    }

    void ScoopFilling()
    {
        spoonHasFilling = true;
        Debug.Log("舀起馅料");
    }

    void PlaceFillingOnDough()
    {
        currentDumpling.hasFilling = true;
        hasSpoon = false;
        spoonHasFilling = false;
        UpdateHandsSprite();
        Debug.Log("放入馅料");
        AdvanceToNextStep();
    }

    // === 步骤5：捏饺子 ===
    void HandleWrapDumplingStep()
    {
        if (InputHandler.GetInputDown() && !InputHandler.IsPointerOverUI())
        {
            Ray ray = Camera.main.ScreenPointToRay(InputHandler.GetInputPosition());
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject == currentDoughImage.gameObject)
            {
                // 开始捏饺子（可以添加mini-game或动画）
                WrapDumpling();
            }
        }
    }

    void WrapDumpling()
    {
        currentDumpling.isWrapped = true;
        Debug.Log("捏好饺子！质量：" + currentDumpling.quality);

        // 允许拖拽到案板
        EnableDragToBoard();
    }

    void EnableDragToBoard()
    {
        // 这里可以启用拖拽逻辑
        // 简化处理：直接完成饺子
        CompleteDumpling();
    }

    void CompleteDumpling()
    {
        // 增加完成计数
        completedDumplingCount++;
        
        // 特殊硬币游戏：包好硬币饺子后立即返回对话场景
        if (isSpecialCoinGame && coinPlacedOnDough)
        {
            Debug.Log("硬币饺子包好了！返回对话场景");
            
            // 设置剧情标记：硬币饺子已完成
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.SetStoryFlag("coinDumplingCompleted", true);
            }
            
            // 清理硬币对象
            if (coinObject != null)
            {
                Destroy(coinObject);
            }
            
            // 触发完成事件（让GameManager知道）
            if (OnDumplingCompleted != null)
            {
                OnDumplingCompleted.Invoke(DumplingQuality.Perfect); // 硬币饺子视为完美
            }
            
            // 延迟0.5秒后返回对话场景
            Invoke("ReturnToDialogue", 0.5f);
            return;
        }
        
        // 普通游戏：继续正常流程
        // 触发完成事件
        if (OnDumplingCompleted != null)
        {
            OnDumplingCompleted.Invoke(currentDumpling.quality);
        }

        // 重置状态，准备下一个饺子
        ResetForNextDumpling();
    }
    
    void ReturnToDialogue()
    {
        // 通过GameManager返回对话场景
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToDialogue();
        }
        else
        {
            Debug.LogError("GameManager未找到，无法返回对话场景");
        }
    }

    void ResetForNextDumpling()
    {
        currentDumpling.Reset();
        currentStep = DumplingMakingStep.PlaceDough;

        if (currentDoughImage != null)
        {
            currentDoughImage.enabled = false;
            currentDoughImage.color = Color.white;
        }

        UpdateStepHint();
        UpdateHandsSprite();
    }
    
    // 公共方法：获取已完成饺子数量
    public int GetCompletedCount()
    {
        return completedDumplingCount;
    }

    // === 辅助方法 ===
    void AdvanceToNextStep()
    {
        currentStep++;
        UpdateStepHint();
    }

    void UpdateStepHint()
    {
        if (stepHintText == null) return;

        switch (currentStep)
        {
            case DumplingMakingStep.PlaceDough:
                stepHintText.text = "点击饺子皮堆，放置一张饺子皮";
                break;
            case DumplingMakingStep.DipWater:
                stepHintText.text = "点击水碗，用指尖蘸取少量水";
                break;
            case DumplingMakingStep.ApplyWater:
                stepHintText.text = "在饺子皮边缘涂抹水分";
                break;
            case DumplingMakingStep.AddFilling:
                stepHintText.text = "点击勺子→点击馅料→点击饺子皮";
                break;
            case DumplingMakingStep.WrapDumpling:
                stepHintText.text = "点击饺子进行捏合，然后拖到案板上";
                break;
        }
    }

    void UpdateHandsSprite()
    {
        if (handsImage == null) return;

        if (hasSpoon)
        {
            handsImage.sprite = spoonHandsSprite;
        }
        else if (currentDumpling.hasWaterOnFinger)
        {
            handsImage.sprite = waterHandsSprite;
        }
        else
        {
            handsImage.sprite = normalHandsSprite;
        }
    }

    void UpdateUI()
    {
        if (waterCoverageSlider != null)
        {
            waterCoverageSlider.value = currentDumpling.waterCoverage;
        }
    }

    // 公共方法：强制完成当前饺子（用于跳过）
    public void ForceCompleteCurrent()
    {
        CompleteDumpling();
    }
}
