using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 进度条控制器 - 新版包饺子小游戏
/// 控制指针在半圆进度条上旋转，判定是否在绿色区域
/// Unity 5兼容版本
/// </summary>
public class ProgressBarController : MonoBehaviour
{
    [Header("UI References")]
    public Image progressBarBackground;  // 进度条背景
    public Image redZone1;               // 红色区域1（左侧）
    public Image greenZone;              // 绿色区域（中间）
    public Image redZone2;               // 红色区域2（右侧）
    public RectTransform pointer;        // 指针
    
    [Header("Gameplay Settings")]
    public float gameDuration = 4f;      // 游戏时长（秒）
    public float autoRotateSpeed = 45f;  // 自动逆时针旋转速度（度/秒）
    public float manualRotateSpeed = 60f; // 长按顺时针旋转速度（度/秒）
    
    [Header("Zone Angles")]
    public float greenZoneStartAngle = 60f;  // 绿色区域起始角度
    public float greenZoneEndAngle = 120f;   // 绿色区域结束角度
    
    [Header("Difficulty Presets")]
    public DifficultyPreset easyPreset;   // 简单难度
    public DifficultyPreset normalPreset; // 普通难度
    public DifficultyPreset hardPreset;   // 困难难度
    
    // 游戏状态
    private bool isGameActive = false;
    private float currentAngle = 0f;     // 当前指针角度（0-180度）
    private float timer = 0f;
    
    // 事件回调
    public System.Action<bool> OnGameComplete;  // 游戏完成回调（true=成功，false=失败）
    
    [System.Serializable]
    public class DifficultyPreset
    {
        public float greenZoneStart = 60f;
        public float greenZoneEnd = 120f;
        public float autoSpeed = 45f;
        public float manualSpeed = 60f;
        public float duration = 4f;
    }
    
    void Update()
    {
        if (!isGameActive)
        {
            return;
        }
        
        // 更新指针旋转
        UpdatePointerRotation();
        
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
        currentAngle = 0f;
        timer = 0f;
        
        // 重置指针位置
        if (pointer != null)
        {
            pointer.rotation = Quaternion.Euler(0, 0, 0);
        }
        
        Debug.Log("[ProgressBar] 游戏开始");
    }
    
    /// <summary>
    /// 设置难度
    /// </summary>
    public void SetDifficulty(MinigameDifficulty difficulty)
    {
        DifficultyPreset preset = normalPreset;
        
        if (difficulty == MinigameDifficulty.Easy)
        {
            preset = easyPreset;
        }
        else if (difficulty == MinigameDifficulty.Hard)
        {
            preset = hardPreset;
        }
        
        // 应用预设
        greenZoneStartAngle = preset.greenZoneStart;
        greenZoneEndAngle = preset.greenZoneEnd;
        autoRotateSpeed = preset.autoSpeed;
        manualRotateSpeed = preset.manualSpeed;
        gameDuration = preset.duration;
        
        Debug.Log(string.Format("[ProgressBar] 难度设置为: {0}", difficulty));
    }
    
    /// <summary>
    /// 更新指针旋转
    /// </summary>
    private void UpdatePointerRotation()
    {
        float deltaAngle = 0f;
        
        // 检测输入（长按）
        bool isHolding = InputHandler.GetInput();
        
        if (isHolding)
        {
            // 长按：顺时针旋转
            deltaAngle = manualRotateSpeed * Time.deltaTime;
        }
        else
        {
            // 自动：逆时针旋转
            deltaAngle = -autoRotateSpeed * Time.deltaTime;
        }
        
        // 更新角度
        currentAngle += deltaAngle;
        
        // 限制在0-180度范围内
        currentAngle = Mathf.Clamp(currentAngle, 0f, 180f);
        
        // 应用旋转到指针
        if (pointer != null)
        {
            pointer.rotation = Quaternion.Euler(0, 0, currentAngle);
        }
    }
    
    /// <summary>
    /// 判定结果
    /// </summary>
    private void JudgeResult()
    {
        isGameActive = false;
        
        // 检查指针是否在绿色区域
        bool inGreenZone = (currentAngle >= greenZoneStartAngle && currentAngle <= greenZoneEndAngle);
        
        Debug.Log(string.Format("[ProgressBar] 判定结果 - 角度: {0}, 绿色区域: {1}-{2}, 成功: {3}", 
            currentAngle, greenZoneStartAngle, greenZoneEndAngle, inGreenZone));
        
        // 触发回调
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
    /// 获取当前角度（用于调试）
    /// </summary>
    public float GetCurrentAngle()
    {
        return currentAngle;
    }
    
    /// <summary>
    /// 获取剩余时间
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, gameDuration - timer);
    }
    
    /// <summary>
    /// 获取进度（0-1）
    /// </summary>
    public float GetProgress()
    {
        return Mathf.Clamp01(timer / gameDuration);
    }
}
