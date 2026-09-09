#if BIZZA_IPINTERCEPT && BIZZA_REAL_WITHDRAW
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Bizza.TokenClientSystem;

namespace Bizza.Loading
{
    public sealed class IPCheckTask : LoadingTaskBase
    {
        private readonly AuthService authService = new AuthService();

        public override LoadingTaskName TaskName => LoadingTaskName.TokenClient;
        public override float Weight => 0.3f;

        public override LoadingTaskName[] Dependencies => new LoadingTaskName[]
        {
            LoadingTaskName.LoadTables,
            // 组合接入时先由 SDK 注入公共 DeviceInfoUtil；IP 只读取，不重复传参。
            LoadingTaskName.WKY_SDK
        };

        public override async UniTask Execute()
        {
            
            AuthFlow authFlow = new AuthFlow(authService);
            await authFlow.Init();
        }
    }
}
#endif
