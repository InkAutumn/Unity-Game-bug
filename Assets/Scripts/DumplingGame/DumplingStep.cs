using UnityEngine;

// 饺子制作步骤枚举
public enum DumplingMakingStep
{
    PlaceDough,      // 步骤1：放置饺子皮
    DipWater,        // 步骤2：蘸水
    ApplyWater,      // 步骤3：涂抹水分
    AddFilling,      // 步骤4：添加馅料
    WrapDumpling     // 步骤5：捏饺子
}

// 饺子质量评级
public enum DumplingQuality
{
    Perfect,         // 完美
    TooLittleWater,  // 水分太少（馅料泄露）
    TooMuchWater,    // 水分太多（破皮）
    Good             // 良好（允许的误差范围内）
}

// 单个饺子的状态数据
[System.Serializable]
public class DumplingState
{
    public bool hasDough = false;
    public bool hasWaterOnFinger = false;
    public float waterCoverage = 0f; // 水分覆盖率 (0-1)
    public bool hasFilling = false;
    public bool isWrapped = false;
    public DumplingQuality quality = DumplingQuality.Perfect;
    
    // 特殊道具支持
    public bool hasSpecialItem = false;
    public SpecialItemType specialItemType = SpecialItemType.None;

    public void Reset()
    {
        hasDough = false;
        hasWaterOnFinger = false;
        waterCoverage = 0f;
        hasFilling = false;
        isWrapped = false;
        quality = DumplingQuality.Perfect;
        hasSpecialItem = false;
        specialItemType = SpecialItemType.None;
    }
}
