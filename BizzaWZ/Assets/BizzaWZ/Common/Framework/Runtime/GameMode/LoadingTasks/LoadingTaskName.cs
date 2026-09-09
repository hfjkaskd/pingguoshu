namespace Bizza.Loading
{
    /// <summary>
    /// 任务标识
    /// </summary>
      [Obfuz.ObfuzIgnore]
    public enum LoadingTaskName
    {
        None = 0,
    
        LoadTables,
        LoadGameData,
        ReadLanguage,
        LoadGameRes,
        PrewarmFinalFeatures,
        PreLoadAsset,
#if BIZZA_REAL_WITHDRAW
        LoadPlayerEquipment,
        InitRemoteGroupData,
        TokenClient,
        WKY_SDK,
#endif
        LoadGamePlayTask,
        TestWait3s,
    }
}

