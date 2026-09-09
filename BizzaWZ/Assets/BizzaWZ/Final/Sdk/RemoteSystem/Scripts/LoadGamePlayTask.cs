using Bizza.Loading;
using Bizza.GameAnalytics;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoadGamePlayTask : LoadingTaskBase
{
    public override LoadingTaskName TaskName => LoadingTaskName.LoadGamePlayTask;
    public override float Weight => 10f;

#if BIZZA_REAL_WITHDRAW
    public override LoadingTaskName[] Dependencies => new[]
    {
        LoadingTaskName.LoadGameRes,
        LoadingTaskName.PreLoadAsset,
        LoadingTaskName.LoadGameData,
        LoadingTaskName.LoadTables,
        LoadingTaskName.ReadLanguage,
        LoadingTaskName.PrewarmFinalFeatures,
        LoadingTaskName.InitRemoteGroupData,
    };
#else
    public override LoadingTaskName[] Dependencies => new[]
    {
        LoadingTaskName.LoadGameRes,
        LoadingTaskName.PreLoadAsset,
        LoadingTaskName.LoadGameData,
        LoadingTaskName.InitRemoteGroupData,
    };
#endif

    public override async UniTask Execute()
    {
#if UNITY_EDITOR
        Debug.Log("[WhiteBootstrap] LoadGamePlayTask SortDash preload begin");
#endif
        SetProgress(0.1f);
        await BridgingUtil.LoadGamePlayAsync();
        SetProgress(1f);
#if UNITY_EDITOR
        Debug.Log("[WhiteBootstrap] LoadGamePlayTask SortDash preload end");
#endif
    }
}

public class InitRemoteGroupDataTask : LoadingTaskBase
{
    public override LoadingTaskName TaskName => LoadingTaskName.InitRemoteGroupData;
    public override float Weight => 0.3f;
    public override LoadingTaskName[] Dependencies => System.Array.Empty<LoadingTaskName>();

    public override async UniTask Execute()
    {
        LogLogger.LogGameConfigLoad("开始执行远端分组配置加载任务。");
        SetProgress(0.1f);

        try
        {
            var shouldInitializeRemoteGroup = true;
#if BIZZA_REAL_WITHDRAW
            if (RemoteGroupDataSystem.current.UsesCountryConfig)
            {
                LogLogger.LogGameConfigLoad("已开启国家相关配置，等待 LoadUserDataTask 完成后再确定最终国家和远端资源路径。");
                await UniTask.WaitUntil(() =>
                {
                    var accountModule = AccountModule.Instance;
                    return accountModule != null
                           && (accountModule.LoadingFinish || accountModule.LoadingUserDataFailed);
                });

                if (AccountModule.Instance == null || !AccountModule.Instance.LoadingFinish)
                {
                    LogLogger.LogGameConfigLoad("用户数据加载失败，跳过本次远端分组配置初始化，继续由用户数据任务处理失败流程。");
                    shouldInitializeRemoteGroup = false;
                }
                else
                {
                    LogLogger.LogGameConfigLoad($"LoadUserDataTask 已完成，最终国家={AccountModule.CountryType}，开始初始化远端分组配置。");
                }
            }
            else
            {
                LogLogger.LogGameConfigLoad("未开启国家相关配置，不等待 LoadUserDataTask，直接初始化远端分组配置。");
            }
#endif

            if (shouldInitializeRemoteGroup)
            {
                await RemoteGroupDataSystem.current.InitGameData();

                BizzaAnalyticsAgent.Instance.BUserProp(new System.Collections.Generic.Dictionary<string, object>()
                {
                    {"UserGroup" , RemoteGroupDataSystem.current.GetUserGroupName()}
                });
                BizzaGameAnalytics.SetUserGroup(RemoteGroupDataSystem.current.GetUserGroupName());
            }
        }
        catch (System.Exception exception)
        {
            LogLogger.LogGameConfigLoad($"远端分组配置加载任务异常：{exception.Message}。");
        }

        SetProgress(1f);
        LogLogger.LogGameConfigLoad("远端分组配置加载任务结束。");
    }
}
