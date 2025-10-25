using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 留影册UI管理器 - 负责成就界面的显示和交互
/// </summary>
public class AchievementUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject achievementPanel;         // 留影册主面板
    public Transform achievementGrid;           // 成就网格容器
    public GameObject achievementItemPrefab;    // 成就条目预制体

    [Header("Detail Panel")]
    public GameObject detailPanel;              // 详情面板
    public Image detailImage;                   // 详情图片
    public Text detailName;                     // 详情名称
    public Text detailDescription;              // 详情描述
    public Button detailCloseButton;            // 关闭详情按钮

    [Header("Statistics")]
    public Text statisticsText;                 // 统计文本

    [Header("Locked State")]
    public Sprite lockedSprite;                 // 锁定状态图片

    // 成就条目UI列表
    private List<GameObject> achievementItems = new List<GameObject>();

    void Start()
    {
        // 默认隐藏面板
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }

        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

        // 绑定详情关闭按钮
        if (detailCloseButton != null)
        {
            detailCloseButton.onClick.AddListener(CloseDetail);
        }
    }

    /// <summary>
    /// 打开留影册
    /// </summary>
    public void OpenAchievementPanel()
    {
        if (achievementPanel == null)
        {
            Debug.LogError("Achievement Panel未设置");
            return;
        }

        achievementPanel.SetActive(true);

        // 刷新成就列表
        RefreshAchievementList();

        // 更新统计
        UpdateStatistics();
    }

    /// <summary>
    /// 关闭留影册
    /// </summary>
    public void CloseAchievementPanel()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }

        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 刷新成就列表
    /// </summary>
    void RefreshAchievementList()
    {
        // 清空现有条目
        foreach (GameObject item in achievementItems)
        {
            Destroy(item);
        }
        achievementItems.Clear();

        if (AchievementManager.Instance == null)
        {
            Debug.LogWarning("AchievementManager未初始化");
            return;
        }

        // 获取所有成就
        List<Achievement> achievements = AchievementManager.Instance.GetAllAchievements();

        // 生成成就条目
        foreach (Achievement achievement in achievements)
        {
            CreateAchievementItem(achievement);
        }
    }

    /// <summary>
    /// 创建单个成就条目UI
    /// </summary>
    void CreateAchievementItem(Achievement achievement)
    {
        if (achievementItemPrefab == null || achievementGrid == null)
        {
            Debug.LogError("成就条目预制体或网格容器未设置");
            return;
        }

        // 实例化预制体
        GameObject itemObj = Instantiate(achievementItemPrefab, achievementGrid);
        achievementItems.Add(itemObj);

        // 获取组件
        AchievementItemUI itemUI = itemObj.GetComponent<AchievementItemUI>();
        if (itemUI == null)
        {
            itemUI = itemObj.AddComponent<AchievementItemUI>();
        }

        // 设置数据
        itemUI.Setup(achievement, this);
    }

    /// <summary>
    /// 显示成就详情
    /// </summary>
    public void ShowDetail(Achievement achievement)
    {
        if (detailPanel == null)
            return;

        detailPanel.SetActive(true);

        // 加载图片
        if (detailImage != null)
        {
            if (achievement.isUnlocked)
            {
                // 已解锁：显示彩色图片
                Sprite sprite = LoadAchievementSprite(achievement.imageFileName);
                detailImage.sprite = sprite != null ? sprite : lockedSprite;
                detailImage.color = Color.white;
            }
            else
            {
                // 未解锁：显示锁定图片
                detailImage.sprite = lockedSprite;
                detailImage.color = Color.gray;
            }
        }

        // 显示名称
        if (detailName != null)
        {
            detailName.text = achievement.isUnlocked ? achievement.achievementName : "？？？";
        }

        // 显示描述
        if (detailDescription != null)
        {
            detailDescription.text = achievement.isUnlocked ? achievement.description : "尚未解锁";
        }
    }

    /// <summary>
    /// 关闭详情面板
    /// </summary>
    public void CloseDetail()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 更新统计信息
    /// </summary>
    void UpdateStatistics()
    {
        if (statisticsText == null || AchievementManager.Instance == null)
            return;

        int unlocked = AchievementManager.Instance.GetUnlockedCount();
        int total = AchievementManager.Instance.GetAllAchievements().Count;

        statisticsText.text = $"成就进度：{unlocked}/{total}";
    }

    /// <summary>
    /// 加载成就图片
    /// </summary>
    Sprite LoadAchievementSprite(string fileName)
    {
        // 从Resources/Achievements/文件夹加载
        return Resources.Load<Sprite>($"Achievements/{fileName}");
    }
}

/// <summary>
/// 成就条目UI组件 - 附加到每个成就条目预制体上
/// </summary>
public class AchievementItemUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image iconImage;                     // 成就图标
    public Image lockedOverlay;                 // 锁定遮罩
    public Text nameText;                       // 成就名称
    public Button button;                       // 点击按钮

    private Achievement achievement;
    private AchievementUIManager uiManager;

    /// <summary>
    /// 设置成就数据
    /// </summary>
    public void Setup(Achievement ach, AchievementUIManager manager)
    {
        achievement = ach;
        uiManager = manager;

        // 自动获取组件
        if (iconImage == null)
            iconImage = transform.Find("Icon")?.GetComponent<Image>();
        if (lockedOverlay == null)
            lockedOverlay = transform.Find("LockedOverlay")?.GetComponent<Image>();
        if (nameText == null)
            nameText = GetComponentInChildren<Text>();
        if (button == null)
            button = GetComponent<Button>();

        // 更新UI
        UpdateUI();

        // 绑定点击事件
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    void UpdateUI()
    {
        if (achievement.isUnlocked)
        {
            // 已解锁状态
            if (iconImage != null)
            {
                Sprite sprite = Resources.Load<Sprite>($"Achievements/{achievement.imageFileName}");
                if (sprite != null)
                {
                    iconImage.sprite = sprite;
                    iconImage.color = Color.white;
                }
            }

            if (nameText != null)
            {
                nameText.text = achievement.achievementName;
                nameText.color = Color.white;
            }

            if (lockedOverlay != null)
            {
                lockedOverlay.gameObject.SetActive(false);
            }
        }
        else
        {
            // 未解锁状态
            if (iconImage != null)
            {
                // 显示锁定图片或变灰
                iconImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            }

            if (nameText != null)
            {
                nameText.text = "？？？";
                nameText.color = Color.gray;
            }

            if (lockedOverlay != null)
            {
                lockedOverlay.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 点击事件
    /// </summary>
    void OnClick()
    {
        if (uiManager != null && achievement != null)
        {
            if (achievement.isUnlocked)
            {
                uiManager.ShowDetail(achievement);
            }
            else
            {
                Debug.Log("此成就尚未解锁");
                
            }
        }
    }
}
