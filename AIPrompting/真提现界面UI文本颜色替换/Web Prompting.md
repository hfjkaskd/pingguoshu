我会上传：
1. 当前游戏界面实际截图
2. 字段注释图
3. RealWithdrawPanel.cs
4. RealWithdrawPanel.prefab
5. Unity 字体颜色配置表

配置表只作为字段说明和颜色落点参考，不需要你重新生成表格，也不需要输出 Excel。

请你只做两件事：

第一，基于当前 UI 截图生成 3 套字体颜色方案横向对比预览图。
要求：
- 三套完整界面横向排列
- 只修改文字主体色、文字描边色、富文本局部颜色
- 不修改 UI 布局、按钮位置、背景、卡片、边框、图标、支付 logo、绿色勾选图标、进度条、金额数值、文案内容、字号、字体资源
- 注释图里的红色箭头、红色文字、字段名不要出现在最终预览图中

第二，直接输出 3 套颜色数据块，不要生成表格。

每套颜色数据块格式如下：

方案 1：

【TMP / 字体材质颜色，RGBA255】
ScreenTitle / Title:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

PassLevelHint:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

MyBalance:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

Balance_Text:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

RateNum:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

WithdrawWayTitle:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

BlanaceHint / BalanceHint:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

MoreWithdraw_Hint:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

progressValue:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

progressHint:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

CompleteHint:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

ButtonText:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

Item_Level:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

Item_BalanceText:
FaceColor = r,g,b,a
OutlineColor = r,g,b,a

【脚本富文本颜色字段，Hex】
withdrawValueKeyColor = #XXXXXXXX
withdrawChannelKeyColor = #XXXXXXXX
progressLevelColor = #XXXXXXXX
progressClashColor = #XXXXXXXX
progressRatioColor = #XXXXXXXX

方案 2：
按同样格式输出。

方案 3：
按同样格式输出。

注意：
1. 材质 / TMP / 字体颜色必须输出 RGBA255，例如 255,255,255,255。
2. 脚本 string 类型富文本颜色字段必须输出 Hex，例如 #FF4A4AFF。
3. 不要输出完整 <color> 标签。
4. 不要讲富文本闭合。
5. 不要重新生成表格。
6. 不要新增字段。
7. progressClashColor 以脚本实际字段名为准，不要改成 progressCashColor。