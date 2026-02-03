# Unity 土壤含水率实验 - 配置教程

## 第一步：创建Layer

1. 打开 `Edit > Project Settings > Tags and Layers`
2. 在 `Layers` 中找到一个空位（如 Layer 6）
3. 命名为 `Interactable`

---

## 第二步：创建管理器物体

### 2.1 创建 GameManagers 空物体
1. 在 Hierarchy 右键 > `Create Empty`
2. 命名为 `GameManagers`
3. 添加以下脚本组件（Add Component）：
   - `GameManager` (Scripts/Core)
   - `StepManager` (Scripts/Core)
   - `DataManager` (Scripts/Core)

4. 在 Inspector 中设置 GameManager：
   - `Step Manager` → 拖入自身的 StepManager 组件
   - `Data Manager` → 拖入自身的 DataManager 组件

### 2.2 创建 InteractionSystem 空物体
1. 创建空物体，命名为 `InteractionSystem`
2. 添加脚本：
   - `InteractionController` (Scripts/Interaction)
   - `HighlightSystem` (Scripts/Interaction)

3. 设置 InteractionController：
   - `Interactable Layer` → 选择刚创建的 `Interactable` 层
   - `Main Camera` → 拖入场景中的 Main Camera
   - `Drag Height` → 1（拖拽时物体离地高度）
   - `Max Ray Distance` → 100

---

## 第三步：配置场景物体

### 3.1 烘箱 (Oven)
1. 选中烘箱父物体
2. 添加脚本 `Oven` (Scripts/Objects)
3. 设置：
   - `Door` → 拖入烘箱的门子物体（名称应包含"door"）
   - `Inside Position` → 创建空子物体放在烘箱内部，拖入此处
   - `Open Angle` → 90（门打开角度）
4. 设置 Layer 为 `Interactable`（包括子物体）

### 3.2 干燥器 (Desiccator)
1. 选中干燥器父物体
2. 添加脚本 `Desiccator` (Scripts/Objects)
3. 设置：
   - `Cap` → 拖入干燥器的盖子子物体（名称应包含"cap"）
   - `Inside Position` → 创建空子物体放在干燥器内部，拖入此处
   - `Open Height` → 0.5（盖子打开高度）
4. 设置 Layer 为 `Interactable`

### 3.3 铝盒A
1. 选中铝盒A父物体
2. 添加脚本 `AluminumBox` (Scripts/Objects)
3. 设置：
   - `Cap` → 拖入铝盒的盖子子物体
   - `Body` → 拖入铝盒的主体子物体
   - `Box Id` → 输入 `A`
   - `Open Angle` → 90
4. 设置 Layer 为 `Interactable`
5. 添加 `Rigidbody` 组件（可选，用于物理）：
   - `Is Kinematic` → 勾选

### 3.4 铝盒B
1. 同铝盒A的步骤
2. `Box Id` → 输入 `B`

### 3.5 天平 (Balance)
1. 选中天平物体
2. 添加脚本 `Balance` (Scripts/Objects)
3. 设置：
   - `Weighing Position` → 创建空子物体放在天平秤盘上，拖入此处
4. 设置 Layer 为 `Interactable`

### 3.6 勺子 (Spoon)
1. 选中勺子物体
2. 添加脚本 `Spoon` (Scripts/Objects)
3. 设置 Layer 为 `Interactable`

### 3.7 垃圾桶 (TrashBin)
1. 选中垃圾桶物体
2. 添加脚本 `TrashBin` (Scripts/Objects)
3. 设置 Layer 为 `Interactable`

### 3.8 干土 (Dry Soil)
1. 选中干土物体
2. 添加脚本 `Soil` (Scripts/Objects)
3. 设置：
   - `Type` → 选择 `Dry`
4. 设置 Layer 为 `Interactable`

### 3.9 湿土 (Wet Soil)
1. 选中湿土物体
2. 添加脚本 `Soil` (Scripts/Objects)
3. 设置：
   - `Type` → 选择 `Wet`
4. 设置 Layer 为 `Interactable`

---

## 第四步：配置高亮系统

选中 `InteractionSystem` 物体，在 HighlightSystem 组件中设置：
- `Highlight Color` → 黄色 (255, 255, 0, 128)
- `Pulse Speed` → 2
- `Pulse Intensity` → 0.3
- `Aluminum Box A` → 拖入铝盒A
- `Aluminum Box B` → 拖入铝盒B
- `Oven` → 拖入烘箱
- `Desiccator` → 拖入干燥器
- `Spoon` → 拖入勺子

---

## 第五步：配置摄像机

1. 选中 Main Camera
2. 添加脚本 `CameraController` (Scripts/Camera)
3. 设置：
   - `Target` → 创建空物体放在实验台中心，拖入此处
   - `Rotation Speed` → 5
   - `Min Vertical Angle` → 10
   - `Max Vertical Angle` → 80
   - `Zoom Speed` → 2
   - `Min Distance` → 3
   - `Max Distance` → 15
   - `Initial Distance` → 8
   - `Initial Pitch` → 45

---

## 第六步：创建UI界面

### 6.1 创建Canvas
1. Hierarchy 右键 > `UI > Canvas`
2. Canvas Scaler 设置：
   - `UI Scale Mode` → Scale With Screen Size
   - `Reference Resolution` → 1920 x 1080

### 6.2 添加UIManager
1. 选中 Canvas
2. 添加脚本 `UIManager` (Scripts/UI)

### 6.3 创建UI元素

#### 步骤提示（左上角）
1. 创建 `UI > Text - TextMeshPro`，命名为 `StepInstructionText`
2. 位置：左上角
3. 字体大小：24
4. 拖入 UIManager 的 `Step Instruction Text`

#### 数据面板（右侧）
1. 创建 `UI > Panel`，命名为 `DataPanel`
2. 位置：右侧
3. 在 Panel 内创建以下 TextMeshPro 文本：
   - `RoundText` - 显示"第1轮 - 干土"
   - `M0Text` - 显示"m₀: --"
   - `M1Text` - 显示"m₁: --"
   - `M2Text` - 显示"m₂: --"
   - `MoistureText` - 显示"含水率: --"
   - `CurrentWeightText` - 显示"当前重量: --"
4. 将这些文本拖入 UIManager 对应字段

#### 按钮面板（底部）
1. 创建 `UI > Panel`，命名为 `ButtonPanel`
2. 位置：底部中央
3. 创建按钮：
   - `StartButton` - 文字"开始实验"
   - `RecordButton` - 文字"记录"
   - `SkipButton` - 文字"加速"
   - `NextRoundButton` - 文字"下一轮"
4. 将按钮拖入 UIManager 对应字段

#### 结果面板
1. 创建 `UI > Panel`，命名为 `ResultPanel`
2. 位置：屏幕中央
3. 创建文本：
   - `Result1Text` - 干土结果
   - `Result2Text` - 湿土结果
4. 拖入 UIManager

#### 错误提示
1. 创建 `UI > Panel`，命名为 `ErrorPopup`
2. 位置：屏幕上方中央
3. 背景色：红色半透明
4. 创建 `ErrorText` 文本
5. 拖入 UIManager
6. `Error Display Time` → 2

---

## 第七步：可选 - 使用 ExperimentSetup 辅助配置

1. 创建空物体 `ExperimentSetup`
2. 添加脚本 `ExperimentSetup` (Scripts/Core)
3. 在 Inspector 中右键点击脚本 > `自动查找场景引用`
4. 检查并手动修正未正确识别的引用

---

## 第八步：测试运行

1. 点击 Play 按钮
2. 点击"开始实验"按钮
3. 按照步骤提示操作：
   - 拖拽铝盒到天平
   - 点击记录按钮
   - 点击铝盒盖打开
   - 点击勺子，再点击土样
   - ...依次完成所有步骤

### 操作方式
- **左键拖拽**：移动铝盒
- **左键点击**：开关门/盖、选择工具
- **右键拖拽**：旋转摄像机
- **滚轮**：缩放摄像机

---

## 常见问题

### Q: 点击物体没反应？
- 检查物体是否设置了 `Interactable` Layer
- 检查 InteractionController 的 `Interactable Layer` 是否正确
- 检查物体是否有 Collider 组件

### Q: 拖拽物体穿过其他物体？
- 这是正常的，当前实现不包含物理碰撞
- 如需碰撞，给物体添加 Rigidbody（Is Kinematic）

### Q: 高亮不显示？
- 检查 HighlightSystem 的引用是否正确设置
- 确保物体有 Renderer 组件

### Q: UI按钮点击没反应？
- 确保 Canvas 有 `Graphic Raycaster` 组件
- 确保场景有 `EventSystem` 物体

---

## 物体命名规范

为确保脚本正确识别，请遵循以下命名：
- 烘箱门：名称包含 `door`（如 `OvenDoor`）
- 干燥器盖：名称包含 `cap`（如 `DesiccatorCap`）
- 铝盒盖：名称包含 `cap`（如 `BoxCap`）
- 铝盒A：BoxId 设为 `A`
- 铝盒B：BoxId 设为 `B`
