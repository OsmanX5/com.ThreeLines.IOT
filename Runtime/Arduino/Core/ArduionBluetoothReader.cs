using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using Sirenix.OdinInspector;
public class ArduionBluetoothReader : MonoBehaviour
{
    BluetoothClient client;
    [Button]
    public void DiscoverDevices()
    {
        try
        {
            client = new BluetoothClient();
            var deviceInfos = client.DiscoverDevices();
            foreach (var deviceInfo in deviceInfos)
            {
                Debug.Log($"Device Name: {deviceInfo.DeviceName}, Address: {deviceInfo.DeviceAddress}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error discovering devices: {ex.Message}");
        }
    }
}
