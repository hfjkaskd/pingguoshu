using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class RemoteGroupConfigWindow : EditorWindow
{
    private const string DefaultRemoteUploadRootDirectory = "RemoteUpload";
    private const string DefaultStrategyOutputDirectory = "RemoteUpload/Common";
    private const string LegacyStrategyOutputDirectory = "Assets/RemoteData";
    private const string LegacyPublicStrategyOutputDirectory = "RemoteUpload";
    private const string DefaultStrategyOutputFileName = "group.bytes";
    private const string StrategyOutputDirectoryPrefsKey = "RemoteGroupConfigWindow.StrategyOutputDirectory";
    private const string StrategyOutputFileNamePrefsKey = "RemoteGroupConfigWindow.StrategyOutputFileName";

    private RemoteGroupRuntimeConfig config = RemoteGroupRuntimeConfig.CreateDefault();
    private RemoteGroupStrategy strategy = CreateDefaultStrategy();
#if BIZZA_REAL_WITHDRAW
    private ChannelABConfig channelABConfig;
#endif
    private string strategyOutputDirectory = DefaultStrategyOutputDirectory;
    private string strategyOutputFileName = DefaultStrategyOutputFileName;
    private bool editorOverrideEnabled;
    private string editorOverrideUserGroupName = RemoteGroupRuntimeConfig.DefaultUserGroupDefaultName;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Remote System/Remote Group Config")]
    private static void Open()
    {
        var window = GetWindow<RemoteGroupConfigWindow>("Remote Group Config");
        window.minSize = new Vector2(460f, 360f);
        window.Show();
    }

    private void OnEnable()
    {
        LoadStrategyOutputPrefs();
        LoadEditorOverridePrefs();
        LoadFromFile();
        LoadStrategyFromPath(GetStrategyOutputPath(), false);
#if BIZZA_REAL_WITHDRAW
        TryFindChannelABConfigAsset();
#endif
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("Remote Group Runtime Config", EditorStyles.boldLabel);
        EditorGUILayout.Space(4f);

        config.RemoteRootUrl = EditorGUILayout.TextField("Remote Root Url", config.RemoteRootUrl);
        config.RemoteGroupDataName = EditorGUILayout.TextField("Group Data Name", config.RemoteGroupDataName);
        config.GameDataDirectoryName = EditorGUILayout.TextField("Game Data Directory", config.GameDataDirectoryName);
        config.CommercialDirectoryName = EditorGUILayout.TextField("Commercial Directory", config.CommercialDirectoryName);
        config.CommercialConfigName = EditorGUILayout.TextField("Commercial Config Name", config.CommercialConfigName);
        config.StartCommercialGroupIndexAtOne = EditorGUILayout.Toggle(
            "Commercial Index Starts At One",
            config.StartCommercialGroupIndexAtOne);
        config.CommonDirectoryName = EditorGUILayout.TextField("Common Directory", config.CommonDirectoryName);
        config.Timeout = EditorGUILayout.FloatField("Timeout", config.Timeout);
        config.RetryTimes = EditorGUILayout.IntField("Retry Times", config.RetryTimes);
        config.UserGroupDefaultName = EditorGUILayout.TextField("Default Group", config.UserGroupDefaultName);
        config.UserGroupNamePrefsKey = EditorGUILayout.TextField("Group Prefs Key", config.UserGroupNamePrefsKey);
        config.CountryGroupEnabled = EditorGUILayout.Toggle("Country Group Enabled", config.CountryGroupEnabled);
        if (config.CountryGroupEnabled)
        {
            EditorGUILayout.HelpBox($"开启后，具体国家使用对应国家分组策略；未使用国家目录时统一使用 {config.CommonDirectoryName} 公共目录；开启国家相关配置且 CountryType.None 时使用本地 Default。", MessageType.Info);
        }

        config.CountryLevelEnabled = EditorGUILayout.Toggle("Country Level Enabled", config.CountryLevelEnabled);
        if (config.CountryLevelEnabled)
        {
            EditorGUILayout.HelpBox($"开启后，具体国家的非 Default 关卡使用对应国家目录；关闭时统一使用 {config.CommonDirectoryName} 公共目录；开启国家相关配置且 CountryType.None 时使用本地 Default。", MessageType.Info);
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.HelpBox($"Export path: {RemoteGroupRuntimeConfig.ResourcesAssetPath}", MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Load Exported Config"))
            {
                LoadFromFile();
            }

            if (GUILayout.Button("Reset Defaults"))
            {
                config = RemoteGroupRuntimeConfig.CreateDefault();
            }
        }

        EditorGUILayout.Space(4f);
        if (GUILayout.Button("Export Encrypted Config", GUILayout.Height(32f)))
        {
            ExportEncryptedConfig();
        }

        EditorGUILayout.Space(16f);
        DrawCurrentAssignmentStatus();

        EditorGUILayout.Space(16f);
        DrawEditorOverrideConfig();

        EditorGUILayout.Space(16f);
        DrawStrategyConfig();

        EditorGUILayout.EndScrollView();
    }

    private void LoadFromFile()
    {
        config = RemoteGroupRuntimeConfig.CreateDefault();

        try
        {
            if (!File.Exists(RemoteGroupRuntimeConfig.ResourcesAssetPath))
            {
                return;
            }

            var encryptedText = RemoteTextUtility.NormalizeText(File.ReadAllText(RemoteGroupRuntimeConfig.ResourcesAssetPath));
            if (!RemoteGroupConfigCrypto.TryDecryptToText(encryptedText, out var configJson))
            {
                return;
            }

            var loadedConfig = RemoteJsonUtility.FromJson<RemoteGroupRuntimeConfig>(configJson);
            if (loadedConfig != null)
            {
                config = loadedConfig.CloneSanitized();
            }
        }
        catch
        {
            config = RemoteGroupRuntimeConfig.CreateDefault();
        }
    }

    private void ExportEncryptedConfig()
    {
        try
        {
#if BIZZA_REAL_WITHDRAW
            if (channelABConfig != null)
            {
                config.CommercialDirectoryName = RemoteGroupRuntimeConfig.DefaultCommercialDirectoryName;
                config.CommercialConfigName = channelABConfig.groupConfigName;
                config.StartCommercialGroupIndexAtOne = channelABConfig.StartGroupIndexAtOne;
            }
#endif
            config = config.CloneSanitized();
            var configJson = JsonUtility.ToJson(config, true);
            var encryptedText = RemoteGroupConfigCrypto.EncryptToText(configJson);
            var directory = Path.GetDirectoryName(RemoteGroupRuntimeConfig.ResourcesAssetPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(RemoteGroupRuntimeConfig.ResourcesAssetPath, encryptedText, RemoteTextUtility.Utf8WithoutBom);
            AssetDatabase.ImportAsset(RemoteGroupRuntimeConfig.ResourcesAssetPath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Remote Group Config", "Encrypted config exported.", "OK");
        }
        catch (System.Exception exception)
        {
            EditorUtility.DisplayDialog("Remote Group Config", $"Export failed: {exception.Message}", "OK");
        }
    }

    private void DrawCurrentAssignmentStatus()
    {
        EditorGUILayout.LabelField("Current Assignment", EditorStyles.boldLabel);
        EditorGUILayout.Space(4f);

        var dataSystem = RemoteGroupDataSystem.current;
        var groupName = dataSystem.GetUserGroupName();
        var source = dataSystem.GetCurrentDataSource();
        var assignmentCompleted = dataSystem.HasCompletedAssignment();
        var path = dataSystem.GetCurrentGameDataPath();

        EditorGUILayout.LabelField("Current Group", groupName);
        EditorGUILayout.LabelField("Assignment Completed", assignmentCompleted ? "Yes" : "No");
        EditorGUILayout.LabelField("Data Source", GetDataSourceLabel(source));
        EditorGUILayout.LabelField("Game Data Path", path);

        if (!dataSystem.TryGetCurrentGameDataBytes(out var gameData))
        {
            EditorGUILayout.HelpBox("Current game data is not available. Runtime does not parse level data yet; this window only inspects the stored binary.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("First Level Decoded Preview", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"Raw size: {gameData.Length} bytes");
        EditorGUILayout.TextArea(RemoteGroupLevelPreviewUtility.BuildPreview(gameData), GUILayout.MinHeight(220f));
        EditorGUILayout.HelpBox("当前窗口会在编辑器侧解码包头、公共字段和第一关记录。项目运行时暂不读取关卡二进制，这个预览不会带入真机。", MessageType.Info);
    }

    private static string GetDataSourceLabel(RemoteGroupDataSource source)
    {
        switch (source)
        {
            case RemoteGroupDataSource.LocalDefault:
                return "Local Default (StreamingAssets)";
            case RemoteGroupDataSource.Remote:
                return "Remote Download (Sandbox)";
            default:
                return "Not Loaded";
        }
    }

    private void DrawStrategyConfig()
    {
        EnsureStrategy();

        EditorGUILayout.LabelField("Remote Group Strategy", EditorStyles.boldLabel);
        EditorGUILayout.Space(4f);

        EditorGUI.BeginChangeCheck();
        strategyOutputDirectory = EditorGUILayout.TextField("Output Directory", strategyOutputDirectory);
        strategyOutputFileName = EditorGUILayout.TextField("Output File Name", strategyOutputFileName);
        if (EditorGUI.EndChangeCheck())
        {
            SaveStrategyOutputPrefs();
        }

        EditorGUILayout.Space(4f);
        EditorGUILayout.HelpBox($"Remote upload root: {DefaultRemoteUploadRootDirectory}. Strategy output: {DefaultStrategyOutputDirectory}/{DefaultStrategyOutputFileName}. This directory is outside Assets and is not included in the Unity build.", MessageType.Info);

#if BIZZA_REAL_WITHDRAW
        EditorGUILayout.Space(4f);
        channelABConfig = (ChannelABConfig)EditorGUILayout.ObjectField(
            "Commercial Config",
            channelABConfig,
            typeof(ChannelABConfig),
            false);

        if (channelABConfig == null)
        {
            EditorGUILayout.HelpBox("导出分组策略时会同时导出商业化二进制配置，请先指定 ChannelABConfig 资源。", MessageType.Warning);
        }
        else
        {
            var indexMode = channelABConfig.StartGroupIndexAtOne ? "从 1 开始" : "从 0 开始";
            var countryCount = channelABConfig.gameAB_CustomDatas == null ? 0 : channelABConfig.gameAB_CustomDatas.Count;
            EditorGUILayout.HelpBox($"商业化配置：{countryCount} 个国家配置，文件前缀：{channelABConfig.groupConfigName}，分组序号{indexMode}。文件统一导出到 {DefaultRemoteUploadRootDirectory}/commercial/ 或对应国家/commercial/ 目录；None 使用本地配置。", MessageType.Info);
        }
#endif

        EditorGUILayout.Space(4f);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("User Group Name", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("Weight", EditorStyles.miniBoldLabel, GUILayout.Width(96f));
            GUILayout.Space(72f);
        }

        for (var i = 0; i < strategy.Datas.Count; i++)
        {
            var data = strategy.Datas[i];
            if (data == null)
            {
                data = new GroupData();
                strategy.Datas[i] = data;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                data.UserGroupName = EditorGUILayout.TextField(data.UserGroupName);
                data.Weight = EditorGUILayout.FloatField(data.Weight, GUILayout.Width(96f));

                if (GUILayout.Button("Remove", GUILayout.Width(72f)))
                {
                    strategy.Datas.RemoveAt(i);
                    GUIUtility.ExitGUI();
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Group"))
            {
                strategy.Datas.Add(new GroupData { UserGroupName = config.UserGroupDefaultName, Weight = 1f });
            }

            if (GUILayout.Button("Reset Strategy"))
            {
                strategy = CreateDefaultStrategy();
                strategyOutputDirectory = DefaultStrategyOutputDirectory;
                strategyOutputFileName = DefaultStrategyOutputFileName;
                SaveStrategyOutputPrefs();
            }
        }

        EditorGUILayout.Space(4f);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Load Strategy"))
            {
                LoadStrategyFromPath(GetStrategyOutputPath());
            }

            if (GUILayout.Button("Export Strategy", GUILayout.Height(28f)))
            {
                ExportStrategy();
            }
        }
    }

    private void DrawEditorOverrideConfig()
    {
        EditorGUILayout.LabelField("Editor User Group Override", EditorStyles.boldLabel);
        EditorGUILayout.Space(4f);

        EditorGUI.BeginChangeCheck();
        editorOverrideEnabled = EditorGUILayout.Toggle("Enable In Editor", editorOverrideEnabled);
        using (new EditorGUI.DisabledScope(!editorOverrideEnabled))
        {
            editorOverrideUserGroupName = EditorGUILayout.TextField("User Group Name", editorOverrideUserGroupName);
        }

        if (EditorGUI.EndChangeCheck())
        {
            SaveEditorOverridePrefs();
        }

        if (GUILayout.Button("Clear Cached Remote Group", GUILayout.Height(26f)))
        {
            ClearCachedRemoteGroup();
        }

        EditorGUILayout.HelpBox("Only affects Unity Editor play mode. Disable it to simulate the real player weighted strategy cache.", MessageType.Info);
    }

    private void LoadStrategyOutputPrefs()
    {
        strategyOutputDirectory = EditorPrefs.GetString(StrategyOutputDirectoryPrefsKey, string.Empty);
        strategyOutputFileName = EditorPrefs.GetString(StrategyOutputFileNamePrefsKey, DefaultStrategyOutputFileName);
        var normalizedStrategyOutputDirectory = string.IsNullOrWhiteSpace(strategyOutputDirectory)
            ? string.Empty
            : strategyOutputDirectory.Trim().TrimEnd('/', '\\');

        if (string.IsNullOrWhiteSpace(strategyOutputDirectory)
            || string.Equals(normalizedStrategyOutputDirectory, LegacyStrategyOutputDirectory, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedStrategyOutputDirectory, LegacyPublicStrategyOutputDirectory, StringComparison.OrdinalIgnoreCase))
        {
            strategyOutputDirectory = DefaultStrategyOutputDirectory;
        }

        if (string.IsNullOrWhiteSpace(strategyOutputFileName))
        {
            strategyOutputFileName = DefaultStrategyOutputFileName;
        }

        if (!strategyOutputFileName.EndsWith(".bytes", StringComparison.OrdinalIgnoreCase))
        {
            strategyOutputFileName = Path.GetFileNameWithoutExtension(strategyOutputFileName) + ".bytes";
        }
    }

    private void SaveStrategyOutputPrefs()
    {
        EditorPrefs.SetString(StrategyOutputDirectoryPrefsKey, strategyOutputDirectory);
        EditorPrefs.SetString(StrategyOutputFileNamePrefsKey, strategyOutputFileName);
    }

    private void LoadEditorOverridePrefs()
    {
        editorOverrideEnabled = RemoteGroupEditorOverride.Enabled;
        editorOverrideUserGroupName = RemoteGroupEditorOverride.UserGroupName;
    }

    private void SaveEditorOverridePrefs()
    {
        RemoteGroupEditorOverride.Enabled = editorOverrideEnabled;
        RemoteGroupEditorOverride.UserGroupName = editorOverrideUserGroupName;
        editorOverrideUserGroupName = RemoteGroupEditorOverride.UserGroupName;
    }

    private void ClearCachedRemoteGroup()
    {
        RemoteGroupDataSystem.current.ClearCachedRemoteData();
        RemoteGroupEditorOverride.ClearSavedGroupMarker();
        EditorUtility.DisplayDialog("Remote Group Config", "Cached remote group data cleared.", "OK");
    }

    private void LoadStrategyFromPath(string path, bool showDialog = true)
    {
        try
        {
            if (!File.Exists(path))
            {
                if (showDialog)
                {
                    EditorUtility.DisplayDialog("Remote Group Strategy", $"Strategy file not found: {path}", "OK");
                }

                return;
            }

            var loadedStrategy = LoadStrategyByExtension(path);
            if (loadedStrategy == null)
            {
                if (showDialog)
                {
                    EditorUtility.DisplayDialog("Remote Group Strategy", "Strategy parse failed.", "OK");
                }

                return;
            }

            strategy = loadedStrategy;
            EnsureStrategy();

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Remote Group Strategy", "Strategy loaded.", "OK");
            }
        }
        catch (System.Exception exception)
        {
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Remote Group Strategy", $"Load failed: {exception.Message}", "OK");
            }
        }
    }

    private RemoteGroupStrategy LoadStrategyByExtension(string path)
    {
        var extension = Path.GetExtension(path);
        if (string.Equals(extension, ".bytes", StringComparison.OrdinalIgnoreCase))
        {
            var encryptedText = RemoteTextUtility.NormalizeText(File.ReadAllText(path));
            if (!RemoteGroupConfigCrypto.TryDecryptToText(encryptedText, out var base64Binary))
            {
                throw new InvalidDataException("Binary group strategy decrypt failed.");
            }

            var strategyBytes = Convert.FromBase64String(base64Binary.Trim());
            return RemoteGroupBinaryUtility.FromStrategyBytes(strategyBytes);
        }

        throw new InvalidDataException($"Unsupported group strategy extension: {extension}");
    }

    private void ExportStrategy()
    {
        try
        {
            EnsureStrategy();

#if BIZZA_REAL_WITHDRAW
            ValidateChannelABConfigForExport();
            var countryStrategyPaths = new System.Collections.Generic.List<string>();
#endif

            var outputPath = GetStrategyOutputPath();
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var extension = Path.GetExtension(outputPath);
            if (string.Equals(extension, ".bytes", StringComparison.OrdinalIgnoreCase))
            {
                var strategyBytes = RemoteGroupBinaryUtility.ToStrategyBytes(strategy);
                var encryptedText = RemoteGroupConfigCrypto.EncryptToText(Convert.ToBase64String(strategyBytes));
                File.WriteAllText(outputPath, encryptedText, RemoteTextUtility.Utf8WithoutBom);
#if BIZZA_REAL_WITHDRAW
                countryStrategyPaths = ExportCountryStrategyCopies(outputPath, encryptedText);
#endif
            }
            else
            {
                throw new InvalidDataException($"Unsupported group strategy extension: {extension}");
            }

#if BIZZA_REAL_WITHDRAW
            var commercialOutputPaths = ChannelABConfig.ExportConfigurationFile(
                channelABConfig,
                GetRemoteUploadRootDirectoryPath());
            for (var i = 0; i < commercialOutputPaths.Count; i++)
            {
                ImportAssetIfInProject(commercialOutputPaths[i]);
            }
#endif

            ImportAssetIfInProject(outputPath);
            AssetDatabase.Refresh();
#if BIZZA_REAL_WITHDRAW
            EditorUtility.DisplayDialog(
                "Remote Group Strategy",
                $"Strategy exported: {outputPath}\nCountry strategy copies: {countryStrategyPaths.Count}\nCommercial configs exported: {commercialOutputPaths.Count}",
                "OK");
#else
            EditorUtility.DisplayDialog("Remote Group Strategy", $"Strategy exported: {outputPath}", "OK");
#endif
        }
        catch (System.Exception exception)
        {
            EditorUtility.DisplayDialog("Remote Group Strategy", $"Export failed: {exception.Message}", "OK");
        }
    }

    private string GetStrategyOutputPath()
    {
        var directory = string.IsNullOrWhiteSpace(strategyOutputDirectory) ? DefaultStrategyOutputDirectory : strategyOutputDirectory.Trim();
        var fileName = string.IsNullOrWhiteSpace(strategyOutputFileName) ? DefaultStrategyOutputFileName : strategyOutputFileName.Trim();

        if (!fileName.EndsWith(".bytes", System.StringComparison.OrdinalIgnoreCase))
        {
            fileName = Path.GetFileNameWithoutExtension(fileName) + ".bytes";
        }

        strategyOutputDirectory = directory.Replace("\\", "/");
        strategyOutputFileName = fileName;
        SaveStrategyOutputPrefs();
        return Path.Combine(GetStrategyOutputDirectoryPath(), strategyOutputFileName).Replace("\\", "/");
    }

    private string GetStrategyOutputDirectoryPath()
    {
        var directory = string.IsNullOrWhiteSpace(strategyOutputDirectory)
            ? DefaultStrategyOutputDirectory
            : strategyOutputDirectory.Trim();
        if (Path.IsPathRooted(directory))
        {
            throw new InvalidDataException("远端上传目录必须使用项目根目录下的相对路径，不能填写绝对路径。");
        }

        var projectRoot = Path.GetFullPath(GetProjectRootPath())
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(projectRoot, directory))
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var projectPrefix = projectRoot + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(projectPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("远端上传目录必须位于项目根目录内。");
        }

        var assetsPath = Path.GetFullPath(Application.dataPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var assetsPrefix = assetsPath + Path.DirectorySeparatorChar;
        if (string.Equals(fullPath, assetsPath, StringComparison.OrdinalIgnoreCase)
            || fullPath.StartsWith(assetsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("远端上传目录不能位于 Assets 内，否则资源可能被打进包体。");
        }

        return fullPath.Replace("\\", "/");
    }

    private string GetRemoteUploadRootDirectoryPath()
    {
        var strategyDirectory = GetStrategyOutputDirectoryPath();
        var uploadRootDirectory = Directory.GetParent(strategyDirectory)?.FullName;
        if (string.IsNullOrEmpty(uploadRootDirectory))
        {
            throw new InvalidDataException("无法从分组策略目录确定远端上传根目录。");
        }

        return uploadRootDirectory.Replace("\\", "/");
    }

    private static string GetProjectRootPath()
    {
        var assetsDirectory = new DirectoryInfo(Application.dataPath);
        return assetsDirectory.Parent?.FullName ?? Directory.GetCurrentDirectory();
    }

    private static void ImportAssetIfInProject(string path)
    {
        var assetPath = TryGetAssetPath(path);
        if (!string.IsNullOrEmpty(assetPath))
        {
            AssetDatabase.ImportAsset(assetPath);
        }
    }

    private static string TryGetAssetPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var assetsPath = Path.GetFullPath(Application.dataPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullPath = Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var assetsPrefix = assetsPath + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(assetsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return "Assets/" + fullPath.Substring(assetsPrefix.Length).Replace("\\", "/");
    }

#if BIZZA_REAL_WITHDRAW
    private List<string> ExportCountryStrategyCopies(string outputPath, string encryptedText)
    {
        var exportedPaths = new List<string>();
        if (!config.CountryGroupEnabled || channelABConfig?.gameAB_CustomDatas == null)
        {
            return exportedPaths;
        }

        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrEmpty(outputDirectory))
        {
            throw new InvalidDataException("无法从分组策略文件确定国家分组输出目录。");
        }

        var uploadRootDirectory = Directory.GetParent(outputDirectory)?.FullName;
        if (string.IsNullOrEmpty(uploadRootDirectory))
        {
            throw new InvalidDataException("无法从分组策略目录确定国家分组输出目录。");
        }

        var fileName = Path.GetFileName(outputPath);
        for (var i = 0; i < channelABConfig.gameAB_CustomDatas.Count; i++)
        {
            var countryData = channelABConfig.gameAB_CustomDatas[i];
            if (countryData == null || countryData.e_CountryType == AccountModule.E_CountryType.None)
            {
                continue;
            }

            var countryDirectory = Path.Combine(uploadRootDirectory, countryData.e_CountryType.ToString());
            Directory.CreateDirectory(countryDirectory);
            var countryPath = Path.Combine(countryDirectory, fileName);
            File.WriteAllText(countryPath, encryptedText, RemoteTextUtility.Utf8WithoutBom);
            exportedPaths.Add(countryPath.Replace("\\", "/"));
        }

        return exportedPaths;
    }

    private void TryFindChannelABConfigAsset()
    {
        var guids = AssetDatabase.FindAssets("t:ChannelABConfig");
        if (guids.Length != 1)
        {
            return;
        }

        var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        channelABConfig = AssetDatabase.LoadAssetAtPath<ChannelABConfig>(assetPath);
    }

    private void ValidateChannelABConfigForExport()
    {
        if (channelABConfig == null)
        {
            throw new InvalidDataException("请先在 Remote Group Config 窗口中指定 ChannelABConfig 资源。");
        }

        if (channelABConfig.gameAB_CustomDatas == null || channelABConfig.gameAB_CustomDatas.Count == 0)
        {
            throw new InvalidDataException("ChannelABConfig 中没有国家商业化配置。");
        }

        var countrySet = new System.Collections.Generic.HashSet<AccountModule.E_CountryType>();
        for (var i = 0; i < channelABConfig.gameAB_CustomDatas.Count; i++)
        {
            var countryData = channelABConfig.gameAB_CustomDatas[i];
            if (countryData == null)
            {
                throw new InvalidDataException($"ChannelABConfig 第 {i + 1} 项国家配置为空。");
            }

            if (!countrySet.Add(countryData.e_CountryType))
            {
                throw new InvalidDataException($"ChannelABConfig 中国家 {countryData.e_CountryType} 配置重复。");
            }

            var commercialCount = countryData.gameAB_Datas == null ? 0 : countryData.gameAB_Datas.Count;
            if (commercialCount != strategy.Datas.Count)
            {
                throw new InvalidDataException(
                    $"国家 {countryData.e_CountryType} 的商业化配置数量必须和分组策略数量一致：策略={strategy.Datas.Count}，商业化配置={commercialCount}。");
            }
        }
    }
#endif

    private void EnsureStrategy()
    {
        if (strategy == null)
        {
            strategy = CreateDefaultStrategy();
        }

        if (strategy.Datas == null)
        {
            strategy.Datas = new System.Collections.Generic.List<GroupData>();
        }
    }

    private static RemoteGroupStrategy CreateDefaultStrategy()
    {
        return new RemoteGroupStrategy
        {
            Datas = new System.Collections.Generic.List<GroupData>
            {
                new GroupData
                {
                    UserGroupName = RemoteGroupRuntimeConfig.DefaultUserGroupDefaultName,
                    Weight = 1f
                }
            }
        };
    }
}
