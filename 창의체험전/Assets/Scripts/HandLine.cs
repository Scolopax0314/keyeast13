using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class HandLines : MonoBehaviour
{
[SerializeField] GameObject targetPrefab;
[SerializeField] LineRenderer lineRenderer;
[SerializeField] Transform[] boneTransforms = new Transform[21];

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
        // ���� ���� ������ ����
        thread = new Thread(ConnectToServer);
        thread.IsBackground = true;
        thread.Start();

        // �հ��� ����Ʈ ������Ʈ ����
        for (int i = 0; i < targetObjects.Length; i++)
        {
            targetObjects[i] = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity);
            targetObjects[i].name = $"Landmark_{i}";
        }

        // ���η����� ����
        lineRenderer.positionCount = bonePairs.Length * 2;
        lineRenderer.useWorldSpace = true;

        AutoAssignBones();
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
                    Vector3 pos = new Vector3(
                        (landmarks[i].x - 0.5f) * 10f, 
                        (0.5f - landmarks[i].y) * 10f,
                        -landmarks[i].z * 10f);
                    targetObjects[i].transform.position = pos;

                    if (boneTransforms[i] != null)
                    {
                        boneTransforms[i].position = pos;
                    }
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

    void AutoAssignBones()
    {
        string[] boneNames = new string[]
        {
            "hand.R",                    // 0: WRIST
            "thumb_01.R",               // 1: THUMB_CMC
            "thumb_02.R",               // 2: THUMB_MCP
            "thumb_03.R",               // 3: THUMB_IP
            "thumb_03.R_end",           // 4: THUMB_TIP

            "index_01.R",               // 5: INDEX_MCP
            "index_02.R",               // 6: INDEX_PIP
            "index_03.R",               // 7: INDEX_DIP
            "index_03.R_end",           // 8: INDEX_TIP

            "middle_01.R",              // 9: MIDDLE_MCP
            "middle_02.R",              // 10: MIDDLE_PIP
            "middle_03.R",              // 11: MIDDLE_DIP
            "middle_03.R_end",          // 12: MIDDLE_TIP

            "ring_01.R",                // 13: RING_MCP
            "ring_02.R",                // 14: RING_PIP
            "ring_03.R",                // 15: RING_DIP
            "ring_03.R_end",            // 16: RING_TIP

            "pinky_01.R",               // 17: PINKY_MCP
            "pinky_02.R",               // 18: PINKY_PIP
            "pinky_03.R",               // 19: PINKY_DIP
            "pinky_03.R_end"            // 20: PINKY_TIP
        };

        for (int i = 0; i < boneNames.Length; i++)
        {
            GameObject found = GameObject.Find(boneNames[i]);
            if (found != null)
            {
                boneTransforms[i] = found.transform;
            }
            else
            {
                Debug.LogWarning($"Bone not found: {boneNames[i]}");
            }
        }
    }


    // Landmark Ŭ���� ���� (x, y, z ���ԵǾ�� ��)
    [Serializable]
    public class Landmark
    {
        public float x;
        public float y;
        public float z;
    }

    // JSON �迭 ������ȭ �����
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