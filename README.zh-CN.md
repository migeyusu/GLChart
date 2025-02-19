[English](README.md) | [简体中文](README.zh-CN.md)

![](https://github.com/migeyusu/GLChart/blob/master/GLChart.WPF/Doc/historical.gif)

## 项目简介

OpenTK WPF Chart 是一个为 WPF 提供基于 OpenTK（OpenGL）的高性能数据可视化控件的开源项目。通过结合 WPF 的原生控件和 OpenGL 的强大渲染能力，项目旨在为用户提供高性能、可扩展且功能丰富的数据可视化解决方案。当前版本侧重于折线图和历史趋势图的实现。

---

## 主要特性

### 1. 原生 WPF 控件
* 使用 WPF 原生实现了坐标轴、网格的绘制，保证了与 WPF 项目的良好兼容性。
* 折线图部分使用 OpenGL 进行渲染，极大地提升了性能，尤其适合大数据量场景。

### 2. 基于 OpenGL Shader 的 2D 和 3D 图表
* 支持 2D 折线图和 3D 平面图的显示。
* 对于 2D 图，支持线条粗细的动态调整。

### 3. 高效的碰撞检测与交互
* 实现了基于四叉树和哈希网格的高效碰撞检测算法。
* 支持鼠标移动到图中指定点位时显示 Tooltip 的功能，提供友好的交互体验。

### 4. 高性能的历史趋势图
* 通过环形缓冲数据结构，结合经过优化的 Shader，实现了高性能的实时历史趋势图显示，适用于实时数据场景。

### 5. Shader 实现的自动高度适配
* 利用着色器动态适配 Y 轴高度，极大提升了性能。
* 相较于传统控件库（如 DevExpress）在大量点位时的高度适配卡顿场景，本方案无性能瓶颈。

### 6. 灵活的轴与网格机制
* 轴的标签显示、网格间距支持完全自定义算法。
* 实现了网格的灵活移动效果，例如在拖动图表时，网格会随鼠标移动一定范围，超过范围后重置，产生独特的动态视觉效果。

---

## 待完成的功能

尽管项目目前已具备一定的功能，但未来将在以下方面进一步完善：

1. **3D 图的完整支持**  
   当前仅实现了简单的 3D 平面图显示，后续会进一步完善 3D 图表的功能。

2. **散点图支持**  
   提供高性能的散点图渲染与交互功能。

3. **对象绘制扩展**  
   支持在图中绘制各种矢量形状（如圆形、矩形、箭头等），以满足更多可视化需求。

4. **2D 图 z 轴判别问题**  
   由于当前没有 z 轴判别，2D 折线图中可能出现线条的绘制顺序错误问题（无法依照先后顺序叠加），后续将进行优化。

5. **自动上下文管理**  
   自动管理 OpenGL 上下文，提升开发便利性。

6. **多模型对比功能**  
   支持在同一图表中进行多模型输出的对比显示。

7. **更多高级特性与优化**  
   提供更多组件、提高渲染性能，同时支持更多样化的图表类型。

---

## 使用场景与目标

OpenTK WPF Chart 旨在为高性能 WPF 应用提供可视化控件解决方案，适合以下使用场景：

- **实时数据监控**  
  利用高性能历史趋势图功能，动态显示和分析海量实时数据。
  
- **金融与科学计算**  
  绘制精确的折线图、趋势图和 3D 图，用于股票分析、科学实验结果展示等。

- **工业自动化**  
  可视化大数据量传感器数据和生产线实时状态。

---

## 快速开始

敬请期待……

---

## 当前状态

* 项目正在积极开发中，欢迎大家提交 Issue 和 PR！

---

## 贡献指南

如果您对 OpenTK WPF Chart 项目感兴趣，非常欢迎您参与项目开发并提出改进建议。
如需贡献代码或功能，请按照以下步骤：

1. Fork 本仓库并克隆到本地开发环境。
2. 创建一个特性分支并提交您的更改。
3. 提交 Pull Request 并等待项目维护者的审核。

---

## 联系我们

* **项目主页**: [GitHub - OpenTK WPF Chart](https://github.com/migeyusu/GLChart)  
* 欢迎各位开发者提交 Issue 或 PR！

---

## License

此项目基于 MIT License 开源。请自由使用、修改和分发。
