// using UnityEngine;

// public class NeedleController : MonoBehaviour
// {
//     [Header("数值范围")]
//     public float minValue = 30f;     // 最小值（比如 0 dB）
//     public float maxValue = 90f;   // 最大值（比如 130 dB）

//     [Header("角度范围")]
//     public float minAngle = -135f;  // 最小角度（最左）
//     public float maxAngle = 135f;   // 最大角度（最右）

//     [Header("当前数值")]
//     public float currentValue = 0f; // 当前输入值（你之后接 MQTT）

//     void Update()
//     {
//         UpdateNeedle(currentValue);
//     }

//     public void UpdateNeedle(float value)
//     {
//         float t = Mathf.InverseLerp(minValue, maxValue, value);
//         float angle = Mathf.Lerp(minAngle, maxAngle, t);

//         // 围绕 Z 轴旋转（UI 指针几乎都是 Z）
//         transform.localRotation = Quaternion.Euler(0, 0, angle);
//     }
// }
using UnityEngine;

public class NeedleController : MonoBehaviour
{
    [Header("Data Settings")]
    public float currentValue = 0f; // The MQTT controller will modify this value

    [Tooltip("Minimum value displayed on the gauge (e.g., 30dB)")]
    public float minDataValue = 30f; 
    
    [Tooltip("Maximum value displayed on the gauge (e.g., 100dB)")]
    public float maxDataValue = 90f;

    [Header("Angle Settings (Z-Axis Rotation)")]
    [Tooltip("Needle angle at minimum value (e.g., 135 degrees / Leans Left)")]
    public float startAngle = 45f; 

    [Tooltip("Needle angle at maximum value (e.g., -135 degrees / Leans Right)")]
    public float endAngle = -225f;

    [Header("Smoothing Settings")]
    [Tooltip("Rotation speed of the needle")]
    public float smoothSpeed = 5f;

    void Update()
    {
        // 1. Calculate ratio (0.0 to 1.0)
        // InverseLerp calculates the percentage of currentValue between min and max
        // E.g.: Range 30-100, current is 65, result is 0.5 (50%)
        float t = Mathf.InverseLerp(minDataValue, maxDataValue, currentValue);

        // 2. Calculate target angle based on ratio
        // Lerp calculates the angle between startAngle and endAngle based on t
        float targetAngleZ = Mathf.Lerp(startAngle, endAngle, t);

        // 3. Smooth rotation
        // Get current rotation
        Quaternion currentRotation = transform.localRotation;
        // Set target rotation (Z-axis only, suitable for UI)
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngleZ);

        // Use Slerp for smooth interpolation
        transform.localRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }
}