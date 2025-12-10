// using System;
// using System.Collections.Concurrent;
// using UnityEngine;
// using XCharts.Runtime;
// using UnityEngine.UI;
// using TMPro;


// public class mqttController : MonoBehaviour
// {
//     [Header("基本设置")]
//     public string nameController = "Controller 1";
//     // 允许留空，代码会自动找
//     public string tag_mqttManager = "";

//     [Header("MQTT 设置")]
//     public string topicSubscribed = "";

//     [Header("图表设置")]
//     public int maxDataPoints = 15;
//     public LineChart lineChart;

//     [Header("四象限指示器")]
//     public QuadrantIndicator quadrantIndicator;


//     [Header("数值显示")]
//     public TMP_Text wifiValueText;    // 显示 WiFi RSSI
//     public TMP_Text soundValueText;   // 显示噪音 dB
//     public TMP_Text PeopleCountText;
//     public TMP_Text LaptopCountText;
//     public TMP_Text PhoneCountText;

//     public float sound_db;
//     public NeedleController needle;


//     private mqttManager _eventSender;
//     private ConcurrentQueue<MySimpleData> _dataQueue = new ConcurrentQueue<MySimpleData>();

//     void Start()
//     {
//         // 【新增】强制刷新 Canvas，确保初始化时能获取到正确的图表宽高
//         Canvas.ForceUpdateCanvases();

//         // 1. 初始化图表
//         InitializeChart();

//         // 2. 查找 MQTT Manager (智能查找：先试 Tag，不行找类型)
//         if (!string.IsNullOrEmpty(tag_mqttManager))
//         {
//             GameObject[] managers = GameObject.FindGameObjectsWithTag(tag_mqttManager);
//             if (managers.Length > 0) _eventSender = managers[0].GetComponent<mqttManager>();
//         }

//         // 如果上面没找到，或者 Tag 没填，直接按类型找（双重保险）
//         if (_eventSender == null)
//         {
//             _eventSender = FindObjectOfType<mqttManager>();
//         }

//         // 3. 连接与监听
//         if (_eventSender != null)
//         {
//             // 防止重复连接
//             //if (!_eventSender.isConnected) 
//             //{
//             // _eventSender.Connect(); 
//             //}

//             // 重新绑定事件
//             _eventSender.OnMessageArrived -= OnMessageArrivedHandler;
//             _eventSender.OnMessageArrived += OnMessageArrivedHandler;
//         }
//         else
//         {
//             Debug.LogError("没有找到 mqttManager，请检查场景中是否有挂载该脚本的物体！");
//         }
//     }

//     void InitializeChart()
//     {
//         if (lineChart == null) return;

//         // =======================================================
//         // 1. 获取图表高度，并根据高度计算字体大小 (适配逻辑)
//         // =======================================================
//         var rectTrans = lineChart.GetComponent<RectTransform>();
//         float chartHeight = rectTrans.rect.height;

//         // 防止一开始还没加载出来高度为0的情况
//         if (chartHeight <= 10) chartHeight = 400;

//         // --- 动态计算比例 ---
//         int titleFontSize = Mathf.RoundToInt(chartHeight / 18f);
//         int axisFontSize = Mathf.RoundToInt(chartHeight / 30f);
//         int legendFontSize = Mathf.RoundToInt(chartHeight / 25f);

//         // 图例图标的大小也随字体变化
//         float legendIconWidth = legendFontSize * 1.5f;
//         float legendIconHeight = legendFontSize * 0.8f;
//         // =======================================================

//         // 1. 清理
//         lineChart.ClearData();

//         // --- A. 布局与组件 (改为百分比布局) ---
//         var grid = lineChart.EnsureChartComponent<GridCoord>();
//         grid.top = chartHeight * 0.22f;
//         grid.bottom = chartHeight * 0.15f;
//         grid.left = chartHeight * 0.12f;
//         grid.right = chartHeight * 0.1f;

//         var title = lineChart.EnsureChartComponent<Title>();
//         title.text = "WiFi & Sound Monitor";
//         title.subText = "RSSI (dBm) / Sound (dB)";

//         // 【3.x 修复】使用 labelStyle 设置动态字体
//         title.labelStyle.textStyle.fontSize = titleFontSize;
//         title.subLabelStyle.textStyle.fontSize = Mathf.RoundToInt(titleFontSize * 0.6f);

//         var legend = lineChart.EnsureChartComponent<Legend>();
//         legend.show = true;
//         // 【3.x 修复】自适应图例大小
//         legend.labelStyle.textStyle.fontSize = legendFontSize;
//         legend.itemWidth = legendIconWidth;
//         legend.itemHeight = legendIconHeight;
//         legend.itemGap = legendFontSize;

//         var tooltip = lineChart.EnsureChartComponent<Tooltip>();
//         tooltip.show = true;
//         tooltip.trigger = Tooltip.Trigger.Axis;
//         tooltip.type = Tooltip.Type.Line;
//         // Tooltip 保持默认，自动跟随主题

//         lineChart.EnsureChartComponent<Background>().show = false;

//         // --- B. 坐标轴 (Y轴格子变密) ---
//         var xAxis = lineChart.EnsureChartComponent<XAxis>();
//         xAxis.type = Axis.AxisType.Category;
//         xAxis.boundaryGap = false;
//         xAxis.maxCache = maxDataPoints;
//         xAxis.splitLine.show = false;
//         xAxis.axisTick.show = false;
//         xAxis.axisLabel.show = true;

//         // 【3.x 修复】使用 distance 和动态字体
//         xAxis.axisLabel.textStyle.fontSize = axisFontSize;
//         xAxis.axisLabel.distance = axisFontSize * 0.6f;

//         var yAxis = lineChart.EnsureChartComponent<YAxis>();
//         yAxis.type = Axis.AxisType.Value;
//         yAxis.min = -100;
//         yAxis.max = 100;
//         yAxis.interval = 20;

//         yAxis.axisLine.show = false;
//         yAxis.axisTick.show = false;
//         yAxis.splitLine.show = true;

//         // 动态字体
//         yAxis.axisLabel.textStyle.fontSize = axisFontSize;

//         if (yAxis.splitLine.lineStyle != null)
//         {
//             yAxis.splitLine.lineStyle.type = LineStyle.Type.Dashed;
//             yAxis.splitLine.lineStyle.color = new Color(1, 1, 1, 0.15f);
//         }

//         // --- C. 线条设置 (颜色统一) ---

//         // Series 0: WiFi
//         if (lineChart.series.Count < 1) lineChart.AddSerie<Line>("WiFi RSSI");
//         else lineChart.series[0].serieName = "WiFi RSSI";

//         // Series 1: Sound
//         if (lineChart.series.Count < 2) lineChart.AddSerie<Line>("Sound dB");
//         else lineChart.series[1].serieName = "Sound dB";

//         // [WiFi 样式] - 青色
//         var s0 = lineChart.series[0];
//         s0.lineType = LineType.Smooth;
//         if (s0.symbol != null) s0.symbol.show = false;

//         // 【关键修改】同时设置 ItemStyle 的颜色，图例就会自动变成青色
//         if (s0.itemStyle != null) s0.itemStyle.color = new Color32(0, 255, 230, 255);

//         if (s0.lineStyle != null)
//         {
//             // 线宽改为动态
//             s0.lineStyle.width = Mathf.Max(2.5f, legendFontSize * 0.15f);
//             s0.lineStyle.color = new Color32(0, 255, 230, 255);
//         }
//         if (s0.areaStyle != null)
//         {
//             s0.areaStyle.show = true;
//             s0.areaStyle.color = new Color32(0, 255, 230, 80);
//             s0.areaStyle.toColor = new Color32(0, 255, 230, 0);
//         }

//         // [Sound 样式] - 橙色
//         var s1 = lineChart.series[1];
//         s1.lineType = LineType.Smooth;
//         if (s1.symbol != null) s1.symbol.show = false;

//         // 【关键修改】同时设置 ItemStyle 的颜色，图例就会自动变成橙色
//         if (s1.itemStyle != null) s1.itemStyle.color = new Color32(255, 160, 0, 255);

//         if (s1.lineStyle != null)
//         {
//             // 线宽改为动态
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

//     // 这一步也做个保险，如果 Start 没连上，OnEnable 再试一次
//     void OnEnable()
//     {
//         if (_eventSender != null) _eventSender.OnMessageArrived += OnMessageArrivedHandler;
//     }

//     void OnDisable()
//     {
//         if (_eventSender != null) _eventSender.OnMessageArrived -= OnMessageArrivedHandler;
//     }

//     private void OnMessageArrivedHandler(mqttObj mqttObject)
//     {
//         if (mqttObject.topic.Contains(topicSubscribed))
//         {
//             try
//             {
//                 var response = JsonUtility.FromJson<MySimpleData>(mqttObject.msg);
//                 if (response != null) _dataQueue.Enqueue(response);
//             }
//             catch (Exception e) { Debug.LogError("JSON Error: " + e.Message); }
//         }
//     }

//     void Update()
//     {
//         while (_dataQueue.TryDequeue(out MySimpleData data))
//         {
//             UpdateChart(data);
//         }
//     }

//     void UpdateChart(MySimpleData data)
//     {
//         if (lineChart == null) return;

//         // 先更新四象限颜色块
//         if (quadrantIndicator != null)
//         {
//             quadrantIndicator.UpdateQuadrant(data.wifi_rssi, data.sound_db);
//         }


//         // 1. 解析时间
//         string timeStr;
//         try
//         {
//             DateTime dt = DateTime.Parse(data.time);
//             timeStr = dt.ToString("HH:mm:ss");
//         }
//         catch
//         {
//             timeStr = DateTime.Now.ToString("HH:mm:ss");
//         }

//         // 2. 添加数据
//         lineChart.AddXAxisData(timeStr);
//         lineChart.AddData(0, data.wifi_rssi);
//         lineChart.AddData(1, data.sound_db);

//         if (wifiValueText != null)
//         {
//             // 只显示数字（可以按需要改成取绝对值或不带负号）
//             wifiValueText.text = data.wifi_rssi.ToString();
//         }

//         if (soundValueText != null)
//         {
//             // 只显示一位小数，不带单位
//             soundValueText.text = data.sound_db.ToString("F1");
//         }
//         sound_db = data.sound_db;
//         if (needle != null)
//         {
//             needle.currentValue = data.sound_db;
//         }

//         // 👉 更新人数
//         if (PeopleCountText != null)
//         {
//             PeopleCountText.text = data.people_count.ToString();
//         }

//         // 👉 更新电脑数量
//         if (LaptopCountText != null)
//         {
//             LaptopCountText.text = data.computer_count.ToString();
//         }

//         // 👉 更新手机数量
//         if (PhoneCountText != null)
//         {
//             PhoneCountText.text = data.phone_count.ToString();
//         }


//         // 3. 【手动移除旧数据逻辑 (保留)】
//         // 使用 List 直接操作，最安全，且符合你的要求
//         if (lineChart.series.Count > 0)
//         {
//             var serie0 = lineChart.series[0];

//             if (serie0.dataCount > maxDataPoints)
//             {
//                 // 移除 Series 0 (WiFi) 第一个点
//                 if (lineChart.series[0].data.Count > 0)
//                     lineChart.series[0].data.RemoveAt(0);

//                 // 移除 Series 1 (Sound) 第一个点
//                 if (lineChart.series.Count > 1 && lineChart.series[1].data.Count > 0)
//                     lineChart.series[1].data.RemoveAt(0);

//                 // 移除 X 轴第一个点
//                 var xAxis = lineChart.EnsureChartComponent<XAxis>();
//                 if (xAxis.data.Count > 0 && xAxis.data.Count > serie0.dataCount)
//                 {
//                     xAxis.data.RemoveAt(0);
//                 }
//             }
//         }


//         // 4. 刷新
//         lineChart.RefreshChart();
//     }
// }

// [Serializable]
// public class MySimpleData
// {
//     public string time;
//     public int wifi_rssi;
//     public float sound_db;
//     public int people_count;
//     public int computer_count;
//     public int phone_count;
// }
using System;
using System.Collections.Concurrent;
using UnityEngine;
using XCharts.Runtime;
using UnityEngine.UI;
using TMPro;

public class mqttController : MonoBehaviour
{
    [Header("Basic Settings")]
    public string nameController = "Controller 1";
    // Can be left empty; the code will auto-find it using tag or type.
    public string tag_mqttManager = "";

    [Header("MQTT Settings")]
    public string topicSubscribed = "";

    [Header("Chart Settings")]
    public int maxDataPoints = 15;
    public LineChart lineChart;

    [Header("Quadrant Indicator")]
    public QuadrantIndicator quadrantIndicator;

    [Header("Value Display (UI)")]

    public TMP_Text wifiValueText;    // Displays WiFi RSSI
    public TMP_Text soundValueText;   // Displays Sound dB
    public TMP_Text PeopleCountText;
    public TMP_Text LaptopCountText;
    public TMP_Text PhoneCountText;

    public float sound_db;
    public NeedleController needle;

    private mqttManager _eventSender;
    private ConcurrentQueue<MySimpleData> _dataQueue = new ConcurrentQueue<MySimpleData>();

    void Start()
    {
        // [Added] Force update Canvas to ensure chart gets correct width/height on init.
        Canvas.ForceUpdateCanvases();

        // 1. Initialize Chart
        InitializeChart();

        // 2. Find MQTT Manager (Smart find: Try Tag first, then Type)
        if (!string.IsNullOrEmpty(tag_mqttManager))
        {
            GameObject[] managers = GameObject.FindGameObjectsWithTag(tag_mqttManager);
            if (managers.Length > 0) _eventSender = managers[0].GetComponent<mqttManager>();
        }

        // If not found above, or Tag is empty, search by Type (Double check)
        if (_eventSender == null)
        {
            _eventSender = FindFirstObjectByType<mqttManager>();
        }

        // 3. Connect and Listen
        if (_eventSender != null)
        {
            // Prevent duplicate connection logic (commented out as per original)
            //if (!_eventSender.isConnected) 
            //{
            // _eventSender.Connect(); 
            //}

            // Re-bind events
            _eventSender.OnMessageArrived -= OnMessageArrivedHandler;
            _eventSender.OnMessageArrived += OnMessageArrivedHandler;
        }
        else
        {
            Debug.LogError("mqttManager not found! Please check if the object with the script is in the scene.");
        }
    }

    void InitializeChart()
    {
        if (lineChart == null) return;

        // =======================================================
        // 1. Get chart height and calculate font size (Adaptive logic)
        // =======================================================
        var rectTrans = lineChart.GetComponent<RectTransform>();
        float chartHeight = rectTrans.rect.height;

        // Prevent height being 0 if not fully loaded yet
        if (chartHeight <= 10) chartHeight = 400;

        // --- Calculate Ratios Dynamically ---
        int titleFontSize = Mathf.RoundToInt(chartHeight / 18f);
        int axisFontSize = Mathf.RoundToInt(chartHeight / 30f);
        int legendFontSize = Mathf.RoundToInt(chartHeight / 25f);

        // Legend icon size changes with font
        float legendIconWidth = legendFontSize * 1.5f;
        float legendIconHeight = legendFontSize * 0.8f;
        // =======================================================

        // 1. Clear
        lineChart.ClearData();

        // --- A. Layout & Components (Percentage-based) ---
        var grid = lineChart.EnsureChartComponent<GridCoord>();
        grid.top = chartHeight * 0.22f;
        grid.bottom = chartHeight * 0.15f;
        grid.left = chartHeight * 0.12f;
        grid.right = chartHeight * 0.1f;

        var title = lineChart.EnsureChartComponent<Title>();
        title.text = "WiFi & Sound Monitor";
        title.subText = "RSSI (dBm) / Sound (dB)";

        // [3.x Fix] Use labelStyle for dynamic font size
        title.labelStyle.textStyle.fontSize = titleFontSize;
        title.subLabelStyle.textStyle.fontSize = Mathf.RoundToInt(titleFontSize * 0.6f);

        var legend = lineChart.EnsureChartComponent<Legend>();
        legend.show = true;
        // [3.x Fix] Adaptive legend size
        legend.labelStyle.textStyle.fontSize = legendFontSize;
        legend.itemWidth = legendIconWidth;
        legend.itemHeight = legendIconHeight;
        legend.itemGap = legendFontSize;

        var tooltip = lineChart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;
        tooltip.trigger = Tooltip.Trigger.Axis;
        tooltip.type = Tooltip.Type.Line;
        // Tooltip uses default settings

        lineChart.EnsureChartComponent<Background>().show = false;

        // --- B. Axes (Denser Y-axis grid) ---
        var xAxis = lineChart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Category;
        xAxis.boundaryGap = false;
        xAxis.maxCache = maxDataPoints;
        xAxis.splitLine.show = false;
        xAxis.axisTick.show = false;
        xAxis.axisLabel.show = true;

        // [3.x Fix] Use distance and dynamic font
        xAxis.axisLabel.textStyle.fontSize = axisFontSize;
        xAxis.axisLabel.distance = axisFontSize * 0.6f;

        var yAxis = lineChart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.min = -100;
        yAxis.max = 100;
        yAxis.interval = 20;

        yAxis.axisLine.show = false;
        yAxis.axisTick.show = false;
        yAxis.splitLine.show = true;

        // Dynamic font
        yAxis.axisLabel.textStyle.fontSize = axisFontSize;

        if (yAxis.splitLine.lineStyle != null)
        {
            yAxis.splitLine.lineStyle.type = LineStyle.Type.Dashed;
            yAxis.splitLine.lineStyle.color = new Color(1, 1, 1, 0.15f);
        }

        // --- C. Line Settings (Unified Colors) ---

        // Series 0: WiFi
        if (lineChart.series.Count < 1) lineChart.AddSerie<Line>("WiFi RSSI");
        else lineChart.series[0].serieName = "WiFi RSSI";

        // Series 1: Sound
        if (lineChart.series.Count < 2) lineChart.AddSerie<Line>("Sound dB");
        else lineChart.series[1].serieName = "Sound dB";

        // [WiFi Style] - Cyan
        var s0 = lineChart.series[0];
        s0.lineType = LineType.Smooth;
        if (s0.symbol != null) s0.symbol.show = false;

        // [Key Change] Set ItemStyle color so the legend automatically matches
        if (s0.itemStyle != null) s0.itemStyle.color = new Color32(187, 255, 42, 255);

        if (s0.lineStyle != null)
        {
            // Line width is now dynamic
            s0.lineStyle.width = Mathf.Max(2.5f, legendFontSize * 0.15f);
            s0.lineStyle.color = new Color32(187, 255, 42, 255);
        }
        if (s0.areaStyle != null)
        {
            s0.areaStyle.show = true;
            s0.areaStyle.color = new Color32(187, 255, 42, 80);
            s0.areaStyle.toColor = new Color32(187, 255, 42, 0);
        }

        // [Sound Style] - Orange
        var s1 = lineChart.series[1];
        s1.lineType = LineType.Smooth;
        if (s1.symbol != null) s1.symbol.show = false;

        // [Key Change] Set ItemStyle color so the legend automatically matches
        if (s1.itemStyle != null) s1.itemStyle.color = new Color32(255, 160, 0, 255);

        if (s1.lineStyle != null)
        {
            // Line width is now dynamic
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

    // Safety check: if Start didn't connect, try again in OnEnable
    void OnEnable()
    {
        if (_eventSender != null) _eventSender.OnMessageArrived += OnMessageArrivedHandler;
    }

    void OnDisable()
    {
        if (_eventSender != null) _eventSender.OnMessageArrived -= OnMessageArrivedHandler;
    }

    private void OnMessageArrivedHandler(mqttObj mqttObject)
    {
        if (mqttObject.topic.Contains(topicSubscribed))
        {
            try
            {
                var response = JsonUtility.FromJson<MySimpleData>(mqttObject.msg);
                if (response != null) _dataQueue.Enqueue(response);
            }
            catch (Exception e) { Debug.LogError("JSON Error: " + e.Message); }
        }
    }

    void Update()
    {
        while (_dataQueue.TryDequeue(out MySimpleData data))
        {
            UpdateChart(data);
        }
    }

    void UpdateChart(MySimpleData data)
    {
        if (lineChart == null) return;

        // First, update the quadrant color blocks
        if (quadrantIndicator != null)
        {
            quadrantIndicator.UpdateQuadrant(data.wifi_rssi, data.sound_db);
        }

        // 1. Parse Time
        string timeStr;
        try
        {
            DateTime dt = DateTime.Parse(data.time);
            timeStr = dt.ToString("HH:mm:ss");
        }
        catch
        {
            timeStr = DateTime.Now.ToString("HH:mm:ss");
        }

        // 2. Add Data
        lineChart.AddXAxisData(timeStr);
        lineChart.AddData(0, data.wifi_rssi);
        lineChart.AddData(1, data.sound_db);

        if (wifiValueText != null)
        {
            // Only show numbers
            wifiValueText.text = data.wifi_rssi.ToString();
        }

        if (soundValueText != null)
        {
            // Only show one decimal place, no unit
            soundValueText.text = data.sound_db.ToString("F1");
        }
        sound_db = data.sound_db;
        if (needle != null)
        {
            needle.currentValue = data.sound_db;
        }

        // 👉 Update People Count
        if (PeopleCountText != null)
        {
            PeopleCountText.text = data.people_count.ToString();
        }

        // 👉 Update Laptop Count
        if (LaptopCountText != null)
        {
            LaptopCountText.text = data.computer_count.ToString();
        }

        // 👉 Update Phone Count
        if (PhoneCountText != null)
        {
            PhoneCountText.text = data.phone_count.ToString();
        }

        // 3. [Manual Old Data Removal Logic (Kept)]
        // Direct List operation, safest and meets requirements
        if (lineChart.series.Count > 0)
        {
            var serie0 = lineChart.series[0];

            if (serie0.dataCount > maxDataPoints)
            {
                // Remove first point of Series 0 (WiFi)
                if (lineChart.series[0].data.Count > 0)
                    lineChart.series[0].data.RemoveAt(0);

                // Remove first point of Series 1 (Sound)
                if (lineChart.series.Count > 1 && lineChart.series[1].data.Count > 0)
                    lineChart.series[1].data.RemoveAt(0);

                // Remove first point of X Axis
                var xAxis = lineChart.EnsureChartComponent<XAxis>();
                if (xAxis.data.Count > 0 && xAxis.data.Count > serie0.dataCount)
                {
                    xAxis.data.RemoveAt(0);
                }
            }
        }

        // 4. Refresh
        lineChart.RefreshChart();
    }
}

[Serializable]
public class MySimpleData
{
    // These names match the JSON keys from MQTT, so they are kept as is.
    public string time;
    public int wifi_rssi;
    public float sound_db;
    public int people_count;
    public int computer_count;
    public int phone_count;
}