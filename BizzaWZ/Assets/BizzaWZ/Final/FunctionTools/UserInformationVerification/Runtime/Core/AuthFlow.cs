using Cysharp.Threading.Tasks;
using Bizza.UserInformationVerification;
using LogLogger = Bizza.UserInformationVerification.AuthLog;

namespace Bizza.TokenClientSystem
{
    /// <summary>
    /// IP 检测统一入口：配置加载 → 公共设备采集 → SCB 保护 → 授权。
    /// SDK 已初始化设备信息时直接复用；独立运行时由本流程完成首次注入。
    /// </summary>
    public sealed class AuthFlow
    {
        private readonly AuthService authService;

        public AuthFlow(AuthService authService = null)
        {
            this.authService = authService ?? new AuthService();
        }

        [System.Obsolete(
            "接入授权 SDK 时请使用 Init()；AppId 会从宿主/DeviceInfoUtil 或 AuthConfig.bytes 读取。",
            false)]
        public UniTask Init(string appId)
        {
            return Init(new DeviceInfoUtil.CustomDeviceInfo
            {
                appId = appId
            });
        }

        /// <summary>
        /// 授权 SDK 的标准初始化入口。
        /// AppId 不由调用方重复传入，按“宿主/DeviceInfoUtil → AuthConfig.bytes”顺序读取。
        /// </summary>
        public UniTask Init()
        {
            return Init((DeviceInfoUtil.CustomDeviceInfo)null);
        }

        public async UniTask Init(DeviceInfoUtil.CustomDeviceInfo customDeviceInfo)
        {
            await AuthConfigLoader.LoadAsync();

            if (!DeviceInfoUtil.IsInitialized)
            {
                DeviceInfoUtil.CustomDeviceInfo deviceInfoConfig =
                    CreateDeviceInfoConfig(customDeviceInfo);
                await DeviceInfoUtil.InitializeAsync(deviceInfoConfig);
            }

            string effectiveAppId = DeviceInfoUtil.Current?.Os_Apid;
            if (string.IsNullOrWhiteSpace(effectiveAppId))
            {
                effectiveAppId = AuthConfig.AppId;
            }

            AuthConfig.SetAppIdOverride(effectiveAppId);
            try
            {
                LogLogger.LogIP("[AuthFlow] 配置加载完成，AppId=" + AuthConfig.AppId);
                LogLogger.LogIP("[AuthFlow] 已复用公共 DeviceInfoUtil 设备快照");
#if !DEBUG_MODE && !UNITY_EDITOR
                if (!ScreenCaptureProtection.TryProtect())
                {
                    const string message = "屏幕采集保护初始化失败，已阻止授权流程。";
                    LogLogger.LogIP("[AuthFlow] " + message);
                    AuthResult protectionFailure = AuthResult.NetworkFailure(
                        string.Empty,
                        "screen capture protection initialization failed");
                    await UserInformationVerificationRuntime.FailureHandler
                        .HandleAsync(protectionFailure);
                    throw new TokenClientBlockedException("SCREEN_CAPTURE_PROTECTION");
                }
#endif
                AuthResult result = await authService.AuthorizeAsync();
                LogLogger.LogIP(result == null
                    ? "[令牌客户端] 授权流程返回空结果"
                    : $"[令牌客户端] 授权流程返回：允许授权={result.Allow}，有服务端响应={result.HasServerResponse}，网络错误={result.IsNetworkError}，原因={result.PublicReason}，requestId={result.RequestId}");

                if (result != null && result.Allow)
                {
                    LogLogger.LogIP($"[令牌客户端] 流程完成：允许授权=true，requestId={result.RequestId}");
                    return;
                }

                LogLogger.LogIP(result == null
                    ? "[令牌客户端] 授权被阻断：结果为空"
                    : $"[令牌客户端] 授权被阻断：原因={result.PublicReason}，错误={result.Error}，requestId={result.RequestId}");
                await UserInformationVerificationRuntime.FailureHandler
                    .HandleAsync(result);
                throw new TokenClientBlockedException(result?.PublicReason ?? "SERVER_ERROR");
            }
            finally
            {
                AuthConfig.ClearAppIdOverride();
            }
        }

        private static DeviceInfoUtil.CustomDeviceInfo CreateDeviceInfoConfig(
            DeviceInfoUtil.CustomDeviceInfo source)
        {
            source = source ?? new DeviceInfoUtil.CustomDeviceInfo();
            return new DeviceInfoUtil.CustomDeviceInfo
            {
                appId = string.IsNullOrWhiteSpace(source.appId)
                    ? AuthConfig.AppId
                    : source.appId,
                country = source.country,
                defaultCountry = source.defaultCountry,
                fakeAndroidId = source.fakeAndroidId
            };
        }
    }

    public sealed class TokenClientBlockedException : System.Exception
    {
        public TokenClientBlockedException(string reason)
            : base($"TokenClient 阻止继续加载：{reason}")
        {
        }
    }
}
