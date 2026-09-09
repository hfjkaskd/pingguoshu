# RemoteGroup 系统说明

## 作用

`RemoteGroupDataSystem` 负责首次启动时的远端分组流程和二进制关卡配置缓存。它不解析关卡内容，也不把关卡配置接入具体玩法。

- 读取加密的运行时元配置。
- 请求远端二进制分组策略 `Common/group.bytes` 或 `{Country}/group.bytes`。
- 按权重随机选择 `UserGroupName`。
- 选择 `Default` 时读取 `StreamingAssets/RemoteGroup/Default.bytes`。
- 选择非 `Default` 时请求 `Common/gamedata/{UserGroupName}.bytes` 或 `{Country}/gamedata/{UserGroupName}.bytes`，并保存到沙盒目录。
- 只有分组和对应二进制配置都成功后，才正式记录分组完成状态。

分组策略和关卡配置均只支持二进制方式，不保留 `group.json` 策略读写或 JSON 兼容分支。运行时仍不会解析关卡二进制。

## 编辑器入口

菜单：`Tools/Remote System/Remote Group Config`

窗口提供：

- 加密运行时元配置导出：`Assets/Resources/RemoteGroupRuntimeConfig.bytes`。
- 二进制分组策略导入/导出：公共策略默认导出到项目根目录 `RemoteUpload/Common/group.bytes`。
- 当前分组、是否完成正式分组、数据来源和实际文件路径。
- 当前本地或沙盒二进制关卡配置的编辑器侧预览。
- 导出分组策略时，同步导出 `ChannelABConfig.gameAB_CustomDatas` 中各国家对应的商业化二进制配置。

当前预览会解码已有二进制包的包头、公共字段和第一关记录；剩余包体保持二进制展示，不参与运行时。

商业化配置导出说明：

- 在 `BizzaGame/Channel AB Config` 菜单创建或指定 `ChannelABConfig` 资源。
- `gameAB_CustomDatas` 的每一项代表一个国家配置，项中的 `gameAB_Datas` 数量必须和 Remote Group Strategy 的分组数量一致。
- `e_CountryType = None` 的商业化配置导出到公共 `commercial` 目录，供编辑器或本地流程使用；具体运行时是否使用本地配置由商业化加载方决定。
- `e_CountryType = US` 等具体国家时，导出到对应国家目录的 `commercial` 子目录，例如 `US/commercial/ChannelConfig0.bytes`。
- `StartGroupIndexAtOne = false` 时，每个国家配置列表的第一个元素序号为 `0`；开启后第一个元素序号为 `1`。
- 如果 `groupConfigName = ChannelConfig`，公共配置导出为 `commercial/ChannelConfig0.bytes`，美国配置导出为 `US/commercial/ChannelConfig0.bytes`。

## 编辑器测试窗口

菜单：`Tools/Remote System/Remote Group Test`

测试窗口用于验证完整的远端分组流程，测试资源直接生成到项目根目录的 `RemoteUpload`，目录结构与正式远端资源一致，不会进入 Unity 包体；本地 Default 测试资源会生成到 `Assets/StreamingAssets/RemoteGroup/Default.bytes`。

测试窗口提供以下操作：

- 直接读取 `Resources/RemoteGroupRuntimeConfig` 中由 Remote Group Config 导出的正式运行时配置，不另写测试地址、分组名称或国家开关。
- 不生成、不修改分组策略内容；分组策略必须先由 `RemoteGroupConfigWindow` 按当前配置权重导出，测试窗口只把公共真实策略复制到各国家测试目录，初始化时直接按运行时逻辑正常随机。
- 根据真实策略中的全部 `UserGroupName` 动态生成对应的公共与全部国家关卡二进制，不再假定只有 `group1`、`group2`，也不会遗漏 `group3` 或其它自定义分组；国家前缀开关只影响运行时实际请求的路径，不影响测试资源矩阵的准备。
- 生成内容不同的各分组以及国家标识关卡 RGPK 二进制，用于通过编辑器预览确认当前下载到的是哪个国家、哪个分组；`Default` 仍只写入 `StreamingAssets` 本地默认文件。
- 清理正式分组记录、沙盒策略缓存和沙盒关卡缓存，同时关闭 Editor User Group Override。
- 直接执行 `RemoteGroupDataSystem.InitGameData()`，观察 `LogLogger.LogGameConfigLoad` 中文日志和当前数据来源。
- 删除当前测试远端关卡文件，用于模拟下载失败并验证本地 Default 回退和下次启动重试。
- 批量遍历 `None` 及全部具体国家，逐个清理缓存、执行初始化并记录每个国家的分组、数据来源和正式完成状态。

推荐测试顺序：先按正式权重导出策略并生成全部关卡资源，清理缓存后执行真实初始化；需要覆盖某个分组时，临时调整主窗口中的权重或使用足够大的权重差，重新导出策略、清理缓存并重复执行，确认结果来自正常随机。随后删除随机命中的分组关卡文件验证失败回退；恢复文件后再次执行，确认系统重新下载并正式记录远端分组。

## 远端上传目录

编辑器默认把可上传资源统一导出到项目根目录的 `RemoteUpload`。该目录位于 `Assets` 外，不会被 Unity 导入，也不会打进包体；导出完成后可以直接按下面的结构上传到 `RemoteRootUrl` 对应的服务器目录。

| 本地导出/放置路径 | 远端资源用途 | 生成或放置方式 |
| --- | --- | --- |
| `RemoteUpload/Common/group.bytes` | 公共分组策略 | Remote Group Config 导出；`CountryGroupEnabled = false` 时由具体国家共用；两个国家开关关闭时，即使 `CountryType.None` 也会正常请求并随机 |
| `RemoteUpload/{Country}/group.bytes` | 国家分组策略 | 开启 `CountryGroupEnabled`，且 `ChannelABConfig` 中存在对应国家时由导出操作自动复制 |
| `RemoteUpload/commercial/ChannelConfig{index}.bytes` | 公共商业化配置 | `ChannelABConfig.e_CountryType = None` 时导出；`None` 运行时使用本地配置 |
| `RemoteUpload/{Country}/commercial/ChannelConfig{index}.bytes` | 国家商业化配置 | `ChannelABConfig.e_CountryType = {Country}` 时导出 |
| `RemoteUpload/Common/gamedata/{GroupName}.bytes` | 公共非 Default 关卡配置 | 由关卡资源导出工具或手动放置；`CountryLevelEnabled = false`，或两个国家开关关闭时使用公共模式 |
| `RemoteUpload/{Country}/gamedata/{GroupName}.bytes` | 国家非 Default 关卡配置 | 由关卡资源导出工具或手动放置；`CountryLevelEnabled = true` 且当前国家为具体国家 |

说明：

- `{Country}` 必须使用 `AccountModule.E_CountryType` 的枚举名称，例如 `US`；`None` 不创建国家目录。
- `{index}` 来自 `ChannelABConfig.gameAB_CustomDatas` 中对应国家的 `gameAB_Datas` 列表序号，`StartGroupIndexAtOne = false` 时从 `0` 开始，开启后从 `1` 开始。
- 商业化配置统一放在作用域下的 `commercial` 父目录，例如 `commercial/ChannelConfig0.bytes` 或 `US/commercial/ChannelConfig0.bytes`。
- `Default` 关卡配置不放入 `RemoteUpload`，由项目维护者手动放在 `Assets/StreamingAssets/RemoteGroup/Default.bytes`，它是本地回退资源，必须随包体提供。
- 远端关卡配置的 `{GroupName}` 必须与分组策略中的 `UserGroupName` 完全一致，例如策略中有 `group1`，公共远端文件就是 `Common/gamedata/group1.bytes`。
- 当前 `RemoteGroupConfigWindow` 只负责导出分组策略和商业化配置；非 Default 关卡二进制由关卡资源导出工具或手动流程放入上述 `Common/gamedata` 或国家 `gamedata` 目录。
- 目录统一后，旧的公共直铺路径 `RemoteUpload/group.bytes`、`RemoteUpload/gamedata/{GroupName}.bytes`（以及服务器根目录下对应的直铺 URL）不再是公共模式的默认读取位置，需要迁移到 `Common` 目录。

## 运行时元配置

配置字段：

- `RemoteRootUrl`
- `RemoteGroupDataName`，必须是 `.bytes` 文件，默认 `group.bytes`
- `GameDataDirectoryName`，默认 `gamedata`
- `CommonDirectoryName`，公共资源目录，默认 `Common`
- `Timeout`
- `RetryTimes`
- `UserGroupDefaultName`，默认 `Default`
- `UserGroupNamePrefsKey`
- `CountryGroupEnabled`，分组策略是否按国家目录区分，默认关闭
- `CountryLevelEnabled`，非 Default 关卡配置是否按国家目录区分，默认关闭

当 `CountryGroupEnabled = false` 时，公共分组策略统一使用 `Common` 目录：

- `https://example.com/Common/group.bytes`
- `https://example.com/Common/gamedata/A.bytes`

当 `CountryGroupEnabled = true` 且当前国家是具体国家时，分组策略侧运行时读取 `AccountModule.CountryType`：

- 当前国家为 `US` 时：

  - `https://example.com/US/group.bytes`
  - 如果 `CountryLevelEnabled = true`，非 `Default` 关卡为 `https://example.com/US/gamedata/A.bytes`；否则仍使用公共 `https://example.com/Common/gamedata/A.bytes`

- 开启任意国家相关配置且当前国家为 `None` 时，使用本地 `StreamingAssets/RemoteGroup/Default.bytes`，且不写入正式分组完成记录。
- 两个国家开关都关闭时，`None` 仅表示不使用国家目录前缀，仍然请求公共 `Common/group.bytes` 并正常随机。

运行时元配置本身仍然是加密的配置文本资源，用于描述地址和超时等元数据；这不代表分组策略支持 JSON。

## `InitGameData` 流程

1. 加载运行时元配置并重置本次会话状态。
   - `CountryGroupEnabled` 或 `CountryLevelEnabled` 任意开启时，`InitRemoteGroupDataTask` 会先等待 `LoadUserDataTask` 完成，以确定最终国家。
   - 两个国家开关都关闭时，不等待用户数据，直接按公共模式请求分组策略。
2. 如果本地存在已完成的分组记录：
   - 记录为 `Default`：读取 `StreamingAssets` 下的本地默认二进制。
   - 记录为非 `Default`：读取沙盒 `RemoteGroup/gamedata/{UserGroupName}.bytes`。
   - 读取成功就直接使用，跳过分组策略请求和远端关卡下载。
3. 如果开启任意国家相关配置且当前国家为 `None`，直接使用本地 `Default`，不请求远端，也不写入正式分组完成记录。
4. 如果两个国家开关都关闭，即使当前国家为 `None`，也请求公共 `Common/group.bytes`；如果任意国家相关配置开启，则先等待用户数据确定最终国家，再请求公共 Common 或国家版 `group.bytes`。
5. 远端策略选中 `Default`：读取本地默认二进制，成功后正式记录 `Default`。
6. 远端策略选中非 `Default`：下载对应的 `{UserGroupName}.bytes`，保存并回读校验成功后，正式记录该远端分组。
7. 任意远端步骤失败：本次临时使用本地 `Default`，不写入“远端分组完成”标记；下一次启动仍然允许重新请求远端。

## 文件位置

远端上传资源的本地镜像根目录为项目根目录 `RemoteUpload`，目录结构与远端资源目录一致；它不在 `Assets` 内，不会进入 Unity 包体。

假设 `RemoteRootUrl = https://example.com/` 且未开启国家前缀：

- 分组策略：`https://example.com/Common/group.bytes`
- 远端关卡目录：`https://example.com/Common/gamedata/`
- `Default`：运行时使用本地 `StreamingAssets/RemoteGroup/Default.bytes`，不下载远端 Default 文件。
- `A`：`https://example.com/Common/gamedata/A.bytes`
- `B`：`https://example.com/Common/gamedata/B.bytes`

如果开启 `CountryGroupEnabled` 且 `AccountModule.CountryType = US`，分组策略地址会使用 `/US/` 目录；如果开启任意国家相关配置且最终国家为 `None`，则直接使用本地 Default；如果两个开关都关闭，`None` 使用公共目录。

关卡配置使用独立的 `CountryLevelEnabled`：

- 关闭时，非 Default 关卡地址为 `https://example.com/Common/gamedata/A.bytes`。
- 开启且当前国家为 `US` 时，非 Default 关卡地址为 `https://example.com/US/gamedata/A.bytes`。
- 开启任意国家相关配置且当前国家为 `None` 时，不请求远端关卡，直接使用本地 `StreamingAssets/RemoteGroup/Default.bytes`。
- 无论是否开启 `CountryLevelEnabled`，Default 都只读取唯一的本地文件 `StreamingAssets/RemoteGroup/Default.bytes`，不会读取或下载国家版 Default 文件。

本地默认配置：

`Assets/StreamingAssets/RemoteGroup/Default.bytes`

远端下载缓存：

`Application.persistentDataPath/RemoteGroup/gamedata/{UserGroupName}.bytes`

分组策略缓存：

`Application.persistentDataPath/RemoteGroup/group.bytes`

## 正式分组记录

使用 `PlayerPrefs` 保存：

- `RemoteGroup.UserGroupName`
- `RemoteGroup.AssignmentCompleted`

只有以下两种情况会写入 `RemoteGroup.AssignmentCompleted = 1`：

- 远端策略成功选中 `Default`，且本地 Default 二进制配置读取成功。
- 远端策略成功选中非 Default，且对应远端二进制配置保存并校验成功。

远端请求失败时的本地 Default 只属于本次会话，不会写入完成标记。

开启任意国家相关配置且 `AccountModule.CountryType = None` 时会清理旧的正式分组记录，直接使用本地 Default，也不会写入完成标记；两个国家开关都关闭时，`None` 按公共模式处理，可以正式记录远端随机结果。

## 对外接口

- `InitGameData()`
- `GetUserGroupName()`
- `IsDefault()`
- `HasCompletedAssignment()`
- `GetCurrentDataSource()`
- `TryGetCurrentGameDataBytes(out byte[])`
- `HasGameData()`
