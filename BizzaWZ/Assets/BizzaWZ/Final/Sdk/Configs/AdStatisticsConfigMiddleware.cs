#if BIZZA_REAL_WITHDRAW
using System;
using System.IO;
using System.Text;
using Bizza.Sdk;

/// <summary>
/// 广告数值配置的统一入口。
/// 业务层不需要感知配置来自远端分组还是本地 ChannelConfig。
/// </summary>
public static class AdStatisticsConfigMiddleware
{
    public static GameAB_CustomData Get(int currentLevel)
    {
#if BIZZA_REMOTEGROUPDATA
        if (RemoteGroupDataSystem.current.TryGetActiveAdStatistics(currentLevel, out var remoteConfig))
        {
            return remoteConfig;
        }

        return default;
#else
        return GetLocalConfig(currentLevel);
#endif
    }

    private static GameAB_CustomData GetLocalConfig(int currentLevel)
    {
        if (ChannelConfig.Instance == null)
        {
            return default;
        }

        var ranges = ChannelConfig.Instance.adStatisticsLevelRanges;
        if (ranges == null || ranges.Count == 0)
        {
            return default;
        }

        var level = Math.Max(1, currentLevel);
        var selectedStartLevel = int.MinValue;
        var selectedConfig = default(GameAB_CustomData);
        for (var i = 0; i < ranges.Count; i++)
        {
            var range = ranges[i];
            if (range == null)
            {
                continue;
            }

            var startLevel = Math.Max(1, range.StartLevel);
            var endLevel = Math.Max(startLevel, range.EndLevel);
            if (level < startLevel || level > endLevel || startLevel < selectedStartLevel)
            {
                continue;
            }

            selectedStartLevel = startLevel;
            selectedConfig = range.Statistics;
        }

        return selectedConfig;
    }
}

#if BIZZA_REMOTEGROUPDATA
internal static class RemoteAdStatisticsConfigBinarySerializer
{
    private const int BinaryVersion = 1;
    private static readonly byte[] BinaryMagic = Encoding.ASCII.GetBytes("CHAB");

    public static bool TryDeserialize(
        byte[] bytes,
        int expectedGroupIndex,
        out GameAB_CustomData config,
        out string errorMessage)
    {
        config = default;
        errorMessage = string.Empty;

        if (bytes == null || bytes.Length == 0)
        {
            errorMessage = "商业化配置内容为空。";
            return false;
        }

        try
        {
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);
            var magic = reader.ReadBytes(BinaryMagic.Length);
            if (magic.Length != BinaryMagic.Length)
            {
                errorMessage = "商业化配置文件头长度无效。";
                return false;
            }

            for (var i = 0; i < BinaryMagic.Length; i++)
            {
                if (magic[i] == BinaryMagic[i])
                {
                    continue;
                }

                errorMessage = "商业化配置 Magic 校验失败。";
                return false;
            }

            var version = reader.ReadInt32();
            if (version != BinaryVersion)
            {
                errorMessage = $"不支持的商业化配置版本：{version}。";
                return false;
            }

            var groupIndex = reader.ReadInt32();
            if (groupIndex != expectedGroupIndex)
            {
                errorMessage = $"商业化配置分组序号不匹配：期望 {expectedGroupIndex}，实际 {groupIndex}。";
                return false;
            }

            config = new GameAB_CustomData
            {
                CloseGetRewardCount = reader.ReadInt32(),
                ShowGetRewardCount = reader.ReadInt32(),
                ShowDollarCount = reader.ReadInt32(),
                InterAdCooldownMs = reader.ReadInt32(),
                InterAdStartLevel = reader.ReadInt32(),
                ReviveAdStartLevel = reader.ReadInt32()
            };
            return true;
        }
        catch (Exception exception)
        {
            config = default;
            errorMessage = $"商业化配置解析失败：{exception.Message}";
            return false;
        }
    }
}
#endif
#endif
