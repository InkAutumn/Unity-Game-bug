using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 进度条控制器
/// 指针在直线进度条上移动，判定是否在绿色区域
/// </summary>
public class ProgressBarController : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform pointer;        // 指针
    public RectTransform progressBar;    // 进度条
    public Image greenZone;              // 绿色区域
    
    [Header("Gameplay Settings")]
    public float gameDuration = 5f;      // 游戏时长
    public float rightMoveSpeed = 200f;  // 向右移动速度
    public float leftMoveSpeed = 240f;   // 向左移动速度
    
    [Header("Progress Bar Range")]
    public float barStartX = -200f;      // 进度条起始X坐标
    public float barEndX = 200f;         // 进度条结束X坐标
    
    [Header("Green Zone Range")]
    public float greenZoneStartX = -50f; // 绿色区域起始X坐标
    public float greenZoneEndX = 50f;    // 绿色区域结束X坐标
    
    // 游戏状态
    private bool isGameActive = false;
    private float currentX = 0f;         // 当前指针X坐标
    private float timer = 0f;
    
    // 事件回调
    public System.Action<bool> OnGameComplete;  // 游戏完成回调
    
    void Start()
    {
        // 自动获取进度条宽度
        if (progressBar != null && barStartX == -200f && barEndX == 200f)
        {
            float barWidth = progressBar.rect.width;
            barStartX = -barWidth / 2f;
            barEndX = barWidth / 2f;
        }
        
        // 自动获取绿色区域位置
        if (greenZone != null && greenZoneStartX == -50f && greenZoneEndX == 50f)
        {
            RectTransform greenRect = greenZone.GetComponent<RectTransform>();
            if (greenRect != null)
            {
                float greenWidth = greenRect.rect.width;
                float greenPosX = greenRect.anchoredPosition.x;
                greenZoneStartX = greenPosX - greenWidth / 2f;
                greenZoneEndX = greenPosX + greenWidth / 2f;
            }
        }
    }
    
    void Update()
    {
        if (!isGameActive)
        {
            return;
        }
        
        // 更新指针位置
        UpdatePointerPosition();
        
        // 更新计时器
        timer += Time.deltaTime;
        
        // 时间到，判定结果
        if (timer >= gameDuration)
        {
            JudgeResult();
        }
    }
    
    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        isGameActive = true;
        currentX = barStartX;  // 从左端开始
        timer = 0f;
        
        // 重置指针到起始位置
        if (pointer != null)
        {
            Vector2 pos = pointer.anchoredPosition;
            pos.x = currentX;
            pointer.anchoredPosition = pos;
        }
        
        Debug.Log(string.Format("[ProgressBar] 游戏开始 - 进度条范围: {0} 到 {1}, 绿色区域: {2} 到 {3}", 
            barStartX, barEndX, greenZoneStartX, greenZoneEndX));
    }
    
    /// <summary>
    /// 设置难度
    /// </summary>
    public void SetDifficulty(MinigameDifficulty difficulty)
    {
        Debug.Log("[ProgressBar] 直线版本不使用难度设置");
    }
    
    /// <summary>
    /// 更新指针位置
    /// </summary>
    private void UpdatePointerPosition()
    {
        float deltaX = 0f;
        
        bool isHolding = InputHandler.GetInput();
        
        if (isHolding)
        {
            deltaX = -leftMoveSpeed * Time.deltaTime;
        }
        else
        {
            deltaX = rightMoveSpeed * Time.deltaTime;
        }
        
        currentX += deltaX;
        
        currentX = Mathf.Clamp(currentX, barStartX, barEndX);
        
        if (pointer != null)
        {
            Vector2 pos = pointer.anchoredPosition;
            pos.x = currentX;
            pointer.anchoredPosition = pos;
        }
    }
    
    /// <summary>
    /// 判定结果
    /// </summary>
    private void JudgeResult()
    {
        isGameActive = false;
        
        bool inGreenZone = (currentX >= greenZoneStartX && currentX <= greenZoneEndX);
        
        Debug.Log(string.Format("[ProgressBar] 判定结果 - 指针位置: {0}, 绿色区域: {1} 到 {2}, 成功: {3}", 
            currentX, greenZoneStartX, greenZoneEndX, inGreenZone));
        
        if (OnGameComplete != null)
        {
            OnGameComplete(inGreenZone);
        }
    }
    
    /// <summary>
    /// 停止游戏
    /// </summary>
    public void StopGame()
    {
        isGameActive = false;
        timer = 0f;
    }
    
    /// <summary>
    /// 获取当前位置
    /// </summary>
    public float GetCurrentPosition()
    {
        return currentX;
    }
    
    /// <summary>
    /// 获取剩余时间
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, gameDuration - timer);
    }
    
    /// <summary>
    /// 获取进度
    /// </summary>
    public float GetProgress()
    {
        return Mathf.Clamp01(timer / gameDuration);
    }
}
