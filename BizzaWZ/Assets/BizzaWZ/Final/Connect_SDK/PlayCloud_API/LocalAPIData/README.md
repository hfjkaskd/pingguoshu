# API 替换本地资料库

这里保存 Swagger 完整接口文档和快速查阅目录，后续 API 替换优先使用本地资料，不重复访问网页。

## 文件说明

- `swagger-doc.json`：完整 Swagger/OpenAPI 接口文档，包含全部路径、Tag、请求/响应模型和字段定义。
- `swagger-doc.meta.json`：来源地址、拉取时间、接口数量、版本和 SHA-256 校验值。
- `API接口目录.md`：按 Tag 分类的接口快速目录，包含方法、路径、OperationId、请求模型、响应模型和字段数量。

## 当前缓存

- 来源：`http://test.api.pandamerge.top/swagger/doc.json`
- Swagger 版本：`2.0`
- 接口路径：`1658`
- 接口操作：`1658`
- Tag：`89`
- 模型定义：`3001`
- 拉取时间：`2026-08-19T10:44:39+08:00`
- SHA-256：`31c35db56a6606f0868ec2aafe6e3f993458e834fc7e7205129a6826b7634ccc`

## 使用规则

1. `swagger-doc.json` 存在且能正常解析时，直接使用本地缓存。
2. 本地文件缺失或损坏时，才从元数据中的来源地址重新拉取。
3. 发行方确认接口文档更新时，重新下载并更新本目录和 `swagger-doc.meta.json`。
4. 不要直接编辑 `swagger-doc.json`；如需更新，应重新获取完整文档。

## 静态检查示例

在 API 检查命令中指定本地 Swagger 文件：

```powershell
python "E:\Puzzle_Project\ARealTools\replace_YunAPISkill\scripts\InvokeApiCommunicationTest.py" `
  --project-root "E:\WKY" `
  --swagger-source "E:\Puzzle_Project\ARealTools\API替换\swagger-doc.json" `
  --tag "PharaohMerge"
```

其中 `--tag` 只负责从本地完整文档中筛选目标接口，不会再次访问网页。
