# GLChart

Under development. 

![](https://github.com/migeyusu/GLChart/blob/master/GLChart.WPF/Doc/historical.gif)

## Project Overview

OpenTK WPF Chart is an open-source project that provides high-performance data visualization controls for WPF applications using OpenTK (OpenGL). By combining WPF's native controls with the rendering power of OpenGL, this project aims to deliver an extensible, performant, and feature-rich solution for data visualization. Current work focuses on line charts and historical trend charts. It perform rendering by OpenGL which is hosted on control [OpenTKWPFHost](https://github.com/migeyusu/OpenTKWPFHost). Following's a simple sample of historical chart.

---

## Key Features

### 1. Native WPF Controls
- Employs native WPF implementation for axes and grid rendering, ensuring seamless integration with WPF projects.
- Uses OpenGL for rendering line charts, significantly improving performance, especially in high-data scenarios.

*(Insert an image of the basic line chart here)*

### 2. 2D and 3D Charts via OpenGL Shaders
- Supports both 2D line charts and 3D planar chart visualization.
- Line thickness in the 2D chart can be dynamically adjusted.

*(Insert an image showcasing a 2D line chart with dynamic thickness)*

### 3. High-Performance Collision Detection & Interaction
- Implements efficient collision detection using quadtree and hash grid techniques.
- Enables tooltips when the mouse hovers over specific points on the chart, enhancing interaction.

*(Insert an image showing tooltip interaction on a chart point)*

### 4. Historical Trend Chart Optimization
- Achieves high-performance real-time historical trend charts using a circular buffer data structure with optimized shaders.
- Perfectly suited for scenarios requiring real-time data visualization.

*(Insert an image of a real-time historical trend chart)*

### 5. Shader-Based Auto Height Adjustment
- Dynamically adjusts the Y-axis height using shaders, significantly boosting performance.
- Unlike traditional libraries (e.g., DevExpress), this solution prevents lag when handling large datasets.

*(Insert a comparison image between this implementation and a laggy DevExpress chart on large datasets)*

### 6. Customizable Axes and Grids
- Fully customizable algorithms for axis labels and grid spacing.
- Implements dynamic grid movement effects: when dragging the chart, the grid moves within a certain range and resets after exceeding the range, creating a unique visual effect.

---

## Planned Features

While the project already offers robust functionality, the following features are planned for future development:

1. **Complete 3D Chart Support**  
   - Extend the current 3D planar chart to fully support 3D visualizations.

*(Insert an example image of a 3D chart concept)*

2. **Scatter Plot Support**  
   - Provide high-performance scatter plot rendering and interaction.

3. **Support for Custom Object Rendering**  
   - Allow rendering of various vector shapes (e.g., circles, rectangles, arrows) in the chart.

4. **2D Chart z-axis Issue**  
   - Resolve the ordering problem in 2D line charts (currently, lines may be incorrectly rendered on top or behind each other due to the lack of z-axis sorting).

*(Insert an illustrative image showing the line ordering issue and the expected result)*

5. **Automatic Context Management**  
   - Automatically manage OpenGL contexts to simplify developer workflows.

6. **Multi-Model Comparison**  
   - Support for comparison of multiple models within a single chart.

7. **Advanced Features**  
   - Expand the library with more chart types and performance optimizations.

---

## Use Cases

OpenTK WPF Chart is well-suited for a wide range of applications, including:

- **Real-Time Data Monitoring**  
  Render real-time historical data efficiently and interactively.

*(Insert an image showcasing real-time monitoring of live data)*

- **Financial and Scientific Computation**  
  Display precise line charts, trend data, or 3D charts for financial analysis or scientific result visualization.

- **Industrial Automation**  
  Visualize sensor data or production line statuses with high performance and customization.

---

## Quick Start

Instructions coming soon...

---

## Current Status

The project is under active development, and contributions are welcome! Feel free to submit issues or pull requests to help improve the library.

---

## Contribution Guidelines

Interested in contributing to the OpenTK WPF Chart project? We’d love to have you onboard! Follow these steps to get started:

1. Fork this repository and clone it to your local environment.
2. Create a feature branch and commit your changes.
3. Submit a pull request and wait for a project maintainer to review.

---

## Contact Us

- **Project Homepage**: [GitHub - OpenTK WPF Chart](https://github.com/migeyusu/GLChart)  
- Feel free to create issues or pull requests to help improve the project!

---

## License

This project is licensed under the MIT License. You’re free to use, modify, and distribute it.
