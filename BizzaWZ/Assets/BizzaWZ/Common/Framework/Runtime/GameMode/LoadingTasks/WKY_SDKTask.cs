using System;
using System.Collections;
using System.Collections.Generic;
using Bizza.Loading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WKY_SDKTask : LoadingTaskBase
{
    public override LoadingTaskName TaskName => LoadingTaskName.WKY_SDK;
    public override float Weight => 0.1f;
    public override LoadingTaskName[] Dependencies => new[] { LoadingTaskName.LoadTables, LoadingTaskName.LoadGameData };
    public override async UniTask Execute()
    {
#if DEBUG_MODE
        Debug.Log("[WhiteBootstrap] " + TaskName + ".Execute start");
#endif
        SetProgress(0.9f);
        WKY_Flow wKY_Flow = new WKY_Flow();
        await wKY_Flow.Start_Init_Flow();
#if DEBUG_MODE
        Debug.Log($"[WhiteBootstrap] {TaskName} Execute end");
#endif
    }
}
