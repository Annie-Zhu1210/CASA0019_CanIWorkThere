using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTrackDebugger : MonoBehaviour
{
    ARTrackedImageManager _manager;

    void Awake()
    {
        _manager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        _manager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        _manager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var added in args.added)
        {
            Debug.Log($"[ImageDebug] ADDED image: {added.referenceImage.name}, trackingState = {added.trackingState}");
        }

        foreach (var updated in args.updated)
        {
            Debug.Log($"[ImageDebug] UPDATED image: {updated.referenceImage.name}, trackingState = {updated.trackingState}");
        }

        foreach (var removed in args.removed)
        {
            Debug.Log($"[ImageDebug] REMOVED image: {removed.referenceImage.name}");
        }
    }
}
