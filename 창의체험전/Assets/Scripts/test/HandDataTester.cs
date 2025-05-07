using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class HandDataTester : MonoBehaviour
{
    UdpClient udpClient;
    public int port = 5055;

    void Start()
    {
        udpClient = new UdpClient(port);
        udpClient.BeginReceive(ReceiveCallback, null);
        Debug.Log("UDP 수신 대기 중 (포트 " + port + ")");
    }

    void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            byte[] data = udpClient.EndReceive(ar, ref ep);
            string message = Encoding.UTF8.GetString(data);

            Debug.Log("수신된 데이터:\n" + message);
        }
        catch (Exception e)
        {
            Debug.LogError("UDP 수신 오류: " + e.Message);
        }
        finally
        {
            udpClient.BeginReceive(ReceiveCallback, null); // 계속 수신
        }
    }

    void OnApplicationQuit()
    {
        udpClient?.Close();
    }
}