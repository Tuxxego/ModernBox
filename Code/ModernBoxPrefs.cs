using UnityEngine;
using System.Linq;

public enum EraProgressMode
{
    Fast,
    Default,
    Slow,
    Realism
}

public enum BalanceMode
{
    Weak,
    VanillaFriendly,
    AssymetricalWarfare,
    OGModernBox,
    Carnage
}

public static class ModernBoxPrefs
{
    public static bool Updates { get; set; } = true;

    public static EraProgressMode EraMode { get; set; } = EraProgressMode.Default;
    public static BalanceMode Balance { get; set; } = BalanceMode.AssymetricalWarfare;

    public static float[] EraProgress => EraProgressTable[EraMode];
    public static float[] RenaissanceTime => RenaissanceTimeTable[EraMode];
    public static float[] HyperfutureTime => HyperfutureTimeTable[EraMode];

    private static readonly System.Collections.Generic.Dictionary<EraProgressMode, float[]> EraProgressTable =
        new System.Collections.Generic.Dictionary<EraProgressMode, float[]>
        {
            { EraProgressMode.Fast,     new float[] { 0, 20, 0, 150 } },
            { EraProgressMode.Default,  new float[] { 100, 120, 0, 700 } },
            { EraProgressMode.Slow,  new float[] { 250, 240, 0, 900 } },
            { EraProgressMode.Realism,  new float[] { 1000, 1200, 0, 2000 } },
        };

    private static readonly System.Collections.Generic.Dictionary<EraProgressMode, float[]> RenaissanceTimeTable =
        new System.Collections.Generic.Dictionary<EraProgressMode, float[]>
        {
            { EraProgressMode.Fast,     new float[] { 0, 15, 0, 120 } },
            { EraProgressMode.Default,  new float[] { 50, 80, 0, 300 } },
            { EraProgressMode.Slow,  new float[] { 150, 140, 0, 400 } },
            { EraProgressMode.Realism,  new float[] { 500, 700, 0, 1000 } },
        };

    private static readonly System.Collections.Generic.Dictionary<EraProgressMode, float[]> HyperfutureTimeTable =
        new System.Collections.Generic.Dictionary<EraProgressMode, float[]>
        {
            { EraProgressMode.Fast,     new float[] { 0, 50, 0, 190 } },
            { EraProgressMode.Default,  new float[] { 400, 520, 0, 1000 } },
            { EraProgressMode.Slow,  new float[] { 550, 740, 0, 1300 } },
            { EraProgressMode.Realism,  new float[] { 2000, 1800, 0, 2400 } },
        };

    public static void Save()
    {
        PlayerPrefs.SetInt("MB_Updates", Updates ? 1 : 0);
        PlayerPrefs.SetInt("MB_EraMode", (int)EraMode);
        PlayerPrefs.SetInt("MB_Balance", (int)Balance);
        PlayerPrefs.Save();
    }

    public static void Load()
    {
        Updates = PlayerPrefs.GetInt("MB_Updates", 1) == 1;
        EraMode = (EraProgressMode)PlayerPrefs.GetInt("MB_EraMode", (int)EraProgressMode.Default);
        Balance = (BalanceMode)PlayerPrefs.GetInt("MB_Balance", (int)BalanceMode.AssymetricalWarfare);

        ModernBoxLogger.Log($"ModernBoxPrefs: Updates = {Updates}");
        ModernBoxLogger.Log($"ModernBoxPrefs: EraMode = {EraMode}");
        ModernBoxLogger.Log($"ModernBoxPrefs: Balance = {Balance}");

        ModernBoxLogger.Log(
            $"EraProgress = [{string.Join(", ", EraProgress.Select(f => f.ToString()))}]"
        );
        ModernBoxLogger.Log(
            $"RenaissanceTime = [{string.Join(", ", RenaissanceTime.Select(f => f.ToString()))}]"
        );
    }
    public static ConstructionCost GetEraConstructionCost()
    {
        float[] v = EraProgress;
        return new ConstructionCost(
            Mathf.RoundToInt(v[0]),
            Mathf.RoundToInt(v[1]),
            Mathf.RoundToInt(v[2]),
            Mathf.RoundToInt(v[3])
        );
    }
    public static ConstructionCost GetStupidRenaissanceShitCost()
    {
        float[] v = RenaissanceTime;
        return new ConstructionCost(
            Mathf.RoundToInt(v[0]),
            Mathf.RoundToInt(v[1]),
            Mathf.RoundToInt(v[2]),
            Mathf.RoundToInt(v[3])
        );
    }
    public static float ScaleBalance(float value)
    {
        switch (Balance)
        {
            case BalanceMode.Weak:
                return value * 0.15f;
            case BalanceMode.VanillaFriendly:
                return value * 0.5f;
            case BalanceMode.AssymetricalWarfare:
                return value * 0.65f;
            case BalanceMode.OGModernBox:
                return value * 1.5f;
            case BalanceMode.Carnage:
                return value * 3.5f;
            default:
                return value;
        }
    }
}
