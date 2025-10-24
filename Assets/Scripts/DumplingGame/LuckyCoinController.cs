using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 幸运硬币控制器 - 新版包饺子小游戏特殊环节
/// 控制硬币的显示、动画和点击检测
/// Unity 5兼容版本
/// </summary>
public class LuckyCoinController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject coinObject;        // 硬币GameObject
    public Image coinImage;              // 硬币Image
    public Button coinButton;            // 硬币按钮
    
    [Header("Animation Settings")]
    public bool enableRotation = true;   // 启用旋转动画
    public float rotationSpeed = 180f;   // 旋转速度（度/秒）
    public bool enablePulse = true;      // 启用脉冲动画
    public float pulseSpeed = 2f;        // 脉冲速度
    public float pulseMin = 0.9f;        // 最小缩放
    public float pulseMax = 1.1f;        // 最大缩放
    
    [Header("Position")]
    public Vector2 coinPosition = new Vector2(400f, 0f);  // 硬币位置（右侧）
    
    // 动画状态
    private float rotationAngle = 0f;
    private float pulseTime = 0f;
    private bool isShowing = false;
    
    // 事件回调
    public System.Action OnCoinClicked;  // 硬币被点击回调
    
    void Start()
    {
        // 初始化隐藏
        if (coinObject != null)
        {
            coinObject.SetActive(false);
        }
        
        // 绑定按钮点击事件
        if (coinButton != null)
        {
            coinButton.onClick.AddListener(OnCoinClick);
        }
    }
    
    void Update()
    {
        if (!isShowing)
        {
            return;
        }
        
        // 旋转动画
        if (enableRotation && coinObject != null)
        {
            rotationAngle += rotationSpeed * Time.deltaTime;
            if (rotationAngle >= 360f)
            {
                rotationAngle -= 360f;
            }
            coinObject.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
        }
        
        // 脉冲动画
        if (enablePulse && coinObject != null)
        {
            pulseTime += pulseSpeed * Time.deltaTime;
            float scale = Mathf.Lerp(pulseMin, pulseMax, (Mathf.Sin(pulseTime) + 1f) * 0.5f);
            coinObject.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
    
    /// <summary>
    /// 显示硬币
    /// </summary>
    public void ShowCoin()
    {
        if (coinObject != null)
        {
            coinObject.SetActive(true);
            isShowing = true;
            
            // 设置位置
            RectTransform rectTransform = coinObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = coinPosition;
            }
            
            // 重置动画状态
            rotationAngle = 0f;
            pulseTime = 0f;
            
            // 播放出现音效（如果有AudioManager）
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("coin_appear");
            }
            
            Debug.Log("[LuckyCoin] 硬币出现");
        }
    }
    
    /// <summary>
    /// 隐藏硬币
    /// </summary>
    public void HideCoin()
    {
        if (coinObject != null)
        {
            coinObject.SetActive(false);
            isShowing = false;
            
            Debug.Log("[LuckyCoin] 硬币隐藏");
        }
    }
    
    /// <summary>
    /// 硬币被点击
    /// </summary>
    private void OnCoinClick()
    {
        Debug.Log("[LuckyCoin] 硬币被点击");
        
        // 播放点击音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("coin_click");
        }
        
        // 触发回调
        if (OnCoinClicked != null)
        {
            OnCoinClicked();
        }
        
        // 隐藏硬币
        HideCoin();
    }
    
    /// <summary>
    /// 设置硬币位置
    /// </summary>
    public void SetPosition(Vector2 position)
    {
        coinPosition = position;
        
        if (coinObject != null && isShowing)
        {
            RectTransform rectTransform = coinObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = coinPosition;
            }
        }
    }
    
    /// <summary>
    /// 是否正在显示
    /// </summary>
    public bool IsShowing()
    {
        return isShowing;
    }
}
