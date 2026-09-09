# APIReplace

用于批量接入发行 API、替换 Unity API 字段、更新发行配置、更新 Obfuz 并执行通信测试。

## 使用方式

1. 打开 `ParamTemp.md`，复制模板内容。
2. 在 Codex 消息中填写最新参数；不要让工具读取模板文件。
3. 多个项目用多个任务块一次提交。
4. 追加 `@APIReplacePrompt.md` 执行。

发行参数名称允许使用常见简写。工具会把 `应用识别码/adjust/adjust 识别码` 识别为 Adjust，把 `SDK Key/MAX SDK Key` 识别为 AppLovin SDK Key，把 `激励/激励视频` 和 `全屏/插屏` 分别识别为 MAX 激励与插屏广告 ID。`应用ID` 始终表示后台 AppId，不会与 Adjust 混用。

也可以直接粘贴下面这种发行短格式：

```text
示例游戏名称
com.example.game
应用识别码：<Adjust 应用识别码>
SDK Key：<AppLovin SDK Key>
激励：<MAX 激励广告 ID>
全屏：<MAX 插屏广告 ID>
```

工具会同时使用参数标签、上下文和内容格式进行校验。激励与插屏的值格式通常相同，因此两者必须保留可区分的标签；出现冲突或歧义时跳过对应项并报告。缺少或留空的参数保留项目原值。

示例：

```text
@APIReplacePrompt.md 执行功能

任务 1：
目标游戏路径：E:\\Puzzle_Project\\FakeProjects\\ArrowSnap
本次最新发行参数：<填写内容>
AccountModuleCfg.cs：已填写本次接入接口，空值不接入

任务 2：
目标游戏路径：<另一个项目路径>
本次最新发行参数：<填写内容>
AccountModuleCfg.cs：已填写本次接入接口，空值不接入
```

## 本地接口资料

- `LocalAPIData/swagger-doc.json`：完整 Swagger 文档。
- `LocalAPIData/API接口目录.md`：按 Tag 的接口目录。
- `LocalAPIData/swagger-doc.meta.json`：版本、来源、时间和 SHA-256。

执行时优先使用本地缓存。缓存不存在或 JSON 损坏时，才更新本地资料；正常情况下不会重复访问网页。

## 缓存检查/更新

```powershell
python "E:\\Puzzle_Project\\ARealTools\\APIReplace\\scripts\\sync_api_cache.py"
```

强制刷新：

```powershell
python "E:\\Puzzle_Project\\ARealTools\\APIReplace\\scripts\\sync_api_cache.py" --refresh
```

查询 Tag：

```powershell
python "E:\\Puzzle_Project\\ARealTools\\APIReplace\\scripts\\sync_api_cache.py" --tag PharaohMerge
```

本目录不读取或修改 `ParamTemp.md`。该文件只供用户复制到 Codex 消息中填写。
