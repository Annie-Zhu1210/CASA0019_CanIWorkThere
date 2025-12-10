// using UnityEngine;
// using UnityEngine.UI;

// public class QuadrantIndicator : MonoBehaviour
// {
//     [Header("四个颜色块（按象限顺时针）")]
//     public Image q1_TopRight;     // 安静 + 好网
//     public Image q2_TopLeft;    // 安静 + 烂网
//     public Image q3_BottomLeft;  // 吵 + 烂网
//     public Image q4_BottomRight; // 吵 + 好网

//     [Header("阈值设置")]
//     // WiFi RSSI，越大越好，一般 -40 ~ -90dBm
//     public float wifiGoodThreshold = -67f;  // > -67 认为“网好”
//     // 噪音 dB，越小越安静
//     public float noiseQuietThreshold = 55f; // < 55 认为“安静”

//     // 颜色配置（你给的那一套）
//     // 左上：亮 378EFF，暗 003F91
//     Color q1Bright = new Color32(0x37, 0x8E, 0xFF, 0xFF);
//     Color q1Dim    = new Color32(0x00, 0x3F, 0x91, 0xFF);

//     // 右上：亮 95FB20，暗 4F9300
//     Color q2Bright = new Color32(0x95, 0xFB, 0x20, 0xFF);
//     Color q2Dim    = new Color32(0x4F, 0x93, 0x00, 0xFF);

//     // 左下：亮 FF34B6，暗 AE0672
//     Color q3Bright = new Color32(0xFF, 0x34, 0xB6, 0xFF);
//     Color q3Dim    = new Color32(0xAE, 0x06, 0x72, 0xFF);

//     // 右下：亮 F3B804，暗 7D5E00
//     Color q4Bright = new Color32(0xF3, 0xB8, 0x04, 0xFF);
//     Color q4Dim    = new Color32(0x7D, 0x5E, 0x00, 0xFF);


//     /// <summary>
//     /// 用当前的 wifi 和 noise 值更新四象限（从外部调用）
//     /// </summary>
//     public void UpdateQuadrant(float wifiRssi, float noiseDb)
//     {
//         bool wifiGood   = wifiRssi > wifiGoodThreshold;   // 网好 / 网差
//         bool noiseQuiet = noiseDb  < noiseQuietThreshold; // 安静 / 吵

//         // 先全部置为“暗色”
//         SetColor(q1_TopRight,     q1Dim);
//         SetColor(q2_TopLeft,    q2Dim);
//         SetColor(q3_BottomLeft,  q3Dim);
//         SetColor(q4_BottomRight, q4Dim);

//         // 根据区间点亮对应块（只会有一个是“亮”的）
//         if (wifiGood && noiseQuiet)
//         {
//             // 好网 + 安静 → 右上
//             SetColor(q1_TopRight, q1Bright);
//         }
//         else if (!wifiGood && noiseQuiet)
//         {
//             // 烂网 + 安静 → 左上
//             SetColor(q2_TopLeft, q2Bright);
//         }
//         else if (wifiGood && !noiseQuiet)
//         {
//             // 烂网 + 吵 → 左下
//             SetColor(q3_BottomLeft, q3Bright);
//         }
//         else
//         {
//             // 好网 + 吵 → 右下
//             SetColor(q4_BottomRight, q4Bright);
//         }
//     }

//     private void SetColor(Image img, Color c)
//     {
//         if (img == null) return;
//         img.color = c;
//     }
// }

// using UnityEngine;
// using UnityEngine.UI;

// public class QuadrantIndicator : MonoBehaviour
// {
//     [Header("四个颜色块（UI Image）")]
//     public Image topRight;     // 右上
//     public Image topLeft;      // 左上
//     public Image bottomLeft;   // 左下
//     public Image bottomRight;  // 右下


//     [Header("右上：亮 / 暗")]
//     public Color32 topRightBright = new Color32(0x95, 0xFB, 0x20, 140); // 95FB20
//     public Color32 topRightDim = new Color32(0x4F, 0x93, 0x00, 140); // 4F9300

//     [Header("左上：亮 / 暗")]
//     public Color32 topLeftBright = new Color32(0x37, 0x8E, 0xFF, 140); // 378EFF
//     public Color32 topLeftDim = new Color32(0x00, 0x3F, 0x91, 140); // 003F91

//     [Header("左下：亮 / 暗")]
//     public Color32 bottomLeftBright = new Color32(0xFF, 0x34, 0xB6, 140); // FF34B6
//     public Color32 bottomLeftDim = new Color32(0xAE, 0x06, 0x72, 140); // AE0672

//     [Header("右下：亮 / 暗")]
//     public Color32 bottomRightBright = new Color32(0xF3, 0xB8, 0x04, 140); // F3B804
//     public Color32 bottomRightDim = new Color32(0x7D, 0x5E, 0x00, 140); // 7D5E00


//     void Awake()
//     {
//         OverrideAlpha(ref topRightBright);
//         OverrideAlpha(ref topRightDim);
//         OverrideAlpha(ref topLeftBright);
//         OverrideAlpha(ref topLeftDim);
//         OverrideAlpha(ref bottomLeftBright);
//         OverrideAlpha(ref bottomLeftDim);
//         OverrideAlpha(ref bottomRightBright);
//         OverrideAlpha(ref bottomRightDim);

//         // 一开始全部用暗色
//         SetAllDim();
//     }
//     void OverrideAlpha(ref Color32 color)
//     {
//         color = new Color32(color.r, color.g, color.b, 140);
//     }

//     void SetAllDim()
//     {
//         if (topRight) topRight.color = topRightDim;
//         if (topLeft) topLeft.color = topLeftDim;
//         if (bottomLeft) bottomLeft.color = bottomLeftDim;
//         if (bottomRight) bottomRight.color = bottomRightDim;
//     }

//     /// <summary>
//     /// 根据 wifi + 噪音 切换高亮象限
//     /// </summary>
//     public void UpdateQuadrant(int wifiRssi, float soundDb)
//     {
//         if (!topRight && !topLeft && !bottomLeft && !bottomRight)
//             return;

//         // 看看函数有没有被调用
//         // Debug.Log($"Quadrant Update wifi={wifiRssi}, sound={soundDb}");

//         SetAllDim();

//         // 你可以按自己定义调这个阈值
//         bool wifiGood = wifiRssi > -67;   // -70 以上算好
//         bool noiseLow = soundDb < 50f;    // 50 以下算安静

//         if (wifiGood && noiseLow)
//         {
//             // ✅ 右上亮
//             if (topRight) topRight.color = topRightBright;
//         }
//         else if (!wifiGood && noiseLow)
//         {
//             // ✅ 左上亮
//             if (topLeft) topLeft.color = topLeftBright;
//         }
//         else if (!wifiGood && !noiseLow)
//         {
//             // ✅ 左下亮
//             if (bottomLeft) bottomLeft.color = bottomLeftBright;
//         }
//         else
//         {
//             // ✅ 右下亮
//             if (bottomRight) bottomRight.color = bottomRightBright;
//         }
//     }
// }
using UnityEngine;
using UnityEngine.UI;

public class QuadrantIndicator : MonoBehaviour
{
    [Header("Quadrant Images (UI)")]
    public Image topRight;     // Top Right
    public Image topLeft;      // Top Left
    public Image bottomLeft;   // Bottom Left
    public Image bottomRight;  // Bottom Right

    [Header("Breathing Animation Settings")]
    [Range(0.1f, 10f)]
    public float breatheSpeed = 2.0f; // Speed of the breathing effect (Higher = Faster)

    [Header("Top Right (Green): Bright / Dim")]

    public Color32 topRightBright = new Color32(0xD7, 0xDE, 0x72, 140); 
    public Color32 topRightDim    = new Color32(0x6B, 0x70, 0x30, 140); 

    [Header("Top Left (Pink): Bright / Dim")]
    public Color32 topLeftBright  = new Color32(0x37, 0x8E, 0xFF, 140); 
    public Color32 topLeftDim     = new Color32(0x00, 0x3F, 0x91, 140); 

    [Header("Bottom Left (Red): Bright / Dim")]
    public Color32 bottomLeftBright = new Color32(0xFF, 0x34, 0xB6, 140); 
    public Color32 bottomLeftDim    = new Color32(0xAE, 0x06, 0x72, 140); 

    [Header("Bottom Right (Orange): Bright / Dim")]
    public Color32 bottomRightBright = new Color32(0xF3, 0xB8, 0x04, 140); 
    public Color32 bottomRightDim    = new Color32(0x7D, 0x5E, 0x00, 140); 

    // Enum to track which quadrant is currently active
    private enum ActiveQuadrant
    {
        None,
        TopRight,
        TopLeft,
        BottomLeft,
        BottomRight
    }

    // Stores the current active state
    private ActiveQuadrant _currentActive = ActiveQuadrant.None;

    void Awake()
    {
        // Apply alpha override to all colors on startup
        OverrideAlpha(ref topRightBright);
        OverrideAlpha(ref topRightDim);
        OverrideAlpha(ref topLeftBright);
        OverrideAlpha(ref topLeftDim);
        OverrideAlpha(ref bottomLeftBright);
        OverrideAlpha(ref bottomLeftDim);
        OverrideAlpha(ref bottomRightBright);
        OverrideAlpha(ref bottomRightDim);
    }

    void OverrideAlpha(ref Color32 color)
    {
        // Force alpha to 140
        color = new Color32(color.r, color.g, color.b, 140);
    }

    /// <summary>
    /// Unity Update loop: Handles the breathing animation logic per frame.
    /// </summary>
    void Update()
    {
        // 1. Calculate breathing factor (cycles smoothly between 0.0 and 1.0)
        // Mathf.Sin returns value between -1 and 1. 
        // We shift it (+1) and scale it (*0.5) to get a 0-1 range.
        float t = (Mathf.Sin(Time.time * breatheSpeed) + 1f) * 0.5f;

        // 2. Update colors based on the active state
        // If active: Lerp (Linear Interpolate) between Dim and Bright using factor 't'.
        // If inactive: Set color directly to Dim.

        // Top Right
        if (topRight)
            topRight.color = (_currentActive == ActiveQuadrant.TopRight) 
                ? Color.Lerp(topRightDim, topRightBright, t) 
                : topRightDim;

        // Top Left
        if (topLeft)
            topLeft.color = (_currentActive == ActiveQuadrant.TopLeft) 
                ? Color.Lerp(topLeftDim, topLeftBright, t) 
                : topLeftDim;

        // Bottom Left
        if (bottomLeft)
            bottomLeft.color = (_currentActive == ActiveQuadrant.BottomLeft) 
                ? Color.Lerp(bottomLeftDim, bottomLeftBright, t) 
                : bottomLeftDim;

        // Bottom Right
        if (bottomRight)
            bottomRight.color = (_currentActive == ActiveQuadrant.BottomRight) 
                ? Color.Lerp(bottomRightDim, bottomRightBright, t) 
                : bottomRightDim;
    }

    /// <summary>
    /// Updates the active quadrant based on Wi-Fi signal (RSSI) and Sound level (DB).
    /// Note: This method updates the STATE, the visual change happens in Update().
    /// </summary>
    public void UpdateQuadrant(int wifiRssi, float soundDb)
    {
        if (!topRight && !topLeft && !bottomLeft && !bottomRight)
            return;

        // Define thresholds (Adjust these as needed)
        bool wifiGood = wifiRssi > -67;   // e.g., > -67 is good signal
        bool noiseLow = soundDb < 50f;    // e.g., < 50dB is quiet

        if (wifiGood && noiseLow)
        {
            // Activate Top Right
            _currentActive = ActiveQuadrant.TopRight;
        }
        else if (!wifiGood && noiseLow)
        {
            // Activate Top Left
            _currentActive = ActiveQuadrant.TopLeft;
        }
        else if (!wifiGood && !noiseLow)
        {
            // Activate Bottom Left
            _currentActive = ActiveQuadrant.BottomLeft;
        }
        else
        {
            // Activate Bottom Right
            _currentActive = ActiveQuadrant.BottomRight;
        }
    }
}