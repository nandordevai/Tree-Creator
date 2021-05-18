using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class GrowthAreaController : MonoBehaviour
{
    InputDevice controller;
    float pointAngleH;

    void Start()
    {
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, inputDevices);
        controller = inputDevices[0];
    }

    void Update()
    {
        Quaternion rotation;
        controller.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation);
        Vector3 v = transform.position;
        v.x = 20 * Mathf.Sin(rotation.eulerAngles.y * Mathf.Deg2Rad);
        v.y = -20 * Mathf.Sin(rotation.eulerAngles.x * Mathf.Deg2Rad);
        v.z = 20 * Mathf.Cos(rotation.eulerAngles.y * Mathf.Deg2Rad);
        transform.position = v;
    }
}
