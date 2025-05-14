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

    private Animator animator;
    public Transform rightHandTarget;

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
    float scaleFactor = 4f;

    Vector3 wristWorldOrigin;
    bool originInitialized = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        thread = new Thread(ConnectToServer);
        thread.IsBackground = true;
        thread.Start();

        for (int i = 0; i < targetObjects.Length; i++)
        {
            targetObjects[i] = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity);
            targetObjects[i].name = $"Landmark_{i}";
        }

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

    void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            if (rightHandTarget != null)
            {
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position); // 위치
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation); // 회전
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1); // IK 강도
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1); // 회전 강도
            }
        }
    }
    void Update()
    {
        if (!originInitialized && boneTransforms[0] != null)
        {
            wristWorldOrigin = boneTransforms[0].position;
            originInitialized = true;
        }

        if (!string.IsNullOrEmpty(latestJson))
        {
            try
            {
                Landmark[] landmarks = JsonHelper.FromJson<Landmark>(latestJson);

                scaleFactor = ComputeScaleFactor(landmarks);

                for (int i = 0; i < landmarks.Length; i++)
                {
                    Vector3 pos = ConvertLandmarkToWorld(landmarks[i]);
                    targetObjects[i].transform.position = pos;

                    if (boneTransforms[i] != null)
                    {
                        boneTransforms[i].position = pos;
                    }
                }

                if (rightHandTarget != null)
                {
                    rightHandTarget.position = targetObjects[4].transform.position;
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

    float ComputeScaleFactor(Landmark[] landmarks)
    {
        Vector2Int[] scalePairs = new Vector2Int[]
        {
            new(0, 5),  // Wrist to Index base
            new(0, 17), // Wrist to Pinky base
            new(5, 8),  // Index finger length
            new(9, 12), // Middle finger length
        };

        float totalModelLength = 0f;
        float totalLandmarkLength = 0f;

        foreach (var pair in scalePairs)
        {
            float modelLength = Vector3.Distance(boneTransforms[pair.x].position, boneTransforms[pair.y].position);

            Vector3 a = new Vector3(landmarks[pair.x].x, landmarks[pair.x].y, landmarks[pair.x].z);
            Vector3 b = new Vector3(landmarks[pair.y].x, landmarks[pair.y].y, landmarks[pair.y].z);
            float landmarkLength = Vector3.Distance(a, b);

            totalModelLength += modelLength;
            totalLandmarkLength += landmarkLength;
        }

        return totalModelLength / totalLandmarkLength;
    }

    Vector3 ConvertLandmarkToWorld(Landmark lm)
    {
        return new Vector3(
            (lm.x - 0.5f) * scaleFactor + wristWorldOrigin.x,
            (0.5f - lm.y) * scaleFactor + wristWorldOrigin.y,
            -lm.z * scaleFactor + wristWorldOrigin.z
        );
    }

    [Serializable]
    public class Landmark
    {
        public float x;
        public float y;
        public float z;
    }

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