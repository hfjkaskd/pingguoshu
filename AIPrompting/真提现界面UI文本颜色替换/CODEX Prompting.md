你是 Unity 工程助手。

现在需要根据我选择的颜色方案，修改 `RealWithdrawPanel` 提现界面的字体颜色、字体材质颜色、文字描边颜色、脚本富文本颜色字段，以及相关子 prefab 中的 TMP 文本颜色。

我选择的是：方案 X。

下面我会粘贴该方案的颜色表。请把颜色表作为唯一颜色来源，严格按照表里的 UI 元素名、字段 Key、材质 Key、富文本颜色 Key、FaceColor、OutlineColor 执行。

【在这里粘贴我选中的方案颜色表】

## 基本限制

不要重新设计颜色。
不要修改颜色表里的字段名。
不要自行调整颜色值。
不要修改 UI 布局、按钮位置、背景、卡片、图标、支付方式 logo、绿色勾选图标、进度条、金额数值、文案内容、字号、字体资源、动画、交互逻辑。
不要为了颜色任务重构业务代码。
出现UI和材质的对应关系问题，一切以Excel表为准。Excel表拥有最高优先级。
不要修改不在颜色表里的 UI 元素，除非它是颜色表元素的 prefab 实例覆盖。

## 已知工程结构

`RealWithdrawPanel.cs` 上有这些颜色字段：

- `withdrawValueKeyColor`
- `withdrawChannelKeyColor`
- `_colorReplaceList` 中的 `progressLevelColor`
- `_colorReplaceList` 中的 `progressClashColor`
- `_colorReplaceList` 中的 `progressRatioColor`

`withdrawValueKeyColor` 和 `withdrawChannelKeyColor` 是 `string` 颜色字段。
只填颜色值，例如 `#FF4A4AFF`。
不要生成完整 `<color>` 标签。
不要改文案。
不要改字符串拼接逻辑。

`_colorReplaceList` 里的 `progressLevelColor`、`progressClashColor`、`progressRatioColor` 只修改 `replaceValue`。
保持 `replaceKey` 不变。

`Item_Level`、`Item_BalanceText`、倍率文字、锁定态文字可能不在 `RealWithdrawPanel.cs` 里，而在 `WithdrawLevelItem.prefab` / `WithdrawLevelItem.cs` 中。必须通过 `RealWithdrawPanel.prefab` 中的 `withdrawLevelItem` 引用继续追踪源 prefab，并同时检查 `RealWithdrawPanel.prefab` 中已有的 prefab instance override。

`WithdrawWay.prefab` / `WithdrawWay.cs` 也要检查；如果支付方式 logo 是图片，不要改图片颜色。

## 查找顺序

优先查找并修改：

1. `RealWithdrawPanel.prefab`
2. `RealWithdrawPanel.cs` 中暴露在 Inspector 的 `public` / `[SerializeField]` 字段
3. `TextMeshProUGUI` / `TMP_Text` 组件
4. TMP 字体材质 / `sharedMaterial`
5. `WithdrawLevelItem.prefab` / `WithdrawLevelItem.cs`
6. `WithdrawWay.prefab` / `WithdrawWay.cs`
7. ScriptableObject / 配置表 / C# 常量
8. 本地化或文案配置里的颜色字段

重点搜索：

- `withdrawValueKeyColor`
- `withdrawChannelKeyColor`
- `progressLevelColor`
- `progressClashColor`
- `progressRatioColor`
- `ScreenTitle`
- `Title`
- `PassLevelHint`
- `MyBalance`
- `Balance_Text`
- `RateNum`
- `WithdrawWayTitle`
- `BalanceHint`
- `BlanaceHint`
- `MoreWithdraw_Hint`
- `progressValue`
- `progressHint`
- `CompleteHint`
- `ButtonText`
- `Item_Level`
- `Item_BalanceText`

注意拼写：工程里可能是 `BlanaceHint`，不要自行改成 `BalanceHint`。
如果颜色表里出现 `progressCashColor`，但工程中实际字段是 `progressClashColor`，优先按工程字段 `progressClashColor` 修改，并在结果中说明对应关系。

## TMP / prefab 修改规则

对颜色表中的 TMP 文本组件，按表写入：

- `m_sharedMaterial`
- `m_fontColor32`
- `m_fontColor`
- `m_faceColor`

如果当前 prefab 序列化中没有某个字段，不要硬塞 Unity / TMP 当前版本不序列化的字段。

TMP 3.x 中 `outlineColor` 可能不是可稳定序列化字段，很多 prefab 不会出现 `m_outlineColor`。这种情况下，不要手动往 YAML 里新增 `m_outlineColor`；描边色应通过 TMP 材质的 `_OutlineColor` 持久化。

`m_fontColor32.rgba` 使用 Unity YAML 的 32 位整数格式：

`rgba = r + g * 256 + b * 65536 + a * 16777216`

其中 `r/g/b/a` 来自颜色表的 RGBA255。

`m_fontColor` / 材质 YAML 中的颜色可以使用 `RGBA255 / 255` 后的浮点值。
如果 C# 代码里需要创建颜色对象，使用：

```csharp
new Color32(r, g, b, a)
```

不要在 C# 代码里写：

```csharp
new Color(1f, 1f, 1f, 1f)
```

## 材质处理规则

本任务明确要求：直接修改当前字体/TMP 引用的现有材质资产。

禁止创建新材质。
禁止复制材质。
禁止改名材质。
禁止新增 `.mat` / `.mat.meta`。
禁止创建当前界面专用 TMP 材质。
禁止因为材质被多个界面共享就自行改成“专用材质覆盖”方案。

执行时应：

1. 从目标 TMP 的 `m_sharedMaterial` 读取当前正在引用的材质 GUID / fileID。
2. 通过 `.meta` 或项目搜索定位这个 GUID 对应的现有 `.mat` 文件。
3. 在这个现有 `.mat` 文件中直接修改 `_FaceColor` 和 `_OutlineColor`。
4. 保持 prefab 中 `m_sharedMaterial` 引用不变。
5. 如果材质位于 Common / Framework / 全局字体目录，仍然直接修改它；这是本任务预期，不要以“共享材质可能影响其它界面”为理由新建材质。
6. 最终结果中说明被修改的材质是否为共享材质，以及可能影响其它引用位置。

如果同一个现有材质资产在颜色表中被要求写入多个不同的 FaceColor / OutlineColor，这是冲突：

- 不要通过新建材质解决。
- 不要擅自选择其中一个颜色覆盖全部。
- 先停止并向用户说明该材质资产只能有一组 `_FaceColor` / `_OutlineColor`，列出冲突的 UI 元素和颜色值，等待用户决定。

## prefab 实例覆盖规则

如果 `RealWithdrawPanel.prefab` 中引用了子 prefab，例如 `WithdrawLevelItem.prefab`：

1. 读取子 prefab 源文件中的目标 TMP，用于定位其当前 `m_sharedMaterial`。
2. 检查 `RealWithdrawPanel.prefab` 中该子 prefab 的 instance override。
3. 如果 override 中已有 `m_fontColor32.rgba`、`m_fontColor.r/g/b`、`m_faceColor`、`m_Color` 等组件颜色覆盖，只记录并在结果中报告可能存在额外颜色叠加风险；不要为了本任务同步修改这些字段。
4. 不要为了颜色替换修改 override 中的 `m_sharedMaterial` 指向，除非它本来就是丢失或错误引用。
5. 不要删除无关 override，不要改位置、尺寸、层级、激活状态等布局字段。

## 脚本与 prefab 字段同步

如果脚本里有默认颜色值，同时 prefab 上也有 Inspector 序列化值，必须两边都改：

- `RealWithdrawPanel.cs` 默认值
- `RealWithdrawPanel.prefab` 中对应序列化字段

否则 prefab 上的 Inspector 值可能覆盖脚本默认值。

## 不在颜色表里的内容

搜索到但不在颜色表里的 UI 元素不要修改，例如额外的 `Title`、`CompleteHint`、`RateText` 等。
最终结果中说明“找到但不在本次颜色表内，保持不动”。

支付方式 logo、银行卡 / Pix / PagBank 等图片资源不要改颜色。

## 完成后的核对

完成后必须检查：

1. 旧颜色值是否仍残留在目标脚本和目标 prefab 中，例如旧的 `#242eb8`、`#17003A`。
2. 颜色表中的每个 UI 元素是否都找到并处理。
3. 颜色表中的每个富文本字段 Key 是否都写入正确 hex。
4. 每个 TMP 是否仍绑定原有材质，没有被改到新材质。
5. 每个被目标 TMP 引用的现有材质资产 `_FaceColor` 和 `_OutlineColor` 是否等于表中的 RGBA255。
6. 子 prefab 源文件和父 prefab instance override 是否都只用于定位与检查；没有同步写入 TMP 组件自身颜色字段。
7. 是否新增了 `.mat` / `.mat.meta`；如果新增了，说明任务执行错误，需要撤回新增材质。
8. 是否误改了布局、图片、文案、字号、动画、交互逻辑。
9. 是否误改了 `m_fontColor32`、`m_fontColor`、`m_faceColor`、`m_Color`、`m_outlineColor` 等 TMP / Graphic 组件颜色字段；如果改了，说明任务执行错误，需要撤回这些组件字段修改。

## 完成后请输出

1. 修改过的文件列表。
2. 每个字段 Key 修改到的位置。
3. 每个 UI 元素 TMP 定位到的当前材质引用位置；不要输出为“TMP 颜色已修改”。
4. 每个现有材质资产修改到的位置。
5. 每个脚本颜色字段修改到的位置。
6. 未找到字段列表。
7. 找到但不在颜色表内、所以保持不动的字段列表。
8. 是否发现 `progressCashColor` / `progressClashColor` 命名不一致。
9. 确认没有创建新材质、没有复制材质、没有新增 `.mat` / `.mat.meta`。
10. 被修改材质是否为共享材质，以及可能影响的其它引用位置。
11. 是否发现 TMP / Graphic 组件自身颜色不是白色；如有，列出 UI 元素、组件颜色、引用材质，仅报告不修改。
12. 简短 diff 摘要。
