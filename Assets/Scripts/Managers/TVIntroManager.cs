using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class TVIntroManager : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public Animator tvAnimator;
    public GameObject tvPowerButton;
    
    [Header("Scene Names")]
    public string dialogueSceneName = "DialogueScene";
    
    [Header("Animation Names (if using Animator)")]
    public string loopAnimationName = "TV_Loop";
    public string stopTriggerName = "Stop";
    
    private bool useVideoPlayer = true;
    
    void Start()
    {
        // 检测使用哪种方式播放
        if (videoPlayer != null)
        {
            useVideoPlayer = true;
            SetupVideoPlayer();
        }
        else if (tvAnimator != null)
        {
            useVideoPlayer = false;
            SetupAnimator();
        }
        
        // 确保按钮初始可见
        if (tvPowerButton != null)
        {
            tvPowerButton.SetActive(true);
        }
    }
    
    void SetupVideoPlayer()
    {
        if (videoPlayer != null)
        {
            videoPlayer.isLooping = true;
            videoPlayer.Play();
            Debug.Log("电视机视频开始播放");
        }
    }
    
    void SetupAnimator()
    {
        if (tvAnimator != null && !string.IsNullOrEmpty(loopAnimationName))
        {
            tvAnimator.Play(loopAnimationName);
            Debug.Log("电视机动画开始播放");
        }
    }
    
    // 点击电视开关按钮
    public void OnTVPowerButtonClick()
    {
        Debug.Log("电视机开关被点击");
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("tvClick");
        }
        
        // 停止播放
        if (useVideoPlayer && videoPlayer != null)
        {
            videoPlayer.Stop();
        }
        else if (!useVideoPlayer && tvAnimator != null && !string.IsNullOrEmpty(stopTriggerName))
        {
            tvAnimator.SetTrigger(stopTriggerName);
        }
        
        // 延迟跳转场景（给停止动画/视频一点时间）
        Invoke("LoadDialogueScene", 0.5f);
    }
    
    void LoadDialogueScene()
    {
        if (!string.IsNullOrEmpty(dialogueSceneName))
        {
            SceneManager.LoadScene(dialogueSceneName);
        }
        else
        {
            Debug.LogError("对话场景名称未设置！");
        }
    }
    
    // 可选：跳过功能
    public void OnSkipButtonClick()
    {
        Debug.Log("跳过按钮被点击");
        OnTVPowerButtonClick();
    }
}
