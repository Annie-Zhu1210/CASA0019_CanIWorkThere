using UnityEngine;

public class FloatUpOnSpawn : MonoBehaviour
{
    [Header("float height (meters)")]
    public float floatHeight = 0.08f; 

    [Header("float duration (seconds)")]
    public float duration = 0.6f; 

    [Header("float direction")]
    public Vector3 floatDirection = Vector3.up;

    private Vector3 targetPos;
    private Vector3 startPos;

    void Start()
    {
        Vector3 dir = floatDirection.normalized;

        targetPos = transform.localPosition;

        startPos = targetPos - dir * floatHeight;

        transform.localPosition = startPos;

        StartCoroutine(FloatUp());
    }

    System.Collections.IEnumerator FloatUp()
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / duration);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, lerp);
            yield return null;
        }

        transform.localPosition = targetPos;
    }
}
