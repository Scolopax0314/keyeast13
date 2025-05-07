using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

public class HandReceiver : MonoBehaviour
{
    UdpClient udpClient;
    public int port = 5055;
    public GameObject linePrefab; // 프리팹으로 생성

    private List<LineRenderer> lines = new List<LineRenderer>();
    private int[][] connections = new int[][]
    {
        new int[]{0, 1}, new int[]{1, 2}, new int[]{2, 3}, new int[]{3, 4},
        new int[]{0, 5}, new int[]{5, 6}, new int[]{6, 7}, new int[]{7, 8},
        new int[]{0, 9}, new int[]{9,10}, new int[]{10,11}, new int[]{11,12},
        new int[]{0,13}, new int[]{13,14}, new int[]{14,15}, new int[]{15,16},
        new int[]{0,17}, new int[]{17,18}, new int[]{18,19}, new int[]{19,20},
    };

    void Start()
    {
        udpClient = new UdpClient(port);
        udpClient.BeginReceive(ReceiveCallback, null);

        // LineRenderer 미리 생성
        for (int i = 0; i < connections.Length; i++)
        {
            var obj = Instantiate(linePrefab);
            var lr = obj.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lines.Add(lr);
        }
    }

    void ReceiveCallback(IAsyncResult ar)
    {
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
        byte[] data = udpClient.EndReceive(ar, ref ep);
        string json = Encoding.UTF8.GetString(data);

        List<Vector3> handPoints = ParseHandData(json);

        // Unity 메인 스레드에서 실행되도록 큐 사용 (간단 구현)
        UnityMainThreadDispatcher.Instance().Enqueue(() => DrawLines(handPoints));

        udpClient.BeginReceive(ReceiveCallback, null);
    }

    void DrawLines(List<Vector3> points)
    {
        if (points.Count != 21) return;

        for (int i = 0; i < connections.Length; i++)
        {
            int start = connections[i][0];
            int end = connections[i][1];

            lines[i].SetPosition(0, points[start] * 5);  // 스케일 조정
            lines[i].SetPosition(1, points[end] * 5);
        }
    }

    List<Vector3> ParseHandData(string json)
    {
        return JsonUtility.FromJson<Wrapper>("{\"points\":" + json + "}").points.ConvertAll(
            p => new Vector3(p.x, -p.y, -p.z)); // y, z 방향 뒤집어서 맞춤
    }

    [System.Serializable]
    public class Point { public float x, y, z; }

    [System.Serializable]
    public class Wrapper { public List<Point> points; }
}
