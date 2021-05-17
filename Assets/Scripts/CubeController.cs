using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CubeController : MonoBehaviour
{
    InputDevice controller;
    float pointAngle;

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
        pointAngle = rotation.eulerAngles.y;
        Vector3 pos = transform.position;
        pos.x = 20 * Mathf.Sin(pointAngle * Mathf.Deg2Rad);
        pos.z = 20 * Mathf.Cos(pointAngle * Mathf.Deg2Rad);
        transform.position = pos;
    }
}
