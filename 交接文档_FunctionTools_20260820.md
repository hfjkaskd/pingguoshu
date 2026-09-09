# FunctionTools 迁移与复制工具交接文档

更新时间：2026-08-20  
交接对象：后续继续处理本项目的 Codex

## 1. 工作范围

当前 Unity 工程：

`E:\Puzzle_Project\BizzaWZ\BizzaWZ`

主要目标：把 Connect_SDK、IP 检测工具和屏幕采集保护整理成可一键复制的功能模块；公共代码只维护一份，SDK/IP 同时存在时不能产生重复脚本或重复 `.meta`。每次导出到独立工具目录时，一个工具的功能代码和公共代码必须全部收敛在用户选择的目标目录内。

测试工程：

`E:\WKY`

IP 模块规划目标：

`E:\Puzzle_Project\ARealTools\UserInformationVerification`

## 2. 已完成的结构调整

### 公共目录

公共代码统一放在：

`Assets/BizzaWZ/Final/FunctionTools/CommonPublic`

当前包含：

- `DeviceInfo/`：`DeviceInfoUtil`、`DeviceNativeBridge`、Android Java Bridge、事件和日志依赖；同时覆盖 Android 构建模板中的 `DeviceUtils.java`。
- `Contracts/`：用户信息验证公共契约程序集。
- `UI/`：`CommonConfirmTipsPanel.cs` 及 `CommonConfirmTipsPanel.prefab`。
- `Logging/`：`LogLogger.cs`。
- `Editor/`：`PostProcessBuild_Android.cs`，带平台和宏保护。

屏幕采集保护（SCB）跟随 IP 功能目录复制，不单独拆包：

`Assets/BizzaWZ/Final/FunctionTools/UserInformationVerification/Runtime/SCB`

`_Bizza_BuildTemplate` 是公共 Android 构建模板，按完整目录递归复制，不再逐文件维护：

`_Bizza_BuildTemplate/`

## 3. 关键边界，后续不要改错

- `AccountModule.cs` 必须保留在 SDK：
  `Assets/BizzaWZ/Final/Connect_SDK/SDK_WKY/API/AccountModule.cs`
- 不要把 `AccountModule.cs` 移入 CommonPublic 或 IP 模块。
- IP 独立模式使用：
  `FunctionTools/StandaloneSources/ChannelConfig.cs`
- SDK 共存时，IP 工具会检测目标目录中 SDK 的 `ChannelConfig.cs`，检测到后自动跳过独立版 `ChannelConfig.cs`，复用 SDK 配置。
- SDK 的正式 `ChannelConfig.cs` 仍由 SDK 自己导出。
- 不要在清单中同时添加某个目录项和该目录下的显式 `.meta` 文件；目录项会自动复制对应目录 `.meta`。
- 不要恢复已经迁移到 CommonPublic 的 `LogLogger`、`CommonConfirmTipsPanel`、`PostProcessBuild_Android` 旧路径副本，否则会产生同名类型或重复资源。

## 4. 复制清单

清单目录：

`Assets/BizzaWZ/Final/FunctionTools/Manifests`

### CommonPublicFileManifest.json

当前只有 3 个递归/文件项：

1. `CommonPublic` 整体递归复制到 `@SHARED/CommonPublic`。
2. `_Bizza_BuildTemplate` 整体递归复制到 `@SHARED/_Bizza_BuildTemplate`。
3. `Assets/Plugins/Android/mainTemplate.gradle` 复制到 `@SHARED/Android/mainTemplate.gradle`。

### SDKFileManifest.json

包含：

- `CommonPublicFileManifest`
- `SDKCommonFileManifest`
- SDK_WKY 功能目录（排除已由公共清单/SDKCommon 提供的设备、事件、加密、HTTP 文件）
- BizzaAnalytics
- Android 插件目录（排除 `mainTemplate.gradle`，由公共清单提供）
- StreamingAssets
- 可选迁移说明文件

SDK 的 AccountModule、SDK ChannelConfig 等 SDK 业务代码仍在 SDK 清单范围内。

### SDKCommonFileManifest.json

放 SDK 仍需要、但不适合放进 IP 公共目录的公共依赖：

- Framework Event
- EncryptionUtil
- HttpUtil
- GameEvent

### IPBlockFileManifest.json

包含：

- `CommonPublicFileManifest`
- `UserInformationVerification` 整体目录，内含 SCB
- 独立 IP 模式的 `FunctionTools/StandaloneSources/ChannelConfig.cs`

IP 功能清单排除了旧 SDK 设备适配入口，设备信息依赖改由 CommonPublic 的设备模块提供。

`@SHARED/...` 的目标规则：导出到同一个 Unity 项目时写入项目公共根目录；导出到项目外的独立工具目录时写入当前目标目录内，不再写到目标目录的父级。

## 5. 复制窗口当前行为

公共复制逻辑：

`Assets/BizzaWZ/Final/FunctionTools/Editor/FunctionFileCopyWindowBase.cs`

SDK 窗口：

`Assets/BizzaWZ/Common/Framework/Editor/SdkFileMigrationWindow.cs`

菜单只保留：

`Bizza/功能工具/SDK 文件复制`

IP 窗口：

`Assets/BizzaWZ/Final/FunctionTools/Editor/IPBlockFileCopyWindow.cs`

### 父项优先规则（本次最新修改）

- 选中目录父项后，其源路径下的子文件、子目录不会再次加入复制计划。
- 子项显示“父项已包含”，选择框禁用。
- 父项 `excludes` 排除的子项仍可以独立处理。
- 目录本身的旁置 `.meta`（例如 `DeviceInfo.meta`）也被视为父项已覆盖，避免重复目标冲突。
- 如果取消父项，子项恢复可单独选择。

### 复制工具的清空策略

- SDK、IP 及继承公共复制基类的工具均关闭“清空前备份”选项。
- 导出确认后直接清空目标目录，再复制当前清单内容。
- 不会新建 `UserInformationVerification.FunctionCopyBackup` 或其他 `.FunctionCopyBackup` 目录。
- 当前已检查 `E:\Puzzle_Project` 和 `E:\WKY`，未发现仍存在的 `.FunctionCopyBackup` 历史目录。

该规则同时作用于界面显示和 `BuildPlan()` 实际复制计划，不能只改界面。

## 6. 当前验证结果

已完成：

- 4 份 JSON 清单解析正常。
- `FunctionFileCopyWindowBase.cs` 花括号匹配。
- CommonPublic、SDK、IP 清单静态复制规划无重复目标文件。
- CommonPublic 目录当前包含设备、契约、UI、日志和 Android Editor 依赖。
- 工程中未发现单独的 `DeviceUtils.cs` C# 文件；`DeviceUtils.java` 位于完整 `_Bizza_BuildTemplate` 中，复制公共构建模板即可带出。

当前 Unity 工程已有 Unity 编辑器实例打开，未强行启动第二个批量编译实例。因此若后续需要最终编译验证，应先关闭现有 Unity，再执行批量编译或在 Unity 界面等待脚本重编译完成。

## 7. 后续建议执行顺序

1. 关闭当前占用 `E:\Puzzle_Project\BizzaWZ\BizzaWZ` 的 Unity 实例。
2. 重新打开工程，确认 Console 没有编译错误。
3. 在 `E:\WKY` 中分别测试：
   - 只导出 IP 工具；
   - 只导出 SDK；
   - 先导出 SDK，再导出 IP；
   - 先导出 IP，再导出 SDK。
4. 检查每种组合下：
   - `CommonPublic` 只有一份；
   - `DeviceInfo.meta` 只有一份；
   - SDK 与 IP 不出现重复全局类型；
   - IP 独立模式有临时 `ChannelConfig.cs`；
   - SDK 共存模式不产生第二份 `ChannelConfig.cs`；
   - SCB 和 `_Bizza_BuildTemplate` 均存在。
5. 如需修改清单，优先修改 JSON；不要在窗口代码中硬编码功能文件。

## 8. 后续修改原则

- 先检查是否属于 CommonPublic，再决定放 SDKCommon、SDK 功能目录还是 IP 功能目录。
- 设备信息采集链（包括 `DeviceInfoUtil` 和 Android `DeviceUtils.java`）视为一个公共设备模块整体迁移。
- 不修改 AccountModule 的归属和 SDK 业务逻辑，除非用户明确提出。
- 不通过增加第二份脚本解决编译问题；优先使用程序集隔离、清单排除和目标路径统一。
- 所有复制规则变更后，都要重新检查目标路径重复和 `.meta` 重复。
