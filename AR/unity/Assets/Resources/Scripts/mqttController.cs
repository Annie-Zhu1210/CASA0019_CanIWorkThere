using System;
using UnityEngine;

public class mqttController : MonoBehaviour
{
    [Tooltip("Optional name for the controller")]
    public string nameController = "Controller 1";

    public string tag_mqttManager = ""; 

    [Header("   Case Sensitive!!")]
    [Tooltip("the topic to subscribe must contain this value. !!Case Sensitive!! ")]
    public string topicSubscribed = ""; 

    private float pointerValue = 0.0f;

    [Space]
    public GameObject objectToControl;

    [Tooltip("Select the rotation axis of the object to control")]
    public enum State { X, Y, Z };
    public State rotationAxis;

    [Space]
    [Tooltip("Direction Rotation")]
    public bool ClockWise = true;
    private int rotationDirection = 1;

    [Space]
    [Tooltip("minimum sensor value mapped to the dial")]
    public float startValue = 30f;

    [Tooltip("maximum sensor value mapped to the dial")]
    public float endValue = 90f;

    [Tooltip("full extension of the gauge in EulerAngles")]
    public float fullAngle = 270f;

    [Tooltip("Adjust the origin of the scale (offset in degrees)")]
    public float adjustedStart = 135f; 

    [Space]
    public mqttManager _eventSender;

    void Awake()
    {
        if (GameObject.FindGameObjectsWithTag(tag_mqttManager).Length > 0)
        {
            _eventSender = GameObject.FindGameObjectsWithTag(tag_mqttManager)[0]
                                .gameObject.GetComponent<mqttManager>();
        }
        else
        {
            Debug.LogError("At least one GameObject with mqttManager component and Tag == tag_mqttManager needs to be provided");
        }
    }

    void OnEnable()
    {
        if (_eventSender != null)
        {
            _eventSender.OnMessageArrived += OnMessageArrivedHandler;
        }
    }

    private void OnDisable()
    {
        if (_eventSender != null)
        {
            _eventSender.OnMessageArrived -= OnMessageArrivedHandler;
        }
    }

    private void OnMessageArrivedHandler(mqttObj mqttObject)
    {
        if (mqttObject.topic.Contains(topicSubscribed))
        {
            var data = JsonUtility.FromJson<LaxrSensor.Root>(mqttObject.msg);

            pointerValue = data.sound_db;

            Debug.Log($"[mqttController] {nameController} sound_db = {pointerValue}, wifi = {data.wifi_rssi}");
        }
    }

    private void Update()
    {
        float step = 1.5f * Time.deltaTime;
        rotationDirection = ClockWise ? -1 : 1;

        if (pointerValue >= startValue)
        {
            float mappedAngle = (pointerValue - startValue) * (fullAngle / (endValue - startValue));
            float finalAngle = rotationDirection * mappedAngle - adjustedStart;

            Vector3 rotationVector = objectToControl.transform.localEulerAngles;

            if (rotationAxis == State.X)
            {
                rotationVector = new Vector3(
                    finalAngle,
                    objectToControl.transform.localEulerAngles.y,
                    objectToControl.transform.localEulerAngles.z);
            }
            else if (rotationAxis == State.Y)
            {
                rotationVector = new Vector3(
                    objectToControl.transform.localEulerAngles.x,
                    finalAngle,
                    objectToControl.transform.localEulerAngles.z);
            }
            else if (rotationAxis == State.Z)
            {
                rotationVector = new Vector3(
                    objectToControl.transform.localEulerAngles.x,
                    objectToControl.transform.localEulerAngles.y,
                    finalAngle);
            }

            objectToControl.transform.localRotation = Quaternion.Lerp(
                objectToControl.transform.localRotation,
                Quaternion.Euler(rotationVector),
                step);
        }
    }
}

[Serializable]
public class LaxrSensor
{
    [Serializable]
    public class Root
    {
        public string time; 
        public float sound_db;
        public float wifi_rssi;
    }
}
