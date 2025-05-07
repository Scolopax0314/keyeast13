// 개선된 HandReceiver.cs
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class HandReceiver : MonoBehaviour
{
    UdpClient udpClient;
    public int port = 5055;
    public GameObject linePrefab;

    private List<LineRenderer> lines = new List<LineRenderer>();
    private int[][] connections = new int[][]
    {
        new int[]{0, 1}, new int[]{1, 2}, new int[]{2, 3}, new int[]{3, 4},
        new int[]{0, 5}, new int[]{5, 6}, new int[]{6, 7}, new int[]{7, 8},
        new int[]{0, 9}, new int[]{9,10}, new int[]{10,11}, new int[]{11,12},
        new int[]{0,13}, new int[]{13,14}, new int[]{14,15}, new int[]{15,16},
        new int[]{0,17}, new int[]{17,18}, new int[]{18,19}, new int[]{19,20},
    };

    private float scaleFactor = 5.0f;
    private List<Vector3> lastValidPoints = new List<Vector3>();

    void Start()
    {
        udpClient = new UdpClient(port);
        udpClient.BeginReceive(ReceiveCallback, null);

        for (int i = 0; i < connections.Length; i++)
        {
            var obj = Instantiate(linePrefab);
            var lr = obj.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.enabled = false;
            lines.Add(lr);
        }
    }

    void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            byte[] data = udpClient.EndReceive(ar, ref ep);
            string json = Encoding.UTF8.GetString(data);

            List<Vector3> handPoints = ParseHandData(json);
            lastValidPoints = handPoints;

            UnityMainThreadDispatcher.Instance().Enqueue(() => DrawLines(handPoints));
        }
        catch (SocketException ex)
        {
            Debug.LogWarning("UDP receive error: " + ex.Message);
        }
        finally
        {
            udpClient.BeginReceive(ReceiveCallback, null);
        }
    }

    void DrawLines(List<Vector3> points)
    {
        bool valid = points != null && points.Count == 21;
        for (int i = 0; i < lines.Count; i++)
        {
            lines[i].enabled = valid;
            if (!valid) continue;

            int start = connections[i][0];
            int end = connections[i][1];

            lines[i].SetPosition(0, points[start] * scaleFactor);
            lines[i].SetPosition(1, points[end] * scaleFactor);
        }
    }

    List<Vector3> ParseHandData(string json)
    {
        try
        {
            var wrapper = JsonUtility.FromJson<Wrapper>("{\"points\":" + json + "}");
            return wrapper.points.ConvertAll(p => new Vector3(p.x, -p.y, -p.z));
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to parse hand data: " + ex.Message);
            return null;
        }
    }

    [System.Serializable]
    public class Point { public float x, y, z; }

    [System.Serializable]
    public class Wrapper { public List<Point> points; }
}
