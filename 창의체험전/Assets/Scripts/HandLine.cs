using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class HandLines : MonoBehaviour
{
[SerializeField] GameObject targetPrefab;
[SerializeField] LineRenderer lineRenderer;

TcpClient client;
    StreamReader reader;
    Thread thread;
    string latestJson;

    GameObject[] targetObjects = new GameObject[21];

    readonly Vector2Int[] bonePairs = new Vector2Int[]
    {
    new(0, 1), new(1, 2), new(2, 3), new(3, 4),
    new(0, 5), new(5, 6), new(6, 7), new(7, 8),
    new(0, 17), new(17, 18), new(18, 19), new(19, 20),
    new(5, 9), new(9, 13), new(13, 17),
    new(9, 10), new(10, 11), new(11, 12),
    new(13, 14), new(14, 15), new(15, 16)
    };

    void Start()
    {
        // 서버 연결 쓰레드 시작
        thread = new Thread(ConnectToServer);
        thread.IsBackground = true;
        thread.Start();

        // 손가락 포인트 오브젝트 생성
        for (int i = 0; i < targetObjects.Length; i++)
        {
            targetObjects[i] = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity);
            targetObjects[i].name = $"Landmark_{i}";
        }

        // 라인렌더러 세팅
        lineRenderer.positionCount = bonePairs.Length * 2;
        lineRenderer.useWorldSpace = true;
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 9999);
            reader = new StreamReader(client.GetStream());
            Debug.Log("Connected to Python server");

            while (true)
            {
                var line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    latestJson = line;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Socket error: " + e.Message);
        }
    }

    void Update()
    {
        lineRenderer.positionCount = bonePairs.Length * 2;

        if (!string.IsNullOrEmpty(latestJson))
        {
            try
            {
                Landmark[] landmarks = JsonHelper.FromJson<Landmark>(latestJson);

                for (int i = 0; i < landmarks.Length; i++)
                {
                    Vector3 pos = new Vector3((landmarks[i].x - 0.5f) * 10f, (0.5f - landmarks[i].y) * 10f, 0);
                    targetObjects[i].transform.position = pos;
                }

                for (int i = 0; i < bonePairs.Length; i++)
                {
                    lineRenderer.SetPosition(i * 2, targetObjects[bonePairs[i].x].transform.position);
                    lineRenderer.SetPosition(i * 2 + 1, targetObjects[bonePairs[i].y].transform.position);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Parse error: " + e.Message);
            }

            latestJson = null;
        }
    }

    void OnApplicationQuit()
    {
        thread?.Abort();
        client?.Close();
    }

    // Landmark 클래스 정의 (x, y, z 포함되어야 함)
    [Serializable]
    public class Landmark
    {
        public float x;
        public float y;
        public float z;
    }

    // JSON 배열 역직렬화 도우미
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{\"array\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}