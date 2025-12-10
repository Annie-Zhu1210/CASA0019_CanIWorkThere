// using System;
// using UnityEngine;
// using XCharts.Runtime;

// public class Random24HChart : MonoBehaviour
// {
//     [Header("图表设置")]
//     public LineChart lineChart;
//     [Tooltip("X轴显示的数据量，这里设为24代表24小时")]
//     public int maxDataPoints = 24; 
//     [Tooltip("数据刷新间隔(秒)")]
//     public float updateInterval = 1.0f; 

//     private float _timer;
//     private DateTime _simulatedTime;

//     void Start()
//     {
//         _simulatedTime = DateTime.Now.Date; 
        
//         // 【关键】强制刷新UI，确保我们能获取到正确的图表宽高
//         Canvas.ForceUpdateCanvases();
        
//         InitializeChart();
//     }

//     void InitializeChart()
//     {
//         if (lineChart == null) return;

//         // =======================================================
//         // 1. 获取图表高度，并根据高度计算字体大小
//         // =======================================================
//         var rectTrans = lineChart.GetComponent<RectTransform>();
//         float chartHeight = rectTrans.rect.height;
        
//         // 防止一开始还没加载出来高度为0的情况
//         if (chartHeight <= 10) chartHeight = 400; 

//         // --- 动态计算比例 (你可以修改这里的除数来微调大小) ---
//         // 比如：高度400时，标题字号=22；高度800时，标题字号=44
//         int titleFontSize = Mathf.RoundToInt(chartHeight / 18f); 
//         int axisFontSize  = Mathf.RoundToInt(chartHeight / 35f); // 坐标轴字小一点
//         int legendFontSize = Mathf.RoundToInt(chartHeight / 30f);
        
//         // 图例图标的大小也随字体变化
//         float legendIconWidth = legendFontSize * 1.5f; 
//         float legendIconHeight = legendFontSize * 0.8f;
//         // =======================================================

//         lineChart.ClearData();

//         // --- A. 布局 (使用比例而不是固定像素) ---
//         var grid = lineChart.EnsureChartComponent<GridCoord>();
//         grid.top = chartHeight * 0.22f;     // 顶部留空 22%
//         grid.bottom = chartHeight * 0.15f;  // 底部留空 15%
//         grid.left = chartHeight * 0.12f;
//         grid.right = chartHeight * 0.1f;

//         var title = lineChart.EnsureChartComponent<Title>();
//         title.text = "24H Simulation Monitor";
//         title.subText = "Random RSSI (dBm) / Sound (dB)";
        
//         // 【3.14.0 修复】使用 labelStyle 设置动态字体
//         title.labelStyle.textStyle.fontSize = titleFontSize;
//         title.subLabelStyle.textStyle.fontSize = Mathf.RoundToInt(titleFontSize * 0.6f);

//         var legend = lineChart.EnsureChartComponent<Legend>();
//         legend.show = true;
//         // 【3.14.0 修复】使用 labelStyle 设置动态字体
//         legend.labelStyle.textStyle.fontSize = legendFontSize;
//         legend.itemWidth = legendIconWidth;
//         legend.itemHeight = legendIconHeight;
//         legend.itemGap = legendFontSize; 

//         var tooltip = lineChart.EnsureChartComponent<Tooltip>();
//         tooltip.show = true;
//         tooltip.trigger = Tooltip.Trigger.Axis;
//         tooltip.type = Tooltip.Type.Line;
//         // 注意：Tooltip 字体通常不需要手动改，XCharts会自动适配，且API变动大，这里不手动设置以防报错

//         lineChart.EnsureChartComponent<Background>().show = false; 

//         // --- B. 坐标轴 ---
//         var xAxis = lineChart.EnsureChartComponent<XAxis>();
//         xAxis.type = Axis.AxisType.Category;
//         xAxis.boundaryGap = false; 
//         xAxis.maxCache = maxDataPoints; 
//         xAxis.splitLine.show = false;
//         xAxis.axisTick.show = false;
//         xAxis.axisLabel.show = true;
        
//         // 【3.14.0 修复】使用 distance 替代 margin，并设置动态字体
//         xAxis.axisLabel.textStyle.fontSize = axisFontSize;
//         xAxis.axisLabel.distance = axisFontSize * 0.8f; 

//         var yAxis = lineChart.EnsureChartComponent<YAxis>();
//         yAxis.type = Axis.AxisType.Value;
//         yAxis.boundaryGap = true; 
//         yAxis.axisLine.show = false;
//         yAxis.axisTick.show = false;
//         yAxis.splitLine.show = true;
        
//         yAxis.axisLabel.textStyle.fontSize = axisFontSize;
        
//         if(yAxis.splitLine.lineStyle != null) 
//         {
//             yAxis.splitLine.lineStyle.type = LineStyle.Type.Dashed;
//             yAxis.splitLine.lineStyle.color = new Color(1, 1, 1, 0.15f);
//         }

//         // --- C. 线条设置 ---
        
//         // Series 0: WiFi
//         if (lineChart.series.Count < 1) lineChart.AddSerie<Line>("WiFi RSSI");
//         else lineChart.series[0].serieName = "WiFi RSSI";

//         // Series 1: Sound
//         if (lineChart.series.Count < 2) lineChart.AddSerie<Line>("Sound dB");
//         else lineChart.series[1].serieName = "Sound dB";

//         // [WiFi 样式]
//         var s0 = lineChart.series[0];
//         s0.lineType = LineType.Smooth;
//         if (s0.symbol != null) s0.symbol.show = false; 
        
//         if (s0.itemStyle != null) s0.itemStyle.color = new Color32(0, 255, 230, 255);
//         if (s0.lineStyle != null)
//         {
//             // 线条宽度也随字体大小动态变化
//             s0.lineStyle.width = Mathf.Max(2.0f, legendFontSize * 0.15f);
//             s0.lineStyle.color = new Color32(0, 255, 230, 255);
//         }
//         if (s0.areaStyle != null)
//         {
//             s0.areaStyle.show = true;
//             s0.areaStyle.color = new Color32(0, 255, 230, 80);
//             s0.areaStyle.toColor = new Color32(0, 255, 230, 0);
//         }

//         // [Sound 样式]
//         var s1 = lineChart.series[1];
//         s1.lineType = LineType.Smooth; 
//         if (s1.symbol != null) s1.symbol.show = false; 
        
//         if (s1.itemStyle != null) s1.itemStyle.color = new Color32(255, 160, 0, 255);
//         if (s1.lineStyle != null)
//         {
//             s1.lineStyle.width = s0.lineStyle.width;
//             s1.lineStyle.color = new Color32(255, 160, 0, 255);
//         }
//         if (s1.areaStyle != null)
//         {
//             s1.areaStyle.show = true;
//             s1.areaStyle.color = new Color32(255, 160, 0, 80);
//             s1.areaStyle.toColor = new Color32(255, 160, 0, 0);
//         }
        
//         lineChart.RefreshChart();
//     }

//     void Update()
//     {
//         _timer += Time.deltaTime;
//         if (_timer >= updateInterval)
//         {
//             _timer = 0;
//             GenerateRandomData();
//         }
//     }

//     void GenerateRandomData()
//     {
//         if (lineChart == null) return;
//         _simulatedTime = _simulatedTime.AddHours(1);
//         string timeStr = _simulatedTime.ToString("HH:00"); 

//         int randomWifi = UnityEngine.Random.Range(-90, -30);
//         float randomSound = UnityEngine.Random.Range(30f, 90f);

//         UpdateChart(timeStr, randomWifi, randomSound);
//     }

//     void UpdateChart(string timeLabel, int wifiVal, float soundVal)
//     {
//         lineChart.AddXAxisData(timeLabel);
//         lineChart.AddData(0, wifiVal); 
//         lineChart.AddData(1, soundVal);  

//         if (lineChart.series.Count > 0)
//         {
//             var serie0 = lineChart.series[0];
//             if (serie0.dataCount > maxDataPoints)
//             {
//                 if (lineChart.series[0].data.Count > 0) lineChart.series[0].data.RemoveAt(0);
//                 if (lineChart.series.Count > 1 && lineChart.series[1].data.Count > 0) lineChart.series[1].data.RemoveAt(0);
//                 var xAxis = lineChart.EnsureChartComponent<XAxis>();
//                 if (xAxis.data.Count > 0 && xAxis.data.Count > serie0.dataCount) xAxis.data.RemoveAt(0);
//             }
//         }
        
//         lineChart.RefreshChart();
//     }
// }

using System;
using UnityEngine;
using XCharts.Runtime;

public class Random24HChart : MonoBehaviour
{
    [Header("Chart Settings")]
    public LineChart lineChart;
    [Tooltip("Amount of data shown on X-axis (e.g. 24 for 24 hours)")]
    public int maxDataPoints = 24; 
    [Tooltip("Data update interval (seconds)")]
    public float updateInterval = 1.0f; 

    private float _timer;
    private DateTime _simulatedTime;

    void Start()
    {
        _simulatedTime = DateTime.Now.Date; 
        
        // [Critical] Force UI update to ensure we get correct chart width/height
        Canvas.ForceUpdateCanvases();
        
        InitializeChart();
    }

    void InitializeChart()
    {
        if (lineChart == null) return;

        // =======================================================
        // 1. Get chart height and calculate font size based on height
        // =======================================================
        var rectTrans = lineChart.GetComponent<RectTransform>();
        float chartHeight = rectTrans.rect.height;
        
        // Prevent height being 0 if not fully loaded yet
        if (chartHeight <= 10) chartHeight = 400; 

        // --- Calculate ratios dynamically (Adjust divisor to tweak size) ---
        // E.g.: height 400 -> font 22; height 800 -> font 44
        int titleFontSize = Mathf.RoundToInt(chartHeight / 18f); 
        int axisFontSize  = Mathf.RoundToInt(chartHeight / 35f); // Axis font slightly smaller
        int legendFontSize = Mathf.RoundToInt(chartHeight / 30f);
        
        // Legend icon size also changes with font
        float legendIconWidth = legendFontSize * 1.5f; 
        float legendIconHeight = legendFontSize * 0.8f;
        // =======================================================

        lineChart.ClearData();

        // --- A. Layout (Use percentage instead of fixed pixels) ---
        var grid = lineChart.EnsureChartComponent<GridCoord>();
        grid.top = chartHeight * 0.22f;     // Top margin 22%
        grid.bottom = chartHeight * 0.15f;  // Bottom margin 15%
        grid.left = chartHeight * 0.12f;
        grid.right = chartHeight * 0.1f;

        var title = lineChart.EnsureChartComponent<Title>();
        title.text = "24H Simulation Monitor";
        title.subText = "Random RSSI (dBm) / Sound (dB)";
        
        // [3.14.0 Fix] Use labelStyle to set dynamic font
        title.labelStyle.textStyle.fontSize = titleFontSize;
        title.subLabelStyle.textStyle.fontSize = Mathf.RoundToInt(titleFontSize * 0.6f);

        var legend = lineChart.EnsureChartComponent<Legend>();
        legend.show = true;
        // [3.14.0 Fix] Use labelStyle to set dynamic font
        legend.labelStyle.textStyle.fontSize = legendFontSize;
        legend.itemWidth = legendIconWidth;
        legend.itemHeight = legendIconHeight;
        legend.itemGap = legendFontSize; 

        var tooltip = lineChart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;
        tooltip.trigger = Tooltip.Trigger.Axis;
        tooltip.type = Tooltip.Type.Line;
        // Note: Tooltip font usually auto-adapts; API changes often, so avoiding manual set to prevent errors.

        lineChart.EnsureChartComponent<Background>().show = false; 

        // --- B. Axes ---
        var xAxis = lineChart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Category;
        xAxis.boundaryGap = false; 
        xAxis.maxCache = maxDataPoints; 
        xAxis.splitLine.show = false;
        xAxis.axisTick.show = false;
        xAxis.axisLabel.show = true;
        
        // [3.14.0 Fix] Use distance instead of margin, and set dynamic font
        xAxis.axisLabel.textStyle.fontSize = axisFontSize;
        xAxis.axisLabel.distance = axisFontSize * 0.8f; 

        var yAxis = lineChart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.boundaryGap = true; 
        yAxis.axisLine.show = false;
        yAxis.axisTick.show = false;
        yAxis.splitLine.show = true;
        
        yAxis.axisLabel.textStyle.fontSize = axisFontSize;
        
        if(yAxis.splitLine.lineStyle != null) 
        {
            yAxis.splitLine.lineStyle.type = LineStyle.Type.Dashed;
            yAxis.splitLine.lineStyle.color = new Color(1, 1, 1, 0.15f);
        }

        // --- C. Series Settings ---
        
        // Series 0: WiFi
        if (lineChart.series.Count < 1) lineChart.AddSerie<Line>("WiFi RSSI");
        else lineChart.series[0].serieName = "WiFi RSSI";

        // Series 1: Sound
        if (lineChart.series.Count < 2) lineChart.AddSerie<Line>("Sound dB");
        else lineChart.series[1].serieName = "Sound dB";

        // [WiFi Style]
        var s0 = lineChart.series[0];
        s0.lineType = LineType.Smooth;
        if (s0.symbol != null) s0.symbol.show = false; 
        
        if (s0.itemStyle != null) s0.itemStyle.color = new Color32(187, 255, 42, 255);
        if (s0.lineStyle != null)
        {
            // Line width also changes dynamically with font size
            s0.lineStyle.width = Mathf.Max(2.0f, legendFontSize * 0.15f);
            s0.lineStyle.color = new Color32(187, 255, 42, 255);
        }
        if (s0.areaStyle != null)
        {
            s0.areaStyle.show = true;
            s0.areaStyle.color = new Color32(187, 255, 42, 80);
            s0.areaStyle.toColor = new Color32(187, 255, 42, 0);
        }

        // [Sound Style]
        var s1 = lineChart.series[1];
        s1.lineType = LineType.Smooth; 
        if (s1.symbol != null) s1.symbol.show = false; 
        
        if (s1.itemStyle != null) s1.itemStyle.color = new Color32(255, 160, 0, 255);
        if (s1.lineStyle != null)
        {
            s1.lineStyle.width = s0.lineStyle.width;
            s1.lineStyle.color = new Color32(255, 160, 0, 255);
        }
        if (s1.areaStyle != null)
        {
            s1.areaStyle.show = true;
            s1.areaStyle.color = new Color32(255, 160, 0, 80);
            s1.areaStyle.toColor = new Color32(255, 160, 0, 0);
        }
        
        lineChart.RefreshChart();
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= updateInterval)
        {
            _timer = 0;
            GenerateRandomData();
        }
    }

    void GenerateRandomData()
    {
        if (lineChart == null) return;
        _simulatedTime = _simulatedTime.AddHours(1);
        string timeStr = _simulatedTime.ToString("HH:00"); 

        int randomWifi = UnityEngine.Random.Range(-90, -30);
        float randomSound = UnityEngine.Random.Range(30f, 90f);

        UpdateChart(timeStr, randomWifi, randomSound);
    }

    void UpdateChart(string timeLabel, int wifiVal, float soundVal)
    {
        lineChart.AddXAxisData(timeLabel);
        lineChart.AddData(0, wifiVal); 
        lineChart.AddData(1, soundVal);  

        if (lineChart.series.Count > 0)
        {
            var serie0 = lineChart.series[0];
            if (serie0.dataCount > maxDataPoints)
            {
                if (lineChart.series[0].data.Count > 0) lineChart.series[0].data.RemoveAt(0);
                if (lineChart.series.Count > 1 && lineChart.series[1].data.Count > 0) lineChart.series[1].data.RemoveAt(0);
                var xAxis = lineChart.EnsureChartComponent<XAxis>();
                if (xAxis.data.Count > 0 && xAxis.data.Count > serie0.dataCount) xAxis.data.RemoveAt(0);
            }
        }
        
        lineChart.RefreshChart();
    }
}