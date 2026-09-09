# Remote Group 配置工具

编辑器菜单：

```text
Tools → Remote System → Remote Group Config
```

## 一、运行时配置

在窗口中配置并导出：

| 配置 | 说明 |
|---|---|
| `Remote Root Url` | 远端根地址，末尾建议保留 `/` |
| `Group Data Name` | 分组策略文件名，默认 `group.bytes`，必须是 `.bytes` |
| `Game Data Directory` | 关卡目录，默认 `gamedata` |
| `Common Directory` | 公共远端资源目录，默认 `Common` |
| `Timeout` | 单次请求超时时间，默认 5 秒 |
| `Retry Times` | 远端请求重试次数，默认 2 次 |
| `Default Group` | 默认分组名，默认 `Default` |
| `Group Prefs Key` | PlayerPrefs 分组名 Key |
| `Country Group Enabled` | 是否按国家目录读取分组策略 |
| `Country Level Enabled` | 是否按国家目录读取非 Default 关卡 |

点击 `Export Encrypted Config` 后生成：

```text
Assets/Resources/RemoteGroupRuntimeConfig.bytes
```

运行时通过 `Resources/RemoteGroupRuntimeConfig` 读取，不要手工编辑 `.bytes` 内容。

## 二、分组策略

在窗口下方维护 `User Group Name` 和 `Weight`，然后导出策略。

默认导出位置：

```text
RemoteUpload/Common/group.bytes
```

分组名必须和远端关卡文件名一致。假设策略中有：

```text
Default、Main
```

则远端公共目录为：

```text
RemoteUpload/
├─ Common/
│  ├─ group.bytes
│  └─ gamedata/
│     └─ Main.bytes
```

`Default` 不下载远端文件，始终使用本地：

```text
Assets/StreamingAssets/RemoteGroup/Default.bytes
```

## 三、推荐的关卡配置组织方式

运行时会按最终分组名请求一个文件：

```text
{RemoteRootUrl}/Common/gamedata/{UserGroupName}.bytes
```

为了减少请求次数，建议：

- 尽量减少非 `Default` 分组数量。
- 将多个关卡打包到同一个分组二进制文件中，例如 `Common/gamedata/Main.bytes`。
- 不要按关卡拆成 `level1.bytes`、`level2.bytes`；当前运行时不会按关卡名请求这些文件。
- 当前远端系统只负责下载、缓存并返回该二进制内容；关卡二进制的具体解析由业务侧负责。

如果开启 `CountryLevelEnabled`，每个国家仍需要一个对应的聚合文件：

```text
RemoteUpload/
├─ Common/gamedata/Main.bytes
├─ US/gamedata/Main.bytes
├─ BR/gamedata/Main.bytes
└─ ID/gamedata/Main.bytes
```

## 四、国家目录规则

### `CountryGroupEnabled = false`

分组策略使用公共文件：

```text
{RemoteRootUrl}/Common/group.bytes
```

### `CountryGroupEnabled = true`

具体国家使用：

```text
{RemoteRootUrl}/{Country}/group.bytes
```

### `CountryLevelEnabled = false`

非 Default 关卡使用公共目录：

```text
{RemoteRootUrl}/Common/gamedata/{UserGroupName}.bytes
```

### `CountryLevelEnabled = true`

具体国家使用：

```text
{RemoteRootUrl}/{Country}/gamedata/{UserGroupName}.bytes
```

当任一国家开关开启且最终 `AccountModule.CountryType == None` 时：

- 不请求远端。
- 使用本地 `Default.bytes`。
- 不写入正式分组完成记录。

## 五、商业化配置

在窗口中指定 `ChannelABConfig` 后，导出分组策略时会同时导出商业化二进制。

默认目录：

```text
RemoteUpload/
├─ commercial/
│  └─ ChannelConfig{index}.bytes
└─ {Country}/
   └─ commercial/
      └─ ChannelConfig{index}.bytes
```

规则：

- `e_CountryType = None` 导出到公共 `commercial/`。
- 具体国家导出到 `{Country}/commercial/`。
- `{index}` 由 `StartGroupIndexAtOne` 决定从 `0` 或 `1` 开始。
- 当前 `InitRemoteGroupDataTask` 不负责商业化文件的运行时读取。

## 六、运行时接入

需要满足以下条件：

1. 包含 `RemoteGroupDataSystem`、`RemoteGroupRuntimeConfig` 及其依赖脚本。
2. 提供本地 `Assets/StreamingAssets/RemoteGroup/Default.bytes`。
3. 在 Loading 流程中添加：

   ```csharp
   new InitRemoteGroupDataTask()
   ```

4. 国家配置开启时，确保 `AccountModule.CountryType` 在用户数据加载后能够得到最终值。
5. 业务侧通过以下接口获取结果：

   ```csharp
   RemoteGroupDataSystem.current.GetUserGroupName();
   RemoteGroupDataSystem.current.GetCurrentDataSource();
   RemoteGroupDataSystem.current.GetCurrentGameDataPath();
   RemoteGroupDataSystem.current.TryGetCurrentGameDataBytes(out var bytes);
   ```

`InitRemoteGroupDataTask` 负责等待条件并调用初始化；远端系统负责请求、随机、缓存、正式分组记录和本地 Default 回退。

## 七、测试

菜单：

```text
Tools → Remote System → Remote Group Test
```

推荐顺序：

1. `1. 重新读取 Remote Group Config`。
2. `检查当前远端资源`，确认策略和关卡 URL 返回成功。
3. `清理缓存并执行当前初始化`，测试首次请求和分组。
4. 再执行一次普通初始化，测试正式记录和沙盒缓存复用。
5. 国家配置开启后，使用 `批量测试全部国家（包含 None 公共配置）`。

测试窗口直接调用 `RemoteGroupDataSystem.InitGameData()`；如果要验证完整 `InitRemoteGroupDataTask` 与 `LoadUserDataTask` 的等待关系，需要通过游戏完整 Loading 流程测试。

## 八、远端发布检查

`RemoteUpload` 是远端根目录，不会自动进入 Unity 包体。上传后至少检查：

```text
{RemoteRootUrl}/Common/group.bytes
{RemoteRootUrl}/Common/gamedata/{UserGroupName}.bytes
{RemoteRootUrl}/{Country}/group.bytes
{RemoteRootUrl}/{Country}/gamedata/{UserGroupName}.bytes
```

其中 `Default.bytes` 只随 Unity 包体提供，不需要上传到远端。
