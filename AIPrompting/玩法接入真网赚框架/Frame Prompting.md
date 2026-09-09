# BizzaWZ 白包玩法接入说明 / Codex 开发提示词

## 0. 使用场景

本文档用于让 Codex 在本地 Unity 工程中理解并接入 BizzaWZ 框架。

使用方式：

1. 将该工程作为白包基础工程。
2. 在该基础工程内开发白包玩法。
3. 不要破坏框架已有入口、API、UI、道具、SDK、存档、加载流程。

---

## 1. 关键词解释

### 1.1 补充功能代码

表示框架只提供入口方法，玩法方需要在这些方法里补充自己玩法的逻辑。

特点：

* 可以在指定方法内写玩法逻辑。
* 不应该改动框架通用流程。
* 不应该把框架 UI、统计、道具数量、SDK 逻辑混入玩法代码。

### 1.2 API 调用

表示玩法方只需要在指定时机调用框架 API。

要求：

* 不要修改 API 内部逻辑。
* 如果必须修改，需要先和框架负责人对齐。
* Codex 默认只能调用这些 API，不能擅自重写其内部实现。

---

## 2. 白包与正式包宏判断

使用以下宏判断当前是否为白包：

```csharp
#if !BIZZA_REAL_WITHDRAW
    // 白包逻辑
#else
    // 正式包逻辑
#endif
```

规则：

* `!BIZZA_REAL_WITHDRAW` 表示白包。
* `BIZZA_REAL_WITHDRAW` 表示正式包。
* 与广告、网赚、MAX、统计相关的正式包逻辑，不要写死到白包玩法里。
* 需要区分环境时，必须使用该宏判断。

---

## 3. Codex 开发总原则

### 3.1 不要破坏框架

Codex 修改工程时，优先遵守：

* 不要改框架核心 API 的内部逻辑。
* 不要重写框架加载流程。
* 不要删除框架已有场景、预制体、Addressables 引用。
* 不要破坏道具 UI、存档、SDK、加载页、新手引导流程。
* 不要把玩法逻辑写进框架通用模块里。

### 3.2 玩法开发边界

玩法代码应主要负责：

* 玩法初始化。
* 关卡加载。
* 游戏元素生成。
* 玩家操作。
* 胜负判断。
* 道具在玩法内的具体效果。
* 音效、动画、反馈。
* 调用框架提供的 API。

玩法代码不应负责：

* 框架道具数量扣除。
* 框架道具 UI 状态刷新。
* 框架埋点统计。
* 框架广告流程。
* 框架 SDK 初始化。
* 框架用户数据加载。
* 框架新手引导通用流程。

### 3.3 禁止用 UI 直接作为游戏元素

游戏开发禁止直接使用 UGUI UI 元素作为游戏内可交互玩法元素。

原因：

* 框架新手引导默认处理游戏场景元素。
* 如果玩法直接把 UI 当游戏元素，框架的新手引导需要特殊处理。
* 容易导致输入阻断、坐标转换、小手指引位置异常。

要求：

* 游戏场景默认在 'GamePlay' 下开发。
* 玩法核心元素优先使用 GameObject、SpriteRenderer、MeshRenderer、Collider 等。
* UI 只用于面板、按钮、文本、道具栏、弹窗等界面内容。
* 新手引导小手需要传 Unity World Space 坐标。

### 3.4 静态结构优先使用预制体和 Inspector

默认规则：

* 静态层级结构放到 Scene 或 Prefab。
* 图片、位置、尺寸、颜色、引用优先在 Inspector 中配置。
* 不要用代码大量动态创建静态 UI 或静态游戏结构。
* 代码只负责运行时逻辑、状态变化、事件流程、刷新、实例化已配置好的预制体。

### 3.5 其它一些要求，优先级均为高

* 不要用代码大量动态创建静态 UI 或静态游戏结构。
* 可以使用 Resources 或 Addressable 按路径加载资源
* 不建议使用 预制件拖拽的方式 将预制件绑定，导致运行时内存峰值偏高
* 由于代码会进行Obfuz混淆，所以代码不能 反射的方式 去调用方法或字段，如果必须使用，请对被调用的地方添加防混淆特性 [Obfuz.ObfuzIgnore] 同样如果有需要，请对调用的地方添加 [Obfuz.ObfuzIgnore] 特性

---

## 4. AI 框架接入

## 4.1 预加载资源

类型：补充功能代码

如果玩法中有资源需要预加载，在以下方法中补充：

```csharp
BridgingUtil.LoadGamePlayAsync()
```

使用场景：

* 玩法场景资源预加载。
* 关卡配置预加载。
* 玩法用预制体预加载。
* 玩法音效或必要资源预加载。

要求：

* 只补充玩法需要的预加载逻辑。
* 不要破坏框架已有加载流程。
* 不要阻塞主线程。
* 如果使用异步加载，需要确保加载完成后再进入玩法初始化。

---

## 4.2 新手引导开始时机

类型：API 调用

玩法场景和玩法 UI 初始化完成后，必须由玩法方调用：

```csharp
BridgingUtil.CanShowGuide();
```

一般调用时机：

```csharp
// 一般在关卡加载完成、玩法元素生成完毕后调用
BridgingUtil.CanShowGuide();
```

原因：

* 避免游戏还没进入，教程提前触发导致报错。
* 避免新手引导找不到目标。
* 避免 UI 或玩法元素尚未初始化完成。

要求：

* 关卡加载完成后调用。
* 玩法元素生成完成后调用。
* 玩法 UI 初始化完成后调用。
* 不要提前调用。

---

## 4.3 新手引导小手显示

类型：API 调用

当玩法需要引导玩家点击时调用：

```csharp
UIModule.Instance.OpenPage(UIPageIds.UI_TeachFinterMove, new UITeachFingerMovePage.MoveArgs()
{
    worldStartPos = startWorldPos,
    worldEndPos = endWorldPos,
    duration = duration,
});
```

说明：

* 无论 3D 还是 2D，都传 Unity World Space 中的位置。
* 框架内部会把 World Space 转换成手指 UI 的 anchoredPosition。
* 如果需要小手从 A 点移动到 B 点，传入起点、终点、移动时间。
* 如果不需要移动，起点和终点传相同位置，时间传 `999`。

不移动示例：

```csharp
UIModule.Instance.OpenPage(UIPageIds.UI_TeachFinterMove, new UITeachFingerMovePage.MoveArgs()
{
    worldStartPos = targetWorldPos,
    worldEndPos = targetWorldPos,
    duration = 999f,
});
```

移动示例：

```csharp
UIModule.Instance.OpenPage(UIPageIds.UI_TeachFinterMove, new UITeachFingerMovePage.MoveArgs()
{
    worldStartPos = startWorldPos,
    worldEndPos = endWorldPos,
    duration = 1.2f,
});
```

注意：

* `UI_TeachFinterMove` 是框架现有 ID，不要擅自改名。
* 小手位置必须来自玩法世界坐标。
* 不要直接传屏幕坐标或 UI 坐标，除非框架明确支持。

---

## 4.4 新手引导小手关闭

类型：API 调用

当玩家点击了当前步骤的玩法引导目标后调用：

```csharp
UIModule.Instance.ClosePage(UIPageIds.UI_TeachFinterMove);
```

使用时机：

* 玩家完成当前引导点击。
* 当前引导步骤结束。
* 引导目标失效。
* 关卡重开或切换关卡时，需要清理当前小手。

---

## 4.5 玩法新手引导结束

类型：API 调用

玩法相关新手引导结束后，调用：

```csharp
BridgingUtil.NewPlayerGuideEnd();
```

说明：

* 这里只包括玩法教程结束。
* 不包括道具提示、机制提示、普通弹窗提示。
* 当白包玩法教程完整结束时调用。

要求：

* 不要提前调用。
* 不要漏调用。
* 教程最后一步完成后调用。
* 调用后框架会继续后续新手流程。

---

# 5. 游戏中常见逻辑

## 5.1 游戏配置

### 道具配置 SO

道具配置使用：

```text
PropConfig
```

包含内容：

* 道具 ICON。
* 道具类型。
* 默认支持五种类型。
* 解锁条件。
* 是否支持取消。
* 每局使用次数限制。
* 新手赠送道具数量。

要求：

* 道具显示和基础行为优先通过 `PropConfig` 配置。
* 不要在玩法代码里写死道具 UI。
* 不要绕过框架道具配置系统。

---

## 5.2 关卡值

类型：API 调用

关卡值可以直接获取和设置：

```csharp
BridgingUtil.GameLevel
```

示例：

```csharp
int level = BridgingUtil.GameLevel;
BridgingUtil.GameLevel = level + 1;
```

注意：

* 框架 `GameWin()` 中已经补充了关卡数值递增逻辑。
* 一般情况下，玩法胜利时不要重复手动增加关卡。
* 只有特殊调试或特殊流程才直接设置关卡。

---

## 5.3 每关进入时初始化道具使用限制

类型：API 调用

每关进入时需要调用：

```csharp
BridgingUtil.PropUseLimitRecover();
```

使用时机：

* 当前关卡开始。
* 关卡重开。
* 下一关加载完成。
* 进入玩法前需要重置每局道具次数限制。

要求：

* 每关开始必须调用。
* 不要在每次使用道具时调用。
* 不要漏掉重开关卡场景。

---

## 5.4 每次合成 / 消除 / 收集完成时调用

类型：API 调用

每次合成、消除、收集满一个盒子时触发：

```csharp
BridgingUtil.SynthesisLogic(int num_Remaining);
```

参数：

```text
num_Remaining：当前关卡中剩余合成/消除次数
```

示例：

```csharp
BridgingUtil.SynthesisLogic(remainingCount);
```

说明：

* 该逻辑只在 `BIZZA_ENABLE_MAX` 环境下生效。
* 非 MAX 环境会直接跳过。
* 白包环境调用也可以，但内部不会执行正式逻辑。

要求：

* 在合成、消除、收集真正完成后调用。
* `num_Remaining` 必须传当前关卡剩余次数。
* 不要在动画开始前提前调用，除非玩法逻辑明确认为已经完成。

---

## 5.5 音频播放

音频资源需要预先放入：

```text
Resources/Audios
```

播放背景音乐：

```csharp
SoundManager.Instance.PlayBGM("fileName");
```

停止背景音乐：

```csharp
SoundManager.Instance.StopBGM();
```

播放音效：

```csharp
SoundManager.Instance.PlaySFX("fileName");
```

播放音效完整签名：

```csharp
SoundManager.Instance.PlaySFX(string name, float minInterval = 0.12f);
```

设置背景音量：

```csharp
SoundManager.Instance.SetBGMVolume(floatValue);
```

设置音效音量：

```csharp
SoundManager.Instance.SetSFXVolume(floatValue);
```

要求：

* 文件名不需要带扩展名。
* 音频文件放入 `Resources/Audios`。
* 高频音效需要注意 `minInterval`，避免短时间疯狂播放。

---

# 6. 道具接入

## 6.1 道具使用入口

类型：补充功能代码

玩法方只需要在对应方法中编写道具的玩法启动逻辑：

```csharp
BridgingUtil.PropUse_1();
BridgingUtil.PropUse_2();
BridgingUtil.PropUse_3();
BridgingUtil.PropUse_4();
BridgingUtil.PropUse_5();
```

方法职责：

* 只写玩法效果。
* 或只写玩法准备逻辑。
* 不处理道具数量扣除。
* 不处理 UI 显示。
* 不处理埋点统计。
* 不处理每局次数统计。

这些内容由框架统一处理：

* 道具数量扣除。
* 道具 UI 状态。
* 使用次数统计。
* 埋点统计。
* 道具栏恢复。

---

## 6.2 PropUse_X 返回值规则

`PropUse_1~5()` 返回 `bool`。

返回 `true`：

```text
表示成功进入道具使用流程。
```

返回 `false`：

```text
表示当前玩法条件下不能使用该道具。
```

返回 `false` 的常见情况：

* 没有可作用目标。
* 棋盘状态不允许。
* 当前正在播放动画。
* 当前处于不可操作状态。
* 玩家未满足使用条件。
* 道具目标不存在。

如果玩法上没有特殊限制，默认返回：

```csharp
return true;
```

如果玩法存在不能使用的情况，需要玩法方在 `PropUse_1~5()` 中自行判断并返回 `false`。

---

## 6.3 点击后立即生效的道具

流程：

1. 在 `PropUse_X()` 中执行玩法效果。
2. 成功返回 `true`。
3. 失败返回 `false`。
4. 不需要主动调用 `PropUse_X_Over()`。

示例结构：

```csharp
public static bool PropUse_1()
{
    if (!CanUseProp1())
    {
        return false;
    }

    ApplyProp1Effect();
    return true;
}
```

说明：

* 框架会在 `PropUse_X()` 返回 `true` 后立即完成扣除和统计。
* 不需要额外调用 `_Over()`。

---

## 6.4 需要选择目标 / 支持取消的道具

支持取消或需要等待玩家二次操作的道具，需要在对应的 `_Over()` 中完成结算通知：

```csharp
BridgingUtil.PropUse_1_Over(bool breakFlow = false);
```

规则：

* 如果道具需要支持取消，需要在道具配置 SO `PropConfig` 中勾选 `支持取消`。
* 如果道具不支持取消，且点击后立即生效，一般不需要额外实现或主动调用 `_Over()`。
* `breakFlow` 表示本次道具流程是否被中断。

`breakFlow == false`：

```text
道具最终使用成功。
会进行数量扣除、使用次数统计、埋点和 UI 恢复。
```

`breakFlow == true`：

```text
道具流程取消或失败。
不扣数量，不统计，仅恢复 UI 状态。
```

需要传 `true` 的情况：

* 玩家取消选择。
* 玩家点击了非预期目标。
* 玩家选择失败。
* 重开关卡。
* 切换关卡。
* 胜利或失败导致流程中断。
* 当前目标失效。

需要传 `false` 的情况：

* 玩家完成正确目标选择。
* 玩法效果真正生效。
* 道具流程成功结束。

---

## 6.5 需要选择目标的道具流程

流程：

1. 在 `PropUse_X()` 中进入选择状态，并返回 `true`。
2. 在 `PropConfig` 中勾选 `支持取消`。
3. 玩家选择合法目标并成功生效时，调用：

```csharp
BridgingUtil.PropUse_X_Over(false);
```

4. 玩家取消、点错目标、流程被打断时，调用：

```csharp
BridgingUtil.PropUse_X_Over(true);
```

示例结构：

```csharp
public static bool PropUse_1()
{
    if (!HasValidTarget())
    {
        return false;
    }

    EnterPropSelectMode();
    return true;
}

private void OnSelectValidTarget()
{
    ApplyPropEffect();
    BridgingUtil.PropUse_1_Over(false);
}

private void OnCancelProp()
{
    BridgingUtil.PropUse_1_Over(true);
}
```

如果 `_Over` 中需要补充玩法侧成功收尾逻辑，应只在：

```csharp
breakFlow == false
```

时执行。

---

# 7. 游戏重开、下一关、胜利、失败

## 7.1 游戏重开

类型：API 调用

```csharp
BridgingUtil.ReloadCurrentLevel();
```

使用时机：

* 玩家点击重开。
* 失败面板选择重试。
* 当前关卡需要重新加载。

注意：

* 重开时需要清理当前玩法状态。
* 如果有正在进行的道具选择流程，应调用对应 `_Over(true)` 或清理道具状态。
* 需要重新调用 `PropUseLimitRecover()`。

---

## 7.2 下一关

类型：API 调用

```csharp
BridgingUtil.LoadNextLevel();
```

说明：

* 胜利面板关闭后会调用。
* 玩法方一般不需要在胜利瞬间直接加载下一关，除非流程明确要求。

---

## 7.3 游戏胜利

类型：API 调用 / 补充功能代码

```csharp
BridgingUtil.GameWin();
```

说明：

* 框架中已经补充了关卡数值递增。
* 玩法方只需要补充自定义游戏的后续逻辑。
* 不要重复增加 `GameLevel`，避免关卡跳级。

胜利前建议处理：

* 停止玩家输入。
* 停止当前道具选择流程。
* 关闭新手引导小手。
* 播放胜利动画或音效。
* 调用 `BridgingUtil.GameWin()`。

---

## 7.4 游戏失败

类型：API 调用

```csharp
BridgingUtil.GameOver();
```

失败前建议处理：

* 停止玩家输入。
* 停止当前道具选择流程。
* 关闭新手引导小手。
* 播放失败动画或音效。
* 调用 `BridgingUtil.GameOver()`。

---

# 8. Obfuz 混淆配置

## 8.1 配置步骤

1. 确认 Obfuz 配置中 `Assemblies To Obfuscate` 包含：

```text
Assembly-CSharp
```

2. `secretSettings.defaultStaticSecretKey` 填入当前包名。

示例：

```text
com.bangsawan.mergesoccer
```

3. `secretSettings.defaultDynamicSecretKey` 填入当前包名。

示例：

```text
com.bangsawan.mergesoccer
```

4. `encryptionVMSettings.codeGenerationSecretKey` 填入包名最后一段内容。

示例：

```text
包名：com.bangsawan.mergesoccer
填写：mergesoccer
```

5. `symbolObfusSettings.obfuscatedNamePrefix` 填入固定的 3 个英文小写字母。

示例：

```text
mgc
```

建议：

* 从包名最后一段中取。
* 不要每次打包随机变化。

6. Unity 菜单执行：

```text
Obfuz/GenerateEncryptionVM
```

7. Unity 菜单执行：

```text
Obfuz/GenerateSecretKeyFile
```

8. 打包前确认生成文件存在：

```text
Assets/Obfuz/GeneratedEncryptionVirtualMachine.cs
Assets/Resources/Obfuz/defaultStaticSecretKey.bytes
Assets/Resources/Obfuz/defaultDynamicSecretKey.bytes
```

## 8.2 重新生成规则

如果修改了以下配置中的密钥相关内容：

* `secretSettings`
* `encryptionVMSettings`

需要重新执行：

```text
Obfuz/GenerateEncryptionVM
Obfuz/GenerateSecretKeyFile
```

---

# 9. AI 框架检查清单

Codex 修改或接入工程时，需要检查以下内容。

## 9.1 初始场景检查

初始场景：

```text
InitWZ
```

必要元素：

```text
InitWzCamera
```

要求：

* `InitWzCamera` 必须存在。
* `InitWzCamera` 必须挂载摄像机组件。
* 缺失可能导致游戏开始蓝屏。

---

## 9.2 LoadingPanel 检查

框架会加载：

```text
LoadingPanel
```

该预制件中可以配置加载界面背景为视频或图片。

如果开启视频作为背景，需要打开：

```text
Obj：LoadingAnim_Raw
Obj：LoadingAnim_Player
脚本：VideoPlayerFallback
```

要求：

* 不要随意删除 LoadingPanel。
* 不要破坏加载页资源引用。
* 不要删除视频兜底脚本。

---

# 10. 框架新手引导流程说明

框架中已经写好新手引导流程，玩法方只需要在正确时机调用 API。

流程：

```text
节点：判断是不是新人
    |
    |-- 如果不是新人：
    |       正常进入游戏
    |
    |-- 如果是新人：
            进入新手引导
            |
            节点：教学阻断输入
            显示透明 TransparentBlock
            此时游戏中所有物体都不能点击
            |
            节点：可以显示教程
            等待游戏场景加载完成
            等待游戏元素生成完毕
            等待玩法方调用 BridgingUtil.CanShowGuide()
            |
            检查白包教程是否完毕
                |
                |-- 如果白包教程完成：
                |       设置 UI 可以点击
                |       关闭教学阻断
                |
                |-- 如果白包教程未完成：
                        关闭教学阻断
                        UI 不可以点击
                        进入自定义引导
                        等待白包新手引导结束
                        等待玩法方调用 BridgingUtil.NewPlayerGuideEnd()
                        |
                        新手教程结束
```

玩法方必须做的事：

1. 玩法初始化完成后调用：

```csharp
BridgingUtil.CanShowGuide();
```

2. 需要显示小手时调用：

```csharp
UIModule.Instance.OpenPage(UIPageIds.UI_TeachFinterMove, args);
```

3. 当前小手步骤结束时调用：

```csharp
UIModule.Instance.ClosePage(UIPageIds.UI_TeachFinterMove);
```

4. 白包玩法教程全部结束时调用：

```csharp
BridgingUtil.NewPlayerGuideEnd();
```

---

# 12. 真网赚框架流程运行说明

框架加载任务流程：

```text
检测网络 CheckNetworkTask
    -->
预加载资源 PreLoadAssetsTask
    -->
加载用户数据 LoadUserDataTask
    -->
初始化 SDK InitPlatformTask
```

## 12.1 检测网络

```text
CheckNetworkTask
```

说明：

* 会进行三次检测。
* 如果三次都失败，则关闭游戏。

玩法方要求：

* 不要绕过网络检测流程。
* 不要在玩法代码中重写网络检测。
* 白包玩法一般不需要处理该流程。

## 12.2 加载用户数据

```text
LoadUserDataTask
```

说明：

* 使用 API 接口请求用户数据。
* 玩法方不要修改该请求流程。
* 玩法只读取框架提供的数据入口。

## 12.3 初始化 SDK

```text
InitPlatformTask
```

说明：

* 正式包 SDK 初始化在此流程中处理。
* 白包玩法不要自己初始化 MAX、统计、网赚 SDK。
* 与 SDK 相关逻辑需要走框架统一入口。

---

# 13. 多语言规则

所有出现在游戏中的文本，都需要使用多语言。

要求：

* 白包玩法相关的多语言 key 统一写到一个静态类脚本中。
* 不要在多个脚本里散落字符串 key。
* 不要在玩法代码中硬编码显示文本。
* UI 文本必须通过多语言 key 获取。

建议结构：

```csharp
public static class GameI18nKeys
{
    public const string Start = "game_start";
    public const string Win = "game_win";
    public const string Fail = "game_fail";
    public const string Retry = "game_retry";
    public const string Next = "game_next";
}
```

使用规则：

* key 命名要稳定。
* key 不要随意改名。
* 新增文本时同步补充多语言表。
* Codex 新增文本时，必须同步新增 key，不能直接写死中文或英文。

---

# 14. 手动接入流程

## 14.1 直接在框架上开发白包游戏

步骤：

1. 复制或拉取 BizzaWZ 框架工程。
2. 将该工程作为基础工程。
3. 在工程内开发白包玩法。
4. 保留框架原有目录和流程。
5. 玩法代码与框架代码边界清晰。

## 14.2 替换 GamePlay 场景

步骤：

1. 替换或配置 `GamePlay` 场景。
2. 直接运行游戏。
3. 确认可以进入玩法。
4. 确认加载流程正常。
5. 确认 `InitWZ` 到 `GamePlay` 的流程正常。

## 14.3 替换 UI / 道具 / 存档

需要替换或接入：

* UI。
* 道具。
* 存档。
* 玩法关卡数据。
* 多语言文本。
* 音频资源。
* 新手引导。

要求：

* 替换 UI 时不要破坏框架引用。
* 替换道具图标时不要丢失 Addressable 引用。
* 存档接入应走框架已有存档入口。
* 不要新增一套独立存档系统，除非明确要求。

---

# 15. Codex 修改前检查

Codex 在开始改代码前，必须先检查：

## 15.1 工程结构

* 是否存在 `InitWZ` 场景。
* 是否存在 `GamePlay` 场景。
* 是否存在 `InitWzCamera`。
* 是否存在 `LoadingPanel`。
* 是否存在 `BridgingUtil`。
* 是否存在 `PropConfig`。
* 是否存在 `SoundManager`。
* 是否存在 `UIModule`。
* 是否存在 `UIPageIds.UI_TeachFinterMove`。
* 是否存在 `UITeachFingerMovePage.MoveArgs`。

## 15.2 框架 API

检查以下 API 是否存在，调用方式是否与本文档一致：

```csharp
BridgingUtil.LoadGamePlayAsync();
BridgingUtil.CanShowGuide();
BridgingUtil.NewPlayerGuideEnd();
BridgingUtil.PropUseLimitRecover();
BridgingUtil.SynthesisLogic(int num_Remaining);
BridgingUtil.ReloadCurrentLevel();
BridgingUtil.LoadNextLevel();
BridgingUtil.GameWin();
BridgingUtil.GameOver();
BridgingUtil.PropUse_1();
BridgingUtil.PropUse_2();
BridgingUtil.PropUse_3();
BridgingUtil.PropUse_4();
BridgingUtil.PropUse_5();
BridgingUtil.PropUse_1_Over(bool breakFlow = false);
```

如果实际工程 API 名称或签名不同：

* 不要猜。
* 先搜索工程现有实现。
* 以工程真实代码为准。
* 保持调用边界不变。

---

# 16. Codex 修改后检查

修改完成后，需要检查：

## 16.1 编译检查

* Unity 是否能编译通过。
* 是否有缺失脚本。
* 是否有命名空间错误。
* 是否有 API 签名错误。
* 是否有 Addressables 引用丢失。
* 是否有场景引用丢失。

## 16.2 运行流程检查

至少验证：

1. 从 `InitWZ` 启动不会蓝屏。
2. LoadingPanel 正常显示。
3. 可以进入 `GamePlay`。
4. 玩法元素正常生成。
5. 玩法 UI 正常显示。
6. 关卡加载完成后调用 `BridgingUtil.CanShowGuide()`。
7. 新手引导小手可以显示和关闭。
8. 教程结束后调用 `BridgingUtil.NewPlayerGuideEnd()`。
9. 每关开始调用 `BridgingUtil.PropUseLimitRecover()`。
10. 胜利调用 `BridgingUtil.GameWin()`。
11. 失败调用 `BridgingUtil.GameOver()`。
12. 重开调用 `BridgingUtil.ReloadCurrentLevel()`。
13. 下一关调用 `BridgingUtil.LoadNextLevel()`。
14. 道具使用逻辑符合 `PropUse_X()` 和 `PropUse_X_Over()` 流程。

## 16.3 道具检查

* 点击后立即生效的道具，不主动调用 `_Over()`。
* 需要选择目标的道具，成功时调用 `_Over(false)`。
* 取消或失败时调用 `_Over(true)`。
* 不在玩法中处理道具数量扣除。
* 不在玩法中处理道具 UI 恢复。
* 不在玩法中处理道具埋点统计。
* `PropConfig` 中正确配置是否支持取消。

## 16.4 多语言检查

* 玩法新增文本没有硬编码。
* 所有文本都有多语言 key。
* 白包玩法 key 统一写在静态类脚本中。
* 多语言表同步补充。

---

# 17. 禁止行为

Codex 不允许执行以下操作：

* 不允许重写框架启动流程。
* 不允许删除 `InitWZ` 必要元素。
* 不允许删除 `InitWzCamera`。
* 不允许破坏 `LoadingPanel`。
* 不允许修改框架 API 内部逻辑，除非任务明确要求。
* 不允许把玩法元素直接做成 UI 元素。
* 不允许硬编码多语言文本。
* 不允许绕过 `PropConfig` 自己写一套道具系统。
* 不允许在玩法代码里处理道具数量扣除、UI 状态、埋点统计。
* 不允许随意丢失 Addressables 引用。
* 不允许每次打包随机修改 Obfuz 混淆前缀。
* 不允许重复增加关卡值。
* 不允许遗漏 `BridgingUtil.CanShowGuide()`。
* 不允许遗漏 `BridgingUtil.NewPlayerGuideEnd()`。

---

# 18. 推荐 Codex 执行方式

当用户要求接入白包玩法时，Codex 应按以下顺序执行：

1. 先搜索框架已有入口和 API。
2. 确认 `BridgingUtil` 真实实现。
3. 确认 `GamePlay` 场景和玩法入口。
4. 确认道具配置 `PropConfig`。
5. 确认新手引导相关 UI ID 和参数结构。
6. 再设计玩法代码。
7. 玩法静态结构优先使用 Prefab / Scene / Inspector。
8. 代码只负责运行时逻辑。
9. 接入框架 API。
10. 最后做编译和流程检查。

输出修改说明时，需要明确：

* 修改了哪些脚本。
* 新增了哪些脚本。
* 哪些是玩法逻辑。
* 哪些是框架 API 调用。
* 是否修改了框架内部逻辑。
* 是否需要手动在 Unity Inspector 中挂引用。
* 是否需要手动配置 Addressables。
* 是否需要手动配置 `PropConfig`。
* 是否需要执行 Obfuz 生成步骤。

---

# 19. 常用 API 汇总

```csharp
// 是否白包
#if !BIZZA_ENABLE_MAX
    // 白包
#else
    // 正式包
#endif

// 预加载玩法资源
BridgingUtil.LoadGamePlayAsync();

// 玩法加载完成，可以显示教程
BridgingUtil.CanShowGuide();

// 新手引导结束
BridgingUtil.NewPlayerGuideEnd();

// 关卡值
BridgingUtil.GameLevel;

// 每关开始恢复道具使用限制
BridgingUtil.PropUseLimitRecover();

// 合成 / 消除 / 收集完成逻辑
BridgingUtil.SynthesisLogic(int num_Remaining);

// 重开当前关
BridgingUtil.ReloadCurrentLevel();

// 下一关
BridgingUtil.LoadNextLevel();

// 胜利
BridgingUtil.GameWin();

// 失败
BridgingUtil.GameOver();

// 道具使用
BridgingUtil.PropUse_1();
BridgingUtil.PropUse_2();
BridgingUtil.PropUse_3();
BridgingUtil.PropUse_4();
BridgingUtil.PropUse_5();

// 道具流程结束
BridgingUtil.PropUse_1_Over(bool breakFlow = false);

// 新手引导小手显示
UIModule.Instance.OpenPage(UIPageIds.UI_TeachFinterMove, new UITeachFingerMovePage.MoveArgs()
{
    worldStartPos = startWorldPos,
    worldEndPos = endWorldPos,
    duration = duration,
});

// 新手引导小手关闭
UIModule.Instance.ClosePage(UIPageIds.UI_TeachFinterMove);

// 音频
SoundManager.Instance.PlayBGM("fileName");
SoundManager.Instance.StopBGM();
SoundManager.Instance.PlaySFX("fileName");
SoundManager.Instance.SetBGMVolume(floatValue);
SoundManager.Instance.SetSFXVolume(floatValue);
```
