using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置管理器
/// 处理音量、图像质量等设置
/// </summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject settingsPanel;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
    public Text bgmVolumeText;
    public Text sfxVolumeText;
    public Button closeButton;

    [Header("Default Values")]
    public float defaultBGMVolume = 0.7f;
    public float defaultSFXVolume = 0.8f;

    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 加载保存的设置
        LoadSettings();

        // 绑定滑块事件
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }

        // 初始隐藏设置面板
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    void LoadSettings()
    {
        // 加载BGM音量
        float bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, defaultBGMVolume);
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = bgmVolume;
        }
        ApplyBGMVolume(bgmVolume);

        // 加载SFX音量
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
        }
        ApplySFXVolume(sfxVolume);

        Debug.Log($"加载设置 - BGM: {bgmVolume}, SFX: {sfxVolume}");
    }

    void OnBGMVolumeChanged(float value)
    {
        ApplyBGMVolume(value);
        SaveBGMVolume(value);

        // 更新显示文本
        if (bgmVolumeText != null)
        {
            bgmVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }

    void OnSFXVolumeChanged(float value)
    {
        ApplySFXVolume(value);
        SaveSFXVolume(value);

        // 更新显示文本
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
        }

        // 播放测试音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }
    }

    void ApplyBGMVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(volume);
        }
    }

    void ApplySFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume);
        }
    }

    void SaveBGMVolume(float volume)
    {
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    void SaveSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("buttonClick");
        }

        // 通知PauseMenuManager返回暂停菜单
        if (PauseMenuManager.Instance != null)
        {
            PauseMenuManager.Instance.CloseAllSubPanels();
        }
    }

    // 公共方法：显示设置面板
    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // 公共方法：重置所有设置
    public void ResetToDefault()
    {
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = defaultBGMVolume;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = defaultSFXVolume;
        }

        Debug.Log("设置已重置为默认值");
    }

    // 获取当前BGM音量
    public float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat(BGM_VOLUME_KEY, defaultBGMVolume);
    }

    // 获取当前SFX音量
    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);
    }
}
